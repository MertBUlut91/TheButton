# General Event System Documentation

## Overview

The `GeneralInteractableEvent` system replaces specific puzzle scripts (like `ValveEvent`, `PuzzlePanelEvent`) with a flexible, configurable event system that can be used for any interactive object in the game.

## Features

### ✅ Item Requirements
- Requires specific `ItemData` objects in player inventory
- Optional item consumption on activation
- Automatic inventory checking

### ✅ Hold-to-Interact System
- Configurable hold duration (0 = instant activation)
- Real-time progress feedback
- Cancellable by releasing the key
- Visual progress indicator in interaction prompt

### ✅ Object Rotation
- Rotate multiple objects during interaction
- Configurable rotation speed and axis
- Option to continue rotation after completion
- Visual gizmos in editor for rotation axis

### ✅ Animation Support
- Separate animation triggers for:
  - Activation start
  - Success
  - Failure
- Fully optional - works without animator

### ✅ Sound Effects
- Start sound (when interaction begins)
- Hold loop sound (plays during hold interaction)
- Success sound (when completed)
- Fail sound (when cancelled)
- Denied sound (when missing required items)
- All sounds are optional

### ✅ Particle Effects
- Start effect
- Success effect
- Fail effect
- All effects are optional

### ✅ Visual Feedback
- Color changes based on state:
  - Locked (red) - not activated
  - Interacting (yellow) - currently being used
  - Unlocked (green) - activated
- Customizable colors

### ✅ Network Synchronized
- Full multiplayer support
- Server-authoritative
- Prevents multiple simultaneous interactions
- Shows "Someone is using this..." to other players

### ✅ One-Time or Reusable
- `oneTimeUse` flag for single-use events
- Reusable events for repeatable interactions

## Setup Guide

### 1. Basic Setup

1. Add `GeneralInteractableEvent` component to your GameObject
2. Ensure the GameObject has a `NetworkObject` component
3. Add a collider for player interaction detection

### 2. Item Requirements

```
Required Items:
- Add ItemData assets that players need in inventory
- Example: Wrench, Screwdriver, Key, etc.

Consume Items:
- ✅ Checked: Items are removed from inventory when used
- ❌ Unchecked: Items stay in inventory (just need to have them)
```

### 3. Interaction Settings

```
One Time Use:
- ✅ Checked: Can only be activated once
- ❌ Unchecked: Can be used multiple times

Hold Duration:
- 0 seconds: Instant activation (press E once)
- 1-10 seconds: Hold E for this duration to activate
- Shows progress percentage to player
```

### 4. Rotation Settings

```
Rotating Objects:
- Add Transform references to objects you want to rotate
- Can be child objects or separate GameObjects

Rotation Speed:
- Degrees per second (e.g., 90 = quarter turn per second)

Rotation Axis:
- Vector3 defining rotation direction
- Common values:
  - (0, 0, 1) = Z-axis (forward)
  - (0, 1, 0) = Y-axis (up)
  - (1, 0, 0) = X-axis (right)

Continue Rotation After Complete:
- ✅ Checked: Objects keep rotating after activation
- ❌ Unchecked: Objects only rotate during interaction
```

### 5. Animation Setup

```
Animator:
- Assign Animator component (optional)

Trigger Names:
- Activation Trigger: "Activate" (when interaction starts)
- Success Trigger: "Success" (when completed)
- Failure Trigger: "Fail" (when cancelled)

Note: Leave triggers empty if not using animations
```

### 6. Audio Setup

```
Sound Clips:
- Start Sound: Plays when interaction begins
- Hold Loop Sound: Loops during hold interaction
- Success Sound: Plays on successful completion
- Fail Sound: Plays when interaction is cancelled
- Denied Sound: Plays when player lacks required items

Note: All sounds are optional
```

### 7. Visual Feedback

```
Visual Renderer:
- Assign MeshRenderer to change colors

Colors:
- Locked Color: Default red (not activated)
- Unlocked Color: Default green (activated)
- Interacting Color: Default yellow (in progress)
```

### 8. Particle Effects

```
Particle Systems (all optional):
- Start Effect: Plays when interaction begins
- Success Effect: Plays on completion
- Fail Effect: Plays when cancelled
```

## Usage Examples

### Example 1: Valve with Wrench

```
Setup:
- Required Items: [Wrench ItemData]
- Consume Items: ✅ (wrench is used up)
- One Time Use: ✅
- Hold Duration: 3 seconds
- Rotating Objects: [Valve Handle Transform]
- Rotation Speed: 180
- Rotation Axis: (0, 0, 1)
- Continue Rotation After Complete: ✅

Result:
- Player needs wrench in inventory
- Hold E for 3 seconds
- Valve handle rotates while holding
- Wrench is consumed
- Valve continues rotating after completion
- Can only be used once
```

