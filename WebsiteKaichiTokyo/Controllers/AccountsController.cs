using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebsiteKaichiTokyo.EmailSender;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;
using WebsiteKaichiTokyo.ViewModels;

namespace WebsiteKaichiTokyo.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public INotyfService _notyfService { get; set; }
        private IEmailSendercs _emailSendercs { get; set; }
        public AccountsController(CuaHangNhatBanContext context, INotyfService notyfService, IEmailSendercs emailSendercs)
        {
            _context = context;
            _notyfService = notyfService;
            _emailSendercs = emailSendercs;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string phoneNumber)
        {
            try
            {
                var khachHang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == phoneNumber);
                if (khachHang != null)
                {
                    return Json(data: "Số điện thoại " + phoneNumber + " đã được sử dụng");
                }
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string email)
        {
            try
            {
                var khachHang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == email);
                if (khachHang != null)
                {
                    return Json(data: "Email " + email + " đã được sử dụng");
                }
                return Json(data: true);
            }
            catch
            {
                return Json(data: true);
            }
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("My-account", Name = "MyAccount")]
        public IActionResult Dashboard()
        {
            try
            {
                var taikhoanId = HttpContext.Session.GetString("CustomerId");
                if (taikhoanId != null)
                {
                    var khachHang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanId));
                    if (khachHang != null)
                    {
                        List<Order> orderList = _context.Orders.AsNoTracking().Where(x => x.CustomerId == khachHang.CustomerId).OrderByDescending(x => x.OrderId).Include(x => x.TransactStatus).Include(x => x.OrderDetails).Include(x => x.Payment).ToList();
                        if (orderList.Count > 0)
                        {
                            foreach (var order in orderList)
                            {
                                long? tongTien = 0;
                                List<OrderDetail> orderDetailList = _context.OrderDetails.AsNoTracking().Where(x => x.OrderId == order.OrderId).ToList();
                                foreach (OrderDetail orderDetail in orderDetailList)
                                {
                                    tongTien += orderDetail.Total;
                                }
                                order.Total = tongTien;
                            }
                            ViewBag.OrderListCustomer = orderList;
                            return View(khachHang);
                        }
                        ViewBag.OrderListCustomer = null;
                        return View(khachHang);
                    }
                }
                HttpContext.SignOutAsync();
                HttpContext.Session.Remove("CustomerId");
                HttpContext.Session.Remove("GioHang");
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                HttpContext.SignOutAsync();
                HttpContext.Session.Remove("CustomerId");
                HttpContext.Session.Remove("GioHang");
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("Dang-ky", Name = "DangKy")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("Dang-ky", Name = "DangKy")]
        public async Task<IActionResult> Register(RegisterModelView taikhoan)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string salt = Utilities.GetRandomKey();
                    Customer khachHang = new Customer
                    {
                        FullName = taikhoan.FullName,
                        Phone = taikhoan.PhoneNumber.Trim().ToLower(),
                        Email = taikhoan.EmailAddress.Trim(),
                        Password = (taikhoan.Password + salt.Trim()).ToMD5(),
                        Active = true,
                        Salt = salt,
                        Avatar = "userDefault.png",
                        CreateDate = DateTime.Now
                    };
                    try
                    {
                        _context.Add(khachHang);
                        await _context.SaveChangesAsync();
                        //Lưu Session Makh
                        HttpContext.Session.SetString("CustomerId", khachHang.CustomerId.ToString());
                        var customerId = HttpContext.Session.GetString("CustomerId");
                        //Identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, khachHang.FullName),
                            new Claim("CustomerId",khachHang.CustomerId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        _notyfService.Success("Đăng ký tài khoản thành công");
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                    catch (Exception ex)
                    {
                        _notyfService.Error("Đăng ký tài khoản không thành công");
                        return View(taikhoan);
                    }
                }
                else
                {
                    _notyfService.Error("Đăng ký tài khoản không thành công");
                    return View(taikhoan);
                }
            }
            catch
            {
                return View(taikhoan);
            }
        }
        [AllowAnonymous]
        [Route("Dang-nhap", Name = "DangNhap")]
        public IActionResult Login()
        {
            var taikhoanId = HttpContext.Session.GetString("CustomerId");
            if (taikhoanId != null)
            {
                return RedirectToAction("Dashboard", "Accounts");
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("Dang-nhap", Name = "DangNhap")]
        public async Task<IActionResult> Login(LoginViewModel customer, string? returnUrl = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail)
                    {
                        return View(customer);
                    }
                    var khachHang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);
                    if (khachHang == null)
                    {
                        return RedirectToAction("Register", "Acctounts");
                    }
                    string pass = (customer.Password + khachHang.Salt.Trim()).ToMD5();
                    if (khachHang.Password != pass)
                    {
                        _notyfService.Error("Thông tin đăng nhập chưa chính xác", 3);
                        return View(customer);
                    }
                    if (!khachHang.Active)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    //lưu session khach hang
                    HttpContext.Session.SetString("CustomerId", khachHang.CustomerId.ToString());
                    var taikhoanId = HttpContext.Session.GetString("CustomerId");
                    //Identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,khachHang.FullName),
                        new Claim("CustomerId", khachHang.CustomerId.ToString())
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    _notyfService.Success("Đăng nhập thành công", 3);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _notyfService.Error("Đăng nhập không thành công", 3);
                    return View(customer);
                }
            }
            catch
            {
                _notyfService.Error("Đăng nhập không thành công", 3);
                return RedirectToAction("Register", "Acccounts");
            }
        }
        public async Task LoginGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claims => new
            {
                claims.Issuer,
                claims.OriginalIssuer,
                claims.Type,
                claims.Value
            });
            //var test = claims.FirstOrDefault(claim => claim.Type.EndsWith("emailaddress")).Value;
            var name = claims.FirstOrDefault(claim => claim.Type.EndsWith("name")).Value;
            var email = claims.FirstOrDefault(claim => claim.Type.EndsWith("emailaddress")).Value;
            Customer customer = _context.Customers.SingleOrDefault(x => x.Email.Trim() == email.Trim());
            if (customer == null)
            {
                customer.FullName = name;
                customer.Email = email;
                customer.Active = true;
                customer.Avatar = "userDefault.png";
                _context.Add(customer);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("CustomerId", customer.CustomerId.ToString());
                var claimsCookie = new List<Claim>
                    {
                        new Claim("CustomerId", customer.CustomerId.ToString())
                    };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claimsCookie, "login");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                _notyfService.Success("Đăng nhập thành công");
            }
            else
            {
                HttpContext.Session.SetString("CustomerId", customer.CustomerId.ToString());
                var claimsCookie = new List<Claim>
                    {
                        new Claim("CustomerId", customer.CustomerId.ToString())
                    };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claimsCookie, "login");
                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                _notyfService.Success("Đăng nhập thành công");
            }
            return RedirectToAction("Index", "Home");
        }
        private bool ValidatePassword(string newpassword, string confirmpassword)
        {
            if (string.IsNullOrEmpty(newpassword.Trim()) || string.IsNullOrEmpty(confirmpassword.Trim()))
            {
                return false;
            }
            if (newpassword.Trim().Length < 6 || newpassword.Trim().Length < 6)
            {
                return false;
            }
            if (newpassword.Trim() != confirmpassword.Trim())
            {
                return false;
            }
            return true;
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldpassword, string newpassword, string confirmpassword)
        {
            if (ModelState.IsValid)
            {
                string id = HttpContext.Session.GetString("CustomerId");
                if (!string.IsNullOrEmpty(id))
                {
                    if (ValidatePassword(newpassword, confirmpassword))
                    {

                        Customer customer = _context.Customers.SingleOrDefault(x => x.CustomerId == Convert.ToInt32(id));
                        if (customer != null && customer.Password == (oldpassword + customer.Salt.Trim()).ToMD5())
                        {
                            customer.Password = (newpassword + customer.Salt.Trim()).ToMD5();
                            _context.Update(customer);
                            await _context.SaveChangesAsync();
                            _notyfService.Success("Đổi mật khẩu thành công");
                        }
                        else
                        {
                            _notyfService.Error("Nhập mật khẩu cũ sai");
                        }
                    }
                    else
                    {
                        _notyfService.Error("Mật khẩu mới và nhập lại mật khẩu phải giống nhau và ít nhất 6 ký tự");
                    }
                }
                else
                {
                    _notyfService.Warning("Có lỗi khi đổi mật khẩu");
                }
            }
            else
            {
                _notyfService.Error("Kiểm tra lại các trường thông tin");
            }
            return RedirectToAction("Dashboard", "Accounts");
        }
        [HttpGet]
        [Route("Dang-xuat", Name = "Logout")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        [Route("/Chi-tiet-don-hang/INV-{id}",Name ="ChiTietDonHang")]
        public IActionResult ViewDetailOrder(int id)
        {
            List<OrderDetail> orderdetail = _context.OrderDetails.AsNoTracking().Where(od=>od.OrderId == id).Include(od=>od.Product).ToList();
            return View(orderdetail);
        }
    }
}
