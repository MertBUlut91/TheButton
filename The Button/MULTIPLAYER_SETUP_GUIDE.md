# The Button - Multiplayer Setup Guide

This guide will help you complete the Unity Editor setup for the multiplayer lobby system.

## Prerequisites

All necessary packages are already installed:
- Unity Netcode for GameObjects (2.5.1)
- Unity Services Multiplayer (1.1.8)
- Unity Transport (2.6.0)
- Unity Input System (1.14.2)

## Part 1: Unity Services Setup

### 1. Link Project to Unity Gaming Services
1. Open Unity Editor
2. Go to **Edit > Project Settings > Services**
3. Click **Create Unity Project ID** or link to existing project
4. Note: You'll need a Unity account

### 2. Enable Required Services
1. Go to **Window > General > Services** (or Unity Dashboard)
2. Enable **Lobby** service
3. Enable **Relay** service

## Part 2: Add Required Package

You need to add the Lobby package:
1. Go to **Window > Package Manager**
2. Click **+** button > **Add package by name**
3. Enter: `com.unity.services.lobbies`
4. Click **Add**

## Part 3: Scene Setup

### Create MainMenu Scene

1. **Create new scene**: `File > New Scene > Empty`
2. **Save as**: `Assets/Scenes/MainMenu.unity`

#### Network Setup GameObject
1. Create empty GameObject: `NetworkSetup`
2. Add components:
   - `NetworkManager` (from Unity Netcode)
   - `UnityTransport` (from Unity Transport)
   - `AuthenticationManager` (our script)
   - `LobbyManager` (our script)
   - `RelayManager` (our script)
   - `NetworkManagerSetup` (our script)
   - `ConnectionManager` (our script)

#### Configure NetworkManager
1. Select `NetworkSetup` GameObject
2. In **NetworkManager** component:
   - Set **Transport** to the UnityTransport component
   - Under **NetworkConfig**:
     - Enable **Enable Scene Management**

#### MainMenu Canvas Setup
1. Create UI Canvas: `GameObject > UI > Canvas`
2. Rename to `MainMenuCanvas`
3. Add **MainMenuUI** script component
4. Set Canvas to **Scale with Screen Size**

#### Create UI Panels

**Main Panel:**
```
MainMenuCanvas
└── MainPanel (Panel)
    ├── Title (TextMeshPro)
    ├── CreateLobbyButton (Button)
    ├── JoinByCodeButton (Button)
    ├── BrowseLobbyButton (Button)
    └── QuitButton (Button)
```

**Create Lobby Panel:**
```
MainMenuCanvas
└── CreateLobbyPanel (Panel)
    ├── LobbyNameInput (TMP InputField)
    ├── MaxPlayersSlider (Slider)
    ├── MaxPlayersText (TextMeshPro)
    ├── PrivateToggle (Toggle)
    ├── CreateButton (Button)
    └── BackButton (Button)
    [Add LobbyCreationUI script component]
```

**Join by Code Panel:**
```
MainMenuCanvas
└── JoinByCodePanel (Panel)
    ├── CodeInput (TMP InputField - 6 chars max)
    ├── ErrorText (TextMeshPro - hidden by default)
    ├── JoinButton (Button)
    └── BackButton (Button)
    [Add JoinByCodeUI script component]
```

**Lobby Browser Panel:**
```
MainMenuCanvas
└── LobbyBrowserPanel (Panel)
    ├── LobbyListContainer (Scroll View > Viewport > Content)
    ├── RefreshButton (Button)
    ├── BackButton (Button)
    └── StatusText (TextMeshPro)
    [Add LobbyBrowserUI script component]
```

**Lobby Room Panel:**
```
MainMenuCanvas
└── LobbyRoomPanel (Panel)
    ├── LobbyNameText (TextMeshPro)
    ├── LobbyCodeText (TextMeshPro)
    ├── CopyCodeButton (Button)
    ├── PlayerListContainer (Scroll View > Viewport > Content)
    ├── StartGameButton (Button)
    └── LeaveLobbyButton (Button)
    [Add LobbyRoomUI script component]
```

**Loading Panel:**
```
MainMenuCanvas
└── LoadingPanel (Panel)
    └── LoadingText (TextMeshPro)
```

#### Create Lobby Item Prefab
1. Create empty GameObject in Hierarchy: `LobbyItem`
2. Add UI elements:
   ```
   LobbyItem (with Horizontal Layout Group)
   ├── LobbyName (TextMeshPro)
   ├── PlayersCount (TextMeshPro)
   └── JoinButton (Button)
   ```
3. Drag to `Assets/Prefabs/` to create prefab
4. Delete from Hierarchy

#### Create Player Item Prefab
1. Create empty GameObject: `PlayerItem`
2. Add:
   ```
   PlayerItem
   └── PlayerNameText (TextMeshPro)
   ```
3. Drag to `Assets/Prefabs/` to create prefab
4. Delete from Hierarchy

#### Connect References in MainMenuUI
Select `MainMenuCanvas`, in **MainMenuUI** script:
- Drag all panel GameObjects to their respective fields
- Connect all buttons
- Connect loading text

#### Connect References in Sub-Panels
For each panel (LobbyCreationUI, JoinByCodeUI, LobbyBrowserUI, LobbyRoomUI):
- Connect all UI elements to script fields
- Set `MainMenuUI` reference to the MainMenuCanvas
- For LobbyBrowserUI: Set `lobbyItemPrefab` to the LobbyItem prefab
- For LobbyRoomUI: Set `playerItemPrefab` to the PlayerItem prefab

### Create GameRoom Scene

