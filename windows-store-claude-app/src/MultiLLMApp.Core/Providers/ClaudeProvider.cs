using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Providers;

/// <summary>
/// Claude (Anthropic) provider implementation.
/// </summary>
public sealed class ClaudeProvider : BaseProvider
{
    private const string ApiBaseUrl = "https://api.anthropic.com/v1";
    private const string ApiVersion = "2023-06-01";

    private static readonly IReadOnlyList<ModelInfo> Models =
    [
        new ModelInfo
        {
            ModelId = "claude-3-haiku-20240307",
            DisplayName = "Claude 3 Haiku",
            MaxContextTokens = 200000,
            MaxOutputTokens = 4096,
            InputTokenCostPer1M = 0.25m,
            OutputTokenCostPer1M = 1.25m,
            IsDefault = true,
            Tier = "fast"
        },
        new ModelInfo
        {
            ModelId = "claude-3-5-sonnet-20241022",
            DisplayName = "Claude 3.5 Sonnet",
            MaxContextTokens = 200000,
            MaxOutputTokens = 8192,
            InputTokenCostPer1M = 3.00m,
            OutputTokenCostPer1M = 15.00m,
            Tier = "balanced"
        },
        new ModelInfo
        {
            ModelId = "claude-sonnet-4-20250514",
            DisplayName = "Claude Sonnet 4",
            MaxContextTokens = 200000,
            MaxOutputTokens = 16384,
            InputTokenCostPer1M = 3.00m,
            OutputTokenCostPer1M = 15.00m,
            Tier = "balanced"
        },
        new ModelInfo
        {
            ModelId = "claude-opus-4-20250514",
            DisplayName = "Claude Opus 4",
            MaxContextTokens = 200000,
            MaxOutputTokens = 16384,
            InputTokenCostPer1M = 15.00m,
            OutputTokenCostPer1M = 75.00m,
            Tier = "powerful"
        }
    ];

    public override string ProviderId => "claude";
    public override string DisplayName => "Claude";

    public ClaudeProvider(string apiKey, HttpClient? httpClient = null)
        : base(apiKey, httpClient)
    {
        HttpClient.BaseAddress = new Uri(ApiBaseUrl);
        HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        HttpClient.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);
    }

    public override IReadOnlyList<ModelInfo> GetAvailableModels() => Models;

    public override async Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", ApiVersion);

            var request = new ClaudeRequest
            {
                Model = "claude-3-haiku-20240307",
                MaxTokens = 1,
                Messages = [new ClaudeMessage { Role = "user", Content = "hi" }]
            };

            var response = await client.PostAsJsonAsync("/messages", request, ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public override async Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken ct = default)
    {
        var claudeRequest = BuildRequest(request, stream: false);
        var response = await HttpClient.PostAsJsonAsync("/messages", claudeRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw HandleHttpError(response, errorBody);
        }

        var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>(ct)
            ?? throw new InvalidResponseException(ProviderId, "Empty response");

        return new LLMResponse
        {
            Content = result.Content?.FirstOrDefault()?.Text ?? string.Empty,
            ModelId = result.Model ?? request.ModelId,
            InputTokens = result.Usage?.InputTokens ?? 0,
            OutputTokens = result.Usage?.OutputTokens ?? 0,
            StopReason = MapStopReason(result.StopReason),
            ResponseId = result.Id
        };
    }

    public override async IAsyncEnumerable<StreamChunk> StreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var claudeRequest = BuildRequest(request, stream: true);
        var json = JsonSerializer.Serialize(claudeRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/messages")
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
        int inputTokens = 0, outputTokens = 0;
        StopReason stopReason = StopReason.Unknown;

        await foreach (var line in ReadSSELinesAsync(stream, ct))
        {
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                continue;

            var data = line[6..]; // Remove "data: " prefix
            if (data == "[DONE]")
                break;

            var evt = JsonSerializer.Deserialize<ClaudeStreamEvent>(data);
            if (evt == null) continue;

            switch (evt.Type)
            {
                case "content_block_delta":
                    if (evt.Delta?.Text != null)
                    {
                        fullText.Append(evt.Delta.Text);
                        yield return StreamChunk.Text(evt.Delta.Text);
                    }
                    break;

                case "message_delta":
                    if (evt.Usage != null)
                    {
                        outputTokens = evt.Usage.OutputTokens;
                    }
                    if (evt.Delta?.StopReason != null)
                    {
                        stopReason = MapStopReason(evt.Delta.StopReason);
                    }
                    break;

                case "message_start":
                    if (evt.Message?.Usage != null)
                    {
                        inputTokens = evt.Message.Usage.InputTokens;
                    }
                    break;
            }
        }

        yield return StreamChunk.Final(new TokenUsage(inputTokens, outputTokens), stopReason);
    }

    private static ClaudeRequest BuildRequest(LLMRequest request, bool stream)
    {
        var messages = request.Messages
            .Where(m => m.Role != MessageRole.System)
            .Select(m => new ClaudeMessage
            {
                Role = m.Role == MessageRole.User ? "user" : "assistant",
                Content = m.Content
            })
            .ToList();

        return new ClaudeRequest
        {
            Model = request.ModelId,
            MaxTokens = request.MaxTokens,
            Messages = messages,
            System = request.SystemPrompt,
            Stream = stream,
            Temperature = request.Temperature,
            StopSequences = request.StopSequences?.ToList()
        };
    }

    private static StopReason MapStopReason(string? reason) => reason switch
    {
        "end_turn" => StopReason.EndTurn,
        "max_tokens" => StopReason.MaxTokens,
        "stop_sequence" => StopReason.StopSequence,
        _ => StopReason.Unknown
    };

    #region Claude API Models

    private sealed class ClaudeRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("messages")]
        public required List<ClaudeMessage> Messages { get; set; }

        [JsonPropertyName("system")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? System { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double Temperature { get; set; }

        [JsonPropertyName("stop_sequences")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? StopSequences { get; set; }
    }

    private sealed class ClaudeMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; set; }

        [JsonPropertyName("content")]
        public required string Content { get; set; }
    }

    private sealed class ClaudeResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("content")]
        public List<ContentBlock>? Content { get; set; }

        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }

        [JsonPropertyName("usage")]
        public UsageInfo? Usage { get; set; }
    }

    private sealed class ContentBlock
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class UsageInfo
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }

    private sealed class ClaudeStreamEvent
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("delta")]
        public DeltaInfo? Delta { get; set; }

        [JsonPropertyName("usage")]
        public UsageInfo? Usage { get; set; }

        [JsonPropertyName("message")]
        public MessageInfo? Message { get; set; }
    }

    private sealed class DeltaInfo
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }
    }

    private sealed class MessageInfo
    {
        [JsonPropertyName("usage")]
        public UsageInfo? Usage { get; set; }
    }

    #endregion
}