### Example 2: Puzzle Panel with Screwdriver

```
Setup:
- Required Items: [Screwdriver ItemData]
- Consume Items: ❌ (screwdriver stays)
- One Time Use: ✅
- Hold Duration: 2 seconds
- Rotating Objects: [Panel Door Transform]
- Rotation Speed: 90
- Rotation Axis: (0, 1, 0)
- Continue Rotation After Complete: ❌

Result:
- Player needs screwdriver in inventory
- Hold E for 2 seconds
- Panel door rotates open
- Screwdriver stays in inventory
- Door stops at completion
- Can only be used once
```

### Example 3: Instant Button (No Requirements)

```
Setup:
- Required Items: (empty)
- One Time Use: ❌
- Hold Duration: 0 seconds
- Rotating Objects: (empty)

Result:
- No items needed
- Press E to activate instantly
- Can be used multiple times
- Just plays effects and changes color
```

### Example 4: Timed Lever

```
Setup:
- Required Items: (empty)
- One Time Use: ❌
- Hold Duration: 5 seconds
- Rotating Objects: [Lever Transform]
- Rotation Speed: 45
- Rotation Axis: (1, 0, 0)
- Continue Rotation After Complete: ❌

Result:
- No items needed
- Hold E for 5 seconds
- Lever rotates while holding
- If released early, lever resets (fail sound)
- Can be used multiple times
```

## Migration from Old Scripts

### Replacing ValveEvent

**Old ValveEvent:**
```csharp
public class ValveEvent : InteractableEvent
{
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Transform valveHandle;
    [SerializeField] private ParticleSystem activationEffect;
}
```

**New GeneralInteractableEvent:**
1. Remove `ValveEvent` component
2. Add `GeneralInteractableEvent` component
3. Configure:
   - Rotating Objects: Add valve handle
   - Rotation Speed: 90
   - Success Effect: Add activation particle system
   - Required Items: Add wrench if needed

### Replacing PuzzlePanelEvent

**Old PuzzlePanelEvent:**
```csharp
public class PuzzlePanelEvent : InteractableEvent
{
    [SerializeField] private Transform panelDoor;
    [SerializeField] private Vector3 openRotation;
    [SerializeField] private Light[] puzzleLights;
}
```

**New GeneralInteractableEvent:**
1. Remove `PuzzlePanelEvent` component
2. Add `GeneralInteractableEvent` component
3. Configure:
   - Rotating Objects: Add panel door
   - Rotation Axis: Based on openRotation
   - Required Items: Add screwdriver if needed
   - For lights: Use animation or separate script

## Advanced Tips

### Combining with Other Systems

The `GeneralInteractableEvent` can trigger other systems through Unity Events or by extending the class:

```csharp
public class DoorOpenEvent : GeneralInteractableEvent
{
    [SerializeField] private Door doorToOpen;
    
    protected override void OnEventActivated(ulong clientId)
    {
        base.OnEventActivated(clientId);
        doorToOpen?.Open();
    }
}
```

### Progress Feedback

The system automatically shows progress in the interaction prompt:
- "Hold E for 3s" (not started)
- "Hold E (33%)" (in progress)
- "Hold E (66%)" (in progress)
- "Hold E (100%)" (completed)

### Network Considerations

- All state is synchronized across clients
- Only one player can interact at a time
- Other players see "Someone is using this..."
- Server validates all interactions

### Performance

- Minimal performance impact
- Rotation updates only when needed
- Audio sources are reused
- Particle systems are optional

## Troubleshooting

### Issue: "Already activated" but should be reusable
**Solution:** Uncheck "One Time Use"

### Issue: Rotation not working
**Solution:** 
- Check that Rotating Objects list has valid transforms
- Verify Rotation Axis is not (0,0,0)
- Check Rotation Speed is not 0

### Issue: Items not being consumed
**Solution:** Check "Consume Items" checkbox

### Issue: Animation not playing
**Solution:**
- Verify Animator component is assigned
- Check trigger names match animator parameters
- Ensure animator has transitions for triggers

### Issue: Sounds not playing
**Solution:**
- Verify AudioClips are assigned
- Check AudioSource settings
- Ensure 3D spatial blend is correct

## Benefits Over Old System

1. **Single Script**: One script for all event types
2. **More Flexible**: Configure behavior without coding
3. **Better UX**: Hold-to-interact with progress feedback
4. **More Features**: Animations, multiple sounds, particles
5. **Easier to Use**: Inspector-based configuration
6. **Better Feedback**: Visual and audio feedback for all states
7. **Reusable**: Can be used for many different puzzle types

## Future Enhancements

Possible additions:
- Unity Events for custom callbacks
- Multiple hold stages (press, hold, release)
- Proximity-based auto-start
- Team-based requirements (multiple players)
- Cooldown system
- Resource cost (not just items)



