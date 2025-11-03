using Microsoft.AspNetCore.Mvc;
using Employees_Attendence.Models;
using Employees_Attendence.Data;
using Microsoft.EntityFrameworkCore;

namespace Employees_Attendence.Controllers
{
    public class MonthlyPayrollController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonthlyPayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // صفحة عرض قائمة السجلات التي تم حفظها بالفعل
        public async Task<IActionResult> Index()
        {
            var records = await _context.MonthlyPayrollRecords
                .Include(r => r.Worker)
                .ThenInclude(w => w.Category)
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToListAsync();

            return View(records);
        }

        // عرض القبض الشهري بنظام الفلترة (الصفحة الرئيسية لإدخال البيانات)
        public async Task<IActionResult> BatchMonthly(int? month, int? year)
        {
            month ??= DateTime.Today.Month;
            year ??= DateTime.Today.Year;

            ViewBag.Month = month;
            ViewBag.Year = year;

            // حساب عدد أيام الشهر لتقدير قيمة القبض المبدئية
            int daysInMonth = DateTime.DaysInMonth(year.Value, month.Value);

            var workers = await _context.Workers.Include(w => w.Category).ToListAsync();
            var existingRecords = await _context.MonthlyPayrollRecords
                .Where(m => m.Month == month && m.Year == year)
                .ToDictionaryAsync(m => m.WorkerId);

            var model = workers.Select(w =>
            {
                var record = existingRecords.GetValueOrDefault(w.Id);

                // 1. إذا كان السجل موجودًا، استخدمه
                if (record != null)
                    return record;

                // 2. إذا لم يكن موجودًا، أنشئ سجلًا جديدًا
                return new MonthlyPayrollRecord
                {
                    WorkerId = w.Id,
                    Worker = w,
                    Month = month.Value,
                    Year = year.Value,
                    // القيمة المقترحة: الأجر اليومي للعامل * عدد أيام الشهر
                    MonthlyAmount = w.DailyWage * daysInMonth,
                    Advances = 0,
                    Deductions = 0,
                    NetPay = 0,
                    Notes = ""
                };
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveMonthly(List<MonthlyPayrollRecord> entries)
        {
            if (entries == null || !entries.Any())
                return BadRequest("لا توجد بيانات محفوظة.");

            foreach (var entry in entries)
            {
                // إعادة حساب الصافي قبل الحفظ
                entry.NetPay = entry.MonthlyAmount - (entry.Advances + entry.Deductions);

                var existing = await _context.MonthlyPayrollRecords
                    .FirstOrDefaultAsync(r => r.WorkerId == entry.WorkerId && r.Month == entry.Month && r.Year == entry.Year);

                if (existing != null)
                {
                    existing.MonthlyAmount = entry.MonthlyAmount;
                    existing.Advances = entry.Advances;
                    existing.Deductions = entry.Deductions;
                    existing.NetPay = entry.NetPay;
                    existing.Notes = entry.Notes;
                    _context.Update(existing);
                }
                else
                {
                    _context.Add(entry);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(BatchMonthly), new { month = entries.First().Month, year = entries.First().Year });
        }
    }
}
