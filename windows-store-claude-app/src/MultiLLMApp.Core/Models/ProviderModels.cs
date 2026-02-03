namespace MultiLLMApp.Core.Models;

/// <summary>
/// Information about an LLM provider.
/// </summary>
public sealed record ProviderInfo
{
    /// <summary>
    /// Unique identifier (e.g., "claude", "openai").
    /// </summary>
    public required string ProviderId { get; init; }

    /// <summary>
    /// Display name for UI (e.g., "Claude", "ChatGPT").
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Provider description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether credentials are configured for this provider.
    /// </summary>
    public bool IsConfigured { get; init; }

    /// <summary>
    /// Available models for this provider.
    /// </summary>
    public IReadOnlyList<ModelInfo> Models { get; init; } = [];
}

/// <summary>
/// Information about a specific model.
/// </summary>
public sealed record ModelInfo
{
    /// <summary>
    /// Model identifier used in API calls.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Display name for UI.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Maximum context window size in tokens.
    /// </summary>
    public int MaxContextTokens { get; init; }

    /// <summary>
    /// Maximum output tokens supported.
    /// </summary>
    public int MaxOutputTokens { get; init; }

    /// <summary>
    /// Cost per 1M input tokens (USD).
    /// </summary>
    public decimal InputTokenCostPer1M { get; init; }

    /// <summary>
    /// Cost per 1M output tokens (USD).
    /// </summary>
    public decimal OutputTokenCostPer1M { get; init; }

    /// <summary>
    /// Whether this is the default/recommended model.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Model tier for UI grouping (e.g., "fast", "balanced", "powerful").
    /// </summary>
    public string? Tier { get; init; }
}

/// <summary>
/// Configuration for a custom LLM provider.
/// </summary>
public sealed record ProviderConfig
{
    /// <summary>
    /// Unique identifier for this provider.
    /// </summary>
    public required string ProviderId { get; init; }

    /// <summary>
    /// Display name for UI.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Base API endpoint URL.
    /// </summary>
    public required Uri ApiEndpoint { get; init; }

    /// <summary>
    /// Authentication type.
    /// </summary>
    public AuthType AuthType { get; init; } = AuthType.BearerToken;

    /// <summary>
    /// Custom header name for API key (if AuthType is CustomHeader).
    /// </summary>
    public string? AuthHeaderName { get; init; }

    /// <summary>
    /// Available models for this provider.
    /// </summary>
    public IReadOnlyList<ModelInfo> Models { get; init; } = [];

    /// <summary>
    /// Whether streaming is supported.
    /// </summary>
    public bool SupportsStreaming { get; init; } = true;

    /// <summary>
    /// Request format type.
    /// </summary>
    public ApiFormat ApiFormat { get; init; } = ApiFormat.OpenAICompatible;
}

/// <summary>
/// Authentication type for API requests.
/// </summary>
public enum AuthType
{
    /// <summary>Bearer token in Authorization header.</summary>
    BearerToken,
    /// <summary>Custom header (e.g., x-api-key).</summary>
    CustomHeader,
    /// <summary>Query parameter.</summary>
    QueryParameter
}

/// <summary>
/// API format for request/response.
/// </summary>
public enum ApiFormat
{
    /// <summary>OpenAI-compatible format.</summary>
    OpenAICompatible,
    /// <summary>Anthropic Claude format.</summary>
    Anthropic,
    /// <summary>Custom format requiring provider-specific handling.</summary>
    Custom
}
