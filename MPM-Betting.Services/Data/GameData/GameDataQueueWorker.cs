using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MPM_Betting.Services.Data.GameData;

public class GameDataQueueWorker(
    ILogger<GameDataQueueWorker> logger, 
    IBackgroundTaskQueue taskQueue) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Queued Hosted Service is running");
        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await taskQueue.DequeueAsync(stoppingToken);

            try
            {
                var result = await workItem(stoppingToken);
                if (result.IsFaulted)
                {
                    logger.LogError(result.Exception,
                        "Error occurred executing {WorkItem}", nameof(workItem));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error occurred executing {WorkItem}", nameof(workItem));
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Queued Hosted Service is stopping");

        await base.StopAsync(stoppingToken);
    }
}