using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteKaichiTokyo.Areas.Admin.Models.Authentication;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SearchController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public SearchController(CuaHangNhatBanContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult FindProduct(string keyword)
        {
            List<Product> ls = new List<Product>();
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                ls = _context.Products.AsNoTracking().Include(x => x.Cat).OrderByDescending(x => x.ProductName).ToList();
                return PartialView("SearchProductListPartial", ls);
            }
            ls = _context.Products.AsNoTracking()
                .Include(x => x.Cat)
                .Where(x => x.ProductName.Contains(keyword) || x.Alias.Contains(keyword) || x.Alias.Replace("-", " ").Contains(keyword))
                .OrderByDescending(x => x.ProductName)
                .Take(10)
                .ToList();
            if (ls == null)
            {
                return PartialView("SearchProductListPartial", null);
            }
            return PartialView("SearchProductListPartial", ls);
        }
        [HttpPost]
        // POST: Admin/Search/Edit/5
        public IActionResult EditOrder(int? orderid)
        {
            if (orderid == null || _context.Orders == null)
            {
                return PartialView("EditOrderPartialView", null);
            }
            var order = _context.Orders.Find(orderid);
            if (order == null)
            {
                return PartialView("EditOrderPartialView", null);
            }
            ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "Status", order.TransactStatusId);
            return PartialView("EditOrderPartialView", order);
        }
        [HttpPost]
        public IActionResult FindMonthOrder(int? month)
        {

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> TotalThisMonthChart()
        {
            //Biểu đồ doanh thu theo tuần
            DateTime dt = DateTime.Now.AddDays(-7);
            string[] dtArr = new string[7];
            long[] figureSalesArr = new long[7];
            int[] figureTotalProductArr = new int[7];
            List<Order> orderList = await _context.Orders.Where(x => x.OrderDate.HasValue &&
                x.OrderDate.Value>dt &&x.Paid.HasValue && x.Paid.Value).Include(x => x.OrderDetails).ToListAsync();
            for (int i = 0; i < dtArr.Length; i++)
            {
                DateTime dtNext = dt.AddDays(i);
                List<Order> orderSmallList = orderList.Where(x => x.OrderDate.HasValue &&
                x.OrderDate.Value.Day == dtNext.Day
                && x.OrderDate.Value.Month == dtNext.Month
                && x.OrderDate.Value.Year == dtNext.Year).ToList();
                figureSalesArr[i] = orderSmallList.Sum(x => (long)x.Total);
                figureTotalProductArr[i] = orderSmallList.Sum(x => x.OrderDetails.Sum(x => (int)x.Quantity));
                dtArr[i] = (dtNext.Day).ToString() + "-" + dtNext.Month.ToString() + "-" + dtNext.Year.ToString();
            }
            //Array.Reverse(dtArr);
            //Array.Reverse(figureSalesArr);
            //Array.Reverse(figureTotalProductArr);
            return Json( new{ status = true, dayArr = dtArr, figureSalesArr = figureSalesArr,figureTotalProductArr = figureTotalProductArr });
        }
        [HttpGet] 
        public async Task<IActionResult> DoanhThuLoiNhuan()
        {
            DateTime dateTime = DateTime.Now;
            string[] dtArr = new string[dateTime.Month];
            long[] doanhthu = new long[dateTime.Month];
            var orderList = await _context.Orders
                .Where(x => x.OrderDate.HasValue && x.OrderDate.Value.Year == dateTime.Year)
                .Include(x=>x.OrderDetails)
                .ToListAsync();
            for (int i = 0;i<dateTime.Month;i++)
            {
                switch (i)
                {
                    case 0:
                        dtArr[i] = "Tháng 1";
                        break;
                    case 1:
                        dtArr[i] = "Tháng 2";
                        break;
                    case 2:
                        dtArr[i] = "Tháng 3";
                        break;
                    case 3:
                        dtArr[i] = "Tháng 4";
                        break;
                    case 4:
                        dtArr[i] = "Tháng 5";
                        break;
                    case 5:
                        dtArr[i] = "Tháng 6";
                        break;
                    case 6:
                        dtArr[i] = "Tháng 7";
                        break;
                    case 7:
                        dtArr[i] = "Tháng 8";
                        break;
                    case 8:
                        dtArr[i] = "Tháng 9";
                        break;
                    case 9:
                        dtArr[i] = "Tháng 10";
                        break;
                    case 10:
                        dtArr[i] = "Tháng 11";
                        break;
                    case 11:
                        dtArr[i] = "Tháng 12";
                        break;
                }
                doanhthu[i]=orderList.Where(x => x.OrderDate.HasValue && x.OrderDate.Value.Month == i+1 && x.Paid.HasValue && x.Paid.Value).Sum(x=>(long)x.Total);
            }
            return Json(new { status = true, dtArr = dtArr,doanhthuTheoThang = doanhthu});
        }
    }
}
