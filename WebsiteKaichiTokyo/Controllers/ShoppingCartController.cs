using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.Models;
using WebsiteKaichiTokyo.Models.Payments;
using WebsiteKaichiTokyo.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WebsiteKaichiTokyo.EmailSender;
using Microsoft.CodeAnalysis;

namespace WebsiteKaichiTokyo.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IEmailSendercs _emailSendercs;
        public INotyfService _notyfService { get; set; }

        public ShoppingCartController(CuaHangNhatBanContext context, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IEmailSendercs emailSendercs)
        {
            _context = context;
            _notyfService = notyfService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _emailSendercs = emailSendercs;
        }
        public List<CartItem> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if (gh == null)
                {
                    gh = new List<CartItem>();
                }
                return gh;
            }
        }
        [HttpPost]
        [Route("api/cart/add")]
        [AllowAnonymous]
        public IActionResult AddToCart(int productId, int? amount)
        {
            try
            {
                List<CartItem> gioHang = GioHang;
                CartItem item = gioHang.SingleOrDefault(p => p.Product.ProductId == productId);
                if (item != null) //đã có trong giỏ hàng => cập nhật số lượng
                {
                    if (amount.HasValue)
                    {
                        item.Amount += amount.Value;
                    }
                    else
                    {
                        item.Amount++;
                    }
                }
                else
                {
                    Product hh = _context.Products.SingleOrDefault(p => p.ProductId == productId);
                    item = new CartItem
                    {
                        Amount = amount.HasValue ? amount.Value : 1,
                        Product = hh,
                    };
                    gioHang.Add(item);
                }
                _notyfService.Success("Thêm sản phẩm thành công", 3);
                //lưu lại session
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _notyfService.Error("Thêm sản phẩm không thành công", 3);
                return Json(new { success = false });
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("api/cart/update")]
        public IActionResult Update(int productId, int? amount)
        {
            var cart = GioHang;
            if (cart != null)
            {
                CartItem item = cart.SingleOrDefault(x => x.Product.ProductId == productId);
                if (item != null && amount.HasValue)
                {
                    if (amount == 0)
                    {
                        cart.Remove(item);
                    }
                    else
                    {
                        item.Amount = amount.Value;
                    }
                }
                //lưu lại session
                HttpContext.Session.Set<List<CartItem>>("GioHang", cart);
                _notyfService.Success("Chỉnh sửa sản phẩm trong giỏ hàng thành công");
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("api/cart/remove")]
        public IActionResult Remove(int productId)
        {
            try
            {
                List<CartItem> gioHang = GioHang;
                if (gioHang != null)
                {
                    CartItem item = gioHang.SingleOrDefault(x => x.Product.ProductId == productId);
                    if (item != null)
                    {
                        gioHang.Remove(item);
                    }
                }
                HttpContext.Session.Set<List<CartItem>>("GioHang", gioHang);
                _notyfService.Success("Xóa sản phẩm thành công");
                return Json(new { success = true });
            }
            catch
            {
                _notyfService.Error("Xóa sản phẩm không thành công");
                return Json(new { success = false });
            }
        }
        [Route("Thanh-toan-gio-hang", Name = "CheckOutCart")]
        [AllowAnonymous]
        public IActionResult ThanhToanGioHang(int? orderid)
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                OrderViewVM model = new OrderViewVM();
                if (customerId != null)
                {
                    var khachHang = _context.Customers.SingleOrDefault(x => x.CustomerId == Convert.ToInt32(customerId));
                    model.CustomerName = khachHang.FullName;
                    model.PhoneNumber = khachHang.Phone;
                    model.Address = khachHang.Address;
                    model.Email = khachHang.Email;
                }
                if (orderid == null)
                {
                    var cart = GioHang;
                    if (cart.Count > 0)
                    {
                        ViewBag.GioHang = cart;
                    }
                }
                else
                {
                    var orderlist = _context.OrderDetails.AsNoTracking().Where(x => x.OrderId == orderid).ToList();
                    if (orderlist != null && orderlist.Count > 0)
                    {
                        List<CartItem> gioHang = new List<CartItem>();
                        foreach (var item in orderlist)
                        {
                            gioHang.Add(new CartItem
                            {
                                Product = _context.Products.AsNoTracking().SingleOrDefault(x => x.ProductId == item.ProductId),
                                Amount = item.Quantity.Value,
                            });
                        }
                        ViewBag.GioHang = gioHang;
                        ViewBag.OrderId = orderid;
                        var order = _context.Orders.SingleOrDefault(x => x.OrderId == orderid);
                        if (order != null)
                        {
                            order.OrderDate = DateTime.Now;
                            _context.Update(order);
                            _context.SaveChanges();
                        }
                    }
                }
                return View(model);
            }
            catch
            {
                HttpContext.Session.Remove("GioHang");
                ViewBag.GioHang = new List<CartItem>();
                return View();
            }
        }
        public ActionResult GetUrlThanhToan(int? productid, int? soluong)
        {
            try
            {
                return Json(new { success = true, url = $"/ThanhToanSanPham/{productid}-{soluong}" });
            }
            catch
            {
                return Json(new { success = false, url = "" });
            }
        }
        [Route("/ThanhToanSanPham/{productid}-{soluong}")]
        [AllowAnonymous]
        public IActionResult ThanhToanSanPham(int? productid, int? soluong)
        {
            try
            {
                var customerId = HttpContext.Session.GetString("CustomerId");
                OrderViewVM model = new OrderViewVM();
                if (customerId != null)
                {
                    var khachHang = _context.Customers.SingleOrDefault(x => x.CustomerId == Convert.ToInt32(customerId));
                    model.CustomerName = khachHang.FullName;
                    model.PhoneNumber = khachHang.Phone;
                    model.Address = khachHang.Address;
                    model.Email = khachHang.Email;
                    model.ProductId = productid;
                    model.SoLuong = soluong;
                }
                if (productid != null)
                {
                    var product = _context.Products.SingleOrDefault(x => x.ProductId == productid);
                    if (product != null)
                    {
                        List<CartItem> gioHang = new List<CartItem>();
                        gioHang.Add(new CartItem
                        {
                            Product = product,
                            Amount = soluong.HasValue ? soluong.Value : 1,
                        });
                        ViewBag.GioHang = gioHang;
                    }
                }
                return View(model);
            }
            catch
            {
                HttpContext.Session.Remove("GioHang");
                ViewBag.GioHang = new List<CartItem>();
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult CheckOut(OrderViewVM req, int? orderid)
        {
            var code = new { Success = false, Code = -1, Url = "" };
            if (ModelState.IsValid)
            {
                if (orderid == null)
                {
                    List<CartItem> cartItems = GioHang;
                    if (cartItems != null)
                    {
                        var customerIdS = HttpContext.Session.GetString("CustomerId");
                        HttpContext.Session.SetString("OrderEmail", req.Email);
                        if (customerIdS != null)
                        {
                            Order order = new Order();
                            order.CustomerId = Convert.ToInt32(customerIdS);
                            order.Address = req.Address;
                            order.PhoneNumber = req.PhoneNumber;
                            order.CustomerName = req.CustomerName;
                            order.Email = req.Email;
                            order.OrderDate = DateTime.Now;
                            order.Paid = false;
                            _context.Add(order);
                            _context.SaveChanges();
                            foreach (var item in cartItems)
                            {
                                _context.Add(new OrderDetail
                                {
                                    OrderId = _context.Orders.OrderBy(x => x.OrderId).LastOrDefault(x => x.CustomerId == order.CustomerId).OrderId,
                                    ProductId = item.Product.ProductId,
                                    Quantity = item.Amount,
                                    Discount = (int)item.Discount,
                                    Total = (int)item.TotalMoney,
                                });
                            }
                            //khi người dùng chọn thanh toán cod
                            if (req.TypePayment == 1)
                            {
                                //TransactStatusId = 2 thì là chờ xác nhận từ admin
                                order.TransactStatusId = 2;
                                order.PaymentId = 1;
                            }
                            order.Total = cartItems.Sum(x => (int)x.TotalMoney);
                            _context.Update(order);
                            _context.SaveChanges();
                            // ngược lại khi người dùng chọn chuyển khoản
                            var url = "";
                            code = new { Success = true, Code = req.TypePayment, Url = "" };
                            if (req.TypePayment == 2)
                            {
                                url = UrlPayment(req.TypePaymentVN, order.OrderId);
                                code = new { Success = true, Code = req.TypePayment, Url = url };
                                return Redirect(url);
                            }
                        }
                        else
                        {
                            _notyfService.Information("Quý khách vui lòng đăng nhập trước khi đặt hàng");
                            return new RedirectToRouteResult(
                                new RouteValueDictionary
                                {
                                    {"Controller","Accounts" },
                                    {"Action","Login" }
                                }
                                );
                        }
                    }
                }
                else if (orderid.HasValue)
                {
                    var url = "";
                    code = new { Success = true, Code = req.TypePayment, Url = "" };
                    if (req.TypePayment == 2)
                    {
                        url = UrlPayment(req.TypePaymentVN, orderid.Value);
                        code = new { Success = true, Code = req.TypePayment, Url = url };
                        return Redirect(url);
                    }
                }
            }
            else
            {
                return RedirectToAction("ThanhToanGioHang", req);
            }
            _notyfService.Success("Đặt hàng thành công");
            return RedirectToAction("Dashboard", "Accounts");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult CheckOutSanPham(OrderViewVM req, int? productid, int? soluong)
        {
            var code = new { Success = false, Code = -1, Url = "" };
            if (ModelState.IsValid)
            {
                var customerIdS = HttpContext.Session.GetString("CustomerId");
                HttpContext.Session.SetString("OrderEmail", req.Email);
                if (customerIdS != null || productid != null)
                {
                    Product product = _context.Products.SingleOrDefault(p => p.ProductId == productid);
                    if (product != null)
                    {
                        Order order = new Order();
                        order.CustomerId = Convert.ToInt32(customerIdS);
                        order.Address = req.Address;
                        order.PhoneNumber = req.PhoneNumber;
                        order.CustomerName = req.CustomerName;
                        order.Email = req.Email;
                        order.OrderDate = DateTime.Now;
                        order.Paid = false;
                        _context.Add(order);
                        _context.SaveChanges();
                        OrderDetail orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = product.ProductId,
                            Quantity = soluong.HasValue ? soluong.Value : 1,
                            Discount = product.Discount,
                            Total = (product.Price * (100 - product.Discount) / 100) * (soluong.HasValue ? soluong.Value : 1),
                        };
                        _context.Add(orderDetail);
                        //khi người dùng chọn thanh toán cod
                        if (req.TypePayment == 1)
                        {
                            //TransactStatusId = 2 thì là chờ xác nhận từ admin
                            order.TransactStatusId = 2;
                            order.PaymentId = 1;
                        }
                        order.Total = orderDetail.Total;
                        _context.Update(order);
                        _context.SaveChanges();
                        // ngược lại khi người dùng chọn chuyển khoản
                        var url = "";
                        code = new { Success = true, Code = req.TypePayment, Url = "" };
                        if (req.TypePayment == 2)
                        {
                            url = UrlPayment(req.TypePaymentVN, order.OrderId);
                            code = new { Success = true, Code = req.TypePayment, Url = url };
                            return Redirect(url);
                        }
                        else
                        {
                            _notyfService.Information("Quý khách vui lòng đăng nhập trước khi đặt hàng");
                            return new RedirectToRouteResult(
                                new RouteValueDictionary
                                {
                                    {"Controller","Accounts" },
                                    {"Action","Login" }
                                }
                                );
                        }
                    }
                }
                else if (customerIdS == null)
                {
                    _notyfService.Information("Quý khách vui lòng đăng nhập trước khi đặt hàng");
                    return new RedirectToRouteResult(
                        new RouteValueDictionary
                        {
                                    {"Controller","Accounts" },
                                    {"Action","Login" }
                        }
                        );
                }
            }
            else
            {
                return RedirectToAction("ThanhToanGioHang", req);
            }
            _notyfService.Success("Đặt hàng thành công");
            return RedirectToAction("Dashboard", "Accounts");
        }
        public string UrlPayment(int typePaymentVNP, int orderId)
        {
            var urlPayment = "";
            //Get payment input
            Order order = _context.Orders.SingleOrDefault(x => x.OrderId == orderId);
            //Get Config Info
            string vnp_Returnurl = _configuration.GetSection("AppSettings:vnp_Returnurl").Value; //URL nhan ket qua tra ve 
            string vnp_Url = _configuration.GetSection("AppSettings:vnp_Url").Value; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = _configuration.GetSection("AppSettings:vnp_TmnCode").Value; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = _configuration.GetSection("AppSettings:vnp_HashSecret").Value; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (order.Total * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            if (typePaymentVNP == 1)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (typePaymentVNP == 2)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (typePaymentVNP == 3)
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            }
            vnpay.AddRequestData("vnp_CreateDate", order.OrderDate.Value.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(_httpContextAccessor));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng:" + order.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);

            return urlPayment;
        }
        public async Task<ActionResult> VNPayReturn()
        {
            if (Request.QueryString.HasValue && Request.QueryString.Value.Length > 0)
            {
                string vnp_HashSecret = _configuration.GetSection("AppSettings:vnp_HashSecret").Value; //Chuoi bi mat
                var vnpayData = Request.QueryString.Value;
                var queryDictionary = QueryHelpers.ParseQuery(vnpayData);
                VnPayLibrary vnpay = new VnPayLibrary();
                foreach (var queryParam in queryDictionary)
                {
                    string key = queryParam.Key;
                    string value = queryParam.Value;

                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(key, value);
                    }
                }
                int orderCode = Convert.ToInt32(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.Query["vnp_SecureHash"];
                String TerminalID = Request.Query["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.Query["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var itemOrder = _context.Orders.FirstOrDefault(x => x.OrderId == orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.Paid = true;
                            itemOrder.TransactStatusId = 1;
                            itemOrder.PaymentDate = DateTime.Now;
                            itemOrder.PaymentId = 2;
                            _context.Orders.Update(itemOrder);
                            List<OrderDetail> lsorderDetail = await _context.OrderDetails.AsNoTracking().Where(x => x.OrderId == orderCode).Include(x => x.Product).ToListAsync();
                            if (lsorderDetail != null)
                            {
                                foreach (OrderDetail detail in lsorderDetail)
                                {
                                    Product product = _context.Products.SingleOrDefault(x => x.ProductId == detail.ProductId && x.UnitInstock > 0);
                                    product.UnitInstock -= detail.Quantity;
                                    _context.Update(product);
                                }
                            }
                            await _context.SaveChangesAsync();
                            Task.WaitAll();
                            string infoOrder = "";
                            string nguyengia = "";
                            try
                            {
                                nguyengia = itemOrder.Discount.HasValue ? (itemOrder.Total + (itemOrder.Total * itemOrder.Discount) / 100).Value.ToString("N0") : itemOrder.Total.Value.ToString("N0");
                            }
                            catch
                            {
                                nguyengia = "";
                            }
                            try
                            {

                                foreach (var item in lsorderDetail)
                                {
                                    infoOrder += string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>"
                                        , item.Product.ProductName
                                        , item.Quantity
                                        , item.Total != null ? item.Total.Value.ToString("#,#0") : "");
                                }
                            }
                            catch
                            {
                                infoOrder = "<tr></tr>";
                            }
                            string messageBody = string.Format("<div class=\"section-header\">\r\n<h1>Thông tin hóa đơn cửa hàng Nhật Bản Kaichi Tokyo</h1>\r\n</div>\r\n<div class=\"section-body\">\r\n<div class=\"card\">\r\n<div class=\"card-body\">\r\n<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" height=\"100%\" width=\"100%\">\r\n<tbody>\r\n<tr>\r\n<td align=\"center\" valign=\"top\">\r\n <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\" style=\"background-color:#ffffff;border:1px solid #dedede;border-radius:3px\">\r\n<tbody>\r\n<tr>\r\n<td align=\"center\" valign=\"top\">\r\n<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"background-color:#338218;color:#ffffff;border-bottom:0;font-weight:bold;line-height:100%;vertical-align:middle;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;border-radius:3px 3px 0 0\">\r\n<tbody>\r\n<tr>\r\n<td style=\"padding:36px 48px;display:block\">\r\n<h1 style=\"font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;font-size:30px;font-weight:300;line-height:150%;margin:0;text-align:left;color:#ffffff;background-color:inherit\">Đơn hàng:{0}</h1>\r\n</td>\r\n</tr>\r\n</tbody>\r\n</table>\r\n</td>\r\n</tr>\r\n<tr>\r\n<td align=\"center\" valign=\"top\">\r\n<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n<tbody>\r\n<tr>\r\n<td valign=\"top\" style=\"background-color:#ffffff\">\r\n<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\">\r\n<tbody>\r\n<tr>\r\n<td valign=\"top\" style=\"padding:48px 48px 32px\">\r\n<div style=\"color:#636363;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;font-size:14px;line-height:150%;text-align:left\">\r\n<p style=\"margin:0 0 16px;font-size:18px\">Chi tiết thông tin đơn hàng:</p>\r\n<h2 style=\"color:#338218;display:block;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif;font-size:18px;font-weight:bold;line-height:130%;margin:0 0 18px;text-align:left\">\r\nĐơn hàng: {1} - {2}\r\n</h2>\r\n</div>\r\n<div style=\"margin-bottom:40px\">\r\n<table cellspacing=\"0\" cellpadding=\"6\" border=\"1\" style=\"color:#636363;border:1px solid #e5e5e5;vertical-align:middle;width:100%;font-family:'Helvetica Neue',Helvetica,Roboto,Arial,sans-serif\">\r\n<thead>\r\n<tr>\r\n<th scope=\"col\" style=\"color:#636363;border:1px solid #e5e5e5;vertical-align:middle;padding:12px;text-align:left\">Sản phẩm</th>\r\n<th scope=\"col\" style=\"color:#636363;border:1px solid #e5e5e5;vertical-align:middle;padding:12px;text-align:left\">Số lượng</th>\r\n<th scope=\"col\" style=\"color:#636363;border:1px solid #e5e5e5;vertical-align:middle;padding:12px;text-align:left\">Giá</th>\r\n</tr>\r\n</thead>\r\n<tbody>\r\n{3}</tbody>\r\n<tfoot>\r\n<tr>\r\n<th scope=\"row\" colspan=\"2\"\r\nstyle=\"color:#636363;border:1px solid #e5e5e5;vertical-align:middle;padding:12px;text-align:left;border-top-width:4px\">\r\nNguyên giá:\r\n</th>\r\n<td style=\"color:#636363;border:1px solid #e5e5e5;vertical-align:middle;padding:12px;text-align:left;border-top-width:4px\">\r\n<span>\r\n{4}<span>₫</span>\r\n</span>\r\n</td>\r\n</tr>\r\n<!--<tr>\r\n<th scope=\"row\" colspan=\"2\"\r\nstyle=\"color:#636363;border:1px solid #e5e5e5;vertical-align:middle;padding:12px;text-align:left\">\r\nPhương thức thanh toán:</th>\r\n<td\r\nstyle=\"color:#636363;border:1px solid #e5e5e5;vertical-align:middle;padding:12px;text-align:left\">\r\nChuyển khoản ngân hàng</td>\r\n</tr>-->\r\n<tr>\r\n<span>{5}<span>₫</span></span>\r\n</td>\r\n</tr>\r\n</tfoot>\r\n</table>\r\n</div>", "INV-" + orderCode, "INV-" + orderCode, itemOrder.OrderDate.Value.ToLongDateString(), infoOrder, nguyengia, itemOrder.Total != null ? itemOrder.Total.Value.ToString("N0") : "");

                            string email = HttpContext.Session.GetString("OrderEmail");
                            await _emailSendercs.SendEmailAsync(email, "Thanh toán thành công", messageBody);
                            //Thanh toan thanh cong
                            ViewBag.Icon = "<i style=\"font-size:150px;color:#00b509\" class=\"fa fa-check-circle text-center\"></i>";
                            ViewBag.InnerText = "<h2 class=\"text-center\" style=\"color:#00b509\">Giao dịch được thực hiện thành công</h2>";
                            ViewBag.OrderId = "INV-" + itemOrder.OrderId;
                            //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                        }
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.Icon = "<i style=\"font-size:150px;color:#db0001\" class=\"fa fa-close\"></i>";
                        ViewBag.InnerText = "<h2 class=\"text-center\" style=\"color:#db0001\">Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode + "</h2>";
                        //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}", orderId, vnpayTranId, vnp_ResponseCode);
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();
                    ViewBag.InvoiceMonney = "Số tiền thanh toán (VND): " + vnp_Amount.ToString("N0") + "₫";
                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
            }
            return View();
        }
        [Route("Gio-hang", Name = "Cart")]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(GioHang);
        }
    }
}
