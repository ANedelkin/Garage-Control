using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Auth;
using Microsoft.AspNetCore.Http;

namespace GarageControl.Core.Contracts
{
    public interface IAuthService
    {
        Task<LoginResponseVM> SignUp(AuthVM model);
        Task<LoginResponseVM> LogIn(AuthVM model);
        Task LogOut(HttpRequest request, HttpResponse response);
        Task<LoginResponseVM> RefreshToken(HttpRequest request, HttpResponse response);
        Task SetAuthCookies(HttpResponse response, LoginResponseVM body);
        Task<bool> UserExists(string email);
        Task<List<string>> GetUserAccess(string userId);
        Task<LoginResponseVM> ExternalLogin(string provider, string providerKey, string email, string? name);
        Task<LoginResponseVM> GenerateTokenForUser(string userId);
    }
}