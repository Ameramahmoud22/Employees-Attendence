using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Employees_Attendence.Controllers
{
    public class WorkersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public WorkersController(ApplicationDbContext db)
        {
            _db = db;
        }

        // عرض كل العمال
        public async Task<IActionResult> Index()
        {
            var workers = await _db.Workers
                .Include(w => w.Category)
                .ToListAsync();

            if (!workers.Any())
            {
                ViewBag.Message = "لا يوجد عمال حالياً في قاعدة البيانات.";
            }

            return View(workers);
        }

        // GET: صفحة الإضافة
        public IActionResult Create()
        {
            var categories = _db.Categories.ToList();

            if (!categories.Any())
            {
                ViewBag.NoCategories = true;
            }
            else
            {
                ViewBag.Categories = new SelectList(_db.Categories.ToList(), "Id", "Name");
            }

            return View();
        }

        // POST: إضافة عامل جديد
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Worker worker)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", worker.CategoryId);
                return View(worker);
            }

            _db.Workers.Add(worker);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: تعديل عامل
        public async Task<IActionResult> Edit(int id)
        {
            var worker = await _db.Workers.FindAsync(id);
            if (worker == null) return NotFound();

            ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", worker.CategoryId);
            return View(worker);
        }

        // POST: تعديل عامل
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Worker worker)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_db.Categories, "Id", "Name", worker.CategoryId);
                return View(worker);
            }

            _db.Workers.Update(worker);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // حذف عامل
        public async Task<IActionResult> Delete(int id)
        {
            var worker = await _db.Workers.FindAsync(id);
            if (worker == null) return NotFound();

            _db.Workers.Remove(worker);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //نجيب كل العمال من قاعدة البيانات، ومعاهم معلومات الفئة
        public async Task<IActionResult> CompanyWorkers()
        {
            var workers = await _db.Workers
                .Include(w => w.Category)
                .ToListAsync();

            return View(workers);
        }
    }
}
