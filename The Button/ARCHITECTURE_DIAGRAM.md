# The Button - Architecture Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Unity Gaming Services                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ Authentication│  │    Lobby     │  │    Relay     │      │
│  │   (Anonymous) │  │   Service    │  │   Service    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
         ↓                  ↓                  ↓
┌─────────────────────────────────────────────────────────────┐
│                    Your Unity Client                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              Manager Layer (Singletons)              │   │
│  │  ┌────────────────┐  ┌────────────────┐             │   │
│  │  │ Authentication │  │ Connection     │             │   │
│  │  │ Manager        │  │ Manager        │             │   │
│  │  └────────────────┘  └────────────────┘             │   │
│  │         ↓                     ↓                      │   │
│  │  ┌────────────────┐  ┌────────────────┐             │   │
│  │  │ Lobby          │  │ Relay          │             │   │
│  │  │ Manager        │  │ Manager        │             │   │
│  │  └────────────────┘  └────────────────┘             │   │
│  │         ↓                     ↓                      │   │
│  │  ┌────────────────────────────────────────┐         │   │
│  │  │   NetworkManager (Unity Netcode)       │         │   │
│  │  │   + UnityTransport (with Relay)        │         │   │
│  │  └────────────────────────────────────────┘         │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                  UI Layer                             │   │
│  │  MainMenu → LobbyCreation → LobbyRoom → GameScene    │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              Game Layer (Networked)                   │   │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐     │   │
│  │  │  Player    │  │  Player    │  │  Player    │     │   │
│  │  │ Controller │  │  Network   │  │ Inventory  │     │   │
│  │  └────────────┘  └────────────┘  └────────────┘     │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Connection Flow

```
┌──────────┐
│  Player  │
│  Opens   │
│   Game   │
└────┬─────┘
     │
     ▼
┌─────────────────┐
│ Authentication  │ ← Signs in anonymously (automatic)
│    Manager      │
└────┬────────────┘
     │
     ▼
┌─────────────────┐
│   Main Menu     │ ← Shows lobby options
│      UI         │
└────┬────────────┘
     │
     ├─────────────┐
     │             │
     ▼             ▼
┌──────────┐  ┌──────────┐
│  Create  │  │   Join   │
│  Lobby   │  │  Lobby   │
└────┬─────┘  └────┬─────┘
     │             │
     ▼             ▼
┌─────────────────────────┐
│  Connection Manager     │ ← Orchestrates connection
└────┬────────────────────┘
     │
     ├─────────────┬─────────────┐
     ▼             ▼             ▼
┌──────────┐ ┌──────────┐ ┌──────────┐
│  Lobby   │ │  Relay   │ │ Network  │
│ Manager  │ │ Manager  │ │ Manager  │
└──────────┘ └──────────┘ └──────────┘
     │             │             │
     └─────────────┴─────────────┘
                   │
                   ▼
           ┌──────────────┐
           │  Lobby Room  │ ← Waiting for players
           │      UI      │
           └──────┬───────┘
                  │
                  ▼ (Host starts game)
           ┌──────────────┐
           │  Game Room   │ ← All players load scene
           │    Scene     │
           └──────┬───────┘
                  │
                  ▼
           ┌──────────────┐
           │   Players    │ ← Spawn and play
           │    Spawn     │
           └──────────────┘
```

## Data Flow - Stats Synchronization

```
        Server/Host                              Clients
┌──────────────────────┐              ┌──────────────────────┐
│  PlayerNetwork       │              │  PlayerNetwork       │
│  (Server Authority)  │              │  (Receives Updates)  │
│                      │              │                      │
│  Update() {          │              │  NetworkVariable     │
│    hunger -= decay   │──────────────▶│  OnValueChanged()   │
│    thirst -= decay   │   Automatic   │        │            │
│    stamina += regen  │  Replication  │        ▼            │
│  }                   │              │  PlayerStatsUI       │
│                      │              │  Updates Display     │
└──────────────────────┘              └──────────────────────┘

NetworkVariables:
- Health: Server → All Clients
- Hunger: Server → All Clients
- Thirst: Server → All Clients
- Stamina: Server → All Clients
```

