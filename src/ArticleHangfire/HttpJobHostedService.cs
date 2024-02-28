using Hangfire;
using Microsoft.Extensions.Options;

namespace ArticleHangfire;

public class HttpJobHostedService(
    ILogger<HttpJobHostedService> logger,
    IOptionsMonitor<HttpJobOptions> options,
    IRecurringJobManager jobManager
) : IHostedService
{
    private readonly HashSet<string> _jobIds = new();
    private IDisposable _onOptionsChange;

    public Task StartAsync(CancellationToken ct)
    {
        _onOptionsChange = options.OnChange(jobOptions =>
        {
            logger.LogDebug("Configuration changed, scheduling jobs");
            ScheduleJobs(jobOptions);
        });

        logger.LogDebug("Initial configuration, scheduling jobs");
        ScheduleJobs(options.CurrentValue);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct)
    {
        _onOptionsChange?.Dispose();

        return Task.CompletedTask;
    }

    private void ScheduleJobs(HttpJobOptions options)
    {
        var currentJobIds = new HashSet<string>();

        if (options.Endpoints is { Count: > 0 })
        {
            foreach (var (category, endpoints) in options.Endpoints)
            {
                if (endpoints is not { Count: > 0 })
                    continue;

                foreach (var (name, endpoint) in endpoints)
                {
                    if (!endpoint.Enabled)
                        continue;

                    var jobId = $"HTTP:{category}:{name}";

                    jobManager.AddOrUpdate<HttpJobRunner>(
                        jobId,
                        runner => runner.RunAsync(category, name, null),
                        endpoint.Cron,
                        new RecurringJobOptions
                        {
                            TimeZone = TimeZoneInfo.Utc
                        }
                    );

                    currentJobIds.Add(jobId);
                }
            }
        }

        _jobIds.RemoveWhere(jobId => currentJobIds.Contains(jobId));

        foreach (var jobId in _jobIds) 
            jobManager.RemoveIfExists(jobId);

        _jobIds.Clear();

        foreach (var jobId in currentJobIds)
            _jobIds.Add(jobId);
    }
}