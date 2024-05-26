using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Controllers
{
    public class ProductController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public INotyfService _notyfService { get; set; }
        public ProductController(CuaHangNhatBanContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }
        [Route("/San-pham", Name = "Products")]
        [AllowAnonymous]
        public IActionResult Index(int? page)
        {
            try
            {
                int pageNumber = page == null || page <= 0 ? 1 : page.Value;
                int pageSize = Utilities.PAGE_SIZE;
                List<Product> productList = _context.Products.AsNoTracking().Where(x => x.Active).OrderByDescending(x => x.ProductId).Include(x => x.Cat).ToList();
                List<Category> categories = _context.Categories.AsNoTracking().Where(x => x.Published).Take(10).ToList();
                PagedList<Product> models = new PagedList<Product>(productList.AsQueryable(), pageNumber, Utilities.PAGE_SIZE);
                List<Product> saleProductList = productList.Where(x => x.Discount > 0).OrderByDescending(x => x.ProductId).ToList();
                List<Product> newestProductList = productList.OrderByDescending(x => x.CreateDate).Take(9).ToList();
                ViewBag.CurrentPage = pageNumber;
                ViewBag.SaleProductList = saleProductList;
                ViewBag.NewestProductList = newestProductList;
                ViewBag.CatList = categories;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [Route("/Danh-muc/{Alias}", Name = "CatProducts")]
        [AllowAnonymous]
        public IActionResult CatProductList(int? catId, int? page)
        {
            try
            {

                int pageNumber = page == null || page <= 0 ? 1 : page.Value;
                int pageSize = 9;
                List<Product> productList = _context.Products.AsNoTracking().Where(x => x.Active).Where(x => x.CatId == catId).OrderByDescending(x => x.ProductId).Include(x => x.Cat).ToList();
                List<Product> discountProductList = _context.Products.AsNoTracking().Where(x => x.Discount > 0 && x.CatId == catId && x.Active).OrderByDescending(x => x.ProductId).Include(x => x.Cat).ToList();
                List<Category> categories = _context.Categories.AsNoTracking().Take(10).ToList();
                PagedList<Product> models = new PagedList<Product>(productList.AsQueryable(), pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;
                ViewBag.DiscountProductList = discountProductList;
                ViewBag.Category = _context.Categories.Find(catId);
                ViewBag.CatList = categories;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [Route("/San-pham/{Alias}-{id}", Name = "ProductDetails")]
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            try
            {
                var product = _context.Products.Include(x => x.Cat).Include(x => x.Feedbacks).FirstOrDefault(x => x.ProductId == id);
                int amountFeedback = _context.Feedbacks.Count(x => x.ProductId == id);
                List<Product> relateProduct = _context.Products.Where(x => x.CatId == product.CatId && x.Active && x.ProductId != product.ProductId).AsNoTracking().Take(4).OrderByDescending(x => x.ProductId).ToList();
                if (product == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                ViewBag.AmountFeedbacks = amountFeedback;
                ViewBag.RelateProducts = relateProduct;
                return View(product);
            }
            catch { return RedirectToAction("Index", "Home"); }

        }
        [HttpGet]
        public IActionResult FeedbackPartial(int id)
        {
            List<Feedback> models = _context.Feedbacks.AsNoTracking().OrderByDescending(x => x.FeedbackId).Where(x => x.ProductId == id).Include(x => x.Customer).ToList();
            return PartialView("_FeedbackPartialView", models);
        }
        [HttpPost]
        public IActionResult Feedback(string messages, int id)
        {
            if (ModelState.IsValid)
            {
                Product product = _context.Products.SingleOrDefault(x => x.ProductId == id);
                if (product != null)
                {
                    string customerId = HttpContext.Session.GetString("CustomerId");
                    if (customerId == null)
                    {
                        return Json(new { success = false, messages = "Bạn phải đăng nhập trước" }) ;
                    }
                    var checkBought = _context.Orders.AsNoTracking().Where(x => x.CustomerId == Convert.ToInt32(customerId)).Include(x => x.OrderDetails);
                    if (!checkBought.Any(x => x.OrderDetails.Any(x => x.ProductId == id)))
                    {
                        return Json(new { success = false, messages = "Bạn phải mua hàng trước" });
                    }
                    var dabinhluan = _context.Feedbacks.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(customerId) && x.ProductId == id);
                    if (dabinhluan != null)
                    {
                        return Json(new { success = false, messages = "Bạn đã bình luận sản phẩm này rồi" });
                    }
                    Feedback newFeedback = new Feedback();
                    newFeedback.ProductId = id;
                    newFeedback.CustomerId = Convert.ToInt32(customerId);
                    newFeedback.Comment = messages;
                    _context.Add(newFeedback);
                    _context.SaveChanges();
                    List<Feedback> models = _context.Feedbacks.AsNoTracking().OrderByDescending(x => x.FeedbackId).Where(x => x.ProductId == id).Include(x => x.Customer).ToList();
                    return PartialView("_FeedbackPartialView",models);
                }
            }
            return Json(new { success = false, messages = "Bạn phải viết đánh giá" });
        }
    }
}
