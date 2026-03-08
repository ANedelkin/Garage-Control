using GarageControl.Core.Contracts;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Auth;
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
using System.Security.Cryptography;
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

        public async Task<LoginResponseVM> SignUp(AuthVM model)
        {
            if (await UserExistsByUsername(model.Username))
                return new LoginResponseVM(false, "User already exists");

            var user = new User
            {
                UserName = model.Username,
                Email = null // Users can set their email later, but initially it's null
            };

            IdentityResult result;

            if (string.IsNullOrEmpty(model.Password))
                result = await _userManager.CreateAsync(user);
            else
                result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new LoginResponseVM(false, errors);
            }

            return await DoLogin(user);
        }

        public async Task<LoginResponseVM> LogIn(AuthVM model)
        {
            if (string.IsNullOrEmpty(model.Password))
                return new LoginResponseVM(false, "Password required");

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null)
                return new LoginResponseVM(false, "Invalid credentials");

            bool passwordMatch = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!passwordMatch)
                return new LoginResponseVM(false, "Invalid credentials");

            if (user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                string message = "Your account has been blocked. Please contact the administrator.";
                if (!string.IsNullOrEmpty(user.BlockReason))
                {
                    message += $" Justification: {user.BlockReason}";
                }
                return new LoginResponseVM(false, message);
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (!roles.Contains("Admin"))
            {
                var workshopId = await GetUserWorkshopId(user.Id);
                if (workshopId != null)
                {
                    var workshop = await _repo.GetByIdAsync<Workshop>(workshopId);
                    if (workshop != null && workshop.IsBlocked)
                    {
                        string message = "This workshop has been blocked by an administrator.";
                        if (!string.IsNullOrEmpty(workshop.BlockReason))
                        {
                            message += $" Justification: {workshop.BlockReason}";
                        }
                        return new LoginResponseVM(false, message);
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

        public async Task<LoginResponseVM> RefreshToken(HttpRequest request, HttpResponse response)
        {
            string refreshToken = request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return new LoginResponseVM(false, "No refresh token");

            var user = await FindByToken(refreshToken);

            if (user == null)
                return new LoginResponseVM(false, "Invalid refresh token");

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                return new LoginResponseVM(false, "Refresh token expired");

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(14);

            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var workshopId = await GetUserWorkshopId(user.Id);
            var accesses = await GetUserAccess(user.Id);

            string newAccess = GenerateAccessToken(user, roles, workshopId, accesses);

            bool hasWorkshop = await UserHasWorkshop(user.Id);
            var workerId = (await _repo.GetAllAsNoTracking<Worker>()
                .FirstOrDefaultAsync(w => w.UserId == user.Id))?.Id;

            return new LoginResponseVM(true, "Token refreshed", newAccess, user.RefreshToken, accesses, hasWorkshop, user.Id, workerId);
        }

        public Task SetAuthCookies(HttpResponse response, LoginResponseVM body)
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

        private async Task<LoginResponseVM> DoLogin(User user)
        {
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(14);

            await _userManager.UpdateAsync(user);

            var workshopId = await GetUserWorkshopId(user.Id);
            var roles = await _userManager.GetRolesAsync(user);
            var accesses = await GetUserAccess(user.Id);

            string token = GenerateAccessToken(user, roles, workshopId, accesses);

            bool hasWorkshop = await UserHasWorkshop(user.Id);

            var workerId = (await _repo.GetAllAsNoTracking<Worker>()
                .FirstOrDefaultAsync(w => w.UserId == user.Id))?.Id;

            return new LoginResponseVM(true, "Successful login", token, user.RefreshToken, accesses, hasWorkshop, user.Id, workerId);
        }

        private async Task<string> GenerateUsernameFromEmail(string email)
        {
            string baseName = email.Split('@')[0]
                .Replace(".", "")
                .Replace("-", "")
                .ToLower();

            string username = baseName;
            int counter = 1;

            while (await _userManager.FindByNameAsync(username) != null)
            {
                username = $"{baseName}{counter}";
                counter++;
            }

            return username;
        }

        private string GenerateAccessToken(User user, IList<string> roles, string? workshopId = null, IList<string>? accesses = null)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            if (accesses != null)
                foreach (var access in accesses)
                    claims.Add(new Claim("Access", access));

            if (!string.IsNullOrEmpty(workshopId))
                claims.Add(new Claim("WorkshopId", workshopId));

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
        public async Task<bool> UserExistsByUsername(string username) =>
            await _userManager.Users.AnyAsync(u => u.UserName == username);

        public async Task<bool> UserExists(string normalizedUsername) =>
            await _userManager.Users.AnyAsync(u => u.NormalizedUserName == normalizedUsername);

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        private async Task<User?> FindByToken(string token) =>
            await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);

        public async Task<LoginResponseVM> ExternalLogin(string provider, string providerKey, string email, string? name)
        {
            email = email.ToLower();
            var normalizedEmail = _userManager.NormalizeEmail(email);

            var user = await _userManager.FindByLoginAsync(provider, providerKey);

            if (user != null)
                return await DoLogin(user);

            user = await _userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user != null)
            {
                var info = new UserLoginInfo(provider, providerKey, provider);

                var result = await _userManager.AddLoginAsync(user, info);

                if (!result.Succeeded)
                {
                    string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new LoginResponseVM(false, errors);
                }

                return await DoLogin(user);
            }

            string username = await GenerateUsernameFromEmail(email);

            user = new User
            {
                UserName = username,
                Email = email
            };

            var createResult = await _userManager.CreateAsync(user);

            if (!createResult.Succeeded)
            {
                string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return new LoginResponseVM(false, errors);
            }

            var loginInfo = new UserLoginInfo(provider, providerKey, provider);
            await _userManager.AddLoginAsync(user, loginInfo);

            return await DoLogin(user);
        }
        private async Task<string?> GetUserWorkshopId(string userId)
        {
            var workshop = await _repo.GetAllAsNoTracking<Workshop>().FirstOrDefaultAsync(s => s.BossId == userId);
            if (workshop != null) return workshop.Id;

            var worker = await _repo.GetAllAsNoTracking<Worker>().FirstOrDefaultAsync(w => w.UserId == userId);
            return worker?.WorkshopId;
        }
        private async Task<bool> UserHasWorkshop(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return true;
            }

            var isOwner = await _repo.GetAllAsNoTracking<Workshop>().AnyAsync(s => s.BossId == userId);
            if (isOwner) return true;

            var isWorker = await _repo.GetAllAsNoTracking<Worker>().AnyAsync(w => w.UserId == userId);
            return isWorker;
        }
        public async Task<List<string>> GetUserAccess(string userId)
        {
            // Check if Admin
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return new List<string>
                {
                    "Admin"
                };
            }

            // Check if Owner
            var isOwner = await _repo.GetAllAsNoTracking<Workshop>().AnyAsync(s => s.BossId == userId);
            if (isOwner)
            {
                var ownerAccesses = await _repo.GetAllAsNoTracking<Access>().Select(a => a.Name).ToListAsync();
                return ownerAccesses;
            }

            // Check if Worker
            var worker = await _repo.GetAllAsNoTracking<Worker>()
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
        public async Task<LoginResponseVM> GenerateTokenForUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new LoginResponseVM(false, "User not found");

            var workshopId = await GetUserWorkshopId(userId);
            var roles = await _userManager.GetRolesAsync(user);
            var accesses = await GetUserAccess(userId);
            string token = GenerateAccessToken(user, roles, workshopId, accesses);
            bool hasWorkshop = await UserHasWorkshop(userId);
            var workerId = (await _repo.GetAllAsNoTracking<Worker>().FirstOrDefaultAsync(w => w.UserId == userId))?.Id;

            return new LoginResponseVM(true, "Token generated", token, user.RefreshToken, accesses, hasWorkshop, user.Id, workerId);
        }
    }
}