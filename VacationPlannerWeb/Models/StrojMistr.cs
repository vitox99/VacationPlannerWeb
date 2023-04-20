using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.Models
{
    public class StrojMistr
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Osobní číslo")]
        public string MistrId { get; set; }
        [Display(Name = "Jméno")]
        public string Name { get; set; }
        
        [Display(Name = "Příjmení")]
        public string LastName { get; set; }
        [Display(Name = "Telefon")]
        public string Telefon { get; set; }
        [Display(Name = "Barva")]
        public string Color {get; set;}
        [Display(Name = "Alias")]
        public string Alias {get; set;}
    }
}
