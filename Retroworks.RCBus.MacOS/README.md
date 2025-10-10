# Retroworks RCBus - macOS DMG Installer

This folder contains the macOS DMG installer build system for Retroworks RCBus.

## Contents

- `build-dmg.sh` - Main script to build the DMG installer
- `create-background.sh` - Script to create/update the DMG background image
- `Info.plist.template` - Template for the macOS app bundle Info.plist
- `Assets/` - Assets for DMG packaging including background image

## Requirements

- macOS 10.15 or later
- .NET 9.0 SDK
- Xcode Command Line Tools (`xcode-select --install`)

## Building the DMG

1. Ensure you have the .NET 9.0 SDK installed
2. Run the build script:
   ```bash
   ./build-dmg.sh
   ```

The script will:
1. Build the .NET application for the current macOS architecture
2. Create a proper macOS app bundle
3. Package everything into a DMG file with proper layout
4. Output the DMG to the `dist/` folder

## Output

The DMG will be created as:
- `dist/Retroworks-RCBus-{version}-{architecture}.dmg`

Where:
- `{version}` is the version from the project file (e.g., 0.1.0)
- `{architecture}` is either `osx-x64` or `osx-arm64`

## Customization

### Background Image
- Replace `Assets/dmg-background.png` with your custom background (recommended size: 640x380)
- Or modify `create-background.sh` to generate a custom background

### App Bundle Info
- Edit `Info.plist.template` to customize app bundle metadata
- The build script will automatically substitute version information

### Code Signing
Uncomment the code signing section in `build-dmg.sh` if you have a valid Apple Developer certificate.

## Notes

- The app bundle includes entitlements for serial port and USB device access
- The DMG includes an Applications folder shortcut for easy installation
- The script automatically detects the current architecture (Intel or Apple Silicon)

## Troubleshooting

### Build Fails
- Ensure .NET 9.0 SDK is installed: `dotnet --version`
- Check that all dependencies are available

### DMG Creation Fails
- Ensure you have sufficient disk space
- Try running with `sudo` if permission errors occur
- Verify Xcode Command Line Tools are installed