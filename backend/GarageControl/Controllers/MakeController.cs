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
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             try 
             {
                await _makeService.UpdateMake(model, userId);
                return Ok(new { success = true });
             } 
             catch(UnauthorizedAccessException) 
             {
                 return Forbid();
             }
        }

        [HttpGet("suggestions")]
        // [Authorize(Roles = "Admin")] // Uncomment if Role-based auth is fully set up
        public async Task<IActionResult> GetSuggestions()
        {
            var suggestions = await _makeService.GetSuggestions();
            return Ok(suggestions);
        }

        [HttpGet("suggestions/{makeName}/models")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSuggestedModels(string makeName)
        {
            var models = await _makeService.GetSuggestedModels(makeName);
            return Ok(models);
        }

        [HttpPost("promote")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteSource([FromBody] PromoteRequest request)
        {
             await _makeService.PromoteSuggestion(request.Name, request.NewName);
             return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _makeService.DeleteMake(id, userId);
                return Ok(new { success = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }

    public class PromoteRequest 
    {
        public string Name { get; set; } = null!;
        public string? NewName { get; set; }
    }
}
