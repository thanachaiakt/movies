using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace server.Services;

/// <summary>
/// Configuration options for Ollama provider
/// </summary>
public class OllamaOptions
{
    public const string SectionName = "LLM:Ollama";
    
    public string Url { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.2";
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 150;
    public int TimeoutSeconds { get; set; } = 60;
}

/// <summary>
/// Ollama LLM provider implementation
/// </summary>
public class OllamaProvider : ILlmProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OllamaProvider> _logger;
    private readonly OllamaOptions _options;

    public string ProviderName => "Ollama";

    public OllamaProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<OllamaProvider> logger,
        IOptions<OllamaOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<string> GenerateResponseAsync(
        string systemPrompt, 
        string userMessage, 
        CancellationToken ct = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            var requestBody = new
            {
                model = _options.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                stream = false,
                options = new
                {
                    temperature = _options.Temperature,
                    num_predict = _options.MaxTokens
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_options.Url}/api/chat", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Ollama API error: {Error}", error);
                throw new HttpRequestException($"Ollama API returned {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            if (result.TryGetProperty("message", out var messageObj) && 
                messageObj.TryGetProperty("content", out var contentProp))
            {
                var text = contentProp.GetString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(text))
                    throw new InvalidOperationException("Ollama returned empty response");
                
                return text;
            }

            throw new InvalidOperationException("Invalid response format from Ollama");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API");
            throw;
        }
    }
}
