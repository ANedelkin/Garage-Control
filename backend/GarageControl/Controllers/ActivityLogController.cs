using GarageControl.Core.Contracts;
using GarageControl.Core.Services;
using GarageControl.Shared.Constants;
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

        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] int page = 0,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var (logs, totalCount) = await _activityLogService.GetLogsAsync(User.GetWorkshopId(), page, startDate, endDate, search);
                return Ok(new { logs, totalCount, pageSize = ActivityLogConstants.DefaultPageSize });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
