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
    public class WorkerController : ControllerBase
    {
        private readonly IWorkerService _workerService;

        public WorkerController(IWorkerService workerService)
        {
            _workerService = workerService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workers = await _workerService.All(userId);
            return Ok(workers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var worker = await _workerService.Details(id);
            if (worker == null) return NotFound();
            return Ok(worker);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] WorkerVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _workerService.Create(model, userId);
                return Ok(new { message = "Worker created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] WorkerVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (string.IsNullOrEmpty(model.Id))
            {
                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                 try
                 {
                    await _workerService.Create(model, userId);
                    return Ok(new { message = "Worker created successfully" });
                 }
                 catch (Exception ex)
                 {
                    return BadRequest(new { message = ex.Message });
                 }
            }

            try
            {
                await _workerService.Edit(model);
                return Ok(new { message = "Worker updated successfully" });
            }
            catch (Exception ex)
            {
                 return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("accesses")]
        public async Task<IActionResult> AllAccesses()
        {
            var accesses = await _workerService.AllAccesses();
            return Ok(accesses);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _workerService.Delete(id);
             return Ok(new { message = "Worker deleted successfully" });
        }
    }
}
