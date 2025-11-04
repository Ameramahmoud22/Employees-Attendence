using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
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

        // ✅ عرض القبض الشهري مع حساب أيام الحضور
        public async Task<IActionResult> BatchMonthly(int? month, int? year)
        {
            month ??= DateTime.Today.Month;
            year ??= DateTime.Today.Year;

            ViewBag.Month = month;
            ViewBag.Year = year;

            var startDate = new DateTime(year.Value, month.Value, 1).Date;
            var endDate = startDate.AddMonths(1).AddDays(-1).Date;

            var workers = await _context.Workers.Include(w => w.Category).ToListAsync();

            var existingRecords = await _context.MonthlyPayrollRecords
                .Where(m => m.Month == month && m.Year == year)
                .ToDictionaryAsync(r => r.WorkerId, r => r);

            var attendanceRecords = await _context.AttendanceRecords
                .Where(r => r.AttendanceDate.Date >= startDate && r.AttendanceDate.Date <= endDate)
                .ToListAsync();

            var model = new List<MonthlyPayrollRecord>();

            foreach (var w in workers)
            {
                var presentDays = attendanceRecords
                    .Count(r => r.WorkerId == w.Id && r.Status == "حاضر");

                if (existingRecords.TryGetValue(w.Id, out var existingRecord))
                {
                    existingRecord.Notes = $"({presentDays} يوم حضور) - " + (existingRecord.Notes ?? "");
                    model.Add(existingRecord);
                }
                else
                {
                    var newRecord = new MonthlyPayrollRecord
                    {
                        WorkerId = w.Id,
                        Worker = w,
                        Month = month.Value,
                        Year = year.Value,
                        MonthlyAmount = w.DailyWage * presentDays,
                        Advances = 0,
                        Deductions = 0,
                        NetPay = w.DailyWage * presentDays,
                        Notes = $"{presentDays} يوم حضور"
                    };
                    model.Add(newRecord);
                }
            }

            return View(model);
        }

        // ✅ حفظ القبض الشهري
        [HttpPost]
        public async Task<IActionResult> SaveMonthly([FromForm] MonthlyPayrollRecord[] entries)
        {
            if (entries == null || !entries.Any())
                return BadRequest("لا توجد بيانات محفوظة.");

            var first = entries.First();
            int month = first.Month;
            int year = first.Year;

            foreach (var entry in entries)
            {
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
            return RedirectToAction(nameof(BatchMonthly), new { month, year });
        }
    }
}
