using GarageControl.Core.Contracts;
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
        private readonly IJobService _jobService;

        public OrderController(IOrderService orderService, IJobService jobService)
        {
            _orderService = orderService;
            _jobService = jobService;
        }

        private string GetWorkshopId()
        {
            return User.FindFirst("WorkshopId")?.Value!;
        }

        private string GetUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
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

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(GetWorkshopId(), isDone: false);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(GetWorkshopId(), isDone: true);
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
                var order = await _orderService.CreateOrderAsync(GetUserId(), GetWorkshopId(), model);
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
                var result = await _orderService.UpdateOrderAsync(GetUserId(), id, GetWorkshopId(), model);
                if (result is Core.Models.MethodResponse resp && !resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }
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

                var jobs = await _jobService.GetMyJobsAsync(userId, GetWorkshopId());
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
                var job = await _jobService.GetJobByIdAsync(jobId, GetWorkshopId());
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
                var result = await _jobService.CreateJobAsync(GetUserId(), id, GetWorkshopId(), model);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }
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
                var result = await _jobService.UpdateJobAsync(GetUserId(), jobId, GetWorkshopId(), model);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
