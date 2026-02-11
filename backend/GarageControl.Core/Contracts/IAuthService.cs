using GarageControl.Core.ViewModels;
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
    }
}