# Windows Store Claude Command App - Development Plan

## Overview
A lightweight Windows Store app for executing commands via Claude API and displaying responses with minimal resource and token consumption.

---

## Architecture

```
┌─────────────────────────────────────────┐
│         Windows Store App (UWP)         │
├─────────────────────────────────────────┤
│  UI Layer (XAML - Minimal)              │
│  ├── Command Input Box                  │
│  ├── Response Display Panel             │
│  └── Settings Flyout                    │
├─────────────────────────────────────────┤
│  Core Logic (C#)                        │
│  ├── API Client (HttpClient)            │
│  ├── Token Manager                      │
│  ├── Response Parser                    │
│  └── Local Cache                        │
├─────────────────────────────────────────┤
│  Data Layer                             │
│  ├── Secure API Key Storage             │
│  └── Command History (SQLite)           │
└─────────────────────────────────────────┘
```

---

## Tech Stack

| Component | Technology | Rationale |
|-----------|------------|-----------|
| Framework | WinUI 3 / UWP | Native Windows, lightweight |
| Language | C# | Fast, low memory |
| HTTP | HttpClient | Built-in, efficient |
| Storage | Windows.Storage APIs | Secure credential vault |
| Cache | SQLite | Lightweight local DB |

---

## Token Optimization Strategies

### 1. Request Optimization
- **System prompt caching**: Store reusable system prompts locally
- **Concise prompts**: Strip unnecessary whitespace/formatting
- **Max tokens limit**: Set explicit `max_tokens` per request
- **Streaming responses**: Use SSE for real-time display, cancel early if needed

### 2. Context Management
- **No conversation history by default**: Single-turn mode
- **Optional context toggle**: User enables multi-turn when needed
- **Context summarization**: Compress long conversations before sending

### 3. Model Selection
- **Default to Claude Haiku**: Fastest, cheapest for simple tasks
- **User override**: Allow Sonnet/Opus selection for complex tasks

---

## Features (MVP)

### Core
- [ ] Single command input with execute button
- [ ] Response display with markdown rendering
- [ ] Copy response to clipboard
- [ ] API key configuration (secure storage)

### Token-Saving Features
- [ ] Token counter display (estimate before send)
- [ ] Model selector (Haiku/Sonnet/Opus)
- [ ] Max response length slider
- [ ] Cancel streaming response

### Quality of Life
- [ ] Command history (last 20, local only)
- [ ] Dark/Light theme
- [ ] Keyboard shortcuts (Ctrl+Enter to send)

---

## Project Structure

```
ClaudeCommandApp/
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Services/
│   ├── ClaudeApiService.cs      # API communication
│   ├── TokenEstimator.cs        # Estimate tokens before send
│   └── SecureStorage.cs         # API key management
├── Models/
│   ├── CommandRequest.cs
│   └── CommandResponse.cs
├── Helpers/
│   └── MarkdownRenderer.cs
├── Assets/
│   └── Icons/
└── Package.appxmanifest
```

---

## API Integration

### Endpoint
```
POST https://api.anthropic.com/v1/messages
```

### Minimal Request Payload
```json
{
  "model": "claude-3-haiku-20240307",
  "max_tokens": 1024,
  "messages": [
    {"role": "user", "content": "user command here"}
  ]
}
```

### Headers
```
x-api-key: {stored_key}
anthropic-version: 2023-06-01
content-type: application/json
```

---

## Implementation Phases

### Phase 1: Foundation (Week 1)
- [ ] Create UWP/WinUI 3 project
- [ ] Implement basic UI (input + output)
- [ ] Add secure API key storage
- [ ] Basic API call (non-streaming)

### Phase 2: Core Features (Week 2)
- [ ] Streaming response support
- [ ] Model selector
- [ ] Token estimation display
- [ ] Max tokens configuration

### Phase 3: Polish (Week 3)
- [ ] Command history
- [ ] Markdown rendering
- [ ] Error handling & retry
- [ ] Theme support

### Phase 4: Store Prep (Week 4)
- [ ] Store assets (icons, screenshots)
- [ ] Privacy policy
- [ ] Store listing description
- [ ] Testing & certification

---

## Performance Targets

| Metric | Target |
|--------|--------|
| App startup | < 1 second |
| Memory usage | < 50 MB idle |
| API response start | < 500ms (streaming) |
| Package size | < 10 MB |

---

## Security Considerations

1. **API Key Storage**: Use `Windows.Security.Credentials.PasswordVault`
2. **No telemetry**: Zero data collection
3. **Local only**: No server-side storage of commands/responses
4. **HTTPS only**: All API calls over TLS

---

## Store Requirements Checklist

- [ ] Privacy policy URL
- [ ] Age rating questionnaire
- [ ] App icons (all sizes)
- [ ] Screenshots (min 2)
- [ ] Description (short + long)
- [ ] Category: Productivity > Developer Tools

---

## Dependencies

```xml
<PackageReference Include="CommunityToolkit.WinUI.UI.Controls" Version="7.x" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.x" />
```

---

## Cost Estimation (Per User)

| Usage | Model | Est. Monthly Cost |
|-------|-------|-------------------|
| Light (50 cmds/day) | Haiku | ~$0.50 |
| Medium (200 cmds/day) | Haiku | ~$2.00 |
| Heavy (200 cmds/day) | Sonnet | ~$15.00 |

*Based on avg 500 input + 1000 output tokens per command*

---

## Future Enhancements (Post-MVP)

- Command templates/snippets
- Export conversation to file
- Custom system prompts
- Keyboard-only mode
- Multi-window support
