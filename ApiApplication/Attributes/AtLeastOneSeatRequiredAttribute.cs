using ApiApplication.DTOs.SeatDTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Attributes
{
    public class AtLeastOneSeatRequiredAttribute : ValidationAttribute
    {
        public AtLeastOneSeatRequiredAttribute(string errorMessage) : base(errorMessage)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var seats = value as List<SeatReserveRequestDTO>;
            if (seats == null || seats.Count == 0)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
