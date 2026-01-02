using GarageControl.Core.Services;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PartController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartController(IPartService partService)
        {
            _partService = partService;
        }

        private string GetGarageId()
        {
            return User.FindFirst("GarageId")?.Value!;
        }

        [HttpGet("folder-content")]
        public async Task<IActionResult> GetFolderContent([FromQuery] string? folderId)
        {
            try
            {
                var content = await _partService.GetFolderContentAsync(GetGarageId(), folderId);
                return Ok(content);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePart([FromBody] CreatePartViewModel model)
        {
            try
            {
                var part = await _partService.CreatePartAsync(GetGarageId(), model);
                return Ok(part);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdatePart([FromBody] UpdatePartViewModel model)
        {
            try
            {
                await _partService.EditPartAsync(GetGarageId(), model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePart(string id)
        {
            try
            {
                await _partService.DeletePartAsync(GetGarageId(), id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("folder/create")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderViewModel model)
        {
            try
            {
                var folder = await _partService.CreateFolderAsync(GetGarageId(), model);
                return Ok(folder);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("folder/rename/{id}")]
        public async Task<IActionResult> RenameFolder(string id, [FromBody] string newName)
        {
            try
            {
                await _partService.RenameFolderAsync(GetGarageId(), id, newName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("folder/delete/{id}")]
        public async Task<IActionResult> DeleteFolder(string id)
        {
            try
            {
                await _partService.DeleteFolderAsync(GetGarageId(), id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
