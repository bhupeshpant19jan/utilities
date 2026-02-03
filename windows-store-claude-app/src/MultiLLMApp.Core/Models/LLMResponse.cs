namespace MultiLLMApp.Core.Models;

/// <summary>
/// Represents a complete response from an LLM provider.
/// </summary>
public sealed class LLMResponse
{
    /// <summary>
    /// The generated content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// The model that generated this response.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Number of tokens in the input/prompt.
    /// </summary>
    public int InputTokens { get; init; }

    /// <summary>
    /// Number of tokens in the output/response.
    /// </summary>
    public int OutputTokens { get; init; }

    /// <summary>
    /// Total tokens used (input + output).
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;

    /// <summary>
    /// The reason the response ended.
    /// </summary>
    public StopReason StopReason { get; init; }

    /// <summary>
    /// Unique ID for this response from the provider.
    /// </summary>
    public string? ResponseId { get; init; }

    /// <summary>
    /// When the response was received.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Reason the LLM stopped generating.
/// </summary>
public enum StopReason
{
    /// <summary>Natural end of response.</summary>
    EndTurn,
    /// <summary>Hit max tokens limit.</summary>
    MaxTokens,
    /// <summary>Hit a stop sequence.</summary>
    StopSequence,
    /// <summary>Request was cancelled.</summary>
    Cancelled,
    /// <summary>Unknown or error.</summary>
    Unknown
}

/// <summary>
/// Represents a single chunk during streaming.
/// </summary>
public sealed class StreamChunk
{
    /// <summary>
    /// The text content of this chunk.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Whether this is the final chunk.
    /// </summary>
    public bool IsFinal { get; init; }

    /// <summary>
    /// Token usage (only populated on final chunk).
    /// </summary>
    public TokenUsage? Usage { get; init; }

    /// <summary>
    /// Stop reason (only populated on final chunk).
    /// </summary>
    public StopReason? StopReason { get; init; }

    /// <summary>
    /// Creates a text chunk.
    /// </summary>
    public static StreamChunk Text(string text) => new() { Text = text };

    /// <summary>
    /// Creates the final chunk with usage info.
    /// </summary>
    public static StreamChunk Final(TokenUsage usage, StopReason reason) => new()
    {
        Text = string.Empty,
        IsFinal = true,
        Usage = usage,
        StopReason = reason
    };
}

/// <summary>
/// Token usage information.
/// </summary>
public sealed record TokenUsage(int InputTokens, int OutputTokens)
{
    public int TotalTokens => InputTokens + OutputTokens;
}
