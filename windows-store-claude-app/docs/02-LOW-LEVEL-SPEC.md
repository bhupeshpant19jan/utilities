# Low-Level Specification Document
## Multi-LLM Command App

**Version:** 1.0
**Date:** 2026-02-03

---

## 1. Requirements Traceability Matrix

### 1.1 Tab Management

| Req ID | Requirement | Acceptance Tests | PRD Ref |
|--------|-------------|------------------|---------|
| TAB-001 | Create new tab with default provider | AT-TAB-001, AT-TAB-002 | F1 |
| TAB-002 | Close tab with confirmation if unsaved | AT-TAB-003, AT-TAB-004 | F1 |
| TAB-003 | Switch between tabs without data loss | AT-TAB-005, AT-TAB-006 | F1 |
| TAB-004 | Rename tab with custom label | AT-TAB-007 | F9 |
| TAB-005 | Restore tabs on app restart | AT-TAB-008, AT-TAB-009 | F1 |
| TAB-006 | Maximum 10 concurrent tabs | AT-TAB-010 | F1 |
| TAB-007 | Tab reordering via drag-drop | AT-TAB-011 | F1 |

**Acceptance Tests - Tab Management:**

| Test ID | Test Description | Expected Result |
|---------|------------------|-----------------|
| AT-TAB-001 | Click "+" button | New tab created with default provider |
| AT-TAB-002 | Press Ctrl+T | New tab created, input focused |
| AT-TAB-003 | Close tab with empty history | Tab closes immediately |
| AT-TAB-004 | Close tab with conversation | Confirmation dialog appears |
| AT-TAB-005 | Type in Tab1, switch to Tab2, return | Tab1 content preserved |
| AT-TAB-006 | Stream response, switch tab, return | Stream continues, display updated |
| AT-TAB-007 | Double-click tab header | Rename mode activated |
| AT-TAB-008 | Close app with 3 tabs, reopen | 3 tabs restored with history |
| AT-TAB-009 | Force-close during stream | Partial response saved on restart |
| AT-TAB-010 | Attempt to create 11th tab | Error message, creation blocked |
| AT-TAB-011 | Drag Tab2 before Tab1 | Tabs reordered, contexts intact |

---

### 1.2 Provider Management

| Req ID | Requirement | Acceptance Tests | PRD Ref |
|--------|-------------|------------------|---------|
| PROV-001 | Configure multiple API keys per provider | AT-PROV-001, AT-PROV-002 | F3 |
| PROV-002 | Test connection before saving | AT-PROV-003, AT-PROV-004 | F3 |
| PROV-003 | Switch provider within tab | AT-PROV-005, AT-PROV-006 | F2 |
| PROV-004 | Provider-specific model list | AT-PROV-007 | F7 |
| PROV-005 | Graceful fallback on provider error | AT-PROV-008, AT-PROV-009 | F2 |
| PROV-006 | API key secure storage | AT-PROV-010, AT-PROV-011 | F3 |

**Acceptance Tests - Provider Management:**

| Test ID | Test Description | Expected Result |
|---------|------------------|-----------------|
| AT-PROV-001 | Add Claude API key | Key stored, provider available |
| AT-PROV-002 | Add multiple OpenAI keys | All keys stored, selectable |
| AT-PROV-003 | Test valid API key | "Connection successful" message |
| AT-PROV-004 | Test invalid API key | "Authentication failed" error |
| AT-PROV-005 | Change provider in empty tab | Provider changed, no warning |
| AT-PROV-006 | Change provider with history | Warning dialog, clear option |
| AT-PROV-007 | Select Claude provider | Haiku/Sonnet/Opus models shown |
| AT-PROV-008 | Send with network down | Retry prompt with timeout |
| AT-PROV-009 | Send with rate limit hit | Rate limit message, retry countdown |
| AT-PROV-010 | Export app data | API keys NOT included |
| AT-PROV-011 | Access credential store externally | Keys encrypted, unreadable |

---

### 1.3 Message Handling

| Req ID | Requirement | Acceptance Tests | PRD Ref |
|--------|-------------|------------------|---------|
| MSG-001 | Send single message | AT-MSG-001, AT-MSG-002 | F4 |
| MSG-002 | Stream response in real-time | AT-MSG-003, AT-MSG-004 | F5 |
| MSG-003 | Cancel in-progress stream | AT-MSG-005, AT-MSG-006 | F5 |
| MSG-004 | Render markdown in response | AT-MSG-007, AT-MSG-008 | F5 |
| MSG-005 | Copy response to clipboard | AT-MSG-009 | F5 |
| MSG-006 | Multi-turn conversation | AT-MSG-010, AT-MSG-011 | F8 |
| MSG-007 | Clear conversation context | AT-MSG-012 | F8 |
| MSG-008 | Handle empty input | AT-MSG-013 | F4 |

