
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
    public class StrojDaysController : Controller
    {
        private readonly AppDbContext _context;

        public StrojDaysController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> EditP1(int id)
        {
            var strojDays = await _context.StrojDays.SingleOrDefaultAsync(m => m.Id == id);
            if (strojDays == null)
            {
                return NotFound();
            }
            return View(strojDays);
        }

        // POST: AbsenceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditP1(int weeknumber, int year, int id, [Bind("Id,P1,P2,P3,P4,P5,P6,P7,P8, VacationDate, Weeknumber, Year")] StrojDay strojDay)
        {
            /*             if (id != strojDay.Id)
                        {
                            return BadRequest();
                        } */
            var aktualStrojDay = await _context.StrojDays.SingleOrDefaultAsync(x => x.StrojBookingId == id && x.VacationDate == strojDay.VacationDate);
            aktualStrojDay.P1 = strojDay.P1;
            aktualStrojDay.P2 = strojDay.P2;
            aktualStrojDay.P3 = strojDay.P3;
            aktualStrojDay.P4 = strojDay.P4;
            aktualStrojDay.P5 = strojDay.P5;
            aktualStrojDay.P6 = strojDay.P6;
            aktualStrojDay.P7 = strojDay.P7;
            aktualStrojDay.P8 = strojDay.P8;
            var a = weeknumber;
            if (ModelState.IsValid)
            {
                /*                 try
                                { */
                _context.Update(aktualStrojDay);
                await _context.SaveChangesAsync();
                /*                 } */
                /*                 catch (DbUpdateConcurrencyException)
                                {
                                    if (!TeamExists(strojDay.Id))
                                    {
                                        return NotFound();
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                } */
                //return RedirectToAction(nameof(Index));
            }
            return RedirectToAction("StrojManagerOverviewV2", "StrojCalendar", new { year = year, weeknumber = weeknumber });
        }
    }
}