using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Employees_Attendence.Models
{
    public class Worker
    {


        public int Id { get; set; }

        [Required, Display(Name = "اسم العامل")]
        public string Name { get; set; }

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; }

        [Display(Name = "العنوان")]
        public string Address { get; set; }

        [Display(Name = "الوظيفة")]
        public string Type { get; set; } // عامل - صنايعي - مساعد

        [Display(Name = "قيمة اليومية (ج.م)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyWage { get; set; }

        [Display(Name = "الخصومات (ج.م)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }

        [Display(Name = "السُلف (ج.م)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Advance { get; set; }

        [Display(Name = "الملاحظات")]
        public string Notes { get; set; }

        [ForeignKey("Category")]
        [Display(Name = "الفئة")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // 💰 خاصية محسوبة تلقائيًا (ما تتحفظش في الداتا بيز)
        [NotMapped]
        [Display(Name = "إجمالي القبض الأسبوعي (ج.م)")]
        public decimal WeeklyPay => (DailyWage * 6) - (Deductions + Advance);
    }
    }

