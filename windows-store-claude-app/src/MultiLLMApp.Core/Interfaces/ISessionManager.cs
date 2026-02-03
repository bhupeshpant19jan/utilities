using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Interfaces;

/// <summary>
/// Manages conversation history and context for a single tab session.
/// Each tab has its own isolated ISessionManager instance.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Unique identifier for this session.
    /// </summary>
    Guid SessionId { get; }

    /// <summary>
    /// Whether conversation context is enabled (multi-turn mode).
    /// When disabled, only the current message is sent without history.
    /// </summary>
    bool ContextEnabled { get; set; }

    /// <summary>
    /// Gets the total number of messages in the session.
    /// </summary>
    int MessageCount { get; }

    /// <summary>
    /// Gets the total estimated tokens used in this session.
    /// </summary>
    int TotalTokensUsed { get; }

    /// <summary>
    /// Adds a message to the conversation history.
    /// </summary>
    /// <param name="message">The message to add.</param>
    void AddMessage(Message message);

    /// <summary>
    /// Gets the conversation context to send with the next request.
    /// Returns empty list if ContextEnabled is false.
    /// </summary>
    /// <returns>List of messages representing the context.</returns>
    IReadOnlyList<Message> GetContext();

    /// <summary>
    /// Gets the full conversation history regardless of ContextEnabled setting.
    /// </summary>
    /// <returns>All messages in the session.</returns>
    IReadOnlyList<Message> GetHistory();

    /// <summary>
    /// Clears all messages and resets the session context.
    /// </summary>
    void ClearContext();

    /// <summary>
    /// Updates token count for the last assistant message after streaming completes.
    /// </summary>
    /// <param name="inputTokens">Actual input tokens used.</param>
    /// <param name="outputTokens">Actual output tokens generated.</param>
    void UpdateLastMessageTokens(int inputTokens, int outputTokens);

    /// <summary>
    /// Exports the session to a serializable format.
    /// </summary>
    /// <returns>Session data for persistence.</returns>
    SessionData ExportSession();

    /// <summary>
    /// Imports session data from a previously exported session.
    /// </summary>
    /// <param name="data">The session data to import.</param>
    void ImportSession(SessionData data);
}
