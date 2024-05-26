using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebsiteKaichiTokyo.Models;
using WebsiteKaichiTokyo.ViewModels;

namespace WebsiteKaichiTokyo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CuaHangNhatBanContext _context;
        private INotyfService _notyfService;

        public HomeController(ILogger<HomeController> logger, CuaHangNhatBanContext context, INotyfService notyfService)
        {
            _logger = logger;
            _context = context;
            _notyfService = notyfService;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                HomeViewVM models = new HomeViewVM();
                var lscategory = _context.Categories.AsNoTracking().Where(x => x.Published).Take(8).ToList();
                var products = _context.Products.AsNoTracking().Where(x => x.Active && x.HomeFlag).OrderByDescending(x => x.ProductId).Take(30).Include(x => x.Cat).Include(x => x.Feedbacks).ToList();
                var lsProduct = products;
                List<HomeProductVM> homeProductVMList = new List<HomeProductVM>();
                foreach (var item in lscategory)
                {
                    HomeProductVM homeProductVM = new HomeProductVM();
                    homeProductVM.Category = item;
                    homeProductVM.Products = lsProduct.Where(x => x.CatId == item.CatId).ToList();
                    homeProductVMList.Add(homeProductVM);
                }
                models.Sliders = await _context.Sliders.AsNoTracking().Where(x => x.HomeFlag && x.Active).ToListAsync();
                models.LastestProduct = products.OrderByDescending(x => x.CreateDate).Take(9).ToList();
                models.TopRateProduct = products.OrderByDescending(x => x.Feedbacks.Count).Take(9).ToList();
                models.AllProduct = products;
                models.LsHomeProductVM = homeProductVMList;
                models.TinTucs = await _context.TinTucs.AsNoTracking().Where(x => x.Published && x.IsNewfeed).OrderByDescending(x => x.PostId).Take(3).ToListAsync();
                return View(models);
            }
            catch
            {
                HttpContext.SignOutAsync();
                HttpContext.Session.Remove("CustomerId");
                HttpContext.Session.Remove("GioHang");
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult FilterCategory(int catId)
        {
            List<Product> listProduct = new List<Product>();
            if (catId != 0)
            {
                listProduct = _context.Products.AsNoTracking().Where(x => x.CatId == catId && x.Active && x.HomeFlag).OrderByDescending(x => x.ProductId).Take(20).Include(x => x.Cat).ToList();
                return PartialView("_FilterCategoryPartial", listProduct);
            }
            if (catId == 0)
            {
                listProduct = _context.Products.AsNoTracking().Where(x => x.Active && x.HomeFlag).OrderByDescending(x => x.ProductId).Take(20).Include(x => x.Cat).ToList();
                return PartialView("_FilterCategoryPartial", listProduct);
            }
            if (listProduct == null || listProduct.Count == 0)
            {
                return PartialView("_FilterCategoryPartial", null);
            }
            return PartialView("_FilterCategoryPartial", null);
        }
        [AllowAnonymous]
        [Route("/Lien-he", Name = "Contact")]
        public IActionResult Contact()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateContact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                if (HttpContext.Session.GetString("CustomerId") != null)
                {
                    contact.CreateDate = DateTime.Now;
                    await _context.AddAsync(contact);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Gửi thành công", 3);
                    return RedirectToAction("Contact", "Home");
                }
                else
                {
                    _notyfService.Warning("Bạn phải đăng nhập trước", 3);
                    return RedirectToAction("Contact", "Home");
                }
            }
            _notyfService.Error("Gửi không thành công", 3);
            return View(contact);
        }
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}