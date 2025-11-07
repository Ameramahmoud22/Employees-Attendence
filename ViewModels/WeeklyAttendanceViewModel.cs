using Employees_Attendence.Models;

namespace Employees_Attendence.ViewModels
{
    public class WeeklyAttendanceViewModel
    {
        public List<WorkerAttendanceRow> Rows { get; set; } = new List<WorkerAttendanceRow>();
        public List<DateTime> WeekDates { get; set; } = new List<DateTime>();
        public decimal Total { get; set; }
    }

    public class WorkerAttendanceRow
    {
        public Worker Worker { get; set; } = new Worker();
        public List<bool> Attendance { get; set; } = new List<bool>(); // true = present, false = absent
        public int DaysWorked { get; set; }
        public decimal DailyWage { get; set; }
        public decimal Deductions { get; set; }
        public decimal TotalEarned { get; set; }
    }

}
