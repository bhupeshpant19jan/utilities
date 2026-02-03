# MultiLLM App - Source Code

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Windows 10/11 (for WinUI 3 app development)
- Visual Studio 2022 (optional, for IDE support)

## Project Structure

```
src/
├── MultiLLMApp.Core/          # Core library (interfaces, models, services)
│   ├── Interfaces/            # Contract definitions
│   ├── Models/                # Data models and DTOs
│   ├── Providers/             # LLM provider implementations
│   └── Services/              # Business logic services
│
├── MultiLLMApp.Data/          # Data layer (persistence, credentials)
│   ├── SecureVault.cs         # Secure credential storage
│   └── LocalDatabase.cs       # Local state persistence
│
└── MultiLLMApp/               # WinUI 3 App (UI layer - coming soon)
    ├── Views/
    └── ViewModels/
```

## Building Locally

### Option 1: Using the Build Script

```bash
# Linux/macOS
chmod +x ../scripts/build.sh
../scripts/build.sh

# Windows (PowerShell)
..\scripts\build.ps1

# Windows (Command Prompt)
..\scripts\build.bat
```

### Option 2: Manual Build

```bash
# Navigate to solution root
cd ..

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Build in Release mode
dotnet build -c Release
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test project
dotnet test tests/MultiLLMApp.Tests.csproj
```

## Development Workflow

### 1. Restore Dependencies
```bash
dotnet restore
```

### 2. Build in Debug Mode
```bash
dotnet build
```

### 3. Run Tests
```bash
dotnet test
```

### 4. Create Release Build
```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

## Adding a New LLM Provider

1. Create a new class in `MultiLLMApp.Core/Providers/`
2. Inherit from `BaseProvider`
3. Implement required methods:
   - `ValidateApiKeyAsync()`
   - `GetAvailableModels()`
   - `SendAsync()`
   - `StreamAsync()`
4. Register in `ProviderFactory`

Example:
```csharp
public sealed class MyProvider : BaseProvider
{
    public override string ProviderId => "myprovider";
    public override string DisplayName => "My Provider";

    // Implement abstract methods...
}
```

## Configuration

### API Keys

API keys are stored securely using the platform's credential manager:
- **Windows**: Windows Credential Vault
- **Development**: In-memory storage (SecureVault fallback)

### Environment Variables (Optional)

```bash
# For development/testing
export CLAUDE_API_KEY="your-claude-key"
export OPENAI_API_KEY="your-openai-key"
```

## Troubleshooting

### Build Errors

**"SDK not found"**
```bash
# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8
```

**"Project dependencies not resolved"**
```bash
dotnet restore --force
```

### Runtime Errors

**"Provider not configured"**
- Ensure API keys are stored via the Settings UI or `SecureVault`

**"Rate limit exceeded"**
- Wait for the retry countdown or use a different API key

## IDE Setup

### Visual Studio 2022
1. Open `MultiLLMApp.sln`
2. Build > Build Solution (Ctrl+Shift+B)
3. Debug > Start Debugging (F5)

### Visual Studio Code
1. Install C# Dev Kit extension
2. Open folder containing solution
3. Run: `dotnet build` in terminal

### JetBrains Rider
1. Open `MultiLLMApp.sln`
2. Build > Build Solution
3. Run/Debug selected project
