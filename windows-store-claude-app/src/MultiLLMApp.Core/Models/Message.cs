namespace MultiLLMApp.Core.Models;

/// <summary>
/// Represents a single message in a conversation.
/// </summary>
public sealed class Message
{
    /// <summary>
    /// Unique identifier for this message.
    /// </summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The role of the message sender.
    /// </summary>
    public MessageRole Role { get; init; }

    /// <summary>
    /// The content of the message.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// When the message was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Estimated or actual token count for this message.
    /// </summary>
    public int TokenCount { get; set; }

    /// <summary>
    /// For assistant messages, the model that generated this response.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// Creates a user message.
    /// </summary>
    public static Message User(string content, int estimatedTokens = 0) => new()
    {
        Role = MessageRole.User,
        Content = content,
        TokenCount = estimatedTokens
    };

    /// <summary>
    /// Creates an assistant message.
    /// </summary>
    public static Message Assistant(string content, string? modelId = null, int tokenCount = 0) => new()
    {
        Role = MessageRole.Assistant,
        Content = content,
        ModelId = modelId,
        TokenCount = tokenCount
    };

    /// <summary>
    /// Creates a system message.
    /// </summary>
    public static Message System(string content) => new()
    {
        Role = MessageRole.System,
        Content = content
    };
}

/// <summary>
/// The role of a message sender in the conversation.
/// </summary>
public enum MessageRole
{
    System,
    User,
    Assistant
}
