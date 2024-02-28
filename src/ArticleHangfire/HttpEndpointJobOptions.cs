namespace ArticleHangfire;

public class HttpEndpointJobOptions
{
    public string Cron { get; init; }

    public bool Enabled { get; init; }

    public string Address { get; init; }

    public string Method { get; init; }

    public IReadOnlyDictionary<string, string> Headers { get; init; }

    public TimeSpan? Timeout { get; init; }
}