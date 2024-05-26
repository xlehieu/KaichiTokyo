using Microsoft.AspNetCore.Mvc;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Controllers.Components
{
    public class NumberFavouriteViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var session = HttpContext.Session.Get<List<Product>>("SanPhamYeuThich");
            int quantity = 0;
            if (session != null) 
            {
                quantity = session.Count;
            }
            return View(quantity);
        }
    }
}
