using System.Threading.Channels;

namespace Social_Media_Chatting_APP_Service.Common.Upload;

// Same in-memory channel pattern as BackgroundEmailQueue, except each job
// carries a TaskCompletionSource so the caller can await the actual result
// instead of firing-and-forgetting like email does.
public class BackgroundUploadQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundUploadQueue()
    {
        _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(100);
    }

    public async ValueTask EnqueueAsync(Func<CancellationToken, Task> job)
    {
        await _queue.Writer.WriteAsync(job);
    }

    public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}