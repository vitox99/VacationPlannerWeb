﻿
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VacationPlannerWeb.Models;


namespace VacationPlannerWeb.DataAccess;

    public class AppDbContext : IdentityDbContext<User, Role, string>
    
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }      
/*
//vma
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VacationWebDB");
                    //"Server=(localdb)\\MSSQLLocalDB;Database=VacationWebDB;Trusted_Connection=True;MultipleActiveResultSets=true"
                }
            }
//vma
*/
        public DbSet<VacationBooking> VacationBookings { get; set; }
        public DbSet<VacationDay> VacationDays { get; set; }
        public DbSet<WorkFreeDay> WorkFreeDays { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Zakazka> AbsenceTypes { get; set; }
        public DbSet<Stroj> Strojs { get; set; }
        public DbSet<StrojBooking>StrojBookings {get; set;}
        public DbSet<StrojDay>StrojDays {get; set;}
        public DbSet<StrojMistr>StrojMistrs {get; set;}
        public DbSet<Zakaznik>Zakaznici {get; set;}
    }
/*
    public class YourDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {

        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
*/

