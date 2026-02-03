using MultiLLMApp.Core.Models;
using MultiLLMApp.Core.Providers;
using MultiLLMApp.Core.Services;
using MultiLLMApp.Data;

namespace MultiLLMApp.Demo;

/// <summary>
/// Console demo application to test Multi-LLM functionality.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("  MultiLLM App - Console Demo");
        Console.WriteLine("==========================================");
        Console.WriteLine();

        // Get API key from environment or prompt
        var apiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.Write("Enter your Claude API key: ");
            apiKey = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("API key is required. Exiting.");
                return;
            }
        }

        // Setup credential store
        var credentialStore = new SecureVault();
        await credentialStore.StoreAsync("claude", "default", apiKey);

        // Create provider factory and tab manager
        var factory = new ProviderFactory(credentialStore);
        var tabManager = new TabManager(factory);

        Console.WriteLine();
        Console.WriteLine("[+] Creating Claude tab...");

        // Create a tab with Claude
        var tabInfo = await tabManager.CreateTabAsync("claude", "Claude - Mango Query");
        var tabContext = tabManager.GetTabContext(tabInfo.TabId);

        if (tabContext == null)
        {
            Console.WriteLine("Failed to create tab context.");
            return;
        }

        Console.WriteLine($"[✓] Tab created: {tabInfo.Label}");
        Console.WriteLine($"    Provider: {tabInfo.ProviderId}");
        Console.WriteLine($"    Model: {tabInfo.ModelId}");
        Console.WriteLine();

        // Send the query about Dusheri mangoes
        var query = "What are Dusheri mangoes? Tell me about their origin, taste, and season.";

        Console.WriteLine("==========================================");
        Console.WriteLine($"User: {query}");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.Write("Claude: ");

        try
        {
            // Stream the response
            await foreach (var chunk in tabContext.SendMessageAsync(query))
            {
                if (!chunk.IsFinal)
                {
                    Console.Write(chunk.Text);
                }
                else if (chunk.Usage != null)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine($"Tokens used - Input: {chunk.Usage.InputTokens}, Output: {chunk.Usage.OutputTokens}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine($"Session stats:");
        Console.WriteLine($"  Messages: {tabContext.Session.MessageCount}");
        Console.WriteLine($"  Total tokens: {tabContext.Session.TotalTokensUsed}");
        Console.WriteLine("==========================================");

        // Cleanup
        tabManager.Dispose();
    }
}
