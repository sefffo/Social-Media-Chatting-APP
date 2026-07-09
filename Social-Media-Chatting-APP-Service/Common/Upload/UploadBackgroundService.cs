using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Social_Media_Chatting_APP_Service.Common.Upload;

/// <summary>
/// A long-running background service that continuously listens for queued upload jobs
/// and executes them asynchronously, decoupled from the HTTP request pipeline.
/// Inherits from <see cref="BackgroundService"/> which is automatically started
/// and stopped by the .NET host lifecycle.
/// </summary>
public class UploadBackgroundService(
    BackgroundUploadQueue queue,
    ILogger<UploadBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[UploadWorker] Background upload processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // BLOCKS here — waits idly (no CPU waste) until a job is dropped
                // into the queue by UploadController.
                var job = await queue.DequeueAsync(stoppingToken);

                logger.LogInformation("[UploadWorker] Picked up upload job, executing...");

                await job(stoppingToken);

                logger.LogInformation("[UploadWorker] Upload job completed successfully.");
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("[UploadWorker] Shutdown requested, stopping upload worker.");
                break;
            }
            catch (Exception ex)
            {
                // Any Cloudinary failure (network timeout, auth error, etc.) lands here.
                // We do NOT rethrow — that would kill the worker permanently.
                logger.LogError(ex,
                    "[UploadWorker] FAILED to process upload job. " +
                    "Exception type: {ExType} | Message: {ExMessage}",
                    ex.GetType().Name, ex.Message);
            }
        }

        logger.LogInformation("[UploadWorker] Background upload processor stopped.");
    }
}