using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebsiteKaichiTokyo.Areas.Admin.Models.Authentication;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminPagesController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public INotyfService _notyfService { get; set; }
        public AdminPagesController(CuaHangNhatBanContext context,INotyfService notyf)
        {
            _context = context;
            _notyfService = notyf;
        }
        [Authentication]
        // GET: Admin/AdminPages
        public IActionResult Index(int? page)
        {
            var pageNumber = page ==null|| page<=0 ? 1 : page.Value;
            var pageSize = Utilities.PAGE_SIZE;
            var pageList = _context.Pages.AsNoTracking().OrderByDescending(x => x.PageId);
            PagedList<Page> models = new PagedList<Page>(pageList, pageNumber,pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        [Authentication]
        // GET: Admin/AdminPages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pages == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .FirstOrDefaultAsync(m => m.PageId == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }
        [Authentication]
        // GET: Admin/AdminPages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminPages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> Create([Bind("PageId,PageName,Contents,Thumb,Published,Title,MetaDesc,MetaKey,Alias,CreateDate,Ordering")] Page page,IFormFile? fThumb)
        {
            if (ModelState.IsValid)
            {
                page.PageName = Utilities.ToTitleCase(page.PageName);
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(page.PageName)+extension;
                    page.Thumb = await Utilities.UpLoadFile(fThumb, @"pages", image);
                }
                if (string.IsNullOrEmpty(page.Thumb))
                {
                    page.Thumb = "default.jpg";
                }
                page.Alias = Utilities.SEOUrl(page.PageName);
                page.CreateDate = DateTime.Now;
                _context.Add(page);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm trang mới thành công",3);
                return RedirectToAction(nameof(Index));
            }
            _notyfService.Error("Thêm trang mới không thành công", 3);
            return View(page);
        }
        [Authentication]
        // GET: Admin/AdminPages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pages == null)
            {
                return NotFound();
            }

            var page = await _context.Pages.FindAsync(id);
            if (page == null)
            {
                return NotFound();
            }
            return View(page);
        }

        // POST: Admin/AdminPages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> Edit(int id, [Bind("PageId,PageName,Contents,Thumb,Published,Title,MetaDesc,MetaKey,Alias,CreateDate,Ordering")] Page page,IFormFile? fThumb)
        {
            if (id != page.PageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    page.PageName = Utilities.ToTitleCase(page.PageName);
                    if(fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string image = Utilities.SEOUrl(page.PageName) + extension;
                        page.Thumb = await Utilities.UpLoadFile(fThumb, @"pages", image);
                    }
                    if(string.IsNullOrEmpty(page.Thumb)) 
                    {
                        page.Thumb = "default.jpg";
                    }
                    page.Alias = Utilities.SEOUrl(page.PageName);
                    _context.Update(page);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PageExists(page.PageId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                _notyfService.Success("Lưu thông tin thành công", 3);
                return RedirectToAction(nameof(Index));
            }
            _notyfService.Error("Lưu thông tin không thành công", 3);
            return View(page);
        }
        [Authentication]
        // GET: Admin/AdminPages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pages == null)
            {
                return NotFound();
            }

            var page = await _context.Pages
                .FirstOrDefaultAsync(m => m.PageId == id);
            if (page == null)
            {
                return NotFound();
            }

            return View(page);
        }

        // POST: Admin/AdminPages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pages == null)
            {
                return Problem("Entity set 'CuaHangNhatBanContext.Pages'  is null.");
            }
            var page = await _context.Pages.FindAsync(id);
            if (page != null)
            {
               _context.Pages.Remove(page);
            }
            
            int a = await _context.SaveChangesAsync();
            Console.WriteLine(a);
            _notyfService.Success("Xóa trang thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool PageExists(int id)
        {
          return (_context.Pages?.Any(e => e.PageId == id)).GetValueOrDefault();
        }
    }
}
