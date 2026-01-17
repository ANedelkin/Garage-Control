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
            _accessTokenExpiryMinutes = 30;
        }

        public async Task<LoginResponse> SignUp(AuthVM model)
        {
            if (await UserExists(model.Email))
                return new LoginResponse(false, "User already exists");

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email
            };

            IdentityResult result;
            if (model.Password == null)
                result = await _userManager.CreateAsync(user);
            else
                result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new LoginResponse(false, errors);
            }

            return await DoLogin(user);
        }

        public async Task<LoginResponse> LogIn(AuthVM model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new LoginResponse(false, "Invalid credentials");
            bool passwordMatch;
            if (model.Password == null)
                passwordMatch = true;
            else
                passwordMatch = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordMatch)
                return new LoginResponse(false, "Invalid credentials");

            return await DoLogin(user);
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
                return new LoginResponse(false, "No refresh token");

            var user = await FindByToken(refreshToken);
            if (user == null)
                return new LoginResponse(false, "Invalid refresh token");
            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                return new LoginResponse(false, "Refresh token expired");

            var garageId = await GetUserGarageId(user.Id);
            string newAccess = GenerateAccessToken(user, garageId);
            var accesses = await GetUserAccess(user.Id);
            bool hasService = await UserHasService(user.Id);

            return new LoginResponse(true, "Token refreshed", newAccess, refreshToken, accesses, hasService);
        }

        public Task SetAuthCookies(HttpResponse response, LoginResponse body)
        {
            response.Cookies.Append("AccessToken", body.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes)
            });

            response.Cookies.Append("RefreshToken", body.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(14)
            });

            return Task.CompletedTask;
        }

        private async Task<LoginResponse> DoLogin(User user)
        {
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(14);
            await _userManager.UpdateAsync(user);

            var garageId = await GetUserGarageId(user.Id);
            string token = GenerateAccessToken(user, garageId);
            var accesses = await GetUserAccess(user.Id);
            bool hasService = await UserHasService(user.Id);

            return new LoginResponse(true, "Successful login", token, user.RefreshToken, accesses, hasService);
        }

        private async Task<bool> UserHasService(string userId)
        {
            var isOwner = await _repo.GetAllAsNoTrackingAsync<CarService>().AnyAsync(s => s.BossId == userId);
            if (isOwner) return true;

            var isWorker = await _repo.GetAllAsNoTrackingAsync<Worker>().AnyAsync(w => w.UserId == userId);
            return isWorker;
        }
        private string GenerateAccessToken(User user, string? garageId = null)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            if (!string.IsNullOrEmpty(garageId))
            {
                claims.Add(new Claim("GarageId", garageId));
            }

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

        public async Task<bool> UserExists(string email) =>
            await _userManager.FindByEmailAsync(email) != null;


        private string GenerateRefreshToken() =>
            Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        private async Task<User?> FindByToken(string token) =>
            await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);

        private async Task<string?> GetUserGarageId(string userId)
        {
            var service = await _repo.GetAllAsNoTrackingAsync<CarService>().FirstOrDefaultAsync(s => s.BossId == userId);
            if (service != null) return service.Id;

            var worker = await _repo.GetAllAsNoTrackingAsync<Worker>().FirstOrDefaultAsync(w => w.UserId == userId);
            return worker?.CarServiceId;
        }

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
