using GarageControl.Core.Services;
using GarageControl.Core.ViewModels.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string GetWorkshopId()
        {
            return User.FindFirst("WorkshopId")?.Value!;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(GetWorkshopId());
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderViewModel model)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(GetWorkshopId(), model);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id, GetWorkshopId());
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(string id, [FromBody] UpdateOrderViewModel model)
        {
            try
            {
                var result = await _orderService.UpdateOrderAsync(id, GetWorkshopId(), model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetMyJobs()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var jobs = await _orderService.GetMyJobsAsync(userId, GetWorkshopId());
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetJobById(string jobId)
        {
            try
            {
                var job = await _orderService.GetJobByIdAsync(jobId, GetWorkshopId());
                if (job == null) return NotFound();
                return Ok(job);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/job")]
        public async Task<IActionResult> CreateJob(string id, [FromBody] CreateJobViewModel model)
        {
            try
            {
                await _orderService.CreateJobAsync(id, GetWorkshopId(), model);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("job/{jobId}")]
        public async Task<IActionResult> UpdateJob(string jobId, [FromBody] UpdateJobViewModel model)
        {
            try
            {
                await _orderService.UpdateJobAsync(jobId, GetWorkshopId(), model);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
