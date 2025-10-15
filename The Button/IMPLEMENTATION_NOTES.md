# Implementation Notes

## Recent Changes & Fixes

### Lobby Code System (Fixed)
- **Changed from**: Custom 6-character code generation
- **Changed to**: Unity's built-in lobby code system
- **Reason**: Unity Lobbies automatically generates lobby codes, more reliable
- **Impact**: Simpler code, better reliability, one less thing to manage

The `LobbyManager.cs` now uses:
- `CurrentLobby.LobbyCode` - to get the lobby code
- `LobbyService.Instance.JoinLobbyByCodeAsync(code)` - to join by code

No more custom code generation or data field queries needed!

## Architecture Decisions

### Why Anonymous Authentication?
- Quick start for testing
- No account management needed initially
- Can add proper auth later (Steam, Epic, custom)
- Unity provides player IDs for identification

### Why Unity Relay?
- NAT traversal without port forwarding
- Works through most firewalls
- Free tier: 8 players, sufficient for this game
- Simple integration with Netcode

### Why Server-Authoritative?
- Prevents cheating (stats on server)
- Reliable state synchronization
- Host acts as server (no dedicated server needed)
- Standard for small multiplayer games

### Why NetworkVariables for Stats?
- Automatic synchronization
- Event-based updates (OnValueChanged)
- Efficient bandwidth usage
- Built into Netcode

### Why Singleton Pattern?
- Easy global access (Manager.Instance)
- Persists across scenes (DontDestroyOnLoad)
- Single source of truth
- Common Unity pattern

## Code Organization

### Namespaces
- `TheButton.Network` - All networking code
- `TheButton.Player` - Player-related code
- `TheButton.UI` - UI controllers

### Naming Conventions
- Scripts: PascalCase (PlayerController)
- Variables: camelCase (maxPlayers)
- Constants: UPPER_SNAKE_CASE (KEY_RELAY_CODE)
- Private fields: camelCase with underscore (not used, kept default)

### Component Organization
Each player has:
- `NetworkObject` - Network identity
- `CharacterController` - Unity physics
- `PlayerController` - Movement logic
- `PlayerNetwork` - Network sync and stats
- `PlayerInventory` - Inventory management

## Performance Considerations

### Network Bandwidth
- NetworkVariables: Only sync when changed
- Stats updates: Server-only, synced via NetworkVariables
- Position sync: Handled by NetworkTransform (not added yet, can add if needed)
- Lobby polling: 2 seconds (adjustable)

### Memory Usage
- Lobbies: Cleaned up automatically
- NetworkObjects: Despawned when players leave
- UI: Reuses list items (Instantiate/Destroy as needed)

### Optimization Opportunities (Future)
- Add NetworkTransform for position sync
- Throttle stats updates (not every frame)
- Object pooling for UI elements
- Compression for relay data

## Testing Strategy

### Unit Testing
- Not implemented (can add Unity Test Framework)
- Would test: Lobby creation, stat calculations, inventory logic

### Integration Testing
- Manual testing with Multiplayer Play Mode
- Test scenarios:
  1. Create lobby
  2. Join lobby
  3. Start game
  4. Player spawning
  5. Stat synchronization
  6. Disconnection handling

### Build Testing
- Build and run multiple instances
- Test actual network conditions
- Verify Relay NAT traversal
- Check performance

## Known Issues & Workarounds

### Issue: No Host Migration
**Impact**: If host leaves, lobby closes  
**Workaround**: Display warning to host  
**Future Fix**: Implement host migration via Netcode

### Issue: No Reconnection
**Impact**: Disconnected players cannot rejoin  
**Workaround**: Leave lobby and create/join new one  
**Future Fix**: Implement reconnection tokens

### Issue: 8 Player Limit
**Impact**: Cannot have more than 8 players  
**Workaround**: None (Relay free tier limit)  
**Future Fix**: Use dedicated server or paid Relay tier

