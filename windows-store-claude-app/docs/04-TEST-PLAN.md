# Test Plan Document
## Multi-LLM Command App

**Version:** 1.0
**Date:** 2026-02-03

---

## 1. Test Strategy Overview

| Test Level | Scope | Tools | Automation |
|------------|-------|-------|------------|
| Unit Tests | Classes, Methods | xUnit, Moq | 100% |
| Integration | Services, Providers | xUnit, WireMock | 100% |
| Functional | UI Components | WinAppDriver | 90% |
| E2E Acceptance | User Scenarios | WinAppDriver + API | 80% |
| Performance | Load, Memory | BenchmarkDotNet | Manual trigger |

---

## 2. Unit Tests

### 2.1 TabManager Tests

```csharp
[TestClass]
public class TabManagerTests
{
    // TAB-001: Create new tab
    [Fact]
    public async Task CreateTab_WithValidProvider_ReturnsNewTabContext()

    [Fact]
    public async Task CreateTab_AtMaxLimit_ThrowsMaxTabsException()

    // TAB-002: Close tab
    [Fact]
    public async Task CloseTab_ExistingTab_RemovesFromCollection()

    [Fact]
    public async Task CloseTab_NonExistentTab_ReturnsFalse()

    // TAB-003: Switch tabs
    [Fact]
    public void SetActiveTab_ValidTabId_UpdatesActiveTab()

    [Fact]
    public void SetActiveTab_InvalidTabId_ThrowsArgumentException()

    // TAB-005: Restore state
    [Fact]
    public async Task RestoreState_WithSavedTabs_RestoresAllTabs()

    [Fact]
    public async Task RestoreState_EmptyDatabase_CreatesDefaultTab()

    // TAB-006: Max tabs limit
    [Fact]
    public void TabCount_AfterMaxCreations_EqualsMaxLimit()

    // Concurrency
    [Fact]
    public async Task CreateTab_ConcurrentCalls_NoRaceCondition()

    [Fact]
    public async Task CloseTab_WhileCreating_ThreadSafe()
}
```

### 2.2 TabContext Tests

```csharp
[TestClass]
public class TabContextTests
{
    // MSG-001: Send message
    [Fact]
    public async Task SendMessageAsync_ValidInput_ReturnsResponse()

    [Fact]
    public async Task SendMessageAsync_EmptyInput_ThrowsArgumentException()

    // MSG-003: Cancel stream
    [Fact]
    public async Task CancelCurrentRequest_DuringStream_StopsImmediately()

    [Fact]
    public async Task CancelCurrentRequest_NoActiveRequest_NoOp()

    // PROV-003: Switch provider
    [Fact]
    public void ChangeProvider_NewProvider_UpdatesProviderInstance()

    [Fact]
    public void ChangeProvider_SameProvider_NoOp()

    // Context management
    [Fact]
    public void Dispose_WithActiveRequest_CancelsAndCleanup()

    [Fact]
    public void Dispose_MultipleCalls_NoDuplicateDisposal()
}
```

### 2.3 SessionManager Tests

```csharp
[TestClass]
public class SessionManagerTests
{
    // MSG-006: Multi-turn conversation
    [Fact]
    public void AddMessage_UserMessage_AppendsToHistory()

    [Fact]
    public void GetContext_WithHistory_ReturnsAllMessages()

    [Fact]
    public void GetContext_ContextDisabled_ReturnsEmpty()

    // MSG-007: Clear context
    [Fact]
    public void ClearContext_WithMessages_EmptiesHistory()

    // MSG-011: Context toggle
    [Fact]
    public void SetContextEnabled_False_ExcludesHistoryFromContext()

    // Thread safety
    [Fact]
    public void AddMessage_ConcurrentCalls_MaintainsOrder()
}
```

### 2.4 Provider Tests

