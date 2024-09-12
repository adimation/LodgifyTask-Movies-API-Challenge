using ApiApplication.DTOs.ShowtimeDTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Services.Abstractions
{
    public interface IShowtimeService
    {
        Task<ShowtimeDTO> Create(CreateShowtimeDTO model, CancellationToken cancellationToken);

        Task<IEnumerable<ShowtimeDTO>> GetAll(CancellationToken cancellationToken);

        Task<ShowtimeDTO> GetById(int id, CancellationToken cancellationToken);
    }
}
