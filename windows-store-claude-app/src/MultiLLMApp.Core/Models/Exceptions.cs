namespace MultiLLMApp.Core.Models;

/// <summary>
/// Base exception for all LLM provider errors.
/// </summary>
public class LLMException : Exception
{
    public string ErrorCode { get; }
    public string? ProviderId { get; }

    public LLMException(string errorCode, string message, string? providerId = null, Exception? inner = null)
        : base(message, inner)
    {
        ErrorCode = errorCode;
        ProviderId = providerId;
    }
}

/// <summary>
/// Exception thrown when API authentication fails.
/// </summary>
public class AuthenticationException : LLMException
{
    public AuthenticationException(string providerId, string message = "Invalid API key or authentication failed.")
        : base("E001", message, providerId) { }
}

/// <summary>
/// Exception thrown when rate limit is exceeded.
/// </summary>
public class RateLimitException : LLMException
{
    public TimeSpan? RetryAfter { get; }

    public RateLimitException(string providerId, TimeSpan? retryAfter = null)
        : base("E002", $"Rate limit exceeded. {(retryAfter.HasValue ? $"Retry after {retryAfter.Value.TotalSeconds:F0}s." : "")}", providerId)
    {
        RetryAfter = retryAfter;
    }
}

/// <summary>
/// Exception thrown when a request times out.
/// </summary>
public class ProviderTimeoutException : LLMException
{
    public ProviderTimeoutException(string providerId, TimeSpan timeout)
        : base("E003", $"Request timed out after {timeout.TotalSeconds:F0} seconds.", providerId) { }
}

/// <summary>
/// Exception thrown when the provider service is unavailable.
/// </summary>
public class ProviderUnavailableException : LLMException
{
    public ProviderUnavailableException(string providerId, string? details = null)
        : base("E004", $"Service unavailable.{(details != null ? $" {details}" : "")}", providerId) { }
}

/// <summary>
/// Exception thrown when the context is too long.
/// </summary>
public class ContextTooLongException : LLMException
{
    public int TokenCount { get; }
    public int MaxTokens { get; }

    public ContextTooLongException(string providerId, int tokenCount, int maxTokens)
        : base("E005", $"Context too long ({tokenCount} tokens). Maximum is {maxTokens} tokens.", providerId)
    {
        TokenCount = tokenCount;
        MaxTokens = maxTokens;
    }
}

/// <summary>
/// Exception thrown when the provider returns an invalid response.
/// </summary>
public class InvalidResponseException : LLMException
{
    public InvalidResponseException(string providerId, string details, Exception? inner = null)
        : base("E006", $"Invalid response from provider: {details}", providerId, inner) { }
}

/// <summary>
/// Exception thrown when max tabs limit is reached.
/// </summary>
public class MaxTabsExceededException : LLMException
{
    public int MaxTabs { get; }

    public MaxTabsExceededException(int maxTabs)
        : base("E007", $"Maximum of {maxTabs} tabs allowed. Close a tab first.")
    {
        MaxTabs = maxTabs;
    }
}

/// <summary>
/// Exception thrown when a provider is not configured.
/// </summary>
public class ProviderNotConfiguredException : LLMException
{
    public ProviderNotConfiguredException(string providerId)
        : base("E008", $"Provider '{providerId}' is not configured. Add API key in settings.", providerId) { }
}
