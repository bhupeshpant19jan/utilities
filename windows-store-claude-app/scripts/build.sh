#!/bin/bash
#
# Build script for MultiLLM App
# Usage: ./build.sh [--release] [--test] [--clean]
#

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
CONFIGURATION="Debug"
RUN_TESTS=false
CLEAN_BUILD=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --release|-r)
            CONFIGURATION="Release"
            shift
            ;;
        --test|-t)
            RUN_TESTS=true
            shift
            ;;
        --clean|-c)
            CLEAN_BUILD=true
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [options]"
            echo ""
            echo "Options:"
            echo "  --release, -r    Build in Release configuration"
            echo "  --test, -t       Run tests after build"
            echo "  --clean, -c      Clean before build"
            echo "  --help, -h       Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "========================================"
echo "MultiLLM App Build Script"
echo "========================================"
echo "Configuration: $CONFIGURATION"
echo "Root Directory: $ROOT_DIR"
echo ""

# Check for .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found!"
    echo ""
    echo "Please install .NET 8.0 SDK:"
    echo "  - Windows: winget install Microsoft.DotNet.SDK.8"
    echo "  - macOS:   brew install dotnet-sdk"
    echo "  - Linux:   https://docs.microsoft.com/dotnet/core/install/linux"
    exit 1
fi

echo "Using .NET SDK: $(dotnet --version)"
echo ""

cd "$ROOT_DIR"

# Clean if requested
if [ "$CLEAN_BUILD" = true ]; then
    echo "[1/4] Cleaning previous build..."
    dotnet clean -c "$CONFIGURATION" --nologo -v q || true
    rm -rf ./bin ./obj ./src/*/bin ./src/*/obj ./tests/bin ./tests/obj
    echo "      Done."
else
    echo "[1/4] Skipping clean (use --clean to clean)"
fi

# Restore dependencies
echo ""
echo "[2/4] Restoring dependencies..."
dotnet restore --nologo
echo "      Done."

# Build
echo ""
echo "[3/4] Building solution ($CONFIGURATION)..."
dotnet build -c "$CONFIGURATION" --no-restore --nologo
echo "      Done."

# Run tests if requested
if [ "$RUN_TESTS" = true ]; then
    echo ""
    echo "[4/4] Running tests..."
    dotnet test -c "$CONFIGURATION" --no-build --nologo --logger "console;verbosity=normal"
    echo "      Done."
else
    echo ""
    echo "[4/4] Skipping tests (use --test to run tests)"
fi

echo ""
echo "========================================"
echo "Build completed successfully!"
echo "========================================"
echo ""
echo "Output location:"
echo "  - Core:  src/MultiLLMApp.Core/bin/$CONFIGURATION/net8.0/"
echo "  - Data:  src/MultiLLMApp.Data/bin/$CONFIGURATION/net8.0/"
echo "  - Tests: tests/bin/$CONFIGURATION/net8.0/"
