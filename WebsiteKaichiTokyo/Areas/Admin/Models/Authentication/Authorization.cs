using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NuGet.Protocol;
using System.Security.Claims;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Areas.Admin.Models.Authentication
{
    public class Authorization:ActionFilterAttribute
    {
        //private static CuaHangNhatBanContext _context = new CuaHangNhatBanContext();
        //public override void OnActionExecuted(ActionExecutedContext context)
        //{
        //    var accountid = context.HttpContext.Session.GetString("UserId");
        //    if (accountid != null)
        //    {
        //        int idConvert = Convert.ToInt32(accountid);
        //        var account = _context.Accounts.SingleOrDefault(x=>x.AccountId==idConvert);
        //        if (account != null && account.RoleId!=1)
        //        {
        //            context.Result = new RedirectToRouteResult
        //            (
        //                new RouteValueDictionary{
        //                    {"Controller","Home" },
        //                    {"Action","Index" }
        //                }
        //            ) ;
        //        }
        //    }
        //}
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var roleId = context.HttpContext.User.Claims.SingleOrDefault(x=>x.Type=="RoleId");
            if(roleId != null && Convert.ToInt32(roleId.Value)==2)
            {
                context.Result = new RedirectToActionResult("Index","Home",null);
            }
        }
    }
}