## Scene Flow

```
┌───────────────┐
│  MainMenu     │  Not networked
│  Scene        │  Lobby UI only
└───────┬───────┘
        │
        │ Host starts game
        ▼
┌───────────────┐
│  GameRoom     │  Networked
│  Scene        │  Gameplay
└───────────────┘

NetworkManager persists across scenes (DontDestroyOnLoad)
```

## Lobby Lifecycle

```
┌─────────────┐
│   Created   │ ← Host creates lobby
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Active    │ ← Players can join
│ (Heartbeat) │   Host sends heartbeat every 15s
└──────┬──────┘   All clients poll every 2s
       │
       ├───────────┬───────────┐
       ▼           ▼           ▼
┌──────────┐ ┌──────────┐ ┌──────────┐
│   Game   │ │  Host    │ │  Timeout │
│  Started │ │  Leaves  │ │ (No HB)  │
└──────────┘ └──────────┘ └──────────┘
       │           │           │
       ▼           ▼           ▼
┌─────────────────────────────────┐
│         Lobby Closed            │
└─────────────────────────────────┘
```

## Network Authority

```
Server/Host Authority:
- Player stats (health, hunger, thirst, stamina)
- Inventory items
- Item spawning (future)
- Game state

Client Authority:
- Player input (movement, look)
- Camera control
- UI interactions

Replicated to Clients:
- All NetworkVariables
- NetworkObject transforms
- RPC calls
```

## Component Hierarchy - Player Prefab

```
Player (GameObject)
├─ CharacterController (Unity)
├─ NetworkObject (Netcode)
├─ PlayerController (Our script)
│  └─ Handles: Input, Movement, Camera
├─ PlayerNetwork (Our script)
│  └─ Handles: Stats, Sync, Name
├─ PlayerInventory (Our script)
│  └─ Handles: Items, Slots
│
├─ PlayerCamera (Child GameObject)
│  └─ Camera (Unity)
│     └─ Active only for local player
│
└─ PlayerNametag (Child GameObject)
   ├─ Canvas (World Space)
   └─ NameText (TextMeshPro)
      └─ Billboard toward camera
```

## UI Hierarchy - MainMenu Scene

```
MainMenuCanvas
├─ MainPanel
│  ├─ CreateLobbyButton
│  ├─ JoinByCodeButton
│  ├─ BrowseLobbyButton
│  └─ QuitButton
│
├─ CreateLobbyPanel
│  ├─ LobbyNameInput
│  ├─ MaxPlayersSlider
│  ├─ PrivateToggle
│  └─ CreateButton
│
├─ JoinByCodePanel
│  ├─ CodeInput (6 chars)
│  ├─ JoinButton
│  └─ ErrorText
│
├─ LobbyBrowserPanel
│  ├─ LobbyListContainer (Scroll)
│  │  └─ LobbyItem (Prefab) × N
│  ├─ RefreshButton
│  └─ StatusText
│
├─ LobbyRoomPanel
│  ├─ LobbyNameText
│  ├─ LobbyCodeText
│  ├─ CopyCodeButton
│  ├─ PlayerListContainer (Scroll)
│  │  └─ PlayerItem (Prefab) × N
│  ├─ StartGameButton (Host only)
│  └─ LeaveLobbyButton
│
└─ LoadingPanel
   └─ LoadingText
```

## File Dependencies

