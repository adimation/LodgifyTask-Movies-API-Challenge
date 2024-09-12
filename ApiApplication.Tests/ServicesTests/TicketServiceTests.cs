using ApiApplication.Configurations;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.DTOs.SeatDTOs;
using ApiApplication.DTOs.TicketDTOs;
using ApiApplication.Services;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xunit;
using System.Linq.Expressions;
using System.Linq;
using ApiApplication.Exceptions;
using FluentValidation.Results;

namespace ApiApplication.Tests.ServicesTests
{
    public class TicketServiceTests
    {
        private readonly Mock<IShowtimesRepository> _showtimesRepositoryMock;
        private readonly Mock<ITicketsRepository> _ticketsRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TicketService>> _loggerMock;
        private readonly Mock<IOptions<GeneralConfig>> _generalConfigMock;
        private readonly Mock<IValidator<ReserveTicketRequestDTO>> _reserveSeatsValidatorMock;
        private readonly TicketService _ticketService;

        public TicketServiceTests()
        {
            _showtimesRepositoryMock = new Mock<IShowtimesRepository>();
            _ticketsRepositoryMock = new Mock<ITicketsRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<TicketService>>();
            _generalConfigMock = new Mock<IOptions<GeneralConfig>>();
            _reserveSeatsValidatorMock = new Mock<IValidator<ReserveTicketRequestDTO>>();

            _generalConfigMock.Setup(x => x.Value).Returns(new GeneralConfig());

            _ticketService = new TicketService(
                _loggerMock.Object,
                _ticketsRepositoryMock.Object,
                _showtimesRepositoryMock.Object,
                _mapperMock.Object,
                _generalConfigMock.Object,
                _reserveSeatsValidatorMock.Object
            );
        }

        #region ReserveSeats Tests
        [Fact]
        public async Task ReserveSeats_ShouldReserveSeats_WhenValidRequest()
        {
            // Arrange
            var requestModel = new ReserveTicketRequestDTO
            {
                ShowtimeId = 1,
                Seats = new List<SeatReserveRequestDTO>
                {
                    new SeatReserveRequestDTO { Row = 1, SeatNumber = 1 },
                    new SeatReserveRequestDTO { Row = 1, SeatNumber = 2 }
                }
            };

            var showtime = new ShowtimeEntity { Id = requestModel.ShowtimeId, AuditoriumId = 1, Movie = new MovieEntity() { Title = "" } };

            _reserveSeatsValidatorMock.Setup(v => v.ValidateAsync(requestModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _showtimesRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity> { showtime });

            _ticketsRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<ShowtimeEntity>(), It.IsAny<IEnumerable<TicketSeatEntity>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TicketEntity
                {
                    Id = Guid.NewGuid(),
                    Showtime = showtime,
                    TicketSeats = new List<TicketSeatEntity>
                    {
                    new TicketSeatEntity { Row = 1, SeatNumber = 1, AuditoriumId = 1 },
                    new TicketSeatEntity { Row = 1, SeatNumber = 2, AuditoriumId = 1 }
                    }
                });

            // Act
            var result = await _ticketService.ReserveSeats(requestModel, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.NumberOfSeats);
        }

        [Fact]
        public async Task ReserveSeats_ShouldThrowValidationException_WhenShowtimeDoesNotExist()
        {
            // Arrange
            var request = new ReserveTicketRequestDTO
            {
                ShowtimeId = 1,
                Seats = new List<SeatReserveRequestDTO>
                {
                    new SeatReserveRequestDTO { Row = 1, SeatNumber = 1 }
                }
            };

            _showtimesRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity>());

            _reserveSeatsValidatorMock
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> {
            new ValidationFailure(nameof(request.ShowtimeId), "Showtime not found.")
                }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _ticketService.ReserveSeats(request, CancellationToken.None));

