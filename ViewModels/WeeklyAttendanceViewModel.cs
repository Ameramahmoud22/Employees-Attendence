using Employees_Attendence.Models;

namespace Employees_Attendence.ViewModels
{
    public class WeeklyAttendanceViewModel
    {
        public List<WorkerAttendanceRow> Rows { get; set; }
        public List<DateTime> WeekDates { get; set; }
        public decimal Total { get; set; }
    }

    public class WorkerAttendanceRow
    {
        public Worker Worker { get; set; }
        public List<bool> Attendance { get; set; } // true = present, false = absent
        public int DaysWorked { get; set; }
        public decimal DailyWage { get; set; }
        public decimal Deductions { get; set; }
        public decimal TotalEarned { get; set; }
    }

}
