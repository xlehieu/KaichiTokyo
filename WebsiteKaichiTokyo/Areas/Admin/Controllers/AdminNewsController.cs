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
    public class AdminNewsController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        private INotyfService _notyfService;
        public AdminNewsController(CuaHangNhatBanContext context,INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }
        [Authentication]
        // GET: Admin/AdminNews
        public IActionResult Index(int? page)
        {
            int pageSize = Utilities.PAGE_SIZE;
            int pageNumber = page == null||page <= 0 ? 1 : page.Value;
            List<TinTuc> newsList = _context.TinTucs.AsNoTracking().OrderByDescending(x=>x.PostId).ToList();
            PagedList<TinTuc> pageListTinTuc = new PagedList<TinTuc>(newsList.AsQueryable(),pageNumber,pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(pageListTinTuc);
        }
        [Authentication]
        // GET: Admin/AdminNews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TinTucs == null)
            {
                return NotFound();
            }

            var tinTuc = await _context.TinTucs
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (tinTuc == null)
            {
                return NotFound();
            }

            return View(tinTuc);
        }
        [Authentication]
        // GET: Admin/AdminNews/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminNews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> Create([Bind("PostId,Title,Scontents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountId,Tags,CatId,IsHot,IsNewfeed,MetaDesc,MetaKey,Views")] TinTuc tinTuc,IFormFile? fThumb)
        {
            if (ModelState.IsValid)
            {
                if (fThumb != null)
                {
                    var extenstion = Path.GetExtension(fThumb.FileName);
                    var image = Utilities.SEOUrl(tinTuc.Title) + extenstion;
                    tinTuc.Thumb= await Utilities.UpLoadFile(fThumb, @"news", image.ToLower());
                }
                if (string.IsNullOrEmpty(tinTuc.Thumb))
                {
                    tinTuc.Thumb = "default.jpg";
                }
                tinTuc.Alias = Utilities.SEOUrl(tinTuc.Title);
                tinTuc.CreateDate = DateTime.Now;
                _context.Add(tinTuc);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm tin tức mới thành công");
                return RedirectToAction(nameof(Index));
            }
            return View(tinTuc);
        }
        [Authentication]
        // GET: Admin/AdminNews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TinTucs == null)
            {
                return NotFound();
            }

            var tinTuc = await _context.TinTucs.FindAsync(id);
            if (tinTuc == null)
            {
                return NotFound();
            }
            return View(tinTuc);
        }

        // POST: Admin/AdminNews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Scontents,Contents,Thumb,Published,Alias,CreateDate,Author,AccountId,Tags,CatId,IsHot,IsNewfeed,MetaDesc,MetaKey,Views")] TinTuc tinTuc,IFormFile? fThumb)
        {
            if (id != tinTuc.PostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (fThumb != null)
                    {
                        var extension = Path.GetExtension(fThumb.FileName);
                        var image = Utilities.SEOUrl(tinTuc.Title) + extension;
                        tinTuc.Thumb = await Utilities.UpLoadFile(fThumb, @"news", image.ToLower());
                    }
                    tinTuc.Alias = Utilities.SEOUrl(tinTuc.Title);
                    _context.Update(tinTuc);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Sửa thông tin thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TinTucExists(tinTuc.PostId))
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
            _notyfService.Error("Sửa thông tin không thành công");
            return View(tinTuc);
        }
        [Authentication]
        // GET: Admin/AdminNews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TinTucs == null)
            {
                return NotFound();
            }

            var tinTuc = await _context.TinTucs
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (tinTuc == null)
            {
                return NotFound();
            }

            return View(tinTuc);
        }

        // POST: Admin/AdminNews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TinTucs == null)
            {
                return Problem("Entity set 'CuaHangNhatBanContext.TinTucs'  is null.");
            }
            var tinTuc = await _context.TinTucs.FindAsync(id);
            if (tinTuc != null)
            {
                _context.TinTucs.Remove(tinTuc);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TinTucExists(int id)
        {
          return (_context.TinTucs?.Any(e => e.PostId == id)).GetValueOrDefault();
        }
    }
}
