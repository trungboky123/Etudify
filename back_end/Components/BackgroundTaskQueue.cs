using System.Threading.Channels;

namespace back_end.Components;

public class BackgroundTaskQueue
{
    private readonly Channel<Func<Task>> _queue = Channel.CreateUnbounded<Func<Task>>();

    public void QueueBackgroundWorkItem(Func<Task> workItem)
    {
        _queue.Writer.TryWrite(workItem);
    }

    public async Task<Func<Task>> DequeueAsync(CancellationToken token)
    {
        try
        {
            return await _queue.Reader.ReadAsync(token);
        }
        catch (OperationCanceledException)
        {
            return () => Task.CompletedTask;
        }
    }
}