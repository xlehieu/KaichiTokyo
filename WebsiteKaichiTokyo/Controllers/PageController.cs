using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Controllers
{
    public class PageController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public PageController(CuaHangNhatBanContext context)
        {
            _context = context;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        [Route("Pages/{Alias}", Name = "PageDetails")]
        public IActionResult Details(string alias)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return RedirectToAction("Index", "Home"); ;
            }
            var page = _context.Pages.AsNoTracking().SingleOrDefault(x => x.Alias == alias);
            if (page == null)
            {
                return RedirectToAction("Index","Home");
            }
            return View(page);
        }
    }
}
