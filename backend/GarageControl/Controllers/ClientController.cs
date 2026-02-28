using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Clients;
using GarageControl.Core.Attributes;

namespace GarageControl.Controllers
{
    [Authorize]
    [RequireAccess("Clients")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var clients = await _clientService.All(userId);
            return Ok(clients);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ClientVM model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _clientService.Create(model, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var client = await _clientService.Details(id);
            if (client == null) return NotFound();
            return Ok(client);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(string id, [FromBody] ClientVM model)
        {
             if (!ModelState.IsValid) return BadRequest(ModelState);
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             await _clientService.Edit(id, model, userId!);
             return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _clientService.Delete(id, userId!);
            return Ok();
        }
    }
}
