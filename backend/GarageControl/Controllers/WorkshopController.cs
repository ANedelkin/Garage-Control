using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Workshop;
using GarageControl.Core.Attributes;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkshopController : ControllerBase
    {
        private readonly IWorkshopService _workshopService;
        private readonly IAuthService _authService;

        public WorkshopController(IWorkshopService workshopService, IAuthService authService)
        {
            _workshopService = workshopService;
            _authService = authService;
        }
        [HttpGet("has-workshop")]
        public async Task<IActionResult> HasWorkshop()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workshop = await _workshopService.GetWorkshopDetailsByUser(userId);
            if (workshop == null)
            {
                return Ok(new { hasWorkshop = false });
            }
            return Ok(new { hasWorkshop = true });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] WorkshopVM workshop)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { message = "Invalid model", errors });
            }
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated or token is invalid." });
            }
            
            var result = await _workshopService.CreateWorkshop(userId, workshop);

            if (result.Success)
            {
                await _authService.SetAuthCookies(Response, result);
                return Ok(result);
            }

            return BadRequest(result);
        }

        [RequireAccess("Workshop Details")]
        [HttpGet("details")]
        public async Task<IActionResult> Details()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workshop = await _workshopService.GetWorkshopDetailsByUser(userId);
            if (workshop == null)
            {
                return NotFound(new { message = "Workshop not found." });
            }
            return Ok(workshop);
        }
        [RequireAccess("Workshop Details")]
        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] WorkshopVM workshop)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { message = "Invalid model", errors });
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _workshopService.UpdateWorkshopDetails(userId, workshop);
            return Ok(new { message = "Workshop edited successfully." });
        }
    }
}
