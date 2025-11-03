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
            // حساب الإحصائيات المطلوبة لعرضها في لوحة التحكم
            ViewBag.TotalWorkers = await _db.Workers.CountAsync();
            ViewBag.TotalCategories = await _db.Categories.CountAsync();
            ViewBag.DailyWageSum = await _db.Workers.SumAsync(w => w.DailyWage);

            // حساب إجمالي القبض الشهري الأخير
            var lastMonth = DateTime.Today.AddMonths(-1);
            var lastMonthlyPay = await _db.MonthlyPayrollRecords
                .Where(r => r.Year == lastMonth.Year && r.Month == lastMonth.Month)
                .SumAsync(r => r.NetPay);
            ViewBag.LastMonthlyPay = lastMonthlyPay;

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