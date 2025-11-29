using GarageControl.Core.Models;
using Microsoft.AspNetCore.Http;

namespace GarageControl.Core.Contracts
{
    public interface IAuthService
    {
        Task<MethodResponse> SignUp(AuthVM model);
        Task<LoginResponse> LogIn(AuthVM model);
        Task LogOut(HttpRequest request, HttpResponse response);
        Task<LoginResponse> RefreshToken(HttpRequest request, HttpResponse response);
        Task SetAuthCookies(HttpResponse response, string accessToken, string refreshToken);
    }
}