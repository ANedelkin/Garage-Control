using GarageControl.Core.Contracts;
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
        public async Task<IActionResult> ToggleUserBlock(string userId)
        {
            var result = await _adminService.ToggleUserBlockAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("workshops")]
        public async Task<IActionResult> GetWorkshops()
        {
            var workshops = await _adminService.GetWorkshopsAsync();
            return Ok(workshops);
        }

        [HttpPost("workshops/{workshopId}/toggle-block")]
        public async Task<IActionResult> ToggleWorkshopBlock(string workshopId)
        {
            var result = await _adminService.ToggleWorkshopBlockAsync(workshopId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
