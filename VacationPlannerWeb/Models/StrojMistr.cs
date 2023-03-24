using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.Models
{
    public class StrojMistr
    {
        [Key]
        public int Id { get; set; }
        public string MistrId { get; set; }
        public string Name { get; set; }
    }
}
