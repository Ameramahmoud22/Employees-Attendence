using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Employees_Attendence.Models
{
    public class Worker
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العامل مطلوب")]
        [Display(Name = "الاسم")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "نوع العامل")]
        public string? Type { get; set; } // "عامل" أو "صنايعي" مثلاً

        [Display(Name = "الهاتف")]
        public string? Phone { get; set; }

        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Display(Name = "مبلغ القبض باليوم")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyWage { get; set; } = 0m;

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        [Display(Name = "الفئة")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        [Display(Name = "نوع القبض")]
        public string PayrollType { get; set; } = string.Empty; // "Weekly" or "Monthly"

        // **[جديد]** لإحصائيات القبض
        public ICollection<WeeklyPayrollRecord> WeeklyPayrollRecords { get; set; } = new List<WeeklyPayrollRecord>();
        public ICollection<MonthlyPayrollRecord> MonthlyPayrollRecords { get; set; } = new List<MonthlyPayrollRecord>();

        // **[جديد]** لإضافة سجلات الحضور
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    }
}
