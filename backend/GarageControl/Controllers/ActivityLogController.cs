using GarageControl.Core.Contracts;
using GarageControl.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Attributes;

namespace GarageControl.Controllers
{
    [Authorize]
    [RequireAccess("Activity Log")]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService;
        }

        private string GetWorkshopId()
        {
            return User.FindFirst("WorkshopId")?.Value!;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var (logs, totalCount) = await _activityLogService.GetLogsAsync(GetWorkshopId(), skip, take, startDate, endDate, search);
                return Ok(new { logs, totalCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
