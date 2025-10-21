# Event System Migration Guide

## Summary of Changes

The old specific event scripts (`ValveEvent`, `PuzzlePanelEvent`) have been replaced with a single, flexible `GeneralInteractableEvent` system.

## Why Migrate?

### Old System Problems:
- ❌ Separate script needed for each puzzle type
- ❌ Limited customization without coding
- ❌ No hold-to-interact functionality
- ❌ Basic audio/visual feedback
- ❌ Hard to add new puzzle types

### New System Benefits:
- ✅ Single script for all event types
- ✅ Full Inspector-based configuration
- ✅ Hold-to-interact with progress feedback
- ✅ Rich audio/visual/animation support
- ✅ Flexible rotation system
- ✅ Easy to create new puzzle types

## Files Affected

### New Files Created:
1. `Assets/Scripts/Interactables/GeneralInteractableEvent.cs` - Main system
2. `GENERAL_EVENT_SYSTEM.md` - English documentation
3. `GENEL_EVENT_SİSTEMİ.md` - Turkish documentation
4. `EVENT_SYSTEM_MIGRATION.md` - This file

### Old Files (Can be deprecated):
1. `Assets/Scripts/Interactables/ValveEvent.cs` - Replace with GeneralInteractableEvent
2. `Assets/Scripts/Interactables/PuzzlePanelEvent.cs` - Replace with GeneralInteractableEvent

### Prefabs to Update:
1. `Assets/Prefabs/TestDoor.prefab` - Currently uses ValveEvent

## Step-by-Step Migration

### For Valve Events

**Before (ValveEvent):**
```
Component: ValveEvent
- requiredItems: [Wrench]
- oneTimeUse: true
- rotationSpeed: 90
- valveHandle: Transform reference
- activationEffect: ParticleSystem
```

**After (GeneralInteractableEvent):**
```
Component: GeneralInteractableEvent

Item Requirements:
- Required Items: [Wrench ItemData]
- Consume Items: ✅

Interaction Settings:
- One Time Use: ✅
- Hold Duration: 3 (seconds)

Rotation Settings:
- Rotating Objects: [Valve Handle Transform]
- Rotation Speed: 90
- Rotation Axis: (0, 0, 1)
- Continue Rotation After Complete: ✅

Particle Effects:
- Success Effect: [Activation Particle System]
```

### For Puzzle Panel Events

**Before (PuzzlePanelEvent):**
```
Component: PuzzlePanelEvent
- requiredItems: [Screwdriver]
- oneTimeUse: true
- panelDoor: Transform
- openRotation: (0, 90, 0)
- openSpeed: 2
- puzzleLights: Light[]
```

**After (GeneralInteractableEvent):**
```
Component: GeneralInteractableEvent

Item Requirements:
- Required Items: [Screwdriver ItemData]
- Consume Items: ❌

Interaction Settings:
- One Time Use: ✅
- Hold Duration: 2 (seconds)

Rotation Settings:
- Rotating Objects: [Panel Door Transform]
- Rotation Speed: 45 (calculated from openSpeed)
- Rotation Axis: (0, 1, 0) (from openRotation)
- Continue Rotation After Complete: ❌

Animation:
- Animator: [Panel Animator]
- Success Trigger: "Open"
```

**Note:** For puzzle lights, either:
1. Use animation to control lights, or
2. Create a simple script that listens to the event

## Migration Checklist

### For Each Prefab Using Old Scripts:

- [ ] Open prefab in Unity
- [ ] Note down all settings from old component
- [ ] Remove old component (ValveEvent or PuzzlePanelEvent)
- [ ] Add GeneralInteractableEvent component
- [ ] Configure all settings:
  - [ ] Item requirements
  - [ ] Hold duration
  - [ ] Rotating objects
  - [ ] Rotation speed and axis
  - [ ] Animation triggers
  - [ ] Sound effects
  - [ ] Particle effects
  - [ ] Visual feedback
- [ ] Test in play mode
- [ ] Test in multiplayer
- [ ] Save prefab

