namespace server.DTOs;

/// <summary>
/// DTO for sending a chat message
/// </summary>
public class ChatMessageDto
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// DTO for chat response
/// </summary>
public class ChatResponseDto
{
    public string Response { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}

/// <summary>
/// DTO for chat history item with both user message and assistant response
/// </summary>
public class ChatHistoryDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
