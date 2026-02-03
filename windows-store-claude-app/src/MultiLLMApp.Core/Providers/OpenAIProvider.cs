using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Providers;

/// <summary>
/// OpenAI (ChatGPT) provider implementation.
/// </summary>
public sealed class OpenAIProvider : BaseProvider
{
    private const string ApiBaseUrl = "https://api.openai.com/v1";

    private static readonly IReadOnlyList<ModelInfo> Models =
    [
        new ModelInfo
        {
            ModelId = "gpt-4o-mini",
            DisplayName = "GPT-4o Mini",
            MaxContextTokens = 128000,
            MaxOutputTokens = 16384,
            InputTokenCostPer1M = 0.15m,
            OutputTokenCostPer1M = 0.60m,
            IsDefault = true,
            Tier = "fast"
        },
        new ModelInfo
        {
            ModelId = "gpt-4o",
            DisplayName = "GPT-4o",
            MaxContextTokens = 128000,
            MaxOutputTokens = 16384,
            InputTokenCostPer1M = 2.50m,
            OutputTokenCostPer1M = 10.00m,
            Tier = "balanced"
        },
        new ModelInfo
        {
            ModelId = "gpt-4-turbo",
            DisplayName = "GPT-4 Turbo",
            MaxContextTokens = 128000,
            MaxOutputTokens = 4096,
            InputTokenCostPer1M = 10.00m,
            OutputTokenCostPer1M = 30.00m,
            Tier = "powerful"
        },
        new ModelInfo
        {
            ModelId = "o1",
            DisplayName = "o1",
            MaxContextTokens = 200000,
            MaxOutputTokens = 100000,
            InputTokenCostPer1M = 15.00m,
            OutputTokenCostPer1M = 60.00m,
            Tier = "reasoning"
        }
    ];

    public override string ProviderId => "openai";
    public override string DisplayName => "ChatGPT";

    public OpenAIProvider(string apiKey, HttpClient? httpClient = null)
        : base(apiKey, httpClient)
    {
        HttpClient.BaseAddress = new Uri(ApiBaseUrl);
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public override IReadOnlyList<ModelInfo> GetAvailableModels() => Models;

    public override async Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.GetAsync("/models", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public override async Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken ct = default)
    {
        var openaiRequest = BuildRequest(request, stream: false);
        var response = await HttpClient.PostAsJsonAsync("/chat/completions", openaiRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw HandleHttpError(response, errorBody);
        }

        var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(ct)
            ?? throw new InvalidResponseException(ProviderId, "Empty response");

        var choice = result.Choices?.FirstOrDefault();

        return new LLMResponse
        {
            Content = choice?.Message?.Content ?? string.Empty,
            ModelId = result.Model ?? request.ModelId,
            InputTokens = result.Usage?.PromptTokens ?? 0,
            OutputTokens = result.Usage?.CompletionTokens ?? 0,
            StopReason = MapStopReason(choice?.FinishReason),
            ResponseId = result.Id
        };
    }

    public override async IAsyncEnumerable<StreamChunk> StreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var openaiRequest = BuildRequest(request, stream: true);
        var json = JsonSerializer.Serialize(openaiRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/chat/completions")
        {
            Content = content
        };

        using var response = await HttpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw HandleHttpError(response, errorBody);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);

        var fullText = new StringBuilder();
        StopReason stopReason = StopReason.Unknown;
        int inputTokens = EstimateRequestTokens(request); // Estimate since OpenAI doesn't provide in stream
        int outputTokens = 0;

        await foreach (var line in ReadSSELinesAsync(stream, ct))
        {
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                continue;

            var data = line[6..];
            if (data == "[DONE]")
                break;

            var evt = JsonSerializer.Deserialize<OpenAIStreamResponse>(data);
            var choice = evt?.Choices?.FirstOrDefault();
            if (choice == null) continue;

            if (choice.Delta?.Content != null)
            {
                var text = choice.Delta.Content;
                fullText.Append(text);
                outputTokens += EstimateTokens(text);
                yield return StreamChunk.Text(text);
            }

            if (choice.FinishReason != null)
            {
                stopReason = MapStopReason(choice.FinishReason);
            }
        }

        yield return StreamChunk.Final(new TokenUsage(inputTokens, outputTokens), stopReason);
    }

    private static OpenAIRequest BuildRequest(LLMRequest request, bool stream)
    {
        var messages = new List<OpenAIMessage>();

        // Add system prompt if present
        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new OpenAIMessage { Role = "system", Content = request.SystemPrompt });
        }

        // Add conversation messages
        messages.AddRange(request.Messages.Select(m => new OpenAIMessage
        {
            Role = m.Role switch
            {
                MessageRole.System => "system",
                MessageRole.User => "user",
                MessageRole.Assistant => "assistant",
                _ => "user"
            },
            Content = m.Content
        }));

        return new OpenAIRequest
        {
            Model = request.ModelId,
            Messages = messages,
            MaxCompletionTokens = request.MaxTokens,
            Stream = stream,
            Temperature = request.Temperature,
            Stop = request.StopSequences?.ToList()
        };
    }

    private static StopReason MapStopReason(string? reason) => reason switch
    {
        "stop" => StopReason.EndTurn,
        "length" => StopReason.MaxTokens,
        "content_filter" => StopReason.StopSequence,
        _ => StopReason.Unknown
    };

    #region OpenAI API Models

    private sealed class OpenAIRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; set; }

        [JsonPropertyName("messages")]
        public required List<OpenAIMessage> Messages { get; set; }

        [JsonPropertyName("max_completion_tokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int MaxCompletionTokens { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double Temperature { get; set; }

        [JsonPropertyName("stop")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Stop { get; set; }
    }

    private sealed class OpenAIMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }

    private sealed class OpenAIResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")]
        public OpenAIMessage? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        [JsonPropertyName("delta")]
        public DeltaContent? Delta { get; set; }
    }

    private sealed class DeltaContent
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
    }

    private sealed class OpenAIStreamResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }

    #endregion
}
