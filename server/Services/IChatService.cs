using server.Common;
using server.DTOs;

namespace server.Services;

/// <summary>
/// Service interface for chat operations
/// </summary>
public interface IChatService
{
    Task<Result<ChatResponseDto>> GetChatResponseAsync(string userId, string message, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ChatHistoryDto>>> GetChatHistoryAsync(string userId, CancellationToken ct = default);
}
