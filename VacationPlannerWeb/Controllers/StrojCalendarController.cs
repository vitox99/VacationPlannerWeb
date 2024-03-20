using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationPlannerWeb.DataAccess;
using VacationPlannerWeb.Models;
using System.Globalization;
using VacationPlannerWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using VacationPlannerWeb.Extensions;
using System.Data;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using VacationPlannerWeb.ViewModels;
using VacationPlannerWeb.DataAccess;
using VacationPlannerWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using VacationPlannerWeb.TagHelpers;


namespace VacationPlannerWeb.Controllers
{
    [Authorize]
    public class StrojCalendarController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private static readonly CultureInfo _cultureInfo = new CultureInfo("sv-SE");

        private const string SessionKeyRoleFilter = "_RoleFilter";
        private const string SessionKeyStrojnikFilter = "_StrojnikFilter";
        private const string SessionKeyDepartmentFilter = "_DepartmentFilter";
        private const string SessionKeyTeamsFilter = "_TeamFilter";
        private const string NoneRoleId = "#None-Role-Id";
        private const string NoneStrojnikId = "#None-Strojnik-Id";
        private const string NoneDepartmentId = "#None-Department-Id";
        private const string NoneTeamId = "#None-Team-Id";

        private const int amountOfWeeks = 3;

        public StrojCalendarController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> MyCalendar(int year, int month)
        {
            PagingLogicAndValidationForYearAndMonth(ref year, ref month);

            var firstDayOfMonth = CalendarHelper.GetFirstDayOfMonth(year, month);
            var shortDayOfWeekList = CalendarHelper.GetDayOfWeekListShort();
            var dayOfWeekList = CalendarHelper.GetDayOfWeekList();

            var user = await GetCurrentUser();

            var vacBookingList = await GetVacationBookingsByUser(user.Id);

            var absenceTypesList = await _context.AbsenceTypes.Select(x => x.Name).ToListAsync();
            var workFreeDaysList = await _context.WorkFreeDays.ToListAsync();

            var vacDaysList = GetAllVacationDaysFromBookings(vacBookingList);

            DateTime dateOfFirstDayInWeekOfMonth = CalendarHelper.GetFirstDayInWeek(firstDayOfMonth, dayOfWeekList);

            var dataLists = new CalendarDataLists(vacBookingList, workFreeDaysList, vacDaysList);

            var weekCalendarDayDic = new Dictionary<int, List<StrojCalendarDay>>();

            var displayDatesOfWeek = 7; //Change to 5 to exclude saturdays and sundays
            /* const int amountOfWeeks = 6; */
            const int totalDaysOfWeek = 7;
            for (int w = 0; w < amountOfWeeks; w++)
            {
                DateTime firstDayInWeek = dateOfFirstDayInWeekOfMonth.AddDays(w * totalDaysOfWeek);
                int weekNumber = CalendarHelper.GetISO8601WeekNumber(firstDayInWeek, _cultureInfo);
                List<StrojCalendarDay> calDaysInWeek = GetCalendarDaysInWeek(dataLists, firstDayInWeek, displayDatesOfWeek);
                weekCalendarDayDic.Add(weekNumber, calDaysInWeek);
            }

            var calendarVM = new StrojCalendarViewModel
            {
                Year = year,
                Month = month,
                Date = firstDayOfMonth,
                DayOfWeekList = shortDayOfWeekList,
                WeekCalendarData = weekCalendarDayDic,
                AbsenceTypes = absenceTypesList,
                PostBackActionName = nameof(MyCalendar)
            };
            return View(calendarVM);
        }

        //[Authorize(Roles = "Admin,Manager")]
        [Authorize]
        public async Task<IActionResult> StrojManagerOverview(int year, int weeknumber, string sortOrder)
        {
            PagingLogicAndValidationForYearAndWeekNumber(ref year, ref weeknumber, _cultureInfo);

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["StrojnikSortParam"] = sortOrder == "strojnik_desc" ? "strojnik_desc" : "strojnik";
            ViewData["RoleSortParam"] = sortOrder == "role_desc" ? "role_desc" : "role";
            ViewData["DepartmentSortParam"] = sortOrder == "department_desc" ? "department" : "department_desc";
            ViewData["TeamSortParam"] = sortOrder == "team_desc" ? "team" : "team_desc";

            var roleFilter = await GetRoleFilterItems();
            var strojnikFilter = await GetStrojnikFilterItems();
            var departmentFilter = await GetDepartmentFilterItems();
            var teamFilter = await GetTeamFilterItems();

            var roleFilterString = HttpContext.Session.GetString(SessionKeyRoleFilter);
            var strojnikFilterString = HttpContext.Session.GetString(SessionKeyStrojnikFilter);
            var departmentFilterString = HttpContext.Session.GetString(SessionKeyDepartmentFilter);
            var teamFilterString = HttpContext.Session.GetString(SessionKeyTeamsFilter);

            var strojList = await GetAllStrojsWithRoles();
            strojList = FilterStrojList(roleFilter, strojnikFilter, departmentFilter, teamFilter, strojList);
            var sortedUsersList = GetSortedStrojList(sortOrder, strojList);

            var absenceTypesList = await _context.AbsenceTypes.Select(x => x.Name).ToListAsync();
            var absenceTypesListBarva = await _context.AbsenceTypes.ToListAsync();
            var workFreeDaysList = await _context.WorkFreeDays.ToListAsync();

            DateTime currentFirstDayInWeek = CalendarHelper.FirstDateOfWeekISO8601(year, weeknumber, _cultureInfo);
            var userCalendarDayDic = await GetStrojCalendarDayDictionary(sortedUsersList, workFreeDaysList, currentFirstDayInWeek);

            List<StrojCalendarDay> caldaysList = GetAllCalendarDays(currentFirstDayInWeek);

            var calendarVM = new StrojCalendarOverviewViewModel
            {
                Year = year,
                WeekNumber = weeknumber,
                Date = currentFirstDayInWeek,
                AbsenceTypes = absenceTypesList,
                AbsenceTypesBarva = absenceTypesListBarva,
                AllUsersCalendarData = userCalendarDayDic,
                CalendarDaysList = caldaysList,
                SortOrder = sortOrder,
                RoleFilter = roleFilter,
                RoleFilterString = roleFilterString,
                StrojnikFilter = strojnikFilter,
                StrojnikFilterString = strojnikFilterString,
                DepartmentFilter = departmentFilter,
                DepartmentFilterString = departmentFilterString,
                TeamFilter = teamFilter,
                TeamFilterString = teamFilterString,
                PostBackActionName = nameof(StrojManagerOverview),
                Barva = "red"
            };
            return View(calendarVM);
        }


