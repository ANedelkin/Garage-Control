using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using GarageControl.Core.Contracts;
using System.Security.Claims;

namespace GarageControl.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireAccessAttribute : TypeFilterAttribute
    {
        public RequireAccessAttribute(params string[] accessNames)
            : base(typeof(RequireAccessFilter))
        {
            Arguments = new object[] { accessNames };
        }
    }

    public class RequireAccessFilter : IAsyncAuthorizationFilter
    {
        private readonly string[] _accessNames;
        private readonly IAuthService _authService;

        public RequireAccessFilter(string[] accessNames, IAuthService authService)
        {
            _accessNames = accessNames;
            _authService = authService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if user has ANY of the required accesses. If array is empty, we just pass.
            if (_accessNames == null || _accessNames.Length == 0)
                return;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var accesses = await _authService.GetUserAccess(userId);

            bool hasAccess = false;
            foreach (var access in _accessNames)
            {
                if (accesses.Contains(access))
                {
                    hasAccess = true;
                    break;
                }
            }

            if (!hasAccess)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
