using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MakeController : ControllerBase
    {
        private readonly IMakeService _makeService;

        public MakeController(IMakeService makeService)
        {
            _makeService = makeService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var makes = await _makeService.GetMakes(userId);
            return Ok(makes);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] MakeVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _makeService.CreateMake(model, userId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                 return BadRequest(ex.Message);
            }
        }

        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] MakeVM model)
        {
             if (!ModelState.IsValid) return BadRequest(ModelState);
             await _makeService.UpdateMake(model);
             return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _makeService.DeleteMake(id);
            return Ok(new { success = true });
        }
    }
}