        //[Authorize(Roles = "Admin,Manager")]
        [Authorize]
        public async Task<IActionResult> StrojManagerOverviewV2(int year, int weeknumber, string sortOrder, string? datum)
        {
            if(datum is not null)
            {
                DateTime _datum = DateTime.ParseExact(datum, "yyyy-MM-dd",
                                       System.Globalization.CultureInfo.InvariantCulture);
                year = _datum.Year;
                weeknumber = GetIso8601WeekOfYear(_datum);
            }
            PagingLogicAndValidationForYearAndWeekNumber(ref year, ref weeknumber, _cultureInfo);

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["StrojnikSortParam"] = sortOrder == "strojnik_desc" ? "strojnik_desc" : "strojnik";
            ViewData["RoleSortParam"] = sortOrder == "role_desc" ? "role_desc" : "role";
            ViewData["DepartmentSortParam"] = sortOrder == "department_desc" ? "department" : "department_desc";
            ViewData["TeamSortParam"] = sortOrder == "team_desc" ? "team" : "team_desc";

            var roleFilter = await GetRoleFilterItems();
            var strojnikFilter = await GetStrojnikFilterItems();
            var departmentFilter = await GetDepartmentFilterItems();
            var teamFilter = await GetTeamFilterItems();

            var roleFilterString = HttpContext.Session.GetString(SessionKeyRoleFilter);
            var strojnikFilterString = HttpContext.Session.GetString(SessionKeyStrojnikFilter);
            var departmentFilterString = HttpContext.Session.GetString(SessionKeyDepartmentFilter);
            var teamFilterString = HttpContext.Session.GetString(SessionKeyTeamsFilter);

            var strojList = await GetAllStrojsWithRoles();
            strojList = FilterStrojList(roleFilter, strojnikFilter, departmentFilter, teamFilter, strojList);
            var sortedUsersList = GetSortedStrojList(sortOrder, strojList);

            var absenceTypesList = await _context.AbsenceTypes.Select(x => x.Name).ToListAsync();
            var absenceTypesListBarva = await _context.AbsenceTypes.ToListAsync();
            var workFreeDaysList = await _context.WorkFreeDays.ToListAsync();

            DateTime currentFirstDayInWeek = CalendarHelper.FirstDateOfWeekISO8601(year, weeknumber, _cultureInfo);
            var userCalendarDayDic = await GetStrojCalendarDayDictionary(sortedUsersList, workFreeDaysList, currentFirstDayInWeek);

            List<StrojCalendarDay> caldaysList = GetAllCalendarDays(currentFirstDayInWeek);

            var calendarVM = new StrojCalendarOverviewViewModel
            {
                Year = year,
                WeekNumber = weeknumber,
                Date = currentFirstDayInWeek,
                AbsenceTypes = absenceTypesList,
                AbsenceTypesBarva = absenceTypesListBarva,
                AllUsersCalendarData = userCalendarDayDic,
                CalendarDaysList = caldaysList,
                SortOrder = sortOrder,
                RoleFilter = roleFilter,
                RoleFilterString = roleFilterString,
                StrojnikFilter = strojnikFilter,
                StrojnikFilterString = strojnikFilterString,
                DepartmentFilter = departmentFilter,
                DepartmentFilterString = departmentFilterString,
                TeamFilter = teamFilter,
                TeamFilterString = teamFilterString,
                PostBackActionName = nameof(StrojManagerOverviewV2),
                Barva = "red"
            };
            return View(calendarVM);
        }

public static int GetIso8601WeekOfYear(DateTime time)
{
    // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
    // be the same week# as whatever Thursday, Friday or Saturday are,
    // and we always get those right
    DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
    if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
    {
        time = time.AddDays(3);
    }

    // Return the week of our adjusted day
    return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
}

/*
        [Authorize]
        public async Task<IActionResult> UserOverview(int year, int weeknumber, string sortOrder)
        {
            PagingLogicAndValidationForYearAndWeekNumber(ref year, ref weeknumber, _cultureInfo);

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["RoleSortParam"] = sortOrder == "role_desc" ? "role_desc" : "role";
            ViewData["DepartmentSortParam"] = sortOrder == "department_desc" ? "department_desc" : "department";
            ViewData["TeamSortParam"] = sortOrder == "team_desc" ? "team_desc" : "team";

            var roleFilter = await GetRoleFilterItems();
            var departmentFilter = await GetDepartmentFilterItems();
            var teamFilter = await GetTeamFilterItems();

            var roleFilterString = HttpContext.Session.GetString(SessionKeyRoleFilter);
            var departmentFilterString = HttpContext.Session.GetString(SessionKeyDepartmentFilter);
            var teamFilterString = HttpContext.Session.GetString(SessionKeyTeamsFilter);

            var userList = await GetAllUsersWithRoles();
            userList = FilterUserList(roleFilter, strojnikFilter, departmentFilter, teamFilter, userList);
            var sortedUsersList = GetSortedUserList(sortOrder, userList);

            var absenceTypesList = await _context.AbsenceTypes.Select(x => x.Name).ToListAsync();
            var workFreeDaysList = await _context.WorkFreeDays.ToListAsync();

            DateTime currentFirstDayInWeek = CalendarHelper.FirstDateOfWeekISO8601(year, weeknumber, _cultureInfo);

            var userCalendarDayDic = await GetUserCalendarDayDictionary(sortedUsersList, workFreeDaysList, currentFirstDayInWeek, true);

            List<StrojCalendarDay> caldaysList = GetAllCalendarDays(currentFirstDayInWeek);

            var calendarVM = new StrojCalendarOverviewViewModel
            {
                Year = year,
                WeekNumber = weeknumber,
                Date = currentFirstDayInWeek,
                AbsenceTypes = absenceTypesList,
                //AllUsersCalendarData = userCalendarDayDic,
                CalendarDaysList = caldaysList,
                SortOrder = sortOrder,
                RoleFilter = roleFilter,
                RoleFilterString = roleFilterString,
                DepartmentFilter = departmentFilter,
                DepartmentFilterString = departmentFilterString,
                TeamFilter = teamFilter,
                TeamFilterString = teamFilterString,
                PostBackActionName = nameof(UserOverview)
            };
            return View(calendarVM);
        }
*/

