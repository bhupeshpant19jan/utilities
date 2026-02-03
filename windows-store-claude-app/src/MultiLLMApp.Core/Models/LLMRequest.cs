namespace MultiLLMApp.Core.Models;

/// <summary>
/// Represents a request to an LLM provider.
/// </summary>
public sealed class LLMRequest
{
    /// <summary>
    /// The model to use for this request.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// The conversation messages to send.
    /// </summary>
    public required IReadOnlyList<Message> Messages { get; init; }

    /// <summary>
    /// Maximum tokens to generate in the response.
    /// </summary>
    public int MaxTokens { get; init; } = 1024;

    /// <summary>
    /// Optional system prompt to prepend.
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// Temperature for response randomness (0.0 - 1.0).
    /// </summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>
    /// Whether to stream the response.
    /// </summary>
    public bool Stream { get; init; } = true;

    /// <summary>
    /// Optional stop sequences.
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; init; }

    /// <summary>
    /// Creates a simple single-message request.
    /// </summary>
    public static LLMRequest Create(string modelId, string userMessage, int maxTokens = 1024) => new()
    {
        ModelId = modelId,
        Messages = [Message.User(userMessage)],
        MaxTokens = maxTokens
    };

    /// <summary>
    /// Creates a request with conversation context.
    /// </summary>
    public static LLMRequest WithContext(
        string modelId,
        IReadOnlyList<Message> context,
        string userMessage,
        int maxTokens = 1024) => new()
    {
        ModelId = modelId,
        Messages = [..context, Message.User(userMessage)],
        MaxTokens = maxTokens
    };
}
