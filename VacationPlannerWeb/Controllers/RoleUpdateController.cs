using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VacationPlannerWeb.DataAccess;
using VacationPlannerWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VacationPlannerWeb.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class RoleUpdateController : Controller
    {
        private RoleManager<Role> roleManager;
        private UserManager<User> userManager;
        public RoleEdit roleEdit { get; set; }

        public RoleUpdateController(RoleManager<Role> roleMgr, UserManager<User> userMrg)
        {
            this.roleManager = roleMgr;
            this.userManager = userMrg;
        }
        public async Task<IActionResult> Index(string id)
        {
            Role role = await roleManager.FindByIdAsync(id);
            List<User> members = new List<User>();
            List<User> nonMembers = new List<User>();
            foreach (User user in userManager.Users)
            {
                var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
                list.Add(user);
            }
            roleEdit = new RoleEdit { Role = role, Members = members, NonMembers = nonMembers };
            return View(new RoleEdit
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string id, RoleModification model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.AddIds ?? new string[] { })
                {
                    User user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                foreach (string userId in model.DeleteIds ?? new string[] { })
                {
                    User user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
                //return RedirectToPage("/Role");
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction(nameof(Index));
            
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
/* 
        public async Task<IActionResult> OnPostAsync(RoleModification model)
        {
            IdentityResult result;
            if (ModelState.IsValid)
            {
                foreach (string userId in model.AddIds ?? new string[] { })
                {
                    IdentityUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.AddToRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
                foreach (string userId in model.DeleteIds ?? new string[] { })
                {
                    IdentityUser user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
                        if (!result.Succeeded)
                            Errors(result);
                    }
                }
            }

            if (ModelState.IsValid)
                return RedirectToPage("/Pozadavky/IndexRole");
            else
                return await OnGetAsync(model.RoleId);
        } */
    }
}