        private async Task<List<FilterItem>> GetTeamFilterItems()
        {
            var teamFilter = HttpContext.Session.Get<List<FilterItem>>(SessionKeyTeamsFilter);
            teamFilter = (teamFilter == null || teamFilter.Count == 0)
                ? AddNoneTeamsFilterItem(await GetTeamsToCheckBoxItemList()) : teamFilter;
            return teamFilter;
        }

        private async Task<List<FilterItem>> GetDepartmentFilterItems()
        {
            var departmentFilter = HttpContext.Session.Get<List<FilterItem>>(SessionKeyDepartmentFilter);
            departmentFilter = (departmentFilter == null || departmentFilter.Count == 0)
                ? AddNoneDepartmentFilterItem(await GetDepartmentsToCheckBoxItemList()) : departmentFilter;
            return departmentFilter;
        }

        private async Task<List<FilterItem>> GetRoleFilterItems()
        {
            var roleFilter = HttpContext.Session.Get<List<FilterItem>>(SessionKeyRoleFilter);
            roleFilter = (roleFilter == null || roleFilter.Count == 0)
                ? AddNoneRolesFilterItem(await GetRolesToCheckBoxItemList()) : roleFilter;
            return roleFilter;
        }
        private async Task<List<FilterItem>> GetStrojnikFilterItems()
        {
            var strojnikFilter = HttpContext.Session.Get<List<FilterItem>>(SessionKeyStrojnikFilter);
            strojnikFilter = (strojnikFilter == null || strojnikFilter.Count == 0)
                ? AddNoneStrojniksFilterItem(await GetStrojniksToCheckBoxItemList()) : strojnikFilter;
            return strojnikFilter;
        }

        private List<Stroj> FilterStrojList(List<FilterItem> roleFilter, List<FilterItem> strojnikFilter, List<FilterItem> departmentFilter, List<FilterItem> teamFilter, List<Stroj> strojList)
        {
            //strojList = FilterStrojRoles(strojList, roleFilter);
            strojList = FilterStrojStrojniks(strojList, strojnikFilter);
            strojList = FilterStrojDepartments(strojList, departmentFilter);
            strojList = FilterStrojTeams(strojList, teamFilter);
            strojList = GetDistinctStrojs(strojList);
            return strojList;
        }

