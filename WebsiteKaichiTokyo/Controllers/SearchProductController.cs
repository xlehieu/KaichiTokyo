using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Controllers
{
    public class SearchProductController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public SearchProductController(CuaHangNhatBanContext context)
        {
            _context = context;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> FilterProduct(int[]? catId,int? pageNumber,int? minamount,int? maxamount)
        {
            var currentPage = pageNumber==null||pageNumber<=0? 1 : pageNumber.Value;
            List<Product> products = await _context.Products.AsNoTracking().Where(x=> catId.Length>0?catId.Contains((int)x.CatId):true
            &&x.Active 
            && x.Price>= (minamount.HasValue ? minamount.Value:0) 
            && x.Price <= (maxamount.HasValue ? maxamount.Value : x.Price))
                .Include(x=>x.Cat).ToListAsync();
            PagedList<Product> models = new PagedList<Product>(products.AsQueryable(),currentPage,18);
            ViewBag.CurrentPage = currentPage;
            return PartialView("FilterProductPartialView",models);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("Tim-kiem",Name ="SearchProduct")]
        public async Task<IActionResult> SearchProduct(string keyword,int? pageNumber)
        {
            var currentPage = pageNumber == null || pageNumber <= 0 ? 1 : pageNumber.Value;
            List<Product> products = await _context.Products.AsNoTracking()
                .Where(x => x.Active && x.ProductName.ToLower().Contains(keyword.ToLower())
                || x.Alias.ToLower().Contains(keyword.ToLower())
                || x.Title.ToLower().Contains(keyword.ToLower())
                || x.ProductName.ToLower().Replace(" ", "").Contains(keyword.ToLower().Replace(" ", ""))
                || x.Alias.ToLower().Replace("-", "").Contains(keyword.ToLower().Replace("-", "")))
                .Take(5)
                .Include(x=>x.Cat)
                .ToListAsync();
            PagedList<Product> models = new PagedList<Product>(products.AsQueryable(), currentPage, 18);
            ViewBag.CurrentPage = currentPage;
            ViewBag.KeyWord = keyword;
            return View(models);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SuggestProduct(string keyword)
        {
            List<Product> models = await _context.Products.AsNoTracking()
                .Where(x => x.Active && x.ProductName.ToLower().Contains(keyword.ToLower()) 
                || x.Alias.ToLower().Contains(keyword.ToLower()) 
                || x.Title.ToLower().Contains(keyword.ToLower())
                || x.ProductName.ToLower().Replace(" ","").Contains(keyword.ToLower().Replace(" ",""))
                || x.Alias.ToLower().Replace("-","").Contains(keyword.ToLower().Replace("-","")))
                .Include(x=>x.Cat)
                .ToListAsync(); 
            return PartialView("SuggestProductPartialView",models);
        }
    }
}
