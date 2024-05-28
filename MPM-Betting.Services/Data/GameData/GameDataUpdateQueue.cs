using System.Threading.Channels;

namespace MPM_Betting.Services.Data.GameData;

public class GameDataUpdateQueue : IBackgroundTaskQueue
{
     private readonly Channel<Func<CancellationToken, ValueTask<MpmResult<object>>>> m_Queue;

    public GameDataUpdateQueue(int capacity)
    {
        // Capacity should be set based on the expected application load and
        // number of concurrent threads accessing the queue.            
        // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
        // which completes only when space became available. This leads to backpressure,
        // in case too many publishers/calls start accumulating.
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        m_Queue = Channel.CreateBounded<Func<CancellationToken, ValueTask<MpmResult<object>>>>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ValueTask<MpmResult<object>>> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        await m_Queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<Func<CancellationToken, ValueTask<MpmResult<object>>>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("hi before read");
            var workItem = await m_Queue.Reader.ReadAsync(cancellationToken);
            Console.WriteLine("hi after read");
            return workItem;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
