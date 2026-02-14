using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.DTOs;
using server.Services;
using System.Security.Claims;

namespace server.Controllers;

/// <summary>
/// Controller for chat-related operations following best practices
/// - Uses Result pattern for error handling
/// - Proper async/await with CancellationToken
/// - Clear separation of concerns
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("message")]
    [Authorize]
    public async Task<ActionResult<ChatResponseDto>> SendMessage(
        [FromBody] ChatMessageDto messageDto, 
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        var result = await _chatService.GetChatResponseAsync(userId, messageDto.Message, ct);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<ChatHistoryDto>>> GetHistory(CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        var result = await _chatService.GetChatHistoryAsync(userId, ct);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to get chat history for user {UserId}: {Error}", userId, result.Error);
            return StatusCode(500, new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(result.Value);
    }
}
