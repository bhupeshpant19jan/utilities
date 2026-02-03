# Technical Specification Document
## Multi-LLM Command App

**Version:** 1.0
**Date:** 2026-02-03

---

## 1. System Architecture

### 1.1 High-Level Block Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              PRESENTATION LAYER                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐              │
│  │   MainWindow    │  │   TabControl    │  │  SettingsPage   │              │
│  │   (Shell)       │  │   (Tab Host)    │  │  (Config UI)    │              │
│  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘              │
│           │                    │                    │                        │
│  ┌────────┴────────────────────┴────────────────────┴────────┐              │
│  │                      ViewModels (MVVM)                     │              │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │              │
│  │  │MainWindowVM  │  │  TabViewModel │  │SettingsVM   │     │              │
│  │  └──────────────┘  └──────────────┘  └──────────────┘     │              │
│  └───────────────────────────┬───────────────────────────────┘              │
└──────────────────────────────┼──────────────────────────────────────────────┘
                               │
┌──────────────────────────────┼──────────────────────────────────────────────┐
│                         SERVICE LAYER                                        │
│  ┌───────────────────────────┴───────────────────────────┐                  │
│  │                    TabManager                          │                  │
│  │    (Orchestrates tab lifecycle, ensures isolation)     │                  │
│  └───────────────────────────┬───────────────────────────┘                  │
│                              │                                               │
│  ┌───────────────┬───────────┴───────────┬───────────────┐                  │
│  │               │                       │               │                  │
│  ▼               ▼                       ▼               ▼                  │
│ ┌─────────┐ ┌─────────┐ ┌─────────────────────┐ ┌─────────────┐            │
│ │SessionMgr│ │TokenSvc │ │  ProviderFactory    │ │SettingsSvc  │            │
│ │(per tab) │ │         │ │                     │ │             │            │
│ └─────────┘ └─────────┘ └──────────┬──────────┘ └─────────────┘            │
│                                    │                                        │
│                    ┌───────────────┼───────────────┐                        │
│                    ▼               ▼               ▼                        │
│              ┌──────────┐   ┌──────────┐   ┌──────────┐                     │
│              │ Claude   │   │ OpenAI   │   │ Custom   │                     │
│              │ Provider │   │ Provider │   │ Provider │                     │
│              └──────────┘   └──────────┘   └──────────┘                     │
└─────────────────────────────────────────────────────────────────────────────┘
                               │
┌──────────────────────────────┼──────────────────────────────────────────────┐
│                          DATA LAYER                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐              │
│  │ SecureVault     │  │ LocalDatabase   │  │ FileStorage     │              │
│  │ (Credentials)   │  │ (SQLite)        │  │ (Export/Import) │              │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Component Specifications

### 2.1 Class Diagram

