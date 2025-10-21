# GeneralInteractableEvent - System Architecture

## System Overview Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    GeneralInteractableEvent                      │
│                    (NetworkBehaviour)                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │   Item       │  │  Interaction │  │   Rotation   │          │
│  │ Requirements │  │    System    │  │    System    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Animation   │  │    Audio     │  │   Particle   │          │
│  │   System     │  │    System    │  │    System    │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │   Visual     │  │   Network    │  │     State    │          │
│  │   Feedback   │  │     Sync     │  │  Management  │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

## Component Interaction Flow

```
Player Presses E
       │
       ▼
┌──────────────┐
│ IInteractable│
│   Interact() │
└──────┬───────┘
       │
       ▼
┌─────────────────────┐
│ Check Requirements  │
│ - Items in inventory│
│ - Not already used  │
│ - Not in use        │
└──────┬──────────────┘
       │
       ├─── [Missing Items] ──→ Play Denied Sound
       │
       ▼
┌─────────────────────┐
│ Start Interaction   │
│ - ServerRpc         │
│ - Set isInteracting │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│  Play Start Effects │
│ - Start sound       │
│ - Loop sound        │
│ - Start particles   │
│ - Animation trigger │
│ - Color to yellow   │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│   Hold E Key Loop   │
│ - Track progress    │
│ - Rotate objects    │
│ - Update prompt     │
└──────┬──────────────┘
       │
       ├─── [Release E Early] ──→ Cancel Interaction ──→ Play Fail Effects
       │
       ▼
┌─────────────────────┐
│  Complete (100%)    │
│ - ServerRpc         │
│ - Set isActivated   │
│ - Consume items     │
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│ Play Success Effects│
│ - Success sound     │
│ - Success particles │
│ - Animation trigger │
│ - Color to green    │
│ - Continue rotation?│
└─────────────────────┘
```

## Network Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                           SERVER                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  NetworkVariables (Authority)                                    │
│  ┌──────────────────┐  ┌──────────────────┐                    │
│  │   isActivated    │  │  isInteracting   │                    │
│  │   (bool)         │  │   (bool)         │                    │
│  └──────────────────┘  └──────────────────┘                    │
│                                                                   │
│  ServerRpc Methods                                               │
│  ┌────────────────────────────────────────────────────┐         │
│  │ StartInteractionServerRpc(clientId)                │         │
│  │ CompleteInteractionServerRpc()                     │         │
│  │ CancelInteractionServerRpc()                       │         │
│  │ ActivateEventServerRpc(clientId)                   │         │
│  └────────────────────────────────────────────────────┘         │
│                                                                   │
│  State Validation                                                │
│  - Check if already activated                                    │
│  - Check if already interacting                                  │
│  - Prevent simultaneous use                                      │
│                                                                   │
└──────────────────┬──────────────────────────────────────────────┘
                   │
                   │ ClientRpc
                   │ (Broadcast to all clients)
                   │
    ┌──────────────┼──────────────┬──────────────┐
    │              │              │              │
    ▼              ▼              ▼              ▼
┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐
│ Client 1│  │ Client 2│  │ Client 3│  │ Client 4│
├─────────┤  ├─────────┤  ├─────────┤  ├─────────┤
│         │  │         │  │         │  │         │
│ Visual  │  │ Visual  │  │ Visual  │  │ Visual  │
│ Audio   │  │ Audio   │  │ Audio   │  │ Audio   │
│ Effects │  │ Effects │  │ Effects │  │ Effects │
│         │  │         │  │         │  │         │
└─────────┘  └─────────┘  └─────────┘  └─────────┘
```

## State Machine Diagram

```
┌─────────────┐
│    IDLE     │ ◄─────────────────────────┐
│ (Locked)    │                           │
└──────┬──────┘                           │
       │                                  │
       │ [Has Items & Press E]            │
       │                                  │
       ▼                                  │