```
No Dependencies:
└─ AuthenticationManager.cs

Depends on Auth:
└─ LobbyManager.cs
   └─ RelayManager.cs
      └─ ConnectionManager.cs
         └─ NetworkManagerSetup.cs

UI depends on:
├─ MainMenuUI.cs (navigation)
├─ LobbyCreationUI.cs → ConnectionManager
├─ JoinByCodeUI.cs → ConnectionManager
├─ LobbyBrowserUI.cs → LobbyManager
├─ LobbyRoomUI.cs → LobbyManager + NetworkManagerSetup
├─ PlayerStatsUI.cs → PlayerNetwork
└─ InventoryUI.cs → PlayerInventory

Player scripts:
├─ PlayerController.cs → NetworkObject
├─ PlayerNetwork.cs → NetworkObject
└─ PlayerInventory.cs → NetworkObject
```

## Event Flow - Creating and Joining Lobby

```
CREATE LOBBY:
User clicks "Create Lobby"
    ↓
LobbyCreationUI.OnCreateButtonClick()
    ↓
ConnectionManager.CreateAndHostLobbyAsync()
    ↓
AuthenticationManager (check signed in)
    ↓
LobbyManager.CreateLobbyAsync()
    ├─ Unity Lobby Service creates lobby
    ├─ Generates lobby code (automatic)
    └─ Returns lobby object
    ↓
RelayManager.CreateRelayAsync()
    ├─ Unity Relay Service creates allocation
    └─ Returns relay join code
    ↓
LobbyManager.UpdateLobbyRelayCodeAsync()
    └─ Stores relay code in lobby data
    ↓
NetworkManagerSetup.StartHost()
    ├─ Configures UnityTransport with relay
    └─ Starts host (server + client)
    ↓
ConnectionManager → Connected state
    ↓
UI shows Lobby Room
    └─ Displays: lobby name, code, players


JOIN LOBBY:
User enters code and clicks "Join"
    ↓
JoinByCodeUI.OnJoinButtonClick()
    ↓
ConnectionManager.JoinLobbyByCodeAsync()
    ↓
AuthenticationManager (check signed in)
    ↓
LobbyManager.JoinLobbyByCodeAsync()
    ├─ Unity Lobby Service finds lobby
    ├─ Adds player to lobby
    └─ Returns lobby object
    ↓
Get relay code from lobby data
    ↓
RelayManager.JoinRelayAsync()
    └─ Configures UnityTransport with relay
    ↓
NetworkManagerSetup.StartClient()
    └─ Connects to host via relay
    ↓
ConnectionManager → Connected state
    ↓
UI shows Lobby Room
    └─ Displays: lobby name, code, players
```

## Network Message Types

```
Client → Server (ServerRpc):
- ModifyHealthServerRpc(amount)
- ModifyHungerServerRpc(amount)
- ModifyThirstServerRpc(amount)
- ModifyStaminaServerRpc(amount)
- AddItemServerRpc(itemId)
- RemoveItemServerRpc(slotIndex)
- UseItemServerRpc(slotIndex)

Server → Client (NetworkVariables):
- Health (float)
- Hunger (float)
- Thirst (float)
- Stamina (float)
- PlayerName (string)
- InventoryItems (NetworkList<int>)

Automatic (Unity Netcode):
- Player spawning
- Player despawning
- Scene changes
- Connection/disconnection
```

## Performance Characteristics

```
Network Traffic:
├─ Lobby Heartbeat: Every 15 seconds
├─ Lobby Polling: Every 2 seconds
├─ Stats Update: Every frame (server only)
├─ NetworkVariable Sync: On change only
└─ Position Sync: Not implemented yet

Memory Usage:
├─ Per Player: ~500KB (Unity overhead)
├─ Per Lobby: ~10KB
├─ NetworkVariables: ~100 bytes per player
└─ UI Elements: Instantiated as needed

Bandwidth:
├─ Lobby: < 1 KB/s
├─ Relay: < 5 KB/s per player
├─ Stats: < 0.5 KB/s per player
└─ Total: < 10 KB/s for 8 players
```

---

This architecture provides:
- ✅ Scalability (up to 8 players via Relay)
- ✅ Reliability (server-authoritative)
- ✅ Easy to extend (modular design)
- ✅ Simple testing (local + multiplayer play mode)
- ✅ Cross-platform (Unity + cloud services)

