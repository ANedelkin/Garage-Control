using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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

            if (result.Success)
            {
                await _authService.SetAuthCookies(Response, result.Token, result.RefreshToken);
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
        [HttpGet("google")]
        public async Task<IActionResult> Google()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback")
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
                return BadRequest(new { Success = false, Message = "Google authentication failed" });

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return BadRequest(new { Success = false, Message = "Email not provided by Google" });

            var userExists = await _authService.UserExists(email);

            LoginResponse response;
            if (!userExists)
            {
                response = await _authService.SignUp(new AuthVM
                {
                    Email = email,
                    Password = null
                });
            }
            else
            {
                response = await _authService.LogIn(new AuthVM
                {
                    Email = email,
                    Password = null
                });
            }

            if (response.Success)
            {
                // Set authentication cookies (optional)
                await _authService.SetAuthCookies(Response, response.Token, response.RefreshToken);

                // Serialize accesses to pass in query param
                var accessesJson = System.Text.Json.JsonSerializer.Serialize(response.Accesses);
                var encodedAccesses = Uri.EscapeDataString(accessesJson);

                // Redirect the user to the frontend with tokens and accesses
                var frontendRedirectUri = $"http://localhost:5174?access_token={response.Token}&refresh_token={response.RefreshToken}&accesses={encodedAccesses}";

                // Redirect to the frontend with the tokens as query parameters
                return Redirect(frontendRedirectUri);
            }

            return BadRequest(new { Success = false, Message = "Authentication failed" });
        }
    }
}