using GarageControl.Core.Contracts;

namespace GarageControl.BackgroundServices
{
    public class NotificationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public NotificationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<NotificationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider
                            .GetRequiredService<INotificationService>();

                        await notificationService.DeleteOldNotificationsAsync();
                        _logger.LogInformation("Old notifications cleaned up successfully");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up old notifications");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
