using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Interfaces;

/// <summary>
/// Factory for creating LLM provider instances.
/// Handles provider registration and instantiation with proper credentials.
/// </summary>
public interface IProviderFactory
{
    /// <summary>
    /// Creates a new provider instance for the specified provider ID.
    /// Each call returns a new instance to ensure tab isolation.
    /// </summary>
    /// <param name="providerId">The provider identifier (e.g., "claude", "openai").</param>
    /// <param name="keyAlias">Optional key alias if multiple keys are configured.</param>
    /// <returns>A new provider instance.</returns>
    /// <exception cref="NotSupportedException">If the provider ID is not registered.</exception>
    /// <exception cref="InvalidOperationException">If credentials are not configured.</exception>
    Task<ILLMProvider> CreateProviderAsync(string providerId, string? keyAlias = null);

    /// <summary>
    /// Gets information about all available providers.
    /// </summary>
    /// <returns>List of provider information.</returns>
    IReadOnlyList<ProviderInfo> GetAvailableProviders();

    /// <summary>
    /// Gets information about providers that have credentials configured.
    /// </summary>
    /// <returns>List of configured provider information.</returns>
    Task<IReadOnlyList<ProviderInfo>> GetConfiguredProvidersAsync();

    /// <summary>
    /// Registers a custom provider configuration.
    /// </summary>
    /// <param name="config">The provider configuration.</param>
    void RegisterCustomProvider(ProviderConfig config);

    /// <summary>
    /// Checks if a provider is available (registered).
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <returns>True if the provider is registered.</returns>
    bool IsProviderAvailable(string providerId);
}