```csharp
[TestClass]
public class ClaudeProviderTests
{
    // PROV-002: Validate key
    [Fact]
    public async Task ValidateApiKeyAsync_ValidKey_ReturnsTrue()

    [Fact]
    public async Task ValidateApiKeyAsync_InvalidKey_ReturnsFalse()

    // PROV-004: Model list
    [Fact]
    public void GetAvailableModels_ReturnsHaikuSonnetOpus()

    // TOK-001: Token estimation
    [Fact]
    public void EstimateTokens_ShortText_ReturnsAccurateEstimate()

    [Fact]
    public void EstimateTokens_EmptyText_ReturnsZero()

    // Streaming
    [Fact]
    public async Task StreamAsync_ValidRequest_YieldsChunks()

    [Fact]
    public async Task StreamAsync_Cancelled_StopsYielding()
}

[TestClass]
public class OpenAIProviderTests
{
    // Same structure as ClaudeProviderTests
    [Fact]
    public async Task ValidateApiKeyAsync_ValidKey_ReturnsTrue()

    [Fact]
    public void GetAvailableModels_ReturnsGPT4oVariants()

    [Fact]
    public async Task StreamAsync_ValidRequest_YieldsChunks()
}
```

### 2.5 ProviderFactory Tests

```csharp
[TestClass]
public class ProviderFactoryTests
{
    [Fact]
    public void CreateProvider_Claude_ReturnsClaudeProvider()

    [Fact]
    public void CreateProvider_OpenAI_ReturnsOpenAIProvider()

    [Fact]
    public void CreateProvider_Unknown_ThrowsNotSupportedException()

    [Fact]
    public void GetAvailableProviders_ReturnsConfiguredProviders()
}
```

### 2.6 TokenEstimator Tests

```csharp
[TestClass]
public class TokenEstimatorTests
{
    [Theory]
    [InlineData("Hello", 1)]
    [InlineData("Hello world", 2)]
    [InlineData("The quick brown fox", 4)]
    public void Estimate_VariousInputs_ReturnsExpectedRange(string input, int minTokens)

    [Fact]
    public void Estimate_WithContext_IncludesHistoryTokens()

    [Fact]
    public void Estimate_CodeContent_AdjustsForTokenizer()
}
```

---

## 3. Integration Tests

### 3.1 Provider API Integration

```csharp
[TestClass]
public class ClaudeApiIntegrationTests
{
    private WireMockServer _mockServer;

    [Fact]
    public async Task SendAsync_MockedResponse_ParsesCorrectly()

    [Fact]
    public async Task StreamAsync_MockedSSE_YieldsCorrectChunks()

    [Fact]
    public async Task SendAsync_RateLimited_ThrowsRateLimitException()

    [Fact]
    public async Task SendAsync_Timeout_ThrowsTimeoutException()

    [Fact]
    public async Task SendAsync_InvalidJson_ThrowsParseException()
}

[TestClass]
public class OpenAIApiIntegrationTests
{
    // Similar structure with OpenAI-specific response formats
    [Fact]
    public async Task StreamAsync_MockedSSE_YieldsCorrectChunks()

    [Fact]
    public async Task SendAsync_AuthError_ThrowsAuthException()
}
```

### 3.2 Database Integration

```csharp
[TestClass]
public class LocalDatabaseTests
{
    [Fact]
    public async Task SaveTab_NewTab_InsertsRecord()

    [Fact]
    public async Task SaveTab_ExistingTab_UpdatesRecord()

    [Fact]
    public async Task LoadTabs_WithData_ReturnsAllTabs()

    [Fact]
    public async Task DeleteTab_CascadesMessages()

    [Fact]
    public async Task SaveMessages_LargeHistory_Succeeds()
}
```

### 3.3 Credential Store Integration

```csharp
[TestClass]
public class SecureVaultTests
{
    [Fact]
    public void Store_ValidKey_SuccessfullyStores()

    [Fact]
    public void Retrieve_ExistingKey_ReturnsValue()

    [Fact]
    public void Retrieve_NonExistent_ReturnsNull()

    [Fact]
    public void Delete_ExistingKey_RemovesFromVault()

    [Fact]
    public void Store_SpecialCharacters_HandlesCorrectly()
}
```

---

## 4. Functional Tests (UI)

### 4.1 Tab UI Tests

