# Multi-LLM Windows Store App - Master Plan

## Overview
A lightweight Windows Store application for interacting with multiple LLM providers (Claude, ChatGPT, etc.) through independent tabbed sessions with isolated contexts.

---

## Documentation Index

| Document | Description |
|----------|-------------|
| [01-PRD.md](docs/01-PRD.md) | Product Requirements & Customer Experience |
| [02-LOW-LEVEL-SPEC.md](docs/02-LOW-LEVEL-SPEC.md) | Detailed Requirements & Acceptance Tests |
| [03-TECHNICAL-SPEC.md](docs/03-TECHNICAL-SPEC.md) | Architecture, Data Flow & Class Diagrams |
| [04-TEST-PLAN.md](docs/04-TEST-PLAN.md) | Unit, Functional & E2E Test Plans |

---

## Key Design Principles

1. **Provider Agnostic**: Loosely coupled via `ILLMProvider` interface
2. **Context Isolation**: Each tab owns its session, no shared state
3. **Lightweight**: < 50MB memory, < 1s startup
4. **Token Efficient**: Haiku default, single-turn mode, cancel streaming

---

## Architecture Summary

```
┌─────────────────────────────────────────────────────────────┐
│                    Multi-Tab UI (WinUI 3)                   │
├─────────────────────────────────────────────────────────────┤
│  Tab1 [Claude]    Tab2 [OpenAI]    Tab3 [Claude]    [+]    │
├─────────────────────────────────────────────────────────────┤
│                       TabManager                            │
│         (Orchestrates isolated TabContext instances)        │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │ TabContext  │  │ TabContext  │  │ TabContext  │         │
│  │ ─Session    │  │ ─Session    │  │ ─Session    │         │
│  │ ─Provider   │  │ ─Provider   │  │ ─Provider   │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
├─────────────────────────────────────────────────────────────┤
│                    ProviderFactory                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                  │
│  │  Claude  │  │  OpenAI  │  │  Custom  │                  │
│  │ Provider │  │ Provider │  │ Provider │                  │
│  └──────────┘  └──────────┘  └──────────┘                  │
├─────────────────────────────────────────────────────────────┤
│  SecureVault (Credentials)  │  SQLite (State Persistence)  │
└─────────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Component | Technology |
|-----------|------------|
| Framework | WinUI 3 |
| Language | C# / .NET 8 |
| Storage | SQLite + Windows Credential Vault |
| HTTP | HttpClient with SSE streaming |

---

## MVP Features

- Multi-tab interface with isolated contexts
- Claude and OpenAI provider support
- Per-tab provider/model selection
- Streaming responses with cancel
- Token estimation and limits
- Secure API key storage
- Session persistence across restarts

---

## Implementation Phases

| Phase | Focus | Duration |
|-------|-------|----------|
| 1 | Core tab infrastructure, provider abstraction | Week 1-2 |
| 2 | Claude + OpenAI providers, streaming | Week 2-3 |
| 3 | UI polish, persistence, error handling | Week 3-4 |
| 4 | Testing, store submission | Week 4-5 |

---

## Success Criteria

- All E2E acceptance tests pass
- No S1/S2 defects at release
- App startup < 1 second
- Memory < 100MB with 10 tabs
- Store certification approved
