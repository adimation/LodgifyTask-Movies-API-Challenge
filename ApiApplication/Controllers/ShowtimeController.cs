using ApiApplication.Attributes;
using ApiApplication.DTOs.Abstract;
using ApiApplication.DTOs.ShowtimeDTOs;
using ApiApplication.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShowtimeController : ControllerBase
    {
        private readonly IShowtimeService _showtimeService;

        public ShowtimeController(IShowtimeService showtimeService) 
        {
            _showtimeService = showtimeService;
        }

        [HttpGet]
        [LogExecutionTime]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await _showtimeService.GetAll(cancellationToken);

            return Ok(new ApiResponseDTO<IEnumerable<ShowtimeDTO>>() { Payload = response });
        }

        [HttpGet("{id}")]
        [LogExecutionTime]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _showtimeService.GetById(id, cancellationToken);

            return Ok(new ApiResponseDTO<ShowtimeDTO>() { Payload = response });
        }

        [HttpPost]
        [ValidateModel]
        [LogExecutionTime]
        public async Task<IActionResult> Create(CreateShowtimeDTO model, CancellationToken cancellationToken)
        {
            var response = await _showtimeService.Create(model, cancellationToken);

            return Ok(new ApiResponseDTO<ShowtimeDTO>() { Payload = response });
        }
    }
}
