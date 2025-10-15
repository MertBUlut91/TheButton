# The Button - Multiplayer Game

A multiplayer survival game built with Unity Netcode for GameObjects and Unity Gaming Services.

## Game Concept

Players spawn in a room and must survive by managing their health, hunger, thirst, and stamina. The room contains buttons that spawn items to help players escape or survive. Items spawn at designated points and players have limited inventory (4-5 slots).

## Current Features

### ✅ Implemented (Phase 1 & 2)
- **Multiplayer Lobby System**
  - Unity Gaming Services (Lobby + Relay) integration
  - Create public/private lobbies
  - Join lobbies via 6-character code
  - Browse and join public lobbies
  - Real-time player list in lobby
  - Automatic lobby heartbeat and cleanup

- **Network Infrastructure**
  - Unity Netcode for GameObjects
  - Unity Transport with Relay (NAT traversal)
  - Anonymous authentication (no login required)
  - Host/Client architecture
  - Scene synchronization

- **Player System**
  - Networked player spawning
  - First-person movement (WASD + mouse look)
  - Character controller with jump
  - Synchronized player stats:
    - Health (100)
    - Hunger (100, decays over time)
    - Thirst (100, decays over time)
    - Stamina (100, regenerates)
  - Player nametags (world space)
  - Player stats UI display

### 🚧 To Be Implemented
- **Room & Button System**
  - Interactive buttons on walls
  - Item spawning system
  - Spawn point configuration
  - Door/exit mechanics

- **Inventory System**
  - 4-5 slot inventory
  - Item pickup/drop
  - Item usage
  - UI display

- **Items**
  - Keys (door unlocking)
  - Medkits (restore health)
  - Food (restore hunger)
  - Water (restore thirst)
  - Hazardous items

- **Steam Integration** (Future)
  - Steam lobbies
  - Friend invites
  - Steam authentication

## Quick Start

1. **Open Project in Unity 2022.3+ LTS**

2. **Follow Setup Guide**
   - See `MULTIPLAYER_SETUP_GUIDE.md` for detailed instructions
   - Link to Unity Gaming Services
   - Enable Lobby and Relay services
   - Install `com.unity.services.lobbies` package
   - Create MainMenu and GameRoom scenes
   - Set up UI and prefabs

3. **Test Locally**
   - Use Unity's Multiplayer Play Mode (Unity 2023+)
   - Or build and run multiple instances

4. **Build for Release**
   - Configure Build Settings
   - Add both scenes
   - Build for your platform

## Project Structure

```
Assets/
├── Scripts/
│   ├── Network/         # Networking, lobby, relay management
│   ├── Player/          # Player controller and stats
│   └── UI/              # All UI controllers
├── Prefabs/             # Player, UI items
└── Scenes/
    ├── MainMenu.unity   # Lobby UI
    └── GameRoom.unity   # Gameplay scene
```

## Technologies

- **Unity 2022.3+ LTS** (URP)
- **Unity Netcode for GameObjects 2.5.1** - Server authoritative networking
- **Unity Gaming Services** - Lobby and Relay
- **Unity Transport 2.6.0** - Network transport layer
- **TextMeshPro** - UI text rendering
- **New Input System 1.14.2** - Player input

## Scripts Overview

### Network Scripts
- `AuthenticationManager.cs` - Unity Authentication (anonymous)
- `LobbyManager.cs` - Lobby creation, joining, management
- `RelayManager.cs` - Relay allocation and join codes
- `NetworkManagerSetup.cs` - Network manager configuration
- `ConnectionManager.cs` - High-level connection flow

### Player Scripts
- `PlayerController.cs` - First-person movement and camera
- `PlayerNetwork.cs` - Network synchronization and stats

### UI Scripts
- `MainMenuUI.cs` - Main menu navigation
- `LobbyCreationUI.cs` - Lobby creation interface
- `JoinByCodeUI.cs` - Join by code interface
- `LobbyBrowserUI.cs` - Browse public lobbies
- `LobbyRoomUI.cs` - Lobby room with player list
- `PlayerStatsUI.cs` - In-game stats display

## Controls

- **WASD** - Move
- **Mouse** - Look around
- **Space** - Jump
- **Escape** - Toggle cursor lock

## Networking Details

- **Architecture**: Client-Server (Host acts as server)
- **Transport**: Unity Transport over Unity Relay
- **NAT Traversal**: Automatic via Unity Relay
- **Max Players**: 8 per lobby (configurable)
- **Lobby Codes**: 6-character alphanumeric
- **Authentication**: Anonymous (no account required)

## Known Limitations

- No host migration (if host leaves, lobby closes)
- No reconnection system
- Stats decay is server-side only
- Limited to 8 players per lobby (Relay limitation)
- Requires internet connection (uses cloud services)

## Future Improvements

- Host migration
- Reconnection support
- Steam integration
- Dedicated server support
- Custom authentication
- More game mechanics (buttons, items, inventory)
- Persistence/save system
- Audio system
- Visual effects

## License

[Add your license here]

## Credits

Created by [Your Name]
Unity Version: 2022.3+ LTS

