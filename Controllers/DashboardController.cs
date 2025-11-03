using Employees_Attendence.Data;
using Microsoft.AspNetCore.Mvc;

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
            var totalWorkers = _context.Workers.Count();
            var totalCategories = _context.Categories.Count();

            var totalDailyPay = _context.Workers.Sum(w => w.DailyWage);
            var totalDeductions = _context.Workers.Sum(w => w.Deductions);
            var totalAdvances = _context.Workers.Sum(w => w.Advance);
            var totalWeeklyPay = (totalDailyPay * 6) - (totalDeductions + totalAdvances);

            var topCategories = _context.Categories
                .Select(c => new
                {
                    c.Name,
                    Count = _context.Workers.Count(w => w.CategoryId == c.Id)
                })
                .ToList();

            ViewBag.TotalWorkers = totalWorkers;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalWeeklyPay = totalWeeklyPay;
            ViewBag.TotalDeductions = totalDeductions + totalAdvances;
            ViewBag.TopCategories = topCategories;

            return View();
        }
    }
}
