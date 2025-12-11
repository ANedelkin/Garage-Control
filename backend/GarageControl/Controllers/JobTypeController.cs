using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using System.Security.Claims;

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

        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var jobTypes = await _jobTypeService.All(userId);
            return Ok(jobTypes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var jobType = await _jobTypeService.Details(id);
            if (jobType == null) return NotFound();
            return Ok(jobType);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] JobTypeVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _jobTypeService.Create(model, userId);
            return Ok(new { message = "Job type created successfully" });
        }

        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] JobTypeVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            // Check if it's a new item masquerading as edit (empty ID)
            if (string.IsNullOrEmpty(model.Id))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _jobTypeService.Create(model, userId);
                return Ok(new { message = "Job type created successfully" });
            }

            await _jobTypeService.Edit(model);
            return Ok(new { message = "Job type updated successfully" });
        }
    }
}
