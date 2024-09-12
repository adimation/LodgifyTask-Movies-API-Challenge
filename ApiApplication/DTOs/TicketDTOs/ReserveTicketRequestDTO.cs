using ApiApplication.Attributes;
using ApiApplication.DTOs.SeatDTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.DTOs.TicketDTOs
{
    public class ReserveTicketRequestDTO
    {
        [Required(ErrorMessage = "ShowtimeId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShowtimeId must be greater than zero.")]
        public int ShowtimeId { get; set; }

        [AtLeastOneSeatRequired("You must select at least one seat to reserve.")]
        public List<SeatReserveRequestDTO> Seats { get; set; }
    }
}
