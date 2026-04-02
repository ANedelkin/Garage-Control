using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using GarageControl.Infrastructure.Data.Models;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace GarageControl.Infrastructure.Interceptors
{
    public class ActivityLogInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private List<ActivityLog> _logsToSave = new();

        public ActivityLogInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            _logsToSave.Clear();

            // Example: We could track explicit actions here in the future
            // For now, implicit actions are not logged automatically because we lack actor/workshop context
            // that our UI requires. But this is the hook where it would be added.

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (_logsToSave.Any() && eventData.Context != null)
            {
                // We'd save the queued implicit logs here
                // eventData.Context.AddRange(_logsToSave);
                // await eventData.Context.SaveChangesAsync(cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
