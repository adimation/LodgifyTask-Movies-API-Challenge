using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Exceptions;
using ApiApplication.Services.Abstractions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using ApiApplication.DTOs.TicketDTOs;
using System.Linq;
using ApiApplication.Configurations;
using Microsoft.Extensions.Options;
using FluentValidation;

namespace ApiApplication.Services
{
    public class TicketService : ITicketService
    {
        private readonly ILogger<TicketService> _logger;
        private readonly IMapper _mapper;
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly GeneralConfig _generalConfig;
        private readonly IValidator<ReserveTicketRequestDTO> _reserveSeatsValidator;

        public TicketService(ILogger<TicketService> logger, 
            ITicketsRepository ticketsRepository, 
            IShowtimesRepository showtimesRepository, 
            IMapper mapper, 
            IOptions<GeneralConfig> config,
            IValidator<ReserveTicketRequestDTO> reserveSeatsValidator)
        {
            _logger = logger;
            _mapper = mapper;
            _ticketsRepository = ticketsRepository;
            _showtimesRepository = showtimesRepository;
            _generalConfig = config.Value;
            _reserveSeatsValidator = reserveSeatsValidator;
        }

        public async Task<TicketReservationResponseDTO> ReserveSeats(ReserveTicketRequestDTO requestModel, CancellationToken cancellationToken)
        {

            var validationResult = await _reserveSeatsValidator.ValidateAsync(requestModel, cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new FluentValidation.ValidationException(validationResult.Errors);
            }

            // Retrieve the showtime details
            var showtime = (await _showtimesRepository.GetAllAsync(s => s.Id == requestModel.ShowtimeId, cancellationToken)).FirstOrDefault();
            var newTicketId = Guid.NewGuid();

            // Create a new reservation
            var seatEntities = requestModel.Seats.Select(s => new TicketSeatEntity
            {
                Row = s.Row,
                SeatNumber = s.SeatNumber,
                AuditoriumId = showtime.AuditoriumId,
                TicketId = newTicketId
            });

            var createdTicketEntity = await _ticketsRepository.CreateAsync(showtime, seatEntities, cancellationToken);

            // Return reservation response
            return new TicketReservationResponseDTO
            {
                ReservationId = createdTicketEntity.Id,
                NumberOfSeats = createdTicketEntity.TicketSeats.Count,
                AuditoriumId = createdTicketEntity.Showtime.AuditoriumId,
                MovieTitle = createdTicketEntity.Showtime.Movie.Title
            };
        }

        public async Task<bool> ConfirmTicket(Guid ticketId, CancellationToken cancellationToken)
        {
            var ticket = await _ticketsRepository.GetAsync(ticketId, cancellationToken);

            if (ticket == null)
                throw new ResourceNotFoundException($"Ticket {ticketId} not found.");

            if (ticket.Paid)
                throw new InvalidOperationException($"Ticket {ticketId} already sold.");

            if ((DateTime.Now - ticket.CreatedTime).TotalMinutes > _generalConfig.GetReservationExpiryInMinutes())
                throw new InvalidOperationException($"Ticket {ticketId} is expired.");

            ticket.Paid = true;

            await _ticketsRepository.ConfirmPaymentAsync(ticket, cancellationToken);

            return true;
        }

        public async Task<IEnumerable<TicketDTO>> GetByShowtimeId(int showtimeId, CancellationToken cancellationToken)
        {
            var tickets = await _ticketsRepository.GetEnrichedAsync(showtimeId, cancellationToken);

            var respTickets = _mapper.Map<IEnumerable<TicketDTO>>(tickets);

            return respTickets;
        }
    }
}
