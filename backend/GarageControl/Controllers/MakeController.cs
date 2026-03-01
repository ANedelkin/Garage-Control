using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Vehicles;
using GarageControl.Core.Attributes;

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

        [RequireAccess("Makes and Models", "Admin", "Cars", "Clients")]
        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var makes = await _makeService.GetMakes(userId);
            return Ok(makes);
        }

        [RequireAccess("Makes and Models", "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] MakeVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var id = await _makeService.CreateMake(model, userId);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                 return BadRequest(ex.Message);
            }
        }

        [RequireAccess("Makes and Models", "Admin")]
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] MakeVM model)
        {
             if (!ModelState.IsValid) return BadRequest(ModelState);
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             try 
              {
                 await _makeService.UpdateMake(id, model, userId!);
                 return Ok();
              } 
             catch(UnauthorizedAccessException) 
              {
                 return Forbid();
              }
        }

        [HttpGet("suggestions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSuggestions()
        {
            var suggestions = await _makeService.GetSuggestions();
            return Ok(suggestions);
        }

        [HttpGet("suggestions/{makeName}/models")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSuggestedModels(string makeName)
        {
            var models = await _makeService.GetSuggestedModels(makeName);
            return Ok(models);
        }

        [HttpPost("promote")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteSource([FromBody] PromoteRequest request)
        {
             await _makeService.PromoteSuggestion(request.Name, request.NewName);
             return Ok();
        }

        [HttpDelete("{id}")]
        [RequireAccess("Makes and Models", "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _makeService.DeleteMake(id, userId);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("promote-model")]
        [RequireAccess("Makes and Models", "Admin")]
        public async Task<IActionResult> PromoteModel([FromBody] PromoteModelRequest request)
        {
            await _makeService.PromoteModelSuggestion(request.MakeName, request.ModelName, request.NewModelName, request.NewMakeName);
            return Ok();
        }

        [HttpPost("merge-with-global")]
        [RequireAccess("Makes and Models")]
        public async Task<IActionResult> MergeMakeWithGlobal([FromBody] MergeMakeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _makeService.MergeMakeWithGlobal(request.CustomMakeId, request.GlobalMakeId, userId);
                return Ok();
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
