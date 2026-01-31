# 🐟 Fish Catcher

A mobile-first fishing game inspired by the classic "Let's Go Fishin'" toy. Catch fish with a claw and drop them in the bucket!

## Gameplay

- **Touch and hold** anywhere to drop the claw
- **Drag left/right** while holding to move the claw
- **Release** to grab and raise the claw
- **Move the caught fish** over the bucket to score
- Catch **10 fish in 30 seconds** to win!

## Setup

### 1. Install Godot 4
Download from: https://godotengine.org/download (Standard version)

### 2. Open Project
- Launch Godot
- Click **Import** → navigate to this folder → select `project.godot`
- Click **Import & Edit**

### 3. Run
Press **F5** or click the Play button

## Project Structure

```
FishingGame/
├── project.godot           # Project config (mobile-oriented)
├── icon.svg
├── scenes/
│   └── main.tscn           # Main game scene
├── scripts/
│   ├── main.gd             # Game controller, timer, scoring
│   ├── claw.gd             # Player-controlled claw with touch input
│   ├── fish.gd             # Individual fish behavior
│   ├── fish_spawner.gd     # Spawns and manages fish
│   └── drop_zone.gd        # Bucket detection
└── assets/                 # Add sprites/sounds here
```

## Customization

In the Godot editor, select nodes to adjust:

**Claw (claw.gd):**
- `move_speed` - Horizontal movement speed
- `drop_speed` - How fast claw drops
- `raise_speed` - How fast claw raises

**FishSpawner (fish_spawner.gd):**
- `num_fish` - Starting number of fish
- Fish colors in the `fish_colors` array

**Main (main.gd):**
- `TARGET_FISH` - Fish needed to win
- `GAME_TIME` - Seconds per round

## Mobile Export

### Android
1. Install Android SDK & set paths in Editor → Editor Settings → Export → Android
2. Project → Export → Add Android → Configure signing
3. Export APK or AAB

### iOS (requires Mac)
1. Project → Export → Add iOS
2. Configure signing with Apple Developer account
3. Export and open in Xcode

## Future: Online Multiplayer

The game is structured to support future online VS mode:

### Planned Architecture
```
┌─────────────────────────────────────────┐
│           Game Server (WebSocket)        │
│  - Match making                          │
│  - Game state sync                       │
│  - Score tracking                        │
└─────────────────────────────────────────┘
         ▲                    ▲
         │                    │
    ┌────┴────┐          ┌────┴────┐
    │ Player 1 │          │ Player 2 │
    │ (Client) │          │ (Client) │
    └──────────┘          └──────────┘
```

### Implementation Steps
1. **Add NetworkManager singleton** - Handle WebSocket connections
2. **Create lobby system** - Match players together
3. **Sync fish positions** - Server authoritative fish spawning
4. **Sync scores** - Real-time score updates
5. **Add VS UI** - Split screen or side-by-side view

### Recommended Backend Options
- **Godot's built-in networking** - Good for simple P2P
- **Nakama** - Open source game server, great for mobile
- **PlayFab** - Microsoft's game backend service
- **Firebase** - Real-time database for simple sync

### Files to Add for Multiplayer
```
scripts/
├── network_manager.gd      # Connection handling
├── lobby.gd                # Matchmaking UI
├── multiplayer_main.gd     # VS mode game logic
└── player_sync.gd          # Position/state sync
```

## License

Free to use and modify.
