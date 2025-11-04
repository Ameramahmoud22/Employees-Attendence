using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Employees_Attendence.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "العامل")]
        public int WorkerId { get; set; }
        public Worker Worker { get; set; }

        [Required]
        [Display(Name = "تاريخ الحضور")]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        // يمكن أن يكون "حاضر"، "غياب"، "إجازة"
        [Display(Name = "حالة الحضور")]
        public string Status { get; set; } = "حاضر";

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }
    }
}

