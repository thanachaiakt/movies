namespace server.Models;

public enum BookingStatus
{
    Confirmed,
    Cancelled
}

public class Booking
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ShowtimeId { get; set; }
    public int SeatsBooked { get; set; }
    public decimal TotalPrice { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Showtime Showtime { get; set; } = null!;
}
