# GeneralInteractableEvent - Quick Reference Card

## 🎯 Quick Setup (5 Steps)

1. **Add Component:** `GeneralInteractableEvent` + `NetworkObject`
2. **Set Items:** Add required ItemData to list
3. **Set Duration:** How long to hold E (0 = instant)
4. **Add Rotation:** Add objects to rotate (optional)
5. **Add Effects:** Sounds, animations, particles (optional)

## 📋 Inspector Fields Reference

### Item Requirements
| Field | Type | Description |
|-------|------|-------------|
| Required Items | List<ItemData> | Items needed in inventory |
| Consume Items | bool | Remove items when used? |

### Interaction Settings
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| One Time Use | bool | false | Can only be used once? |
| Hold Duration | float | 2.0 | Seconds to hold E (0 = instant) |

### Rotation Settings
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Rotating Objects | List<Transform> | empty | Objects to rotate |
| Rotation Speed | float | 90 | Degrees per second |
| Rotation Axis | Vector3 | (0,0,1) | Rotation direction |
| Continue After Complete | bool | false | Keep rotating after done? |

### Animation
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Animator | Animator | null | Animator component |
| Activation Trigger | string | "Activate" | Start trigger name |
| Success Trigger | string | "Success" | Complete trigger name |
| Failure Trigger | string | "Fail" | Cancel trigger name |

### Audio
| Field | Type | Description |
|-------|------|-------------|
| Start Sound | AudioClip | When interaction begins |
| Hold Loop Sound | AudioClip | Loops during hold |
| Success Sound | AudioClip | When completed |
| Fail Sound | AudioClip | When cancelled |
| Denied Sound | AudioClip | When missing items |

### Visual Feedback
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Visual Renderer | MeshRenderer | null | Mesh to change color |
| Locked Color | Color | Red | Not activated |
| Unlocked Color | Color | Green | Activated |
| Interacting Color | Color | Yellow | In progress |

### Particle Effects
| Field | Type | Description |
|-------|------|-------------|
| Start Effect | ParticleSystem | When interaction begins |
| Success Effect | ParticleSystem | When completed |
| Fail Effect | ParticleSystem | When cancelled |

## 🎮 Common Configurations

### Instant Button (No Items)
```
Required Items: (empty)
Hold Duration: 0
One Time Use: ❌
```

### Hold Button (No Items)
```
Required Items: (empty)
Hold Duration: 3
One Time Use: ❌
```

### Valve with Wrench
```
Required Items: [Wrench]
Consume Items: ✅
Hold Duration: 3
Rotating Objects: [Valve Handle]
Rotation Speed: 180
Rotation Axis: (0, 0, 1)
Continue After Complete: ✅
One Time Use: ✅
```

### Panel with Screwdriver
```
Required Items: [Screwdriver]
Consume Items: ❌
Hold Duration: 2
Rotating Objects: [Panel Door]
Rotation Speed: 90
Rotation Axis: (0, 1, 0)
Continue After Complete: ❌
One Time Use: ✅
```

### Lever (Reusable)
```
Required Items: (empty)
Hold Duration: 5
Rotating Objects: [Lever]
Rotation Speed: 45
Rotation Axis: (1, 0, 0)
Continue After Complete: ❌
One Time Use: ❌
```

## 🔧 Common Rotation Axes

| Axis | Vector | Use Case |
|------|--------|----------|
| Forward (Z) | (0, 0, 1) | Valve handles, wheels |
| Up (Y) | (0, 1, 0) | Doors, panels, turntables |
| Right (X) | (1, 0, 0) | Levers, switches |
| Custom | (x, y, z) | Any diagonal direction |

## 🎯 Interaction Prompts

| State | Prompt Example |
|-------|----------------|
| Ready (instant) | "Press E to interact" |
| Ready (hold) | "Hold E for 3s" |
| With items | "Hold E for 3s (needs: Wrench)" |
| In progress | "Hold E (67%)" |
| Already used | "Already activated" |
| Someone else using | "Someone is using this..." |

## 🔊 Audio Tips

- **Start Sound:** Short click/clunk (0.1-0.5s)
- **Loop Sound:** Continuous mechanical sound
- **Success Sound:** Satisfying completion sound (0.5-2s)
- **Fail Sound:** Error/cancel sound (0.2-1s)
- **Denied Sound:** Lock/denied sound (0.2-0.5s)

## ✨ Particle Effect Tips

- **Start Effect:** Small burst, 0.5s duration
- **Success Effect:** Larger burst, 1-2s duration
- **Fail Effect:** Quick puff, 0.3s duration

## 🎬 Animation Tips

Create Animator with these triggers:
- `Activate` - Plays when interaction starts
- `Success` - Plays when completed
- `Fail` - Plays when cancelled

Use Animator transitions:
```
Idle -> Activating (trigger: Activate)
Activating -> Success (trigger: Success)
Activating -> Idle (trigger: Fail)
Success -> Idle (after animation)
```

## 🐛 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| Not rotating | Check Rotating Objects list, Rotation Axis not (0,0,0), Speed > 0 |
| No sound | Assign AudioClips, check AudioSource settings |
| No animation | Assign Animator, check trigger names match |
| Can't interact | Check CanInteract() returns true, not already activated |
| Items not consumed | Check "Consume Items" checkbox |
| Wrong rotation direction | Negate axis values (e.g., (0,0,1) → (0,0,-1)) |
| Too fast/slow | Adjust Rotation Speed value |

## 📊 Performance Notes

- ✅ Minimal CPU usage
- ✅ Only updates when interacting
- ✅ Network optimized
- ✅ Audio sources reused
- ✅ Particles optional

## 🌐 Network Behavior

- Server authoritative
- State synchronized to all clients
- Only one player can interact at a time
- Progress shown only to interacting player
- Other players see "Someone is using this..."

## 🔗 Related Files

- **Main Script:** `Assets/Scripts/Interactables/GeneralInteractableEvent.cs`
- **Full Docs:** `GENERAL_EVENT_SYSTEM.md`
- **Turkish Docs:** `GENEL_EVENT_SİSTEMİ.md`
- **Migration Guide:** `EVENT_SYSTEM_MIGRATION.md`

## 💡 Pro Tips

1. **Test hold duration:** 2-3 seconds is comfortable for most players
2. **Visual feedback:** Always use color changes or particles
3. **Audio feedback:** At least success and denied sounds
4. **Rotation speed:** 90-180 degrees/second looks good
5. **Item consumption:** Consider if items should be reusable
6. **One-time use:** Use for story progression, not for gameplay loops
7. **Progress feedback:** Players love seeing progress bars
8. **Cancel option:** Always allow players to cancel by releasing E

## 🎓 Learning Path

1. **Beginner:** Start with instant button (no items, no hold)
2. **Intermediate:** Add hold duration and visual feedback
3. **Advanced:** Add rotation, animations, and particles
4. **Expert:** Extend class for custom behavior

---

**Quick Start:** Add component → Set hold duration → Add rotating objects → Done!

