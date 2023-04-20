using VacationPlannerWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

using System.Collections.Generic;


namespace VacationPlannerWeb.TagHelpers
{
    
    [HtmlTargetElement("td", Attributes = "i-role")]
    public class RoleUsersTH : TagHelper
    {
        //private UserManager<IdentityUser> userManager;
        
        //private RoleManager<IdentityRole> roleManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        //public RoleUsersTH(UserManager<IdentityUser> usermgr, RoleManager<IdentityRole> rolemgr)
        public RoleUsersTH(UserManager<User> usermgr, RoleManager<Role> rolemgr)
        //public RoleUsersTH(UserManager<AppUser> usermgr, RoleManager<IdentityRole> rolemgr)
        {
            _userManager = usermgr;
            _roleManager = rolemgr;
        }
 
        [HtmlAttributeName("i-role")]
        public string Role { get; set; }
 
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        //public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            List<string> names = new List<string>();
            IdentityRole role = await _roleManager.FindByIdAsync(Role);
            if (role != null)
            {
                foreach (var user in _userManager.Users)
                {
                    if (user != null && await _userManager.IsInRoleAsync(user, role.Name))
                        names.Add(user.UserName);
                }
            }
            output.Content.SetContent(names.Count == 0 ? "No Users" : string.Join(", ", names));
        }
    }


    
}  