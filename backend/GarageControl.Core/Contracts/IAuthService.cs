using GarageControl.Core.Models;
using Microsoft.AspNetCore.Http;

namespace GarageControl.Core.Contracts
{
    public interface IAuthService
    {
        Task<string> SignUp(AuthVM model);
        Task<string> LogIn(AuthVM model);
        Task LogOut(HttpRequest request, HttpResponse response);
        Task<string> RefreshToken(HttpRequest request, HttpResponse response);
        Task SetAuthCookies(HttpResponse response, string accessToken, string refreshToken);
    }
}