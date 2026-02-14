namespace server.Services;

/// <summary>
/// Configuration options for the Chat service
/// </summary>
public class ChatServiceOptions
{
    public const string SectionName = "LLM";
    
    public string Provider { get; set; } = "fallback";
    public int MaxHistoryItems { get; set; } = 50;
    public string SystemPrompt { get; set; } = @"You are a helpful movie booking assistant. You can help users with:
- Finding movies and showtimes
- Booking tickets
- Checking their bookings
- General movie information
Keep responses concise and helpful.";
}
