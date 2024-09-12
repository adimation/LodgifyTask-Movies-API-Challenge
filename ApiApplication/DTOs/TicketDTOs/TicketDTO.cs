using ApiApplication.Database.Entities;
using System.Collections.Generic;
using System;
using ApiApplication.DTOs.ShowtimeDTOs;
using ApiApplication.DTOs.SeatDTOs;

namespace ApiApplication.DTOs.TicketDTOs
{
    public class TicketDTO
    {
        public Guid Id { get; set; }
        public int ShowtimeId { get; set; }
        public ICollection<TicketSeatDTO> TicketSeats { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool Paid { get; set; }
        public ShowtimeDTO Showtime { get; set; }
    }
}
