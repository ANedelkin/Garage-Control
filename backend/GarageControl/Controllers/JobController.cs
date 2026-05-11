using GarageControl.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using GarageControl.Core.ViewModels.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GarageControl.Core.Attributes;

namespace GarageControl.Controllers
{
    [Authorize]
    [RequireAccess("Orders", "To Do")]
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        private string GetWorkshopId()
        {
            return User.FindFirst("WorkshopId")?.Value!;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }

        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetMyJobs()
        {
            try
            {
                var jobs = await _jobService.GetMyJobsAsync(GetUserId(), GetWorkshopId());
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpGet("worker/{workerId}")]
        public async Task<IActionResult> GetJobsByWorkerId(string workerId)
        {
            try
            {
                var jobs = await _jobService.GetJobsByWorkerIdAsync(workerId, GetWorkshopId());
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{jobId}")]
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

        [HttpGet("archived/{jobId}")]
        public async Task<IActionResult> GetArchivedJobById(string jobId)
        {
            try
            {
                var job = await _jobService.GetArchivedJobByIdAsync(jobId, GetWorkshopId());
                if (job == null) return NotFound();
                return Ok(job);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetJobsByOrderId(string orderId)
        {
            try
            {
                var jobs = await _jobService.GetJobsByOrderIdAsync(orderId, GetWorkshopId());
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("order/{orderId}")]
        public async Task<IActionResult> CreateJob(string orderId, [FromBody] CreateJobVM model)
        {
            try
            {
                var result = await _jobService.CreateJobAsync(GetUserId(), orderId, GetWorkshopId(), model);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{jobId}")]
        public async Task<IActionResult> UpdateJob(string jobId, [FromBody] UpdateJobVM model)
        {
            try
            {
                var result = await _jobService.UpdateJobAsync(GetUserId(), jobId, GetWorkshopId(), model);
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{jobId}")]
        public async Task<IActionResult> DeleteJob(string jobId)
        {
            try
            {
                var result = await _jobService.DeleteJobAsync(GetUserId(), jobId, GetWorkshopId());
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }
                return Ok();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This job is referenced by other records and cannot be deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("busy-slots")]
        public async Task<IActionResult> GetBusySlots(string workerId, DateTime start, DateTime end, string? excludeJobId = null)
        {
            try
            {
                // Ensure dates are UTC for PostgreSQL compatibility
                var utcStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
                var utcEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc);

                var slots = await _jobService.GetBusySlotsAsync(workerId, utcStart, utcEnd, excludeJobId);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
