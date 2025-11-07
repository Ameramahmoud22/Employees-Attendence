using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employees_Attendence.Controllers
{
    public class WeeklyPayrollController : Controller
    {
        private readonly ApplicationDbContext _db;

        public WeeklyPayrollController(ApplicationDbContext db)
        {
            _db = db;
        }

        // تحديد بداية الأسبوع (السبت)
        private DateTime GetWeekStart(DateTime date)
        {
            int daysToAdd = ((int)DayOfWeek.Saturday - (int)date.DayOfWeek - 7) % 7;
            return date.AddDays(daysToAdd).Date;
        }

        // ✅ عرض سجلات القبض الأسبوعي + حساب عدد أيام الحضور
        public async Task<IActionResult> Index(DateTime? weekStart)
        {
            var start = GetWeekStart(weekStart ?? DateTime.Today);
            var end = start.AddDays(5).Date; // Week from Saturday to Thursday
            ViewBag.WeekStart = start;

            var workers = await _db.Workers.Where(w => w.PayrollType == "Weekly").Include(w => w.Category).OrderBy(w => w.Name).ToListAsync();

            var attendanceRecords = await _db.AttendanceRecords
                .Where(r => r.AttendanceDate.Date >= start && r.AttendanceDate.Date <= end && r.Status == "حاضر")
                .GroupBy(r => r.WorkerId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            var existingPayroll = await _db.WeeklyPayrollRecords
                .Where(r => r.WeekStart.Date == start.Date)
                .ToDictionaryAsync(r => r.WorkerId, r => r);

            var payrollModel = new List<WeeklyPayrollRecord>();
            var attendanceDaysDict = new Dictionary<int, int>();

            foreach (var w in workers)
            {
                var presentDays = attendanceRecords.TryGetValue(w.Id, out var days) ? days : 0;
                attendanceDaysDict[w.Id] = presentDays;

                if (existingPayroll.TryGetValue(w.Id, out var existingRecord))
                {
                    existingRecord.Worker = w; // Ensure navigation property is loaded
                    payrollModel.Add(existingRecord);
                }
                else
                {
                    var newRecord = new WeeklyPayrollRecord
                    {
                        WorkerId = w.Id,
                        Worker = w,
                        WeekStart = start,
                        WeeklyAmount = w.DailyWage * presentDays,
                        Advances = 0,
                        Deductions = 0,
                        NetPay = w.DailyWage * presentDays,
                        Notes = ""
                    };
                    payrollModel.Add(newRecord);
                }
            }

            var uncategorizedCategory = new Category { Id = 0, Name = "غير مصنف" };
            var payrollByCategory = payrollModel
                .GroupBy(p => p.Worker.Category)
                .OrderBy(g => g.Key == null ? 1 : 0)
                .ThenBy(g => g.Key?.Name)
                .ToDictionary(g => g.Key ?? uncategorizedCategory, g => g.ToList());

            ViewBag.PayrollByCategory = payrollByCategory;
            ViewBag.AttendanceDays = attendanceDaysDict;
            ViewBag.GrandTotal = payrollModel.Sum(p => p.NetPay);

            return View(payrollModel); // Pass the flat list for form model binding
        }

        // ✅ حفظ بيانات القبض الأسبوعي
        [HttpPost]
        public async Task<IActionResult> SaveWeekly([FromForm] List<WeeklyPayrollRecord> entries)
        {
            if (entries == null || !entries.Any())
            {
                return RedirectToAction(nameof(Index)); // No data to save
            }

            var weekStart = GetWeekStart(entries.First().WeekStart.Date);

            foreach (var entry in entries)
            {
                // Find the worker to get the daily wage
                var worker = await _db.Workers.FindAsync(entry.WorkerId);
                if (worker == null) continue;

                // Recalculate amounts based on form values
                var attendanceDays = (await _db.AttendanceRecords.Where(r => r.WorkerId == entry.WorkerId && r.AttendanceDate >= weekStart && r.AttendanceDate < weekStart.AddDays(6) && r.Status == "حاضر").CountAsync());
                entry.WeeklyAmount = worker.DailyWage * attendanceDays;
                entry.NetPay = entry.WeeklyAmount - (entry.Advances + entry.Deductions);
                entry.WeekStart = weekStart;
                 entry.Notes ??= string.Empty;

                var existing = await _db.WeeklyPayrollRecords
                    .FirstOrDefaultAsync(r => r.WorkerId == entry.WorkerId && r.WeekStart.Date == weekStart);

                if (existing != null)
                {
                    existing.Advances = entry.Advances;
                    existing.Deductions = entry.Deductions;
                    existing.NetPay = entry.NetPay;
                    existing.Notes = entry.Notes;
                    _db.Update(existing);
                }
                else
                {
                     // Only add if there's something to record
                    if(entry.Advances > 0 || entry.Deductions > 0 || !string.IsNullOrEmpty(entry.Notes))
                    {
                        _db.Add(entry);
                    }
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { weekStart });
        }
    }
}
