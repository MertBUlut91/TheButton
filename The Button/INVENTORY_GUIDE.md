# 📦 Inventory System - Kullanım Kılavuzu

## 🎯 Genel Bakış

The Button oyununda **5 slotluk** inventory sistemi mevcut. Network synchronized, category-based kullanım desteği var.

---

## 🏗️ Unity Editor Setup

### 1. Player Prefab'a Inventory Ekleme

Player prefab'ında zaten `PlayerInventory.cs` component'i var. Kontrol:

```
Player Prefab:
├─ CharacterController
├─ NetworkObject
├─ PlayerController
├─ PlayerNetwork
└─ PlayerInventory ✅
   ├─ Max Slots: 5
   └─ Player Network: [Auto-assigned]
```

**Ayarlar:**
- `Max Slots`: 5 (varsayılan, değiştirilebilir)
- `Player Network`: Otomatik bulunur, manuel atamaya gerek yok

### 2. Inventory UI Setup

GameRoom scene'inde Canvas altında:

```
GameRoomCanvas
└─ InventoryUI GameObject
   ├─ InventoryUI script
   ├─ Inventory Slots Container (Panel)
   │  └─ Horizontal Layout Group
   └─ Slot Prefab (assign)
```

**InventoryUI Script:**
- `Inventory Slots Container`: Panel with Horizontal Layout
- `Slot Prefab`: UI prefab for each slot
- `Max Slots`: 5

**Slot Prefab Yapısı:**
```
InventorySlot (Prefab)
├─ Button component
├─ Image (background)
├─ ItemIcon (Image child)
├─ ItemText (TextMeshPro child)
└─ SlotNumber (TextMeshPro child)
```

---

## 🎮 Inventory Kullanımı

### Player Tarafında

**Item Toplama:**
1. Item'a bak (raycast)
2. "Press E to pick up [Item Name]" görünür
3. E'ye bas
4. Item envantere eklenir

**Item Kullanma:**
- **1-5 tuşları**: İlgili slottaki item'i kullan
- **Tıklama**: Slot'a tıklayarak kullan

**Davranışlar:**
- **Consumable**: Kullanılınca tüketilir, slottan silinir
- **Key**: Kapıda kullanılınca tüketilir
- **Usable**: Kullanılınca aktif olur, envanterde kalır
- **Collectible**: Envanterde kullanılamaz, place mode gerekir

---

## 💻 Script Kullanımı

### PlayerInventory API

```csharp
// Inventory'ye erişim (local player)
PlayerInventory inventory = GetComponent<PlayerInventory>();

// Item ekleme (Server RPC - otomatik)
// WorldItem pickup sırasında çağrılır
inventory.AddItemServerRpc("ItemData_AssetName");

// Item kullanma
inventory.UseItemServerRpc(slotIndex); // 0-4 arası

// Item çıkarma
inventory.RemoveItemAtSlotServerRpc(slotIndex);

// Inventory temizleme
inventory.ClearInventoryServerRpc();

// Kontroller (local)
bool isFull = inventory.IsFull();
int count = inventory.GetItemCount();
ItemData item = inventory.GetItemAtSlot(0);
List<ItemData> allItems = inventory.GetAllItems();

// Type/Category kontrolü
bool hasKey = inventory.HasItemOfCategory(ItemCategory.Key);
bool hasFood = inventory.HasItemOfType(ItemType.Food);
int keySlot = inventory.GetFirstItemOfCategory(ItemCategory.Key);
```

### Events

```csharp
// Inventory değiştiğinde
inventory.OnInventoryChanged += OnInventoryUpdated;

void OnInventoryUpdated()
{
    Debug.Log("Inventory changed!");
    // UI güncelle, vb.
}

// Key kullanıldığında
inventory.OnKeyUsed += OnKeyUsedHandler;

void OnKeyUsedHandler()
{
    Debug.Log("A key was used!");
    // Door unlock logic
}
```

---

## 🔧 Item Ekleme Süreci

### Automatic Pickup (WorldItem)

```csharp
// WorldItem.cs - IInteractable
public void Interact(GameObject playerGameObject)
{
    var playerInventory = playerGameObject.GetComponent<PlayerInventory>();
    
    if (!playerInventory.IsFull())
    {
        // Server'a item ekle
        playerInventory.AddItemServerRpc(itemData.name);
        
        // Item'ı despawn et
        NetworkObject.Despawn(true);
    }
    else
    {
        Debug.Log("Inventory is full!");
    }
}
```

### Manuel Ekleme (Script'ten)

```csharp
// Server-side only!
if (IsServer)
{
    PlayerInventory inventory = player.GetComponent<PlayerInventory>();
    inventory.AddItemServerRpc("Apple_Data"); // Asset name
}
```

---

## 📊 Network Synchronization

### Nasıl Çalışır?

```
Server (Host):
├─ AddItemServerRpc("Apple_Data")
├─ NetworkList<NetworkString>.Add("Apple_Data")
└─ Network sync → Clients

Client:
├─ NetworkList changed event
├─ RebuildLocalInventory()
│  └─ Resources.Load<ItemData>("Items/Apple_Data")
├─ Local cache update (List<ItemData>)
└─ OnInventoryChanged event → UI update
```

