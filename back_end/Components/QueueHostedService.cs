namespace back_end.Components;

public class QueueHostedService : BackgroundService
{
    private readonly BackgroundTaskQueue _taskQueue;

    public QueueHostedService(BackgroundTaskQueue taskQueue)
    {
        _taskQueue = taskQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);
            await workItem();
        }
    }
}