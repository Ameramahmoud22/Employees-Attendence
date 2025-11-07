using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employees_Attendence.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoriesController(ApplicationDbContext db) => _db = db;

        // قائمة الفئات
        public async Task<IActionResult> Index() {
            return View(await _db.Categories.Include(c => c.Workers).ToListAsync());
        }

        // إضافة فئة (GET)
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category cat)
        {
            if (!ModelState.IsValid)
            {
                return View(cat);
            }
            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // تعديل فئة (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        // تعديل فئة (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category cat)
        {
            if (id != cat.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(cat);
            }
            _db.Update(cat);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}