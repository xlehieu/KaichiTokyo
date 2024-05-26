using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebsiteKaichiTokyo.Areas.Admin.Models.Authentication;
using WebsiteKaichiTokyo.EmailSender;
using WebsiteKaichiTokyo.Extension;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAccountsController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public INotyfService _notyfService { get; set; }
        public IEmailSendercs _emailSendercs { get; set; }
        public AdminAccountsController(CuaHangNhatBanContext context, INotyfService notyfService,IEmailSendercs emailSendercs)
        {
            _context = context;
            _notyfService = notyfService;
            _emailSendercs = emailSendercs;
        }
        [Authentication]
        [Authorization]
        // GET: Admin/AdminAccounts
        public IActionResult Index(int? page, int Status = 0, int RoleId = 0)
        {
            List<Account> accounts = new List<Account>();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.PAGE_SIZE;
            if (RoleId != 0)
            {
                accounts = _context.Accounts.AsNoTracking().Where(x => x.RoleId == RoleId).Include(x => x.Role).OrderByDescending(x => x.AccountId).ToList();
            }
            else
            {
                accounts = _context.Accounts.AsNoTracking().Include(x => x.Role).OrderByDescending(x => x.AccountId).ToList();
            }
            if (Status == 1)
            {
                accounts = accounts.Where(x => x.Active = true).ToList();
            }
            else if (Status == 2)
            {
                accounts = accounts.Where(x => x.Active = false).ToList();
            }
            PagedList<Account> models = new PagedList<Account>(accounts.AsQueryable(), pageNumber, pageSize);
            ViewData["QuyenTruyCap"] = new SelectList(_context.Roles, "RoleId", "Description");
            List<SelectListItem> statusList = new List<SelectListItem>();
            statusList.Add(new SelectListItem() { Text = "Hoạt động ", Value = "1" });
            statusList.Add(new SelectListItem() { Text = "Bị chặn", Value = "2" });
            ViewData["TrangThai"] = statusList;
            ViewBag.CurrentPage = pageNumber;
            return View(models);

        }

        public IActionResult FilterRole(int RoleId = 0)
        {
            var url = $"/Admin/AdminAccounts";
            if (RoleId != 0)
            {
                url = $"/Admin/AdminAccounts/Index?RoleId={RoleId}";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        public IActionResult FilterStatus(int Status = 0)
        {
            var url = $"/Admin/AdminAccounts";
            if (Status != 0)
            {
                url = $"/Admin/AdminAccounts/Index?Status={Status}";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        [Authorization]
        [Authentication]
        // GET: Admin/AdminAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        [Authentication]
        [Authorization]
        // GET: Admin/AdminAccounts/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Description");
            return View();
        }

        // POST: Admin/AdminAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorization]
        public async Task<IActionResult> Create([Bind("AccountId,UserName,Password,FullName,Phone,Email,Salt,Active,RoleId,LastLogin,CreateDate")] Account account)
        {
            if (ModelState.IsValid)
            {
                DateTime date = DateTime.Now;
                account.CreateDate = date;
                string salt = Utilities.GetRandomKey();
                account.Salt = salt;
                account.Password = (account.Password + salt.Trim()).ToMD5();
                _context.Add(account);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm tài khoản thành công", 3);
                return RedirectToAction(nameof(Index));
            }
            _notyfService.Error("Thêm tài khoản không thành công", 3);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Description", account.RoleId);
            return View(account);
        }

        [Authentication]
        [Authorization]
        // GET: Admin/AdminAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Description", account.RoleId);
            return View(account);
        }

        // POST: Admin/AdminAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        [Authorization]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,UserName,Password,FullName,Phone,Email,Salt,Active,RoleId,LastLogin,CreateDate")] Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    Account checkAcount = _context.Accounts.Where(x=>x.AccountId==id).Distinct().FirstOrDefault();
                    if (checkAcount != null)
                    {
                        if (checkAcount.Active==false && account.Active==true)
                        {
                            await _emailSendercs.SendEmailAsync(account.Email, "Thông báo cửa hàng Nhật Bản Kaichi Tokyo","<h3>Chúc mừng bạn đã trở thành thành viên của cửa hàng Nhật Bản Kaichi Tokyo</h3>");
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                _notyfService.Success("Sửa thông tin thành công", 3);
                return RedirectToAction(nameof(Index));
            }
            _notyfService.Error("Sửa thông tin không thành công", 3);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "Description", account.RoleId);
            return View(account);
        }
        [Authentication]
        [Authorization]
        // GET: Admin/AdminAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Accounts == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Admin/AdminAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authentication]
        [Authorization]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Accounts == null)
            {
                _notyfService.Information("Có lỗi");
                return Problem("Entity set 'CuaHangNhatBanContext.Accounts'  is null.");
            }
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }
            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa thông tin thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return (_context.Accounts?.Any(e => e.AccountId == id)).GetValueOrDefault();
        }
    }
}
