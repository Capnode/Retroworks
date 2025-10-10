#!/bin/bash

# Retroworks RCBus DMG Builder Script
# This script creates a macOS DMG installer for the Retroworks.RCBus application

set -e

# Configuration
APP_NAME="Retroworks-RCBus"
APP_BUNDLE_NAME="Retroworks RCBus.app"
DMG_NAME="Retroworks-RCBus"
VERSION="0.1.0"
BUILD_DIR="$(cd "$(dirname "$0")" && pwd)/build"
DMG_DIR="$(cd "$(dirname "$0")" && pwd)/dmg"
ASSETS_DIR="$(cd "$(dirname "$0")" && pwd)/Assets"
PROJECT_DIR="$(cd "$(dirname "$0")/../Retroworks.RCBus" && pwd)"
OUTPUT_DIR="$(cd "$(dirname "$0")" && pwd)/dist"

# Detect macOS version and architecture
MACOS_VERSION=$(sw_vers -productVersion | cut -d '.' -f 1,2)
HOST_ARCH=$(uname -m)

# Allow override of target architecture via command line argument
if [ "$1" == "x64" ] || [ "$1" == "osx-x64" ]; then
    RID="osx-x64"
    TARGET_ARCH="x64"
elif [ "$1" == "arm64" ] || [ "$1" == "osx-arm64" ]; then
    RID="osx-arm64"
    TARGET_ARCH="arm64"
elif [ -z "$1" ]; then
    # Default to host architecture if no argument provided
    if [[ "$HOST_ARCH" == "arm64" ]]; then
        RID="osx-arm64"
        TARGET_ARCH="arm64"
    elif [[ "$HOST_ARCH" == "x86_64" ]]; then
        RID="osx-x64"
        TARGET_ARCH="x64"
    else
        echo "Unsupported host architecture: $HOST_ARCH"
        exit 1
    fi
else
    echo "Usage: $0 [x64|arm64|osx-x64|osx-arm64]"
    echo "  x64/osx-x64    - Build for Intel Macs"
    echo "  arm64/osx-arm64 - Build for Apple Silicon Macs"
    echo "  (no argument)  - Build for current architecture ($HOST_ARCH)"
    exit 1
fi

echo "Building for macOS $MACOS_VERSION on $HOST_ARCH (targeting $TARGET_ARCH -> $RID)"

# Clean previous builds (but preserve other architecture DMGs)
echo "Cleaning previous builds for $TARGET_ARCH..."
rm -rf "$BUILD_DIR"
rm -rf "$DMG_DIR"
# Only remove the DMG for the current target architecture
rm -f "$OUTPUT_DIR/${DMG_NAME}-${VERSION}-${RID}.dmg"
# Also clean any leftover temp DMGs anywhere in the project
find "$PROJECT_DIR" -name "*temp.dmg" -delete 2>/dev/null || true
find "$(dirname "$0")" -name "*temp.dmg" -delete 2>/dev/null || true

# Create directories
mkdir -p "$BUILD_DIR"
mkdir -p "$DMG_DIR"
mkdir -p "$OUTPUT_DIR"

# Build the .NET application
echo "Building .NET application..."
cd "$PROJECT_DIR"
dotnet publish -c Release -r "$RID" --self-contained true -p:PublishSingleFile=false -o "$BUILD_DIR/publish"

if [ ! -d "$BUILD_DIR/publish" ]; then
    echo "Error: Build failed - publish directory not found"
    exit 1
fi

# Create the app bundle structure
echo "Creating app bundle..."
APP_BUNDLE="$DMG_DIR/$APP_BUNDLE_NAME"
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copy the application files
cp -r "$BUILD_DIR/publish/"* "$APP_BUNDLE/Contents/MacOS/"

# Copy the icon (use existing retro.png or create icns if needed)
if [ -f "$PROJECT_DIR/Assets/retro.png" ]; then
    cp "$PROJECT_DIR/Assets/retro.png" "$APP_BUNDLE/Contents/Resources/icon.png"
fi

# Create Info.plist
cat > "$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleExecutable</key>
    <string>Retroworks.RCBus</string>
    <key>CFBundleIconFile</key>
    <string>icon.png</string>
    <key>CFBundleIdentifier</key>
    <string>com.capnode.retroworks.rcbus</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSHumanReadableCopyright</key>
    <string>© 2025 Capnode AB</string>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
    <key>LSApplicationCategoryType</key>
    <string>public.app-category.developer-tools</string>
    <key>NSAppTransportSecurity</key>
    <dict>
        <key>NSAllowsArbitraryLoads</key>
        <true/>
    </dict>
