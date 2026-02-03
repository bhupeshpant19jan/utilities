namespace MultiLLMApp.Core.Interfaces;

/// <summary>
/// Provides secure storage for API keys and credentials.
/// Implementation should use OS-level secure storage (e.g., Windows Credential Vault).
/// </summary>
public interface ICredentialStore
{
    /// <summary>
    /// Stores a credential securely.
    /// </summary>
    /// <param name="providerId">The provider identifier (e.g., "claude", "openai").</param>
    /// <param name="keyAlias">User-defined alias for the key (e.g., "work", "personal").</param>
    /// <param name="apiKey">The API key to store.</param>
    /// <returns>True if stored successfully.</returns>
    Task<bool> StoreAsync(string providerId, string keyAlias, string apiKey);

    /// <summary>
    /// Retrieves a stored credential.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="keyAlias">The key alias. If null, returns the default key.</param>
    /// <returns>The API key, or null if not found.</returns>
    Task<string?> RetrieveAsync(string providerId, string? keyAlias = null);

    /// <summary>
    /// Deletes a stored credential.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="keyAlias">The key alias to delete.</param>
    /// <returns>True if deleted successfully.</returns>
    Task<bool> DeleteAsync(string providerId, string keyAlias);

    /// <summary>
    /// Checks if a credential exists for the given provider.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="keyAlias">Optional key alias to check.</param>
    /// <returns>True if a credential exists.</returns>
    Task<bool> ExistsAsync(string providerId, string? keyAlias = null);

    /// <summary>
    /// Gets all stored key aliases for a provider.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <returns>List of key aliases.</returns>
    Task<IReadOnlyList<string>> GetAliasesAsync(string providerId);

    /// <summary>
    /// Gets all provider IDs that have stored credentials.
    /// </summary>
    /// <returns>List of provider IDs with credentials.</returns>
    Task<IReadOnlyList<string>> GetConfiguredProvidersAsync();
}
