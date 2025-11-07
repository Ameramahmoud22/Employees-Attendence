using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Employees_Attendence.Models
{
    public class MonthlyPayrollRecord
    {
        public int Id { get; set; }

        public int WorkerId { get; set; }
        public Worker? Worker { get; set; }

        [Display(Name = "شهر")]
        public int Month { get; set; } // 1..12

        [Display(Name = "سنة")]
        public int Year { get; set; }

        [Display(Name = "مبلغ القبض (شهري)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyAmount { get; set; }

        [Display(Name = "الاستلاف")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Advances { get; set; }

        [Display(Name = "خصومات")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }

        [Display(Name = "صافي القبض")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPay { get; set; }

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; } = string.Empty;
    }
}
