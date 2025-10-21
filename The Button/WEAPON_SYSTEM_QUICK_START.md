# Weapon System - Quick Start Guide

## ✅ What's Been Implemented

The weapon system is now fully functional! Here's what you can do:

### Features
- ✅ **Weapon Category**: New `Weapon` category added to ItemData
- ✅ **6 Weapon Types**: Pistol, Rifle, Shotgun, Knife, Bat, Axe
- ✅ **Damage System**: Weapons deal 1-100 damage to players
- ✅ **5 Inventory Slots**: Press 1-5 to select slots (previously only 1-4)
- ✅ **Auto-Equip**: Selecting a weapon slot automatically equips it
- ✅ **Left-Click Attack**: Attack with equipped weapon
- ✅ **Melee & Ranged**: Support for both weapon types
- ✅ **Network Sync**: All attacks and damage are synchronized

## 🎮 How to Use (Player)

1. **Pick up a weapon** - Press E near a weapon in the world
2. **Select weapon** - Press 1-5 to select the inventory slot with the weapon
3. **Weapon equips** - The weapon automatically appears in your hand
4. **Attack** - Left-click to attack
5. **Drop weapon** - Press Q to drop

## 🛠️ Setup Steps

### Step 1: Add Component to Player Prefab
1. Open your player prefab in Unity
2. Add Component → Search "PlayerWeaponSystem"
3. The component will auto-find necessary references

### Step 2: Test with Example Weapons
Two example weapons have been created:
- **Knife** (Melee): 10 damage, 2m range, 0.3s cooldown
- **Pistol** (Ranged): 15 damage, 30m range, 0.5s cooldown

To test:
```csharp
// Add weapon to player inventory (in your test code)
PlayerInventory inventory = player.GetComponent<PlayerInventory>();
inventory.AddItemServerRpc("Knife");  // or "Pistol"
```

### Step 3: Create Your Own Weapons
1. Right-click in Project → Create → TheButton → Item Data
2. Configure:
   - **Category**: Weapon
   - **Item Type**: Choose weapon type (Pistol, Rifle, Shotgun, Knife, Bat, Axe)
   - **Weapon Damage**: 1-100 (recommended 10-30)
   - **Attack Range**: 
     - Melee: 2-3 meters
     - Ranged: 10-50 meters
   - **Attack Speed**: Time between attacks (0.3-2.0 seconds)
   - **Is Melee Weapon**: Check for melee, uncheck for ranged
3. Save in `Assets/Resources/Items/` folder

## 📝 Code Examples

### Add Weapon to Player
```csharp
PlayerInventory inventory = player.GetComponent<PlayerInventory>();
inventory.AddItemServerRpc("Pistol");
```

### Check if Player Has Weapon Equipped
```csharp
PlayerWeaponSystem weaponSystem = player.GetComponent<PlayerWeaponSystem>();
if (weaponSystem.HasWeaponEquipped())
{
    ItemData weapon = weaponSystem.GetCurrentWeapon();
    Debug.Log($"Equipped: {weapon.itemName}, Damage: {weapon.weaponDamage}");
}
```

### Listen to Weapon Events
```csharp
PlayerWeaponSystem weaponSystem = player.GetComponent<PlayerWeaponSystem>();

weaponSystem.OnWeaponEquipped += (weapon) => {
    Debug.Log($"Equipped: {weapon.itemName}");
};

weaponSystem.OnAttack += (damage) => {
    Debug.Log($"Attacked for {damage} damage!");
};
```

## 🎯 Weapon Properties Explained

### Weapon Damage (1-100)
- **Low**: 5-15 (fast weapons, knives, pistols)
- **Medium**: 20-30 (bats, rifles)
- **High**: 35-50 (axes, shotguns)

### Attack Range (meters)
- **Melee**: 2-3 meters
- **Short Range**: 10-20 meters (shotguns)
- **Medium Range**: 25-35 meters (pistols)
- **Long Range**: 40-50 meters (rifles)

### Attack Speed (seconds)
- **Very Fast**: 0.2-0.4 (knives)
- **Fast**: 0.5-0.7 (pistols)
- **Medium**: 0.8-1.2 (rifles, bats)
- **Slow**: 1.5-2.5 (shotguns, axes)

### Is Melee Weapon
- **Checked**: Close-range weapon (knife, bat, axe)
- **Unchecked**: Ranged weapon (pistol, rifle, shotgun)

## 🔧 Files Modified

1. `ItemCategory.cs` - Added Weapon category
2. `ItemType.cs` - Added 6 weapon types
3. `ItemData.cs` - Added weapon properties (damage, range, speed)
4. `PlayerWeaponSystem.cs` - NEW - Main weapon system
5. `PlayerItemUsage.cs` - Added 5th slot support
6. `PlayerInventory.cs` - Added weapon handling

## 📚 Documentation

For detailed information, see:
- **English**: `WEAPON_SYSTEM_GUIDE.md`
- **Turkish**: `SİLAH_SİSTEMİ_KILAVUZU.md`

## 🎨 Next Steps (Optional)

To make weapons more visual:
1. Create 3D weapon models
2. Assign them to `handModel` in ItemData
3. Create weapon prefabs for world items
4. Add attack effects (particles, sounds)

## ⚠️ Important Notes

- **Network Required**: Weapons only work in multiplayer (host/server must be running)
- **PlayerNetwork Required**: Target must have PlayerNetwork component to take damage
- **5 Slots**: Inventory now supports 5 slots (1-5 keys) instead of 4
- **Auto-Equip**: Weapons equip automatically when you select their slot
- **Left-Click Only**: Right-click is not used for weapons

## 🐛 Troubleshooting

**Weapon not equipping?**
- Make sure PlayerWeaponSystem component is on player prefab
- Check that ItemData category is set to "Weapon"
- Verify the item is in the selected inventory slot

**Can't attack?**
- Wait for attack cooldown (check attackSpeed)
- Make sure weapon is equipped (check console logs)
- Ensure you're within attack range

**No damage dealt?**
- Target must have PlayerNetwork component
- Server must be running (host/dedicated)
- Check console for error messages

## 🎉 Summary

The weapon system is complete and ready to use! You can now:
- ✅ Create weapons with customizable damage (1-100)
- ✅ Select weapons with 1-5 keys
- ✅ Equip weapons automatically
- ✅ Attack with left-click
- ✅ Deal damage to other players
- ✅ Use both melee and ranged weapons

Enjoy your new weapon system! 🎮



