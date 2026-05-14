using System.Threading.Channels;

namespace Social_Media_Chatting_APP_Service.Common;
//This is the in-memory channel we talked about. Any service drops an email job here and forgets about it — the background worker picks it up.
public class BackgroundEmailQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundEmailQueue()
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

