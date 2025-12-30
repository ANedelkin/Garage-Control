using GarageControl.Core.Models;
using Microsoft.AspNetCore.Http;

namespace GarageControl.Core.Contracts
{
    public interface IAuthService
    {
        Task<LoginResponse> SignUp(AuthVM model);
        Task<LoginResponse> LogIn(AuthVM model);
        Task LogOut(HttpRequest request, HttpResponse response);
        Task<LoginResponse> RefreshToken(HttpRequest request, HttpResponse response);
        Task SetAuthCookies(HttpResponse response, string accessToken, string refreshToken);
        Task<bool> UserExists(string email);
    }
}