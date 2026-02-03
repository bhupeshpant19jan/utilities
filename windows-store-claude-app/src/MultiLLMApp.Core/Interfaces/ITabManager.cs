using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Interfaces;

/// <summary>
/// Manages the lifecycle of tabs and ensures isolation between them.
/// Acts as the central coordinator for all tab operations.
/// </summary>
public interface ITabManager : IDisposable
{
    /// <summary>
    /// Maximum number of concurrent tabs allowed.
    /// </summary>
    int MaxTabs { get; }

    /// <summary>
    /// Current number of open tabs.
    /// </summary>
    int TabCount { get; }

    /// <summary>
    /// Gets the currently active tab ID.
    /// </summary>
    Guid? ActiveTabId { get; }

    /// <summary>
    /// Event raised when a tab is created.
    /// </summary>
    event EventHandler<TabEventArgs>? TabCreated;

    /// <summary>
    /// Event raised when a tab is closed.
    /// </summary>
    event EventHandler<TabEventArgs>? TabClosed;

    /// <summary>
    /// Event raised when the active tab changes.
    /// </summary>
    event EventHandler<TabEventArgs>? ActiveTabChanged;

    /// <summary>
    /// Creates a new tab with the specified provider.
    /// </summary>
    /// <param name="providerId">The provider to use for this tab.</param>
    /// <param name="label">Optional custom label for the tab.</param>
    /// <returns>The created tab context.</returns>
    /// <exception cref="InvalidOperationException">If max tabs limit is reached.</exception>
    Task<TabInfo> CreateTabAsync(string providerId, string? label = null);

    /// <summary>
    /// Closes a tab and disposes its resources.
    /// </summary>
    /// <param name="tabId">The tab ID to close.</param>
    /// <returns>True if the tab was closed.</returns>
    Task<bool> CloseTabAsync(Guid tabId);

    /// <summary>
    /// Gets a tab by its ID.
    /// </summary>
    /// <param name="tabId">The tab ID.</param>
    /// <returns>The tab info, or null if not found.</returns>
    TabInfo? GetTab(Guid tabId);

    /// <summary>
    /// Gets all open tabs in order.
    /// </summary>
    /// <returns>Ordered list of tab information.</returns>
    IReadOnlyList<TabInfo> GetAllTabs();

    /// <summary>
    /// Sets the active tab.
    /// </summary>
    /// <param name="tabId">The tab ID to activate.</param>
    void SetActiveTab(Guid tabId);

    /// <summary>
    /// Reorders tabs by moving a tab to a new position.
    /// </summary>
    /// <param name="tabId">The tab to move.</param>
    /// <param name="newIndex">The new position index.</param>
    void ReorderTab(Guid tabId, int newIndex);

    /// <summary>
    /// Renames a tab.
    /// </summary>
    /// <param name="tabId">The tab ID.</param>
    /// <param name="newLabel">The new label.</param>
    void RenameTab(Guid tabId, string newLabel);

    /// <summary>
    /// Saves the current state of all tabs for persistence.
    /// </summary>
    Task SaveStateAsync();

    /// <summary>
    /// Restores tabs from persisted state.
    /// </summary>
    Task RestoreStateAsync();
}

/// <summary>
/// Event arguments for tab events.
/// </summary>
public class TabEventArgs : EventArgs
{
    public Guid TabId { get; init; }
    public TabInfo? Tab { get; init; }
}
