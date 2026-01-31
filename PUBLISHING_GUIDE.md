# Fish Catcher - App Store Publishing Guide

## Quick Start Checklist

- [ ] **Step 1**: Create developer accounts
- [ ] **Step 2**: Set up Godot export templates
- [ ] **Step 3**: Create app icons and screenshots
- [ ] **Step 4**: Host privacy policy online
- [ ] **Step 5**: Configure and build exports
- [ ] **Step 6**: Create app store listings
- [ ] **Step 7**: Upload and submit

---

## Step 1: Create Developer Accounts

### Apple Developer Program
1. Go to https://developer.apple.com/programs/
2. Click "Enroll"
3. Sign in with Apple ID (or create one)
4. Pay $99/year fee
5. Wait for approval (usually 24-48 hours)

### Google Play Developer
1. Go to https://play.google.com/console/
2. Sign in with Google account
3. Pay $25 one-time fee
4. Complete account details
5. Verify identity (may take a few days)

---

## Step 2: Set Up Godot Export Templates

### In Godot Editor:
1. Open your project
2. Go to **Editor → Manage Export Templates**
3. Click **Download and Install** for version 4.6
4. Wait for download to complete

### Android SDK Setup:
1. Install Android Studio from https://developer.android.com/studio
2. Open Android Studio → SDK Manager
3. Install:
   - Android SDK Platform 34
   - Android SDK Build-Tools
   - Android SDK Command-line Tools
4. In Godot: **Editor → Editor Settings → Export → Android**
5. Set paths:
   - Android SDK Path: `/Users/YOUR_NAME/Library/Android/sdk`
   - Debug Keystore: (will be auto-created)

### iOS Setup (Mac only):
1. Install Xcode from Mac App Store
2. Open Xcode → Preferences → Accounts
3. Add your Apple Developer account
4. Xcode → Preferences → Locations → Command Line Tools (select version)

---

## Step 3: Create App Icons & Screenshots

### Generate Icons
Use your `icon.svg` to create all required sizes:

**Option A: Online Tool**
- Go to https://appicon.co/
- Upload your icon
- Download all sizes

**Option B: Command Line (with ImageMagick)**
```bash
# Install ImageMagick first: brew install imagemagick

cd /Users/bethanplayground/Desktop/FishingGame/app_store

# From your SVG, create PNG icons
convert ../icon.svg -resize 1024x1024 icon_1024x1024.png
convert ../icon.svg -resize 512x512 icon_512x512.png
convert ../icon.svg -resize 192x192 icon_192x192.png
convert ../icon.svg -resize 144x144 icon_144x144.png
convert ../icon.svg -resize 96x96 icon_96x96.png
convert ../icon.svg -resize 72x72 icon_72x72.png
convert ../icon.svg -resize 48x48 icon_48x48.png
```

### Take Screenshots
1. Run your game in Godot
2. Use screenshot tool or `F12` if configured
3. Recommended: Use iPhone/Android simulator for authentic device frames

