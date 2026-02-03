using System.Runtime.CompilerServices;
using MultiLLMApp.Core.Interfaces;
using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Services;

/// <summary>
/// Represents an isolated tab context with its own provider and session.
/// Each tab has completely independent state to prevent data mixing.
/// </summary>
public sealed class TabContext : IDisposable
{
    private readonly IProviderFactory _providerFactory;
    private ILLMProvider? _provider;
    private CancellationTokenSource? _currentRequestCts;
    private bool _disposed;
    private readonly object _streamLock = new();

    public Guid TabId { get; }
    public TabInfo Info { get; }
    public ISessionManager Session { get; }

    public bool IsStreaming
    {
        get { lock (_streamLock) { return _currentRequestCts != null; } }
    }

    public TabContext(
        IProviderFactory providerFactory,
        string providerId,
        string? label = null)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));

        TabId = Guid.NewGuid();
        Session = new SessionManager();

        var providers = providerFactory.GetAvailableProviders();
        var providerInfo = providers.FirstOrDefault(p => p.ProviderId == providerId)
            ?? throw new ArgumentException($"Unknown provider: {providerId}");

        var defaultModel = providerInfo.Models.FirstOrDefault(m => m.IsDefault)
            ?? providerInfo.Models.FirstOrDefault()
            ?? throw new InvalidOperationException($"No models available for {providerId}");

        Info = new TabInfo
        {
            TabId = TabId,
            Label = label ?? $"{providerInfo.DisplayName} - New",
            ProviderId = providerId,
            ModelId = defaultModel.ModelId
        };
    }

    /// <summary>
    /// Initializes the provider. Call after construction.
    /// </summary>
    public async Task InitializeAsync(string? keyAlias = null)
    {
        _provider = await _providerFactory.CreateProviderAsync(Info.ProviderId, keyAlias);
    }

    /// <summary>
    /// Sends a message and streams the response.
    /// </summary>
    public async IAsyncEnumerable<StreamChunk> SendMessageAsync(
        string userMessage,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        EnsureNotDisposed();

        if (_provider == null)
            throw new InvalidOperationException("Tab not initialized. Call InitializeAsync first.");

        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("Message cannot be empty", nameof(userMessage));

        // Create cancellation source for this request
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        lock (_streamLock)
        {
            _currentRequestCts?.Dispose();
            _currentRequestCts = cts;
            Info.IsStreaming = true;
        }

        try
        {
            // Add user message to session
            var userMsg = Message.User(userMessage, _provider.EstimateTokens(userMessage));
            Session.AddMessage(userMsg);

            // Build request with context
            var context = Session.GetContext();
            var request = new LLMRequest
            {
                ModelId = Info.ModelId,
                Messages = context.Count > 0
                    ? context.Take(context.Count - 1).ToList() // Exclude last (current) message
                    : [],
                MaxTokens = Info.MaxTokens
            };

            // Add current message
            request = request with
            {
                Messages = [..request.Messages, userMsg]
            };

            // Track response
            var responseBuilder = new System.Text.StringBuilder();
            int inputTokens = 0, outputTokens = 0;

            await foreach (var chunk in _provider.StreamAsync(request, cts.Token))
            {
                if (chunk.IsFinal)
                {
                    inputTokens = chunk.Usage?.InputTokens ?? _provider.EstimateRequestTokens(request);
                    outputTokens = chunk.Usage?.OutputTokens ?? _provider.EstimateTokens(responseBuilder.ToString());
                }
                else
                {
                    responseBuilder.Append(chunk.Text);
                }

                yield return chunk;
            }

            // Add assistant message to session
            var assistantMsg = Message.Assistant(
                responseBuilder.ToString(),
                Info.ModelId,
                outputTokens);
            Session.AddMessage(assistantMsg);
            Session.UpdateLastMessageTokens(inputTokens, outputTokens);

            // Update tab info
            Info.MessageCount = Session.MessageCount;
            Info.TotalTokensUsed = Session.TotalTokensUsed;
            Info.LastActiveAt = DateTimeOffset.UtcNow;
        }
        finally
        {
            lock (_streamLock)
            {
                _currentRequestCts = null;
                Info.IsStreaming = false;
            }
            cts.Dispose();
        }
    }

    /// <summary>
    /// Cancels the current streaming request if any.
    /// </summary>
    public void CancelCurrentRequest()
    {
        lock (_streamLock)
        {
            _currentRequestCts?.Cancel();
        }
    }

    /// <summary>
    /// Changes the model for this tab.
    /// </summary>
    public void ChangeModel(string modelId)
    {
        EnsureNotDisposed();
        Info.ModelId = modelId;
    }

    /// <summary>
    /// Changes the provider for this tab. Clears conversation history.
    /// </summary>
    public async Task ChangeProviderAsync(string providerId, string? keyAlias = null)
    {
        EnsureNotDisposed();

        // Dispose old provider
        _provider?.Dispose();
        _provider = null;

        // Clear session (provider change means new context)
        Session.ClearContext();

        // Get new provider info
        var providers = _providerFactory.GetAvailableProviders();
        var providerInfo = providers.FirstOrDefault(p => p.ProviderId == providerId)
            ?? throw new ArgumentException($"Unknown provider: {providerId}");

        var defaultModel = providerInfo.Models.FirstOrDefault(m => m.IsDefault)
            ?? providerInfo.Models.First();

        // Update tab info
        // Note: We can't change ProviderId on TabInfo as it's init-only
        // In real implementation, we'd need to recreate the TabContext
        Info.ModelId = defaultModel.ModelId;
        Info.MessageCount = 0;
        Info.TotalTokensUsed = 0;

        // Initialize new provider
        _provider = await _providerFactory.CreateProviderAsync(providerId, keyAlias);
    }

    /// <summary>
    /// Gets the current provider instance.
    /// </summary>
    public ILLMProvider? GetProvider() => _provider;

    /// <summary>
    /// Exports tab state for persistence.
    /// </summary>
    public TabState ExportState()
    {
        var sessionData = Session.ExportSession();
        return new TabState
        {
            TabId = TabId,
            Label = Info.Label,
            ProviderId = Info.ProviderId,
            ModelId = Info.ModelId,
            ContextEnabled = Session.ContextEnabled,
            MaxTokens = Info.MaxTokens,
            Order = Info.Order,
            CreatedAt = Info.CreatedAt,
            LastActiveAt = Info.LastActiveAt,
            Session = sessionData with { TabId = TabId }
        };
    }

    /// <summary>
    /// Restores tab state from persisted data.
    /// </summary>
    public void ImportState(TabState state)
    {
        Info.Label = state.Label;
        Info.ModelId = state.ModelId;
        Info.ContextEnabled = state.ContextEnabled;
        Info.MaxTokens = state.MaxTokens;
        Info.Order = state.Order;

        if (state.Session != null)
        {
            Session.ImportSession(state.Session);
            Info.MessageCount = Session.MessageCount;
            Info.TotalTokensUsed = Session.TotalTokensUsed;
        }
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_streamLock)
        {
            _currentRequestCts?.Cancel();
            _currentRequestCts?.Dispose();
            _currentRequestCts = null;
        }

        _provider?.Dispose();
        _disposed = true;
    }
}
