<#
.SYNOPSIS
    Build script for MultiLLM App

.DESCRIPTION
    Restores dependencies, builds the solution, and optionally runs tests.

.PARAMETER Release
    Build in Release configuration (default: Debug)

.PARAMETER Test
    Run tests after build

.PARAMETER Clean
    Clean before build

.EXAMPLE
    .\build.ps1
    .\build.ps1 -Release
    .\build.ps1 -Release -Test
    .\build.ps1 -Clean -Release -Test
#>

param(
    [switch]$Release,
    [switch]$Test,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$Configuration = if ($Release) { "Release" } else { "Debug" }

Write-Host "========================================"
Write-Host "MultiLLM App Build Script"
Write-Host "========================================"
Write-Host "Configuration: $Configuration"
Write-Host "Root Directory: $RootDir"
Write-Host ""

# Check for .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "Using .NET SDK: $dotnetVersion"
} catch {
    Write-Host "ERROR: .NET SDK not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install .NET 8.0 SDK:"
    Write-Host "  winget install Microsoft.DotNet.SDK.8"
    Write-Host "  -or-"
    Write-Host "  https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
}

Write-Host ""
Set-Location $RootDir

# Clean if requested
if ($Clean) {
    Write-Host "[1/4] Cleaning previous build..."
    dotnet clean -c $Configuration --nologo -v q 2>$null
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue `
        .\bin, .\obj, `
        .\src\*\bin, .\src\*\obj, `
        .\tests\bin, .\tests\obj
    Write-Host "      Done."
} else {
    Write-Host "[1/4] Skipping clean (use -Clean to clean)"
}

# Restore dependencies
Write-Host ""
Write-Host "[2/4] Restoring dependencies..."
dotnet restore --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "      Done."

# Build
Write-Host ""
Write-Host "[3/4] Building solution ($Configuration)..."
dotnet build -c $Configuration --no-restore --nologo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "      Done."

# Run tests if requested
if ($Test) {
    Write-Host ""
    Write-Host "[4/4] Running tests..."
    dotnet test -c $Configuration --no-build --nologo --logger "console;verbosity=normal"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "      Done."
} else {
    Write-Host ""
    Write-Host "[4/4] Skipping tests (use -Test to run tests)"
}

Write-Host ""
Write-Host "========================================"
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "========================================"
Write-Host ""
Write-Host "Output location:"
Write-Host "  - Core:  src\MultiLLMApp.Core\bin\$Configuration\net8.0\"
Write-Host "  - Data:  src\MultiLLMApp.Data\bin\$Configuration\net8.0\"
Write-Host "  - Tests: tests\bin\$Configuration\net8.0\"