```csharp
[TestClass]
public class TabUITests
{
    private WindowsDriver<WindowsElement> _driver;

    // AT-TAB-001
    [Fact]
    public void ClickNewTabButton_CreatesNewTab()
    {
        var newTabButton = _driver.FindElementByAccessibilityId("NewTabButton");
        var initialCount = GetTabCount();

        newTabButton.Click();

        Assert.Equal(initialCount + 1, GetTabCount());
    }

    // AT-TAB-002
    [Fact]
    public void PressCtrlT_CreatesNewTabAndFocusesInput()
    {
        _driver.Keyboard.SendKeys(Keys.Control + "t" + Keys.Control);

        var input = _driver.FindElementByAccessibilityId("CommandInput");
        Assert.True(input.Selected);
    }

    // AT-TAB-004
    [Fact]
    public void CloseTabWithHistory_ShowsConfirmationDialog()
    {
        // Setup: create tab with history
        SendTestMessage();

        var closeButton = GetActiveTabCloseButton();
        closeButton.Click();

        var dialog = _driver.FindElementByAccessibilityId("ConfirmDialog");
        Assert.NotNull(dialog);
    }

    // AT-TAB-007
    [Fact]
    public void DoubleClickTabHeader_EntersRenameMode()
    {
        var tabHeader = GetActiveTabHeader();
        var actions = new Actions(_driver);
        actions.DoubleClick(tabHeader).Perform();

        var renameInput = _driver.FindElementByAccessibilityId("TabRenameInput");
        Assert.True(renameInput.Displayed);
    }
}
```

### 4.2 Message UI Tests

```csharp
[TestClass]
public class MessageUITests
{
    // AT-MSG-001
    [Fact]
    public void TypeAndClickSend_DisplaysResponse()
    {
        var input = _driver.FindElementByAccessibilityId("CommandInput");
        input.SendKeys("Hello");

        var sendButton = _driver.FindElementByAccessibilityId("SendButton");
        sendButton.Click();

        WaitForResponse();
        var responseArea = _driver.FindElementByAccessibilityId("ResponseArea");
        Assert.False(string.IsNullOrEmpty(responseArea.Text));
    }

    // AT-MSG-002
    [Fact]
    public void PressCtrlEnter_SendsMessage()
    {
        var input = _driver.FindElementByAccessibilityId("CommandInput");
        input.SendKeys("Test message");
        input.SendKeys(Keys.Control + Keys.Enter + Keys.Control);

        WaitForResponse();
        Assert.True(ResponseReceived());
    }

    // AT-MSG-005
    [Fact]
    public void ClickStopDuringStream_HaltsResponse()
    {
        SendLongQuery();

        var stopButton = _driver.FindElementByAccessibilityId("StopButton");
        stopButton.Click();

        Thread.Sleep(500);
        var isStreaming = IsCurrentlyStreaming();
        Assert.False(isStreaming);
    }

    // AT-MSG-009
    [Fact]
    public void ClickCopyButton_CopiesResponseToClipboard()
    {
        SendTestMessage();
        WaitForResponse();

        var copyButton = _driver.FindElementByAccessibilityId("CopyButton");
        copyButton.Click();

        var clipboardText = Clipboard.GetText();
        Assert.False(string.IsNullOrEmpty(clipboardText));
    }
}
```

### 4.3 Provider Selection UI Tests

```csharp
[TestClass]
public class ProviderUITests
{
    // AT-PROV-005
    [Fact]
    public void ChangeProviderInEmptyTab_NoWarning()
    {
        var providerDropdown = _driver.FindElementByAccessibilityId("ProviderSelector");
        providerDropdown.Click();

        var openAIOption = _driver.FindElementByName("OpenAI");
        openAIOption.Click();

        var dialog = TryFindElement("WarningDialog");
        Assert.Null(dialog);
    }

    // AT-PROV-006
    [Fact]
    public void ChangeProviderWithHistory_ShowsWarning()
    {
        SendTestMessage();
        WaitForResponse();

        var providerDropdown = _driver.FindElementByAccessibilityId("ProviderSelector");
        providerDropdown.Click();

        var openAIOption = _driver.FindElementByName("OpenAI");
        openAIOption.Click();

        var dialog = _driver.FindElementByAccessibilityId("WarningDialog");
        Assert.NotNull(dialog);
    }

    // AT-PROV-007
    [Fact]
    public void SelectClaude_ShowsHaikuSonnetOpusModels()
    {
        SelectProvider("Claude");

        var modelDropdown = _driver.FindElementByAccessibilityId("ModelSelector");
        modelDropdown.Click();

        Assert.NotNull(_driver.FindElementByName("Haiku"));
        Assert.NotNull(_driver.FindElementByName("Sonnet"));
        Assert.NotNull(_driver.FindElementByName("Opus"));
    }
}
```

