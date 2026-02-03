using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MultiLLMApp.Core.Interfaces;
using MultiLLMApp.Core.Models;
using MultiLLMApp.Core.Services;

namespace MultiLLMApp.Core.Providers;

/// <summary>
/// Abstract base class for LLM providers with common functionality.
/// </summary>
public abstract class BaseProvider : ILLMProvider
{
    protected readonly HttpClient HttpClient;
    protected readonly TokenEstimator TokenEstimator;
    protected readonly string ApiKey;

    private bool _disposed;

    public abstract string ProviderId { get; }
    public abstract string DisplayName { get; }
    public bool IsConfigured => !string.IsNullOrEmpty(ApiKey);

    protected BaseProvider(string apiKey, HttpClient? httpClient = null)
    {
        ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        HttpClient = httpClient ?? new HttpClient();
        TokenEstimator = new TokenEstimator();
    }

    public abstract Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default);
    public abstract IReadOnlyList<ModelInfo> GetAvailableModels();

    public virtual int EstimateTokens(string text)
    {
        return TokenEstimator.EstimateTokens(text);
    }

    public virtual int EstimateRequestTokens(LLMRequest request)
    {
        return TokenEstimator.EstimateRequestTokens(request);
    }

    public abstract Task<LLMResponse> SendAsync(LLMRequest request, CancellationToken ct = default);

    public abstract IAsyncEnumerable<StreamChunk> StreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken ct = default);

    /// <summary>
    /// Handles common HTTP errors and converts to appropriate exceptions.
    /// </summary>
    protected LLMException HandleHttpError(HttpResponseMessage response, string? responseBody)
    {
        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized or
            System.Net.HttpStatusCode.Forbidden =>
                new AuthenticationException(ProviderId),

            System.Net.HttpStatusCode.TooManyRequests =>
                CreateRateLimitException(response),

            System.Net.HttpStatusCode.RequestTimeout or
            System.Net.HttpStatusCode.GatewayTimeout =>
                new ProviderTimeoutException(ProviderId, TimeSpan.FromSeconds(30)),

            System.Net.HttpStatusCode.ServiceUnavailable or
            System.Net.HttpStatusCode.BadGateway =>
                new ProviderUnavailableException(ProviderId),

            _ => new InvalidResponseException(ProviderId,
                $"HTTP {(int)response.StatusCode}: {responseBody ?? "Unknown error"}")
        };
    }

    private RateLimitException CreateRateLimitException(HttpResponseMessage response)
    {
        TimeSpan? retryAfter = null;
        if (response.Headers.RetryAfter?.Delta.HasValue == true)
        {
            retryAfter = response.Headers.RetryAfter.Delta;
        }
        return new RateLimitException(ProviderId, retryAfter);
    }

    /// <summary>
    /// Safely reads SSE lines from a stream.
    /// </summary>
    protected static async IAsyncEnumerable<string> ReadSSELinesAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line != null)
            {
                yield return line;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            HttpClient.Dispose();
        }

        _disposed = true;
    }
}
