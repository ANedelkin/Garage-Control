using GarageControl.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private string GetGarageId()
        {
            return User.FindFirst("GarageId")?.Value!;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            try
            {
                var data = await _dashboardService.GetDashboardDataAsync(GetGarageId());
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
