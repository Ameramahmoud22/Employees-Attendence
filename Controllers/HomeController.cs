using System.Diagnostics;
using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employees_Attendence.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalEmployees = await _db.Workers.CountAsync();
            ViewBag.TotalCategories = await _db.Categories.CountAsync();

            var weeklyPayrolls = await _db.WeeklyPayrollRecords.ToListAsync();
            ViewBag.TotalPayrollRecords = weeklyPayrolls.Sum(p => p.NetPay);

            var monthlyPayrolls = await _db.MonthlyPayrollRecords.ToListAsync();
            ViewBag.TotalMonthlyPayroll = monthlyPayrolls.Sum(p => p.NetPay);

            ViewBag.MostActiveCategory = _db.AttendanceRecords.Include(a => a.Worker.Category)
                .GroupBy(a => a.Worker.Category.Name)
                .Select(g => new { CategoryName = g.Key, AttendanceCount = g.Count() })
                .OrderByDescending(x => x.AttendanceCount)
                .FirstOrDefault()?.CategoryName;

            ViewBag.PresentEmployeesToday = await _db.AttendanceRecords.CountAsync(a => a.AttendanceDate.Date == DateTime.Today);
            ViewBag.TotalDeductionsThisWeek = weeklyPayrolls
                .Where(p => p.WeekStart >= DateTime.Today.AddDays(-7))
                .Sum(p => p.Deductions);

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
