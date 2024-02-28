namespace ArticleHangfire;

public class HttpJobEndpointOptions
{
    public string Cron { get; init; }

    public bool Enabled { get; init; }

    public string Address { get; init; }

    public string Method { get; init; }

    public IReadOnlyDictionary<string, string> Headers { get; init; }

    public TimeSpan? Timeout { get; init; }

    public bool IgnoreInvalidStatusCode { get; init; }
}