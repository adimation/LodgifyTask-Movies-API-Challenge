using ApiApplication.ApiClient;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.DTOs.ShowtimeDTOs;
using ApiApplication.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ProtoDefinitions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Xunit;
using ApiApplication.DTOs.MovieDTOs;
using Microsoft.Extensions.Options;
using ApiApplication.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using ApiApplication.Exceptions;
using System.Linq;

namespace ApiApplication.Tests.ServicesTests
{
    public class ShowtimeServiceTests
    {
        private readonly ShowtimeService _service;
        private readonly Mock<IShowtimesRepository> _showtimesRepositoryMock;
        private readonly Mock<IAuditoriumsRepository> _auditoriumsRepositoryMock;
        private readonly Mock<IApiClientGrpc> _moviesApiClientMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<ShowtimeService>> _loggerMock;
        private readonly Mock<IOptions<MoviesApiConfig>> _moviesApiConfigOptionsMock;

        public ShowtimeServiceTests()
        {
            _showtimesRepositoryMock = new Mock<IShowtimesRepository>();
            _auditoriumsRepositoryMock = new Mock<IAuditoriumsRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<ShowtimeService>>();
            _moviesApiConfigOptionsMock = new Mock<IOptions<MoviesApiConfig>>();
            _moviesApiConfigOptionsMock.Setup(x => x.Value).Returns(new MoviesApiConfig() { });
            _moviesApiClientMock = new Mock<IApiClientGrpc>();

            _service = new ShowtimeService(
                _loggerMock.Object,
                _showtimesRepositoryMock.Object,
                _auditoriumsRepositoryMock.Object,
                _mapperMock.Object,
                _moviesApiClientMock.Object
            );
        }