┌─────────────┐                           │
│ INTERACTING │                           │
│ (Yellow)    │                           │
└──────┬──────┘                           │
       │                                  │
       ├── [Release E Early] ─────────────┤
       │                                  │
       │ [Hold Complete]                  │
       │                                  │
       ▼                                  │
┌─────────────┐                           │
│  ACTIVATED  │                           │
│  (Green)    │                           │
└──────┬──────┘                           │
       │                                  │
       │ [If NOT oneTimeUse] ─────────────┘
       │
       │ [If oneTimeUse]
       │
       ▼
┌─────────────┐
│   LOCKED    │
│ (Permanent) │
└─────────────┘
```

## Module Breakdown

### 1. Item Requirements Module

```
┌─────────────────────────────────────┐
│     Item Requirements Module        │
├─────────────────────────────────────┤
│                                     │
│ Input:                              │
│ - List<ItemData> requiredItems     │
│ - bool consumeItems                 │
│                                     │
│ Functions:                          │
│ - SetRequiredItems()                │
│ - PlayerHasAllRequiredItems()       │
│ - ConsumeRequiredItems()            │
│ - GetRequiredItemNames()            │
│                                     │
│ Output:                             │
│ - bool: Has all items?              │
│ - string: Item names for prompt     │
│                                     │
└─────────────────────────────────────┘
```

### 2. Interaction Module

```
┌─────────────────────────────────────┐
│      Interaction Module             │
├─────────────────────────────────────┤
│                                     │
│ Input:                              │
│ - float holdDuration                │
│ - bool oneTimeUse                   │
│                                     │
│ State:                              │
│ - float interactionProgress         │
│ - bool isLocalPlayerInteracting     │
│ - GameObject currentInteractingPlayer│
│                                     │
│ Functions:                          │
│ - Interact(player)                  │
│ - StartLocalInteraction()           │
│ - StopLocalInteraction()            │
│ - Update() - Track progress         │
│                                     │
│ Output:                             │
│ - Progress percentage               │
│ - Completion/Cancellation           │
│                                     │
└─────────────────────────────────────┘
```

### 3. Rotation Module

```
┌─────────────────────────────────────┐
│       Rotation Module               │
├─────────────────────────────────────┤
│                                     │
│ Input:                              │
│ - List<Transform> rotatingObjects   │
│ - float rotationSpeed               │
│ - Vector3 rotationAxis              │
│ - bool continueRotationAfterComplete│
│                                     │
│ Functions:                          │
│ - Update() - Rotate objects         │
│ - OnDrawGizmosSelected() - Show axis│
│                                     │
│ Output:                             │
│ - Continuous rotation of objects    │
│                                     │
└─────────────────────────────────────┘
```

### 4. Animation Module

```
┌─────────────────────────────────────┐
│      Animation Module               │
├─────────────────────────────────────┤
│                                     │
│ Input:                              │
│ - Animator animator                 │
│ - string activationTrigger          │
│ - string successTrigger             │
│ - string failureTrigger             │
│                                     │
│ Functions:                          │
│ - PlayStartEffectsClientRpc()       │
│ - PlaySuccessEffectsClientRpc()     │
│ - PlayFailEffectsClientRpc()        │
│                                     │
│ Output:                             │
│ - Animator.SetTrigger() calls       │
│                                     │
└─────────────────────────────────────┘
```

### 5. Audio Module

```
┌─────────────────────────────────────┐
│        Audio Module                 │
├─────────────────────────────────────┤
│                                     │
│ Components:                         │
│ - AudioSource audioSource           │
│ - AudioSource loopAudioSource       │
│                                     │
│ Input:                              │
│ - AudioClip startSound              │
│ - AudioClip holdLoopSound           │
│ - AudioClip successSound            │
│ - AudioClip failSound               │
│ - AudioClip deniedSound             │
│                                     │
│ Functions:                          │
│ - PlayStartEffectsClientRpc()       │
│ - PlaySuccessEffectsClientRpc()     │
│ - PlayFailEffectsClientRpc()        │
│ - PlayDeniedSoundClientRpc()        │
│                                     │
│ Output:                             │
│ - 3D spatial audio playback         │
│                                     │
└─────────────────────────────────────┘
```

### 6. Particle Module

```
┌─────────────────────────────────────┐
│      Particle Module                │
├─────────────────────────────────────┤
│                                     │
│ Input:                              │
│ - ParticleSystem startEffect        │
│ - ParticleSystem successEffect      │
│ - ParticleSystem failEffect         │
│                                     │
│ Functions:                          │
│ - PlayStartEffectsClientRpc()       │
│ - PlaySuccessEffectsClientRpc()     │
│ - PlayFailEffectsClientRpc()        │
│                                     │
│ Output:                             │
│ - ParticleSystem.Play() calls       │
│                                     │
└─────────────────────────────────────┘
```

### 7. Visual Feedback Module

```
┌─────────────────────────────────────┐
│    Visual Feedback Module           │
├─────────────────────────────────────┤
│                                     │
│ Input:                              │
│ - MeshRenderer visualRenderer       │
│ - Color lockedColor                 │
│ - Color unlockedColor               │
│ - Color interactingColor            │
│                                     │
│ Functions:                          │
│ - UpdateVisuals()                   │
│ - OnActivatedStateChanged()         │
│ - OnInteractingStateChanged()       │
│                                     │
│ Output:                             │
│ - Material color changes            │
│                                     │
└─────────────────────────────────────┘
```

### 8. Network Sync Module

```
┌─────────────────────────────────────┐
│     Network Sync Module             │
├─────────────────────────────────────┤
│                                     │
│ NetworkVariables:                   │
│ - isActivated (bool)                │
│ - isInteracting (bool)              │
│                                     │
│ ServerRpc:                          │
│ - StartInteractionServerRpc()       │
│ - CompleteInteractionServerRpc()    │
│ - CancelInteractionServerRpc()      │
│ - ActivateEventServerRpc()          │
│                                     │
│ ClientRpc:                          │
│ - PlayStartEffectsClientRpc()       │
│ - PlaySuccessEffectsClientRpc()     │
│ - PlayFailEffectsClientRpc()        │
│ - PlayDeniedSoundClientRpc()        │
│                                     │
│ Callbacks:                          │
│ - OnNetworkSpawn()                  │
│ - OnNetworkDespawn()                │
│ - OnValueChanged callbacks          │
│                                     │
└─────────────────────────────────────┘
```

### 9. State Management Module

```
┌─────────────────────────────────────┐
│    State Management Module          │
├─────────────────────────────────────┤
│                                     │
│ Properties:                         │
│ - IsActivated (bool)                │
│ - IsInteracting (bool)              │
│ - InteractionProgress (float)       │
│                                     │
│ Functions:                          │
│ - CanInteract()                     │
│ - GetInteractionPrompt()            │
│                                     │
│ Output:                             │
│ - UI prompt strings                 │
│ - Interaction availability          │
│                                     │
└─────────────────────────────────────┘
```

## Data Flow Diagram

```
Player Input (E Key)
       │
       ▼
