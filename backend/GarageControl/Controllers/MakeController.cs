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
                var id = await _makeService.CreateMake(model, userId);
                return Ok(new { success = true, id });
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

        [HttpPost("promote-model")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteModel([FromBody] PromoteModelRequest request)
        {
            await _makeService.PromoteModelSuggestion(request.MakeName, request.ModelName, request.NewModelName, request.NewMakeName);
            return Ok(new { success = true });
        }

        [HttpPost("merge-with-global")]
        public async Task<IActionResult> MergeMakeWithGlobal([FromBody] MergeMakeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _makeService.MergeMakeWithGlobal(request.CustomMakeId, request.GlobalMakeId, userId);
                return Ok(new { success = true });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class PromoteRequest 
    {
        public string Name { get; set; } = null!;
        public string? NewName { get; set; }
    }

    public class PromoteModelRequest
    {
        public string MakeName { get; set; } = null!;
        public string? NewMakeName { get; set; }
        public string ModelName { get; set; } = null!;
        public string? NewModelName { get; set; }
    }

    public class MergeMakeRequest
    {
        public string CustomMakeId { get; set; } = null!;
        public string GlobalMakeId { get; set; } = null!;
    }
}
