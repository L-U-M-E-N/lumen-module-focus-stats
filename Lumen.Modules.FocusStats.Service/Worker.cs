namespace Lumen.Modules.FocusStats.Service;

public sealed class WindowsBackgroundService(
    GetActivitiesService activityService,
    IConfiguration configuration,
    ILogger<WindowsBackgroundService> logger) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        GetActivitiesService.LUMEN_API_URL = configuration.GetValue<string>("LUMEN_API_URL") ?? throw new NullReferenceException(nameof(GetActivitiesService.LUMEN_API_URL));
        GetActivitiesService.LUMEN_API_KEY = configuration.GetValue<string>("LUMEN_API_KEY") ?? throw new NullReferenceException(nameof(GetActivitiesService.LUMEN_API_KEY));

        var lastSubmitTime = DateTime.UtcNow;
        try {
            while (!stoppingToken.IsCancellationRequested) {
                try {
                    logger.LogDebug("[Lumen.Modules.FocusStats.Service] Getting focused activity ...");
                    activityService.GetActivity();

                    if ((DateTime.UtcNow - lastSubmitTime) >= TimeSpan.FromMinutes(2)) {
                        logger.LogDebug("[Lumen.Modules.FocusStats.Service] Submitting data to the server ...");
                        lastSubmitTime = DateTime.UtcNow;
                        try {
                            await activityService.SubmitActivities();
                        } catch (Exception ex) {
                            logger.LogError(ex, "[Lumen.Modules.FocusStats.Service] SubmitActivities - {Message}", ex.Message);
                        }
                    }
                } catch (Exception ex) {
                    logger.LogError(ex, "[Lumen.Modules.FocusStats.Service]  {Message}", ex.Message);
                }
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        } catch (OperationCanceledException) {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
        } catch (Exception ex) {
            logger.LogError(ex, "[Lumen.Modules.FocusStats.Service]  {Message}", ex.Message);

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }
}
