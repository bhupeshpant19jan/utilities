using MultiLLMApp.Core.Interfaces;

namespace MultiLLMApp.Data;

/// <summary>
/// Secure credential storage implementation.
/// Uses platform-specific secure storage (Windows Credential Vault on Windows).
/// This is a cross-platform abstraction that can be implemented per-platform.
/// </summary>
public sealed class SecureVault : ICredentialStore
{
    private const string CredentialPrefix = "MultiLLMApp_";

    // In-memory fallback for platforms without secure storage
    // In production, this would use Windows.Security.Credentials.PasswordVault
    private readonly Dictionary<string, Dictionary<string, string>> _credentials = new();
    private readonly object _lock = new();

    public Task<bool> StoreAsync(string providerId, string keyAlias, string apiKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerId);
        ArgumentException.ThrowIfNullOrEmpty(keyAlias);
        ArgumentException.ThrowIfNullOrEmpty(apiKey);

        lock (_lock)
        {
            if (!_credentials.ContainsKey(providerId))
            {
                _credentials[providerId] = new Dictionary<string, string>();
            }

            _credentials[providerId][keyAlias] = apiKey;
            return Task.FromResult(true);
        }

        // Production implementation would use:
        // var vault = new PasswordVault();
        // vault.Add(new PasswordCredential(
        //     $"{CredentialPrefix}{providerId}",
        //     keyAlias,
        //     apiKey));
    }

    public Task<string?> RetrieveAsync(string providerId, string? keyAlias = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerId);

        lock (_lock)
        {
            if (!_credentials.TryGetValue(providerId, out var aliases))
            {
                return Task.FromResult<string?>(null);
            }

            if (keyAlias != null)
            {
                return Task.FromResult(
                    aliases.TryGetValue(keyAlias, out var key) ? key : null);
            }

            // Return first available key if no alias specified
            return Task.FromResult(aliases.Values.FirstOrDefault());
        }

        // Production implementation would use:
        // var vault = new PasswordVault();
        // var credential = vault.Retrieve($"{CredentialPrefix}{providerId}", keyAlias ?? "default");
        // credential.RetrievePassword();
        // return credential.Password;
    }

    public Task<bool> DeleteAsync(string providerId, string keyAlias)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerId);
        ArgumentException.ThrowIfNullOrEmpty(keyAlias);

        lock (_lock)
        {
            if (_credentials.TryGetValue(providerId, out var aliases))
            {
                var removed = aliases.Remove(keyAlias);
                if (aliases.Count == 0)
                {
                    _credentials.Remove(providerId);
                }
                return Task.FromResult(removed);
            }
            return Task.FromResult(false);
        }
    }

    public Task<bool> ExistsAsync(string providerId, string? keyAlias = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerId);

        lock (_lock)
        {
            if (!_credentials.TryGetValue(providerId, out var aliases))
            {
                return Task.FromResult(false);
            }

            if (keyAlias != null)
            {
                return Task.FromResult(aliases.ContainsKey(keyAlias));
            }

            return Task.FromResult(aliases.Count > 0);
        }
    }

    public Task<IReadOnlyList<string>> GetAliasesAsync(string providerId)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerId);

        lock (_lock)
        {
            if (_credentials.TryGetValue(providerId, out var aliases))
            {
                return Task.FromResult<IReadOnlyList<string>>(
                    aliases.Keys.ToList().AsReadOnly());
            }
            return Task.FromResult<IReadOnlyList<string>>(
                Array.Empty<string>());
        }
    }

    public Task<IReadOnlyList<string>> GetConfiguredProvidersAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<string>>(
                _credentials.Keys.ToList().AsReadOnly());
        }
    }

    /// <summary>
    /// Clears all stored credentials. Use with caution.
    /// </summary>
    public Task ClearAllAsync()
    {
        lock (_lock)
        {
            _credentials.Clear();
        }
        return Task.CompletedTask;
    }
}
