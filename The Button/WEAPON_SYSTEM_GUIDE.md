# Weapon System Guide

## Overview
The weapon system allows players to equip and use weapons from their inventory. Players can select weapons using number keys (1-5) and attack with left-click. Weapons deal damage to other players and have customizable properties like damage, range, and attack speed.

## Features
- ✅ Weapon category added to item system
- ✅ 6 weapon types: Pistol, Rifle, Shotgun, Knife, Bat, Axe
- ✅ Automatic weapon equipping when selecting inventory slot
- ✅ Left-click to attack with equipped weapon
- ✅ Melee and ranged weapon support
- ✅ Damage system integrated with PlayerNetwork
- ✅ Attack cooldown system
- ✅ Visual weapon models in hand
- ✅ Network synchronized attacks

## Components

### 1. ItemCategory (Updated)
Added new `Weapon` category to the item system:
```csharp
public enum ItemCategory
{
    Consumable,   // Food, water, medkit
    Collectible,  // Furniture, decoration
    Usable,       // Tools, keys
    Key,          // Special door keys
    Weapon        // NEW: Weapons for combat
}
```

### 2. ItemType (Updated)
Added 6 weapon types:
```csharp
// Weapons
Pistol,         // Ranged weapon - low damage, fast fire rate
Rifle,          // Ranged weapon - medium damage, medium fire rate
Shotgun,        // Ranged weapon - high damage, slow fire rate
Knife,          // Melee weapon - low damage, very fast
Bat,            // Melee weapon - medium damage, medium speed
Axe,            // Melee weapon - high damage, slow speed
```

### 3. ItemData (Updated)
Added weapon-specific properties:
```csharp
[Header("Weapon Properties")]
[Range(1f, 100f)]
public float weaponDamage = 10f;        // Damage per attack

[Range(1f, 50f)]
public float attackRange = 2f;          // Attack range in meters

[Range(0.1f, 5f)]
public float attackSpeed = 1f;          // Time between attacks (seconds)

public bool isMeleeWeapon = true;       // Melee vs Ranged
```

### 4. PlayerWeaponSystem (NEW)
Main weapon handling script:
- **Weapon Equipping**: Automatically equips weapons when inventory slot is selected
- **Attack System**: Handles left-click attacks with raycast detection
- **Damage Dealing**: Network-synchronized damage to other players
- **Visual Models**: Displays weapon model in player's hand
- **Attack Cooldown**: Prevents spam attacks based on weapon's attackSpeed

Key methods:
```csharp
EquipWeapon(ItemData weaponData)        // Equip a weapon
UnequipWeapon()                         // Remove current weapon
TryAttack()                             // Attempt to attack
PerformMeleeAttack()                    // Execute melee attack
PerformRangedAttack()                   // Execute ranged attack
```

### 5. PlayerItemUsage (Updated)
Now supports 5 inventory slots (1-5 keys) instead of 4:
```csharp
if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SetSelectedSlot(0);
if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SetSelectedSlot(1);
if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.SetSelectedSlot(2);
if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.SetSelectedSlot(3);
if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.SetSelectedSlot(4);  // NEW
```

### 6. PlayerInventory (Updated)
Added weapon handling in UseItemServerRpc:
```csharp
case ItemCategory.Weapon:
    // Weapons are equipped/unequipped automatically
    // PlayerWeaponSystem handles the equipping
    consumeItem = false;  // Weapons stay in inventory
    break;
```

## Setup Instructions

### 1. Add PlayerWeaponSystem Component
Add the `PlayerWeaponSystem` component to your player prefab:
1. Select your player prefab
2. Add Component → TheButton → Player → PlayerWeaponSystem
3. The component will auto-find PlayerInventory, PlayerNetwork, and Camera

### 2. Configure Weapon Holder (Optional)
For better weapon positioning:
1. Create an empty GameObject as child of player camera
2. Name it "WeaponHolder"
3. Position it at approximately: (0.3, -0.2, 0.5) local position
4. Assign it to PlayerWeaponSystem's "Weapon Holder" field

### 3. Create Weapon ItemData
To create a new weapon:
1. Right-click in Project → Create → TheButton → Item Data
2. Set the following properties:
   - **Item Name**: e.g., "Pistol"
   - **Category**: Weapon
   - **Item Type**: Choose from Pistol, Rifle, Shotgun, Knife, Bat, Axe
   - **Weapon Damage**: 1-100 (recommended: 10-30 for balance)
   - **Attack Range**: 
     - Melee: 2-3 meters
     - Ranged: 10-50 meters
   - **Attack Speed**: 
     - Fast: 0.3-0.5 seconds
     - Medium: 0.8-1.2 seconds
     - Slow: 1.5-2.5 seconds
   - **Is Melee Weapon**: Check for melee, uncheck for ranged
   - **Hand Model**: Assign weapon 3D model prefab
   - **Item Prefab**: Assign world item prefab (for dropping)

### 4. Example Weapon Configurations

#### Pistol (Ranged)
- Damage: 15
- Range: 30
- Attack Speed: 0.5
- Is Melee: false

#### Rifle (Ranged)
- Damage: 25
- Range: 50
- Attack Speed: 1.0
- Is Melee: false

#### Shotgun (Ranged)
- Damage: 40
- Range: 15
- Attack Speed: 2.0
- Is Melee: false

