using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacationPlannerWeb.Models
{
    public class Role : IdentityRole
    {
        //Add custom Role fields here
        [NotMapped]
        public ICollection<User> Users { get; set; }
        [Display(Name = "Zkratka")]
        public string Shortening { get; set; }
    }


}
