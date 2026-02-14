namespace server.Services;

/// <summary>
/// Interface for retrieving database context to enhance LLM responses
/// </summary>
public interface IDatabaseContextService
{
    /// <summary>
    /// Gets formatted database context including movies, showtimes, and user bookings
    /// </summary>
    Task<string> GetDatabaseContextAsync(string userId, CancellationToken ct = default);
}
