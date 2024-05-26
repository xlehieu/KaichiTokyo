using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using WebsiteKaichiTokyo.Helper;
using WebsiteKaichiTokyo.Models;

namespace WebsiteKaichiTokyo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminSlidersController : Controller
    {
        private readonly CuaHangNhatBanContext _context;

        public AdminSlidersController(CuaHangNhatBanContext context)
        {
            _context = context;
        }

        // GET: Admin/AdminSliders
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page==null||page<=0? 1: page.Value;
            List<Slider> sliders = await _context.Sliders.AsNoTracking().OrderByDescending(x=>x.SliderId).ToListAsync();
            PagedList<Slider> models = new PagedList<Slider>(sliders.AsQueryable(),pageNumber,Utilities.PAGE_SIZE);
              return models!=null ? 
                          View(models) :
                          Problem("Entity set 'CuaHangNhatBanContext.Sliders'  is null.");
        }
        // GET: Admin/AdminSliders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Sliders == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.SliderId == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // GET: Admin/AdminSliders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminSliders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SliderId,SliderName,Alias,Active,HomeFlag,Thumb")] Slider slider,IFormFile? fThumb)
        {
            if (ModelState.IsValid)
            {
                slider.SliderName = Utilities.ToTitleCase(slider.SliderName);
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string imageName = Utilities.SEOUrl(slider.SliderName) + extension;
                    slider.Thumb = await Utilities.UpLoadFile(fThumb, @"sliders", imageName.ToLower());
                }
                if (string.IsNullOrEmpty(slider.Thumb))
                {
                    slider.Thumb = "default.jpg";
                }
                _context.Add(slider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slider);
        }

        // GET: Admin/AdminSliders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Sliders == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders.FindAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        // POST: Admin/AdminSliders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SliderId,SliderName,Alias,Active,HomeFlag,Thumb")] Slider slider,IFormFile? fThumb)
        {
            if (id != slider.SliderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    slider.SliderName = Utilities.ToTitleCase(slider.SliderName);
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string imageName = Utilities.SEOUrl(slider.SliderName) + extension;
                        slider.Thumb = await Utilities.UpLoadFile(fThumb, @"sliders", imageName.ToLower());
                    }
                    if (string.IsNullOrEmpty(slider.Thumb))
                    {
                        slider.Thumb = "default.jpg";
                    }
                    _context.Update(slider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SliderExists(slider.SliderId))
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
            return View(slider);
        }

        // GET: Admin/AdminSliders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Sliders == null)
            {
                return NotFound();
            }

            var slider = await _context.Sliders
                .FirstOrDefaultAsync(m => m.SliderId == id);
            if (slider == null)
            {
                return NotFound();
            }

            return View(slider);
        }

        // POST: Admin/AdminSliders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Sliders == null)
            {
                return Problem("Entity set 'CuaHangNhatBanContext.Sliders'  is null.");
            }
            var slider = await _context.Sliders.FindAsync(id);
            if (slider != null)
            {
                _context.Sliders.Remove(slider);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SliderExists(int id)
        {
          return (_context.Sliders?.Any(e => e.SliderId == id)).GetValueOrDefault();
        }
    }
}
