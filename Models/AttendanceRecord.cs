using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Employees_Attendence.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        [Required]
        public int WorkerId { get; set; }
        public Worker Worker { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public bool IsPresent { get; set; }
    }
}

