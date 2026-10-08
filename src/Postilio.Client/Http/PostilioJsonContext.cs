using System.Text.Json;
using System.Text.Json.Serialization;
using Postilio.Webhooks;

namespace Postilio.Http;

// Source-generated, so the client needs no reflection: safe for trimming and native AOT.
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SendEmailRequest))]
[JsonSerializable(typeof(SendEmailResponse))]
[JsonSerializable(typeof(EmailDetails))]
[JsonSerializable(typeof(TestEmailRequest))]
[JsonSerializable(typeof(TestEmailResponse))]
[JsonSerializable(typeof(ApiUsage))]
[JsonSerializable(typeof(CreateDomainRequest))]
[JsonSerializable(typeof(DomainResponse))]
[JsonSerializable(typeof(DomainList))]
[JsonSerializable(typeof(CreateSuppressionRequest))]
[JsonSerializable(typeof(RemoveSuppressionRequest))]
[JsonSerializable(typeof(SuppressionResponse))]
[JsonSerializable(typeof(SuppressionList))]
[JsonSerializable(typeof(CreateWebhookEndpointRequest))]
[JsonSerializable(typeof(UpdateWebhookEndpointRequest))]
[JsonSerializable(typeof(WebhookEndpointResponse))]
[JsonSerializable(typeof(WebhookEndpointList))]
[JsonSerializable(typeof(CreatedWebhookEndpoint))]
[JsonSerializable(typeof(RotatedWebhookSecret))]
[JsonSerializable(typeof(WebhookDeliveryResponse))]
[JsonSerializable(typeof(WebhookDeliveryList))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
[JsonSerializable(typeof(WebhookEvent))]
internal sealed partial class PostilioJsonContext : JsonSerializerContext;
