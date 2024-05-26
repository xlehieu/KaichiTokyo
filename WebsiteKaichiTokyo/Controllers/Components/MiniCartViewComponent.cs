using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.ViewModels;

namespace WebsiteKaichiTokyo.Controllers.Components
{
    public class MiniCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            return  View(cart);
        }
    }
}
