using GarageControl.Core.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Jobs;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly IExcelExportService _excelService;
        private readonly IPdfExportService _pdfService;
        private readonly IOrderService _orderService;
        private readonly IJobService _jobService;
        private readonly IClientService _clientService;
        private readonly IWorkerService _workerService;
        private readonly IPartService _partService;
        private readonly IJobTypeService _jobTypeService;
        private readonly IVehicleService _vehicleService;

        public ExportController(
            IExcelExportService excelService,
            IPdfExportService pdfService,
            IOrderService orderService,
            IJobService jobService,
            IClientService clientService,
            IWorkerService workerService,
            IPartService partService,
            IJobTypeService jobTypeService,
            IVehicleService vehicleService)
        {
            _excelService = excelService;
            _pdfService = pdfService;
            _orderService = orderService;
            _jobService = jobService;
            _clientService = clientService;
            _workerService = workerService;
            _partService = partService;
            _jobTypeService = jobTypeService;
            _vehicleService = vehicleService;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> ExportOrders([FromQuery] bool? getArchived, [FromQuery] string format = "excel")
        {
            var orders = await _orderService.GetOrdersAsync(User.GetWorkshopId(), getArchived);
            var ordersWithJobs = new List<(OrderListVM Order, List<JobListVM> Jobs)>();

            foreach (var order in orders)
            {
                var jobs = await _jobService.GetJobsByOrderIdAsync(order.Id, User.GetWorkshopId());
                ordersWithJobs.Add((order, jobs));
            }

            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportOrdersAsync(ordersWithJobs)
                : await _excelService.ExportOrdersAsync(ordersWithJobs);

            string filename = getArchived == true ? "Archived_Orders" : "Active_Orders";
            return ExportFile(bytes, filename, format);
        }

        [HttpGet("clients")]
        public async Task<IActionResult> ExportClients([FromQuery] string format = "excel")
        {
            var clients = await _clientService.All(User.GetUserId());
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportClientsAsync(clients)
                : await _excelService.ExportClientsAsync(clients);
            return ExportFile(bytes, "Clients", format);
        }

        [HttpGet("workers")]
        public async Task<IActionResult> ExportWorkers([FromQuery] string types = "details", [FromQuery] string format = "excel")
        {
            var workers = await _workerService.All(User.GetUserId());
            var exportTypes = types.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportWorkersAsync(workers, exportTypes)
                : await _excelService.ExportWorkersAsync(workers, exportTypes);
            
            return ExportFile(bytes, "Workers", format);
        }

        [HttpGet("parts")]
        public async Task<IActionResult> ExportParts([FromQuery] string format = "excel")
        {
            var parts = await _partService.GetAllPartsAsync(User.GetWorkshopId());
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportPartsAsync(parts)
                : await _excelService.ExportPartsAsync(parts);
            return ExportFile(bytes, "Parts_Stock", format);
        }



        [HttpGet("job/{id}")]
        public async Task<IActionResult> ExportJob(string id, [FromQuery] string format = "excel")
        {
            var job = await _jobService.GetArchivedJobByIdAsync(id, User.GetWorkshopId());
            if (job == null) job = await _jobService.GetJobByIdAsync(id, User.GetWorkshopId());
            if (job == null) return NotFound();

            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportJobAsync(job)
                : await _excelService.ExportJobAsync(job);

            return ExportFile(bytes, $"Job_{job.JobTypeName}", format);
        }

        [HttpGet("job-types")]
        public async Task<IActionResult> ExportJobTypes([FromQuery] string format = "excel")
        {
            var jobTypes = await _jobTypeService.All(User.GetUserId());
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportJobTypesAsync(jobTypes)
                : await _excelService.ExportJobTypesAsync(jobTypes);
            return ExportFile(bytes, "Job_Types", format);
        }

        [HttpGet("cars")]
        public async Task<IActionResult> ExportCars([FromQuery] string format = "excel")
        {
            var cars = await _vehicleService.All(User.GetUserId());
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportCarsAsync(cars)
                : await _excelService.ExportCarsAsync(cars);
            return ExportFile(bytes, "Cars", format);
        }

        [HttpGet("todo")]
        public async Task<IActionResult> ExportToDo([FromQuery] string? workerId, [FromQuery] string format = "excel")
        {
            var targetWorkerId = workerId ?? User.FindFirst("WorkerId")?.Value;
            if (string.IsNullOrEmpty(targetWorkerId)) return BadRequest("WorkerId not found");

            var jobs = await _jobService.GetJobsByWorkerIdAsync(targetWorkerId, User.GetWorkshopId());
            var worker = await _workerService.Details(targetWorkerId);
            
            var bytes = format.ToLower() == "pdf"
                ? await _pdfService.ExportToDoAsync(jobs, worker?.Name ?? "Worker")
                : await _excelService.ExportToDoAsync(jobs, worker?.Name ?? "Worker");

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
