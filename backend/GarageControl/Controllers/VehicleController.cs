using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Vehicles;
using GarageControl.Core.Attributes;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [RequireAccess("Cars", "Orders")]
        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _vehicleService.All(userId);
            return Ok(result);
        }
        [RequireAccess("Clients")]
        [HttpGet("by-client/{clientId}")]
        public async Task<IActionResult> GetByClient(string clientId)
        {
            var result = await _vehicleService.GetByClient(clientId);
            return Ok(result);
        }
       [RequireAccess("Cars")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] VehicleVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _vehicleService.Create(model, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [RequireAccess("Cars")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var result = await _vehicleService.Details(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [RequireAccess("Cars")]
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] VehicleVM model)
        {
             if (!ModelState.IsValid) return BadRequest(ModelState);
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             await _vehicleService.Edit(id, model, userId!);
             return Ok();
        }
        [RequireAccess("Cars")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _vehicleService.Delete(id, userId);
            return Ok();
        }
    }
}
