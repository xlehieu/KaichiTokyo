using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System.Data;
using WebsiteKaichiTokyo.Areas.Admin.Models;
using WebsiteKaichiTokyo.Areas.Admin.Models.Authentication;
using WebsiteKaichiTokyo.Areas.Admin.Models.ViewModel;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public HomeController(CuaHangNhatBanContext context)
        {
            _context = context;
        }
        [Authentication]
        public async Task<IActionResult> Index()
        {
            AdminHomeView models = new AdminHomeView();
            //danh sách chọn theo tháng để hiển thị thống kê số liệu hóa đơn
            //List<SelectListItem> listMonthOfYear = new List<SelectListItem>();
            DateTime now = DateTime.Now;
            //for(int i = 1; i <= now.Month; i++)
            //{
            //    listMonthOfYear.Add(new SelectListItem 
            //    {
            //        Text = "Tháng "+ i.ToString(),
            //        Value = i.ToString(),
            //        Selected= now.Month==i?true:false
            //    });
            //}
            //ViewBag.MonthOfYear = listMonthOfYear;
            List<Order> orderList = await _context.Orders.AsNoTracking().Where(x => x.OrderDate.Value.Month == now.Month && x.OrderDate.Value.Year == now.Year).Include(x => x.OrderDetails).ToListAsync();
            List<OrderDetail> orderDetails = orderList.SelectMany(x => x.OrderDetails).OrderByDescending(x => x.ProductId).ToList();
            var top5Products = orderDetails
    .GroupBy(od => od.ProductId)
    .Select(g => new
    {
        MaSP = g.Key,
        SoLuong = g.Sum(od => od.Quantity),
        DoanhThu = g.Sum(od =>(long)od.Total),
    })
    .OrderByDescending(p => p.SoLuong)
    .Take(5)
    .ToList();
            List<TopOfProduct> listTopOfProduct = new List<TopOfProduct>();
            foreach (var topProduct in top5Products) 
            {
                TopOfProduct topOfProduct = new TopOfProduct();
                topOfProduct.Product = _context.Products.Find(topProduct.MaSP);
                topOfProduct.Amount = topProduct.SoLuong;
                topOfProduct.Total = topProduct.DoanhThu;
                listTopOfProduct.Add(topOfProduct);
            }
            var orrder = _context.Orders.AsNoTracking().OrderByDescending(x => x.OrderDate).Include(o => o.Customer).Include(o => o.TransactStatus).Include(x => x.Payment).Take(5).ToList();
            var contact = _context.Contacts.AsNoTracking().OrderByDescending(x => x.ContactId).Take(5).ToList();
            int totalOrdersOfMonth = (int)orderList.Sum(x => x.OrderDetails.Sum(x => x.Quantity));
            models.TotalOrder = totalOrdersOfMonth;
            models.ThisMonth = now.Month;
            models.SaleThisMonth = orderList.Sum(x => (long?)x.Total);
            models.TotalProduct = orderList.Sum(x => (int?)x.OrderDetails.Sum(x => x.Quantity));
            models.TopOfProducts = listTopOfProduct;
            models.OrderList = orrder;
            models.ContactList = contact;
            return View(models);
        }
    }
}
