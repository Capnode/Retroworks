#!/bin/bash

# Script to create a DMG background image using existing assets
# This creates a simple background with the RCBus logo

# Check if ImageMagick is available
if ! command -v convert &> /dev/null; then
    echo "ImageMagick not found. Installing via Homebrew..."
    if command -v brew &> /dev/null; then
        brew install imagemagick
    else
        echo "Please install ImageMagick manually or via package manager"
        echo "You can install Homebrew and then run: brew install imagemagick"
        exit 1
    fi
fi

ASSETS_DIR="$(dirname "$0")/Assets"
PROJECT_ASSETS="../Retroworks.RCBus/Assets"

# Create a simple DMG background (640x380 to match DMG window size)
convert -size 640x380 gradient:#f0f0f0-#e0e0e0 \
    -gravity center \
    -pointsize 24 \
    -fill "#333333" \
    -annotate +0-120 "Retroworks RCBus" \
    -pointsize 14 \
    -fill "#666666" \
    -annotate +0-90 "Drag the application to Applications folder to install" \
    "$ASSETS_DIR/dmg-background.png"

echo "DMG background created at $ASSETS_DIR/dmg-background.png"

# If the project has a logo, we could overlay it
if [ -f "$PROJECT_ASSETS/rcbus.png" ]; then
    echo "Overlaying project logo..."
    convert "$ASSETS_DIR/dmg-background.png" \
        "$PROJECT_ASSETS/rcbus.png" \
        -gravity center \
        -geometry +0+20 \
        -composite \
        "$ASSETS_DIR/dmg-background.png"
fi