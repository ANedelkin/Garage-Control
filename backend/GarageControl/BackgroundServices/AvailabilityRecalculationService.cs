using GarageControl.Core.Contracts;
using GarageControl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarageControl.BackgroundServices
{
    public class AvailabilityRecalculationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AvailabilityRecalculationService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public AvailabilityRecalculationService(
            IServiceProvider serviceProvider,
            ILogger<AvailabilityRecalculationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Availability recalculation service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<GarageControlDbContext>();
                        var partService = scope.ServiceProvider.GetRequiredService<IPartService>();

                        var workshopIds = await db.Workshops.Select(w => w.Id).ToListAsync(stoppingToken);

                        foreach (var workshopId in workshopIds)
                        {
                            await partService.RecalculateAvailabilityBalanceAsync(workshopId);
                            _logger.LogInformation("Recalculated availability for workshop {WorkshopId}", workshopId);
                        }

                        _logger.LogInformation("Availability recalculation completed successfully");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while recalculating availability balances");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}