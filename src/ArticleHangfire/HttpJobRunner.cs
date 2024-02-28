using Hangfire.Server;
using Microsoft.Extensions.Options;

namespace ArticleHangfire;

public class HttpJobRunner(
    ILogger<HttpJobRunner> logger,
    IOptionsMonitor<HttpJobOptions> options,
    IHttpClientFactory clientFactory
)
{
    private HttpJobOptions Options => options.CurrentValue;

    public async Task RunAsync(string category, string name, PerformContext ctx)
    {
        using var _ = logger.BeginScope(
            "Category:{Category} Name:'{Name}' JobId:'{JobId}'",
            category,
            name,
            ctx.BackgroundJob.Id
        );

        if (!Options.Endpoints.TryGetValue(category, out var endpoints))
        {
            logger.LogWarning("Configuration for category not found");
            return;
        }

        if (!endpoints.TryGetValue(name, out var endpoint))
        {
            logger.LogWarning("Configuration for endpoint not found");
            return;
        }

        if (!endpoint.Enabled)
        {
            logger.LogWarning("Endpoint was disabled while still scheduled, nothing will be done");
            return;
        }

        using var client = clientFactory.CreateClient(
            $"{nameof(HttpJobRunner)}.{category}.{name}"
        );

        client.Timeout = endpoint.Timeout ?? Options.DefaultTimeout;

        logger.LogDebug(
            "Starting HTTP request '{Method} {Address}' [Timeout:{Timeout}]",
            endpoint.Method,
            endpoint.Address,
            client.Timeout
        );

        using var request = new HttpRequestMessage(
            new HttpMethod(endpoint.Method),
            endpoint.Address
        );

        if (endpoint.Headers?.Count > 0)
        {
            foreach (var (key, value) in endpoint.Headers)
                request.Headers.Add(key, value);
        }

        var ct = ctx.CancellationToken.ShutdownToken;

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, ct);
        }
        catch (TaskCanceledException e) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("Request timed out", e);
        }

        using (response)
        {
            logger.LogInformation(
                "Completed HTTP request '{Method} {Address}' with status code {StatusCode}",
                endpoint.Method,
                endpoint.Address,
                response.StatusCode
            );

            if (!endpoint.IgnoreInvalidStatusCode)
                response.EnsureSuccessStatusCode();
        }
    }
}