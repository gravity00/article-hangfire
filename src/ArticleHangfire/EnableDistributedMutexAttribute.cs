using Hangfire.Common;
using Hangfire.Server;

namespace ArticleHangfire;

public class EnableDistributedMutexAttribute : JobFilterAttribute, IServerFilter
{
    private const string Key = nameof(EnableDistributedMutexAttribute);

    private readonly string _nameFormat;
    private readonly TimeSpan _timeout;

    public EnableDistributedMutexAttribute(string nameFormat, int timeoutInSeconds)
    {
        ArgumentNullException.ThrowIfNull(nameFormat);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutInSeconds, 0);

        _nameFormat = nameFormat;
        _timeout = TimeSpan.FromSeconds(timeoutInSeconds);
    }

    public void OnPerforming(PerformingContext ctx)
    {
        var distributedLockName = string.Format(
            _nameFormat,
            ctx.BackgroundJob.Job.Args.ToArray()
        );

        var distributedLock = ctx.Connection.AcquireDistributedLock(
            distributedLockName,
            _timeout
        );

        ctx.Items[Key] = distributedLock;
    }

    public void OnPerformed(PerformedContext ctx)
    {
        if (!ctx.Items.TryGetValue(Key, out var distributedLock))
            throw new InvalidOperationException("Can not release a distributed lock: it was not acquired.");

        ((IDisposable)distributedLock).Dispose();
    }
}