┌─────────────────┐
│  IInteractable  │
│   Interface     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐      ┌──────────────┐
│ Check Items     │─────→│ Inventory    │
└────────┬────────┘      └──────────────┘
         │
         ▼
┌─────────────────┐
│ Start Interact  │
└────────┬────────┘
         │
         ├──→ [Server] ──→ NetworkVariable Update
         │                        │
         │                        ▼
         │                 All Clients Notified
         │                        │
         ▼                        ▼
┌─────────────────┐      ┌──────────────┐
│ Local Progress  │      │ Visual Update│
│ Tracking        │      │ All Clients  │
└────────┬────────┘      └──────────────┘
         │
         ▼
┌─────────────────┐
│ Rotation Update │
│ (Local)         │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Complete?       │
└────────┬────────┘
         │
         ├──→ [Server] ──→ Set Activated
         │                        │
         │                        ▼
         │                 ClientRpc Effects
         │                        │
         ▼                        ▼
┌─────────────────┐      ┌──────────────┐
│ Consume Items   │      │ Play Effects │
│ (Server)        │      │ All Clients  │
└─────────────────┘      └──────────────┘
```

## Performance Considerations

```
┌─────────────────────────────────────────────────────────┐
│                   Performance Profile                    │
├─────────────────────────────────────────────────────────┤
│                                                           │
│ Update Loop:                                             │
│ ├─ Only runs when: isInteracting OR                     │
│ │                  (continueRotation AND isActivated)   │
│ └─ Cost: Minimal (rotation calculations only)           │
│                                                           │
│ Network Traffic:                                         │
│ ├─ NetworkVariables: 2 bools (minimal bandwidth)        │
│ ├─ ServerRpc: Only on state changes                     │
│ └─ ClientRpc: Only for effects (infrequent)             │
│                                                           │
│ Audio:                                                   │
│ ├─ AudioSources: 2 per instance (reused)                │
│ ├─ 3D spatial audio (Unity optimized)                   │
│ └─ No continuous audio processing                       │
│                                                           │
│ Memory:                                                  │
│ ├─ Component size: ~1-2 KB                              │
│ ├─ No runtime allocations                               │
│ └─ Lists pre-allocated in Inspector                     │
│                                                           │
└─────────────────────────────────────────────────────────┘
```

## Extension Points

```
┌─────────────────────────────────────────────────────────┐
│              How to Extend the System                    │
├─────────────────────────────────────────────────────────┤
│                                                           │
│ Method 1: Inherit and Override                          │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ public class CustomEvent : GeneralInteractableEvent │ │
│ │ {                                                   │ │
│ │     protected void OnEventActivated(ulong clientId) │ │
│ │     {                                               │ │
│ │         // Custom logic here                        │ │
│ │     }                                               │ │
│ │ }                                                   │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                           │
│ Method 2: Add Unity Events                              │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ [SerializeField] private UnityEvent onActivated;    │ │
│ │                                                     │ │
│ │ protected void OnEventActivated(ulong clientId)     │ │
│ │ {                                                   │ │
│ │     onActivated?.Invoke();                          │ │
│ │ }                                                   │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                           │
│ Method 3: Component Composition                          │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ Add separate components that listen to state        │ │
│ │ changes via NetworkVariable callbacks               │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                           │
└─────────────────────────────────────────────────────────┘
```

## Dependency Graph

```
GeneralInteractableEvent
    │
    ├─→ Unity.Netcode.NetworkBehaviour
    │   ├─→ NetworkVariable<bool>
    │   ├─→ ServerRpc
    │   └─→ ClientRpc
    │
    ├─→ IInteractable (TheButton.Interactables)
    │   ├─→ Interact(GameObject)
    │   ├─→ GetInteractionPrompt()
    │   └─→ CanInteract()
    │
    ├─→ ItemData (TheButton.Items)
    │   └─→ ScriptableObject
    │
    ├─→ PlayerInventory (TheButton.Player)
    │   ├─→ HasItem(string)
    │   ├─→ GetFirstItemSlot(string)
    │   └─→ UseItemServerRpc(int)
    │
    └─→ Unity Components
        ├─→ Animator
        ├─→ AudioSource
        ├─→ ParticleSystem
        └─→ MeshRenderer
```

---

## Summary

The `GeneralInteractableEvent` system is architected with:

1. **Modular Design** - Each feature is self-contained
2. **Network-First** - Built for multiplayer from the ground up
3. **Performance-Conscious** - Minimal overhead, efficient updates
4. **Extensible** - Easy to add custom behavior
5. **Well-Integrated** - Works with existing game systems

The architecture supports all required features while maintaining clean separation of concerns and efficient performance.



