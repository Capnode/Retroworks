#!/bin/bash

# Test script to verify the DMG build system setup

echo "=== Retroworks RCBus macOS DMG Build System Test ==="
echo

# Check basic requirements
echo "Checking requirements..."

# Check .NET SDK
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✓ .NET SDK: $DOTNET_VERSION"
else
    echo "✗ .NET SDK not found"
    exit 1
fi

# Check if we're on macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    MACOS_VERSION=$(sw_vers -productVersion)
    echo "✓ macOS: $MACOS_VERSION"
else
    echo "✗ Not running on macOS"
    exit 1
fi

# Check architecture
ARCH=$(uname -m)
echo "✓ Architecture: $ARCH"

# Check if hdiutil is available (for DMG creation)
if command -v hdiutil &> /dev/null; then
    echo "✓ hdiutil available for DMG creation"
else
    echo "✗ hdiutil not found"
    exit 1
fi

# Check project structure
PROJECT_DIR="$(dirname "$0")/../Retroworks.RCBus"
if [ -f "$PROJECT_DIR/Retroworks.RCBus.csproj" ]; then
    echo "✓ Project file found"
else
    echo "✗ Project file not found at $PROJECT_DIR"
    exit 1
fi

# Check if build script exists and is executable
BUILD_SCRIPT="$(dirname "$0")/build-dmg.sh"
if [ -x "$BUILD_SCRIPT" ]; then
    echo "✓ Build script is executable"
else
    echo "✗ Build script not found or not executable"
    exit 1
fi

echo
echo "=== All checks passed! ==="
echo
echo "To build the DMG installer, run:"
echo "  ./build-dmg.sh"
echo
echo "Or use VS Code task:"
echo "  Cmd+Shift+P → Tasks: Run Task → build-dmg"