**Acceptance Tests - Message Handling:**

| Test ID | Test Description | Expected Result |
|---------|------------------|-----------------|
| AT-MSG-001 | Type message, click Send | Message sent, response received |
| AT-MSG-002 | Type message, press Ctrl+Enter | Message sent |
| AT-MSG-003 | Send query, observe response | Text streams word-by-word |
| AT-MSG-004 | Stream long response (>1000 tokens) | Auto-scroll follows stream |
| AT-MSG-005 | Click Stop during stream | Stream halts, partial response kept |
| AT-MSG-006 | Press Escape during stream | Stream halts |
| AT-MSG-007 | Response contains code block | Syntax highlighting applied |
| AT-MSG-008 | Response contains table | Table rendered correctly |
| AT-MSG-009 | Click copy button on response | Full response in clipboard |
| AT-MSG-010 | Send follow-up question | Context from previous Q&A used |
| AT-MSG-011 | Toggle context OFF, send follow-up | No previous context sent |
| AT-MSG-012 | Click Clear button | Conversation cleared, context reset |
| AT-MSG-013 | Click Send with empty input | Send button disabled |

---

### 1.4 Token Management

| Req ID | Requirement | Acceptance Tests | PRD Ref |
|--------|-------------|------------------|---------|
| TOK-001 | Estimate tokens before send | AT-TOK-001, AT-TOK-002 | F6 |
| TOK-002 | Display actual tokens after response | AT-TOK-003 | F6 |
| TOK-003 | Set max response tokens | AT-TOK-004, AT-TOK-005 | F6 |
| TOK-004 | Warn on high token count | AT-TOK-006 | F6 |
| TOK-005 | Track session token usage | AT-TOK-007 | F6 |

**Acceptance Tests - Token Management:**

| Test ID | Test Description | Expected Result |
|---------|------------------|-----------------|
| AT-TOK-001 | Type 100-word message | Estimate shows ~130-150 tokens |
| AT-TOK-002 | Type with context enabled | Estimate includes history tokens |
| AT-TOK-003 | Receive response | Actual in/out tokens displayed |
| AT-TOK-004 | Set max tokens to 500 | Response truncates at ~500 tokens |
| AT-TOK-005 | Set max tokens to 100, ask long question | Response ends with truncation indicator |
| AT-TOK-006 | Type message >4000 tokens | Warning: "High token usage" |
| AT-TOK-007 | Multiple exchanges in session | Running total displayed |

---

### 1.5 Context Isolation

| Req ID | Requirement | Acceptance Tests | PRD Ref |
|--------|-------------|------------------|---------|
| ISO-001 | Tabs maintain separate contexts | AT-ISO-001, AT-ISO-002 | F1 |
| ISO-002 | Provider change clears context | AT-ISO-003 | F2 |
| ISO-003 | No cross-tab data leakage | AT-ISO-004, AT-ISO-005 | F1 |
| ISO-004 | Concurrent requests from tabs | AT-ISO-006, AT-ISO-007 | F1 |
| ISO-005 | Tab crash isolation | AT-ISO-008 | F1 |

**Acceptance Tests - Context Isolation:**

| Test ID | Test Description | Expected Result |
|---------|------------------|-----------------|
| AT-ISO-001 | Discuss "cats" in Tab1, "dogs" in Tab2 | Each tab retains own topic |
| AT-ISO-002 | Ask "what did I ask?" in each tab | Each returns its own history |
| AT-ISO-003 | Change provider in Tab1 | Tab2 unaffected |
| AT-ISO-004 | Inspect memory during multi-tab | No shared message buffers |
| AT-ISO-005 | Close Tab1 with sensitive data | Data not in Tab2 memory |
| AT-ISO-006 | Send in Tab1 and Tab2 simultaneously | Both complete independently |
| AT-ISO-007 | Stream in Tab1, send in Tab2 | Both operate without interference |
| AT-ISO-008 | Cause error in Tab1 provider call | Tab2 continues working |

---

### 1.6 Settings & Configuration

| Req ID | Requirement | Acceptance Tests | PRD Ref |
|--------|-------------|------------------|---------|
| SET-001 | Theme selection (Light/Dark/System) | AT-SET-001 | NF |
| SET-002 | Default provider preference | AT-SET-002 | F2 |
| SET-003 | Default model per provider | AT-SET-003 | F7 |
| SET-004 | Keyboard shortcuts customization | AT-SET-004 | F13 |
| SET-005 | Export/Import settings | AT-SET-005, AT-SET-006 | NF |

