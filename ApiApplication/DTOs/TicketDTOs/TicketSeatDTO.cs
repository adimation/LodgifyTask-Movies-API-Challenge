using System;

namespace ApiApplication.DTOs.TicketDTOs
{
    public class TicketSeatDTO
    {
        public Guid TicketId { get; set; }
        public short Row { get; set; }
        public short SeatNumber { get; set; }
        public int AuditoriumId { get; set; }
    }
}
