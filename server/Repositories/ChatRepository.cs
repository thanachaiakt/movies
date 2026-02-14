using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Repositories;

/// <summary>
/// Repository implementation for chat message data access using EF Core
/// </summary>
public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ChatMessage> CreateAsync(ChatMessage message, CancellationToken ct = default)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(ct);
        return message;
    }

    public async Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(
        string userId, 
        int take = 50, 
        CancellationToken ct = default)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<ChatMessage?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
