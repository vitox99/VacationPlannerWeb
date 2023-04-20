using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.Models
{
    public class Zakazka
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Číslo zakázky")]
        public string CisloZakazky {get; set;}
        [Display(Name = "Zákazník")]
        public int? ZakaznikId {get; set;}
        [NotMapped]
        public Zakaznik Zakaznik { get; set; }
        [Required]
        [Display(Name = "Název zakázky")]
        public string Name { get; set; }
        public ICollection<VacationBooking> VacationBookings { get; set; }
        [Display(Name ="Barva")]
        public string Color {get; set;}
        [Display(Name = "Směr práce")]
        public string SmerPrace {get; set;}
        [Display(Name = "Poznámka")]
        public string Poznamka {get; set;}

    }
}