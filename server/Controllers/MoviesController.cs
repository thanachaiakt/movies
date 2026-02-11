using Microsoft.AspNetCore.Mvc;
using server.DTOs;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> GetAll(CancellationToken ct)
    {
        var movies = await _movieService.GetAllAsync(ct);
        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDetailDto>> GetById(int id, CancellationToken ct)
    {
        var movie = await _movieService.GetByIdAsync(id, ct);
        if (movie is null) return NotFound();
        return Ok(movie);
    }
}