</dict>
</plist>
EOF

# Make the executable file executable
chmod +x "$APP_BUNDLE/Contents/MacOS/Retroworks.RCBus"

# Create Applications symlink
ln -sf /Applications "$DMG_DIR/Applications"

# Copy DMG background image if it exists
if [ -f "$ASSETS_DIR/dmg-background.png" ]; then
    cp "$ASSETS_DIR/dmg-background.png" "$DMG_DIR/.background.png"
fi

# Create DMG
echo "Creating DMG..."
DMG_TEMP="$BUILD_DIR/${DMG_NAME}-temp.dmg"
DMG_FINAL="$OUTPUT_DIR/${DMG_NAME}-${VERSION}-${RID}.dmg"

# Calculate size needed (with more padding for the app bundle)
APP_SIZE=$(du -sm "$APP_BUNDLE" | cut -f1)
SIZE=$((APP_SIZE + 100))  # Add 100MB padding for the app and background

echo "App bundle size: ${APP_SIZE}MB, DMG size: ${SIZE}MB"

# Create temporary DMG
hdiutil create -size ${SIZE}m -fs HFS+ -volname "$APP_NAME" "$DMG_TEMP"

# Mount the DMG
echo "Mounting DMG..."
MOUNT_OUTPUT=$(hdiutil attach "$DMG_TEMP" 2>&1)
MOUNT_DIR=$(echo "$MOUNT_OUTPUT" | grep "/Volumes" | sed 's/.*\(\/Volumes\/.*\)/\1/')

if [ -z "$MOUNT_DIR" ]; then
    echo "Error: Failed to mount DMG"
    echo "Mount output was: $MOUNT_OUTPUT"
    exit 1
fi

echo "DMG mounted at: $MOUNT_DIR"

# Copy contents to mounted DMG
echo "Copying files to DMG..."
# Copy the app bundle
cp -R "$APP_BUNDLE" "$MOUNT_DIR/"

# Copy the background image if it exists
if [ -f "$DMG_DIR/.background.png" ]; then
    cp "$DMG_DIR/.background.png" "$MOUNT_DIR/"
fi

# Create Applications symlink in the mounted DMG
ln -sf /Applications "$MOUNT_DIR/Applications"

# Set DMG window properties and icon positions
if command -v osascript &> /dev/null; then
    echo "Setting DMG appearance..."
    osascript << EOF
tell application "Finder"
    tell disk "$APP_NAME"
        open
        set current view of container window to icon view
        set toolbar visible of container window to false
        set statusbar visible of container window to false
        set the bounds of container window to {100, 100, 640, 480}
        set opts to the icon view options of container window
        set arrangement of opts to not arranged
        set icon size of opts to 128
        
        -- Position icons
        set position of item "$APP_BUNDLE_NAME" of container window to {150, 200}
        set position of item "Applications" of container window to {350, 200}
        
        -- Set background if available
        try
            set background picture of opts to file ".background.png"
        end try
        
        close
        open
        update without registering applications
        delay 2
    end tell
end tell
EOF
fi

# Unmount the DMG
echo "Finalizing DMG..."
hdiutil detach "$MOUNT_DIR"

# Convert to compressed read-only DMG
ACTUAL_DMG_TEMP="$BUILD_DIR/${DMG_NAME}-temp.dmg"

if [ -f "$ACTUAL_DMG_TEMP" ]; then
    echo "Converting temporary DMG to final DMG..."
    hdiutil convert "$ACTUAL_DMG_TEMP" -format UDZO -o "$DMG_FINAL"
    if [ $? -eq 0 ]; then
        # Clean up temporary DMG
        rm "$ACTUAL_DMG_TEMP"
    else
        echo "hdiutil convert failed"
        exit 1
    fi
else
    echo "Error: Temporary DMG not found at $ACTUAL_DMG_TEMP"
    echo "Contents of build directory:"
    ls -la "$BUILD_DIR/" || echo "Build directory doesn't exist"
    exit 1
fi

echo "DMG created successfully: $DMG_FINAL"

# Display file information
echo "File size: $(du -h "$DMG_FINAL" | cut -f1)"
echo "Architecture: $RID"
echo "Version: $VERSION"

# Optional: Code signing (uncomment if you have a developer certificate)
# echo "Code signing..."
# codesign --force --deep --sign "Developer ID Application: Your Name" "$DMG_FINAL"

echo "Build complete!"