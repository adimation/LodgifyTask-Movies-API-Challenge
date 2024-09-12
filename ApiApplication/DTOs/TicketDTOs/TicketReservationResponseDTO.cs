using System;

namespace ApiApplication.DTOs.TicketDTOs
{
    public class TicketReservationResponseDTO
    {
        public Guid ReservationId { get; set; }
        public int NumberOfSeats { get; set; }
        public int AuditoriumId { get; set; }
        public string MovieTitle { get; set; }
    }
}
