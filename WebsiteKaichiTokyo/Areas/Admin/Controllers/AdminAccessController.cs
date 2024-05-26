using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using WebsiteKaichiTokyo.Areas.Admin.Models.Authentication;
using WebsiteKaichiTokyo.EmailSender;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;
using WebsiteKaichiTokyo.ViewModels;

namespace WebsiteKaichiTokyo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccessController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        private IEmailSendercs _emailSendercs;
        private INotyfService _notyfService { get; set; }
        public AdminAccessController(CuaHangNhatBanContext context, INotyfService notyfService, IEmailSendercs emailSendercs)
        {
            _context = context;
            _notyfService = notyfService;
            _emailSendercs = emailSendercs;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetUserName(int? id)
        {
            var userName = HttpContext.Session.GetString("UserId");
            if (userName != null)
            {
                return Json(new { success = true, data = userName });
            }
            return Json(new { success = false });
        }
        [HttpGet]
        [AllowAnonymous]
        public bool CheckPhone(string phoneNumber)
        {
            try
            {
                var khachHang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == phoneNumber);
                if (khachHang != null)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public bool CheckEmail(string email)
        {
            try
            {
                var khachHang = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == email);
                if (khachHang != null)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        [AllowAnonymous]
        [Route("Dang-nhap-admin-Kaichi-Tokyo", Name = "DangNhapAdminKaichiTokyo")]
        public IActionResult LoginAdmin()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("Dang-nhap-admin-Kaichi-Tokyo", Name = "DangNhapAdminKaichiTokyo")]
        public async Task<IActionResult> LoginAdmin(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(model.UserName);
                    if (!isEmail)
                    {
                        return View(model);
                    }
                    var user = _context.Accounts.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == model.UserName);
                    if (user == null)
                    {
                        _notyfService.Error("Thông tin đăng nhập chưa chính xác", 3);
                        return RedirectToAction("Index", "Home");
                    }
                    // kiểm tra mật khẩu cộng với chuỗi bảo mật rồi mã hóa
                    string pass = (model.Password + user.Salt.Trim()).ToMD5();
                    if (user.Password != pass)
                    {
                        _notyfService.Error("Thông tin đăng nhập chưa chính xác", 3);
                        return View(model);
                    }
                    if (!user.Active)
                    {
                        _notyfService.Error("Tài khoản của bạn đã bị chặn");
                        return RedirectToAction("LoginAdmin", "AdminAccess");
                    }
                    //lưu session user
                    HttpContext.Session.SetString("UserId", user.AccountId.ToString());
                    //Identity
                    //Ở View chúng ta có thể sử dụng ClaimTypes bằng cách User.Identity.Name
                    // Sử dụng Claim("UserId", user.AccountId.ToString()) bằng cách User.Claims.FirstOrDefault(x=>x.Type =="UserId").Value
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,user.FullName.ToString()),
                        new Claim("UserId", user.AccountId.ToString()),
                        new Claim("RoleId",user.RoleId.ToString())
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "loginAdmin");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    _notyfService.Success("Đăng nhập thành công", 3);
                    HttpContext.Session.SetString("UserName", user.FullName);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _notyfService.Error("Đăng nhập không thành công", 3);
                    return View(model);
                }
            }
            catch
            {
                _notyfService.Error("Đăng nhập không thành công", 3);
                return RedirectToAction("Register", "Acccounts");
            }
        }

        [AllowAnonymous]
        [Route("Dang-ky-admin-Kaichi-Tokyo", Name = "DangKyAdmin")]
        public IActionResult RegisterAdmin()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [Route("Dang-ky-admin-Kaichi-Tokyo", Name = "DangKyAdmin")]
        public async Task<IActionResult> RegisterAdmin(RegisterModelView model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (CheckPhone(model.PhoneNumber) && CheckEmail(model.EmailAddress))
                    {
                        string salt = Utilities.GetRandomKey();
                        Account user = new Account
                        {
                            FullName = model.FullName,
                            Phone = model.PhoneNumber.Trim().ToLower(),
                            Email = model.EmailAddress.Trim(),
                            Password = (model.Password + salt.Trim()).ToMD5(),
                            RoleId=2,
                            Active = false,
                            Salt = salt,
                            CreateDate = DateTime.Now,
                        };
                        try
                        {
                            _context.Add(user);
                            await _context.SaveChangesAsync();
                            await _emailSendercs.SendEmailAsync(model.EmailAddress, "Thông báo từ Kaichi Tokyo Admin","<h3>Tài khoản của quý khách đang chờ quản trị viên duyệt</h3>");
                            _notyfService.Success("Đăng ký tài khoản thành công, hãy đợi phê duyệt");
                            return RedirectToAction("LoginAdmin", "AdminAccess");
                        }
                        catch
                        {
                            _notyfService.Error("Đăng ký tài khoản không thành công");
                            return View(model);
                        }
                    }
                    else
                    {
                        _notyfService.Error("Số điện thoại hoặc Email đã được đăng ký");
                        return View(model);
                    }
                }
                else
                {
                    _notyfService.Error("Đăng ký tài khoản không thành công");
                    return View(model);
                }
            }
            catch
            {
                return View(model);
            }
        }
        public IActionResult LogoutAdmin()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("LoginAdmin", "AdminAccess");
        }
    }
}