**Screenshot Requirements:**
| Store | Size | Quantity |
|-------|------|----------|
| iOS (6.7") | 1290 x 2796 | 3-10 |
| iOS (6.5") | 1284 x 2778 | 3-10 |
| iOS (5.5") | 1242 x 2208 | 3-10 |
| Google Play | 1080 x 1920+ | 2-8 |
| Feature Graphic | 1024 x 500 | 1 |

---

## Step 4: Host Privacy Policy

### Option A: GitHub Pages (Free)
1. Create a GitHub repository
2. Add `privacy_policy.html` (already created in `app_store/` folder)
3. Go to repo Settings → Pages → Enable
4. Your URL: `https://YOUR_USERNAME.github.io/REPO_NAME/privacy_policy.html`

### Option B: Free Hosting
- https://sites.google.com (Google Sites)
- https://www.netlify.com (Netlify - free tier)

---

## Step 5: Configure and Build

### Android Build

1. **Open Godot Project**

2. **Project → Export → Android**

3. **Configure Settings:**
   ```
   Package Name: com.yourcompany.fishcatcher
   Name: Fish Catcher
   Version Code: 1
   Version Name: 1.0.0
   Min SDK: 21
   Target SDK: 34
   ```

4. **Create Release Keystore:**
   ```bash
   keytool -genkey -v \
     -keystore ~/fish-catcher-release.keystore \
     -alias fishcatcher \
     -keyalg RSA \
     -keysize 2048 \
     -validity 10000
   ```
   **⚠️ SAVE THE PASSWORD! You need it for every update!**

5. **In Godot Export Settings:**
   - Keystore → Release: `~/fish-catcher-release.keystore`
   - Release User: `fishcatcher`
   - Release Password: (your password)

6. **Export:**
   - Click "Export Project"
   - Choose `.aab` format (required for Play Store)
   - Save to `builds/android/fish-catcher.aab`

### iOS Build

1. **Open Godot Project**

2. **Project → Export → iOS**

3. **Configure Settings:**
   ```
   Bundle Identifier: com.yourcompany.fishcatcher
   App Store Team ID: (from developer.apple.com)
   Version: 1.0.0
   Short Version: 1.0
   ```

4. **Export:**
   - Click "Export Project"
   - Save to `builds/ios/FishCatcher.xcodeproj`

5. **In Xcode:**
   - Open the exported `.xcodeproj`
   - Select your Team in Signing & Capabilities
   - Product → Archive
   - Distribute App → App Store Connect

---

## Step 6: Create App Store Listings

### Google Play Console

1. Go to https://play.google.com/console/
2. Click **Create app**
3. Fill in:
   - App name: `Fish Catcher`
   - Default language: English
   - App or game: Game
   - Free or paid: Free

4. **Complete App Content:**
   - Privacy policy URL
   - Ads declaration: No ads
   - Content rating: Complete questionnaire
   - Target audience: All ages
   - News app: No

5. **Store Listing:**
   - Short description (80 chars)
   - Full description
   - App icon (512x512)
   - Feature graphic (1024x500)
   - Screenshots (min 2)
   - Category: Games → Arcade

6. **Release:**
   - Production → Create new release
   - Upload `.aab` file
   - Add release notes
   - Review and roll out

### App Store Connect

1. Go to https://appstoreconnect.apple.com/
2. Click **+** → **New App**
3. Fill in:
   - Platform: iOS
   - Name: Fish Catcher
   - Primary Language: English
   - Bundle ID: (select from dropdown)
   - SKU: fishcatcher001

4. **App Information:**
   - Privacy Policy URL
   - Category: Games → Arcade
   - Content Rights: Yes, owns rights
   - Age Rating: Complete questionnaire

5. **Prepare for Submission:**
   - Screenshots for each device size
   - App Preview (optional video)
   - Promotional Text
   - Description
   - Keywords
   - Support URL
   - Marketing URL (optional)

6. **Build:**
   - Upload from Xcode (Product → Archive → Distribute)
   - Select the build in App Store Connect

7. **Submit for Review**

---

## Step 7: Post-Submission

### Review Times
- **Google Play**: 1-7 days (usually 1-3 for established accounts)
- **App Store**: 24-48 hours typically

### Common Rejection Reasons

**Google Play:**
- Missing privacy policy
- Incorrect content rating
- Broken functionality

**App Store:**
- Crashes on launch
- Incomplete metadata
- Guideline 4.2 (minimum functionality) - unlikely for games

### After Approval
1. 🎉 Celebrate!
2. Share your app links
3. Monitor reviews and ratings
4. Plan updates based on feedback

---

## Quick Reference: Your App Info

```
App Name:        Fish Catcher
Bundle ID:       com.yourcompany.fishcatcher
Version:         1.0.0
Category:        Games → Arcade
Content Rating:  Everyone / 4+
Price:           Free
```

---

## Need Help?

- Godot Export Docs: https://docs.godotengine.org/en/stable/tutorials/export/
- Google Play Help: https://support.google.com/googleplay/android-developer/
- App Store Help: https://developer.apple.com/help/app-store-connect/

