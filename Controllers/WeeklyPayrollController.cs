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
            while (date.DayOfWeek != DayOfWeek.Saturday)
                date = date.AddDays(-1);
            return date.Date;
        }

        // ✅ عرض سجلات القبض الأسبوعي + حساب عدد أيام الحضور
        public async Task<IActionResult> Index(DateTime? weekStart)
        {
            var start = GetWeekStart(weekStart ?? DateTime.Today);
            var end = start.AddDays(6).Date;
            ViewBag.WeekStart = start;

            var workers = await _db.Workers.Include(w => w.Category).ToListAsync();

            // نجيب سجلات الحضور للأسبوع ده
            var attendanceRecords = await _db.AttendanceRecords
                .Where(r => r.AttendanceDate.Date >= start && r.AttendanceDate.Date <= end)
                .ToListAsync();

            // السجلات القديمة (لو موجودة)
            var existingPayroll = await _db.WeeklyPayrollRecords
                .Where(r => r.WeekStart.Date == start.Date)
                .ToDictionaryAsync(r => r.WorkerId, r => r);

            var model = new List<WeeklyPayrollRecord>();

            foreach (var w in workers)
            {
                var presentDays = attendanceRecords
                    .Count(r => r.WorkerId == w.Id && r.Status == "حاضر");

                if (existingPayroll.TryGetValue(w.Id, out var existingRecord))
                {
                    existingRecord.Notes = $"({presentDays} يوم حضور) - " + (existingRecord.Notes ?? "");
                    model.Add(existingRecord);
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
                        Notes = $"{presentDays} يوم حضور"
                    };
                    model.Add(newRecord);
                }
            }

            return View(model);
        }

        // ✅ حفظ بيانات القبض الأسبوعي
        [HttpPost]
        public async Task<IActionResult> SaveWeekly([FromForm] WeeklyPayrollRecord[] entries)
        {
            if (entries == null || !entries.Any())
                return BadRequest("لا توجد بيانات لحفظها.");

            var weekStart = entries.First().WeekStart.Date;

            foreach (var entry in entries)
            {
                entry.NetPay = entry.WeeklyAmount - (entry.Advances + entry.Deductions);
                entry.WeekStart = weekStart;

                var existing = await _db.WeeklyPayrollRecords
                    .FirstOrDefaultAsync(r => r.WorkerId == entry.WorkerId && r.WeekStart.Date == weekStart);

                if (existing != null)
                {
                    existing.WeeklyAmount = entry.WeeklyAmount;
                    existing.Advances = entry.Advances;
                    existing.Deductions = entry.Deductions;
                    existing.NetPay = entry.NetPay;
                    existing.Notes = entry.Notes;
                    _db.Update(existing);
                }
                else
                {
                    _db.Add(entry);
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { weekStart });
        }
    }
}
