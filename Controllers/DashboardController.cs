using Employees_Attendence.Data;
using Employees_Attendence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
            // Chart 1: Total Payroll by Category
            var payrollByCategory = _context.Categories
                .Include(c => c.Workers)
                .ThenInclude(w => w.WeeklyPayrollRecords)
                .Include(c => c.Workers)
                .ThenInclude(w => w.MonthlyPayrollRecords)
                .AsEnumerable()
                .Select(c => new
                {
                    CategoryName = c.Name,
                    TotalPayroll = c.Workers.SelectMany(w => w.WeeklyPayrollRecords).Sum(p => p.NetPay) +
                                   c.Workers.SelectMany(w => w.MonthlyPayrollRecords).Sum(p => p.NetPay)
                })
                .ToList();

            ViewBag.ChartLabels = payrollByCategory.Select(p => p.CategoryName).ToList();
            ViewBag.ChartData = payrollByCategory.Select(p => p.TotalPayroll).ToList();

            // Chart 2: Worker Distribution by Category
            var workerDistribution = _context.Categories
                .Include(c => c.Workers)
                .AsEnumerable()
                .Select(c => new
                {
                    CategoryName = c.Name,
                    WorkerCount = c.Workers.Count()
                })
                .ToList();

            ViewBag.WorkerDistLabels = workerDistribution.Select(d => d.CategoryName).ToList();
            ViewBag.WorkerDistData = workerDistribution.Select(d => d.WorkerCount).ToList();

            // Chart 3: Total Weekly Payroll (Last 4 Weeks)
            var weeklyPayroll = _context.WeeklyPayrollRecords
                .Where(p => p.WeekStart >= DateTime.Now.AddDays(-28))
                .AsEnumerable()
                .GroupBy(p => p.WeekStart.Date)
                .Select(g => new
                {
                    Week = g.Key,
                    TotalAmount = g.Sum(p => p.NetPay)
                })
                .OrderBy(g => g.Week)
                .ToList();

            ViewBag.WeeklyPayrollLabels = weeklyPayroll.Select(p => p.Week.ToShortDateString()).ToList();
            ViewBag.WeeklyPayrollData = weeklyPayroll.Select(p => p.TotalAmount).ToList();

            // Chart 4: Attendance by Category
            var attendanceByCategory = _context.Categories
                .Include(c => c.Workers)
                .ThenInclude(w => w.AttendanceRecords)
                .AsEnumerable()
                .Select(c => new
                {
                    CategoryName = c.Name,
                    TotalAttendance = c.Workers.SelectMany(w => w.AttendanceRecords).Count(a => a.Status == "حاضر")
                })
                .ToList();
            
            ViewBag.AttendanceLabels = attendanceByCategory.Select(a => a.CategoryName).ToList();
            ViewBag.AttendanceData = attendanceByCategory.Select(a => a.TotalAttendance).ToList();

            return View();
        }
    }
}
