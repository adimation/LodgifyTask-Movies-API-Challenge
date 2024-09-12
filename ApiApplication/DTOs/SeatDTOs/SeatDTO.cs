using System;

namespace ApiApplication.DTOs.SeatDTOs
{
    public class SeatDTO
    {
        public short Row { get; set; }
        public short SeatNumber { get; set; }
        public int AuditoriumId { get; set; }
    }
}
