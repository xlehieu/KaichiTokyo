using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Controllers
{
    public class BlogController : Controller
    {
        private readonly CuaHangNhatBanContext _context;

        public BlogController(CuaHangNhatBanContext context) 
        {
        _context = context;
        }
        [Route("Blogs",Name ="Blog")]
        [AllowAnonymous]
        public IActionResult Index(int? page)
        {
            try
            {
                int pageNumber = page == null || page <= 0 ? 1 : page.Value;
                int pageSize = Utilities.NEWS_PAGE_SIZE;
                List<TinTuc> newsList = _context.TinTucs.AsNoTracking().Where(x=>x.Published).OrderByDescending(x => x.PostId).ToList();
                PagedList<TinTuc> pagedListTintuc = new PagedList<TinTuc>(newsList.AsQueryable(), pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;
                return View(pagedListTintuc);
            }
            catch
            {
                return RedirectToAction("Index","Home");
            }
        }
        [Route("News/{Alias}-{id}",Name ="TinDetails")]
        [AllowAnonymous]
        public async Task<IActionResult> DetailsAsync(int? id)
        {
            try
            {
                if (id == null || _context.TinTucs == null)
                {
                    return NotFound();
                }
                var ls = await _context.TinTucs.AsNoTracking().Where(x => x.Published && x.PostId != id).OrderByDescending(x => x.PostId).Take(3).ToListAsync();
                var tindang = _context.TinTucs.AsNoTracking().SingleOrDefault(x => x.PostId == id);
                if (tindang == null)
                {
                    return RedirectToAction("Index");
                }
                ViewData["PostMayYouLike"] = ls;
                return View(tindang);
            }
            catch
            {
                return RedirectToAction("Index","Home");
            }
        }
    }
}