## TestDoor.prefab Migration

Current state: Uses `ValveEvent` with no required items and reusable.

### Recommended Migration:

```
1. Open TestDoor.prefab
2. Select the GameObject with ValveEvent component
3. Remove ValveEvent component
4. Add GeneralInteractableEvent component
5. Configure:
   - Required Items: (empty) - no items needed
   - One Time Use: ❌ - reusable
   - Hold Duration: 0 - instant activation
   - (Leave other settings as default)
6. Save prefab
```

## Testing After Migration

### Test Checklist:
- [ ] Can interact with event
- [ ] Item requirements work correctly
- [ ] Hold-to-interact shows progress
- [ ] Rotation works during interaction
- [ ] Animations play correctly
- [ ] Sounds play at right times
- [ ] Particle effects trigger
- [ ] Visual feedback (color changes)
- [ ] One-time use works if enabled
- [ ] Reusable works if enabled
- [ ] Multiplayer synchronization
- [ ] Multiple players can't use simultaneously
- [ ] Item consumption works if enabled

## Common Migration Issues

### Issue: Rotation axis is wrong
**Solution:** The old scripts used local rotation. Check the Transform's local axes:
- Forward (blue) = (0, 0, 1)
- Up (green) = (0, 1, 0)
- Right (red) = (1, 0, 0)

### Issue: Rotation speed doesn't match
**Solution:** Old scripts may have used different units. Adjust the speed value:
- Old speed * 2 for similar visual result
- Test and tweak until it looks right

### Issue: Missing functionality
**Solution:** Some old scripts had custom logic. You may need to:
1. Extend GeneralInteractableEvent
2. Override OnEventActivated method
3. Add custom behavior

Example:
```csharp
public class CustomDoorEvent : GeneralInteractableEvent
{
    [SerializeField] private Door doorToOpen;
    
    protected void OnEventActivated(ulong clientId)
    {
        // Custom logic here
        doorToOpen?.Open();
    }
}
```

## Backward Compatibility

The old scripts (`ValveEvent`, `PuzzlePanelEvent`) are still in the project and will continue to work. However:

1. **Not Recommended:** Using old scripts for new content
2. **Recommended:** Migrate existing content when convenient
3. **Future:** Old scripts may be removed in future updates

## New Features Available

After migration, you can use these new features:

### 1. Hold-to-Interact
```
Hold Duration: 3 seconds
Shows progress: "Hold E (67%)"
Cancellable by releasing E
```

### 2. Multiple Rotating Objects
```
Rotating Objects:
- Valve Handle
- Gear 1
- Gear 2
All rotate together during interaction
```

### 3. Rich Audio Feedback
```
Start Sound: "valve_start.wav"
Hold Loop Sound: "valve_turning_loop.wav"
Success Sound: "valve_complete.wav"
Fail Sound: "valve_fail.wav"
Denied Sound: "locked.wav"
```

### 4. Animation Integration
```
Activation Trigger: "StartTurn"
Success Trigger: "Complete"
Failure Trigger: "Fail"
```

### 5. Particle Effects
```
Start Effect: Dust particles
Success Effect: Steam burst
Fail Effect: Error sparks
```

### 6. Better Visual Feedback
```
Locked: Red
Interacting: Yellow (with progress)
Unlocked: Green
```

## Support

For questions or issues:
1. Check `GENERAL_EVENT_SYSTEM.md` for detailed documentation
2. Check `GENEL_EVENT_SİSTEMİ.md` for Turkish documentation
3. Look at example configurations in this document
4. Test in play mode to see behavior

## Next Steps

1. **Immediate:** New content should use `GeneralInteractableEvent`
2. **Short-term:** Migrate `TestDoor.prefab` to new system
3. **Medium-term:** Migrate other prefabs as needed
4. **Long-term:** Consider removing old scripts once all content is migrated

---

**Last Updated:** October 18, 2025
**System Version:** 1.0
**Unity Version:** 2022.3+



