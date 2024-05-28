namespace MPM_Betting.Services.Data.GameData;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask<MpmResult<object>>> workItem);

    ValueTask<Func<CancellationToken, ValueTask<MpmResult<object>>>> DequeueAsync(
        CancellationToken cancellationToken);
}
