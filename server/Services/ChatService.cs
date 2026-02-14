using Microsoft.Extensions.Options;
using server.Common;
using server.DTOs;
using server.Models;
using server.Repositories;

namespace server.Services;

/// <summary>
/// Chat service implementation following best practices
/// - Separated concerns (repository for data, providers for LLM)
/// - IOptions pattern for configuration
/// - Result pattern for error handling
/// - Async all the way down with CancellationToken support
/// </summary>
public class ChatService : IChatService
{
    private readonly IChatRepository _repository;
    private readonly IDatabaseContextService _databaseContext;
    private readonly ILlmProvider _ollamaProvider;
    private readonly ILlmProvider _huggingFaceProvider;
    private readonly ILlmProvider _fallbackProvider;
    private readonly ILogger<ChatService> _logger;
    private readonly ChatServiceOptions _options;

    public ChatService(
        IChatRepository repository,
        IDatabaseContextService databaseContext,
        OllamaProvider ollamaProvider,
        HuggingFaceProvider huggingFaceProvider,
        FallbackProvider fallbackProvider,
        ILogger<ChatService> logger,
        IOptions<ChatServiceOptions> options)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        _ollamaProvider = ollamaProvider ?? throw new ArgumentNullException(nameof(ollamaProvider));
        _huggingFaceProvider = huggingFaceProvider ?? throw new ArgumentNullException(nameof(huggingFaceProvider));
        _fallbackProvider = fallbackProvider ?? throw new ArgumentNullException(nameof(fallbackProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<Result<ChatResponseDto>> GetChatResponseAsync(
        string userId, 
        string message, 
        CancellationToken ct = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(userId))
            return Result<ChatResponseDto>.Failure("User ID is required", "INVALID_USER_ID");

        if (string.IsNullOrWhiteSpace(message))
            return Result<ChatResponseDto>.Failure("Message cannot be empty", "EMPTY_MESSAGE");

        try
        {
            // Get database context
            var dbContext = await _databaseContext.GetDatabaseContextAsync(userId, ct);
            
            // Select provider and generate response with database context
            var (response, providerName) = await GenerateResponseWithFallbackAsync(userId, message, dbContext, ct);

            // Save to database via repository
            var chatMessage = new ChatMessage
            {
                UserId = userId,
                Message = message,
                Response = response,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(chatMessage, ct);

            return Result<ChatResponseDto>.Success(new ChatResponseDto
            {
                Response = response,
                Model = providerName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat response for user {UserId}", userId);
            
            // Return fallback result on error
            return Result<ChatResponseDto>.Success(new ChatResponseDto
            {
                Response = "I'm having trouble connecting right now. Please try asking about movies, showtimes, or bookings!",
                Model = "fallback-error"
            });
        }
    }

    /// <summary>
    /// Generates response with automatic fallback on provider failure
    /// </summary>
    private async Task<(string Response, string ProviderName)> GenerateResponseWithFallbackAsync(
        string userId,
        string message,
        string databaseContext,
        CancellationToken ct)
    {
        var provider = _options.Provider.ToLower();
        var baseSystemPrompt = _options.SystemPrompt;
        
        // Enhance system prompt with database context
        var enhancedSystemPrompt = $@"{baseSystemPrompt}

{databaseContext}

IMPORTANT INSTRUCTIONS:
- Answer questions ONLY based on the database data provided above
- If the user asks about movies, showtimes, or bookings, use ONLY the data shown above
- If the information doesn't exist in the database data, say ""I don't have that information in our database""
- Be specific and accurate - use exact titles, times, prices, and theater names from the database
- For date/time questions, use the exact dates and times from the database
- Do not make up or assume any information not present in the database data
- The Movies data is from table Movies
- The Showtimes data is from table Showtimes
- The Booking data is from table Booking
- Keep responses concise and helpful";

        // Try primary provider
        try
        {
            if (provider == "ollama")
            {
                var response = await _ollamaProvider.GenerateResponseAsync(enhancedSystemPrompt, message, ct);
                return (response, _ollamaProvider.ProviderName);
            }
            else if (provider == "huggingface")
            {
                var response = await _huggingFaceProvider.GenerateResponseAsync(enhancedSystemPrompt, message, ct);
                return (response, _huggingFaceProvider.ProviderName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary provider {Provider} failed, falling back", provider);
        }

        // Fallback provider always succeeds
        var fallbackResponse = await _fallbackProvider.GenerateResponseAsync(enhancedSystemPrompt, message, ct);
        return (fallbackResponse, _fallbackProvider.ProviderName);
    }

    public async Task<Result<IReadOnlyList<ChatHistoryDto>>> GetChatHistoryAsync(
        string userId, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<IReadOnlyList<ChatHistoryDto>>.Failure("User ID is required", "INVALID_USER_ID");

        try
        {
            var messages = await _repository.GetHistoryAsync(userId, _options.MaxHistoryItems, ct);

            var historyDtos = messages
                .Select(m => new ChatHistoryDto
                {
                    Id = m.Id,
                    Message = m.Message,
                    Response = m.Response,
                    CreatedAt = m.CreatedAt
                })
                .ToList();

            return Result<IReadOnlyList<ChatHistoryDto>>.Success(historyDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat history for user {UserId}", userId);
            return Result<IReadOnlyList<ChatHistoryDto>>.Failure(
                "Failed to retrieve chat history", 
                "HISTORY_ERROR");
        }
    }
}