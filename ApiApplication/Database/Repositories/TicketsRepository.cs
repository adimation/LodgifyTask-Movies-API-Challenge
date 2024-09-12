using ApiApplication.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using ApiApplication.Database.Repositories.Abstractions;

namespace ApiApplication.Database.Repositories
{
    public class TicketsRepository : ITicketsRepository
    {
        private readonly CinemaContext _context;

        public TicketsRepository(CinemaContext context)
        {
            _context = context;
        }

        public Task<TicketEntity> GetAsync(Guid id, CancellationToken cancel)
        {
            return _context.Tickets.FirstOrDefaultAsync(x => x.Id == id, cancel);
        }

        public async Task<IEnumerable<TicketEntity>> GetEnrichedAsync(int showtimeId, CancellationToken cancel)
        {
            return await _context.Tickets
                .Include(x => x.Showtime).ThenInclude(s => s.Movie)
                .Include(x => x.TicketSeats)
                .Where(x => x.ShowtimeId == showtimeId)
                .ToListAsync(cancel);
        }

        public async Task<TicketEntity> CreateAsync(ShowtimeEntity showtime, IEnumerable<TicketSeatEntity> selectedSeats, CancellationToken cancel)
        {
            var ticket = new TicketEntity
            {
                Showtime = showtime,
                TicketSeats = new List<TicketSeatEntity>(selectedSeats)
            };

            //foreach (var seat in selectedSeats)
            //{
            //    // Attach the seat to the context if it's not already tracked
            //    var trackedSeat = await _context.Seats
            //        .FirstOrDefaultAsync(s => s.Row == seat.Row && s.SeatNumber == seat.SeatNumber && s.AuditoriumId == seat.AuditoriumId, cancel);

            //    // Update the existing seat to associate it with the new ticket
            //    trackedSeat.TicketSeats = ticket;
            //    ticket.Seats.Add(trackedSeat);
            //}

            _context.Tickets.Add(ticket);

            await _context.SaveChangesAsync(cancel);

            return ticket;

            // Exception: An item with the same key has already been added. Key: System.Object[] EF Core
            //var ticket = _context.Tickets.Add(new TicketEntity
            //{
            //    Showtime = showtime,
            //    Seats = new List<SeatEntity>(selectedSeats)
            //});

            //await _context.SaveChangesAsync(cancel);

            //return ticket.Entity;
        }

        public async Task<TicketEntity> ConfirmPaymentAsync(TicketEntity ticket, CancellationToken cancel)
        {
            ticket.Paid = true;
            _context.Update(ticket);
            await _context.SaveChangesAsync(cancel);
            return ticket;
        }
    }
}
