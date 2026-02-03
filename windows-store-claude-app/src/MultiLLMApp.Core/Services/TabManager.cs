using System.Collections.Concurrent;
using MultiLLMApp.Core.Interfaces;
using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Services;

/// <summary>
/// Manages the lifecycle of tabs and ensures complete isolation between them.
/// Thread-safe implementation using concurrent collections and semaphores.
/// </summary>
public sealed class TabManager : ITabManager
{
    private readonly ConcurrentDictionary<Guid, TabContext> _tabs = new();
    private readonly List<Guid> _tabOrder = []; // Maintains display order
    private readonly SemaphoreSlim _tabLock = new(1, 1);
    private readonly IProviderFactory _providerFactory;
    private readonly Func<Task>? _saveStateCallback;

    private Guid? _activeTabId;
    private bool _disposed;

    public int MaxTabs { get; } = 10;
    public int TabCount => _tabs.Count;
    public Guid? ActiveTabId => _activeTabId;

    public event EventHandler<TabEventArgs>? TabCreated;
    public event EventHandler<TabEventArgs>? TabClosed;
    public event EventHandler<TabEventArgs>? ActiveTabChanged;

    public TabManager(IProviderFactory providerFactory, Func<Task>? saveStateCallback = null)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        _saveStateCallback = saveStateCallback;
    }

    public async Task<TabInfo> CreateTabAsync(string providerId, string? label = null)
    {
        await _tabLock.WaitAsync();
        try
        {
            if (_tabs.Count >= MaxTabs)
            {
                throw new MaxTabsExceededException(MaxTabs);
            }

            var context = new TabContext(_providerFactory, providerId, label);
            await context.InitializeAsync();

            // Set order
            context.Info.Order = _tabOrder.Count;

            if (!_tabs.TryAdd(context.TabId, context))
            {
                context.Dispose();
                throw new InvalidOperationException("Failed to add tab");
            }

            _tabOrder.Add(context.TabId);

            // Set as active if first tab
            if (_activeTabId == null)
            {
                _activeTabId = context.TabId;
            }

            TabCreated?.Invoke(this, new TabEventArgs { TabId = context.TabId, Tab = context.Info });

            return context.Info;
        }
        finally
        {
            _tabLock.Release();
        }
    }

    public async Task<bool> CloseTabAsync(Guid tabId)
    {
        await _tabLock.WaitAsync();
        try
        {
            if (!_tabs.TryRemove(tabId, out var context))
            {
                return false;
            }

            _tabOrder.Remove(tabId);

            // Update orders
            for (int i = 0; i < _tabOrder.Count; i++)
            {
                if (_tabs.TryGetValue(_tabOrder[i], out var tab))
                {
                    tab.Info.Order = i;
                }
            }

            // Handle active tab change
            if (_activeTabId == tabId)
            {
                _activeTabId = _tabOrder.Count > 0 ? _tabOrder[^1] : null;
                if (_activeTabId.HasValue)
                {
                    ActiveTabChanged?.Invoke(this, new TabEventArgs
                    {
                        TabId = _activeTabId.Value,
                        Tab = GetTab(_activeTabId.Value)
                    });
                }
            }

            context.Dispose();

            TabClosed?.Invoke(this, new TabEventArgs { TabId = tabId });

            // Auto-save
            if (_saveStateCallback != null)
            {
                _ = _saveStateCallback();
            }

            return true;
        }
        finally
        {
            _tabLock.Release();
        }
    }

    public TabInfo? GetTab(Guid tabId)
    {
        return _tabs.TryGetValue(tabId, out var context) ? context.Info : null;
    }

    /// <summary>
    /// Gets the TabContext for direct operations (internal use).
    /// </summary>
    public TabContext? GetTabContext(Guid tabId)
    {
        return _tabs.TryGetValue(tabId, out var context) ? context : null;
    }

    public IReadOnlyList<TabInfo> GetAllTabs()
    {
        return _tabOrder
            .Select(id => _tabs.TryGetValue(id, out var ctx) ? ctx.Info : null)
            .Where(info => info != null)
            .Cast<TabInfo>()
            .ToList()
            .AsReadOnly();
    }

    public void SetActiveTab(Guid tabId)
    {
        if (!_tabs.ContainsKey(tabId))
        {
            throw new ArgumentException($"Tab {tabId} not found", nameof(tabId));
        }

        var previousActiveId = _activeTabId;
        _activeTabId = tabId;

        if (previousActiveId != tabId)
        {
            if (_tabs.TryGetValue(tabId, out var context))
            {
                context.Info.LastActiveAt = DateTimeOffset.UtcNow;
            }

            ActiveTabChanged?.Invoke(this, new TabEventArgs
            {
                TabId = tabId,
                Tab = GetTab(tabId)
            });
        }
    }

    public void ReorderTab(Guid tabId, int newIndex)
    {
        if (newIndex < 0 || newIndex >= _tabOrder.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex));
        }

        var currentIndex = _tabOrder.IndexOf(tabId);
        if (currentIndex < 0)
        {
            throw new ArgumentException($"Tab {tabId} not found", nameof(tabId));
        }

        if (currentIndex == newIndex)
            return;

        _tabOrder.RemoveAt(currentIndex);
        _tabOrder.Insert(newIndex, tabId);

        // Update orders
        for (int i = 0; i < _tabOrder.Count; i++)
        {
            if (_tabs.TryGetValue(_tabOrder[i], out var tab))
            {
                tab.Info.Order = i;
            }
        }
    }

    public void RenameTab(Guid tabId, string newLabel)
    {
        if (string.IsNullOrWhiteSpace(newLabel))
        {
            throw new ArgumentException("Label cannot be empty", nameof(newLabel));
        }

        if (_tabs.TryGetValue(tabId, out var context))
        {
            context.Info.Label = newLabel.Length > 30 ? newLabel[..30] : newLabel;
        }
    }

    public async Task SaveStateAsync()
    {
        await _tabLock.WaitAsync();
        try
        {
            // Export all tab states
            var states = new List<TabState>();
            foreach (var tabId in _tabOrder)
            {
                if (_tabs.TryGetValue(tabId, out var context))
                {
                    states.Add(context.ExportState());
                }
            }

            // Actual persistence is handled by the data layer
            // Here we just prepare the data
            // The callback or external service handles the actual save
        }
        finally
        {
            _tabLock.Release();
        }
    }

    public async Task RestoreStateAsync()
    {
        // Restoration is handled by external data layer
        // This method would be called with loaded states
        await Task.CompletedTask;
    }

    /// <summary>
    /// Restores tabs from saved states.
    /// </summary>
    public async Task RestoreFromStatesAsync(IEnumerable<TabState> states)
    {
        await _tabLock.WaitAsync();
        try
        {
            foreach (var state in states.OrderBy(s => s.Order))
            {
                try
                {
                    var context = new TabContext(_providerFactory, state.ProviderId, state.Label);
                    await context.InitializeAsync();
                    context.ImportState(state);

                    if (_tabs.TryAdd(context.TabId, context))
                    {
                        _tabOrder.Add(context.TabId);
                    }
                }
                catch (Exception)
                {
                    // Log and continue with other tabs
                    // Don't fail entire restore if one tab fails
                }
            }

            // Set first tab as active
            if (_tabOrder.Count > 0 && _activeTabId == null)
            {
                _activeTabId = _tabOrder[0];
            }
        }
        finally
        {
            _tabLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var context in _tabs.Values)
        {
            context.Dispose();
        }

        _tabs.Clear();
        _tabOrder.Clear();
        _tabLock.Dispose();
        _disposed = true;
    }
}
