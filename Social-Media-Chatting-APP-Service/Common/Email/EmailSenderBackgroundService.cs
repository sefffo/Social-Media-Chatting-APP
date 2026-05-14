using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Social_Media_Chatting_APP_Service.Common.Email;
/// <summary>
/// A long-running background service that continuously listens for queued email jobs
/// and executes them asynchronously, decoupled from the HTTP request pipeline.
/// Inherits from <see cref="BackgroundService"/> which is automatically started
/// and stopped by the .NET host lifecycle.
/// </summary>
public class EmailSenderBackgroundService(
    BackgroundEmailQueue queue,
    ILogger<EmailSenderBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // BLOCKS here — waits idly (no CPU waste) until a job is dropped
                // into the queue by any service (OtpService, AuthService, etc.)
                // Once a job arrives, execution continues to the next line.
                var job = await queue.DequeueAsync(stoppingToken);
                
                // Execute the job — this calls emailService.SendAsync(...) internally.
                // The job is a Func<CancellationToken, Task> so we pass stoppingToken
                // to allow the job itself to be canceled if the app shuts down mid-send.
                await job(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Any other error (SMTP failure, network timeout, wrong credentials, etc.)
                // We log it but DO NOT rethrow — rethrowing would kill the entire worker
                // and no future emails would ever be sent for the rest of the app's lifetime.
                // By catching here, the loop continues and the next job is processed normally.
                logger.LogError(ex, "Error occurred while sending background email.");
            }
        }
    }
}