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

            if (user.LockoutEnd > DateTimeOffset.UtcNow)
                return new LoginResponse(false, "Your account has been blocked. Please contact the administrator.");

            var roles = await _userManager.GetRolesAsync(user);

            if (!roles.Contains("Admin"))
            {
                var workshopId = await GetUserWorkshopId(user.Id);
                if (workshopId != null)
                {
                    var workshop = await _repo.GetByIdAsync<Workshop>(workshopId);
                    if (workshop != null && workshop.IsBlocked)
                    {
                        return new LoginResponse(false, "This workshop has been blocked by an administrator.");
                    }
                }
            }

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

            if (user.LockoutEnd > DateTimeOffset.UtcNow)
                return new LoginResponse(false, "Your account has been blocked.");

            var roles = await _userManager.GetRolesAsync(user);
            var workshopId = await GetUserWorkshopId(user.Id);

            if (workshopId != null)
            {
                var workshop = await _repo.GetByIdAsync<Workshop>(workshopId);
                if (workshop != null && workshop.IsBlocked)
                {
                    return new LoginResponse(false, "This workshop has been blocked by an administrator.");
                }
            }

            string newAccess = GenerateAccessToken(user, roles, workshopId);
            var accesses = await GetUserAccess(user.Id);
            bool hasWorkshop = await UserHasWorkshop(user.Id);

            return new LoginResponse(true, "Token refreshed", newAccess, refreshToken, accesses, hasWorkshop);
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

            var workshopId = await GetUserWorkshopId(user.Id);
            var roles = await _userManager.GetRolesAsync(user);
            string token = GenerateAccessToken(user, roles, workshopId);
            var accesses = await GetUserAccess(user.Id);
            bool hasWorkshop = await UserHasWorkshop(user.Id);

            return new LoginResponse(true, "Successful login", token, user.RefreshToken, accesses, hasWorkshop);
        }

        private async Task<bool> UserHasWorkshop(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return true;
            }

            var isOwner = await _repo.GetAllAsNoTrackingAsync<Workshop>().AnyAsync(s => s.BossId == userId);
            if (isOwner) return true;

            var isWorker = await _repo.GetAllAsNoTrackingAsync<Worker>().AnyAsync(w => w.UserId == userId);
            return isWorker;
        }
        private string GenerateAccessToken(User user, IList<string> roles, string? workshopId = null)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (!string.IsNullOrEmpty(workshopId))
            {
                claims.Add(new Claim("WorkshopId", workshopId));
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

        private async Task<string?> GetUserWorkshopId(string userId)
        {
            var workshop = await _repo.GetAllAsNoTrackingAsync<Workshop>().FirstOrDefaultAsync(s => s.BossId == userId);
            if (workshop != null) return workshop.Id;

            var worker = await _repo.GetAllAsNoTrackingAsync<Worker>().FirstOrDefaultAsync(w => w.UserId == userId);
            return worker?.WorkshopId;
        }

        private async Task<List<string>> GetUserAccess(string userId)
        {
            // Check if Admin
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return new List<string>
                {
                    "Admin Dashboard",
                    "Admin Makes and Models",
                    "Admin Users",
                    "Admin Workshops"
                };
            }

            // Check if Owner
            var isOwner = await _repo.GetAllAsNoTrackingAsync<Workshop>().AnyAsync(s => s.BossId == userId);
            if (isOwner)
            {
                var ownerAccesses = await _repo.GetAllAsNoTrackingAsync<Access>().Select(a => a.Name).ToListAsync();
                return ownerAccesses;
            }

            // Check if Worker
            var worker = await _repo.GetAllAsNoTrackingAsync<Worker>()
                .Include(w => w.Accesses)
                .Include(w => w.Activities)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (worker != null)
            {
                var workerAccesses = worker.Accesses.Select(a => a.Name).ToList();
                if (worker.Activities.Any())
                {
                    workerAccesses.Add("To Do");
                }
                return workerAccesses;
            }

            return new List<string>();
        }
    }
}
