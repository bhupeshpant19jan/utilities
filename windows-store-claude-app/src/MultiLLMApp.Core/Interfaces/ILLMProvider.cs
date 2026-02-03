using System.Runtime.CompilerServices;
using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Interfaces;

/// <summary>
/// Defines the contract for LLM provider implementations.
/// Each provider (Claude, OpenAI, etc.) implements this interface.
/// </summary>
public interface ILLMProvider : IDisposable
{
    /// <summary>
    /// Unique identifier for this provider (e.g., "claude", "openai").
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Display name for UI (e.g., "Claude", "ChatGPT").
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Whether this provider instance is properly configured with valid credentials.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Validates the provided API key against the provider's API.
    /// </summary>
    /// <param name="apiKey">The API key to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the key is valid, false otherwise.</returns>
    Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default);

    /// <summary>
    /// Gets the list of available models for this provider.
    /// </summary>
    IReadOnlyList<ModelInfo> GetAvailableModels();

    /// <summary>
    /// Estimates the token count for the given text using provider-specific tokenization.
    /// </summary>
    /// <param name="text">The text to estimate tokens for.</param>
    /// <returns>Estimated token count.</returns>
    int EstimateTokens(string text);

    /// <summary>
    /// Estimates tokens for a full request including context.
    /// </summary>
    /// <param name="request">The request to estimate.</param>
    /// <returns>Estimated input token count.</returns>
    int EstimateRequestTokens(LLMRequest request);

    /// <summary>
    /// Sends a request and returns the complete response.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The complete response.</returns>
    Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken ct = default);

    /// <summary>
    /// Sends a request and streams the response chunks.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Async enumerable of response text chunks.</returns>
    IAsyncEnumerable<StreamChunk> StreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken ct = default);
}
