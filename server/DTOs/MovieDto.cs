namespace server.DTOs;

public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Rating { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
}

public class MovieDetailDto : MovieDto
{
    public List<ShowtimeDto> Showtimes { get; set; } = new();
}

public class CreateMovieDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Rating { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
}
