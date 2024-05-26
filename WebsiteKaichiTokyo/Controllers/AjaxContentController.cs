using Microsoft.AspNetCore.Mvc;

namespace WebsiteKaichiTokyo.Controllers
{
    public class AjaxContentController : Controller
    {
        public IActionResult HeaderCart()
        {
            return ViewComponent("HeaderCart");
        }
        public IActionResult NumberCart()
        {
            return ViewComponent("NumberCart");
        }
        public IActionResult NumberFavourite()
        {
            return ViewComponent("NumberFavourite");
        }
    }
}
