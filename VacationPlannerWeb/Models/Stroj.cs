using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.Models
{
    public class Stroj
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Inventární číslo")]
        public string InvNr { get; set; }
        [Display(Name = "Název")]
        public string Name { get; set; }
        [Display(Name = "Popis")]
        public string Popis { get; set; }
        [Display(Name = "Posádka")]
        public int? TeamId { get; set; }
        [NotMapped]
        public Team Team { get; set; }
        [Display(Name = "Oddělení")]
        public int? DepartmentId { get; set; }
        [NotMapped]
        public Department Department { get; set; }
        [Display(Name = "Je skrytý")]
        public bool IsHidden { get; set; }
        [Display(Name = "Mistr")]
        public int? MistrId { get; set; }
        [NotMapped]
        public StrojMistr StrojMistr { get; set; }
    }
}
