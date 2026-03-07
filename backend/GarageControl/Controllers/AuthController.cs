using Microsoft.AspNetCore.Mvc;
using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Auth;
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

            if (result.Success)
            {
                await _authService.SetAuthCookies(Response, result);
                return Ok(result);
            }
            else return BadRequest(result);
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
                await _authService.SetAuthCookies(Response, result);
                return Ok(result);
            }

            return Unauthorized(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogOut(Request, Response);
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _authService.RefreshToken(Request, Response);
            return result.Success ? Ok(result) : Unauthorized(result);
        }
        [HttpGet("microsoft")]
        public async Task<IActionResult> Microsoft()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("MicrosoftCallback")
            };
            return Challenge(props, "Microsoft");
        }
        [HttpGet("microsoft-callback")]
        public async Task<IActionResult> MicrosoftCallback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
                return BadRequest(new { Message = "Microsoft authentication failed" });

            var externalUserId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);

            // Use preferred_username or upn as fallback for email
            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value ??
                        result.Principal.FindFirst("preferred_username")?.Value ??
                        result.Principal.FindFirst("upn")?.Value;

            if (externalUserId == null || email == null)
                return BadRequest(new { Message = "Email or Microsoft ID not provided" });

            // Handle external login via AuthService
            var response = await _authService.ExternalLogin("Microsoft", externalUserId, email);

            if (response.Success)
            {
                await _authService.SetAuthCookies(Response, response);
                var frontendRedirectUri = $"https://localhost:5173";
                return Redirect(frontendRedirectUri);
            }

            return BadRequest(new { Message = response.Message });
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
            // Authenticate external login using Identity's external scheme
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
                return BadRequest(new { Message = "Google authentication failed" });

            // Get the permanent Google user ID (sub) - DO NOT rely only on email
            var externalUserId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);

            if (externalUserId == null || email == null)
                return BadRequest(new { Message = "Email or Google ID not provided" });

            // Use the new AuthService method to handle external login properly
            var response = await _authService.ExternalLogin("Google", externalUserId, email);

            if (response.Success)
            {
                await _authService.SetAuthCookies(Response, response);
                var frontendRedirectUri = $"https://localhost:5173";
                return Redirect(frontendRedirectUri);
            }

            return BadRequest(new { Message = response.Message });
        }
    }
}