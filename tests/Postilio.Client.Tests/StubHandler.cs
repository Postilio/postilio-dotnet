using System.Net;
using System.Text;

namespace Postilio.Client.Tests;

/// <summary>Answers requests from a queue and records what was sent, including the body as it went over the wire.</summary>
internal sealed class StubHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpResponseMessage>> _answers = new();

    public List<(HttpRequestMessage Request, string? Body)> Requests { get; } = [];

    public StubHandler Answer(HttpStatusCode status, string? json = null, string mediaType = "application/json", Action<HttpResponseMessage>? configure = null)
    {
        _answers.Enqueue(() =>
        {
            var response = new HttpResponseMessage(status);
            if (json is not null)
            {
                response.Content = new StringContent(json, Encoding.UTF8, mediaType);
            }
            configure?.Invoke(response);
            return response;
        });
        return this;
    }

    public StubHandler Fail()
    {
        _answers.Enqueue(() => throw new HttpRequestException("Connection refused"));
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Requests.Add((request, body));
        return _answers.Dequeue()();
    }
}
