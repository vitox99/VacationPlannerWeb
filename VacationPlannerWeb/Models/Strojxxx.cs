using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacationPlannerWeb.Models
{
    public class Strojxxx
    {
        public string InvNr { get; set; }   //Inventární číslo :	5000079573
        public string Type { get; set; }    //Typ drážního vozidla :	Pluh pro úpravu štěrk. lože
        public string Rada { get; set; }    //Řada :	PUŠL 71
        public string NazevTSS { get; set; }    //Název TSS :	PUŠL 71 / 109
        public string Nr12 { get; set; }    //12 - místné číslo:
        public string Kategorie { get; set; }    //Kategorie stroje :	Pluhy na úpravu štěrkového lože
        public string PrZpusobilosti { get; set; }    //Číslo průkazu způsobilosti :	PZ 5869 / 07 - V.34
        public string VyrobniNr { get; set; }    //Výrobní číslo :	1209
        public int RokVyroby { get; set; }    //Rok výroby :	1984
        public string VlastnikTSS { get; set; }    //Vlastník TSS :	212
        public string DomovskaTSS { get; set; }    //Domovská TSS :	Lov Lovosice
        public string Vztah { get; set; }    //Vztah k vozidlu :	vlastní
        public string Doklad { get; set; }    //Doklad o pořízení :	2 / A / 310 / 06
        public bool EvDU { get; set; }    //Evidováno u DÚ :	A
        public string Vyrobce { get; set; }    //Výrobce :	MTH Praha a.s. - ČR
        public string Umisteni { get; set; }    //Dlouhodobé umístění :	
        public string PovoleniKPr { get; set; }    //Povolení k provozu na ŽSR :	
        public string AktCinnost { get; set; }    //Aktuální činnost :

        public string DisplayName { get; set; }
        public int? TeamId { get; set; }
        [NotMapped]
        public Team Team { get; set; }
        public int? DepartmentId { get; set; }
        [NotMapped]
        public Department Department { get; set; }
        [NotMapped]
        public bool IsHidden { get; set; }
        public string ManagerUserId { get; set; }
        [NotMapped]
        public User ManagerUser { get; set; }
    }
}