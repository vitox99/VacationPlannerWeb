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
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime VacationDate { get; set; }
    }
}
