using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Employees_Attendence.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employees_Attendence.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AttendanceController(ApplicationDbContext db) => _db = db;

        // GET: Attendance (الأسبوع الحالي)
        public async Task<IActionResult> Index(int? weekOffset)
        {
            // weekOffset = 0 => الأسبوع الحالي، -1 => الأسبوع السابق، +1 => الأسبوع القادم
            var offset = weekOffset ?? 0;

            // حساب بداية الأسبوع الذي يبدأ يوم السبت
            var today = DateTime.Today;
            // تعيين اليوم ليوم السبت للأسبوع الحالي
            int daysSinceSaturday = ((int)today.DayOfWeek + 1) % 7; // Saturday -> 0
            var saturdayThisWeek = today.AddDays(-daysSinceSaturday).Date;

            // تطبيق الـ offset أسابيع (كل offset زيادة/نقص بسبعة أيام)
            var weekStart = saturdayThisWeek.AddDays(offset * 7);
            var weekDates = Enumerable.Range(0, 6).Select(i => weekStart.AddDays(i)).ToList(); // السبت → الخميس

            var workers = await _db.Workers.Include(w => w.Category).OrderBy(w => w.Name).ToListAsync();

            // تحميل سجلات الحضور للأسبوع المعني
            var dateFrom = weekDates.First();
            var dateTo = weekDates.Last();

            var attendance = await _db.AttendanceRecords
                .Where(a => a.Date >= dateFrom && a.Date <= dateTo)
                .ToListAsync();

            // سنبني ViewModel خفيف في الـ View مباشرةً (وليس ملف منفصل) — لكن نمرر البيانات اللازمة
            ViewBag.WeekDates = weekDates;
            ViewBag.Workers = workers;
            ViewBag.Attendance = attendance.ToDictionary(a => (a.WorkerId, a.Date.Date), a => a);

            // تمرير offset لتمكين التنقل بين الأسابيع
            ViewBag.WeekOffset = offset;

            return View();
        }

        // POST: Save week attendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWeek(int weekOffset)
        {
            // إعادة بناء تواريخ الأسبوع كـ GET
            var today = DateTime.Today;
            int daysSinceSaturday = ((int)today.DayOfWeek + 1) % 7;
            var saturdayThisWeek = today.AddDays(-daysSinceSaturday).Date;
            var weekStart = saturdayThisWeek.AddDays(weekOffset * 7);
            var weekDates = Enumerable.Range(0, 6).Select(i => weekStart.AddDays(i)).ToList();

            var workers = await _db.Workers.ToListAsync();

            // لكل عامل ولكل تاريخ نقرأ من الـ Request.Form وجود الحقل "present_{workerId}_{yyyyMMdd}"
            foreach (var worker in workers)
            {
                foreach (var d in weekDates)
                {
                    string key = $"present_{worker.Id}_{d:yyyyMMdd}";
                    bool isPresent = Request.Form.ContainsKey(key) && Request.Form[key] == "on";

                    var existing = await _db.AttendanceRecords
                        .FirstOrDefaultAsync(a => a.WorkerId == worker.Id && a.Date == d.Date);

                    if (existing == null)
                    {
                        // إذا لا يوجد سجل وأن العامل حاضر فقط → نضيف سجل جديد
                        var rec = new AttendanceRecord
                        {
                            WorkerId = worker.Id,
                            Date = d.Date,
                            IsPresent = isPresent
                        };
                        _db.AttendanceRecords.Add(rec);
                    }
                    else
                    {
                        // تحديث الحالة لو اختلفت
                        if (existing.IsPresent != isPresent)
                        {
                            existing.IsPresent = isPresent;
                            _db.AttendanceRecords.Update(existing);
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { weekOffset = weekOffset });
        }
    }
}
