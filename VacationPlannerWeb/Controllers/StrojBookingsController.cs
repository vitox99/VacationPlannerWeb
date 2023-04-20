using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VacationPlannerWeb.DataAccess;
using VacationPlannerWeb.Models;

namespace VacationPlannerWeb.Controllers
{
    [Authorize]
    public class StrojBookingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public StrojBookingsController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: StrojBookings
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            if (user == null)
            {
                return NotFound($"Aktuální uživatel nebyl nalezen v databázi.");
            }

            List<StrojBooking> vacBookingList;
            if (await HasRolesAsync(user, "Admin"))
            {
                vacBookingList = await GetAllVacationBookings();
            }
            else if (await HasRolesAsync(user, "Manager"))
            {
                //vacBookingList = GetVacationBookingsByManager(user); //zobrazení strojů podle role přihlášeného
                vacBookingList = await GetAllVacationBookings();
            }
            else
            {
                //vacBookingList = await GetVacationBookingsByUser(user);
                vacBookingList = await GetAllVacationBookings();
            }

            return View(vacBookingList);
        }

        private Task<User> GetCurrentUser()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private async Task<Stroj> GetCurrentStroj(int? strojId)
        {
            return await _context.Strojs.FindAsync(strojId);
        }

/*         private async Task<StrojMistr> GetCurrentStrojMistr(int? strojId)
        {
            return await _context.StrojMistrs.FindAsync(strojId);
        } */
        private async Task<StrojMistr> GetCurrentStrojMistr(int? strojId)
        {
            var strojnik = (from s in _context.Strojs
            join r in _context.StrojMistrs on s.MistrId equals r.Id
            where s.Id == strojId
            select r).FirstOrDefault();
            if(strojnik == null)
            {
                strojnik = _context.StrojMistrs.Where(x => x.Id == 8).FirstOrDefault();
            }
            return strojnik;
        }

        private async Task<User> GetOldUser(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> TodoList()
        {
            var user = await GetCurrentUser();
            var vacBookingList = await _context.StrojBookings.AsNoTracking().Include(v => v.AbsenceType).Include(v => v.User)
                .Where(v => v.Approval == ApprovalState.Pending.ToString())
                .OrderBy(x => x.FromDate).ToListAsync();

            var result = vacBookingList.Where(v => IsManagerForBookingUser(v, user)).ToList();

            return View(result);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> TodoListApproval(int vacationBookingId, string approvalState)
        {
            var strojBooking = await _context.StrojBookings
                .Include(x => x.AbsenceType).Include(u => u.User)
                .SingleOrDefaultAsync(x => x.Id == vacationBookingId);
            if (strojBooking == null)
            {
                return NotFound();
            }
            if (approvalState != ApprovalState.Approved.ToString() && approvalState != ApprovalState.Denied.ToString())
            {
                return BadRequest();
            }
            strojBooking.Approval = approvalState;
            var hasChanges = _context.ChangeTracker.HasChanges();
            try
            {
                _context.Update(strojBooking);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return RedirectToAction(nameof(TodoList));
        }

        private async Task<List<StrojBooking>> GetVacationBookingsByUser(User user)
        {
            return await _context.StrojBookings.Include(v => v.User).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .Where(v => v.UserId == user.Id).ToListAsync();
        }

        private async Task<List<StrojBooking>> GetVacationBookingsByStroj(Stroj stroj)
        {
            return await _context.StrojBookings.Include(v => v.Stroj).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .Where(v => v.StrojId == stroj.Id).ToListAsync();
        }

        private async Task<List<StrojBooking>> GetVacationBookingsNoTrackingByUserId(string userId)
        {
            return await _context.StrojBookings.AsNoTracking().Include(v => v.User).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .Where(v => v.UserId == userId).ToListAsync();
        }

        private async Task<List<StrojBooking>> GetVacationBookingsNoTrackingByStrojId(int? strojId)
        {
            return await _context.StrojBookings.AsNoTracking().Include(v => v.Stroj).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .Where(v => v.StrojId.Equals(strojId)).ToListAsync();
        }

        private async Task<List<StrojBooking>> GetAllVacationBookings()
        {
            return await _context.StrojBookings.Include(v => v.User).Include(v => v.VacationDays).Include(v => v.AbsenceType).Include(v => v.Stroj).Include(v => v.StrojMistr)
                .ToListAsync();
        }

        private List<StrojBooking> GetVacationBookingsByManager(User userManager)
        {
            var vacationBookingList = new List<StrojBooking>();
            var allBookings = _context.StrojBookings.Include(v => v.VacationDays).Include(v => v.AbsenceType).Include(v => v.User).ToList();
            vacationBookingList.AddRange(allBookings.Where(v => v.UserId == userManager.Id));
            vacationBookingList.AddRange(allBookings.Where(v => IsManagerForBookingUser(v, userManager)));
            
            return vacationBookingList.Distinct().ToList();
        }

        // GET: StrojBookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var strojBooking = await _context.StrojBookings
                .Include(v => v.User).Include(v => v.VacationDays).Include(v => v.AbsenceType).Include(v => v.Stroj).Include(v => v.StrojMistr)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (strojBooking == null)
            {
                return NotFound();
            }

            strojBooking.VacationDays = strojBooking.VacationDays.OrderBy(x => x.VacationDate).ToList();

            var user = await GetCurrentUser();
            var stroj = await GetCurrentStroj(strojBooking.StrojId);
            
/*             if (!await HasRolesAsync(user, "Admin,Manager") && ! IsManagerForBookingUser(strojBooking, user))
            {
                if (!IsOwner(strojBooking, user))
                {
                    return View("AccessDenied");
                }
            } */

            return View(strojBooking);
        }

        private static bool IsOwner(StrojBooking strojBooking, User user)
        {
            return strojBooking.UserId == user.Id;
        }

        private bool IsManagerForBookingUser(StrojBooking strojBooking, User user)
        {
            return strojBooking.User.ManagerUserId == user.Id;
        }

        //Todo: Add both start and end-date as in parameters
        [Authorize(Roles = "Admin,Manager,Writer")]
        public async Task<IActionResult> Create(int? id, string date)
        {
            var vacBooking = new StrojBooking();
            if(!String.IsNullOrEmpty(id.ToString()))
            {
                vacBooking.StrojId = id.Value;
            }
            if (!string.IsNullOrWhiteSpace(date))
            {
                vacBooking.FromDate = DateTime.Parse(date);
                vacBooking.ToDate = DateTime.Parse(date);
            }
            else
            {
                vacBooking.FromDate = DateTime.Today;
                vacBooking.ToDate = DateTime.Today;
            }
            ViewData["AbsenceTypes"] = new SelectList(await GetAbsenceTypes(), nameof(Zakazka.Id), nameof(Zakazka.Name));
            ViewData["StrojTypes"] = new SelectList(await GetStrojTypes(), nameof(Stroj.Id), nameof(Stroj.Name));
            //ViewData["StrojMistrTypes"] = new SelectList(await GetStrojMistrTypes(), nameof(StrojMistr.Id), nameof(StrojMistr.Alias));
            //ViewData["StrojMistrCurrent"] = new SelectList(await GetStrojMistrCurrent(), nameof(StrojMistr.Id), nameof(StrojMistr.Alias));
            return View(vacBooking);
        }

        // POST: StrojBookings/Create
        [Authorize(Roles = "Admin,Manager,Writer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StrojId,FromDate,ToDate,SmerPrace,Comment,AbsenceTypeId")] StrojBooking strojBooking)
        {
            var absenceType = await GetAbsenceTypeById(strojBooking.AbsenceTypeId);
            if (absenceType == null)
            {
                AddInvalidAbsenceTypeError();
            }

            if (ModelState.IsValid)
            {
                strojBooking.Approval = ApprovalState.Pending.ToString();

                bool isErrors = false;

                var user = await GetCurrentUser();
                var stroj = await GetCurrentStroj(strojBooking.StrojId);
                var strojnik = await GetCurrentStrojMistr(strojBooking.StrojId);

                var holidayList = _context.WorkFreeDays.AsNoTracking().Select(x => x.Date).ToList();
                var userVacbookings = await GetVacationBookingsByUser(user);
                var strojVacbookings = await GetVacationBookingsByStroj(stroj);

                var userVacDates = GetVacationDatesFromBookings(userVacbookings);
                var strojVacDates = GetVacationDatesFromBookingsStroj(strojVacbookings);

                GenerateVacationDaysListFromBooking(strojBooking, holidayList, userVacDates,
                    out List<DateTime> doubleBookingDatesList, out List<StrojDay> vacdayList);

                GenerateStrojDaysListFromBooking(strojBooking, holidayList, strojVacDates,
                    out List<DateTime> doubleBookingDatesListStroj, out List<StrojDay> vacdayListStroj);

                isErrors = ValidateVacationDaysList(vacdayListStroj, isErrors);
                isErrors = ValidateDoubleBookingDatesList(doubleBookingDatesListStroj, isErrors);

                strojBooking.VacationDays = vacdayListStroj;
                strojBooking.UserId = user.Id;
                strojBooking.User = user;
                strojBooking.AbsenceType = absenceType;
                strojBooking.StrojId = stroj.Id;
                strojBooking.Stroj = stroj;
                strojBooking.StrojMistrId = strojnik.Id;
                //strojBooking.StrojMistrId = strojnik;

                if (!isErrors)
                {
                    _context.Add(strojBooking);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

            }
            ViewData["AbsenceTypes"] = new SelectList(await GetAbsenceTypes(), nameof(Zakazka.Id), nameof(Zakazka.Name));
            ViewData["StrojTypes"] = new SelectList(await GetStrojTypes(), nameof(Stroj.Id), nameof(Stroj.Name));
            return View(strojBooking);
        }

        private void AddInvalidAbsenceTypeError()
        {
            ModelState.AddModelError(nameof(StrojBooking.AbsenceTypeId), "Invalid AbscenceType");
        }

        private async Task<Zakazka> GetAbsenceTypeById(int? absenceTypeId)
        {
            return await _context.AbsenceTypes.FindAsync(absenceTypeId);
        }

        [Authorize(Roles = "Admin,Manager,Writer")]
        // GET: StrojBookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var strojBooking = await _context.StrojBookings
                .Include(v => v.User).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (strojBooking == null)
            {
                return NotFound();
            }

            var user = await GetCurrentUser();
            var stroj = await GetCurrentStroj(strojBooking.StrojId);
            bool isNotEditable = false;

            if ((!await HasRolesAsync(user, "Admin,Manager") && !IsManagerForBookingUser(strojBooking, user)))
            {
/*                 if (!IsOwner(strojBooking, user))
                {
                    return View("AccessDenied");
                } */

                if (ApprovalIsNotPending(strojBooking))
                {
/*                     isNotEditable = true;
                    ViewBag.NotEditableMessage = "Nemůžete editovat událost se statusem Approved nebo Denied. " +
                        "\nProsím, smažte existující událost a vytvořte novou."; */
                }
            }

            ViewBag.NotEditable = isNotEditable;
            ViewData["AbsenceTypes"] = new SelectList(await GetAbsenceTypes(), nameof(Zakazka.Id), nameof(Zakazka.Name), strojBooking.AbsenceTypeId);
            ViewData["ApprovalStates"] = new SelectList(await GetApprovalStatesForUser(strojBooking, user), "Value", "Value", strojBooking.Approval);
            ViewData["UserId"] = new SelectList(await _context.Users.Where(x => x.Id == strojBooking.UserId).ToListAsync(), "Id", "DisplayName", strojBooking.UserId);
            ViewData["StrojTypes"] = new SelectList(await GetStrojTypes(), nameof(Stroj.Id), nameof(Stroj.Name));
            ViewData["StrojMistrTypes"] = new SelectList(await GetStrojMistrTypes(), nameof(StrojMistr.Id), nameof(StrojMistr.Alias));
            
            return View(strojBooking);
        }

        private async Task<List<Zakazka>> GetAbsenceTypes()
        {
            return await _context.AbsenceTypes.AsNoTracking().ToListAsync();
        }

        private async Task<List<Stroj>> GetStrojTypes()
        {
            return await _context.Strojs.AsNoTracking().ToListAsync();
        }
        private async Task<List<StrojMistr>> GetStrojMistrTypes()
        {
            return await _context.StrojMistrs.AsNoTracking().ToListAsync();
        }

/*         private async Task<List<StrojMistr>> GetStrojMistrTypes()
        {
            return await _context.StrojMistrs.AsNoTracking().ToListAsync();
        } */

/*         private async Task<List<StrojMistr>> GetStrojMistrCurrent()
        {
            return await _context.StrojMistrs.Select(x => x.MistrId = ).AsNoTracking().ToListAsync();
        } */
        ///<summary>
        ///Returns list of all valid approval states for the specified user.
        ///</summary>
        private async Task<IEnumerable<object>> GetApprovalStatesForUser(StrojBooking strojBooking, User user)
        {
            var apporalStateList = (ApprovalState[])Enum.GetValues(typeof(ApprovalState));
            return await HasRolesAsync(user, "Admin") || IsManagerForBookingUser(strojBooking, user)
                ? apporalStateList.Select(x => new { Value = x.ToString() })
                : apporalStateList.Select(x => new { Value = x.ToString() }).Where(a => a.Value == strojBooking.Approval);
        }



        // POST: StrojBookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,StrojId,FromDate,ToDate,Approval,SmerPrace,Comment,AbsenceTypeId,StrojMistrId")] StrojBooking strojBooking)
        {
            if (id != strojBooking.Id)
            {
                return NotFound();
            }

            var vacbookingReadOnly = await _context.StrojBookings.AsNoTracking()
                .Include(v => v.User).Include(v => v.VacationDays).Include(v => v.AbsenceType)
                .SingleOrDefaultAsync(v => v.Id == id);

            var strojbookingReadOnly = await _context.StrojBookings.AsNoTracking()
                .Include(v => v.Stroj).Include(v => v.VacationDays).Include(v => v.AbsenceType).Include(v => v.User)
                .SingleOrDefaultAsync(v => v.Id == id);    

/*             if (vacbookingReadOnly.UserId != strojBooking.UserId)
            {
                return NotFound();
            }
 */
            if (strojbookingReadOnly.StrojId != strojBooking.StrojId)
            {
                return NotFound();
            }


            var user = await GetCurrentUser();
            var stroj = await GetCurrentStroj(strojBooking.StrojId);
            var oldUser = await GetOldUser(strojBooking.UserId);

            bool isNotEditable = false;

            if (!await HasRolesAsync(user, "Admin") && !IsManagerForBookingUser(vacbookingReadOnly, user))
            {
/*                 if (ApprovalIsNotPending(vacbookingReadOnly))
                {
                    isNotEditable = true;
                    ViewBag.NotEditableMessage = "Nemůžete změnit událost se statusem Approved nebo Denied. " +
                        "\nProsím, smažte existující událost a vytvořte novou.";
                    ModelState.AddModelError("UserId", "Nemůžete změnit událost se statusem Approved nebo Denied.");
                } */

                ValidateVacationBookingChanges(strojBooking, vacbookingReadOnly);
            }

            var absenceType = await GetAbsenceTypeById(strojBooking.AbsenceTypeId);
            if (absenceType == null)
            {
                AddInvalidAbsenceTypeError();
            }

            if (ModelState.IsValid)
            {
                bool isErrors = false;

                var userVacbookings = await GetVacationBookingsNoTrackingByUserId(strojBooking.UserId);
                var strojVacbookings = await GetVacationBookingsNoTrackingByStrojId(strojBooking.StrojId);//seznam všech dovolených stroje
                var userVacDates = GetVacationDatesFromBookings(userVacbookings);
                var strojVacDates = GetVacationDatesFromBookingsStroj(strojVacbookings);//seznam všech dnů ze strojDays
                var holidayList = await _context.WorkFreeDays.Select(x => x.Date).ToListAsync();
                //var previousDates = vacbookingReadOnly.VacationDays.Select(x => x.VacationDate).ToList();
                var previousDates = strojbookingReadOnly.VacationDays.Select(x => x.VacationDate).ToList();//asi poslední den ve strojDays

                GenerateVacationDaysListFromBooking(strojBooking, holidayList, userVacDates, previousDates,
                    out List<DateTime> doubleBookingDatesList, out List<StrojDay> vacdayList);

                GenerateStrojDaysListFromBooking(strojBooking, holidayList, strojVacDates, previousDates,
                    out List<DateTime> doubleBookingDatesListStroj, out List<StrojDay> vacdayListStroj);


                isErrors = ValidateVacationDaysList(vacdayListStroj, isErrors);
                isErrors = ValidateDoubleBookingDatesList(doubleBookingDatesListStroj, isErrors);

                strojBooking.VacationDays = vacdayListStroj;
                //strojBooking.User = vacbookingReadOnly.User;
                strojBooking.User = oldUser;
                strojBooking.AbsenceType = absenceType;
                strojBooking.StrojId = stroj.Id;
                strojBooking.Stroj = stroj;
                

                if (!isErrors)
                {
                    var oldVacationDays = _context.StrojDays.Where(v => v.StrojBookingId == id);
                    try
                    {
                        _context.RemoveRange(oldVacationDays);
                        _context.Update(strojBooking);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!VacationBookingExists(strojBooking.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }

                    //return RedirectToAction(nameof(Index));
                    return RedirectToAction("Index");
                }
            }

            if (strojBooking.User == null)
            {
                strojBooking.User = vacbookingReadOnly.User;
            }
            ViewBag.NotEditable = isNotEditable;
/*             ViewData["StrojTypes"] = new SelectList(await GetStrojTypes(), nameof(Stroj.Id), nameof(Stroj.Name));
            ViewData["AbsenceTypes"] = new SelectList(await GetAbsenceTypes(), nameof(Zakazka.Id), nameof(Zakazka.Name), strojBooking.AbsenceTypeId);
            ViewData["ApprovalStates"] = new SelectList(await GetApprovalStatesForUser(strojBooking, user), "Value", "Value", strojBooking.Approval);
            ViewData["UserId"] = new SelectList(await _context.Users.Where(x => x.Id == strojBooking.UserId).ToListAsync(), "Id", "DisplayName", strojBooking.UserId); */
            return View(strojBooking);
        }

        private static List<DateTime> GetVacationDatesFromBookings(List<StrojBooking> userVacbookings)
        {
            return userVacbookings.SelectMany(x => x.VacationDays.Select(d => d.VacationDate)).ToList();
        }

        private static List<DateTime> GetVacationDatesFromBookingsStroj(List<StrojBooking> strojVacbookings)
        {
            return strojVacbookings.SelectMany(x => x.VacationDays.Select(d => d.VacationDate)).ToList();
        }

        private static void GenerateVacationDaysListFromBooking(StrojBooking strojBooking, List<DateTime> holidayList, List<DateTime> userVacDates,
            out List<DateTime> doubleBookingDatesList, out List<StrojDay> vacdayList)
        {
            doubleBookingDatesList = new List<DateTime>();
            vacdayList = new List<StrojDay>();
            for (DateTime d = strojBooking.FromDate; d <= strojBooking.ToDate; d = d.AddDays(1))
            {
                var vacday = new StrojDay()
                {
                    Id = 0,
                    VacationDate = d,
                    StrojBookingId = strojBooking.Id,
                };
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    //continue;
                }
                if (holidayList.Contains(d))
                {
                    continue;
                }
                if (userVacDates.Contains(d))
                {
                    doubleBookingDatesList.Add(d);
                    continue;
                }
                vacdayList.Add(vacday);
            }
        }

        private static void GenerateStrojDaysListFromBooking(StrojBooking strojBooking, List<DateTime> holidayList, List<DateTime> strojVacDates,
            out List<DateTime> doubleBookingDatesListStroj, out List<StrojDay> vacdayListStroj)
        {
            doubleBookingDatesListStroj = new List<DateTime>();
            vacdayListStroj = new List<StrojDay>();
            for (DateTime d = strojBooking.FromDate; d <= strojBooking.ToDate; d = d.AddDays(1))
            {
                var vacday = new StrojDay()
                {
                    Id = 0,
                    VacationDate = d,
                    StrojBookingId = strojBooking.Id,
                };
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                //    continue;
                }
                if (holidayList.Contains(d))
                {
                    continue;
                }
                if (strojVacDates.Contains(d))
                {
                    doubleBookingDatesListStroj.Add(d);
                    continue;
                }
                vacdayListStroj.Add(vacday);
            }
        }

        private static void GenerateVacationDaysListFromBooking(StrojBooking strojBooking, List<DateTime> holidayList, List<DateTime> userVacDates, List<DateTime> previousDates,
            out List<DateTime> doubleBookingDatesList, out List<StrojDay> vacdayList)
        {
            doubleBookingDatesList = new List<DateTime>();
            vacdayList = new List<StrojDay>();
            for (DateTime d = strojBooking.FromDate; d <= strojBooking.ToDate; d = d.AddDays(1))
            {
                var vacday = new StrojDay()
                {
                    Id = 0,
                    VacationDate = d,
                    StrojBookingId = strojBooking.Id,
                };
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    //continue;
                }
                if (holidayList.Contains(d))
                {
                    continue;
                }
                if (previousDates.Contains(d))
                {
                    vacdayList.Add(vacday);
                    continue;
                }
                if (userVacDates.Contains(d))
                {
                    doubleBookingDatesList.Add(d);
                    continue;
                }
                vacdayList.Add(vacday);
            }
        }

        private static void GenerateStrojDaysListFromBooking(StrojBooking strojBooking, List<DateTime> holidayList, List<DateTime> strojVacDates, List<DateTime> previousDates,
            out List<DateTime> doubleBookingDatesListStroj, out List<StrojDay> vacdayListStroj)
        {
            doubleBookingDatesListStroj = new List<DateTime>();
            vacdayListStroj = new List<StrojDay>();
            for (DateTime d = strojBooking.FromDate; d <= strojBooking.ToDate; d = d.AddDays(1))
            {
                var vacday = new StrojDay()
                {
                    Id = 0,
                    VacationDate = d,
                    StrojBookingId = strojBooking.Id,
                };
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    //continue;
                }
                if (holidayList.Contains(d))
                {
                    continue;
                }
                if (previousDates.Contains(d))
                {
                    vacdayListStroj.Add(vacday);
                    continue;
                }
                if (strojVacDates.Contains(d))
                {
                    doubleBookingDatesListStroj.Add(d);
                    continue;
                }
                vacdayListStroj.Add(vacday);
            }
        }

        private bool ValidateDoubleBookingDatesList(List<DateTime> doubleBookingDatesList, bool isErrors)
        {
            if (doubleBookingDatesList.Count > 0)
            {
                var displayDatesString = DatesToDisplayString(doubleBookingDatesList);
                ModelState.AddModelError("FromDate", $"Pro tato data již máte vytvořenou událost: {displayDatesString}");
                ModelState.AddModelError("ToDate", $"Pro tato data již máte vytvořenou událost: {displayDatesString}");
                isErrors = true;
            }
            return isErrors;
        }

        private bool ValidateVacationDaysList(List<StrojDay> vacdayList, bool isErrors)
        {
            if (vacdayList.Count <= 0)
            {
                ModelState.AddModelError("FromDate", "Není možné vytvořit událost o délce 0 dnů.");
                ModelState.AddModelError("ToDate", "Není možné vytvořit událost o délce 0 dnů.");
                isErrors = true;
            }
            return isErrors;
        }

        ///<summary>
        ///Returns a string with all dates included in the list, formated and separated with ",".
        ///</summary>
        private static string DatesToDisplayString(List<DateTime> datesList)
        {
            var listOfDates = datesList.Select(x => x.Date.ToString("yyyy-MM-dd"));
            var displayDatesString = string.Join(", ", listOfDates);
            return displayDatesString;
        }

        private static void ValidateVacationBookingChanges(StrojBooking strojBooking, StrojBooking vacbookingReadOnly)
        {
            strojBooking.Approval = vacbookingReadOnly.Approval;
            strojBooking.UserId = vacbookingReadOnly.UserId;
        }

        //private static bool VacBookingApprovalHasChanged(StrojBooking strojBooking, StrojBooking vacbookingReadOnly)
        //{
        //    return vacbookingReadOnly.Approval != strojBooking.Approval;
        //}

        ///<summary>
        ///Returns the result if the user has a role included in the string separated with ",".
        ///</summary>
        private async Task<bool> HasRolesAsync(User user, string rolesString)
        {
            var rolesArray = rolesString.Split(',');
            foreach (var role in rolesArray)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ApprovalIsNotPending(StrojBooking vacbookingReadOnly)
        {
            return vacbookingReadOnly.Approval != ApprovalState.Pending.ToString();
        }

        // GET: StrojBookings/Delete/5
        [Authorize(Roles = "Admin,Manager,Writer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var strojBooking = await _context.StrojBookings
                .Include(v => v.User).Include(x => x.AbsenceType).Include(x => x.AbsenceType).Include(v => v.Stroj)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (strojBooking == null)
            {
                return NotFound();
            }

            var user = await GetCurrentUser();

            if (!await HasRolesAsync(user, "Admin,Manager") && !IsManagerForBookingUser(strojBooking, user))
            {
                if (!IsOwner(strojBooking, user))
                {
                    return View("AccessDenied");
                }

                if (strojBooking.Approval == ApprovalState.Approved.ToString() && strojBooking.FromDate < DateTime.Today)
                {
                    return BadRequest("Událost se statusem Approved není možné odstranit, pokud již začala.");
                }
            }

            return View(strojBooking);
        }

        // POST: StrojBookings/Delete/5,        
        [Authorize(Roles = "Admin,Manager,Writer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var strojBooking = await _context.StrojBookings.Include(v => v.VacationDays).SingleOrDefaultAsync(m => m.Id == id);
            _context.StrojBookings.Remove(strojBooking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VacationBookingExists(int id)
        {
            return _context.StrojBookings.Any(e => e.Id == id);
        }
    }
}