#### Knife (Melee)
- Damage: 10
- Range: 2
- Attack Speed: 0.3
- Is Melee: true

#### Bat (Melee)
- Damage: 20
- Range: 2.5
- Attack Speed: 0.8
- Is Melee: true

#### Axe (Melee)
- Damage: 35
- Range: 2.5
- Attack Speed: 1.5
- Is Melee: true

## Usage

### For Players
1. **Pick up a weapon**: Interact (E) with a weapon in the world
2. **Select weapon**: Press 1-5 to select the inventory slot with the weapon
3. **Weapon auto-equips**: The weapon will appear in your hand
4. **Attack**: Left-click to attack
5. **Switch weapons**: Press another number key (1-5) to switch
6. **Drop weapon**: Press Q to drop the currently selected weapon

### Controls
- **1-5 Keys**: Select inventory slot (auto-equips weapon if present)
- **Left Mouse Button**: Attack with equipped weapon
- **Q**: Drop selected item
- **E**: Use/interact (for non-weapons)

### Attack System
- **Melee Weapons**: 
  - Short range (2-3 meters)
  - Instant hit detection
  - Fast attack speed
  - Good for close combat

- **Ranged Weapons**:
  - Long range (10-50 meters)
  - Instant hit detection (hitscan)
  - Slower attack speed
  - Good for distance combat

### Damage System
- Weapons deal damage to other players on hit
- Damage is synchronized across the network
- Health is managed by PlayerNetwork component
- Players can die when health reaches 0

## Code Integration

### Adding Weapon to Inventory (Code)
```csharp
// Get player inventory
PlayerInventory inventory = player.GetComponent<PlayerInventory>();

// Add weapon by asset name (must be in Resources/Items/)
inventory.AddItemServerRpc("Pistol");
```

### Checking if Player Has Weapon Equipped
```csharp
PlayerWeaponSystem weaponSystem = player.GetComponent<PlayerWeaponSystem>();

if (weaponSystem.HasWeaponEquipped())
{
    ItemData weapon = weaponSystem.GetCurrentWeapon();
    Debug.Log($"Player has {weapon.itemName} equipped");
}
```

### Getting Attack Cooldown
```csharp
PlayerWeaponSystem weaponSystem = player.GetComponent<PlayerWeaponSystem>();
float cooldown = weaponSystem.GetAttackCooldown();

if (cooldown > 0)
{
    Debug.Log($"Cannot attack for {cooldown} seconds");
}
```

### Listening to Weapon Events
```csharp
PlayerWeaponSystem weaponSystem = player.GetComponent<PlayerWeaponSystem>();

// Subscribe to events
weaponSystem.OnWeaponEquipped += (weapon) => {
    Debug.Log($"Equipped: {weapon.itemName}");
};

weaponSystem.OnWeaponUnequipped += () => {
    Debug.Log("Weapon unequipped");
};

weaponSystem.OnAttack += (damage) => {
    Debug.Log($"Attacked with {damage} damage");
};
```

## Network Synchronization

The weapon system is fully network synchronized:
- ✅ Weapon equipping is local (visual only)
- ✅ Attacks are validated on server
- ✅ Damage is applied server-side
- ✅ Hit effects are shown to all clients
- ✅ Inventory changes are synchronized

## Debug Features

### Gizmos
When a weapon is equipped, the attack range is visualized in the Scene view:
- Red ray shows attack direction and range
- Only visible in Unity Editor

### Console Logs
The system logs important events:
- Weapon equipped/unequipped
- Attack attempts
- Hits and misses
- Damage dealt
- Cooldown messages

## Troubleshooting

### Weapon Not Equipping
1. Check that PlayerWeaponSystem component is on player prefab
2. Verify ItemData has category set to "Weapon"
3. Ensure inventory slot contains a weapon item
4. Check console for error messages

### Attacks Not Working
1. Verify weapon is equipped (check console logs)
2. Check attack cooldown (wait for attackSpeed duration)
3. Ensure target is within attack range
4. Check LayerMask settings on PlayerWeaponSystem

### Damage Not Applied
1. Verify target has PlayerNetwork component
2. Check that target is a NetworkObject
3. Ensure server is running (host/dedicated server)
4. Check console for server-side logs

### Weapon Model Not Showing
1. Assign handModel in ItemData
2. Check that handModel prefab has renderers
3. Verify WeaponHolder is properly positioned
4. Check camera reference in PlayerWeaponSystem

## Future Enhancements

Possible additions:
- 🔲 Ammunition system for ranged weapons
- 🔲 Weapon durability/breaking
- 🔲 Weapon attachments/upgrades
- 🔲 Different attack animations
- 🔲 Critical hits/headshots
- 🔲 Weapon recoil and spread
- 🔲 Reload mechanics
- 🔲 Weapon sound effects
- 🔲 Muzzle flash effects
- 🔲 Bullet tracers for ranged weapons
- 🔲 Hit markers and damage indicators
- 🔲 Weapon crafting system

## Summary

The weapon system is now fully implemented and ready to use! Players can:
1. Pick up weapons from the world
2. Store weapons in inventory (5 slots)
3. Select weapons with 1-5 keys (auto-equip)
4. Attack with left-click
5. Deal damage to other players
6. Drop weapons with Q key

All weapon properties are configurable through ItemData ScriptableObjects, making it easy to create and balance different weapons without code changes.



