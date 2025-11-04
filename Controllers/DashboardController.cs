using Employees_Attendence.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Employees_Attendence.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // عدد العمال والتصنيفات
            var totalWorkers = _context.Workers.Count();
            var totalCategories = _context.Categories.Count();

            // إجمالي الأجر اليومي (من جدول العمال)
            var totalDailyPay = _context.Workers.Sum(w => w.DailyWage);

            // إجمالي السلف والخصومات (من جدول المرتبات الأسبوعية)
            var totalAdvances = _context.WeeklyPayrollRecords.Sum(r => (decimal?)r.Advance) ?? 0;
            var totalDeductions = _context.WeeklyPayrollRecords.Sum(r => (decimal?)r.Deductions) ?? 0;

            // الحساب الإجمالي للأسبوع
            var totalWeeklyPay = (totalDailyPay * 6) - (totalDeductions + totalAdvances);

            // أكثر التصنيفات وجودًا
            var topCategories = _context.Categories
                .Select(c => new
                {
                    c.Name,
                    Count = _context.Workers.Count(w => w.CategoryId == c.Id)
                })
                .OrderByDescending(c => c.Count)
                .Take(5)
                .ToList();

            // تمرير القيم إلى الواجهة
            ViewBag.TotalWorkers = totalWorkers;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalWeeklyPay = totalWeeklyPay;
            ViewBag.TotalDeductions = totalDeductions + totalAdvances;
            ViewBag.TopCategories = topCategories;

            return View();
        }
    }
}
