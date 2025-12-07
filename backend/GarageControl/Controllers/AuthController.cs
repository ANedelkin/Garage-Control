using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace GarageControl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] AuthVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.SignUp(model);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LogIn(model);
            
            // Parse the result to set cookies
            var jsonResult = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(result);
            if (jsonResult.TryGetProperty("Success", out var successProp) && successProp.GetBoolean())
            {
                if (jsonResult.TryGetProperty("AccessToken", out var accessToken) &&
                    jsonResult.TryGetProperty("RefreshToken", out var refreshToken))
                {
                    await _authService.SetAuthCookies(Response, accessToken.GetString()!, refreshToken.GetString()!);
                }
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogOut(Request, Response);
            return Ok(new { Success = true, Message = "Logged out successfully" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _authService.RefreshToken(Request, Response);
            return Ok(result);
        }
    }
}