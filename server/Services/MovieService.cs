using AutoMapper;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs;
using server.Models;

namespace server.Services;

public class MovieService : IMovieService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public MovieService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MovieDto>> GetAllAsync(CancellationToken ct = default)
    {
        var movies = await _context.Movies
            .AsNoTracking()
            .OrderByDescending(m => m.ReleaseDate)
            .ToListAsync(ct);

        return _mapper.Map<List<MovieDto>>(movies);
    }

    public async Task<MovieDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var movie = await _context.Movies
            .AsNoTracking()
            .Include(m => m.Showtimes.Where(s => s.StartTime > DateTime.UtcNow && s.AvailableSeats > 0))
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (movie is null) return null;

        return _mapper.Map<MovieDetailDto>(movie);
    }
}
