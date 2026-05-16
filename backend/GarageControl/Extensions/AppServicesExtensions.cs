using GarageControl.Infrastructure.Data.Common;
using GarageControl.Core.Contracts;
using GarageControl.Core.Services;
using GarageControl.Core.Services.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace GarageControl.Extensions
{
    public static class AppServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IWorkshopService, WorkshopService>();
            services.AddScoped<IJobTypeService, JobTypeService>();
            services.AddScoped<IMakeService, MakeService>();
            services.AddScoped<IModelService, ModelService>();
            services.AddScoped<IWorkerService, WorkerService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IJobService, JobService>();
            services.AddScoped<IPartService, PartService>();
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IDeficitService, DeficitService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IActivityLogService, ActivityLogService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IPdfExportService, PdfExportService>();

            services.AddHostedService<GarageControl.BackgroundServices.NotificationCleanupService>();

            return services;
        }
    }
}
