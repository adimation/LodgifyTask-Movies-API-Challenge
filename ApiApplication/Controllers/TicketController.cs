using ApiApplication.DTOs.Abstract;
using ApiApplication.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using ApiApplication.DTOs.TicketDTOs;
using ApiApplication.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ILogger<TicketController> _logger;
        private readonly ITicketService _ticketService;

        public TicketController(ILogger<TicketController> logger, ITicketService ticketService)
        {
            _logger = logger;
            _ticketService = ticketService;
        }

        [HttpPost("reserve-seats")]
        [ValidateModel]
        public async Task<IActionResult> ReserveSeats(ReserveTicketRequestDTO model, CancellationToken cancellationToken)
        {
            var response = await _ticketService.ReserveSeats(model, cancellationToken);

            return Ok(new ApiResponseDTO<TicketReservationResponseDTO>() { Payload = response });
        }

        [HttpPost("confirm-ticket")]
        [ValidateModel]
        public async Task<IActionResult> ConfirmTicket(Guid ticketId, CancellationToken cancellationToken)
        {
            var response = await _ticketService.ConfirmTicket(ticketId, cancellationToken);

            return Ok(new ApiResponseDTO<bool>() { Payload = response });
        }

        [HttpGet("by-showtime/{showtimeId}")]
        public async Task<IActionResult> TicketsByShowtimeId(int showtimeId, CancellationToken cancellationToken)
        {
            var response = await _ticketService.GetByShowtimeId(showtimeId, cancellationToken);

            return Ok(new ApiResponseDTO<List<TicketDTO>>() { Payload = response.ToList() });
        }
    }
}
