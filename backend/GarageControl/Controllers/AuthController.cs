using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;

namespace GarageControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] AuthVM model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                _logger.LogWarning("Invalid signup model: {@Errors}", errors);
                return BadRequest(new { message = "Invalid model", errors });
            }

            var stat = await _authService.SignUp(model);

            if (!stat.Success)
            {
                return BadRequest(stat);
            }

            return Ok(stat);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LogIn([FromBody] AuthVM model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state: {ModelState}", ModelState);
                return BadRequest(GetModelErrors());
            }

            var response = await _authService.LogIn(model);

            if (!response.Success)
            {
                _logger.LogWarning("Login failed: {Message}", response.Message);
                return BadRequest(new { message = response.Message });
            }

            await _authService.SetAuthCookies(Response, response.Token, response.RefreshToken);

            return Ok(new { response.Username, response.Token, response.RefreshToken, response.Message });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            await _authService.LogOut(Request, Response);

            return Ok();
        }

        [Authorize]
        [HttpGet("checkAuth")]
        public IActionResult CheckAuth()
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (username == null)
            {
                return Unauthorized();
            }

            return Ok(new { username });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshRequest = await _authService.RefreshToken(Request, Response);

            if (!refreshRequest.Success)
            {
                return Unauthorized(refreshRequest.Message);
            }

            await _authService.SetAuthCookies(Response, refreshRequest.Token, refreshRequest.RefreshToken);

            return Ok(refreshRequest);
        }
        private List<string> GetModelErrors()
        {
            return ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        }
    }
}