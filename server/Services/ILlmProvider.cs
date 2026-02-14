namespace server.Services;

/// <summary>
/// Interface for LLM (Large Language Model) providers
/// </summary>
public interface ILlmProvider
{
    string ProviderName { get; }
    Task<string> GenerateResponseAsync(string systemPrompt, string userMessage, CancellationToken ct = default);
}
