using server.DTOs;
using server.Models;

namespace server.Services;

public interface IMovieService
{
    Task<List<MovieDto>> GetAllAsync(CancellationToken ct = default);
    Task<MovieDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);
}
