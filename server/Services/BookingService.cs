using System.Security.Cryptography;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.DTOs;
using server.Models;

namespace server.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public BookingService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BookingResponseDto> CreateBookingAsync(
        string userId, CreateBookingDto dto, CancellationToken ct = default)
    {
        var showtime = await _context.Showtimes
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.Id == dto.ShowtimeId, ct)
            ?? throw new InvalidOperationException("Showtime not found.");

        if (showtime.StartTime <= DateTime.UtcNow)
            throw new InvalidOperationException("This showtime has already passed.");

        if (showtime.AvailableSeats < dto.SeatsBooked)
            throw new InvalidOperationException($"Only {showtime.AvailableSeats} seats available.");

        showtime.AvailableSeats -= dto.SeatsBooked;

        var booking = new Booking
        {
            UserId = userId,
            ShowtimeId = dto.ShowtimeId,
            SeatsBooked = dto.SeatsBooked,
            TotalPrice = showtime.Price * dto.SeatsBooked,
            BookingCode = GenerateBookingCode(),
            Status = BookingStatus.Confirmed
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(ct);

        // Reload with navigation for mapping
        var result = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .FirstAsync(b => b.Id == booking.Id, ct);

        return _mapper.Map<BookingResponseDto>(result);
    }

    public async Task<List<BookingResponseDto>> GetUserBookingsAsync(
        string userId, CancellationToken ct = default)
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Showtime)
                .ThenInclude(s => s.Movie)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        return _mapper.Map<List<BookingResponseDto>>(bookings);
    }

    public async Task<bool> CancelBookingAsync(
        int bookingId, string userId, CancellationToken ct = default)
    {
        var booking = await _context.Bookings
            .Include(b => b.Showtime)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId, ct);

        if (booking is null) return false;
        if (booking.Status == BookingStatus.Cancelled) return false;

        booking.Status = BookingStatus.Cancelled;
        booking.Showtime.AvailableSeats += booking.SeatsBooked;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    private static string GenerateBookingCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(6);
        return $"BK-{Convert.ToHexString(bytes).ToUpper()}";
    }
}
