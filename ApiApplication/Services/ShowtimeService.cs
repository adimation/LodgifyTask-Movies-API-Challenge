using ApiApplication.ApiClient;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.DTOs.ShowtimeDTOs;
using ApiApplication.Exceptions;
using ApiApplication.Services.Abstractions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly ILogger<ShowtimeService> _logger;
        private readonly IMapper _mapper;
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;
        private readonly IApiClientGrpc _moviesApiClient;

        public ShowtimeService(ILogger<ShowtimeService> logger, IShowtimesRepository showtimesRepo, IAuditoriumsRepository auditoriumsRepository, IMapper mapper, IApiClientGrpc moviesApiClient)
        {
            _logger = logger;
            _mapper = mapper;
            _showtimesRepository = showtimesRepo;
            _auditoriumsRepository = auditoriumsRepository;
            _moviesApiClient = moviesApiClient;
        }

        public async Task<ShowtimeDTO> Create(CreateShowtimeDTO model, CancellationToken cancellationToken)
        {
            var auditorium = await _auditoriumsRepository.GetAsync(model.AuditoriumId, cancellationToken);

            if (auditorium is null)
                throw new ResourceNotFoundException($"No auditorium found with id {model.AuditoriumId}", Constants.Constants.NOTFOUND_ERROR);

            var movie = await _moviesApiClient.GetByIdOrCached(model.ImdbId);
            if (movie is null)
                throw new ResourceNotFoundException($"No Movie found with id {model.ImdbId}", Constants.Constants.NOTFOUND_ERROR);

            var showTimeEntity = new ShowtimeEntity
            {
                Movie = new MovieEntity
                {
                    ImdbId = model.ImdbId,
                    ReleaseDate = new DateTime(int.Parse(movie.Year), 1, 1),
                    Stars = movie.ImDbRating,
                    Title = movie.Title
                },
                SessionDate = model.SessionDate,
                AuditoriumId = model.AuditoriumId
            };

            showTimeEntity = await _showtimesRepository.CreateShowtime(showTimeEntity, cancellationToken);

            return _mapper.Map<ShowtimeDTO>(showTimeEntity);
        }

        public async Task<IEnumerable<ShowtimeDTO>> GetAll(CancellationToken cancellationToken)
        {
            var response = await _showtimesRepository.GetAllAsync(null, cancellationToken);

            var responseDTO = _mapper.Map<IEnumerable<ShowtimeDTO>>(response);

            return responseDTO;
        }

        public async Task<ShowtimeDTO> GetById(int id, CancellationToken cancellationToken)
        {
            var response = await _showtimesRepository.GetAllAsync(s => s.Id == id, cancellationToken);

            if (response.FirstOrDefault() is null) throw new ResourceNotFoundException($"No showtime found with id {id}", Constants.Constants.NOTFOUND_ERROR);

            var responseDTO = _mapper.Map<ShowtimeDTO>(response.FirstOrDefault());

            return responseDTO;
        }
    }
}
