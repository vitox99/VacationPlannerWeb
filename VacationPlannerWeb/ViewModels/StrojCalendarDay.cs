using System;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.ViewModels
{
    public class StrojCalendarDay
    {
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsHoliday { get; set; }
        public bool IsToday { get; set; }
        public bool IsStartOfWeek { get; set; }
        public int WeekNumber { get; set; }
        public bool IsPlannedVacation { get; set; }
        public string Approval { get; set; }
        public string Note { get; set; }
        public string AbsenceType { get; set; }
        public string AbsenceColor {get; set;}
        public string AbsenceSmer {get; set;}
        public string AbsenceStrojnik {get; set;}
        public string AbsenceNazev {get; set;}

        public int StrojBookingId { get; set; }

        public string Poznamka1 {get; set;}
        public string Poznamka2 {get; set;}
        public string Poznamka3 {get; set;}
        public string Poznamka4 {get; set;}
        public string Poznamka5 {get; set;}
        public string Poznamka6 {get; set;}
        public string Poznamka7 {get; set;}
        public string Poznamka8 {get; set;}

        public int StrojDayId {get; set;}
    }
}