```
┌────────────────────────────────────────────────────────────────────────────┐
│                              INTERFACES                                     │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌─────────────────────────┐      ┌─────────────────────────┐             │
│  │   <<interface>>         │      │   <<interface>>         │             │
│  │   ILLMProvider          │      │   ISessionManager       │             │
│  ├─────────────────────────┤      ├─────────────────────────┤             │
│  │ +SendAsync()            │      │ +AddMessage()           │             │
│  │ +StreamAsync()          │      │ +GetContext()           │             │
│  │ +CancelRequest()        │      │ +ClearContext()         │             │
│  │ +ValidateKey()          │      │ +GetHistory()           │             │
│  │ +GetModels()            │      │ +SetContextEnabled()    │             │
│  │ +EstimateTokens()       │      └─────────────────────────┘             │
│  └─────────────────────────┘                                              │
│              △                    ┌─────────────────────────┐             │
│              │                    │   <<interface>>         │             │
│   ┌──────────┼──────────┐         │   ICredentialStore      │             │
│   │          │          │         ├─────────────────────────┤             │
│   ▼          ▼          ▼         │ +Store()                │             │
│ ┌──────┐ ┌──────┐ ┌──────┐        │ +Retrieve()             │             │
│ │Claude│ │OpenAI│ │Custom│        │ +Delete()               │             │
│ │Provdr│ │Provdr│ │Provdr│        │ +Exists()               │             │
│ └──────┘ └──────┘ └──────┘        └─────────────────────────┘             │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────────────────┐
│                              CORE CLASSES                                   │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                            │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                          TabManager                                  │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │ - _tabs: ConcurrentDictionary<Guid, TabContext>                     │   │
│  │ - _activeTabId: Guid                                                │   │
│  │ - _maxTabs: int = 10                                                │   │
│  │ - _lock: SemaphoreSlim                                              │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │ + CreateTab(providerId): TabContext                                 │   │
│  │ + CloseTab(tabId): bool                                             │   │
│  │ + GetTab(tabId): TabContext                                         │   │
│  │ + SetActiveTab(tabId): void                                         │   │
│  │ + GetAllTabs(): IReadOnlyList<TabContext>                           │   │
│  │ + SaveState(): Task                                                 │   │
│  │ + RestoreState(): Task                                              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                       │                                    │
│                                       │ owns                               │
│                                       ▼                                    │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                          TabContext                                  │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │ + TabId: Guid                                                       │   │
│  │ + Label: string                                                     │   │
│  │ + Provider: ILLMProvider                                            │   │
│  │ + Session: ISessionManager                                          │   │
│  │ + CancellationTokenSource: CTS                                      │   │
│  │ + IsStreaming: bool                                                 │   │
│  │ + CurrentModel: string                                              │   │
│  │ + MaxTokens: int                                                    │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │ + SendMessageAsync(text): IAsyncEnumerable<string>                  │   │
│  │ + CancelCurrentRequest(): void                                      │   │
│  │ + ChangeProvider(providerId): void                                  │   │
│  │ + Dispose(): void                                                   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                            │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                      ProviderFactory                                 │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │ - _credentialStore: ICredentialStore                                │   │
│  │ - _httpClientFactory: IHttpClientFactory                            │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │ + CreateProvider(providerId): ILLMProvider                          │   │
│  │ + GetAvailableProviders(): IList<ProviderInfo>                      │   │
│  │ + RegisterCustomProvider(config): void                              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                            │
└────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Data Flow Diagrams

### 3.1 Send Message Flow

```
┌──────┐      ┌──────────┐      ┌────────────┐      ┌──────────┐      ┌─────────┐
│ User │      │ TabView  │      │ TabContext │      │ Session  │      │Provider │
│      │      │ Model    │      │            │      │ Manager  │      │         │
└──┬───┘      └────┬─────┘      └─────┬──────┘      └────┬─────┘      └────┬────┘
   │               │                  │                  │                 │
   │ Type+Send     │                  │                  │                 │
   │──────────────>│                  │                  │                 │
   │               │                  │                  │                 │
   │               │ SendMessageAsync │                  │                 │
   │               │─────────────────>│                  │                 │
   │               │                  │                  │                 │
   │               │                  │ AddMessage(user) │                 │
   │               │                  │─────────────────>│                 │
   │               │                  │                  │                 │
   │               │                  │ GetContext()     │                 │
   │               │                  │─────────────────>│                 │
   │               │                  │<─────────────────│                 │
   │               │                  │  messages[]      │                 │
   │               │                  │                  │                 │
   │               │                  │ StreamAsync(messages)              │
   │               │                  │────────────────────────────────────>
   │               │                  │                  │                 │
   │               │    ┌─────────────┴──────────────────┴─────────────────┤
   │               │    │  STREAMING LOOP                                  │
   │               │    │                                                  │
   │               │<───│──chunk─────────────────────────────────────────<─│
   │<──────────────│    │                                                  │
   │  Display      │<───│──chunk─────────────────────────────────────────<─│
   │<──────────────│    │                                                  │
   │  Display      │    │                                                  │
   │               │    └─────────────┬──────────────────┬─────────────────┤
   │               │                  │                  │                 │
   │               │                  │ AddMessage(asst) │                 │
   │               │                  │─────────────────>│                 │
   │               │                  │                  │                 │
   │               │ Complete         │                  │                 │
   │<──────────────│<─────────────────│                  │                 │
   │               │                  │                  │                 │
