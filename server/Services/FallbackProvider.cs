namespace server.Services;

/// <summary>
/// Fallback LLM provider with predefined responses
/// </summary>
public class FallbackProvider : ILlmProvider
{
    public string ProviderName => "Fallback";

    public Task<string> GenerateResponseAsync(
        string systemPrompt, 
        string userMessage, 
        CancellationToken ct = default)
    {
        var lowerMessage = userMessage.ToLower();
        
        var response = lowerMessage switch
        {
            string s when s.Contains("movie") || s.Contains("film") =>
                "I can help you find movies and showtimes! Check out our Movies page to see what's playing.",
            
            string s when s.Contains("book") || s.Contains("ticket") =>
                "To book a ticket, browse our Movies page, select a showtime, and click Book Now!",
            
            string s when s.Contains("booking") || s.Contains("reservation") =>
                "You can view your bookings in the My Bookings section. Each booking has a unique booking code.",
            
            string s when s.Contains("help") || s.Contains("how") =>
                "I'm here to help! I can assist you with finding movies, booking tickets, and managing your reservations.",
            
            _ => "Hello! I'm your movie booking assistant. How can I help you today?"
        };

        return Task.FromResult(response);
    }
}
