using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Postilio.Client.Tests;

/// <summary>
/// Holds the client to spec/openapi-v1.json, the copy of the API's /v1 document: one method per operation, one type
/// per schema with the same fields, types and nullability. Updating the copy makes these fail until the client follows.
/// </summary>
public sealed class SpecTests
{
    private static readonly string SpecDirectory = Path.Combine(AppContext.BaseDirectory, "spec");
    private static readonly JsonObject Spec = JsonNode.Parse(File.ReadAllText(Path.Combine(SpecDirectory, "openapi-v1.json")))?.AsObject()
        ?? throw new InvalidOperationException("spec/openapi-v1.json is empty.");
    private static readonly JsonObject Schemas = Spec["components"]?["schemas"]?.AsObject() ?? [];

    // Error bodies are read leniently (any field may be missing), and problem details carry a traceId the spec omits.
    private static readonly HashSet<string> ErrorSchemas = ["ErrorResponse", "HttpValidationProblemDetails"];
    private static readonly HashSet<string> Extensions = ["HttpValidationProblemDetails.traceId"];
    private static readonly NullabilityInfoContext Nullability = new();

    [Fact]
    public void Copy_MatchesItsFingerprint()
    {
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(SpecDirectory, "openapi-v1.json")))).ToLowerInvariant();

        Assert.Equal(File.ReadAllText(Path.Combine(SpecDirectory, "openapi-v1.sha256")).Trim(), hash);
    }

    [Fact]
    public void Client_HasOneMethodPerOperation()
    {
        var operations = Spec["paths"]?.AsObject().SelectMany(p => p.Value?.AsObject() ?? []).Select(o => (string?)o.Value?["operationId"]).Order();
        var methods = typeof(PostilioClient).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => m.Name[..^"Async".Length]).Order();

        Assert.Equal(operations, methods);
    }

    public static TheoryData<string> SchemaNames() => [.. Schemas.Select(s => s.Key)];

    [Theory, MemberData(nameof(SchemaNames))]
    public void Schema_HasATypeWithTheSameFields(string schema)
    {
        var type = typeof(PostilioClient).Assembly.GetType($"Postilio.{schema}");
        Assert.NotNull(type);
        var specFields = Schemas[schema]?["properties"]?.AsObject() ?? [];
        var properties = type.GetProperties().ToDictionary(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name));

        Assert.Equal(specFields.Select(f => f.Key).Order(), properties.Keys.Where(k => !Extensions.Contains($"{schema}.{k}")).Order());
        var mismatches = specFields
            .Select(f => (Field: f.Key, Problem: Mismatch(f.Value ?? new JsonObject(), properties[f.Key], CheckNullability(schema))))
            .Where(m => m.Problem is not null)
            .Select(m => $"{schema}.{m.Field}: {m.Problem}");
        Assert.Empty(mismatches);
    }

    // A request type may require a field the server's record marks nullable; what comes back must match exactly.
    private static bool CheckNullability(string schema) => !ErrorSchemas.Contains(schema) && ResponseSchemas.Value.Contains(schema);

    private static readonly Lazy<HashSet<string>> ResponseSchemas = new(() =>
    {
        var found = new HashSet<string>();
        var answers = Spec["paths"]?.AsObject().SelectMany(p => p.Value?.AsObject() ?? []).SelectMany(o => o.Value?["responses"]?.AsObject() ?? []) ?? [];
        foreach (var answer in answers)
        {
            CollectReferences(answer.Value, found);
        }
        return found;
    });

    private static void CollectReferences(JsonNode? node, HashSet<string> found)
    {
        switch (node)
        {
            case JsonObject o when (string?)o["$ref"] is { } reference:
                if (found.Add(reference.Split('/')[^1]))
                {
                    CollectReferences(Schemas[reference.Split('/')[^1]], found);
                }
                break;
            case JsonObject o:
                foreach (var child in o)
                {
                    CollectReferences(child.Value, found);
                }
                break;
            case JsonArray a:
                foreach (var child in a)
                {
                    CollectReferences(child, found);
                }
                break;
        }
    }

    private static string? Mismatch(JsonNode field, PropertyInfo property, bool checkNullability)
    {
        var nullable = (bool?)field["nullable"] == true || field["oneOf"]?.AsArray().Any(o => (bool?)o?["nullable"] == true) == true;
        var clrNullable = Nullable.GetUnderlyingType(property.PropertyType) is not null
            || Nullability.Create(property).ReadState == NullabilityState.Nullable;
        if (checkNullability && nullable != clrNullable)
        {
            return $"nullable is {nullable} in the spec, {clrNullable} in {property.DeclaringType?.Name}";
        }
        var clrType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        return Matches(field, clrType) ? null : $"{Describe(field)} in the spec, {clrType.Name} in {property.DeclaringType?.Name}";
    }

    private static bool Matches(JsonNode field, Type clrType)
    {
        var reference = (string?)field["$ref"] ?? field["oneOf"]?.AsArray().Select(o => (string?)o?["$ref"]).FirstOrDefault(r => r is not null);
        if (reference is not null)
        {
            return clrType.Name == reference.Split('/')[^1];
        }
        return ((string?)field["type"], (string?)field["format"]) switch
        {
            ("string", "uuid") => clrType == typeof(Guid),
            ("string", "date-time") => clrType == typeof(DateTimeOffset),
            ("string", _) => clrType == typeof(string) || clrType == typeof(byte[]),
            ("integer", "int16") => clrType == typeof(short),
            ("integer", "int32") => clrType == typeof(int),
            ("integer", _) => clrType == typeof(long),
            ("boolean", _) => clrType == typeof(bool),
            ("array", _) => clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                && Matches(field["items"] ?? new JsonObject(), clrType.GetGenericArguments()[0]),
            ("object", _) => typeof(IDictionary).IsAssignableFrom(clrType),
            _ => false,
        };
    }

    private static string Describe(JsonNode field) => field.ToJsonString();
}
