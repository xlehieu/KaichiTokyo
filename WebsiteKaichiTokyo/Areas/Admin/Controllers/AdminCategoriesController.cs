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
    public class AdminCategoriesController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public INotyfService _notyfService { get; set; }
        public AdminCategoriesController(CuaHangNhatBanContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/AdminCategories
        [Authentication]
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.PAGE_SIZE;
            var listCategory = _context.Categories.AsNoTracking().OrderByDescending(x => x.CatId).ToList();
            foreach (var cat in listCategory)
            {
                if (cat.ParentId != null && cat.ParentId != 0)
                {
                    cat.CatParentName = _context.Categories.Find(cat.ParentId).CatName;
                }
            }
            PagedList<Category> model = new PagedList<Category>(listCategory.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(model);
        }
        [Authentication]
        // GET: Admin/AdminCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CatId == id);
            if (category == null)
            {
                return NotFound();
            }
            var listCategory = _context.Categories.ToList();
            var catParentName = _context.Categories.Find(category.ParentId);
            category.CatParentName = catParentName!=null ? catParentName.CatName : null;
            return View(category);
        }
        [Authentication]
        // GET: Admin/AdminCategories/Create
        public IActionResult Create()
        {
            ViewData["CatParent"] = CatList();
            return View();
        }
        public SelectList CatList()
        {
            SelectList list = new SelectList(_context.Categories, "CatId", "CatName");
            return list;
        }
        // POST: Admin/AdminCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> Create([Bind("CatId,CatName,Description,ParentId,Levels,Ordering,Published,Thumb,Title,Alias,MetaDesc,MetaKey,Cover,SchemaMarkup")] Category category, IFormFile? fThumb)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    // Lấy thông báo lỗi
                    var errorMessage = error.ErrorMessage;
                    // In thông báo lỗi ra màn hình console
                    Console.WriteLine(errorMessage);
                }
            }
            if (ModelState.IsValid)
            {
                category.CatName = Utilities.ToTitleCase(category.CatName);
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(fThumb.FileName) + extension;
                    category.Thumb = await Utilities.UpLoadFile(fThumb, @"categories", image.ToLower());
                }
                if (string.IsNullOrEmpty(category.Thumb))
                {
                    category.Thumb = "default.jpg";
                }
                category.Alias = Utilities.SEOUrl(category.CatName);
                _context.Add(category);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm danh mục mới thành công", 3);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CatParent"] = CatList();
            _notyfService.Error("Thêm danh mục mới không thành công", 3);
            return View(category);
        }
        [Authentication]
        // GET: Admin/AdminCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["CatParent"] = CatList();
            return View(category);
        }

        // POST: Admin/AdminCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authentication]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CatId,CatName,Description,ParentId,Levels,Ordering,Published,Thumb,Title,Alias,MetaDesc,MetaKey,Cover,SchemaMarkup")] Category category, IFormFile? fThumb)
        {
            if (id != category.CatId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.CatName = Utilities.ToTitleCase(category.CatName);
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(category.Thumb);
                        string image = Utilities.SEOUrl(fThumb.FileName) + extension;
                        category.Thumb = await Utilities.UpLoadFile(fThumb, @"categories", image.ToLower());
                    }
                    if (string.IsNullOrEmpty(category.Thumb))
                    {
                        category.Thumb = "default.jpg";
                    }
                    category.Alias = Utilities.SEOUrl(category.CatName);
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CatId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                _notyfService.Success("Sửa thông tin danh mục thành công", 3);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CatParent"] = CatList();
            _notyfService.Error("Sửa thông tin danh mục không thành công", 3);
            return View(category);
        }
        [Authentication]
        // GET: Admin/AdminCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CatId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/AdminCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authentication]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'CuaHangNhatBanContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }
            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa danh mục" + category.CatName + " thành công", 3);
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.CatId == id)).GetValueOrDefault();
        }
    }
}
