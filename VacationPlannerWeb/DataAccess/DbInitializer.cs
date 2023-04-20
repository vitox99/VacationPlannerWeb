using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using VacationPlannerWeb.Models;
using Microsoft.EntityFrameworkCore;


namespace VacationPlannerWeb.DataAccess
{
    public class DbInitializer
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;

        public DbInitializer(IServiceProvider services)
        {
            _context = services.GetRequiredService<AppDbContext>();
            _roleManager = services.GetRequiredService<RoleManager<Role>>();
            _userManager = services.GetRequiredService<UserManager<User>>();
        }

        public void Initialize()
        {
            _context.Database.EnsureCreated();
            
            
            //_context.Database.Migrate();

            if (_context.Users.Any())
            {
                return;
            }

            ClearDatabase();
            CreateAdminRole();
            SeedDatabase();
            StrojSeedDb();
        }

        private void CreateAdminRole()
        {
            bool roleExists = _roleManager.RoleExistsAsync("Admin").Result;
            if (roleExists)
            {
                return;
            }

            var adminRole = new Role()
            {
                Name = "Admin",
                Shortening = "Admin"
            };

            _roleManager.CreateAsync(adminRole).Wait();

            var user = new User()
            {
                FirstName = "Admin",
                LastName = "Admin",
                DisplayName = "Admin",
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com"
            };

            string adminPassword = "Password123";
            var userResult = _userManager.CreateAsync(user, adminPassword).Result;

            if (userResult.Succeeded)
            {
                _userManager.AddToRoleAsync(user, "Admin").Wait();
            }
        }

