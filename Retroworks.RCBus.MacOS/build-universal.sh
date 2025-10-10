#!/bin/bash

# Build DMGs for both architectures
# This script builds both ARM64 and x64 DMGs

echo "=== Building Universal DMG Package ==="
echo "This will create DMGs for both ARM64 and x64 architectures"
echo

cd "$(dirname "$0")"

echo "Building ARM64 DMG..."
./build-dmg.sh arm64

if [ $? -eq 0 ]; then
    echo "✅ ARM64 DMG build completed successfully"
    echo
else
    echo "❌ ARM64 DMG build failed"
    exit 1
fi

echo "Building x64 DMG..."
./build-dmg.sh x64

if [ $? -eq 0 ]; then
    echo "✅ x64 DMG build completed successfully"
    echo
else
    echo "❌ x64 DMG build failed"
    exit 1
fi

echo "=== Universal DMG Build Complete ==="
echo "Available DMG files:"
ls -la dist/*.dmg

echo
echo "Both architectures have been built successfully!"
echo "• ARM64: For Apple Silicon Macs (M1, M2, M3, M4)"
echo "• x64:   For Intel Macs (and Apple Silicon via Rosetta)"