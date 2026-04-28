using GarageControl.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly IExcelExportService _exportService;
        private readonly IPdfExportService _pdfService;
        private readonly IOrderService _orderService;
        private readonly IJobService _jobService;
        private readonly IClientService _clientService;
        private readonly IWorkerService _workerService;
        private readonly IPartService _partService;
        private readonly IJobTypeService _jobTypeService;
        private readonly IVehicleService _vehicleService;

        public ExportController(
            IExcelExportService exportService,
            IPdfExportService pdfService,
            IOrderService orderService,
            IJobService jobService,
            IClientService clientService,
            IWorkerService workerService,
            IPartService partService,
            IJobTypeService jobTypeService,
            IVehicleService vehicleService)
        {
            _exportService = exportService;
            _pdfService = pdfService;
            _orderService = orderService;
            _jobService = jobService;
            _clientService = clientService;
            _workerService = workerService;
            _partService = partService;
            _jobTypeService = jobTypeService;
            _vehicleService = vehicleService;
        }

        private string GetWorkshopId() => User.FindFirst("WorkshopId")?.Value!;
        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

        [HttpGet("orders")]
        public async Task<IActionResult> ExportOrders([FromQuery] bool? isDone, [FromQuery] string format = "excel")
        {
            var orders = await _orderService.GetOrdersAsync(GetWorkshopId(), isDone);
            var ordersWithJobs = new List<(GarageControl.Core.ViewModels.Orders.OrderListVM Order, List<GarageControl.Core.ViewModels.Jobs.JobListVM> Jobs)>();

            foreach (var order in orders)
            {
                var jobs = await _jobService.GetJobsByOrderIdAsync(order.Id, GetWorkshopId());
                ordersWithJobs.Add((order, jobs));
            }

            var bytes = format.ToLower() == "pdf" 
                ? await _pdfService.ExportOrdersAsync(ordersWithJobs)
                : await _exportService.ExportOrdersAsync(ordersWithJobs);

            string filename = isDone == true ? "Done_Orders" : "Active_Orders";
            return ExportFile(bytes, filename, format);
        }

        [HttpGet("clients")]
        public async Task<IActionResult> ExportClients([FromQuery] string format = "excel")
        {
            var clients = await _clientService.All(GetUserId());
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportClientsAsync(clients)
                : await _exportService.ExportClientsAsync(clients);
            return ExportFile(bytes, "Clients", format);
        }

        [HttpGet("workers")]
        public async Task<IActionResult> ExportWorkers([FromQuery] string types = "details", [FromQuery] string format = "excel")
        {
            var workers = await _workerService.All(GetUserId());
            var exportTypes = types.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportWorkersAsync(workers, exportTypes)
                : await _exportService.ExportWorkersAsync(workers, exportTypes);
            
            return ExportFile(bytes, "Workers", format);
        }

        [HttpGet("parts")]
        public async Task<IActionResult> ExportParts([FromQuery] string format = "excel")
        {
            var parts = await _partService.GetAllPartsAsync(GetWorkshopId());
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportPartsAsync(parts)
                : await _exportService.ExportPartsAsync(parts);
            return ExportFile(bytes, "Parts_Stock", format);
        }

        [HttpGet("client/{id}")]
        public async Task<IActionResult> ExportClientDetails(string id)
        {
            var client = await _clientService.Details(id);
            if (client == null) return NotFound();

            var bytes = await _exportService.ExportClientDetailsAsync(client);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Client_{client.Name}.xlsx");
        }

        [HttpGet("worker/{id}/schedule")]
        public async Task<IActionResult> ExportWorkerSchedule(string id)
        {
            var worker = await _workerService.Details(id);
            if (worker == null) return NotFound();

            var bytes = await _exportService.ExportWorkerScheduleAsync(worker);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Schedule_{worker.Name}.xlsx");
        }

        [HttpGet("job/{id}")]
        public async Task<IActionResult> ExportJob(string id)
        {
            var job = await _jobService.GetCompletedJobByIdAsync(id, GetWorkshopId());
            if (job == null) job = await _jobService.GetJobByIdAsync(id, GetWorkshopId());
            if (job == null) return NotFound();

            var bytes = await _exportService.ExportJobAsync(job);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Job_{job.JobTypeName}.xlsx");
        }

        [HttpGet("job-types")]
        public async Task<IActionResult> ExportJobTypes([FromQuery] string format = "excel")
        {
            var jobTypes = await _jobTypeService.All(GetUserId());
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportJobTypesAsync(jobTypes)
                : await _exportService.ExportJobTypesAsync(jobTypes);
            return ExportFile(bytes, "Job_Types", format);
        }

        [HttpGet("cars")]
        public async Task<IActionResult> ExportCars([FromQuery] string format = "excel")
        {
            var cars = await _vehicleService.All(GetUserId());
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportCarsAsync(cars)
                : await _exportService.ExportCarsAsync(cars);
            return ExportFile(bytes, "Cars", format);
        }

        [HttpGet("todo")]
        public async Task<IActionResult> ExportToDo([FromQuery] string? workerId, [FromQuery] string format = "excel")
        {
            var targetWorkerId = workerId ?? User.FindFirst("WorkerId")?.Value;
            if (string.IsNullOrEmpty(targetWorkerId)) return BadRequest("WorkerId not found");

            var jobs = await _jobService.GetJobsByWorkerIdAsync(targetWorkerId, GetWorkshopId());
            var worker = await _workerService.Details(targetWorkerId);
            
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportToDoAsync(jobs, worker?.Name ?? "Worker")
                : await _exportService.ExportToDoAsync(jobs, worker?.Name ?? "Worker");

            string filename = worker != null ? $"ToDo_{worker.Name}" : "ToDo_List";
            return ExportFile(bytes, filename, format);
        }

        private IActionResult ExportFile(byte[] bytes, string filename, string format)
        {
            string contentType = format.ToLower() == "pdf" 
                ? "application/pdf" 
                : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            string extension = format.ToLower() == "pdf" ? ".pdf" : ".xlsx";
            return File(bytes, contentType, filename + extension);
        }
    }
}
