using System;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.Models
{
    public class StrojDay
    {
        public int Id { get; set; }
        //public int VacationBookingId { get; set; }
        public int StrojBookingId { get; set; }
        public StrojBooking StrojBooking { get; set; }

        [Required]
        [DataType(DataType.Date)]
        /* [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")] */
        public DateTime VacationDate { get; set; }

        [Display(Name ="Místo")]
        public string P1 {get; set;}
        [Display(Name ="Vedoucí práce")]
        public string P2 {get; set;}
        [Display(Name ="ZPŘS")]
        public string P3 {get; set;}
        [Display(Name ="Info o výluce")]
        public string P4 {get; set;}
        [Display(Name ="Směr práce")]
        public string P5 {get; set;}
        [Display(Name ="Měření do APK/rádia")]
        public string P6 {get; set;}
        [Display(Name ="Začátek výluky")]
        public string P7 {get; set;}
        [Display(Name ="Odstavení stroje")]
        public string P8 {get; set;}
    }
}