            Assert.Contains("Showtime not found.", exception.Message);
        }

        [Fact]
        public async Task ReserveSeats_ShouldThrowValidationException_WhenSeatsAreInvalid()
        {
            // Arrange
            var request = new ReserveTicketRequestDTO
            {
                ShowtimeId = 1,
                Seats = new List<SeatReserveRequestDTO>
                {
                    new SeatReserveRequestDTO { Row = 99, SeatNumber = 99 } // Invalid seat
                }
            };

            var showtime = new ShowtimeEntity { Id = 1, AuditoriumId = 1 };
            var auditorium = new AuditoriumEntity
            {
                Id = 1,
                Seats = new List<SeatEntity>
                {
                    new SeatEntity { Row = 1, SeatNumber = 1 } // Valid seat
                }
            };

            _showtimesRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity> { showtime });

            _reserveSeatsValidatorMock
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> {
                    new ValidationFailure("Seats", "One or more seats are invalid.")
                }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _ticketService.ReserveSeats(request, CancellationToken.None));

            Assert.Contains("One or more seats are invalid.", exception.Message);
        }

        [Fact]
        public async Task ReserveSeats_ShouldThrowValidationException_WhenSeatsAreAlreadyReservedOrSold()
        {
            // Arrange
            var request = new ReserveTicketRequestDTO
            {
                ShowtimeId = 1,
                Seats = new List<SeatReserveRequestDTO>
                {
                    new SeatReserveRequestDTO { Row = 1, SeatNumber = 1 } // Already reserved/sold seat
                }
            };

            var showtime = new ShowtimeEntity { Id = 1, AuditoriumId = 1 };
            var existingTicket = new TicketEntity
            {
                ShowtimeId = 1,
                Paid = true, // Already paid, thus sold
                CreatedTime = DateTime.Now.AddMinutes(-5),
                TicketSeats = new List<TicketSeatEntity>
                {
                    new TicketSeatEntity { Row = 1, SeatNumber = 1, AuditoriumId = 1 }
                }
            };

            _showtimesRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity> { showtime });

            _ticketsRepositoryMock
                .Setup(repo => repo.GetEnrichedAsync(showtime.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TicketEntity> { existingTicket });

            _reserveSeatsValidatorMock
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> {
                    new ValidationFailure("Seats", "Seats are already reserved or sold.")
                }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _ticketService.ReserveSeats(request, CancellationToken.None));

            Assert.Contains("Seats are already reserved or sold.", exception.Message);
        }

        [Fact]
        public async Task ReserveSeats_ShouldThrowValidationException_WhenSeatsAreNotContiguous()
        {
            // Arrange
            var request = new ReserveTicketRequestDTO
            {
                ShowtimeId = 1,
                Seats = new List<SeatReserveRequestDTO>
                {
                    new SeatReserveRequestDTO { Row = 1, SeatNumber = 1 },
                    new SeatReserveRequestDTO { Row = 1, SeatNumber = 3 } // Non-contiguous seat
                }
            };

            var showtime = new ShowtimeEntity { Id = 1, AuditoriumId = 1 };
            var auditorium = new AuditoriumEntity
            {
                Id = 1,
                Seats = new List<SeatEntity>
                {
                    new SeatEntity { Row = 1, SeatNumber = 1 },
                    new SeatEntity { Row = 1, SeatNumber = 2 },
                    new SeatEntity { Row = 1, SeatNumber = 3 }
                }
            };

            _showtimesRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity> { showtime });

            _reserveSeatsValidatorMock
                .Setup(validator => validator.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure> {
                    new ValidationFailure("Seats", "Seats must be contiguous.")
                }));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _ticketService.ReserveSeats(request, CancellationToken.None));

            Assert.Contains("Seats must be contiguous.", exception.Message);
        }
        #endregion

        #region ConfirmTicket Tests
        [Fact]
        public async Task ConfirmTicket_ShouldConfirmPayment_WhenTicketIsValid()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var ticket = new TicketEntity
            {
                Id = ticketId,
                CreatedTime = DateTime.Now.AddMinutes(-5),
                Paid = false
            };

            var ticketResp = new TicketEntity
            {
                Id = ticketId,
                CreatedTime = DateTime.Now.AddMinutes(-5),
                Paid = true
            };

            _ticketsRepositoryMock.Setup(repo => repo.GetAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticket);

            _ticketsRepositoryMock.Setup(repo => repo.ConfirmPaymentAsync(ticket, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ticketResp);

            // Act
            var result = await _ticketService.ConfirmTicket(ticketId, CancellationToken.None);

            // Assert
            Assert.True(result);
            _ticketsRepositoryMock.Verify(repo => repo.ConfirmPaymentAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ConfirmTicket_ShouldThrowResourceNotFoundException_WhenTicketDoesNotExist()
        {
            // Arrange
            var nonExistentTicketId = Guid.NewGuid();

            _ticketsRepositoryMock
                .Setup(repo => repo.GetAsync(nonExistentTicketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TicketEntity)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
                _ticketService.ConfirmTicket(nonExistentTicketId, CancellationToken.None));

            Assert.Equal($"Ticket {nonExistentTicketId} not found.", exception.Message);
        }

        [Fact]
        public async Task ConfirmTicket_ShouldThrowInvalidOperationException_WhenTicketIsExpired()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var expiredTicket = new TicketEntity
            {
                Id = ticketId,
                CreatedTime = DateTime.Now.AddMinutes(_generalConfigMock.Object.Value.GetReservationExpiryInMinutes() * -2)
            };

            _ticketsRepositoryMock
                .Setup(repo => repo.GetAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredTicket);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _ticketService.ConfirmTicket(ticketId, CancellationToken.None));

            Assert.Equal($"Ticket {ticketId} is expired.", exception.Message);
        }

        [Fact]
        public async Task ConfirmTicket_ShouldThrowInvalidOperationException_WhenTicketAlreadySold()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var alreadySoldTicket = new TicketEntity
            {
                Id = ticketId,
                CreatedTime = DateTime.Now.AddMinutes(-2),
                Paid = true
            };

            _ticketsRepositoryMock
                .Setup(repo => repo.GetAsync(ticketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(alreadySoldTicket);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _ticketService.ConfirmTicket(ticketId, CancellationToken.None));

            Assert.Equal($"Ticket {ticketId} already sold.", exception.Message);
        }
        #endregion

        #region GetByShowtimeId Tests
        [Fact]
        public async Task GetByShowtimeId_ShouldReturnTickets_WhenTicketsExist()
        {
            // Arrange
            var showtimeId = 1;
            var tickets = new List<TicketEntity>
            {
                new TicketEntity
                {
                    Id = Guid.NewGuid(),
                    ShowtimeId = showtimeId,
                    Showtime = new ShowtimeEntity { Id = showtimeId, AuditoriumId = 1, Movie = new MovieEntity { Title = "Movie Title" } },
                    TicketSeats = new List<TicketSeatEntity>
                    {
                        new TicketSeatEntity { Row = 1, SeatNumber = 1, AuditoriumId = 1 },
                        new TicketSeatEntity { Row = 1, SeatNumber = 2, AuditoriumId = 1 }
                    }
                }
            };

            _ticketsRepositoryMock.Setup(repo => repo.GetEnrichedAsync(showtimeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tickets);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<TicketDTO>>(It.IsAny<IEnumerable<TicketEntity>>()))
                .Returns(new List<TicketDTO>
                {
                    new TicketDTO
                    {
                        Id = tickets.First().Id,
                        ShowtimeId = showtimeId,
                    }
                });

            // Act
            var result = await _ticketService.GetByShowtimeId(showtimeId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tickets.Count, result.Count());
        }
        #endregion
    }
}
