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
    public class StrojMistrsController : Controller
    {
        private readonly AppDbContext _context;

        public StrojMistrsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AbsenceTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.StrojMistrs.ToListAsync());
        }

        // GET: AbsenceTypes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var strojMistr = await _context.StrojMistrs
                .SingleOrDefaultAsync(m => m.Id == id);
            if (strojMistr == null)
            {
                return NotFound();
            }

            return View(strojMistr);
        }

        // GET: AbsenceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AbsenceTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MistrId,Name,LastName,Telefon,Color")] StrojMistr strojMistr)
        {
            strojMistr.Alias = strojMistr.LastName + " " + strojMistr.Name;
            if (ModelState.IsValid)
            {
                _context.Add(strojMistr);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(strojMistr);
        }

        // GET: AbsenceTypes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var strojMistr = await _context.StrojMistrs.SingleOrDefaultAsync(m => m.Id == id);
            if (strojMistr == null)
            {
                return NotFound();
            }
            return View(strojMistr);
        }

        // POST: AbsenceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MistrId,Name,LastName,Telefon,Color")] StrojMistr strojMistr)
        {
            if (id != strojMistr.Id)
            {
                return BadRequest();
            }
            strojMistr.Alias = strojMistr.LastName + " " + strojMistr.Name;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(strojMistr);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(strojMistr.Id))
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
            return View(strojMistr);
        }

        // GET: AbsenceTypes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var strojMistr = await _context.StrojMistrs
                .SingleOrDefaultAsync(m => m.Id == id);
            if (strojMistr == null)
            {
                return NotFound();
            }

            return View(strojMistr);
        }

        // POST: AbsenceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var strojMistr = await _context.StrojMistrs.SingleOrDefaultAsync(m => m.Id == id);
            var strojnikPouzit = StrojnikPouzit(id);
            if(!strojnikPouzit)
            {
            _context.StrojMistrs.Remove(strojMistr);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));    

            }
            ModelState.AddModelError("Alias", "Strojník nemůže být vymazán, protože je použit v kalendáři nebo ve stroji.");
            return View(strojMistr);
        }

        private bool DepartmentExists(int id)
        {
            return _context.StrojMistrs.Any(e => e.Id == id);
        }

        private bool StrojnikPouzit(int id)
        {
            bool pouzit = false;
            var pouzitStrojBooking = _context.StrojBookings.Any(e => e.StrojMistrId == id);
            var pouzitStroj = _context.Strojs.Any(e => e.MistrId == id);
            
            if(pouzitStrojBooking || pouzitStroj)
            {
                pouzit = true;
            }
            return pouzit;
        }
    }
}