1. **Create new scene**: `File > New Scene > Basic (URP)`
2. **Save as**: `Assets/Scenes/GameRoom.unity`

#### Network Setup
1. Don't add NetworkManager here (it persists from MainMenu)

#### Create Player Prefab
1. Create Capsule: `GameObject > 3D Object > Capsule`
2. Rename to `Player`
3. Add **Character Controller** component
4. Add **Network Object** component (from Netcode)
5. Add **Player Controller** script
6. Add **Player Network** script

#### Player Camera Setup
1. Create child object: `PlayerCamera`
2. Add **Camera** component
3. Position: (0, 0.6, 0)
4. Tag: MainCamera
5. In PlayerController script, set `cameraTransform` to PlayerCamera

#### Player Nametag Setup
1. Create child object: `PlayerNametag`
2. Position: (0, 1.2, 0)
3. Add Canvas component (World Space)
   - Width: 200, Height: 50
   - Scale: 0.01, 0.01, 0.01
4. Add TextMeshPro as child: `NameText`
   - Center alignment
   - Font size: 24
5. In PlayerNetwork script:
   - Set `playerNameText` to NameText
   - Set `nameTagTransform` to PlayerNametag

#### Finalize Player Prefab
1. Drag Player to `Assets/Prefabs/` to create prefab
2. Delete from Hierarchy

#### Configure NetworkManager for Player Prefab
1. Open MainMenu scene
2. Select `NetworkSetup` GameObject
3. In **NetworkManager** component:
   - Under **NetworkPrefabs**, add the Player prefab

#### Create Player Spawn Points
In GameRoom scene:
1. Create empty GameObjects at different positions: `SpawnPoint1`, `SpawnPoint2`, etc.
2. Tag them as `Respawn` (or create custom tag `SpawnPoint`)

#### Game Room UI Canvas
1. Create UI Canvas: `GameObject > UI > Canvas`
2. Rename to `GameRoomCanvas`
3. Add **PlayerStatsUI** script

#### Create Stats UI
```
GameRoomCanvas
└── StatsPanel (Panel - bottom left)
    ├── HealthBar (Slider)
    ├── HealthText (TextMeshPro)
    ├── HungerBar (Slider)
    ├── HungerText (TextMeshPro)
    ├── ThirstBar (Slider)
    ├── ThirstText (TextMeshPro)
    ├── StaminaBar (Slider)
    └── StaminaText (TextMeshPro)
```

#### Connect PlayerStatsUI References
Select `GameRoomCanvas`, connect all sliders and texts to the **PlayerStatsUI** script.

### Configure Build Settings

1. Go to **File > Build Settings**
2. Add scenes in order:
   - **0**: MainMenu
   - **1**: GameRoom
3. Click **Add Open Scenes** when each is open

## Part 4: Testing

### Single Editor Test (Host Only)
1. Open MainMenu scene
2. Click Play
3. Wait for "Signed in anonymously" in Console
4. Create a lobby
5. Note: You won't see other players (need two instances)

### Multiple Editor Test (requires Unity 2023+)
1. Go to **Window > Multiplayer Play Mode**
2. Enable "Virtual Players" (2-4 players)
3. Click Play
4. Test lobby creation and joining

### Build and Test
1. Build the game: **File > Build and Run**
2. Run one instance as Host (Create Lobby)
3. Run another as Client (Join by Code)
4. Share the 6-digit code between instances

## Part 5: Advanced Configuration

### Unity Transport Settings
In NetworkSetup > UnityTransport:
- Protocol Type: **Unity Transport**
- Use Relay: Will be set programmatically
- Max Packet Size: 9000 (default)

### Network Manager Settings
- Player Prefab: Set to Player prefab
- Enable Scene Management: ✓
- Connection Approval: ✗ (for now)

## Troubleshooting

### "Relay not initialized"
- Make sure you've enabled Relay service in Unity Dashboard
- Check that AuthenticationManager signed in (console log)

### "Lobby not found"
- Lobbies expire after 30 seconds without heartbeat
- Make sure host is still connected
- Code is case-sensitive (should be uppercase)

### Players not spawning
- Ensure Player prefab is in NetworkManager's prefab list
- Check that NetworkObject component is on Player prefab
- Verify GameRoom scene is in Build Settings

### Camera issues
- Only local player's camera should be enabled
- PlayerController disables cameras for non-owned players
- Check camera is child of Player prefab

## Next Steps

After completing this setup:
1. Test lobby creation and joining
2. Test player spawning and movement
3. Verify player stats synchronization
4. Add game-specific mechanics (buttons, items, etc.)

## Notes

- All lobbies are automatically cleaned up on application quit
- Host migration is not implemented yet
- Player data is synchronized via NetworkVariables
- Stats decay happens only on server/host

## File Structure Created

```
Assets/
├── Scripts/
│   ├── Network/
│   │   ├── AuthenticationManager.cs
│   │   ├── ConnectionManager.cs
│   │   ├── LobbyManager.cs
│   │   ├── NetworkManagerSetup.cs
│   │   └── RelayManager.cs
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   └── PlayerNetwork.cs
│   └── UI/
│       ├── JoinByCodeUI.cs
│       ├── LobbyBrowserUI.cs
│       ├── LobbyCreationUI.cs
│       ├── LobbyRoomUI.cs
│       ├── MainMenuUI.cs
│       └── PlayerStatsUI.cs
├── Prefabs/
│   ├── Player.prefab (to be created)
│   ├── LobbyItem.prefab (to be created)
│   └── PlayerItem.prefab (to be created)
└── Scenes/
    ├── MainMenu.unity (to be created)
    └── GameRoom.unity (to be created)
```

