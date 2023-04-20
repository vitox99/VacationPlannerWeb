
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationPlannerWeb.DataAccess;
using VacationPlannerWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace VacationPlannerWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager,Writer")]
    public class ZakazniksController : Controller
    {
        private readonly AppDbContext _context;

        public ZakazniksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AbsenceTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Zakaznici.ToListAsync());
        }

        // GET: AbsenceTypes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var zakaznik = await _context.Zakaznici
                .SingleOrDefaultAsync(m => m.Id == id);
            if (zakaznik == null)
            {
                return NotFound();
            }

            return View(zakaznik);
        }

        // GET: AbsenceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AbsenceTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nazev,IC,Tel,Email,Poznamka")] Zakaznik zakaznik)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zakaznik);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(zakaznik);
        }

        // GET: AbsenceTypes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var zakaznik = await _context.Zakaznici.SingleOrDefaultAsync(m => m.Id == id);
            if (zakaznik == null)
            {
                return NotFound();
            }
            return View(zakaznik);
        }

        // POST: AbsenceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nazev,IC,Tel,Email,Poznamka")] Zakaznik zakaznik)
        {
            if (id != zakaznik.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zakaznik);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(zakaznik.Id))
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
            return View(zakaznik);
        }

        // GET: AbsenceTypes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var zakaznik = await _context.Zakaznici
                .SingleOrDefaultAsync(m => m.Id == id);
            if (zakaznik == null)
            {
                return NotFound();
            }

            return View(zakaznik);
        }

        // POST: AbsenceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var zakaznik = await _context.Zakaznici.SingleOrDefaultAsync(m => m.Id == id);
            _context.Zakaznici.Remove(zakaznik);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }
    }
}
