using ApiApplication.Attributes;
using ApiApplication.DTOs.SeatDTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ApiApplication.Tests.AttrubutesTests
{
    public class AtLeastOneSeatRequiredAttributeTests
    {
        [Fact]
        public void IsValid_ShouldReturnSuccess_WhenListContainsAtLeastOneSeat()
        {
            // Arrange
            var attribute = new AtLeastOneSeatRequiredAttribute("At least one seat is required.");
            var seats = new List<SeatReserveRequestDTO>
        {
            new SeatReserveRequestDTO { Row = 1, SeatNumber = 1 }
        };
            var validationContext = new ValidationContext(seats);

            // Act
            var result = attribute.GetValidationResult(seats, validationContext);

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IsValid_ShouldReturnError_WhenListIsEmpty()
        {
            // Arrange
            var attribute = new AtLeastOneSeatRequiredAttribute("At least one seat is required.");
            var seats = new List<SeatReserveRequestDTO>();
            var validationContext = new ValidationContext(seats);

            // Act
            var result = attribute.GetValidationResult(seats, validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("At least one seat is required.", result.ErrorMessage);
        }

        [Fact]
        public void IsValid_ShouldReturnError_WhenValueIsNull()
        {
            // Arrange
            var attribute = new AtLeastOneSeatRequiredAttribute("At least one seat is required.");
            List<SeatReserveRequestDTO> seats = null;
            var validationContext = new ValidationContext(new object());

            // Act
            var result = attribute.GetValidationResult(seats, validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("At least one seat is required.", result.ErrorMessage);
        }

        [Fact]
        public void IsValid_ShouldReturnError_WhenValueIsNotAList()
        {
            // Arrange
            var attribute = new AtLeastOneSeatRequiredAttribute("At least one seat is required.");
            var value = "Not a list of seats";
            var validationContext = new ValidationContext(value);

            // Act
            var result = attribute.GetValidationResult(value, validationContext);

            // Assert
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("At least one seat is required.", result.ErrorMessage);
        }
    }
}

