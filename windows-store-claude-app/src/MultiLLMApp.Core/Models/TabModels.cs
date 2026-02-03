namespace MultiLLMApp.Core.Models;

/// <summary>
/// Information about a tab for UI display and management.
/// </summary>
public sealed class TabInfo
{
    /// <summary>
    /// Unique identifier for this tab.
    /// </summary>
    public required Guid TabId { get; init; }

    /// <summary>
    /// User-defined or auto-generated label.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// The provider being used in this tab.
    /// </summary>
    public required string ProviderId { get; init; }

    /// <summary>
    /// The currently selected model.
    /// </summary>
    public required string ModelId { get; set; }

    /// <summary>
    /// Whether context/history is enabled.
    /// </summary>
    public bool ContextEnabled { get; set; } = true;

    /// <summary>
    /// Maximum tokens for responses.
    /// </summary>
    public int MaxTokens { get; set; } = 1024;

    /// <summary>
    /// Whether a request is currently streaming.
    /// </summary>
    public bool IsStreaming { get; set; }

    /// <summary>
    /// Number of messages in the conversation.
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// Total tokens used in this session.
    /// </summary>
    public int TotalTokensUsed { get; set; }

    /// <summary>
    /// Order index for tab display.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// When the tab was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// When the tab was last active.
    /// </summary>
    public DateTimeOffset LastActiveAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Serializable session data for persistence.
/// </summary>
public sealed class SessionData
{
    /// <summary>
    /// Session identifier.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Associated tab ID.
    /// </summary>
    public required Guid TabId { get; init; }

    /// <summary>
    /// Whether context was enabled.
    /// </summary>
    public bool ContextEnabled { get; init; }

    /// <summary>
    /// All messages in the session.
    /// </summary>
    public required IReadOnlyList<MessageData> Messages { get; init; }

    /// <summary>
    /// Total tokens used.
    /// </summary>
    public int TotalTokensUsed { get; init; }
}

/// <summary>
/// Serializable message data for persistence.
/// </summary>
public sealed class MessageData
{
    public required Guid MessageId { get; init; }
    public required MessageRole Role { get; init; }
    public required string Content { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public int TokenCount { get; init; }
    public string? ModelId { get; init; }
}

/// <summary>
/// Complete tab state for persistence.
/// </summary>
public sealed class TabState
{
    public required Guid TabId { get; init; }
    public required string Label { get; init; }
    public required string ProviderId { get; init; }
    public required string ModelId { get; init; }
    public bool ContextEnabled { get; init; }
    public int MaxTokens { get; init; }
    public int Order { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset LastActiveAt { get; init; }
    public SessionData? Session { get; init; }
}
