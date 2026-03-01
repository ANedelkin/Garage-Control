using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Parts;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GarageControl.Core.Attributes;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PartController : ControllerBase
    {
        private readonly IPartService _partService;
        private readonly IFolderService _folderService;

        public PartController(IPartService partService, IFolderService folderService)
        {
            _partService = partService;
            _folderService = folderService;
        }

        private string GetWorkshopId()
        {
            return User.FindFirst("WorkshopId")?.Value!;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }

        [RequireAccess("Parts Stock")]
        [HttpGet("folder-content")]
        public async Task<IActionResult> GetFolderContent([FromQuery] string? folderId)
        {
            try
            {
                var content = await _folderService.GetFolderContentAsync(GetWorkshopId(), folderId);
                return Ok(content);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock", "Orders")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllParts()
        {
            try
            {
                var parts = await _partService.GetAllPartsAsync(GetWorkshopId());
                return Ok(parts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPartById(string id)
        {
            try
            {
                var part = await _partService.GetPartByIdAsync(id, GetWorkshopId());
                if (part == null) return NotFound();
                return Ok(part);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePart([FromBody] CreatePartVM model)
        {
            try
            {
                var part = await _partService.CreatePartAsync(GetUserId(), GetWorkshopId(), model);
                return Ok(part);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdatePart(string id, [FromBody] UpdatePartVM model)
        {
            try
            {
                await _partService.EditPartAsync(GetUserId(), GetWorkshopId(), id, model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePart(string id)
        {
            try
            {
                await _partService.DeletePartAsync(GetUserId(), GetWorkshopId(), id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpPost("folder/create")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderVM model)
        {
            try
            {
                var folder = await _folderService.CreateFolderAsync(GetUserId(), GetWorkshopId(), model);
                return Ok(folder);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpPut("folder/rename/{id}")]
        public async Task<IActionResult> RenameFolder(string id, [FromBody] string newName)
        {
            try
            {
                await _folderService.RenameFolderAsync(GetUserId(), GetWorkshopId(), id, newName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpDelete("folder/delete/{id}")]
        public async Task<IActionResult> DeleteFolder(string id)
        {
            try
            {
                await _folderService.DeleteFolderAsync(GetUserId(), GetWorkshopId(), id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpPut("move/{id}")]
        public async Task<IActionResult> MovePart(string id, [FromBody] string? newParentId)
        {
            try
            {
                await _partService.MovePartAsync(GetUserId(), GetWorkshopId(), id, newParentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [RequireAccess("Parts Stock")]
        [HttpPut("folder/move/{id}")]
        public async Task<IActionResult> MoveFolder(string id, [FromBody] string? newParentId)
        {
            try
            {
                await _folderService.MoveFolderAsync(GetUserId(), GetWorkshopId(), id, newParentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
