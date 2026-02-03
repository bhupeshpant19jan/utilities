using MultiLLMApp.Core.Interfaces;
using MultiLLMApp.Core.Models;
using MultiLLMApp.Core.Providers;

namespace MultiLLMApp.Core.Services;

/// <summary>
/// Factory for creating LLM provider instances.
/// Ensures each tab gets its own isolated provider instance.
/// </summary>
public sealed class ProviderFactory : IProviderFactory
{
    private readonly ICredentialStore _credentialStore;
    private readonly Dictionary<string, ProviderInfo> _builtInProviders;
    private readonly Dictionary<string, ProviderConfig> _customProviders = new();

    public ProviderFactory(ICredentialStore credentialStore)
    {
        _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));

        // Register built-in providers
        _builtInProviders = new Dictionary<string, ProviderInfo>
        {
            ["claude"] = new ProviderInfo
            {
                ProviderId = "claude",
                DisplayName = "Claude",
                Description = "Anthropic's Claude AI assistant",
                Models = new ClaudeProvider("temp").GetAvailableModels()
            },
            ["openai"] = new ProviderInfo
            {
                ProviderId = "openai",
                DisplayName = "ChatGPT",
                Description = "OpenAI's GPT models",
                Models = new OpenAIProvider("temp").GetAvailableModels()
            }
        };
    }

    public async Task<ILLMProvider> CreateProviderAsync(string providerId, string? keyAlias = null)
    {
        if (string.IsNullOrEmpty(providerId))
            throw new ArgumentException("Provider ID is required", nameof(providerId));

        // Get API key from secure storage
        var apiKey = await _credentialStore.RetrieveAsync(providerId, keyAlias);
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ProviderNotConfiguredException(providerId);
        }

        // Create appropriate provider instance
        return providerId.ToLowerInvariant() switch
        {
            "claude" => new ClaudeProvider(apiKey),
            "openai" => new OpenAIProvider(apiKey),
            _ when _customProviders.ContainsKey(providerId) =>
                CreateCustomProvider(_customProviders[providerId], apiKey),
            _ => throw new NotSupportedException($"Provider '{providerId}' is not supported")
        };
    }

    public IReadOnlyList<ProviderInfo> GetAvailableProviders()
    {
        var providers = new List<ProviderInfo>();

        // Add built-in providers
        providers.AddRange(_builtInProviders.Values);

        // Add custom providers
        foreach (var config in _customProviders.Values)
        {
            providers.Add(new ProviderInfo
            {
                ProviderId = config.ProviderId,
                DisplayName = config.DisplayName,
                Models = config.Models
            });
        }

        return providers.AsReadOnly();
    }

    public async Task<IReadOnlyList<ProviderInfo>> GetConfiguredProvidersAsync()
    {
        var configuredIds = await _credentialStore.GetConfiguredProvidersAsync();
        var providers = new List<ProviderInfo>();

        foreach (var providerId in configuredIds)
        {
            if (_builtInProviders.TryGetValue(providerId, out var provider))
            {
                providers.Add(provider with { IsConfigured = true });
            }
            else if (_customProviders.TryGetValue(providerId, out var config))
            {
                providers.Add(new ProviderInfo
                {
                    ProviderId = config.ProviderId,
                    DisplayName = config.DisplayName,
                    Models = config.Models,
                    IsConfigured = true
                });
            }
        }

        return providers.AsReadOnly();
    }

    public void RegisterCustomProvider(ProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (string.IsNullOrEmpty(config.ProviderId))
            throw new ArgumentException("Provider ID is required");

        if (_builtInProviders.ContainsKey(config.ProviderId))
            throw new ArgumentException($"Cannot override built-in provider '{config.ProviderId}'");

        _customProviders[config.ProviderId] = config;
    }

    public bool IsProviderAvailable(string providerId)
    {
        return _builtInProviders.ContainsKey(providerId) ||
               _customProviders.ContainsKey(providerId);
    }

    private static ILLMProvider CreateCustomProvider(ProviderConfig config, string apiKey)
    {
        // For now, custom providers use OpenAI-compatible format
        // Future: Add support for other API formats
        if (config.ApiFormat == ApiFormat.OpenAICompatible)
        {
            return new OpenAIProvider(apiKey);
        }

        throw new NotSupportedException(
            $"Custom provider format '{config.ApiFormat}' is not yet supported");
    }
}
