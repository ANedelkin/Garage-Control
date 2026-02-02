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
    public class ModelController : ControllerBase
    {
        private readonly IModelService _modelService;

        public ModelController(IModelService modelService)
        {
            _modelService = modelService;
        }

        [HttpGet("all/{makeId}")]
        public async Task<IActionResult> All(string makeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var models = await _modelService.GetModels(makeId, userId);
            return Ok(models);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ModelVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _modelService.CreateModel(model, userId);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] ModelVM model)
        {
             if (!ModelState.IsValid) return BadRequest(ModelState);
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             try
             {
                 await _modelService.UpdateModel(model, userId);
                 return Ok(new { success = true });
             }
             catch(UnauthorizedAccessException)
             {
                 return Forbid();
             }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _modelService.DeleteModel(id, userId);
                return Ok(new { success = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("merge-with-global")]
        public async Task<IActionResult> MergeModelWithGlobal([FromBody] MergeModelRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _modelService.MergeModelWithGlobal(request.CustomModelId, request.GlobalModelId, userId);
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

    public class MergeModelRequest
    {
        public string CustomModelId { get; set; } = null!;
        public string GlobalModelId { get; set; } = null!;
    }
}
