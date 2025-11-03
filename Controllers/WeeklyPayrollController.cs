using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Employees_Attendence.Controllers
{
    public class WeeklyPayrollController : Controller
    {

        private readonly ApplicationDbContext _db;
        public WeeklyPayrollController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(DateTime? weekStart)
        {
            var start = weekStart ?? DateTime.Today.StartOfWeek(DayOfWeek.Saturday); // helper below
            ViewBag.WeekStart = start;

            var records = await _db.Workers
                .Include(w => w.Category)
                .Select(w => new WeeklyPayrollRecord
                {
                    Worker = w,
                    WorkerId = w.Id,
                    WeekStart = start,
                    WeeklyAmount = w.DailyWage * 6 // example: 6 أيام
                })
                .ToListAsync();

            // We will use the view to allow entering advances/discounts and saving.

            return View(records);
        }

        [HttpPost]
        public async Task<IActionResult> SaveWeekly([FromForm] WeeklyPayrollRecord[] entries)
        {
            foreach (var e in entries)
            {
                e.NetPay = e.WeeklyAmount - e.Advances - e.Deductions;
                _db.WeeklyPayrollRecords.Add(e);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }

    public static class DateExt
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}

