using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;

namespace GarageControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly ICarServiceService _carServiceService;
        public ServiceController(ICarServiceService carServiceService)
        {
            _carServiceService = carServiceService;
        }
        [HttpGet("has-service")]
        public async Task<IActionResult> HasService()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var service = await _carServiceService.GetServiceDetailsByUser(userId);
            if (service == null)
            {
                return Ok(new { hasService = false });
            }
            return Ok(new { hasService = true });
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ServiceVM service)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { message = "Invalid model", errors });
            }
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            await _carServiceService.CreateService(user?.Value, service);

            return Ok(new { message = "Service created successfully." });
        }
        [HttpGet("details")]
        public async Task<IActionResult> Details()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var service = await _carServiceService.GetServiceDetailsByUser(userId);
            if (service == null)
            {
                return NotFound(new { message = "Service not found." });
            }
            return Ok(service);
        }
        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] ServiceVM service)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { message = "Invalid model", errors });
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _carServiceService.UpdateServiceDetails(userId, service);
            return Ok(new { message = "Service edited successfully." });
        }
    }
}