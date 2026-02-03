/// <summary>
/// Scenario validation tests for the Multi-LLM App core components.
/// These validate the key scenarios from the specification documents.
/// </summary>

using MultiLLMApp.Core.Interfaces;
using MultiLLMApp.Core.Models;
using MultiLLMApp.Core.Services;
using MultiLLMApp.Data;

namespace MultiLLMApp.Tests;

public static class ScenarioValidation
{
    /// <summary>
    /// Runs all validation scenarios and reports results.
    /// </summary>
    public static async Task<ValidationResult> RunAllScenariosAsync()
    {
        var result = new ValidationResult();

        // Scenario 1: Tab Isolation
        await RunScenario(result, "Tab Context Isolation", ValidateTabIsolation);

        // Scenario 2: Session Management
        await RunScenario(result, "Session Context Management", ValidateSessionManagement);

        // Scenario 3: Provider Factory
        await RunScenario(result, "Provider Factory", ValidateProviderFactory);

        // Scenario 4: Tab Manager
        await RunScenario(result, "Tab Manager Lifecycle", ValidateTabManager);

        // Scenario 5: Credential Storage
        await RunScenario(result, "Secure Credential Storage", ValidateCredentialStorage);

        return result;
    }

    private static async Task RunScenario(
        ValidationResult result,
        string name,
        Func<Task<(bool Success, string Details)>> scenario)
    {
        try
        {
            var (success, details) = await scenario();
            result.AddResult(name, success, details);
        }
        catch (Exception ex)
        {
            result.AddResult(name, false, $"Exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Scenario 1: Validates that tabs maintain isolated contexts.
    /// </summary>
    private static async Task<(bool, string)> ValidateTabIsolation()
    {
        var credentialStore = new SecureVault();
        await credentialStore.StoreAsync("claude", "test", "test-key");

        var factory = new ProviderFactory(credentialStore);

        // Create two independent tab contexts
        var tab1 = new TabContext(factory, "claude", "Tab 1");
        var tab2 = new TabContext(factory, "claude", "Tab 2");

        // Verify different tab IDs
        if (tab1.TabId == tab2.TabId)
            return (false, "Tab IDs should be unique");

        // Verify different session IDs
        if (tab1.Session.SessionId == tab2.Session.SessionId)
            return (false, "Session IDs should be unique");

        // Add message to tab1
        tab1.Session.AddMessage(Message.User("Message for tab 1"));

        // Verify tab2 is not affected
        if (tab2.Session.MessageCount != 0)
            return (false, "Tab 2 should not have messages from Tab 1");

        if (tab1.Session.MessageCount != 1)
            return (false, "Tab 1 should have 1 message");

        // Cleanup
        tab1.Dispose();
        tab2.Dispose();

        return (true, "Tabs maintain isolated contexts");
    }

    /// <summary>
    /// Scenario 2: Validates session context management.
    /// </summary>
    private static Task<(bool, string)> ValidateSessionManagement()
    {
        var session = new SessionManager();

        // Test adding messages
        session.AddMessage(Message.User("Hello", 5));
        session.AddMessage(Message.Assistant("Hi there!", tokenCount: 3));

        if (session.MessageCount != 2)
            return Task.FromResult((false, $"Expected 2 messages, got {session.MessageCount}"));

        // Test context retrieval
        var context = session.GetContext();
        if (context.Count != 2)
            return Task.FromResult((false, $"Expected 2 context messages, got {context.Count}"));

        // Test context disabled mode
        session.ContextEnabled = false;
        var emptyContext = session.GetContext();
        if (emptyContext.Count != 0)
            return Task.FromResult((false, "Context should be empty when disabled"));

        // History should still be available
        var history = session.GetHistory();
        if (history.Count != 2)
            return Task.FromResult((false, "History should still contain messages"));

        // Test clear
        session.ClearContext();
        if (session.MessageCount != 0)
            return Task.FromResult((false, "Messages should be cleared"));

        // Test token tracking
        session.ContextEnabled = true;
        session.AddMessage(Message.User("Test", 10));
        session.AddMessage(Message.Assistant("Response", tokenCount: 20));
        session.UpdateLastMessageTokens(10, 25);

        if (session.TotalTokensUsed != 35) // 10 + 25
            return Task.FromResult((false, $"Expected 35 total tokens, got {session.TotalTokensUsed}"));

        return Task.FromResult((true, "Session management works correctly"));
    }

    /// <summary>
    /// Scenario 3: Validates provider factory.
    /// </summary>
    private static async Task<(bool, string)> ValidateProviderFactory()
    {
        var credentialStore = new SecureVault();
        var factory = new ProviderFactory(credentialStore);

        // Test available providers
        var providers = factory.GetAvailableProviders();
        if (providers.Count < 2)
            return (false, "Should have at least Claude and OpenAI providers");

        var hasClause = providers.Any(p => p.ProviderId == "claude");
        var hasOpenAI = providers.Any(p => p.ProviderId == "openai");

        if (!hasClause || !hasOpenAI)
            return (false, "Missing expected providers");

        // Test provider availability check
        if (!factory.IsProviderAvailable("claude"))
            return (false, "Claude should be available");

        if (factory.IsProviderAvailable("unknown"))
            return (false, "Unknown provider should not be available");

        // Test provider creation without credentials (should fail)
        try
        {
            await factory.CreateProviderAsync("claude");
            return (false, "Should throw when credentials not configured");
        }
        catch (ProviderNotConfiguredException)
        {
            // Expected
        }

        // Configure credentials and test creation
        await credentialStore.StoreAsync("claude", "default", "test-api-key");
        var configuredProviders = await factory.GetConfiguredProvidersAsync();

        if (configuredProviders.Count != 1)
            return (false, $"Expected 1 configured provider, got {configuredProviders.Count}");

        // Create provider
        var provider = await factory.CreateProviderAsync("claude");
        if (provider.ProviderId != "claude")
            return (false, "Created provider should be Claude");

        provider.Dispose();

        return (true, "Provider factory works correctly");
    }

    /// <summary>
    /// Scenario 4: Validates tab manager lifecycle.
    /// </summary>
    private static async Task<(bool, string)> ValidateTabManager()
    {
        var credentialStore = new SecureVault();
        await credentialStore.StoreAsync("claude", "default", "test-key");
        await credentialStore.StoreAsync("openai", "default", "test-key");

        var factory = new ProviderFactory(credentialStore);
        var manager = new TabManager(factory);

        // Test initial state
        if (manager.TabCount != 0)
            return (false, "Should start with no tabs");

        // Create tabs
        var tab1 = await manager.CreateTabAsync("claude", "Claude Tab");
        if (tab1.ProviderId != "claude")
            return (false, "Tab 1 should use Claude");

        var tab2 = await manager.CreateTabAsync("openai", "GPT Tab");
        if (tab2.ProviderId != "openai")
            return (false, "Tab 2 should use OpenAI");

        if (manager.TabCount != 2)
            return (false, $"Should have 2 tabs, got {manager.TabCount}");

        // Test active tab
        if (manager.ActiveTabId != tab1.TabId)
            return (false, "First tab should be active");

        manager.SetActiveTab(tab2.TabId);
        if (manager.ActiveTabId != tab2.TabId)
            return (false, "Tab 2 should now be active");

        // Test tab retrieval
        var allTabs = manager.GetAllTabs();
        if (allTabs.Count != 2)
            return (false, "Should return 2 tabs");

        // Test rename
        manager.RenameTab(tab1.TabId, "Renamed Tab");
        var renamedTab = manager.GetTab(tab1.TabId);
        if (renamedTab?.Label != "Renamed Tab")
            return (false, "Tab should be renamed");

        // Test close
        var closed = await manager.CloseTabAsync(tab1.TabId);
        if (!closed)
            return (false, "Tab should be closed");

        if (manager.TabCount != 1)
            return (false, "Should have 1 tab after close");

        // Active tab should switch
        if (manager.ActiveTabId != tab2.TabId)
            return (false, "Active tab should switch to remaining tab");

        // Cleanup
        manager.Dispose();

        return (true, "Tab manager lifecycle works correctly");
    }

    /// <summary>
    /// Scenario 5: Validates credential storage.
    /// </summary>
    private static async Task<(bool, string)> ValidateCredentialStorage()
    {
        var vault = new SecureVault();

        // Test store and retrieve
        await vault.StoreAsync("claude", "work", "api-key-work");
        await vault.StoreAsync("claude", "personal", "api-key-personal");
        await vault.StoreAsync("openai", "default", "openai-key");

        // Test existence
        if (!await vault.ExistsAsync("claude"))
            return (false, "Claude credentials should exist");

        if (!await vault.ExistsAsync("claude", "work"))
            return (false, "Claude work key should exist");

        if (await vault.ExistsAsync("unknown"))
            return (false, "Unknown provider should not exist");

        // Test retrieval
        var workKey = await vault.RetrieveAsync("claude", "work");
        if (workKey != "api-key-work")
            return (false, "Should retrieve correct work key");

        var personalKey = await vault.RetrieveAsync("claude", "personal");
        if (personalKey != "api-key-personal")
            return (false, "Should retrieve correct personal key");

        // Test default key retrieval
        var defaultKey = await vault.RetrieveAsync("claude");
        if (string.IsNullOrEmpty(defaultKey))
            return (false, "Should retrieve a default key");

        // Test aliases
        var aliases = await vault.GetAliasesAsync("claude");
        if (aliases.Count != 2)
            return (false, $"Should have 2 aliases, got {aliases.Count}");

        // Test configured providers
        var providers = await vault.GetConfiguredProvidersAsync();
        if (providers.Count != 2)
            return (false, $"Should have 2 configured providers, got {providers.Count}");

        // Test delete
        await vault.DeleteAsync("claude", "work");
        if (await vault.ExistsAsync("claude", "work"))
            return (false, "Work key should be deleted");

        var aliasesAfterDelete = await vault.GetAliasesAsync("claude");
        if (aliasesAfterDelete.Count != 1)
            return (false, "Should have 1 alias after delete");

        return (true, "Credential storage works correctly");
    }
}

/// <summary>
/// Holds validation test results.
/// </summary>
public class ValidationResult
{
    private readonly List<(string Name, bool Success, string Details)> _results = [];

    public void AddResult(string name, bool success, string details)
    {
        _results.Add((name, success, details));
    }

    public bool AllPassed => _results.All(r => r.Success);
    public int PassedCount => _results.Count(r => r.Success);
    public int FailedCount => _results.Count(r => !r.Success);
    public int TotalCount => _results.Count;

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Scenario Validation Results ===");
        sb.AppendLine();

        foreach (var (name, success, details) in _results)
        {
            var status = success ? "✓ PASS" : "✗ FAIL";
            sb.AppendLine($"{status}: {name}");
            sb.AppendLine($"       {details}");
            sb.AppendLine();
        }

        sb.AppendLine($"Summary: {PassedCount}/{TotalCount} passed");
        return sb.ToString();
    }
}
