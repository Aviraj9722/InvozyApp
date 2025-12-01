using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eOrderTouchApp.Models
{
    
    public class AuthorizeToRoles : Attribute, IAuthorizationFilter
    {
        string[] roles;

        public AuthorizeToRoles(params string[] _roles)
        {
            roles = _roles;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Identity.IsAuthenticated == false)
            {
                //  context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                context.Result = new RedirectResult("/Account/Login");
            }

            bool isAuthorized = false;
            foreach (var item in roles)
            {
                if (context.HttpContext.User.IsInRole(item))
                {
                    isAuthorized = true;
                    break;
                }
            }

            if (isAuthorized == false)
            {
                //  context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                context.Result = new RedirectResult("/Account/Login");
            }

        }
    }

}
