using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;
using Microsoft.AspNetCore.Http;
using AspNetCoreHero.ToastNotification.Abstractions;
using WebsiteKaichiTokyo.Areas.Admin.Models.Authentication;

namespace WebsiteKaichiTokyo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminProductsController : Controller
    {
        private readonly CuaHangNhatBanContext _context;
        public INotyfService _notyfService { get; }
        public AdminProductsController(CuaHangNhatBanContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/AdminProducts
        [Authentication]
        public IActionResult Index(int? page, int CatId = 0)
        {
            List<Product> ls = new List<Product>();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = Utilities.PAGE_SIZE;
            if (CatId != 0)
            {
                ls = _context.Products.Where(x => x.CatId == CatId).AsNoTracking().Include(x => x.Cat).OrderByDescending(x => x.ProductId).ToList();
            }
            else
            {
                ls = _context.Products.AsNoTracking().Include(x => x.Cat).OrderByDescending(x => x.ProductId).ToList();
            }
            PagedList<Product> model = new PagedList<Product>(ls.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View(model);
        }
        [Authentication]
        // GET: Admin/AdminProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [Authentication]
        // GET: Admin/AdminProducts/Create
        public IActionResult Create()
        {
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName");
            return View();
        }

        // POST: Admin/AdminProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> Create(Product product, IFormFile? fThumb)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    product.ProductName = Utilities.ToTitleCase(product.ProductName);
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string image = Utilities.SEOUrl(product.ProductName) + extension;
                        product.Thumb = await Utilities.UpLoadFile(fThumb, @"products", image.ToLower());
                    }
                    if (string.IsNullOrEmpty(product.Thumb))
                    {
                        product.Thumb = "default.jpg";
                    }
                    product.Alias = Utilities.SEOUrl(product.ProductName);
                    product.ModifiedDate = DateTime.Now;
                    product.CreateDate = DateTime.Now;

                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Thêm sản phẩm mới thành công", 3);
                    return RedirectToAction(nameof(Index));
                }
                _notyfService.Success("Thêm sản phẩm mới không thành công", 3);
                ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", product.CatId);
                return View(product);
            }
            catch (Exception ex)
            {
                _notyfService.Error("Lỗi" + ex.Message.ToString());
                return RedirectToAction("Index", "AdminProducts");
            }
        }

        // GET: Admin/AdminProducts/Edit/5
        [Authentication]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", product.CatId);
            return View(product);
        }

        // POST: Admin/AdminProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? fThumb)
        {
            try
            {

                if (id != product.ProductId)
                {
                    return NotFound();
                }
                if (ModelState.IsValid)
                {
                    try
                    {
                        product.ProductName = Utilities.ToTitleCase(product.ProductName);
                        if (fThumb != null)
                        {
                            string extension = Path.GetExtension(fThumb.FileName);
                            string image = Utilities.SEOUrl(product.ProductName) + extension;
                            product.Thumb = await Utilities.UpLoadFile(fThumb, @"products", image.ToLower());
                        }
                        if (string.IsNullOrEmpty(product.Thumb))
                        {
                            product.Thumb = "default.jpg";
                        }
                        product.Alias = Utilities.SEOUrl(product.ProductName);
                        product.ModifiedDate = DateTime.Now;
                        _context.Update(product);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProductExists(product.ProductId))
                        {
                            _notyfService.Error("Sửa thông tin sản phẩm lỗi", 3);
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    _notyfService.Success("Sửa thông tin sản phẩm thành công", 3);
                    return RedirectToAction(nameof(Index));
                }
                _notyfService.Error("Sửa thông tin sản phẩm không thành công", 3);
                ViewData["CatId"] = new SelectList(_context.Categories, "CatId", "CatName", product.CatId);
                return View(product);
            }
            catch (Exception ex)
            {
                _notyfService.Error("Lỗi" + ex.Message.ToString());
                return RedirectToAction("Index", "AdminProducts");
            }
        }
        [Authentication]
        // GET: Admin/AdminProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cat)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authentication]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'CuaHangNhatBanContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa thông tin sản phẩm" + product.ProductName + " thành công", 3);
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }

        public IActionResult CatFilter(int CatId = 0)
        {
            var url = $"/Admin/AdminProducts";
            if (CatId != 0)
            {
                url = $"/Admin/AdminProducts/Index?CatId={CatId}";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
    }
}
