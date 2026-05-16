using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace GarageControl.Extensions
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddIdentityAndAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<User, IdentityRole>(o => o.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<GarageControlDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddMicrosoftAuth(configuration)
            .AddGoogleAuth(configuration)
            .AddJwtAuth(configuration);

            return services;
        }

        public static AuthenticationBuilder AddMicrosoftAuth(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            var msClientId = configuration["Microsoft:ClientId"];
            var msClientSecret = configuration["Microsoft:ClientSecret"];
            var msCallbackPath = configuration["Microsoft:CallbackPath"];
            var msTenantId = configuration["Microsoft:TenantId"];

            if (!string.IsNullOrEmpty(msClientId) && !string.IsNullOrEmpty(msClientSecret)
                && !string.IsNullOrEmpty(msCallbackPath) && !string.IsNullOrEmpty(msTenantId))
            {
                builder.AddOpenIdConnect("Microsoft", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.ClientId = msClientId;
                    options.ClientSecret = msClientSecret;
                    options.CallbackPath = msCallbackPath;
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Authority = $"https://login.microsoftonline.com/{msTenantId}/v2.0";
                    options.ResponseType = "code";
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                });
            }

            return builder;
        }

        public static AuthenticationBuilder AddGoogleAuth(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            var googleClientId = configuration["Google:ClientId"];
            var googleClientSecret = configuration["Google:ClientSecret"];

            if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
            {
                builder.AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                });
            }

            return builder;
        }

        public static AuthenticationBuilder AddJwtAuth(this AuthenticationBuilder builder, IConfiguration configuration)
        {
            builder.AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured"))),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["AccessToken"];
                        return Task.CompletedTask;
                    }
                };
            });

            return builder;
        }
    }
}
