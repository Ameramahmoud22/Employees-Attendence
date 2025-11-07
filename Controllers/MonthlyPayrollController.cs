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

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var currentMonth = month ?? DateTime.Today.Month;
            var currentYear = year ?? DateTime.Today.Year;

            ViewBag.Month = currentMonth;
            ViewBag.Year = currentYear;

            var startDate = new DateTime(currentYear, currentMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var workers = await _context.Workers.Where(w => w.PayrollType == "Monthly").Include(w => w.Category).OrderBy(w => w.Name).ToListAsync();

            var attendanceDays = await _context.AttendanceRecords
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate && a.Status == "حاضر")
                .GroupBy(a => a.WorkerId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var existingRecords = await _context.MonthlyPayrollRecords
                .Where(m => m.Month == currentMonth && m.Year == currentYear)
                .ToDictionaryAsync(m => m.WorkerId);

            var model = new List<MonthlyPayrollRecord>();
            var attendanceDaysDict = new Dictionary<int, int>();

            foreach (var worker in workers)
            {
                var days = attendanceDays.TryGetValue(worker.Id, out var count) ? count : 0;
                attendanceDaysDict[worker.Id] = days;

                if (existingRecords.TryGetValue(worker.Id, out var record))
                {
                    record.Worker = worker;
                    model.Add(record);
                }
                else
                {
                    model.Add(new MonthlyPayrollRecord
                    {
                        WorkerId = worker.Id,
                        Worker = worker,
                        Month = currentMonth,
                        Year = currentYear,
                        MonthlyAmount = worker.DailyWage * days,
                        Advances = 0,
                        Deductions = 0,
                        NetPay = worker.DailyWage * days,
                        Notes = ""
                    });
                }
            }
            var uncategorizedCategory = new Category { Id = 0, Name = "غير مصنف" };
             var payrollByCategory = model
                .GroupBy(p => p.Worker.Category)
                .OrderBy(g => g.Key == null ? 1 : 0)
                .ThenBy(g => g.Key?.Name)
                .ToDictionary(g => g.Key ?? uncategorizedCategory, g => g.ToList());


            ViewBag.PayrollByCategory = payrollByCategory;
            ViewBag.AttendanceDays = attendanceDaysDict;
            ViewBag.GrandTotal = model.Sum(p => p.NetPay);

            return View(model ?? new List<MonthlyPayrollRecord>());
        }

        [HttpPost]
        public async Task<IActionResult> SaveMonthly([FromForm] List<MonthlyPayrollRecord> entries)
        {
            if (entries == null || !entries.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var firstEntry = entries.First();
            int month = firstEntry.Month;
            int year = firstEntry.Year;

            foreach (var entry in entries)
            {
                var worker = await _context.Workers.FindAsync(entry.WorkerId);
                if (worker == null) continue;
                
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                var presentDays = await _context.AttendanceRecords.CountAsync(a => a.WorkerId == entry.WorkerId && a.AttendanceDate >= startDate && a.AttendanceDate <= endDate && a.Status == "حاضر");

                entry.MonthlyAmount = worker.DailyWage * presentDays;
                entry.NetPay = entry.MonthlyAmount - (entry.Advances + entry.Deductions);
                entry.Notes ??= string.Empty;

                var existing = await _context.MonthlyPayrollRecords
                    .FirstOrDefaultAsync(r => r.WorkerId == entry.WorkerId && r.Month == month && r.Year == year);

                if (existing != null)
                {
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
            return RedirectToAction(nameof(Index), new { month, year });
        }
    }
}
