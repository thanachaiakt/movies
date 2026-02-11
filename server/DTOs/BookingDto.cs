using System.ComponentModel.DataAnnotations;

namespace server.DTOs;

public class CreateBookingDto
{
    [Required]
    public int ShowtimeId { get; set; }

    [Required]
    [Range(1, 10, ErrorMessage = "You can book between 1 and 10 seats.")]
    public int SeatsBooked { get; set; }
}

public class BookingResponseDto
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string Theater { get; set; } = string.Empty;
    public DateTime ShowTime { get; set; }
    public int SeatsBooked { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