---

## 5. End-to-End Acceptance Tests

### 5.1 User Journey: First-Time Setup

```csharp
[TestClass]
public class FirstTimeSetupE2ETests
{
    [Fact]
    public void E2E_FirstLaunch_CompleteSetupFlow()
    {
        // GIVEN: Fresh install, no saved state
        ClearAllAppData();
        LaunchApp();

        // WHEN: Welcome screen appears
        var welcomeScreen = _driver.FindElementByAccessibilityId("WelcomeScreen");
        Assert.True(welcomeScreen.Displayed);

        // AND: User selects Claude
        var claudeOption = _driver.FindElementByName("Claude");
        claudeOption.Click();

        // AND: User enters API key
        var apiKeyInput = _driver.FindElementByAccessibilityId("ApiKeyInput");
        apiKeyInput.SendKeys(TestConfig.ClaudeApiKey);

        // AND: User clicks Test Connection
        var testButton = _driver.FindElementByAccessibilityId("TestConnectionButton");
        testButton.Click();
        WaitForElement("ConnectionSuccessMessage");

        // AND: User clicks Continue
        var continueButton = _driver.FindElementByAccessibilityId("ContinueButton");
        continueButton.Click();

        // THEN: Main window opens with one Claude tab
        var mainWindow = _driver.FindElementByAccessibilityId("MainWindow");
        Assert.True(mainWindow.Displayed);

        var tabs = _driver.FindElementsByAccessibilityId("TabHeader");
        Assert.Single(tabs);

        var providerLabel = _driver.FindElementByAccessibilityId("CurrentProviderLabel");
        Assert.Contains("Claude", providerLabel.Text);
    }
}
```

### 5.2 User Journey: Multi-Provider Comparison

```csharp
[TestClass]
public class MultiProviderE2ETests
{
    [Fact]
    public void E2E_CompareResponsesAcrossProviders()
    {
        // GIVEN: App with Claude and OpenAI configured
        SetupBothProviders();
        LaunchApp();

        // WHEN: User creates Tab1 with Claude
        var tab1 = CreateNewTab("Claude", "Claude Tab");

        // AND: User creates Tab2 with OpenAI
        var tab2 = CreateNewTab("OpenAI", "GPT Tab");

        // AND: User sends same query to both tabs
        string query = "What is 2+2? Answer in one word.";

        SwitchToTab(tab1);
        SendMessage(query);
        var response1 = WaitForResponseComplete();

        SwitchToTab(tab2);
        SendMessage(query);
        var response2 = WaitForResponseComplete();

        // THEN: Both tabs have responses
        Assert.False(string.IsNullOrEmpty(response1));
        Assert.False(string.IsNullOrEmpty(response2));

        // AND: Responses are independent (may differ)
        // AND: No context mixing
        SwitchToTab(tab1);
        SendMessage("What did I just ask?");
        var followUp1 = WaitForResponseComplete();
        Assert.Contains("2+2", followUp1);

        SwitchToTab(tab2);
        SendMessage("What did I just ask?");
        var followUp2 = WaitForResponseComplete();
        Assert.Contains("2+2", followUp2);
    }

    [Fact]
    public void E2E_ConcurrentRequestsToMultipleProviders()
    {
        // GIVEN: Two tabs with different providers
        var claudeTab = CreateNewTab("Claude", "Claude");
        var openaiTab = CreateNewTab("OpenAI", "GPT");

        // WHEN: Send requests nearly simultaneously
        SwitchToTab(claudeTab);
        var input1 = _driver.FindElementByAccessibilityId("CommandInput");
        input1.SendKeys("Count to 10");

        SwitchToTab(openaiTab);
        var input2 = _driver.FindElementByAccessibilityId("CommandInput");
        input2.SendKeys("Count to 10");

        // Send both
        input2.SendKeys(Keys.Control + Keys.Enter + Keys.Control);
        SwitchToTab(claudeTab);
        input1.SendKeys(Keys.Control + Keys.Enter + Keys.Control);

        // THEN: Both complete without errors
        SwitchToTab(claudeTab);
        WaitForResponseComplete(timeout: 30000);
        var claudeResponse = GetResponseText();

        SwitchToTab(openaiTab);
        WaitForResponseComplete(timeout: 30000);
        var openaiResponse = GetResponseText();

        Assert.False(string.IsNullOrEmpty(claudeResponse));
        Assert.False(string.IsNullOrEmpty(openaiResponse));
    }
}
```

