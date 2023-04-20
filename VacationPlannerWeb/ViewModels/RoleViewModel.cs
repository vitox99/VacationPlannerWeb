using VacationPlannerWeb.Models;
using System;
using System.Collections.Generic;

namespace VacationPlannerWeb.ViewModels
{
    
    public class RoleViewModel
    {
        public IEnumerable<Role> Roles {get; set;}
        public IEnumerable<User> Users {get; set;}
        public RoleEdit roleEdit {get; set;}
    }
}