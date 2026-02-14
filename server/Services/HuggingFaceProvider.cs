using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace server.Services;

/// <summary>
/// Configuration options for HuggingFace provider
/// </summary>
public class HuggingFaceOptions
{
    public const string SectionName = "LLM:HuggingFace";
    
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "TinyLlama/TinyLlama-1.1B-Chat-v1.0";
    public double Temperature { get; set; } = 0.7;
    public int MaxTokens { get; set; } = 150;
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// HuggingFace LLM provider implementation
/// </summary>
public class HuggingFaceProvider : ILlmProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HuggingFaceProvider> _logger;
    private readonly HuggingFaceOptions _options;

    public string ProviderName => "HuggingFace";

    public HuggingFaceProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<HuggingFaceProvider> logger,
        IOptions<HuggingFaceOptions> options)
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
            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                throw new InvalidOperationException("HuggingFace API key not configured");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
            client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            var fullPrompt = $"{systemPrompt}\n\nUser: {userMessage}\nAssistant:";

            var requestBody = new
            {
                inputs = fullPrompt,
                parameters = new
                {
                    max_new_tokens = _options.MaxTokens,
                    temperature = _options.Temperature,
                    top_p = 0.95,
                    do_sample = true,
                    return_full_text = false
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                $"https://api-inference.huggingface.co/models/{_options.Model}",
                content,
                ct
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("HuggingFace API error: {Error}", error);
                throw new HttpRequestException($"HuggingFace API returned {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<List<JsonElement>>(responseJson);

            if (result != null && result.Count > 0 && 
                result[0].TryGetProperty("generated_text", out var generatedText))
            {
                var text = generatedText.GetString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(text))
                    throw new InvalidOperationException("HuggingFace returned empty response");
                
                return text;
            }

            throw new InvalidOperationException("Invalid response format from HuggingFace");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling HuggingFace API");
            throw;
        }
    }
}
