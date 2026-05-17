using System.Security.Claims;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetWorkshopId(this ClaimsPrincipal user)
        {
            return user.FindFirst("WorkshopId")?.Value!;
        }

        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        }
    }
}