        private void SeedDatabase()
        {
//vytvoření nultého teamu
            var team0 = new Team { Name = "nepřiřazeno", Shortening = "N/A"};
/*             
            var team1 = new Team { Name = "Pandas", Shortening = "Pand" };
            var team2 = new Team { Name = "Tigers", Shortening = "Tigr" };
            var team3 = new Team { Name = "Rabbits", Shortening = "Rabt" };
 */
            var teams = new List<Team>()
            {
                team0//team1, team2, team3
            };
            _context.Teams.AddRange(teams);

//vytvoření nultého oddělení
            var dep0 = new Department { Name = "nepřiřazeno", Shortening = "N/A" };
            var deps = new List<Department>()
            {
                dep0//dep1, dep2, dep3
            };
            _context.Departments.AddRange(deps);

//vytvoření nultého strojníka
            var strojMistr = new StrojMistr { MistrId = "0", Color = "#ffffff", Alias = "nepřiřazeno"};
            _context.StrojMistrs.AddRange(strojMistr);

//vytvoření nultého zákazníka            
            var zak0 = new Zakaznik { Nazev = "Zákazník0", IC = "12345678" , Tel = "+420737737737", Email = "zakaznik@zakaznik.cz", Poznamka = "Poznámka" };
            var zakaznici = new List<Zakaznik>()
            {
                zak0
            };
            _context.Zakaznici.AddRange(zakaznici);


            bool roleExists = _roleManager.RoleExistsAsync("Manager").Result;
            if (!roleExists)
            {
                var managerRole = new Role() {Name = "Manager",Shortening = "Mangr"};
                var writerRole = new Role() {Name = "Writer", Shortening = "Wr"};
                var readerRole = new Role() {Name = "Reader", Shortening = "Rd"};

                var roles = new List<Role>() {managerRole, writerRole, readerRole};

                foreach(var role in roles)
                {
                    _roleManager.CreateAsync(role).Wait();
                }
                //_roleManager.CreateAsync(managerRole).Wait();
            }

            const string userPassword = "Password123";

            var managerUser = new User { UserName = "manager@gmail.com", Email = "manager@gmail.com", FirstName = "Mike", LastName = "Manager", DisplayName = "Mike Manager", TeamId = team0.Id, DepartmentId = dep0.Id, };
            _userManager.CreateAsync(managerUser, userPassword).Wait();

            var user0 = new User { UserName = "spacil@tssas.cz", Email = "spacil@tssas.cz", FirstName = "Petr", LastName = "Spáčil", DisplayName = "Petr Spáčil", TeamId = team0.Id, DepartmentId = dep0.Id, ManagerUserId = managerUser.Id };

            /* var user1 = new User { UserName = "user1@gmail.com", Email = "user1@gmail.com", FirstName = "Pelle", LastName = "Svantesson", DisplayName = "Pelle Svantesson", TeamId = team1.Id, DepartmentId = dep3.Id, ManagerUserId = managerUser.Id };
            var user2 = new User { UserName = "user2@gmail.com", Email = "user2@gmail.com", FirstName = "Thom", LastName = "Ivarsson", DisplayName = "Thom Ivarsson", TeamId = team2.Id, DepartmentId = dep2.Id, ManagerUserId = managerUser.Id };
            var user3 = new User { UserName = "user3@gmail.com", Email = "user3@gmail.com", FirstName = "Britta", LastName = "Johnsson", DisplayName = "Britta Johnsson", TeamId = team3.Id, DepartmentId = dep1.Id };
            var user4 = new User { UserName = "user4@gmail.com", Email = "user4@gmail.com", FirstName = "Einar", LastName = "Andersson", DisplayName = "Einar Andersson", TeamId = team1.Id, DepartmentId = dep2.Id, ManagerUserId = managerUser.Id };
            var user5 = new User { UserName = "user5@gmail.com", Email = "user5@gmail.com", FirstName = "Sarah", LastName = "Qvistsson", DisplayName = "Sarah Qvistsson", TeamId = team2.Id, DepartmentId = dep3.Id, ManagerUserId = managerUser.Id }; */

            var users = new List<User>()
            {
                user0//, user1, user2, user3, user4, user5
            };

            foreach (var user in users)
            {
                _userManager.CreateAsync(user, userPassword).Wait();
            }

            var abs0 = new Zakazka { CisloZakazky = "Zak001", ZakaznikId = zak0.Id, Name = "Název zakázky", Color = "#a8ffe2", SmerPrace = "kupředu", Poznamka = "Poznámka"};
            /* var abs1 = new AbsenceType { Name = "Vacation" };
            var abs2 = new AbsenceType { Name = "Leave" };
            var abs3 = new AbsenceType { Name = "Away" }; */

            var abses = new List<Zakazka>()
            {
                abs0//abs1, abs2, abs3
            };

            _context.AbsenceTypes.AddRange(abses);

            var vac0 = new StrojBooking { Comment = "Poznámka", Approval = ApprovalState.Pending.ToString(), FromDate = DateTime.Now.Date.AddDays(-3), ToDate = DateTime.Now.Date.AddDays(3), User = user0, AbsenceType = abs0 };
/*          var vac1 = new VacationBooking { Comment = "Trip to Paris", Approval = ApprovalState.Pending.ToString(), FromDate = DateTime.Now.Date.AddDays(-3), ToDate = DateTime.Now.Date.AddDays(3), User = user1, AbsenceType = abs1 };
            var vac2 = new VacationBooking { Comment = "Away from home", Approval = ApprovalState.Pending.ToString(), FromDate = DateTime.Now.Date.AddDays(10), ToDate = DateTime.Now.Date.AddDays(12), User = user1, AbsenceType = abs3 };
            var vac3 = new VacationBooking { Comment = "Party day", Approval = ApprovalState.Approved.ToString(), FromDate = DateTime.Now.Date.AddDays(15), ToDate = DateTime.Now.Date.AddDays(17), User = user1, AbsenceType = abs1 };
            var vac4 = new VacationBooking { Comment = "Going to Hawaii", Approval = ApprovalState.Pending.ToString(), FromDate = DateTime.Now.Date.AddDays(8), ToDate = DateTime.Now.Date.AddDays(16), User = user2, AbsenceType = abs1 };
            var vac5 = new VacationBooking { Comment = "Tripping", Approval = ApprovalState.Denied.ToString(), FromDate = DateTime.Now.Date.AddDays(2), ToDate = DateTime.Now.Date.AddDays(4), User = user2, AbsenceType = abs2 };
            var vac6 = new VacationBooking { Comment = "Cruise trip", Approval = ApprovalState.Pending.ToString(), FromDate = DateTime.Now.Date.AddDays(11), ToDate = DateTime.Now.Date.AddDays(14), User = user3, AbsenceType = abs1 };
            var vac7 = new VacationBooking { Comment = "Barcelona", Approval = ApprovalState.Approved.ToString(), FromDate = DateTime.Now.Date.AddDays(-1), ToDate = DateTime.Now.Date.AddDays(4), User = user3, AbsenceType = abs1 };
            var vac8 = new VacationBooking { Comment = "Road trip", Approval = ApprovalState.Denied.ToString(), FromDate = DateTime.Now.Date.AddDays(5), ToDate = DateTime.Now.Date.AddDays(20), User = user4, AbsenceType = abs2 };
            var vac9 = new VacationBooking { Comment = "Kentucky", Approval = ApprovalState.Pending.ToString(), FromDate = DateTime.Now.Date.AddDays(20), ToDate = DateTime.Now.Date.AddDays(25), User = user5, AbsenceType = abs3 };
            var vac10 = new VacationBooking { Comment = "Far away", Approval = ApprovalState.Approved.ToString(), FromDate = DateTime.Now.Date.AddDays(-8), ToDate = DateTime.Now.Date.AddDays(-2), User = user5, AbsenceType = abs1 };
 */
            var vacs = new List<StrojBooking>()
            {
                vac0//vac1, vac2, vac3, vac4, vac5, vac6, vac7, vac8, vac9, vac10
            };

            foreach (var vac in vacs)
            {
                //vac.VacationDays = GetVacationDayFromBookings(vac);
                vac.VacationDays = GetStrojDayFromBookings(vac);
                
            }

            //_context.AbsenceTypes.AddRange(abses);
            _context.StrojBookings.AddRange(vacs);

            _context.SaveChanges();
        }

