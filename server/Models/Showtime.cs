namespace server.Models;

public class Showtime
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public DateTime StartTime { get; set; }
    public string Theater { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }

    public Movie Movie { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
