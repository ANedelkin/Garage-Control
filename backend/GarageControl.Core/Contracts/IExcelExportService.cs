using GarageControl.Core.ViewModels.Clients;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Core.ViewModels.Workers;
using GarageControl.Core.ViewModels.Vehicles;

namespace GarageControl.Core.Contracts
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportOrdersAsync(List<(OrderListVM Order, List<JobListVM> Jobs)> ordersWithJobs);
        Task<byte[]> ExportClientsAsync(IEnumerable<ClientVM> clients);
        Task<byte[]> ExportWorkersAsync(IEnumerable<WorkerVM> workers, List<string> exportTypes);
        Task<byte[]> ExportToDoAsync(IEnumerable<JobToDoVM> jobs, string workerName);
        Task<byte[]> ExportPartsAsync(List<PartVM> parts);
        Task<byte[]> ExportClientDetailsAsync(ClientVM client);
        Task<byte[]> ExportWorkerScheduleAsync(WorkerVM worker);
        Task<byte[]> ExportJobAsync(JobDetailsVM job);
        Task<byte[]> ExportJobTypesAsync(IEnumerable<JobTypeVM> jobTypes);
        Task<byte[]> ExportCarsAsync(IEnumerable<VehicleVM> cars);
    }
}
