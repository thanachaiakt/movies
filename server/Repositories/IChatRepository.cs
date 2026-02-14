using server.Models;

namespace server.Repositories;

/// <summary>
/// Repository interface for chat message data access
/// </summary>
public interface IChatRepository
{
    Task<ChatMessage> CreateAsync(ChatMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<ChatMessage>> GetHistoryAsync(string userId, int take = 50, CancellationToken ct = default);
    Task<ChatMessage?> GetByIdAsync(int id, CancellationToken ct = default);
}