        [Fact]
        public async Task CreateShowtime_ShouldCreateShowtime()
        {
            // Arrange
            var model = new CreateShowtimeDTO
            {
                ImdbId = "tt0111161",
                AuditoriumId = 1,
                SessionDate = DateTime.UtcNow
            };

            var movie = new showResponse
            {
                ImDbRating = "9.3",
                Title = "The Shawshank Redemption",
                Year = "1994"
            };

            var auditorium = new AuditoriumEntity
            {
                Id = model.AuditoriumId
            };

            var showtimeEntity = new ShowtimeEntity
            {
                Movie = new MovieEntity
                {
                    ImdbId = model.ImdbId,
                    Title = movie.Title,
                    ReleaseDate = new DateTime(1994, 1, 1),
                    Stars = movie.ImDbRating
                },
                SessionDate = model.SessionDate,
                AuditoriumId = model.AuditoriumId
            };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(model.AuditoriumId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auditorium);
            _moviesApiClientMock.Setup(client => client.GetByIdOrCached(model.ImdbId))
                .ReturnsAsync(movie);
            _showtimesRepositoryMock.Setup(repo => repo.CreateShowtime(It.IsAny<ShowtimeEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(showtimeEntity);
            _mapperMock.Setup(mapper => mapper.Map<ShowtimeDTO>(It.IsAny<ShowtimeEntity>()))
                .Returns(new ShowtimeDTO
                {
                    Id = 1,
                    Movie = new MovieDTO() { Title = movie.Title },
                    AuditoriumId = auditorium.Id,
                    SessionDate = model.SessionDate
                });

            // Act
            var result = await _service.Create(model, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(movie.Title, result.Movie.Title);
            Assert.Equal(auditorium.Id, result.AuditoriumId);
        }

        [Fact]
        public async Task CreateShowtime_InvalidAuditorium_ThrowsResourceNotFoundException()
        {
            // Arrange
            var model = new CreateShowtimeDTO
            {
                ImdbId = "tt0111161",
                AuditoriumId = 999, // Invalid ID
                SessionDate = DateTime.UtcNow
            };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(model.AuditoriumId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((AuditoriumEntity)null); // Simulating that the auditorium is not found

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.Create(model, CancellationToken.None));
        }

        [Fact]
        public async Task CreateShowtime_InvalidMovie_ThrowsResourceNotFoundException()
        {
            // Arrange
            var model = new CreateShowtimeDTO
            {
                ImdbId = "tt0111161",
                AuditoriumId = 1,
                SessionDate = DateTime.UtcNow
            };

            var auditorium = new AuditoriumEntity
            {
                Id = model.AuditoriumId
            };

            _auditoriumsRepositoryMock.Setup(repo => repo.GetAsync(model.AuditoriumId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(auditorium);

            _moviesApiClientMock.Setup(client => client.GetByIdOrCached(model.ImdbId))
                .ReturnsAsync((showResponse)null); // Simulating that the movie is not found

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.Create(model, CancellationToken.None));
        }

        [Fact]
        public async Task GetShowtimeById_UsesCachedData_Success()
        {
            // Arrange
            var showtimeId = 1;
            var showtimeEntity = new ShowtimeEntity
            {
                Id = showtimeId,
                Movie = new MovieEntity
                {
                    ImdbId = "tt0111161",
                    Title = "The Shawshank Redemption"
                },
                SessionDate = DateTime.UtcNow,
                AuditoriumId = 1
            };

            _showtimesRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity> { showtimeEntity });

            _mapperMock.Setup(mapper => mapper.Map<ShowtimeDTO>(It.IsAny<ShowtimeEntity>()))
                .Returns(new ShowtimeDTO
                {
                    Id = showtimeId,
                    Movie = new MovieDTO() { Title = showtimeEntity.Movie.Title },
                    AuditoriumId = showtimeEntity.AuditoriumId,
                    SessionDate = showtimeEntity.SessionDate
                });

            // Act
            var result = await _service.GetById(showtimeId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(showtimeEntity.Movie.Title, result.Movie.Title);
        }

        [Fact]
        public async Task GetById_ShowtimeNotFound_ThrowsResourceNotFoundException()
        {
            // Arrange
            var showtimeId = 999; // ID that does not exist

            _showtimesRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity>()); // Empty list

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _service.GetById(showtimeId, CancellationToken.None));
        }

        [Fact]
        public async Task GetById_ValidId_CacheMiss_ReturnsCorrectData()
        {
            // Arrange
            var showtimeId = 1;
            var showtimeEntity = new ShowtimeEntity
            {
                Id = showtimeId,
                Movie = new MovieEntity
                {
                    ImdbId = "tt0111161",
                    Title = "The Shawshank Redemption"
                },
                SessionDate = DateTime.UtcNow,
                AuditoriumId = 1
            };

            _showtimesRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity> { showtimeEntity });

            _mapperMock.Setup(mapper => mapper.Map<ShowtimeDTO>(It.IsAny<ShowtimeEntity>()))
                .Returns(new ShowtimeDTO
                {
                    Id = showtimeId,
                    Movie = new MovieDTO() { Title = showtimeEntity.Movie.Title },
                    AuditoriumId = showtimeEntity.AuditoriumId,
                    SessionDate = showtimeEntity.SessionDate
                });

            // Act
            var result = await _service.GetById(showtimeId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(showtimeEntity.Movie.Title, result.Movie.Title);
        }

        [Fact]
        public async Task GetAll_DataPresent_ReturnsListOfShowtimes()
        {
            // Arrange
            var showtimeEntities = new List<ShowtimeEntity>
            {
                new ShowtimeEntity
                {
                    Id = 1,
                    Movie = new MovieEntity
                    {
                        ImdbId = "tt0111161",
                        Title = "The Shawshank Redemption"
                    },
                    SessionDate = DateTime.UtcNow.AddDays(-1),
                    AuditoriumId = 1
                },
                new ShowtimeEntity
                {
                    Id = 2,
                    Movie = new MovieEntity
                    {
                        ImdbId = "tt0068646",
                        Title = "The Godfather"
                    },
                    SessionDate = DateTime.UtcNow.AddDays(1),
                    AuditoriumId = 2
                }
            };

            var showtimeDTOs = new List<ShowtimeDTO>
            {
                new ShowtimeDTO
                {
                    Id = 1,
                    Movie = new MovieDTO { Title = "The Shawshank Redemption" },
                    AuditoriumId = 1,
                    SessionDate = showtimeEntities[0].SessionDate
                },
                new ShowtimeDTO
                {
                    Id = 2,
                    Movie = new MovieDTO { Title = "The Godfather" },
                    AuditoriumId = 2,
                    SessionDate = showtimeEntities[1].SessionDate
                }
            };

            _showtimesRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(showtimeEntities);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<ShowtimeDTO>>(It.IsAny<IEnumerable<ShowtimeEntity>>()))
                .Returns(showtimeDTOs);

            // Act
            var result = await _service.GetAll(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count()); // Verifying that two showtimes are returned
            Assert.Contains(result, dto => dto.Id == 1 && dto.Movie.Title == "The Shawshank Redemption");
            Assert.Contains(result, dto => dto.Id == 2 && dto.Movie.Title == "The Godfather");
        }

        [Fact]
        public async Task GetAll_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _showtimesRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ShowtimeEntity>()); // Empty list

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<ShowtimeDTO>>(It.IsAny<IEnumerable<ShowtimeEntity>>()))
                .Returns(new List<ShowtimeDTO>());

            // Act
            var result = await _service.GetAll(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}