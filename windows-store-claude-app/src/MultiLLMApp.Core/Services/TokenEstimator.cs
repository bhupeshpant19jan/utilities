using MultiLLMApp.Core.Models;

namespace MultiLLMApp.Core.Services;

/// <summary>
/// Estimates token counts for text content.
/// Uses approximate calculations that work across providers.
/// </summary>
public sealed class TokenEstimator
{
    // Average characters per token (conservative estimate)
    private const double CharsPerToken = 4.0;

    // Overhead tokens per message for formatting
    private const int MessageOverhead = 4;

    /// <summary>
    /// Estimates tokens for plain text.
    /// </summary>
    /// <param name="text">The text to estimate.</param>
    /// <returns>Estimated token count.</returns>
    public int EstimateTokens(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Basic estimation: characters / chars_per_token
        // This is intentionally conservative (slightly overestimates)
        var baseEstimate = (int)Math.Ceiling(text.Length / CharsPerToken);

        // Adjust for code content (typically more tokens)
        if (LooksLikeCode(text))
        {
            baseEstimate = (int)(baseEstimate * 1.2);
        }

        return Math.Max(1, baseEstimate);
    }

    /// <summary>
    /// Estimates tokens for a single message including overhead.
    /// </summary>
    public int EstimateMessageTokens(Message message)
    {
        return EstimateTokens(message.Content) + MessageOverhead;
    }

    /// <summary>
    /// Estimates total tokens for a list of messages.
    /// </summary>
    public int EstimateMessagesTokens(IEnumerable<Message> messages)
    {
        return messages.Sum(EstimateMessageTokens);
    }

    /// <summary>
    /// Estimates tokens for a complete request.
    /// </summary>
    public int EstimateRequestTokens(LLMRequest request)
    {
        var total = 0;

        // System prompt
        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            total += EstimateTokens(request.SystemPrompt) + MessageOverhead;
        }

        // Messages
        total += EstimateMessagesTokens(request.Messages);

        // Request overhead (formatting, special tokens)
        total += 10;

        return total;
    }

    /// <summary>
    /// Estimates cost for a request given token pricing.
    /// </summary>
    public decimal EstimateCost(int inputTokens, int outputTokens, ModelInfo model)
    {
        var inputCost = (inputTokens / 1_000_000m) * model.InputTokenCostPer1M;
        var outputCost = (outputTokens / 1_000_000m) * model.OutputTokenCostPer1M;
        return inputCost + outputCost;
    }

    /// <summary>
    /// Checks if text appears to be code.
    /// </summary>
    private static bool LooksLikeCode(string text)
    {
        // Simple heuristics for code detection
        var codeIndicators = new[]
        {
            "function ", "class ", "public ", "private ", "const ",
            "var ", "let ", "def ", "import ", "from ",
            "if (", "for (", "while (", "switch (",
            "=> ", "->", "::", "();", "[]", "{}", "<>",
            "```"
        };

        var lowerText = text.ToLowerInvariant();
        var indicatorCount = codeIndicators.Count(i => lowerText.Contains(i.ToLowerInvariant()));

        // If multiple indicators found, likely code
        return indicatorCount >= 2;
    }
}
