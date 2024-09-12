using ApiApplication.DTOs.TicketDTOs;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace ApiApplication.Services.Abstractions
{
    public interface ITicketService
    {
        Task<TicketReservationResponseDTO> ReserveSeats(ReserveTicketRequestDTO requestModel, CancellationToken cancellationToken);

        Task<bool> ConfirmTicket(Guid ticketId, CancellationToken cancellationToken);

        Task<IEnumerable<TicketDTO>> GetByShowtimeId(int showtimeId, CancellationToken cancellationToken);
    }
}
