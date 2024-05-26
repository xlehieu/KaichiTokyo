using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Controllers
{
    public class FavouriteProductController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        private INotyfService _notyfService { get; set; }
        public FavouriteProductController(CuaHangNhatBanContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }
        public List<Product> FavouriteProduct
        {
            get
            {
                var favouriteProduct = HttpContext.Session.Get<List<Product>>("SanPhamYeuThich");
                if (favouriteProduct == default(List<Product>))
                {
                    return new List<Product>();
                }
                return favouriteProduct;
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("api/favourite/add")]
        public IActionResult AddToFavourite(int productId)
        {
            try
            {
                var favourite = FavouriteProduct;
                Product item = _context.Products.SingleOrDefault(x => x.ProductId == productId);
                if (item != null)
                {
                    if (favourite.Contains(item) == false)
                    {
                        favourite.Add(item);
                    }
                    else
                    {
                        _notyfService.Information("Sản phẩm đã có trong danh mục yêu thích");
                        return Json(new { success = false });
                    }
                HttpContext.Session.Set<List<Product>>("SanPhamYeuThich", favourite);
                _notyfService.Success("Thêm thành công");
                return Json(new { success = true });
                }
                else
                {
                    _notyfService.Error("Lỗi");
                    return Json(new { success = false });
                }
            }
            catch
            {
                _notyfService.Error("Lỗi");
                return Json(new { success = false });
            }
        }
        [Route("api/favourite/remove")]
        [AllowAnonymous]
        public IActionResult Remove(int productId)
        {
            try
            {
                var favourite = FavouriteProduct;
                Product item = _context.Products.SingleOrDefault(x => x.ProductId == productId);
                if (item != null)
                {
                    if (favourite.Contains(item))
                    {
                        favourite.Remove(item);
                    }
                    else
                    {
                        _notyfService.Information("Sản phẩm không có trong danh mục yêu thích");
                        return Json(new { success = false });
                    }
                    HttpContext.Session.Set<List<Product>>("SanPhamYeuThich", favourite);
                    _notyfService.Success("Loại bỏ thành công");
                    return Json(new { success = true });
                }
                else
                {
                    _notyfService.Error("Lỗi");
                    return Json(new { success = false });
                }
            }
            catch
            {
                _notyfService.Error("Lỗi");
                return Json(new { success = false });
            }
        }
        [Route("San-pham-yeu-thich")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(FavouriteProduct);
        }
    }
}
