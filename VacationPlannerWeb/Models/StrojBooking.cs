using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VacationPlannerWeb.Validations;

namespace VacationPlannerWeb.Models
{
    public class StrojBooking
    {
        public StrojBooking()
        {
            VacationDays = new HashSet<StrojDay>();
        }
        [Key]
        public int Id { get; set; }
        public int? StrojId {get; set;}
        public Stroj Stroj {get; set;}
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Od data")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [StrojFromBookingDateValidations(2, 12)]
        public DateTime FromDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Do data")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [StrojToBookingDateValidation(2, 12)]
        public DateTime ToDate { get; set; }

        public int? AbsenceTypeId { get; set; }
        
        [Display(Name = "Zakázka")]
        public AbsenceType AbsenceType { get; set; }
        
        [Display(Name = "Schválení")]
        public string Approval { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Poznámka")]
        public string Comment { get; set; }
        [Display(Name = "Dny v kalendáři")]
        public ICollection<StrojDay> VacationDays { get; set; }
    }

    public enum StrojSchvaleni
    {
        Pending,
        Approved,
        Denied,
    }
}
