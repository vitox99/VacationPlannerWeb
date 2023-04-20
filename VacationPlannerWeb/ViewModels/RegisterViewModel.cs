using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VacationPlannerWeb.Models;


namespace VacationPlannerWeb.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Heslo")]
        public string Password { get; set; }

        [Required]
        [DisplayName("Jméno")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Příjmení")]
        public string LastName { get; set; }

        [DisplayName("Team")]
        public int? TeamId { get; set; }

        [DisplayName("Oddělení")]
        public int? DepartmentId { get; set; }

        public List<string> Errors { get; set; }
    }
}