        private List<User> FilterUserList(List<FilterItem> roleFilter, List<FilterItem> strojnikFilter, List<FilterItem> departmentFilter, List<FilterItem> teamFilter, List<User> userList)
        {
            userList = FilterUserRoles(userList, roleFilter);
            userList = FilterUserStrojniks(userList, strojnikFilter);
            userList = FilterUserDepartments(userList, departmentFilter);
            userList = FilterUserTeams(userList, teamFilter);
            userList = GetDistinctUsers(userList);
            return userList;
        }

        private async Task<Dictionary<User, List<StrojCalendarDay>>> GetUserCalendarDayDictionary(IEnumerable<User> sortedUsersList, List<WorkFreeDay> workFreeDaysList, DateTime currentFirstDayInWeek, bool excludeAbsenceType = false)
        {
            var userCalendarDayDic = new Dictionary<User, List<StrojCalendarDay>>();

            foreach (var user in sortedUsersList)
            {
                var userVacBookingList = await GetVacationBookingsByUser(user.Id, excludeAbsenceType);
                if (userVacBookingList.Count == 0)
                {
                    continue;
                }
                var userVacDaysList = GetAllVacationDaysFromBookings(userVacBookingList);
                var dataLists = new CalendarDataLists(userVacBookingList, workFreeDaysList, userVacDaysList);

                var displayDatesOfWeek = 7; //Change to 7 to include saturdays and sundays
                var allUserCalendarDays = GetAllCalendarDays(currentFirstDayInWeek, dataLists, displayDatesOfWeek);

                user.Team = await _context.Teams.AsNoTracking().SingleOrDefaultAsync(x => x.Id == user.TeamId);
                user.Department = await _context.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == user.DepartmentId);
                userCalendarDayDic.Add(user, allUserCalendarDays);
            }