```

### 3.2 Tab Isolation Flow

```
┌───────────────────────────────────────────────────────────────────────────┐
│                              TabManager                                    │
│  ┌─────────────────────────────────────────────────────────────────────┐  │
│  │              ConcurrentDictionary<Guid, TabContext>                  │  │
│  │                                                                      │  │
│  │   ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐ │  │
│  │   │   TabContext    │    │   TabContext    │    │   TabContext    │ │  │
│  │   │   (Tab 1)       │    │   (Tab 2)       │    │   (Tab 3)       │ │  │
│  │   │   ════════════  │    │   ════════════  │    │   ════════════  │ │  │
│  │   │                 │    │                 │    │                 │ │  │
│  │   │ ┌─────────────┐ │    │ ┌─────────────┐ │    │ ┌─────────────┐ │ │  │
│  │   │ │ Session     │ │    │ │ Session     │ │    │ │ Session     │ │ │  │
│  │   │ │ (isolated)  │ │    │ │ (isolated)  │ │    │ │ (isolated)  │ │ │  │
│  │   │ │ ─messages[] │ │    │ │ ─messages[] │ │    │ │ ─messages[] │ │ │  │
│  │   │ └─────────────┘ │    │ └─────────────┘ │    │ └─────────────┘ │ │  │
│  │   │                 │    │                 │    │                 │ │  │
│  │   │ ┌─────────────┐ │    │ ┌─────────────┐ │    │ ┌─────────────┐ │ │  │
│  │   │ │ CTS         │ │    │ │ CTS         │ │    │ │ CTS         │ │ │  │
│  │   │ │ (cancel)    │ │    │ │ (cancel)    │ │    │ │ (cancel)    │ │ │  │
│  │   │ └─────────────┘ │    │ └─────────────┘ │    │ └─────────────┘ │ │  │
│  │   │                 │    │                 │    │                 │ │  │
│  │   │ Provider:Claude │    │ Provider:OpenAI│    │ Provider:Claude │ │  │
│  │   │ Model: Haiku    │    │ Model: GPT-4o  │    │ Model: Sonnet   │ │  │
│  │   └─────────────────┘    └─────────────────┘    └─────────────────┘ │  │
│  │           │                      │                      │           │  │
│  └───────────┼──────────────────────┼──────────────────────┼───────────┘  │
│              │                      │                      │              │
│              ▼                      ▼                      ▼              │
│         No Shared State Between TabContexts                               │
└───────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Concurrency & Thread Safety

### 4.1 Race Condition Prevention

| Component | Synchronization Mechanism | Protected Resource |
|-----------|---------------------------|-------------------|
| TabManager | `SemaphoreSlim(1,1)` | Tab creation/deletion |
| TabContext | `CancellationTokenSource` | Request cancellation |
| SessionManager | `lock` object | Message list modification |
| CredentialStore | OS-level (Vault) | API keys |
| Settings | `ReaderWriterLockSlim` | Config read/write |

### 4.2 Tab Context Isolation Strategy

```csharp
// Each TabContext owns its own instances - NO sharing
public class TabContext : IDisposable
{
    // Private instances - not shared with other tabs
    private readonly ISessionManager _session;      // Unique per tab
    private readonly ILLMProvider _provider;        // Unique instance per tab
    private readonly CancellationTokenSource _cts;  // Unique per tab

    // No static state, no shared collections
    // Each tab operates on its own thread context
}
```

### 4.3 Concurrent Request Handling

