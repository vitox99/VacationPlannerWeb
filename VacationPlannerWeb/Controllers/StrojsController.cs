using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationPlannerWeb.DataAccess;
using VacationPlannerWeb.Models;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;


namespace VacationPlannerWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class StrojsController : Controller
    {
        private readonly AppDbContext _context;

        public StrojsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AbsenceTypes
        public async Task<IActionResult> Index()
        {
            var allStrojs = await _context.Strojs.AsNoTracking().ToListAsync();
            var allStrojsWithTeamsAndDepartments = new List<Stroj>();
            foreach(var stroj in allStrojs)
            {
                stroj.Team = await _context.Teams.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.TeamId);
                stroj.Department = await _context.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.DepartmentId);
                stroj.StrojMistr = await _context.StrojMistrs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.MistrId);
                allStrojsWithTeamsAndDepartments.Add(stroj);
            }

            return View(allStrojsWithTeamsAndDepartments);
        }


/*                 public async Task<IActionResult> Index(bool showHidden = false)
        {
            var allUsers = await _context.Users.AsNoTracking().Where(x => x.IsHidden == showHidden).ToListAsync();
            var allUsersWithTeamsAndDepartment = new List<User>();
            foreach (var user in allUsers)
            {
                user.Team = await _context.Teams.AsNoTracking().SingleOrDefaultAsync(x => x.Id == user.TeamId);
                user.Department = await _context.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == user.DepartmentId);
                allUsersWithTeamsAndDepartment.Add(user);
            }

            var accountViewModel = new AccountViewModel()
            {
                Users = allUsersWithTeamsAndDepartment,
                ShowHidden = !showHidden,
            };

            return View(accountViewModel);
        }
 */


        // GET: AbsenceTypes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var stroj = await _context.Strojs
                .SingleOrDefaultAsync(m => m.Id == id);
            if (stroj == null)
            {
                return NotFound();
            }
            stroj.Team = await _context.Teams.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.TeamId);
            stroj.Department = await _context.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.DepartmentId);
            stroj.StrojMistr = await _context.StrojMistrs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.MistrId);
            return View(stroj);
        }

        // GET: AbsenceTypes/Create
        public async Task<IActionResult> Create()
        {
            ViewData["TeamId"] = new SelectList(await GetTeamDisplayList(), "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(await GetDepartmentDisplayList(), "Id", "Name");
            ViewData["MistrId"] = new SelectList(await GetMistrDisplayList(), "Id", "Alias");
            return View();
        }

        // POST: AbsenceTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvNr,Name,Popis,TeamId,DepartmentId,IsHidden,MistrId,StrojColor")] Stroj stroj)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stroj);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //ViewBag.NotEditable = isNotEditable;
            ViewData["TeamId"] = new SelectList(await GetTeamDisplayList(), "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(await GetDepartmentDisplayList(), "Id", "Name");
            ViewData["MistrId"] = new SelectList(await GetMistrDisplayList(), "Id", "Name");
            //ViewData["AbsenceTypes"] = new SelectList(await GetAbsenceTypes(), nameof(AbsenceType.Id), nameof(AbsenceType.Name), vacationBooking.AbsenceTypeId);
            //ViewData["ApprovalStates"] = new SelectList(await GetApprovalStatesForUser(vacationBooking, user), "Value", "Value", vacationBooking.Approval);
            //ViewData["UserId"] = new SelectList(await _context.Users.Where(x => x.Id == vacationBooking.UserId).ToListAsync(), "Id", "DisplayName", vacationBooking.UserId);
            return View(stroj);
        }
        private async Task<List<Department>> GetDepartmentDisplayList()
        {
            var depList = await _context.Departments.ToListAsync();
            depList.Add(new Department() { Name = "< None >" });
            return depList.OrderBy(d => d.Id).ToList();
        }
        
        private async Task<List<Team>> GetTeamDisplayList()
        {
            var teamList = await _context.Teams.ToListAsync();
            teamList.Add(new Team() { Name = "< None >" });
            return teamList.OrderBy(d => d.Id).ToList();
        }

        private async Task<List<StrojMistr>> GetMistrDisplayList()
        {
            var mistrList = await _context.StrojMistrs.ToListAsync();
            mistrList.Add(new StrojMistr() { Alias = "< None >" });
            return mistrList.OrderByDescending(d => d.Id).ToList();
        }

        // GET: AbsenceTypes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var stroje = await _context.Strojs.SingleOrDefaultAsync(m => m.Id == id);
            if (stroje == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(await GetDepartmentDisplayList(), "Id", "Name");
            ViewData["TeamId"] = new SelectList(await GetTeamDisplayList(), "Id", "Name");
            ViewData["MistrId"] = new SelectList(await GetMistrDisplayList(), "Id", "Alias");
            return View(stroje);
        }

        // POST: AbsenceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InvNr,Name,Popis,TeamId,DepartmentId,IsHidden,MistrId,StrojColor")] Stroj stroj)
        {
            if (id != stroj.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stroj);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StrojExists(stroj.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamId"] = new SelectList(await GetTeamDisplayList(), "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(await GetDepartmentDisplayList(), "Id", "Name");
            ViewData["MistrId"] = new SelectList(await GetMistrDisplayList(), "Id", "Name");
            return View(stroj);
        }

        // GET: AbsenceTypes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var stroj = await _context.Strojs
                .SingleOrDefaultAsync(m => m.Id == id);
            if (stroj == null)
            {
                return NotFound();
            }
            stroj.Team = await _context.Teams.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.TeamId);
            stroj.Department = await _context.Departments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.DepartmentId);
            stroj.StrojMistr = await _context.StrojMistrs.AsNoTracking().SingleOrDefaultAsync(x => x.Id == stroj.MistrId);
            return View(stroj);
        }

        // POST: AbsenceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stroj = await _context.Strojs.SingleOrDefaultAsync(m => m.Id == id);
            var strojBylPouzit = strojPouzit(id);
            if(!strojBylPouzit)
            {
            _context.Strojs.Remove(stroj);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("Name", "Stroj nemůže být vymazán, protože je použit v kalendáři.");
            return View(stroj);
        }

        public bool strojPouzit(int id)
        {
            //bool pouzit = false;
            var pouzitStrojBooking = _context.StrojBookings.Any(e => e.StrojId == id);

/*             if(pouzitStrojBooking)
            {
                pouzit = true;
            } */
            return pouzitStrojBooking;
        }

        private bool StrojExists(int id)
        {
            return _context.Strojs.Any(e => e.Id == id);
        }
    }
}