**Önemli:**
- ItemData asset'ları **Resources/Items/** klasöründe olmalı!
- Asset name sync ediliyor (max 64 karakter)
- Local cache client-side
- Server authoritative

---

## 🎨 UI Customization

### Slot Appearance

**Category Colors (InventoryUI.cs):**
```csharp
Consumable  → Green  (0, 1, 0)
Collectible → Cyan   (0, 1, 1)
Usable      → Yellow (1, 1, 0)
Key         → Gold   (1, 0.8, 0)
```

### Değiştirme:

```csharp
// InventoryUI.cs
private Color GetItemCategoryColor(ItemCategory category)
{
    return category switch
    {
        ItemCategory.Consumable => Color.green,
        ItemCategory.Collectible => Color.cyan,
        ItemCategory.Usable => Color.yellow,
        ItemCategory.Key => new Color(1f, 0.8f, 0f),
        _ => Color.white
    };
}
```

### Slot Count Değiştirme:

```csharp
// PlayerInventory.cs
[SerializeField] private int maxSlots = 5; // 10'a çıkar

// InventoryUI.cs
[SerializeField] private int maxSlots = 5; // Aynı değer olmalı
```

---

## 🐛 Troubleshooting

### "Inventory is full!"
**Sorun:** Max slot dolmuş.  
**Çözüm:** 
- Item kullan (1-5 tuşları)
- maxSlots sayısını artır
- Item drop sistemi ekle (TODO)

### "Failed to load ItemData from Resources"
**Sorun:** ItemData asset Resources/Items/ klasöründe değil.  
**Çözüm:**
```
Assets/
└─ Resources/
   └─ Items/
      ├─ Apple_Data.asset ✅
      ├─ Key_Data.asset ✅
      └─ ...
```

### Item kullanılmıyor
**Sorun:** Category doğru değil veya stat restore 0.  
**Çözüm:**
```csharp
// ItemData Inspector'da kontrol:
Category: Consumable ✅
Health Restore: 50 ✅ (0 değil!)
```

### Inventory sync olmuyor (multiplayer)
**Sorun:** Server RPC kullanılmamış.  
**Çözüm:**
```csharp
// ❌ YANLIŞ (local only)
inventory.inventoryItems.Add(item);

// ✅ DOĞRU (networked)
inventory.AddItemServerRpc(itemName);
```

---

## 📝 Best Practices

### 1. Server Authority
```csharp
// ✅ DOĞRU - Server RPC kullan
inventory.AddItemServerRpc(itemName);
inventory.UseItemServerRpc(slotIndex);

// ❌ YANLIŞ - Direkt değişiklik yapma
inventory.GetAllItems().Add(item); // Bu çalışmaz!
```

### 2. Item Asset Naming
```csharp
// ✅ DOĞRU - Descriptive names
"Apple_Data"
"WoodenChair_Data"
"Screwdriver_Data"

// ❌ YANLIŞ - Generic names
"Item1"
"Object"
"Thing"
```

### 3. Category Usage
```csharp
// ✅ DOĞRU - Category kontrolü
if (inventory.HasItemOfCategory(ItemCategory.Key))
{
    // Key var, kapıyı aç
}

// ⚠️ ESKI - Type kontrolü (hala çalışır ama category daha iyi)
if (inventory.HasItemOfType(ItemType.Key))
{
    // Bu da çalışır
}
```

---

## 🚀 Gelecek Özellikler

### Planned
- [ ] Item drop (dünyaya atma)
- [ ] Drag & drop (slot'lar arası)
- [ ] Stack system (stackable items)
- [ ] Inventory weight limit
- [ ] Quick slots (hotbar)
- [ ] Item tooltips
- [ ] Sort/filter

### Future
- [ ] Multiple inventory pages
- [ ] Item crafting
- [ ] Item trading (player to player)
- [ ] Persistent inventory (save/load)

---

## 📖 Quick Reference

### Key Methods
```csharp
// Adding
AddItemServerRpc(string itemName)

// Using
UseItemServerRpc(int slotIndex)

// Removing
RemoveItemAtSlotServerRpc(int slotIndex)
ClearInventoryServerRpc()

// Checking
IsFull() → bool
GetItemCount() → int
GetItemAtSlot(int) → ItemData
GetAllItems() → List<ItemData>
HasItemOfCategory(ItemCategory) → bool
HasItemOfType(ItemType) → bool
GetFirstItemOfCategory(ItemCategory) → int (slot)
GetFirstItemOfType(ItemType) → int (slot)
```

### Key Events
```csharp
OnInventoryChanged → Called when inventory changes
OnKeyUsed → Called when a key is used
```

### Important Locations
```
PlayerInventory.cs → Assets/Scripts/Player/
InventoryUI.cs → Assets/Scripts/UI/
ItemData assets → Assets/Resources/Items/
```

---

**Status:** ✅ Complete  
**Network:** Server Authoritative  
**Max Slots:** 5 (configurable)  
**Sync Method:** NetworkList<NetworkString>