### Issue: Requires Internet
**Impact**: Cannot play offline or LAN  
**Workaround**: None (depends on cloud services)  
**Future Fix**: Add LAN mode with direct IP connection

## Script Dependencies

```
AuthenticationManager (no deps)
    ↓
LobbyManager → RelayManager
    ↓              ↓
ConnectionManager ←┘
    ↓
NetworkManagerSetup
    ↓
PlayerController
PlayerNetwork
PlayerInventory
```

UI Scripts depend on:
- Network managers for lobby operations
- Player scripts for stats display

## Future Enhancements

### Near Term (Next 1-2 weeks)
- [ ] Button system on walls
- [ ] Item spawning
- [ ] Item database
- [ ] Inventory UI polish

### Medium Term (1-2 months)
- [ ] More item types
- [ ] Door/exit mechanics
- [ ] Win/lose conditions
- [ ] Sound effects
- [ ] Visual polish

### Long Term (2+ months)
- [ ] Steam integration
- [ ] Host migration
- [ ] Reconnection
- [ ] Dedicated server option
- [ ] More game modes

## Debugging Tips

### Enable Detailed Logging
Add to any script:
```csharp
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod]
    static void EnableDebugLogging()
    {
        Debug.unityLogger.logEnabled = true;
        Debug.unityLogger.filterLogType = LogType.Log;
    }
#endif
```

### Network Debug Logging
In NetworkManager:
- Enable "Log Level: Developer" for detailed netcode logs

### Lobby Debug
All lobby operations log with `[Lobby]` prefix
Look for: "Created lobby", "Joined lobby", "Heartbeat", etc.

### Relay Debug
All relay operations log with `[Relay]` prefix
Look for: "Created relay", "Joined relay"

### Common Debug Scenarios

**Problem**: Players not syncing
- Check: NetworkObject on player prefab?
- Check: Player in NetworkManager prefab list?
- Check: IsOwner checks in PlayerController?

**Problem**: Stats not updating
- Check: Server/Host running?
- Check: NetworkVariable values in Network tab
- Check: PlayerNetwork Update() running?

**Problem**: Can't join lobby
- Check: Lobby code correct (case-sensitive)?
- Check: Lobby service enabled?
- Check: Authentication successful?

**Problem**: Relay fails
- Check: Relay service enabled?
- Check: Network connectivity?
- Check: Firewall settings?

## Development Workflow

### Recommended Steps
1. Make code changes
2. Test in Unity Editor (Play Mode)
3. Test with Multiplayer Play Mode (2+ players)
4. Build and test multi-instance
5. Commit changes

### Scene Testing
- MainMenu: Test lobby UI
- GameRoom: Test gameplay

### Multi-Player Testing
- Window > Multiplayer Play Mode
- Enable 2-4 virtual players
- Each runs in separate process
- Can debug all at once

## Version History

### v0.1 - Initial Implementation (Current)
- ✅ Core networking scripts
- ✅ Lobby system
- ✅ Player movement and stats
- ✅ Basic UI framework
- ✅ Inventory structure

### v0.2 - Unity Setup (In Progress)
- ⬜ Scene creation
- ⬜ UI implementation
- ⬜ Prefab creation
- ⬜ Testing

### v0.3 - Game Mechanics (Planned)
- ⬜ Button system
- ⬜ Item spawning
- ⬜ Item usage
- ⬜ Win/lose conditions

## Resources

### Unity Documentation
- [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Lobbies](https://docs.unity.com/lobby/introduction.html)
- [Unity Relay](https://docs.unity.com/relay/introduction.html)

### Community
- [Unity Multiplayer Forum](https://forum.unity.com/forums/multiplayer.26/)
- [Netcode Discord](https://discord.gg/unity)

### Learning Resources
- [Unity Multiplayer Networking Cookbook](https://docs-multiplayer.unity3d.com/netcode/current/learn/index.html)
- [Boss Room Sample](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop)

---

**Last Updated**: Initial implementation complete
**Status**: Ready for Unity Editor configuration

