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
        private readonly ApplicationDbContext _db; // إضافة السياق

        // تعديل Constructor لاستقبال السياق
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // ربط الإحصائيات بالبيانات الفعلية (الطلبات 6 و 7)
            ViewBag.TotalEmployees = await _db.Workers.CountAsync();
            ViewBag.TotalCategories = await _db.Categories.CountAsync();

            // يمكنك تغيير هذه الإحصائية لتكون أكثر دقة (مثلاً مجموع صافي القبض)
            ViewBag.TotalPayrollRecords = await _db.WeeklyPayrollRecords.CountAsync();

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