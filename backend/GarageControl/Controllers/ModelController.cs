using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Vehicles;
using GarageControl.Core.Attributes;

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

        [RequireAccess("Makes and Models", "Admin", "Clients")]
        [HttpGet("all/{makeId}")]
        public async Task<IActionResult> All(string makeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var models = await _modelService.GetModels(makeId, userId);
            return Ok(models);
        }

        [RequireAccess("Makes and Models", "Admin", "Clients")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await _modelService.GetModel(id, userId);
            return Ok(model);
        }

        [RequireAccess("Makes and Models", "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ModelVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _modelService.CreateModel(model, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [RequireAccess("Makes and Models", "Admin")]
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] ModelVM model)
        {
             if (!ModelState.IsValid) return BadRequest(ModelState);
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             try
             {
                 await _modelService.UpdateModel(id, model, userId!);
                 return Ok();
             }
             catch(UnauthorizedAccessException)
             {
                 return Forbid();
             }
        }
        [RequireAccess("Makes and Models", "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _modelService.DeleteModel(id, userId);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This model is used by existing vehicles and cannot be deleted." });
            }
        }
        [RequireAccess("Makes and Models")]
        [HttpPost("merge-with-global")]
        public async Task<IActionResult> MergeModelWithGlobal([FromBody] MergeModelRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _modelService.MergeModelWithGlobal(request.CustomModelId, request.GlobalModelId, userId);
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

    public class MergeModelRequest
    {
        public string CustomModelId { get; set; } = null!;
        public string GlobalModelId { get; set; } = null!;
    }
}