**Acceptance Tests - Settings:**

| Test ID | Test Description | Expected Result |
|---------|------------------|-----------------|
| AT-SET-001 | Select Dark theme | UI switches to dark mode |
| AT-SET-002 | Set default provider to OpenAI | New tabs open with OpenAI |
| AT-SET-003 | Set default Claude model to Sonnet | New Claude tabs use Sonnet |
| AT-SET-004 | Remap send to Ctrl+Shift+Enter | New shortcut works |
| AT-SET-005 | Export settings | JSON file with prefs (no keys) |
| AT-SET-006 | Import settings on new install | Preferences restored |

---

## 2. Detailed Scenarios

### 2.1 Scenario: First-Time Setup

```
GIVEN user launches app for first time
WHEN welcome screen appears
THEN user sees provider selection list
AND user sees API key input field
AND "Continue" button is disabled

WHEN user selects "Claude"
AND user enters valid API key
AND user clicks "Test Connection"
THEN app validates key with provider
AND shows "Connection successful"
AND "Continue" button becomes enabled

WHEN user clicks "Continue"
THEN app saves encrypted key
AND opens main window with one tab
AND tab is configured for Claude
```

### 2.2 Scenario: Parallel Multi-Provider Query

```
GIVEN user has Tab1 (Claude) and Tab2 (ChatGPT) open
AND both providers are configured

WHEN user types "Explain REST APIs" in Tab1
AND user switches to Tab2
AND user types same query
AND user clicks Send on Tab2
AND user switches to Tab1
AND user clicks Send on Tab1
THEN both requests execute in parallel
AND Tab1 shows Claude response
AND Tab2 shows ChatGPT response
AND no response mixing occurs
```

### 2.3 Scenario: Error Recovery

```
GIVEN user is streaming response in Tab1
WHEN network disconnects mid-stream
THEN stream pauses
AND warning icon appears
AND partial response is preserved
AND "Retry" button appears

WHEN user clicks "Retry"
AND network is restored
THEN request resends
AND response continues (or restarts)
```

---

## 3. Data Specifications

### 3.1 Tab State Object

```json
{
  "tabId": "uuid-v4",
  "label": "string (max 30 chars)",
  "providerId": "claude|openai|custom",
  "modelId": "string",
  "contextEnabled": "boolean",
  "maxTokens": "integer (100-4096)",
  "messages": [
    {
      "role": "user|assistant",
      "content": "string",
      "timestamp": "ISO-8601",
      "tokenCount": "integer"
    }
  ],
  "createdAt": "ISO-8601",
  "lastActiveAt": "ISO-8601"
}
```

### 3.2 Provider Configuration

```json
{
  "providerId": "string",
  "displayName": "string",
  "apiEndpoint": "URL",
  "authType": "bearer|header",
  "authHeaderName": "string",
  "models": [
    {
      "modelId": "string",
      "displayName": "string",
      "maxContextTokens": "integer",
      "inputTokenCost": "decimal",
      "outputTokenCost": "decimal"
    }
  ],
  "streamingSupported": "boolean"
}
```

### 3.3 API Key Storage

```json
{
  "providerId": "string",
  "keyAlias": "string (user-defined)",
  "credentialId": "reference to Windows Credential Vault"
}
```

---

## 4. Error Codes & Handling

| Error Code | Description | User Message | Recovery |
|------------|-------------|--------------|----------|
| E001 | Invalid API key | "API key is invalid. Check settings." | Open settings |
| E002 | Rate limited | "Rate limit reached. Retry in {n}s." | Auto-retry countdown |
| E003 | Network timeout | "Connection timed out. Check network." | Retry button |
| E004 | Provider unavailable | "Service unavailable. Try later." | Retry button |
| E005 | Context too long | "Conversation too long. Clear context." | Clear context button |
| E006 | Invalid response | "Unexpected response. Please retry." | Retry button |
| E007 | Max tabs reached | "Maximum tabs open. Close one first." | None |

---

## 5. Keyboard Shortcuts

| Action | Default Shortcut | Configurable |
|--------|------------------|--------------|
| Send message | Ctrl+Enter | Yes |
| New tab | Ctrl+T | Yes |
| Close tab | Ctrl+W | Yes |
| Next tab | Ctrl+Tab | No |
| Previous tab | Ctrl+Shift+Tab | No |
| Stop stream | Escape | No |
| Clear context | Ctrl+Shift+C | Yes |
| Focus input | Ctrl+L | Yes |
| Copy last response | Ctrl+Shift+V | Yes |
| Open settings | Ctrl+, | No |
