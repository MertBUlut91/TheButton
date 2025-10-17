# Procedural Room Generation - Setup Guide

This guide explains how to set up the procedural room generation system in Unity Editor.

## Overview

The system generates a random room made of cubes with buttons on each game start. Each button can spawn different items. The room is networked and identical for all players.

## Scripts Created

### Core Scripts
1. **RoomConfiguration.cs** - ScriptableObject for room parameters
2. **RoomItemPool.cs** - ScriptableObject for item management
3. **ProceduralRoomGenerator.cs** - Main room generation logic

### Modified Scripts
1. **SpawnButton.cs** - Added `SetItemData()` method for procedural setup
2. **NetworkManagerSetup.cs** - Integrated room generation on game start
3. **GameManager.cs** - Added room regeneration on restart

## Unity Editor Setup

### Step 1: Create WallButton Prefab

The system needs a button prefab to place on walls. You have two options:

#### Option A: Use Button.fbx Model (Recommended)
1. Navigate to `Assets/Toon Suburban Pack/Button.fbx`
2. Drag it into the scene
3. Add these components:
   - **NetworkObject** (from Unity Netcode)
   - **SpawnButton** script (already exists)
   - **BoxCollider** (if not present, set as non-trigger)
4. Configure SpawnButton:
   - Leave `itemToSpawn` empty (set programmatically)
   - Leave `spawnPoint` empty (set programmatically)
   - Set `cooldownTime: 5`
   - Assign `buttonRenderer` (MeshRenderer on the model)
   - Set colors:
     - Normal Color: Green (0, 1, 0)
     - Cooldown Color: Red (1, 0, 0)
     - Pressed Color: Yellow (1, 1, 0)
5. Scale the button appropriately (e.g., 0.3, 0.3, 0.2)
6. Save as prefab: `Assets/Prefabs/Item Prefabs/WallButton.prefab`
7. **IMPORTANT**: Add this prefab to NetworkManager's NetworkPrefabs list

#### Option B: Use Simple Cube
1. Create a cube in the scene
2. Follow same steps as Option A
3. Save as WallButton.prefab

### Step 2: Create RoomItemPool Asset

This defines which items can spawn in the room.

1. In Project window, right-click in `Assets/Resources/`
2. Select `Create > The Button > Room Item Pool`
3. Name it `RoomItemPool`
4. Configure in Inspector:

   **Required Items** (must spawn in every room):
   - Currently empty - you can add items like keys, door openers here
   - To add required items, drag ItemData assets from `Assets/Resources/Items/`
   
   **Random Item Pool** (spawn randomly):
   - Add existing items:
     - Chair (Assets/Resources/Items/Chair.asset)
     - Lamp (Assets/Resources/Items/Lamp.asset)
     - Stair (Assets/Resources/Items/Stair.asset)
     - TV (Assets/Resources/Items/Tv.asset)
   - You can add more ItemData assets as needed

### Step 3: Create RoomConfiguration Asset

This defines room size and appearance.

1. In Project window, right-click in `Assets/Resources/`
2. Select `Create > The Button > Room Configuration`
3. Name it `DefaultRoomConfiguration`
4. Configure in Inspector:

   **Room Dimensions:**
   - Room Width: 15 (cubes)
   - Room Height: 10 (cubes)
   - Room Depth: 15 (cubes)
   - Cube Size: 1 (meters)
   
   **Structure Prefabs:**
   - Floor Prefab: (optional - uses procedural cubes if empty)
   - Ceiling Prefab: (optional - uses procedural cubes if empty)
   - Wall Cube Prefab: (optional - uses procedural cubes if empty)
   - **Button Prefab**: Assign WallButton.prefab from Step 1
   
   **Materials:**
   - Floor Material: (optional - assign a material)
   - Ceiling Material: (optional - assign a material)
   - Wall Material: (optional - assign a material)
   
   **Button Generation:**
   - Button Density: 0.3 (30% of wall cubes will have buttons)
   - Min Random Buttons: 10
   - Max Random Buttons: 30
   
   **Spawn Settings:**
   - Player Spawn Offset: (0, 1, 0) - spawns 1m above room center
   - Item Spawn Offset: 0.5 - items spawn 0.5m in front of button

### Step 4: Add ProceduralRoomGenerator to GameRoom Scene

You have two options:

#### Option A: Pre-place in Scene (Recommended)
1. Open `Assets/Scenes/GameRoom.unity`
2. Create empty GameObject named `ProceduralRoomGenerator`
3. Add components:
   - **NetworkObject**
   - **ProceduralRoomGenerator** script
4. Configure ProceduralRoomGenerator:
   - Room Config: Assign `DefaultRoomConfiguration` asset
   - Item Pool: Assign `RoomItemPool` asset
   - Show Debug Logs: ✓ (check for testing)
5. **IMPORTANT**: Make sure this GameObject is in the scene, NOT as a prefab
6. Save the scene

#### Option B: Dynamic Creation
- The system will automatically create the generator if not found
- Less control but works automatically

### Step 5: Verify Network Setup

1. Open `Assets/Scenes/GameRoom.unity`
2. Find `NetworkManager` GameObject
3. Check `NetworkPrefabsList`:
   - Verify `WallButton` prefab is in the list
   - If not, add it manually
4. Make sure `ItemSpawner` GameObject exists in the scene

### Step 6: Test the System

1. **Host a Game:**
   - Run the game
   - Create or join a lobby as host
   - Click "Start Game"
   - Room should generate with buttons on walls

