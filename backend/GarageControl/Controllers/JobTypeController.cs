using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Jobs;
using System.Security.Claims;
using GarageControl.Core.Attributes;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JobTypeController : ControllerBase
    {
        private readonly IJobTypeService _jobTypeService;

        public JobTypeController(IJobTypeService jobTypeService)
        {
            _jobTypeService = jobTypeService;
        }

        [RequireAccess("Job Types", "Orders", "Workers")]
        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var jobTypes = await _jobTypeService.All(userId);
            return Ok(jobTypes);
        }
        [RequireAccess("Job Types")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var jobType = await _jobTypeService.Details(id);
            if (jobType == null) return NotFound();
            return Ok(jobType);
        }
        [RequireAccess("Job Types")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] JobTypeVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _jobTypeService.Create(model, userId);
            return Ok(new { message = "Job type created successfully" });
        }
        [RequireAccess("Job Types")]
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] JobTypeVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            await _jobTypeService.Edit(id, model, userId);
            return Ok(new { message = "Job type updated successfully" });
        }
        [RequireAccess("Job Types")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            try
            {
                await _jobTypeService.Delete(id, userId);
                return Ok(new { message = "Job type deleted successfully" });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This job type is in use by one or more jobs and cannot be deleted." });
            }
        }
    }
}
