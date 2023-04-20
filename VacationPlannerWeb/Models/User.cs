using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacationPlannerWeb.Models
{
    public class User : IdentityUser
    {
        [Display(Name = "Jméno")]
        public string FirstName { get; set; }
        [Display(Name = "Příjmení")]
        public string LastName { get; set; }
        [Display(Name = "Název")]
        public string DisplayName { get; set; }
        [Display(Name = "Team")]
        public int? TeamId { get; set; }
        [NotMapped]
        public Team Team { get; set; }
        [Display(Name = "Oddělení")]
        public int? DepartmentId { get; set; }
        [NotMapped]
        public Department Department { get; set; }
        [NotMapped]
        [Display(Name = "Role")]
        public ICollection<Role> Roles { get; set; }
        [Display(Name = "Je skrytý")]
        public bool IsHidden { get; set; }
        [Display(Name = "Strojník")]
        public int? MistrId { get; set; }
        [NotMapped]
        public StrojMistr StrojMistr { get; set; }
        [Display(Name = "Manažer")]
        public string ManagerUserId { get; set; }
        [NotMapped]
        public User ManagerUser { get; set; }
        [NotMapped]
        public string Password { get; set; }
    }
}
