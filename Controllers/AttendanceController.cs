using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employees_Attendence.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📅 عرض أسبوع الحضور
        public async Task<IActionResult> Index(int weekOffset = 0)
        {
            var today = DateTime.Today.AddDays(weekOffset * 7);
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Saturday);
            var weekDates = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();

            // نجيب كل العمال مع الفئة
            var workers = await _context.Workers.Include(w => w.Category).ToListAsync();

            // نجيب الحضور في هذا الأسبوع
            var attendanceRecords = await _context.AttendanceRecords
                .Where(a => a.AttendanceDate >= startOfWeek && a.AttendanceDate <= startOfWeek.AddDays(6))
                .ToListAsync();

            // نخزنهم في Dictionary ليسهل الوصول
            var attendanceDict = attendanceRecords.ToDictionary(
                a => (a.WorkerId, a.AttendanceDate.Date),
                a => a
            );

            ViewBag.WeekDates = weekDates;
            ViewBag.Workers = workers;
            ViewBag.Attendance = attendanceDict;
            ViewBag.WeekOffset = weekOffset;

            return View();
        }

        // 💾 حفظ الحضور للأسبوع الحالي
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWeek(int weekOffset)
        {
            var today = DateTime.Today.AddDays(weekOffset * 7);
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Saturday);
            var weekDates = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();

            var workers = await _context.Workers.ToListAsync();

            foreach (var w in workers)
            {
                foreach (var d in weekDates)
                {
                    // الاسم المتوقع من الـ form
                    var key = $"status_{w.Id}{d:yyyyMMdd}";
                    var selectedStatus = Request.Form[key];

                    if (string.IsNullOrEmpty(selectedStatus))
                        continue; // مفيش اختيار

                    var existing = await _context.AttendanceRecords
                        .FirstOrDefaultAsync(a => a.WorkerId == w.Id && a.AttendanceDate.Date == d.Date);

                    if (existing != null)
                    {
                        existing.Status = selectedStatus;
                        _context.Update(existing);
                    }
                    else
                    {
                        var record = new AttendanceRecord
                        {
                            WorkerId = w.Id,
                            AttendanceDate = d,
                            Status = selectedStatus,
                            Notes = null
                        };
                        _context.Add(record);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "تم حفظ الحضور الأسبوعي بنجاح ✅";
            return RedirectToAction(nameof(Index), new { weekOffset });
        }
    }
}
