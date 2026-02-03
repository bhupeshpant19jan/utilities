using MultiLLMApp.Core.Interfaces;
using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Services;

/// <summary>
/// Manages conversation history and context for a single tab session.
/// Thread-safe implementation with isolation guarantees.
/// </summary>
public sealed class SessionManager : ISessionManager
{
    private readonly List<Message> _messages = [];
    private readonly object _lock = new();
    private int _totalTokensUsed;

    public Guid SessionId { get; }
    public bool ContextEnabled { get; set; } = true;

    public int MessageCount
    {
        get { lock (_lock) { return _messages.Count; } }
    }

    public int TotalTokensUsed
    {
        get { lock (_lock) { return _totalTokensUsed; } }
    }

    public SessionManager() : this(Guid.NewGuid()) { }

    public SessionManager(Guid sessionId)
    {
        SessionId = sessionId;
    }

    public void AddMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        lock (_lock)
        {
            _messages.Add(message);
            _totalTokensUsed += message.TokenCount;
        }
    }

    public IReadOnlyList<Message> GetContext()
    {
        lock (_lock)
        {
            if (!ContextEnabled)
            {
                return [];
            }

            // Return copy to prevent external modification
            return _messages
                .Where(m => m.Role != MessageRole.System)
                .ToList()
                .AsReadOnly();
        }
    }

    public IReadOnlyList<Message> GetHistory()
    {
        lock (_lock)
        {
            return _messages.ToList().AsReadOnly();
        }
    }

    public void ClearContext()
    {
        lock (_lock)
        {
            _messages.Clear();
            _totalTokensUsed = 0;
        }
    }

    public void UpdateLastMessageTokens(int inputTokens, int outputTokens)
    {
        lock (_lock)
        {
            if (_messages.Count == 0)
                return;

            var lastMessage = _messages[^1];
            if (lastMessage.Role == MessageRole.Assistant)
            {
                // Adjust total: remove estimate, add actual
                _totalTokensUsed -= lastMessage.TokenCount;
                lastMessage.TokenCount = outputTokens;
                _totalTokensUsed += outputTokens + inputTokens;
            }
        }
    }

    public SessionData ExportSession()
    {
        lock (_lock)
        {
            return new SessionData
            {
                SessionId = SessionId,
                TabId = Guid.Empty, // Set by TabContext
                ContextEnabled = ContextEnabled,
                TotalTokensUsed = _totalTokensUsed,
                Messages = _messages.Select(m => new MessageData
                {
                    MessageId = m.MessageId,
                    Role = m.Role,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    TokenCount = m.TokenCount,
                    ModelId = m.ModelId
                }).ToList().AsReadOnly()
            };
        }
    }

    public void ImportSession(SessionData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        lock (_lock)
        {
            _messages.Clear();
            _totalTokensUsed = 0;

            foreach (var msgData in data.Messages)
            {
                var message = new Message
                {
                    MessageId = msgData.MessageId,
                    Role = msgData.Role,
                    Content = msgData.Content,
                    Timestamp = msgData.Timestamp,
                    TokenCount = msgData.TokenCount,
                    ModelId = msgData.ModelId
                };
                _messages.Add(message);
                _totalTokensUsed += message.TokenCount;
            }

            ContextEnabled = data.ContextEnabled;
        }
    }
}