```
Tab1: SendAsync()─────────────────────────────────────>Complete
                    │
Tab2:               │  SendAsync()────────────────────────────>Complete
                    │        │
Tab3:               │        │     SendAsync()────────>Cancel (user)
                    │        │           │
                    ▼        ▼           ▼
              HttpClient instances are SEPARATE per provider instance
              No request queuing - true parallel execution
```

---

## 5. Provider Abstraction Layer

### 5.1 Provider Interface Contract

```csharp
public interface ILLMProvider
{
    string ProviderId { get; }

    Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken ct);

    IReadOnlyList<ModelInfo> GetAvailableModels();

    int EstimateTokens(string text);

    Task<LLMResponse> SendAsync(
        LLMRequest request,
        CancellationToken ct);

    IAsyncEnumerable<string> StreamAsync(
        LLMRequest request,
        CancellationToken ct);

    void CancelPendingRequests();
}
```

### 5.2 Provider Implementation Pattern

```csharp
public class ClaudeProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;  // Owned, not shared
    private readonly string _apiKey;

    public async IAsyncEnumerable<string> StreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var httpRequest = BuildRequest(request);

        using var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (TryParseSSE(line, out var content))
            {
                yield return content;
            }
        }
    }
}
```

---

## 6. State Persistence

### 6.1 Data Storage Schema

```sql
-- SQLite Schema

CREATE TABLE tabs (
    tab_id TEXT PRIMARY KEY,
    label TEXT NOT NULL,
    provider_id TEXT NOT NULL,
    model_id TEXT NOT NULL,
    context_enabled INTEGER DEFAULT 1,
    max_tokens INTEGER DEFAULT 1024,
    created_at TEXT NOT NULL,
    last_active_at TEXT NOT NULL,
    tab_order INTEGER NOT NULL
);

CREATE TABLE messages (
    message_id TEXT PRIMARY KEY,
    tab_id TEXT NOT NULL,
    role TEXT NOT NULL,
    content TEXT NOT NULL,
    timestamp TEXT NOT NULL,
    token_count INTEGER,
    FOREIGN KEY (tab_id) REFERENCES tabs(tab_id) ON DELETE CASCADE
);

CREATE TABLE settings (
    key TEXT PRIMARY KEY,
    value TEXT NOT NULL
);

CREATE INDEX idx_messages_tab ON messages(tab_id);
```

### 6.2 State Save/Restore Flow

```
┌──────────────┐        ┌──────────────┐        ┌──────────────┐
│  App Close   │        │  TabManager  │        │  SQLite DB   │
└──────┬───────┘        └──────┬───────┘        └──────┬───────┘
       │                       │                       │
       │ OnSuspending          │                       │
       │──────────────────────>│                       │
       │                       │                       │
       │                       │ foreach tab:          │
       │                       │   SerializeState()    │
       │                       │──────────────────────>│
       │                       │                       │ INSERT/UPDATE
       │                       │<──────────────────────│
       │                       │                       │
       │ Complete              │                       │
       │<──────────────────────│                       │


┌──────────────┐        ┌──────────────┐        ┌──────────────┐
│  App Start   │        │  TabManager  │        │  SQLite DB   │
└──────┬───────┘        └──────┬───────┘        └──────┬───────┘
       │                       │                       │
       │ OnLaunched            │                       │
       │──────────────────────>│                       │
       │                       │                       │
       │                       │ SELECT * FROM tabs    │
       │                       │──────────────────────>│
       │                       │<──────────────────────│
       │                       │  tab records          │
       │                       │                       │
       │                       │ foreach record:       │
       │                       │   CreateTabContext()  │
       │                       │   RestoreMessages()   │
       │                       │                       │
       │ Tabs Restored         │                       │
       │<──────────────────────│                       │
```

---

## 7. Error Handling Strategy

