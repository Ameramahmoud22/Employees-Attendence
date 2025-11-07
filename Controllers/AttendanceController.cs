using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
            int daysToAdd = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek - 7) % 7;
            var startOfWeek = today.AddDays(daysToAdd);
            var weekDates = Enumerable.Range(0, 6).Select(i => startOfWeek.AddDays(i)).ToList();

            // نجيب كل العمال مع الفئة
            var workers = await _context.Workers.Include(w => w.Category).ToListAsync();

            var workersByCategory = workers
                .GroupBy(w => w.Category)
                .OrderBy(g => g.Key == null ? 1 : 0)
                .ThenBy(g => g.Key?.Name)
                .ToDictionary(
                    g => g.Key ?? new Category { Id = 0, Name = "غير مصنف" },
                    g => g.OrderBy(w => w.Name).ToList()
                );

            // نجيب الحضور في هذا الأسبوع
            var attendanceRecords = await _context.AttendanceRecords
                .Where(a => a.AttendanceDate >= startOfWeek && a.AttendanceDate <= startOfWeek.AddDays(5))
                .ToListAsync();

            // نخزنهم في Dictionary ليسهل الوصول
            var attendanceDict = attendanceRecords.ToDictionary(
                a => (a.WorkerId, a.AttendanceDate.Date),
                a => a
            );

            ViewBag.WeekDates = weekDates;
            ViewBag.WorkersByCategory = workersByCategory;
            ViewBag.Attendance = attendanceDict;
            ViewBag.WeekOffset = weekOffset;

            return View();
        }

        // 💾 حفظ الحضور للأسبوع الحالي
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveWeek(int weekOffset, IFormCollection form)
        {
            var recordsToUpdate = new List<AttendanceRecord>();
            var recordsToAdd = new List<AttendanceRecord>();

            var today = DateTime.Today.AddDays(weekOffset * 7);
            int daysToAddInWeek = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek - 7) % 7;
            var startOfWeek = today.AddDays(daysToAddInWeek);
            var endOfWeek = startOfWeek.AddDays(5);

            var existingRecords = await _context.AttendanceRecords
                .Where(a => a.AttendanceDate >= startOfWeek && a.AttendanceDate <= endOfWeek)
                .ToDictionaryAsync(a => (a.WorkerId, a.AttendanceDate.Date));

            foreach (var key in form.Keys)
            {
                if (!key.StartsWith("status_")) continue;

                var parts = key.Split('_');
                if (parts.Length != 3) continue;

                if (!int.TryParse(parts[1], out int workerId)) continue;
                if (!DateTime.TryParseExact(parts[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) continue;

                var workerExists = await _context.Workers.AnyAsync(w => w.Id == workerId);
                if (!workerExists)
                {
                    continue;
                }

                var selectedStatus = form[key];
                if (string.IsNullOrEmpty(selectedStatus)) continue;

                if (existingRecords.TryGetValue((workerId, date.Date), out var existing))
                {
                    if (existing.Status != selectedStatus)
                    {
                        existing.Status = selectedStatus;
                        recordsToUpdate.Add(existing);
                    }
                }
                else
                {
                    var record = new AttendanceRecord
                    {
                        WorkerId = workerId,
                        AttendanceDate = date,
                        Status = selectedStatus,
                        Notes = ""
                    };
                    recordsToAdd.Add(record);
                }
            }

            if (recordsToUpdate.Any())
            {
                _context.UpdateRange(recordsToUpdate);
            }
            if (recordsToAdd.Any())
            {
                _context.AddRange(recordsToAdd);
            }

            if (recordsToUpdate.Any() || recordsToAdd.Any())
            {
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "تم حفظ الحضور الأسبوعي بنجاح ✅";
            return RedirectToAction(nameof(Index), new { weekOffset });
        }
    }
}
