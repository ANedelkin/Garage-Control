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
            var models = await _modelService.GetModels(makeId);
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
             await _modelService.UpdateModel(model);
             return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _modelService.DeleteModel(id);
            return Ok(new { success = true });
        }
    }
}
