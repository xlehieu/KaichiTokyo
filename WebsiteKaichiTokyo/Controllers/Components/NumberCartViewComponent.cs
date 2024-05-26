using Microsoft.AspNetCore.Mvc;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.ViewModels;

namespace WebsiteKaichiTokyo.Controllers.Components
{
    public class NumberCartViewComponent: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            int quantity = 0;
            if(cart != null)
            {
                quantity= cart.Count;
            }
            return View(quantity);
        }
    }
}
