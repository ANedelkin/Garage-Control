using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

    public class RequireAccessFilter : IAuthorizationFilter
    {
        private readonly string[] _accessNames;

        public RequireAccessFilter(string[] accessNames)
        {
            _accessNames = accessNames;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
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

            bool hasAccess = false;
            foreach (var access in _accessNames)
            {
                if (user.HasClaim("Access", access))
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
