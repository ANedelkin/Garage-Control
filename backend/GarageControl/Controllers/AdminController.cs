using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GarageControl.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _adminService.GetUsersAsync();
            return Ok(users);
        }

        [HttpPost("users/{userId}/toggle-block")]
        public async Task<IActionResult> ToggleUserBlock(string userId, [FromQuery] string? reason)
        {
            var result = await _adminService.ToggleUserBlockAsync(userId, reason);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("workshops")]
        public async Task<IActionResult> GetWorkshops()
        {
            var workshops = await _adminService.GetWorkshopsAsync();
            return Ok(workshops);
        }

        [HttpPost("workshops/{workshopId}/toggle-block")]
        public async Task<IActionResult> ToggleWorkshopBlock(string workshopId, [FromQuery] string? reason)
        {
            var result = await _adminService.ToggleWorkshopBlockAsync(workshopId, reason);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}
