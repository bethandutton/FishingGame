#!/bin/bash

# Fish Catcher - Automated Build & Deploy Script
# Usage: ./deploy.sh [beta|release|build]

set -e

GODOT_PATH="/Applications/Godot.app/Contents/MacOS/Godot"
PROJECT_PATH="$(dirname "$0")"

echo "🐟 Fish Catcher Deployment Script"
echo "=================================="

# Check if Godot exists
if [ ! -f "$GODOT_PATH" ]; then
    echo "❌ Godot not found at $GODOT_PATH"
    echo "   Please update GODOT_PATH in this script"
    exit 1
fi

# Step 1: Export from Godot
echo ""
echo "📦 Step 1: Exporting from Godot..."
cd "$PROJECT_PATH"

# Export iOS build (headless)
"$GODOT_PATH" --headless --export-release "iOS" "builds/ios/FishCatcher.xcodeproj"

if [ $? -eq 0 ]; then
    echo "✅ Godot export complete"
else
    echo "❌ Godot export failed"
    exit 1
fi

# Step 2: Run Fastlane
echo ""
echo "🚀 Step 2: Running Fastlane..."

cd "$PROJECT_PATH"

case "$1" in
    "release")
        echo "   Mode: App Store Release"
        bundle exec fastlane ios release
        ;;
    "beta")
        echo "   Mode: TestFlight Beta"
        bundle exec fastlane ios beta
        ;;
    "build")
        echo "   Mode: Build Only"
        bundle exec fastlane ios build
        ;;
    "test")
        echo "   Mode: Run Tests"
        bundle exec fastlane ios test
        ;;
    *)
        echo "   Mode: Build Only (default)"
        bundle exec fastlane ios build
        ;;
esac

echo ""
echo "✅ Deployment complete!"
