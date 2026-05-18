using GarageControl.Core.ViewModels.Clients;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Core.ViewModels.Workers;
using GarageControl.Core.ViewModels.Vehicles;
using GarageControl.Core.Enums;

namespace GarageControl.Core.Contracts
{
    public interface IExcelExportService
    {
        byte[] ExportOrders(List<(OrderListVM Order, List<JobListVM> Jobs)> ordersWithJobs);
        byte[] ExportClients(IEnumerable<ClientVM> clients);
        byte[] ExportWorkers(IEnumerable<WorkerVM> workers, WorkerExportFlags exportFlags);
        byte[] ExportToDo(IEnumerable<JobToDoVM> jobs, string workerName);
        byte[] ExportParts(List<PartVM> parts);

        byte[] ExportJob(JobDetailsVM job);
        byte[] ExportJobTypes(IEnumerable<JobTypeVM> jobTypes);
        byte[] ExportCars(IEnumerable<VehicleVM> cars);
    }
}
