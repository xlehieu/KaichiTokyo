using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebsiteKaichiTokyo.Models;
using PagedList.Core;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Areas.Admin.Models.Authentication;

namespace WebsiteKaichiTokyo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        private INotyfService _notyfService { get; set; }
        public AdminOrdersController(CuaHangNhatBanContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }
        [Authentication]
        // GET: Admin/AdminOrders
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var orderlist = _context.Orders.AsNoTracking().OrderByDescending(x => x.OrderDate).Include(o => o.Customer).Include(o => o.TransactStatus).Include(x => x.Payment);
            PagedList<Order> model = new PagedList<Order>(orderlist, pageNumber, Utilities.PAGE_SIZE);
            ViewBag.CurrentPage = pageNumber;
            return View(model);
        }
        [Authentication]
        // GET: Admin/AdminOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.TransactStatus)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            var orderDetails = _context.OrderDetails.AsNoTracking().Where(x => x.OrderId == id).Include(x => x.Product).OrderBy(x => x.ProductId).ToList();
            ViewBag.OrderDetailList = orderDetails;
            return View(order);
        }

        //[Authentication]
        //// GET: Admin/AdminOrders/Create
        //public IActionResult Create()
        //{
        //    ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
        //    ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId");
        //    return View();
        //}

        //// POST: Admin/AdminOrders/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authentication]
        //public async Task<IActionResult> Create([Bind("OrderId,CustomerId,OrderDate,ShipDate,TransactStatusId,Deleted,Paid,PaymentDate,PaymentId,Note,CustomerName,PhoneNumber,Address,Email,Discount,Total")] Order order)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(order);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
        //    ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId", order.TransactStatusId);
        //    return View(order);
        //}

        [Authentication]
        [HttpGet]
        // GET: Admin/AdminOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "Status", order.TransactStatusId);
            return PartialView("EditOrderPartialView", order);
        }

        // POST: Admin/AdminOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,ShipDate,TransactStatusId,Deleted,Paid,PaymentDate,PaymentId,Note,CustomerName,PhoneNumber,Address,Email,Discount,Total")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (order.TransactStatusId == 1)
                    {
                        List<OrderDetail> lsorderDetail = await _context.OrderDetails.Where(x => x.OrderId == id).ToListAsync();
                        if (lsorderDetail != null)
                        {
                            foreach (OrderDetail detail in lsorderDetail)
                            {
                                Product product = _context.Products.SingleOrDefault(x => x.ProductId == detail.ProductId && x.UnitInstock > 0);
                                product.UnitInstock -= detail.Quantity;
                                _context.Update(product);
                            }
                        }
                    }
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            //Giai thích SelectList : _context.TransactStatuses là để xác định xem danh sách cho trường gì, TransactStatuses là giá trị của trường đó
            //Status là tên hiển thị trường đó, ví dụ id =1 thì hiển thị tên trạng thái có id là 1, order.TransactStatusId là trạng thái mặc định chọn, nếu không chọn thì mặc định là order.TransactStatusId như cũ
            ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatuses", "Status", order.TransactStatusId);
            return RedirectToAction("Index", "AdminOrders");
        }
        private bool OrderExists(int id)
        {
            return (_context.Orders?.Any(e => e.OrderId == id)).GetValueOrDefault();
        }
    }
}