2. **Expected Behavior:**
   - Floor and ceiling appear (solid surfaces)
   - 4 walls made of cubes
   - Buttons appear on some wall cubes
   - Players spawn in center of room
   - Pressing E on a button spawns an item

3. **Debug Console:**
   - Watch for `[RoomGenerator]` messages
   - Should see: "Starting room generation with seed: XXX"
   - Should see: "Room generation complete!"
   - Should see: "Spawned X players"

## How It Works

### Generation Flow
```
1. Host clicks "Start Game"
2. GameRoom scene loads
3. NetworkManagerSetup detects scene loaded
4. ProceduralRoomGenerator.GenerateRoom() is called
5. System generates:
   - Random seed (synced to all clients)
   - Floor tiles (no buttons)
   - Ceiling tiles (no buttons)
   - 4 walls made of cubes
   - Required buttons placed first
   - Random buttons fill remaining positions
6. Players spawn in room center
7. Game starts
```

### Network Synchronization
- **Seed**: NetworkVariable ensures all clients generate identical rooms
- **Buttons**: Each button is a NetworkObject, spawned by server
- **Deterministic**: Same seed = same room on all clients

### Game Restart
- When game restarts (GameManager.RestartGameServerRpc):
  - Old room is cleared
  - New room is generated with new seed
  - Players are repositioned to new room center

## Troubleshooting

### Room doesn't generate
**Problem:** No room appears when game starts  
**Solution:**
- Check console for errors
- Verify RoomConfiguration and RoomItemPool are assigned
- Make sure ProceduralRoomGenerator exists in scene or NetworkManager can spawn it

### Buttons don't work
**Problem:** Can't interact with buttons  
**Solution:**
- Verify SpawnButton script is on button prefab
- Check that BoxCollider is NOT trigger
- Make sure button has NetworkObject component
- Verify PlayerInteraction script is on Player prefab

### "Failed to spawn NetworkObject"
**Problem:** Console shows spawn errors  
**Solution:**
- Add WallButton prefab to NetworkManager's NetworkPrefabs list
- Make sure all referenced prefabs have NetworkObject component

### Items spawn but fall through floor
**Problem:** Items phase through floor  
**Solution:**
- Check that ItemData.itemPrefab has Rigidbody and Collider
- Verify floor colliders are present and not triggers

### All buttons spawn same item
**Problem:** Every button gives the same item  
**Solution:**
- Check RoomItemPool.randomItemPool has multiple items
- Make sure items are properly assigned in the list

### Room is too small/large
**Problem:** Room size is wrong  
**Solution:**
- Adjust RoomConfiguration:
  - roomWidth, roomHeight, roomDepth (number of cubes)
  - cubeSize (size of each cube in meters)

### Not enough buttons
**Problem:** Too few buttons spawn  
**Solution:**
- Increase RoomConfiguration.buttonDensity (0-1)
- Increase maxRandomButtons value

### Players spawn outside room
**Problem:** Players appear in wrong location  
**Solution:**
- Check RoomConfiguration.playerSpawnOffset
- Verify room center calculation is correct

## Advanced Customization

### Adding Required Items (Keys, Doors, etc.)

To add items that MUST spawn in every room:

1. Create ItemData for the item (e.g., "Key")
2. Open RoomItemPool asset
3. Add item to **Required Items** list
4. These items will always spawn before random items

Example workflow for exit key:
```
1. Create Key.asset (ItemData) in Resources/Items/
2. Add to RoomItemPool.requiredItems
3. Button with key will spawn in every room
4. Player must find and press this button
```

### Custom Button Visuals

To use different button models:

1. Import your 3D model
2. Create prefab with:
   - NetworkObject
   - SpawnButton script
   - Collider
3. Assign to RoomConfiguration.buttonPrefab
4. Update NetworkPrefabs list

### Multiple Room Configurations

To have different room types:

1. Create multiple RoomConfiguration assets:
   - SmallRoom.asset (10x8x10)
   - LargeRoom.asset (25x15x25)
   - VerticalRoom.asset (10x20x10)
2. Randomly select in ProceduralRoomGenerator
3. Or let host choose before game starts

### Custom Wall Patterns

Modify `ProceduralRoomGenerator.GenerateWalls()` to:
- Skip certain positions (windows, doors)
- Create non-rectangular rooms
- Add decorative elements
- Place buttons in specific patterns

## Files Reference

### Created Files
- `Assets/Scripts/Game/RoomConfiguration.cs`
- `Assets/Scripts/Game/RoomItemPool.cs`
- `Assets/Scripts/Game/ProceduralRoomGenerator.cs`
- `Assets/Resources/RoomItemPool.asset` (create in editor)
- `Assets/Resources/DefaultRoomConfiguration.asset` (create in editor)
- `Assets/Prefabs/Item Prefabs/WallButton.prefab` (create in editor)

### Modified Files
- `Assets/Scripts/Interactables/SpawnButton.cs`
- `Assets/Scripts/Network/NetworkManagerSetup.cs`
- `Assets/Scripts/Game/GameManager.cs`

## Next Steps

1. **Create assets in Unity Editor** (Steps 1-4 above)
2. **Test single player** to verify room generation
3. **Test multiplayer** to verify network sync
4. **Add required items** (key, door, etc.)
5. **Customize visuals** (materials, models)
6. **Balance gameplay** (room size, button density)

## Support

If you encounter issues:
1. Check console for `[RoomGenerator]` debug messages
2. Verify all assets are properly assigned
3. Ensure NetworkPrefabs list is complete
4. Test in single player first before multiplayer

