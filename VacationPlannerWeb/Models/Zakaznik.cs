using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.Models
{
    public class Zakaznik
    {
        [Key]
        public int Id { get; set; }

        [Display(Name ="Název")]
        public string Nazev {get; set;}

        [Display(Name ="IČ")]
        public string IC {get; set;}

        [Display(Name ="Telefon")]
        public string Tel {get; set;}

        [Display(Name ="Email")]
        public string Email {get; set;}

        [Display(Name ="Poznámka")]
        public string Poznamka {get; set;}
    }
}