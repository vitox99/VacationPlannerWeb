using VacationPlannerWeb.Models;
using System;
using System.Collections.Generic;

namespace VacationPlannerWeb.ViewModels
{
    public class StrojCalendarOverviewViewModel
    {
        public DateTime? Date { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public string SortOrder { get; set; }
        //public List<string> AbsenceTypes { get; set; }
        public List<string> AbsenceTypes { get; set; }
        public List<Zakazka> AbsenceTypesBarva { get; set; }
        //public List<AbsenceType> Zakazky { get; set; }
        public Dictionary<Stroj, List<StrojCalendarDay>> AllUsersCalendarData { get; set; }
        public List<StrojCalendarDay> CalendarDaysList { get; set; }
        public List<FilterItem> RoleFilter { get; set; }
        public List<FilterItem> StrojnikFilter { get; set; }
        public List<FilterItem> DepartmentFilter { get; set; }
        public List<FilterItem> TeamFilter { get; set; }
        public string RoleFilterString { get; set; }
        public string StrojnikFilterString { get; set; }
        public string DepartmentFilterString { get; set; }
        public string TeamFilterString { get; set; }
        public string PostBackActionName { get; set; }
        public string Barva {get; set;}
        public StrojDay StrojDay {get; set;}
/*         public string Poznamka1 {get; set;}
        public int IdP1 {get;set;} */
    }
}
