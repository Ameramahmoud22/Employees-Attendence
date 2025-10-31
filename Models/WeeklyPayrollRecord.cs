using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Employees_Attendence.Models
{
    public class WeeklyPayrollRecord
    {
        public int Id { get; set; }

        [Required]
        public int WorkerId { get; set; }
        public Worker Worker { get; set; }

        [Display(Name = "بداية الأسبوع")]
        public DateTime WeekStart { get; set; }

        [Display(Name = "مبلغ القبض (أسبوعي)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal WeeklyAmount { get; set; } // غالبًا DailyWage * أيام العمل أو قيمة ثابتة

        [Display(Name = "الاستلاف خلال الأسبوع")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Advances { get; set; }

        [Display(Name = "خصومات")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }

        [Display(Name = "إجمالي القبض بعد الخصم")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPay { get; set; }

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }
    }
}
