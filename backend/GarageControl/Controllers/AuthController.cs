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
            return result.Success ? Ok(result) : BadRequest(result);
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

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value ??
                        result.Principal.FindFirst("preferred_username")?.Value ??
                        result.Principal.FindFirst("upn")?.Value;

            if (email == null)
                return BadRequest(new { Message = "Email not provided by Microsoft" });

            var userExists = await _authService.UserExists(email);

            LoginResponseVM response;
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
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
                return BadRequest(new { Message = "Google authentication failed" });

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return BadRequest(new { Message = "Email not provided by Google" });

            var userExists = await _authService.UserExists(email);

            LoginResponseVM response;
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
                await _authService.SetAuthCookies(Response, response);

                var frontendRedirectUri = $"https://localhost:5173";

                return Redirect(frontendRedirectUri);
            }

            return BadRequest(new { Message = response.Message });
        }
    }
}