using VacationPlannerWeb.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace VacationPlannerWeb.Validations
{
    public class StrojToBookingDateValidation : ValidationAttribute
    {
        private readonly int pastMonths;
        private readonly int futureMonths;

        public StrojToBookingDateValidation(int pastBoookingLimitMonths, int furuteBookingLimitMonths)
        {
            pastMonths = pastBoookingLimitMonths;
            futureMonths = furuteBookingLimitMonths;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            StrojBooking vac = (StrojBooking)validationContext.ObjectInstance;

            if (vac.ToDate < DateTime.Today.AddMonths(- pastMonths) || vac.ToDate > DateTime.Today.AddMonths(futureMonths))
            {
                return new ValidationResult($"You can only book vacation {pastMonths} months in the past and {futureMonths} months in the future.");
            }
            if (vac.FromDate > vac.ToDate)
            {
                return new ValidationResult($"Your ToDate must be large than your FromDate.");
            }

            return ValidationResult.Success;
        }
    }
}
