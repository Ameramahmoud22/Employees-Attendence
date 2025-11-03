using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Employees_Attendence.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الفئة مطلوب")]
        [Display(Name = "اسم الفئة")]
        public string Name { get; set; }

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        public ICollection<Worker>? Workers { get; set; }

    }
}
