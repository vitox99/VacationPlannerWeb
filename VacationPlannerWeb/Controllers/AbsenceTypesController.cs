using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationPlannerWeb.DataAccess;
using VacationPlannerWeb.Models;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace VacationPlannerWeb.Controllers
{
    //[Authorize(Roles = "Admin,Manager")]
    public class AbsenceTypesController : Controller
    {
        private readonly AppDbContext _context;

        public AbsenceTypesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AbsenceTypes
        [Authorize]
        public async Task<IActionResult> Index()
        {
            //ViewBag.NotEditable = isNotEditable;
            var zakazky = await _context.AbsenceTypes.AsNoTracking().ToListAsync();
            var zakazkySeZakazniky = new List<Zakazka>();
            foreach(var zakazka in zakazky)
            {
                zakazka.Zakaznik = await _context.Zakaznici.AsNoTracking().SingleOrDefaultAsync(x => x.Id == zakazka.ZakaznikId);
                zakazkySeZakazniky.Add(zakazka);
            }
            return View(zakazkySeZakazniky);
            //return View(await _context.AbsenceTypes.ToListAsync());
        }

/*         public async Task<IActionResult> Index()
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
        } */

        // GET: AbsenceTypes/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var absenceType = await _context.AbsenceTypes
                .SingleOrDefaultAsync(m => m.Id == id);
            if (absenceType == null)
            {
                return NotFound();
            }

            return View(absenceType);
        }

        // GET: AbsenceTypes/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ZakaznikId"] = new SelectList(await GetZakaznikDisplayList(), "Id", "Nazev");
            return View();
        }

        // POST: AbsenceTypes/Create
        [Authorize(Roles = "Admin,Manager,Writer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id", "CisloZakazky", "ZakaznikId", "Name", "Color", "SmerPrace", "Poznamka")] Zakazka absenceType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(absenceType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(absenceType);
        }

        // GET: AbsenceTypes/Edit/5
        [Authorize(Roles = "Admin,Manager,Writer")]
        public async Task<IActionResult> Edit(int id)
        {
            var absenceType = await _context.AbsenceTypes.SingleOrDefaultAsync(m => m.Id == id);
            if (absenceType == null)
            {
                return NotFound();
            }
            ViewData["ZakaznikId"] = new SelectList(await GetZakaznikDisplayList(), "Id", "Nazev");
            return View(absenceType);
        }

        // POST: AbsenceTypes/Edit/5
        [Authorize(Roles = "Admin,Manager,Writer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id", "CisloZakazky", "ZakaznikId", "Name", "Color", "SmerPrace", "Poznamka")] Zakazka absenceType)
        {
            if (id != absenceType.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(absenceType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AbsenceTypeExists(absenceType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
                //return RedirectToAction("Index", "StrojManagerOverview");
            }
            return View(absenceType);
        }

        // GET: AbsenceTypes/Delete/5
        [Authorize(Roles = "Admin,Manager,Writer")]
        public async Task<IActionResult> Delete(int id)
        {
            var absenceType = await _context.AbsenceTypes
                .SingleOrDefaultAsync(m => m.Id == id);
            var strojBookingList = await _context.StrojBookings.Select(a => a.AbsenceTypeId).Distinct().ToListAsync();
            if (absenceType == null)
            {
                return NotFound();
            }

            return View(absenceType);
        }

        // POST: AbsenceTypes/Delete/5
        [Authorize(Roles = "Admin,Manager,Writer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var absenceType = await _context.AbsenceTypes.SingleOrDefaultAsync(m => m.Id == id);
            var strojBookingList = await _context.StrojBookings.Select(a => a.AbsenceTypeId).Distinct().ToListAsync();
            
            if(strojBookingList.Contains(absenceType.Id))
            {
                ViewData["ZakPouzita"] = "true";
                return View(absenceType);
            }

            _context.AbsenceTypes.Remove(absenceType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AbsenceTypeExists(int id)
        {
            return _context.AbsenceTypes.Any(e => e.Id == id);
        }

        private async Task<List<Zakaznik>> GetZakaznikDisplayList()
        {
            var zakaznikList = await _context.Zakaznici.ToListAsync();
            zakaznikList.Add(new Zakaznik() { Nazev = "< None >" });
            return zakaznikList.OrderBy(d => d.Id).ToList();
        }
    }
}