### 5.3 User Journey: Context Isolation

```csharp
[TestClass]
public class ContextIsolationE2ETests
{
    [Fact]
    public void E2E_TabsHaveIndependentContexts()
    {
        // GIVEN: Two Claude tabs
        var tab1 = CreateNewTab("Claude", "Cats");
        var tab2 = CreateNewTab("Claude", "Dogs");

        // WHEN: Discuss different topics
        SwitchToTab(tab1);
        SendMessage("Let's talk about cats. What's a popular breed?");
        WaitForResponseComplete();

        SwitchToTab(tab2);
        SendMessage("Let's talk about dogs. What's a popular breed?");
        WaitForResponseComplete();

        // THEN: Each tab maintains its context
        SwitchToTab(tab1);
        SendMessage("Tell me more about that breed.");
        var catFollowUp = WaitForResponseComplete();
        Assert.True(ContainsCatReference(catFollowUp));

        SwitchToTab(tab2);
        SendMessage("Tell me more about that breed.");
        var dogFollowUp = WaitForResponseComplete();
        Assert.True(ContainsDogReference(dogFollowUp));

        // AND: No cross-contamination
        Assert.False(ContainsDogReference(catFollowUp));
        Assert.False(ContainsCatReference(dogFollowUp));
    }
}
```

### 5.4 User Journey: Error Recovery

```csharp
[TestClass]
public class ErrorRecoveryE2ETests
{
    [Fact]
    public void E2E_NetworkError_RetrySucceeds()
    {
        // GIVEN: Working tab
        var tab = CreateNewTab("Claude", "Test");

        // WHEN: Network fails during request (simulated)
        SimulateNetworkFailure();
        SendMessage("Hello");

        // THEN: Error message appears
        var errorMessage = WaitForElement("ErrorMessage");
        Assert.Contains("network", errorMessage.Text.ToLower());

        // AND: Retry button appears
        var retryButton = _driver.FindElementByAccessibilityId("RetryButton");
        Assert.True(retryButton.Displayed);

        // WHEN: Network restored and retry clicked
        RestoreNetwork();
        retryButton.Click();

        // THEN: Request succeeds
        var response = WaitForResponseComplete();
        Assert.False(string.IsNullOrEmpty(response));
    }

    [Fact]
    public void E2E_TabCrashIsolation()
    {
        // GIVEN: Two tabs
        var tab1 = CreateNewTab("Claude", "Tab1");
        var tab2 = CreateNewTab("Claude", "Tab2");

        // Setup context in Tab2
        SwitchToTab(tab2);
        SendMessage("Remember the code: ALPHA123");
        WaitForResponseComplete();

        // WHEN: Tab1 encounters error
        SwitchToTab(tab1);
        TriggerProviderError();

        // THEN: Tab2 still works
        SwitchToTab(tab2);
        SendMessage("What was the code?");
        var response = WaitForResponseComplete();
        Assert.Contains("ALPHA123", response);
    }
}
```

### 5.5 User Journey: Session Persistence

```csharp
[TestClass]
public class SessionPersistenceE2ETests
{
    [Fact]
    public void E2E_RestoreSessionAfterRestart()
    {
        // GIVEN: Multiple tabs with history
        var tab1 = CreateNewTab("Claude", "Session1");
        SendMessage("My favorite number is 42");
        WaitForResponseComplete();

        var tab2 = CreateNewTab("OpenAI", "Session2");
        SendMessage("My favorite color is blue");
        WaitForResponseComplete();

        // WHEN: App is closed and reopened
        CloseApp();
        LaunchApp();

        // THEN: Tabs are restored
        var tabs = _driver.FindElementsByAccessibilityId("TabHeader");
        Assert.Equal(2, tabs.Count);

        // AND: History is preserved
        SwitchToTabByName("Session1");
        var history1 = GetConversationHistory();
        Assert.Contains("42", history1);

        SwitchToTabByName("Session2");
        var history2 = GetConversationHistory();
        Assert.Contains("blue", history2);

        // AND: Context works
        SendMessage("What was my favorite?");
        var response = WaitForResponseComplete();
        Assert.Contains("blue", response.ToLower());
    }
}
```

