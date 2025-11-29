using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GarageControl.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly int _accessTokenExpiryMinutes;
        private readonly string _jwtSecret;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;

            _jwtSecret = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT Key not found");
        }

        public async Task<MethodResponse> SignUp(AuthVM model)
        {
            if (await UserExists(model.Email))
                return new MethodResponse()
                {
                    Success = false,
                    Message = "User already exists",
                    returnUrl = "api/signup"
                };

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new MethodResponse
                {
                    Success = false,
                    Message = errors,
                    returnUrl = "api/authentication/register"
                };
            }

            _logger.LogInformation("User {UserName} registered successfully", user.UserName);

            return new MethodResponse
            {
                Success = true,
                Message = "Successful registration",
                returnUrl = "api/login"
            };
        }
        public async Task<LoginResponse> LogIn(AuthVM model)
        {
            if (model.Email == null)
            {
                return new LoginResponse("Invalid password or email/username", false);
            }

            var user = await FindByEmail(model.Email);

            if (user == null)
            {
                return new LoginResponse("Invalid password or email/username", false);
            }

            bool passwordMatch = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordMatch)
            {
                return new LoginResponse("Invalid password or email/username", false);
            }

            string accessToken = GenerateAccessToken(user);
            string refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {UserName} logged in successfully", user.UserName);
            return new LoginResponse(user.Email, accessToken, refreshToken, "Successful login", true);
        }
        public async Task LogOut(HttpRequest request, HttpResponse response)
        {
            string refreshToken = request.Cookies["RefreshToken"] ?? string.Empty;

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var user = await FindByToken(refreshToken);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = new DateTime();
                    await _userManager.UpdateAsync(user);
                }
            }

            response.Cookies.Delete("AccessToken");
            response.Cookies.Delete("RefreshToken");
        }
        public async Task<LoginResponse> RefreshToken(HttpRequest request, HttpResponse response)
        {
            string refreshToken = request.Cookies["RefreshToken"] ?? string.Empty;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var user = await FindByToken(refreshToken);

                DateTime tokenExpiration = user.RefreshTokenExpiry;

                if (await IsRefreshTokenValid(tokenExpiration))
                {
                    string newToken = GenerateAccessToken(user);
                    return new LoginResponse("Token refreshed", newToken, refreshToken, "", true);
                }
            }
            return new LoginResponse();
        }

        public Task SetAuthCookies(HttpResponse response, string accessToken, string refreshToken)
        {
            response.Cookies.Append("AccessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes)
            });

            response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(12)
            });

            return Task.CompletedTask;
        }
        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(20),
                NotBefore = DateTime.UtcNow,
                Issuer = "https://localhost:5173",
                Audience = "https://localhost:5173",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<bool> UserExists(string email) => await _userManager.FindByEmailAsync(email) != null;
        private string GenerateRefreshToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        private async Task<User?> FindByEmail(string userName) => await _userManager.FindByEmailAsync(userName);
        private async Task<User?> FindByToken(string refreshToken) => await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        private async Task<bool> IsRefreshTokenValid(DateTime refreshTokenExpiry) => refreshTokenExpiry > DateTime.UtcNow;
    }
}