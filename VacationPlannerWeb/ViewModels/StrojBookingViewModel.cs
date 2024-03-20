using VacationPlannerWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.ViewModels
{
    public class StrojBookingViewModel
    {
        public StrojBooking StrojBooking {get; set;}
        public StrojDay StrojDay {get; set;}
        [Display(Name ="Cílový stroj")]
        public int KopieDest {get; set;}
    }
}