using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebsiteKaichiTokyo.Areas.Admin.Models.Authentication
{
    public class Authentication : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Session.GetString("UserId") == null)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"Controller","AdminAccess" },
                        {"Action","LoginAdmin" }
                    }
                    );
            }
        }
    }
}
