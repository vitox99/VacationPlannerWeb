using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Název")]
        public string Name { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Zkratka")]
        public string Shortening { get; set; }
    }
}
