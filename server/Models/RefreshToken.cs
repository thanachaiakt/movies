namespace server.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    // Activity-based extensions
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public int ActivityExtensionCount { get; set; } = 0;
    public int MaxActivityExtensions { get; set; } = 10; // Allow 10 extensions (70 days total if each extends 7 days)
    
    // Refresh limiting
    public int RefreshCount { get; set; } = 0;
    public int MaxRefreshCount { get; set; } = 672; // 7 days * 24 hours * 4 refreshes/hour = 672
}
