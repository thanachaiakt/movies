using Microsoft.EntityFrameworkCore;
using server.Data;
using System.Text;

namespace server.Services;

/// <summary>
/// Service that provides database context for LLM chat responses
/// Fetches movies, showtimes, and bookings to give the LLM accurate data
/// </summary>
public class DatabaseContextService : IDatabaseContextService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseContextService> _logger;

    public DatabaseContextService(AppDbContext context, ILogger<DatabaseContextService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetDatabaseContextAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== AVAILABLE DATA IN DATABASE ===");
            sb.AppendLine();

            // Get all movies with their showtimes
            var movies = await _context.Movies
                .Include(m => m.Showtimes)
                .OrderBy(m => m.Title)
                .ToListAsync(ct);

            sb.AppendLine("MOVIES AND SHOWTIMES:");
            if (movies.Any())
            {
                foreach (var movie in movies)
                {
                    sb.AppendLine($"- Movie: {movie.Title}");
                    sb.AppendLine($"  Genre: {movie.Genre}");
                    sb.AppendLine($"  Rating: {movie.Rating}");
                    sb.AppendLine($"  Duration: {movie.DurationMinutes} minutes");
                    sb.AppendLine($"  Description: {movie.Description}");
                    sb.AppendLine($"  Release Date: {movie.ReleaseDate:yyyy-MM-dd}");
                    
                    if (movie.Showtimes.Any())
                    {
                        sb.AppendLine($"  Showtimes:");
                        foreach (var showtime in movie.Showtimes.OrderBy(s => s.StartTime))
                        {
                            sb.AppendLine($"    - Theater: {showtime.Theater}");
                            sb.AppendLine($"      Time: {showtime.StartTime:yyyy-MM-dd HH:mm}");
                            sb.AppendLine($"      Price: ${showtime.Price}");
                            sb.AppendLine($"      Available Seats: {showtime.AvailableSeats}/{showtime.TotalSeats}");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"  No showtimes available");
                    }
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("No movies available in database.");
                sb.AppendLine();
            }

            // Get user's bookings if userId is provided
            if (!string.IsNullOrEmpty(userId))
            {
                var userBookings = await _context.Bookings
                    .Include(b => b.Showtime)
                        .ThenInclude(s => s.Movie)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync(ct);

                sb.AppendLine("USER'S BOOKINGS:");
                if (userBookings.Any())
                {
                    foreach (var booking in userBookings)
                    {
                        sb.AppendLine($"- Booking Code: {booking.BookingCode}");
                        sb.AppendLine($"  Movie: {booking.Showtime?.Movie?.Title ?? "Unknown"}");
                        sb.AppendLine($"  Theater: {booking.Showtime?.Theater ?? "Unknown"}");
                        sb.AppendLine($"  Showtime: {booking.Showtime?.StartTime:yyyy-MM-dd HH:mm}");
                        sb.AppendLine($"  Seats Booked: {booking.SeatsBooked}");
                        sb.AppendLine($"  Total Price: ${booking.TotalPrice}");
                        sb.AppendLine($"  Status: {booking.Status}");
                        sb.AppendLine($"  Booked At: {booking.CreatedAt:yyyy-MM-dd HH:mm}");
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("User has no bookings yet.");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("=== END OF DATABASE DATA ===");
            
            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving database context for user {UserId}", userId);
            return "I am temporarily unable to access the database, so I can't retrieve movie or booking information right now. Please try again in a moment.";
        }
    }
}
