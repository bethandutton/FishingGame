# GitHub CI/CD Setup Guide

This guide will help you set up automated builds when you push to GitHub.

## How It Works

- **Push to `main`** → Builds and uploads to TestFlight (beta testing)
- **Push a tag `v1.2.0`** → Builds and uploads to App Store (production)

## Step 1: Create GitHub Repository

```bash
cd /Users/bethanplayground/Desktop/FishingGame
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/FishCatcher.git
git push -u origin main
```

## Step 2: Create App Store Connect API Key

1. Go to [App Store Connect → Users and Access → Keys](https://appstoreconnect.apple.com/access/api)
2. Click **+** to create a new key
3. Name: `GitHub Actions`
4. Access: `App Manager`
5. Download the `.p8` file (you can only download once!)
6. Note the **Key ID** and **Issuer ID**

## Step 3: Export Your Signing Certificate

1. Open **Keychain Access** on your Mac
2. Find your Apple Distribution certificate
3. Right-click → **Export**
4. Save as `.p12` file with a password
5. Convert to base64:
   ```bash
   base64 -i Certificates.p12 | pbcopy
   ```

## Step 4: Export Provisioning Profile

1. Open Xcode → Settings → Accounts
2. Select your team → Manage Certificates
3. Or download from [Apple Developer Portal](https://developer.apple.com/account/resources/profiles/list)
4. Convert to base64:
   ```bash
   base64 -i profile.mobileprovision | pbcopy
   ```

## Step 5: Add GitHub Secrets

Go to your repo → **Settings → Secrets and variables → Actions → New repository secret**

Add these secrets:

| Secret Name | Value |
|-------------|-------|
| `BUILD_CERTIFICATE_BASE64` | Your .p12 certificate in base64 |
| `P12_PASSWORD` | Password for the .p12 file |
| `KEYCHAIN_PASSWORD` | Any password (e.g., `temp123`) |
| `PROVISIONING_PROFILE_BASE64` | Your .mobileprovision in base64 |
| `APP_STORE_CONNECT_API_KEY_ID` | Key ID from Step 2 |
| `APP_STORE_CONNECT_ISSUER_ID` | Issuer ID from Step 2 |
| `APP_STORE_CONNECT_API_KEY` | Contents of .p8 file |

## Step 6: Test It!

### For TestFlight (beta):
```bash
git add .
git commit -m "New feature"
git push origin main
```

### For App Store (production):
```bash
git tag v1.1.0
git push origin v1.1.0
```

## Monitoring Builds

- Go to your repo → **Actions** tab
- Watch the build progress
- If it fails, check the logs for errors

## Troubleshooting

### "Code signing error"
- Make sure certificate and provisioning profile match
- Check that secrets are correctly base64 encoded

### "Godot export failed"
- Ensure export presets are committed to the repo

### "Upload failed"
- Verify App Store Connect API key permissions
- Check that bundle ID matches App Store Connect
