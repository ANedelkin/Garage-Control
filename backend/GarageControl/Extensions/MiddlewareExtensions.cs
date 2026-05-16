using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GarageControl.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseProjectMiddleware(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            app.UseForwardedHeaders();
            app.UseCookiePolicy();

            if (environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            return app;
        }
    }
}