---

## 6. Performance Tests

### 6.1 Load Tests

```csharp
[TestClass]
public class PerformanceTests
{
    [Fact]
    public void Perf_AppStartup_Under1Second()
    {
        var stopwatch = Stopwatch.StartNew();
        LaunchApp();
        WaitForMainWindow();
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 1000);
    }

    [Fact]
    public void Perf_CreateMaxTabs_MemoryUnder100MB()
    {
        LaunchApp();
        var initialMemory = GetAppMemoryMB();

        for (int i = 0; i < 10; i++)
        {
            CreateNewTab("Claude", $"Tab{i}");
        }

        var finalMemory = GetAppMemoryMB();
        Assert.True(finalMemory < 100);
        Assert.True(finalMemory - initialMemory < 50); // +10MB per tab max
    }

    [Fact]
    public void Perf_StreamingResponse_FirstChunkUnder500ms()
    {
        var tab = CreateNewTab("Claude", "Test");
        SendMessageAsync("Write a long story");

        var stopwatch = Stopwatch.StartNew();
        WaitForFirstStreamChunk();
        stopwatch.Stop();

        Assert.True(stopwatch.ElapsedMilliseconds < 500);
    }

    [Fact]
    public void Perf_LargeConversation_NoMemoryLeak()
    {
        var tab = CreateNewTab("Claude", "Test");

        var initialMemory = GetAppMemoryMB();

        for (int i = 0; i < 50; i++)
        {
            SendMessage($"Message {i}");
            WaitForResponseComplete();
        }

        GC.Collect();
        Thread.Sleep(1000);

        var finalMemory = GetAppMemoryMB();
        // Memory should not grow unbounded
        Assert.True(finalMemory < initialMemory + 30);
    }
}
```

---

## 7. Test Coverage Requirements

| Component | Min Coverage | Target Coverage |
|-----------|--------------|-----------------|
| TabManager | 90% | 95% |
| TabContext | 85% | 90% |
| SessionManager | 90% | 95% |
| Providers | 80% | 85% |
| ViewModels | 75% | 80% |
| Overall | 80% | 85% |

---

## 8. Test Environment

### 8.1 Requirements

| Resource | Specification |
|----------|---------------|
| OS | Windows 10/11 (latest) |
| RAM | 8GB minimum |
| WinAppDriver | v1.2.1+ |
| .NET | 8.0 SDK |
| API Keys | Test keys for Claude, OpenAI |

### 8.2 CI/CD Pipeline

```yaml
stages:
  - stage: UnitTests
    jobs:
      - job: RunUnitTests
        steps:
          - script: dotnet test tests/MultiLLMApp.UnitTests

  - stage: IntegrationTests
    dependsOn: UnitTests
    jobs:
      - job: RunIntegrationTests
        steps:
          - script: dotnet test tests/MultiLLMApp.IntegrationTests

  - stage: FunctionalTests
    dependsOn: IntegrationTests
    jobs:
      - job: RunFunctionalTests
        pool: windows-latest
        steps:
          - script: Start-Process WinAppDriver
          - script: dotnet test tests/MultiLLMApp.FunctionalTests

  - stage: E2ETests
    dependsOn: FunctionalTests
    condition: eq(variables['Build.Reason'], 'Manual')
    jobs:
      - job: RunE2ETests
        steps:
          - script: dotnet test tests/MultiLLMApp.E2ETests
```

---

## 9. Defect Severity Levels

| Severity | Definition | Example |
|----------|------------|---------|
| S1 - Critical | App unusable | Crash on launch, data loss |
| S2 - Major | Feature broken | Cannot send messages, provider fails |
| S3 - Minor | Feature impaired | Keyboard shortcut fails, UI glitch |
| S4 - Cosmetic | Visual only | Alignment issue, typo |

---

## 10. Exit Criteria

| Milestone | Criteria |
|-----------|----------|
| Alpha | All unit tests pass, 80% coverage |
| Beta | All integration tests pass, no S1 defects |
| RC | All functional tests pass, no S1/S2 defects |
| Release | All E2E tests pass, no S1/S2, max 5 S3 defects |
