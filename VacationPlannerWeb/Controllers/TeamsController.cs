
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationPlannerWeb.DataAccess;
using VacationPlannerWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace VacationPlannerWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class TeamsController : Controller
    {
        private readonly AppDbContext _context;

        public TeamsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AbsenceTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Teams.ToListAsync());
        }

        // GET: AbsenceTypes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.Teams
                .SingleOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // GET: AbsenceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AbsenceTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Team team)
        {
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }

        // GET: AbsenceTypes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: AbsenceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Shortening")] Team team)
        {
            if (id != team.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.Id))
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
            return View(team);
        }

        // GET: AbsenceTypes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var team = await _context.Teams
                .SingleOrDefaultAsync(m => m.Id == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: AbsenceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.SingleOrDefaultAsync(m => m.Id == id);

            var teamBylPouzit = teamPouzit(id);
            if(!teamBylPouzit)
            {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("Name", "Skupina strojů nemůže být vymazána, protože je použita ve stroji.");
            return View(team);
/*             _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); */
        }

        public bool teamPouzit(int id)
        {
            //bool pouzit = false;
            //var pouzitStrojBooking = _context.StrojBookings.Any(e => e.StrojId == id);

/*             if(pouzitStrojBooking)
            {
                pouzit = true;
            } */
            return _context.Strojs.Any(e => e.TeamId == id);
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }
    }
}
