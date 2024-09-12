using ApiApplication.Configurations;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.DTOs.TicketDTOs;
using FluentValidation;
using Microsoft.Extensions.Options;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using ApiApplication.Database.Entities;

namespace ApiApplication.Validators
{
    public class ReserveTicketRequestDTOValidator : AbstractValidator<ReserveTicketRequestDTO>
    {
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;
        private readonly GeneralConfig _generalConfig;
        private ShowtimeEntity _showtime;

        public ReserveTicketRequestDTOValidator(ITicketsRepository ticketsRepository, IShowtimesRepository showtimesRepository, IAuditoriumsRepository auditoriumsRepository, IOptions<GeneralConfig> config)
        {
            _ticketsRepository = ticketsRepository;
            _showtimesRepository = showtimesRepository;
            _auditoriumsRepository = auditoriumsRepository;
            _generalConfig = config.Value;

            RuleFor(x => x)
                .MustAsync(ShowtimeExists).WithMessage("Showtime not found.")
                .MustAsync(BeValidSeats).WithMessage("One or more seats are invalid.")
                .MustAsync(AreNotAlreadyReservedOrBought).WithMessage("Seats are already reserved or sold.")
                .MustAsync(BeUniqueSeats).WithMessage("Seats must be unique.")
                .Must(AreSeatsContiguous).WithMessage("Seats must be contiguous.");
        }

        private async Task<bool> AreNotAlreadyReservedOrBought(ReserveTicketRequestDTO requestModel, CancellationToken cancellationToken)
        {
            // This validation depends on show time hence return true if showtime is null
            if (_showtime == null) return true;

            var existingShowtimeTickets = await _ticketsRepository.GetEnrichedAsync(_showtime.Id, cancellationToken);
            // Check if any of the requested seats are already reserved or sold
            foreach (var seat in requestModel.Seats)
            {
                var existingSeat = existingShowtimeTickets
                    .SelectMany(t => t.TicketSeats)
                    .FirstOrDefault(s => s.Row == seat.Row && s.SeatNumber == seat.SeatNumber);

                if (existingSeat != null)
                {
                    if (existingSeat.Ticket != null && (existingSeat.Ticket.Paid || (DateTime.Now - existingSeat.Ticket.CreatedTime).TotalMinutes < _generalConfig.GetReservationExpiryInMinutes()))
                        return false;
                }
            }

            return true;
        }

        private async Task<bool> ShowtimeExists(ReserveTicketRequestDTO requestModel, CancellationToken cancellationToken)
        {
            _showtime = (await _showtimesRepository.GetAllAsync(s => s.Id == requestModel.ShowtimeId, cancellationToken)).FirstOrDefault();

            if (_showtime == null)
                return false;

            return true;
        }

        private async Task<bool> BeValidSeats(ReserveTicketRequestDTO requestModel, CancellationToken cancellationToken)
        {
            // This validation depends on show time hence return true if showtime is null
            if (_showtime == null) return true;

            var auditorium = await _auditoriumsRepository.GetAsync(_showtime.AuditoriumId, cancellationToken);
            bool allSeatsExist = requestModel.Seats.All(seat =>
                (auditorium).Seats.Any(dbSeat =>
                    dbSeat.Row == seat.Row &&
                    dbSeat.SeatNumber == seat.SeatNumber));

            if (!allSeatsExist)
                return false;

            return true;
        }

        private async Task<bool> BeUniqueSeats(ReserveTicketRequestDTO requestModel, CancellationToken cancellationToken)
        {
            return !requestModel.Seats
                .GroupBy(seat => new { seat.Row, seat.SeatNumber })
                .Any(group => group.Count() > 1);
        }

        private bool AreSeatsContiguous(ReserveTicketRequestDTO requestModel)
        {
            var seats = requestModel.Seats;

            // True in case of 1 seat
            if (seats.Count == 1) return true;

            Dictionary<(int, int), bool> seatsDict = new Dictionary<(int, int), bool>();
            foreach (var seat in seats)
            {
                seatsDict[(seat.Row, seat.SeatNumber)] = true;
            }

            foreach (var seat in seats)
            {
                if (
                    !seatsDict.ContainsKey((seat.Row + 1, seat.SeatNumber))
                    && !seatsDict.ContainsKey((seat.Row - 1, seat.SeatNumber))
                    && !seatsDict.ContainsKey((seat.Row, seat.SeatNumber + 1))
                    && !seatsDict.ContainsKey((seat.Row, seat.SeatNumber - 1))
                    )
                    return false;
            }

            return true;
        }
    }
}