```
┌─────────────────────────────────────────────────────────────────┐
│                      Error Handler Chain                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   Provider Layer          Service Layer         UI Layer        │
│   ─────────────          ─────────────         ────────         │
│                                                                 │
│   HttpException  ───────> ProviderException ───> ErrorDialog    │
│   JsonException  ───────> ProviderException ───> ErrorDialog    │
│   TimeoutException ─────> RetryableException ──> RetryPrompt    │
│   RateLimitException ───> RetryableException ──> Countdown      │
│   AuthException  ───────> ConfigException ─────> SettingsLink   │
│   CancelledException ───> (swallow) ───────────> (no action)    │
│                                                                 │
│   All unhandled  ───────> GlobalExceptionHandler ─> CrashReport │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 8. Memory Management

### 8.1 Resource Lifecycle

| Resource | Creation | Disposal |
|----------|----------|----------|
| TabContext | User creates tab | User closes tab or app exit |
| HttpClient | Provider construction | Provider disposal |
| CancellationTokenSource | Request start | Request complete or cancel |
| StreamReader | Stream start | Stream end (using) |
| SQLite Connection | Query start | Query end (using) |

### 8.2 Memory Limits

```csharp
public class TabContext
{
    private const int MaxMessagesInMemory = 100;
    private const int MaxMessageContentLength = 50_000;

    // Trim old messages when limit exceeded
    private void TrimMessagesIfNeeded()
    {
        if (_messages.Count > MaxMessagesInMemory)
        {
            // Archive to DB, keep last 50 in memory
            ArchiveMessages(_messages.Take(50));
            _messages = _messages.Skip(50).ToList();
        }
    }
}
```

---

## 9. Security Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Security Boundaries                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │  Windows Credential Vault (OS-Level Encryption)          │   │
│   │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐      │   │
│   │  │ Claude Key  │  │ OpenAI Key  │  │ Custom Keys │      │   │
│   │  └─────────────┘  └─────────────┘  └─────────────┘      │   │
│   └─────────────────────────────────────────────────────────┘   │
│                              │                                   │
│                              │ Retrieved at runtime only         │
│                              ▼                                   │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │  App Memory (Process Isolation)                          │   │
│   │  - Keys held only during API call                        │   │
│   │  - Never written to logs                                 │   │
│   │  - Never included in crash reports                       │   │
│   └─────────────────────────────────────────────────────────┘   │
│                              │                                   │
│                              │ HTTPS Only                        │
│                              ▼                                   │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │  External API (TLS 1.3)                                  │   │
│   └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 10. Project File Structure

```
MultiLLMApp/
├── MultiLLMApp.sln
├── src/
│   ├── MultiLLMApp/
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── Package.appxmanifest
│   │   │
│   │   ├── Views/
│   │   │   ├── MainWindow.xaml
│   │   │   ├── TabContentView.xaml
│   │   │   └── SettingsPage.xaml
│   │   │
│   │   ├── ViewModels/
│   │   │   ├── MainWindowViewModel.cs
│   │   │   ├── TabViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   │
│   │   └── Assets/
│   │
│   ├── MultiLLMApp.Core/
│   │   ├── Interfaces/
│   │   │   ├── ILLMProvider.cs
│   │   │   ├── ISessionManager.cs
│   │   │   └── ICredentialStore.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── TabManager.cs
│   │   │   ├── TabContext.cs
│   │   │   ├── SessionManager.cs
│   │   │   ├── ProviderFactory.cs
│   │   │   └── TokenEstimator.cs
│   │   │
│   │   ├── Providers/
│   │   │   ├── ClaudeProvider.cs
│   │   │   ├── OpenAIProvider.cs
│   │   │   └── BaseProvider.cs
│   │   │
│   │   └── Models/
│   │       ├── LLMRequest.cs
│   │       ├── LLMResponse.cs
│   │       ├── Message.cs
│   │       └── ProviderConfig.cs
│   │
│   └── MultiLLMApp.Data/
│       ├── SecureVault.cs
│       ├── LocalDatabase.cs
│       └── Migrations/
│
└── tests/
    ├── MultiLLMApp.UnitTests/
    ├── MultiLLMApp.IntegrationTests/
    └── MultiLLMApp.E2ETests/
```