            return userCalendarDayDic;
        }

        private async Task<Dictionary<Stroj, List<StrojCalendarDay>>> GetStrojCalendarDayDictionary(IEnumerable<Stroj> sortedStrojsList, List<WorkFreeDay> workFreeDaysList, DateTime currentFirstDayInWeek, bool excludeAbsenceType = false)
        {
            var strojCalendarDayDic = new Dictionary<Stroj, List<StrojCalendarDay>>();

            foreach (var stroj in sortedStrojsList)
            {
                var userVacBookingList = await GetVacationBookingsByStroj(stroj.Id, excludeAbsenceType);
                if (userVacBookingList.Count == 0)
                {
                    continue;
                }
                var userVacDaysList = GetAllVacationDaysFromBookings(userVacBookingList);
                var dataLists = new CalendarDataLists(userVacBookingList, workFreeDaysList, userVacDaysList);

                var displayDatesOfWeek = 7; //Change to 7 to include saturdays and sundays
                var allUserCalendarDays = GetAllCalendarDays(currentFirstDayInWeek, dataLists, displayDatesOfWeek);

                stroj.Team = await _context.Teams.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.TeamId);
                stroj.Department = await _context.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.DepartmentId);
                stroj.StrojMistr = await _context.StrojMistrs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.MistrId);
                strojCalendarDayDic.Add(stroj, allUserCalendarDays);
            }

            return strojCalendarDayDic;
        }

        private List<FilterItem> AddNoneTeamsFilterItem(List<FilterItem> list)
        {
            list.Add(
                new FilterItem()
                {
                    Id = NoneTeamId,
                    Name = "No Teams",
                    Selected = true,
                });
            return list;
        }

        private List<FilterItem> AddNoneDepartmentFilterItem(List<FilterItem> list)
        {
            list.Add(
                new FilterItem()
                {
                    Id = NoneDepartmentId,
                    Name = "No Departments",
                    Selected = true,
                });
            return list;
        }

        private List<FilterItem> AddNoneRolesFilterItem(List<FilterItem> list)
        {
            list.Add(
                new FilterItem()
                {
                    Id = NoneRoleId,
                    Name = "No Roles",
                    Selected = true,
                });
            return list;
        }

        private List<FilterItem> AddNoneStrojniksFilterItem(List<FilterItem> list)
        {
            list.Add(
                new FilterItem()
                {
                    Id = NoneStrojnikId,
                    Name = "Žádný strojník",
                    Selected = true,
                });
            return list;
        }

        private async Task<List<User>> GetAllUsersWithRoles()
        {
            var userList = await _context.Users.AsNoTracking().Where(x => !x.IsHidden).ToListAsync();
            foreach (var user in userList)
            {
                var userRoleIds = _context.UserRoles.AsNoTracking()
                    .Where(x => x.UserId == user.Id).Select(x => x.RoleId);
                var roles = _context.Roles.AsNoTracking()
                    .Join(userRoleIds, role => role.Id, id => id, (role, id) => role);
                user.Roles = await roles.ToListAsync();
            }
            return userList;
        }

        private async Task<List<Stroj>> GetAllStrojsWithRoles()
        {
            var strojList = await _context.Strojs.AsNoTracking().Where(x => !x.IsHidden).ToListAsync();
/*
            foreach (var stroj in strojList)
            {
                var userRoleIds = _context.UserRoles.AsNoTracking()
                    .Where(x => x.UserId == user.Id).Select(x => x.RoleId);
                var roles = _context.Roles.AsNoTracking()
                    .Join(userRoleIds, role => role.Id, id => id, (role, id) => role);
                user.Roles = await roles.ToListAsync();
            }*/
            return strojList;
        }

       /*  private static IEnumerable<User> GetSortedUserList(string sortOrder, IEnumerable<User> userList)
        {
            IEnumerable<User> userOrdered;
            switch (sortOrder)
            {
                case "name_desc":
                    userOrdered = userList.OrderByDescending(x => x.DisplayName);
                    break;
                case "role":
                    userOrdered = userList.OrderBy(x => x.Roles.FirstOrDefault()?.Name);
                    break;
                case "role_desc":
                    userOrdered = userList.OrderByDescending(x => x.Roles.FirstOrDefault()?.Name);
                    break;
                case "department":
                    userOrdered = userList.OrderBy(x => x.DepartmentId);
                    break;
                case "department_desc":
                    userOrdered = userList.OrderByDescending(x => x.DepartmentId);
                    break;
                case "team":
                    userOrdered = userList.OrderBy(x => x.TeamId);
                    break;
                case "team_desc":
                    userOrdered = userList.OrderByDescending(x => x.TeamId);
                    break;
                default:
                    userOrdered = userList.OrderBy(x => x.DisplayName);
                    break;
            }
            return userOrdered;
        }
 */
        private static IEnumerable<Stroj> GetSortedStrojList(string sortOrder, IEnumerable<Stroj> strojList)
        {
            IEnumerable<Stroj> userOrdered;
            switch (sortOrder)
            {
                case "name_desc":
                    userOrdered = strojList.OrderByDescending(x => x.Name);
                    break;
/*
                case "role":
                    userOrdered = strojList.OrderBy(x => x.Roles.FirstOrDefault()?.Name);
                    break;
                    
                case "role_desc":
                    userOrdered = strojList.OrderByDescending(x => x.Roles.FirstOrDefault()?.Name);
                    break;
*/
                case "department":
                    userOrdered = strojList.OrderBy(x => x.DepartmentId);
                    break;
                case "department_desc":
                    userOrdered = strojList.OrderByDescending(x => x.DepartmentId);
                    break;
                case "team":
                    userOrdered = strojList.OrderBy(x => x.TeamId);
                    break;
                case "team_desc":
                    userOrdered = strojList.OrderByDescending(x => x.TeamId);
                    break;
                default:
                    userOrdered = strojList.OrderBy(x => x.Name);
                    break;
            }
            return userOrdered;
        }

        private List<User> FilterUserRoles(List<User> userList, List<FilterItem> roleFilterList)
        {
            var list = new List<User>();
            foreach (var roleFilterItem in roleFilterList)
            {
                if (roleFilterItem.Selected)
                {
                    list.AddRange(roleFilterItem.Id == NoneRoleId
                        ? userList.Where(x => x.IsHidden == false).Where(x => !x.Roles.Any())
                        : userList.Where(u => u.Roles.Any(r => r.Id == roleFilterItem.Id)));
                }
            }
            return list;
        }


        private List<Stroj> FilterStrojRoles(List<Stroj> strojList, List<FilterItem> roleFilterList)
        {
            var list = new List<Stroj>();
            /*
            foreach (var roleFilterItem in roleFilterList)
            {
                if (roleFilterItem.Selected)
                {
                    list.AddRange(roleFilterItem.Id == NoneRoleId
                        ? strojList.Where(x => x.IsHidden == false).Where(x => !x.Roles.Any())
                        : strojList.Where(u => u.Roles.Any(r => r.Id == roleFilterItem.Id)));
                }
            }*/
            return list;
        }

        private List<User> FilterUserStrojniks(List<User> userList, List<FilterItem> strojnikFilter)
        {
            var list = new List<User>();
            foreach (var stroj in strojnikFilter)
            {
                if (stroj.Selected)
                {
                    list.AddRange(stroj.Id == NoneStrojnikId
                        ? userList.Where(x => x.IsHidden == false)
                            .Where(x => x.MistrId == 0 || x.MistrId == null)
                        : userList.Where(x => x.DepartmentId?.ToString() == stroj.Id));
                }
            }
            return list;
        }

        private List<Stroj> FilterStrojStrojniks(List<Stroj> strojList, List<FilterItem> strojnikFilter)
        {
            var list = new List<Stroj>();
            foreach (var stroj in strojnikFilter)
            {
                if (stroj.Selected)
                {
                    list.AddRange(stroj.Id == NoneStrojnikId
                        ? strojList.Where(x => x.IsHidden == false)
                            .Where(x => x.MistrId == 0 || x.MistrId == null)
                        : strojList.Where(x => x.MistrId.ToString() == stroj.Id));
                }
            }
            return list;
        }

        private List<User> FilterUserDepartments(List<User> userList, List<FilterItem> departmentFilter)
        {
            var list = new List<User>();
            foreach (var dep in departmentFilter)
            {
                if (dep.Selected)
                {
                    list.AddRange(dep.Id == NoneDepartmentId
                        ? userList.Where(x => x.IsHidden == false)
                            .Where(x => x.DepartmentId == 0 || x.DepartmentId == null)
                        : userList.Where(x => x.DepartmentId?.ToString() == dep.Id));
                }
            }
            return list;
        }

        private List<Stroj> FilterStrojDepartments(List<Stroj> strojList, List<FilterItem> departmentFilter)
        {
            var list = new List<Stroj>();
            foreach (var dep in departmentFilter)
            {
                if (dep.Selected)
                {
                    list.AddRange(dep.Id == NoneDepartmentId
                        ? strojList.Where(x => x.IsHidden == false)
                            .Where(x => x.DepartmentId == 0 || x.DepartmentId == null)
                        : strojList.Where(x => x.DepartmentId?.ToString() == dep.Id));
                }
            }
            return list;
        }

        private List<User> FilterUserTeams(List<User> userList, List<FilterItem> teamsFilter)
        {
            var list = new List<User>();
            foreach (var team in teamsFilter)
            {
                if (team.Selected)
                {
                    list.AddRange(team.Id == NoneTeamId
                        ? userList.Where(x => x.IsHidden == false).Where(x => x.TeamId == 0 || x.TeamId == null)
                        : userList.Where(x => x.TeamId?.ToString() == team.Id));
                }
            }
            return list;
        }

        private List<Stroj> FilterStrojTeams(List<Stroj> strojList, List<FilterItem> teamsFilter)
        {
            var list = new List<Stroj>();
            foreach (var team in teamsFilter)
            {
                if (team.Selected)
                {
                    list.AddRange(team.Id == NoneTeamId
                        ? strojList.Where(x => x.IsHidden == false).Where(x => x.TeamId == 0 || x.TeamId == null)
                        : strojList.Where(x => x.TeamId?.ToString() == team.Id));
                }
            }
            return list;
        }

        private static List<User> GetDistinctUsers(List<User> userList)
        {
            return userList.GroupBy(u => u.Id).Select(g => g.First()).ToList();
        }

        private static List<Stroj> GetDistinctStrojs(List<Stroj> strojList)
        {
            return strojList.GroupBy(u => u.Id).Select(g => g.First()).ToList();
        }

        [HttpPost, ActionName("OverviewSetFilter")]
        public async Task<IActionResult> OverviewSetFilter([Bind] StrojCalendarOverviewViewModel model)
        {
            var roleFilter = model.RoleFilter;
            var strojnikFilter = model.StrojnikFilter;
            var departmentFilter = model.DepartmentFilter;
            var teamFilter = model.TeamFilter;

            roleFilter = (roleFilter == null || roleFilter.Count == 0)
                ? AddNoneRolesFilterItem(await GetRolesToCheckBoxItemList()) : roleFilter;
            strojnikFilter = (strojnikFilter == null || strojnikFilter.Count == 0)
                ? AddNoneStrojniksFilterItem(await GetStrojniksToCheckBoxItemList()) : strojnikFilter;
            departmentFilter = (departmentFilter == null || departmentFilter.Count == 0)
                ? AddNoneDepartmentFilterItem(await GetDepartmentsToCheckBoxItemList()) : departmentFilter;
            teamFilter = (teamFilter == null || teamFilter.Count == 0)
                ? AddNoneTeamsFilterItem(await GetTeamsToCheckBoxItemList()) : teamFilter;

            var calendarVM = new StrojCalendarOverviewViewModel
            {
                Year = model.Year,
                WeekNumber = model.WeekNumber,
                SortOrder = model.SortOrder,
            };

            HttpContext.Session.Set(SessionKeyRoleFilter, roleFilter);
            HttpContext.Session.Set(SessionKeyStrojnikFilter, strojnikFilter);
            HttpContext.Session.Set(SessionKeyDepartmentFilter, departmentFilter);
            HttpContext.Session.Set(SessionKeyTeamsFilter, teamFilter);
            return RedirectToAction(model.PostBackActionName, calendarVM);
        }

        private List<StrojCalendarDay> GetAllCalendarDays(DateTime currentFirstDayInWeek)
        {
            var displayDatesOfWeek = 7; //Change to 7 to include saturdays and sundays
            /* const int amountOfWeeks = 4; */
            const int totalDaysOfWeek = 7;
            return Enumerable.Range(0, amountOfWeeks)
                .SelectMany(num =>
                    GetCalendarDaysInWeek(currentFirstDayInWeek.AddDays(num * totalDaysOfWeek), displayDatesOfWeek))
                .ToList();
        }

        private static List<StrojCalendarDay> GetAllCalendarDays(DateTime currentFirstDayInWeek, CalendarDataLists dataLists, int displayDatesOfWeek)
        {
            //const int amountOfWeeks = 4;
            const int totalDaysOfWeek = 7;
            return Enumerable.Range(0, amountOfWeeks)
                .SelectMany(num =>
                    GetCalendarDaysInWeek(dataLists, currentFirstDayInWeek.AddDays(num * totalDaysOfWeek), displayDatesOfWeek))
                .ToList();
        }

        private async Task<List<FilterItem>> GetRolesToCheckBoxItemList()
        {
            return await _context.Roles.Where(x => x.Name != "Admin" && x.Name != "Manager")
                .Select(x => new FilterItem { Id = x.Id, Name = $"{x.Name} - {x.Shortening}", Selected = true }).ToListAsync();
        }

        private async Task<List<FilterItem>> GetStrojniksToCheckBoxItemList()
        {
            return await _context.StrojMistrs.Select(x => new FilterItem { Id = x.Id.ToString(), Name = $"{x.LastName} {x.Name}", Selected = true }).ToListAsync();
        }

        private async Task<List<FilterItem>> GetDepartmentsToCheckBoxItemList()
        {
            return await _context.Departments.Select(x => new FilterItem { Id = x.Id.ToString(), Name = $"{x.Name} - {x.Shortening}", Selected = true }).ToListAsync();
        }

        private async Task<List<FilterItem>> GetTeamsToCheckBoxItemList()
        {
            return await _context.Teams.Select(x => new FilterItem { Id = x.Id.ToString(), Name = $"{x.Name} - {x.Shortening}", Selected = true }).ToListAsync();
        }

        private void PagingLogicAndValidationForYearAndWeekNumber(ref int year, ref int weeknumber, CultureInfo culture)
        {
            var today = DateTime.Today;

            if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
            {
                year = today.Year;
                weeknumber = CalendarHelper.GetISO8601WeekNumber(today, culture);
            }

            var lastWeekOfYear = CalendarHelper.GetLastWeekOfYear(year, culture);
            if (year > 0 && weeknumber <= 0)
            {
                year -= 1;
                weeknumber = lastWeekOfYear;
            }
            if (year > 0 && weeknumber > lastWeekOfYear)
            {
                year += 1;
                weeknumber = 1;
            }

        }

        private static List<StrojCalendarDay> GetCalendarDaysInWeek(CalendarDataLists dataLists, DateTime firstDayInWeek, int displayDatesOfWeek)
        {
            return Enumerable.Range(0, displayDatesOfWeek)
                .Select(num => GetCalendarDay(dataLists, firstDayInWeek.AddDays(num))).ToList();
        }

        private static List<StrojCalendarDay> GetCalendarDaysInWeek(DateTime firstDayInWeek, int displayDatesOfWeek)
        {
            return Enumerable.Range(0, displayDatesOfWeek)
                .Select(num => GetCalendarDay(firstDayInWeek.AddDays(num))).ToList();
        }

        private static StrojCalendarDay GetCalendarDay(CalendarDataLists dataLists, DateTime weekDate)
        {
            string approval = null;
            string absenceType = null;
            string absenceColor = null;
            string absenceSmer = null;
            string absenceStrojnik = null;
            string absenceNazev = null;
            bool isPlanned = false;
            var vacationBookingId = 0;
            bool isHoliday = false;
            string note = null;
            string poznamka1 = null;
            string poznamka2 = null;
            string poznamka3 = null;
            string poznamka4 = null;
            string poznamka5 = null;
            string poznamka6 = null;
            string poznamka7 = null;
            string poznamka8 = null;
            int strojDayId = 0;

            if (dataLists.VacDaysList.Any(v => v.VacationDate == weekDate))
            {
                var vacBookingId = dataLists.VacDaysList.FirstOrDefault(v => v.VacationDate == weekDate).StrojBookingId;
                poznamka1 = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).P1;
                poznamka2 = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).P2;
                poznamka3 = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).P3;
                poznamka4 = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).P4;
                poznamka5 = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).P5;
                poznamka6 = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).P6;
                poznamka7 = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).P7;
                poznamka8 = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).P8;
                strojDayId = dataLists.VacDaysList.FirstOrDefault(x => x.VacationDate == weekDate).Id;

                var vacbooking = dataLists.VacBookingList.FirstOrDefault(v => v.Id == vacBookingId);
                approval = vacbooking.Approval;
                absenceType = vacbooking.AbsenceType?.Name;
                absenceColor = vacbooking.AbsenceType?.Color;
                absenceNazev = vacbooking.AbsenceType.CisloZakazky;
                //absenceSmer = vacbooking.AbsenceType?.SmerPrace;
                absenceSmer = vacbooking.SmerPrace;
                absenceStrojnik = vacbooking.StrojMistr.Alias;
                isPlanned = true;
                
                vacationBookingId = vacBookingId;
            }
            if (dataLists.HolidayList.Contains(weekDate))
            {
                isHoliday = true;
                note = dataLists.WorkFreeDaysList.FirstOrDefault(x => x.Date == weekDate).Name;
            }

            return new StrojCalendarDay()
            {
                Approval = approval,
                AbsenceType = absenceType,
                AbsenceColor = absenceColor,
                AbsenceSmer = absenceSmer,
                AbsenceStrojnik = absenceStrojnik,
                AbsenceNazev = absenceNazev,
                IsPlannedVacation = isPlanned,
                StrojBookingId = vacationBookingId,
                IsHoliday = isHoliday,
                Note = note,
                IsToday = (weekDate == DateTime.Today),
                IsWeekend = (weekDate.DayOfWeek == DayOfWeek.Saturday || weekDate.DayOfWeek == DayOfWeek.Sunday),
                IsStartOfWeek = (weekDate.DayOfWeek == DayOfWeek.Monday),
                Date = weekDate,
                WeekNumber = CalendarHelper.GetISO8601WeekNumber(weekDate, _cultureInfo),
                Poznamka1 = poznamka1,
                Poznamka2 = poznamka2,
                Poznamka3 = poznamka3,
                Poznamka4 = poznamka4,
                Poznamka5 = poznamka5,
                Poznamka6 = poznamka6,
                Poznamka7 = poznamka7,
                Poznamka8 = poznamka8,
                StrojDayId = strojDayId,


            };
        }

        private static StrojCalendarDay GetCalendarDay(DateTime weekDate)
        {
            string approval = null;
            string absenceType = null;
            string absenceColor = null;
            string absenceSmer = null;
            //string absenceStrojnik = null;
            bool isPlanned = false;
            var vacationBookingId = 0;
            bool isHoliday = false;
            string note = null;

            return new StrojCalendarDay()
            {
                Approval = approval,
                AbsenceType = absenceType,
                AbsenceColor = absenceColor,
                AbsenceSmer = absenceSmer,
                //AbsenceStrojnik = absenceStrojnik,
                IsPlannedVacation = isPlanned,
                StrojBookingId = vacationBookingId,
                IsHoliday = isHoliday,
                Note = note,
                IsToday = (weekDate == DateTime.Today),
                IsWeekend = (weekDate.DayOfWeek == DayOfWeek.Saturday || weekDate.DayOfWeek == DayOfWeek.Sunday),
                IsStartOfWeek = (weekDate.DayOfWeek == DayOfWeek.Monday),
                Date = weekDate,
                WeekNumber = CalendarHelper.GetISO8601WeekNumber(weekDate, _cultureInfo),
            };
        }

        public class CalendarDataLists
        {
            public CalendarDataLists(List<StrojBooking> vacBookingList,
                List<WorkFreeDay> workFreeDaysList,
                List<StrojDay> vacDaysList)
            {
                VacBookingList = vacBookingList;
                WorkFreeDaysList = workFreeDaysList;
                HolidayList = workFreeDaysList.Select(x => x.Date).ToList(); ;
                VacDaysList = vacDaysList;
            }

            public List<StrojBooking> VacBookingList { get; }
            public List<WorkFreeDay> WorkFreeDaysList { get; }
            public List<DateTime> HolidayList { get; }
            public List<StrojDay> VacDaysList { get; }
        }

        private static List<StrojDay> GetAllVacationDaysFromBookings(List<StrojBooking> vacBookingList)
        {
            return vacBookingList.SelectMany(x => x.VacationDays).ToList();
        }

        private async Task<List<StrojBooking>> GetVacationBookingsByUser(string strojId, bool excludeAbsenceType = false)
        {
            if (excludeAbsenceType)
            {
                return await _context.StrojBookings.AsNoTracking()
                .Include(v => v.User).Include(v => v.VacationDays)
                //.Where(x => x.UserId == userjId).ToListAsync();
                .Where(x => x.UserId == strojId).ToListAsync();
            }
 /*           else
            {
                return await _context.StrojBookings.AsNoTracking()
                .Include(v => v.User).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .Where(x => x.UserId == userId).ToListAsync();
            }*/
            else
            {
                return await _context.StrojBookings.AsNoTracking()
                .Include(v => v.Stroj).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .Where(x => x.StrojId.Equals(strojId)).ToListAsync();
            }

        }

        private async Task<List<StrojBooking>> GetVacationBookingsByStroj(int strojId, bool excludeAbsenceType = false)
        {
            if (excludeAbsenceType)
            {
                return await _context.StrojBookings.AsNoTracking()
                .Include(v => v.User).Include(v => v.VacationDays)
                //.Where(x => x.UserId == userjId).ToListAsync();
                .Where(x => x.StrojId == strojId).ToListAsync();
            }
 /*           else
            {
                return await _context.StrojBookings.AsNoTracking()
                .Include(v => v.User).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .Where(x => x.UserId == userId).ToListAsync();
            }*/
            else
            {
                return await _context.StrojBookings.AsNoTracking()
                .Include(v => v.Stroj).Include(v => v.VacationDays).Include(v => v.AbsenceType).Include(v => v.StrojMistr)
                .Where(x => x.StrojId.Equals(strojId)).ToListAsync();
            }

        }

        private async Task<List<StrojBooking>> GetVacationBookingsByUserNoAbsenceType(string userId)
        {
            return await _context.StrojBookings.AsNoTracking()
                .Include(v => v.User).Include(v => v.VacationDays)
                .Where(x => x.UserId == userId).ToListAsync();
        }

        private async Task<User> GetCurrentUser()
        {
            return await _userManager.GetUserAsync(HttpContext.User);
        }

        private static void PagingLogicAndValidationForYearAndMonth(ref int year, ref int month)
        {
            var today = DateTime.Today;
            if (year > 0 && month <= 0)
            {
                year -= 1;
                month = 12;
            }
            if (year > 0 && month >= 13)
            {
                year += 1;
                month = 1;
            }

            if (year < DateTime.MinValue.Year || year > DateTime.MaxValue.Year)
            {
                year = today.Year;
                month = today.Month;
            }
        }
    }
}