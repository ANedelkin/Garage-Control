using GarageControl.Core.Contracts;
using GarageControl.Core.Models;
using GarageControl.Infrastructure.Data.Models;
using GarageControl.Infrastructure.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GarageControl.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository _repo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        private readonly int _accessTokenExpiryMinutes;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthService(
            UserManager<User> userManager,
            IRepository repo,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _repo = repo;
            _configuration = configuration;
            _logger = logger;

            _jwtSecret = _configuration["Jwt:Key"];
            _jwtIssuer = _configuration["Jwt:Issuer"];
            _jwtAudience = _configuration["Jwt:Audience"];
            _accessTokenExpiryMinutes = 700;
        }

        public async Task<LoginResponse> SignUp(AuthVM model)
        {
            if (await UserExists(model.Email))
                return new LoginResponse("User already exists", false);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new LoginResponse(errors, false);
            }

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            await _userManager.UpdateAsync(user);

            string token = GenerateAccessToken(user);

            return new LoginResponse(user.Email, token, user.RefreshToken, "Successful registration", true);
        }

        public async Task<LoginResponse> LogIn(AuthVM model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new LoginResponse("Invalid credentials", false);

            bool passwordMatch = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordMatch)
                return new LoginResponse("Invalid credentials", false);

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            await _userManager.UpdateAsync(user);

            string token = GenerateAccessToken(user);
            var accesses = await GetUserAccess(user.Id);

            return new LoginResponse(user.Email, token, user.RefreshToken, "Successful login", true, accesses);
        }

        public async Task LogOut(HttpRequest request, HttpResponse response)
        {
            string refreshToken = request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var user = await FindByToken(refreshToken);
                if (user != null)
                {
                    user.RefreshToken = null;
                    user.RefreshTokenExpiry = DateTime.MinValue;
                    await _userManager.UpdateAsync(user);
                }
            }

            response.Cookies.Delete("AccessToken");
            response.Cookies.Delete("RefreshToken");
        }

        public async Task<LoginResponse> RefreshToken(HttpRequest request, HttpResponse response)
        {
            string refreshToken = request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return new LoginResponse("No refresh token", false);

            var user = await FindByToken(refreshToken);
            if (user == null)
                return new LoginResponse("Invalid refresh token", false);

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                return new LoginResponse("Refresh token expired", false);

            string newAccess = GenerateAccessToken(user);
            var accesses = await GetUserAccess(user.Id);

            return new LoginResponse("Token refreshed", newAccess, refreshToken, "", true, accesses);
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
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            });

            return handler.WriteToken(token);
        }

        private Task<bool> UserExists(string email) =>
            _userManager.FindByEmailAsync(email).ContinueWith(t => t.Result != null);

        private string GenerateRefreshToken() =>
            Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        private Task<User?> FindByToken(string token) =>
            _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);

        private async Task<List<string>> GetUserAccess(string userId)
        {
            // Check if Owner
            var isOwner = await _repo.GetAllAsNoTrackingAsync<CarService>().AnyAsync(s => s.BossId == userId);
            if (isOwner)
            {
                return await _repo.GetAllAsNoTrackingAsync<Access>().Select(a => a.Name).ToListAsync();
            }

            // Check if Worker
            var worker = await _repo.GetAllAsNoTrackingAsync<Worker>()
                .Include(w => w.Accesses)
                .FirstOrDefaultAsync(w => w.UserId == userId);
                
            if (worker != null)
            {
                return worker.Accesses.Select(a => a.Name).ToList();
            }
            
            return new List<string>();
        }
    }
}