        private void StrojSeedDb() {

            var team1 = new Team { Name = "StrojTeam1", Shortening = "ST1" };
            var team2 = new Team { Name = "StrojTeam2", Shortening = "ST2" };
            var team3 = new Team { Name = "StrojTeam3", Shortening = "ST3" };
            var teams = new List<Team>()
            {
                team1, team2, team3
            };
            _context.Teams.AddRange(teams);

            var dep1 = new Department { Name = "Louny", Shortening = "Ln" };
            var dep2 = new Department { Name = "Plzeň", Shortening = "Pl" };
            var dep3 = new Department { Name = "Ostrava", Shortening = "Os" };
            var deps = new List<Department>()
            {
                dep1, dep2, dep3
            };
            _context.Departments.AddRange(deps);

            var mistr1 = new StrojMistr { MistrId = "m001", Name = "Pepe", LastName = "Novák", Telefon = "+420737373737", Alias = "Novák Pepe" };
            var mistr2 = new StrojMistr { MistrId = "m002", Name = "Jiřík", LastName = "Novák", Telefon = "+420737373737", Alias = "Novák Jiřík" };
            var mistrs = new List<StrojMistr>()
            {
                mistr1, mistr2
            };
            _context.StrojMistrs.AddRange(mistrs);
            _context.SaveChanges();
            
            var stroj1 = new Stroj { InvNr = "S001", Name = "Vagon001", Popis = "Vagon na pohladku koleji", TeamId = team1.Id, DepartmentId = dep3.Id, MistrId = mistr1.Id, StrojColor = "red" };
            var stroj2 = new Stroj { InvNr = "S002", Name = "Vagon002", Popis = "Vagon na převoz koleji", TeamId = team2.Id, DepartmentId = dep1.Id, MistrId = mistr2.Id, StrojColor = "blue" };
            var strojs = new List<Stroj>()
            {
                stroj1, stroj2
            };

            _context.Strojs.AddRange(strojs);

            _context.SaveChanges();
        }

        public List<StrojDay> GetStrojDayFromBookings(StrojBooking strojBooking)
        {
            var vacdayList = new List<StrojDay>();
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
                    continue;
                }
                vacdayList.Add(vacday);
            }
            return vacdayList;
        }

       /*  public List<VacationDay> GetVacationDayFromBookings(VacationBooking vacationBooking)
        {
            var vacdayList = new List<VacationDay>();
            for (DateTime d = vacationBooking.FromDate; d <= vacationBooking.ToDate; d = d.AddDays(1))
            {
                var vacday = new VacationDay()
                {
                    Id = 0,
                    VacationDate = d,
                    VacationBookingId = vacationBooking.Id,
                };
                if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }
                vacdayList.Add(vacday);
            }
            return vacdayList;
        }
 */
        private void ClearDatabase(bool clearAll = false)
        {
            var departments = _context.Departments.ToList();
            _context.Departments.RemoveRange(departments);

            var teams = _context.Teams.ToList();
            _context.Teams.RemoveRange(teams);

            var absenceTypes = _context.AbsenceTypes.ToList();
            _context.AbsenceTypes.RemoveRange(absenceTypes);

            var vacationDays = _context.VacationDays.ToList();
            _context.VacationDays.RemoveRange(vacationDays);

            var vacationBookings = _context.VacationBookings.ToList();
            _context.VacationBookings.RemoveRange(vacationBookings);

            if (clearAll)
            {
                var userRolesRemove = _context.UserRoles.ToList();
                _context.UserRoles.RemoveRange(userRolesRemove);

                var roles = _context.Roles.ToList();
                _context.Roles.RemoveRange(roles);

                var usersRemove = _context.Users.ToList();
                _context.Users.RemoveRange(usersRemove);
            }
            else
            {
                var users = _context.Users.ToList();
                var userRoles = _context.UserRoles.ToList();

                foreach (var user in users)
                {
                    if (!userRoles.Any(r => r.UserId == user.Id))
                    {
                        _context.Users.Remove(user);
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}
