namespace ArticleHangfire;

public class HttpJobOptions
{
    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(5);

    public IReadOnlyDictionary<
        string, 
        IReadOnlyDictionary<string, HttpJobEndpointOptions>
    > Endpoints { get; init; }
}