using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace B_S_Skyline.Filters
{
    public class RoleFilter : IAuthorizationFilter
    {
        private readonly string _requiredRole;
        public RoleFilter(string requiredRole)
        {
            _requiredRole = requiredRole;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var role = context.HttpContext.Session.GetString("UserRole");
            if (role != _requiredRole)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
        }
    }
}
