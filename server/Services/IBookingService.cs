using server.DTOs;
using server.Models;

namespace server.Services;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(string userId, CreateBookingDto dto, CancellationToken ct = default);
    Task<List<BookingResponseDto>> GetUserBookingsAsync(string userId, CancellationToken ct = default);
    Task<bool> CancelBookingAsync(int bookingId, string userId, CancellationToken ct = default);
}
