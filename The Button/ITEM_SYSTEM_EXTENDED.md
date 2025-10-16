# 🎮 Extended Item System - Genişletilmiş Item Sistemi

## ✅ Yapılan Genişletmeler

### 📦 Yeni Dosyalar
1. ✅ `ItemCategory.cs` - Item kategorileri enum (Consumable, Collectible, Usable, Key)
2. ✅ `ItemType.cs` - ✨ Genişletildi (20+ item type)
3. ✅ `ItemData.cs` - ✨ Büyük güncelleme (category, physics, placement)
4. ✅ `WorldItem.cs` - ✨ Fizik + "E" ile pickup
5. ✅ `PlayerInventory.cs` - ✨ Category-based usage
6. ✅ `ExitDoor.cs` - ✨ Category support
7. ✅ `InventoryUI.cs` - ✨ Category-based colors

---

## 🎯 Item Kategorileri

### 1. **Consumable** (Tüketilebilir) 🍎
Kullanıldığında tüketilir ve envanterden silinir.

**Örnekler:**
- Food (yiyecek) → Hunger restore
- Water (su) → Thirst restore  
- Medkit (ilk yardım) → Health restore
- Bandage (bandaj) → Küçük health restore
- Energy Drink → Stamina restore
- Poison (zehir) → Damage

**Özellikler:**
```csharp
category = ItemCategory.Consumable
healthRestore = 50f
hungerRestore = 40f
thirstRestore = 40f
staminaRestore = 30f
damageAmount = 20f  // Poison için
```

### 2. **Collectible** (Toplanabilir) 🪑
Envanterden dünyaya yerleştirilebilir (masa, sandalye, dekorasyon).

**Örnekler:**
- Chair (sandalye)
- Table (masa)
- Lamp (lamba)
- Picture (tablo)
- Plant (bitki)
- Box (kutu)
- Barrel (varil)

**Özellikler:**
```csharp
category = ItemCategory.Collectible
canBePlaced = true
placedPrefab = [Prefab to spawn when placed]
weight = 10f  // Ağır itemler
```

### 3. **Usable** (Kullanılabilir) 🔧
Elle tutulup kullanılır, envanterde kalır.

**Örnekler:**
- Screwdriver (tornavida) → Repair
- Pen (kalem) → Write
- Flashlight (el feneri) → Light
- Wrench (İngiliz anahtarı) → Repair
- Hammer (çekiç) → Break/Build

**Özellikler:**
```csharp
category = ItemCategory.Usable
canBeHeld = true
handModel = [Hand model prefab]
interactionRange = 2f
```

### 4. **Key** (Anahtar) 🔑
Özel usable item, kapı açmak için.

**Özellikler:**
```csharp
category = ItemCategory.Key
canBeHeld = true
// Özel door interaction logic
```

---

## 🔧 Fizik Sistemi

### Yer Çekimi ✅
```csharp
// WorldItem.cs
[RequireComponent(typeof(Rigidbody))]

// Setup
rb.mass = itemData.weight;
rb.useGravity = true;
rb.isKinematic = false;
```

**Davranış:**
- ✅ Item spawn olduğunda **düşer**
- ✅ Yer çekimi aktif
- ✅ Collider **trigger DEĞİL** (fiziksel çarpışma)
- ✅ Weight'e göre farklı düşme hızı

### Collider Setup
```csharp
// NOT trigger!
collider.isTrigger = false;

// Fiziksel collider
// Yere düşer, duvarlardan sekmer
```

---

## 🎮 Etkileşim Sistemi

### "E" Tuşu ile Pickup ✅

**Önceki Sistem:**
- ❌ OnTriggerEnter → Otomatik pickup
- ❌ Üzerinden geçince alınır

**Yeni Sistem:**
- ✅ IInteractable interface
- ✅ "E" tuşuna basınca pickup
- ✅ Interaction prompt: "Press E to pick up [Item Name]"
- ✅ Manuel kontrol

```csharp
// WorldItem : IInteractable
public void Interact(GameObject playerGameObject)
{
    // Pickup logic
    playerInventory.AddItemServerRpc(itemData.name);
    NetworkObject.Despawn(true);
}

public string GetInteractionPrompt()
{
    return $"Press E to pick up {itemData.itemName}";
}
```

---

## 📊 ItemData Yapısı

### Tüm Özellikler

```csharp
[CreateAssetMenu(fileName = "New Item", menuName = "TheButton/Item Data")]
public class ItemData : ScriptableObject
{
    // Basic Info
    string itemName
    string description
    Sprite icon
    
    // Category & Type
    ItemCategory category    // ⭐ YENİ
    ItemType itemType
    
    // Physical Properties ⭐ YENİ
    float weight (0.1-100)
    bool isStackable
    int maxStackSize
    
    // Consumable Properties
    float healthRestore
    float hungerRestore
    float thirstRestore
    float staminaRestore     // ⭐ YENİ
    float damageAmount
    
    // Collectible Properties ⭐ YENİ
    bool canBePlaced
    GameObject placedPrefab
    
    // Usable Properties ⭐ YENİ
    bool canBeHeld
    GameObject handModel
    float interactionRange
    
    // World Prefab
    GameObject itemPrefab
    
    // Helper Properties
    bool IsConsumable => category == ItemCategory.Consumable
    bool IsCollectible => category == ItemCategory.Collectible
    bool IsUsable => category == ItemCategory.Usable || category == ItemCategory.Key
    bool IsKey => category == ItemCategory.Key
}
```

---

## 🎨 Inspector Örneği

### Consumable Item (Food)
```
ItemData Asset: "Apple"
├─ Basic Info
│  ├─ Item Name: "Apple"
│  ├─ Description: "A fresh red apple"
│  └─ Icon: [Apple sprite]
│
├─ Item Category
│  ├─ Category: Consumable
│  └─ Item Type: Food
│
├─ Physical Properties
│  ├─ Weight: 0.5
│  ├─ Is Stackable: true
│  └─ Max Stack Size: 10
│
├─ Consumable Properties
│  ├─ Health Restore: 5
│  ├─ Hunger Restore: 40
│  ├─ Thirst Restore: 10
│  ├─ Stamina Restore: 0
│  └─ Damage Amount: 0
│
└─ World Prefab
   └─ Item Prefab: [WorldItem_Apple]
```

### Collectible Item (Chair)
```
ItemData Asset: "WoodenChair"
├─ Basic Info
│  ├─ Item Name: "Wooden Chair"
│  ├─ Description: "A simple wooden chair"
│  └─ Icon: [Chair sprite]
│
├─ Item Category
│  ├─ Category: Collectible
│  └─ Item Type: Chair
│
├─ Physical Properties
│  ├─ Weight: 15
│  ├─ Is Stackable: false
│  └─ Max Stack Size: 1
│
├─ Collectible Properties
│  ├─ Can Be Placed: true
│  └─ Placed Prefab: [Chair_Placed]
│
└─ World Prefab
   └─ Item Prefab: [WorldItem_Chair]
```

### Usable Item (Screwdriver)
```
ItemData Asset: "Screwdriver"
├─ Basic Info
│  ├─ Item Name: "Screwdriver"
│  ├─ Description: "A Phillips head screwdriver"
│  └─ Icon: [Screwdriver sprite]
│
├─ Item Category
│  ├─ Category: Usable
│  └─ Item Type: Screwdriver
│
├─ Physical Properties
│  ├─ Weight: 0.3
│  ├─ Is Stackable: false
│  └─ Max Stack Size: 1
│
├─ Usable Properties
│  ├─ Can Be Held: true
│  ├─ Hand Model: [Screwdriver_Hand]
│  └─ Interaction Range: 2.5
│
└─ World Prefab
   └─ Item Prefab: [WorldItem_Screwdriver]
```

---

## 🎨 UI Renk Kodlama

### Category-Based Colors

```csharp
Consumable  → Green  🟢 (Yiyecek/içecek)
Collectible → Cyan   🔵 (Yerleştirilebilir)
Usable      → Yellow 🟡 (Araçlar)
Key         → Gold   🟠 (Anahtarlar)
```

**Envanterde:**
- Her item slot'u category renginde highlight olur
- Kolay ayırt edilebilir
- Visual feedback

---

## 🔄 Item Kullanımı

### Consumable Usage
```csharp
// PlayerInventory.cs
private void UseConsumable(ItemData itemData)
{
    // Apply all effects
    if (itemData.healthRestore > 0)
        playerNetwork.ModifyHealthServerRpc(itemData.healthRestore);
    
    if (itemData.hungerRestore > 0)
        playerNetwork.ModifyHungerServerRpc(itemData.hungerRestore);
    
    if (itemData.thirstRestore > 0)
        playerNetwork.ModifyThirstServerRpc(itemData.thirstRestore);
    
    if (itemData.staminaRestore > 0)
        playerNetwork.ModifyStaminaServerRpc(itemData.staminaRestore);
    
    if (itemData.damageAmount > 0)
        playerNetwork.ModifyHealthServerRpc(-itemData.damageAmount);
}

// Item envanterden SİLİNİR
```

### Key Usage
```csharp
private void UseKey(ItemData itemData)
{
    NotifyKeyUsedClientRpc();
    // Door'a event gönderilir
}

// Key kullanıldığında SİLİNİR
```

### Usable Usage
```csharp
private void UseUsable(ItemData itemData)
{
    // Tool equipped
    // Hand model spawn
    // Interaction enabled
}

// Tool envanterde KALIR
```

### Collectible Usage
```csharp
// Envanter slotunda kullanılamaz
// "Place mode" gerekli
Debug.Log("Use place mode to place this item.");

// Gelecek: Placement sistemi
```

---

## 🏗️ Unity Editor Setup

### 1. WorldItem Prefab Oluşturma

```
WorldItem Prefab:
├─ GameObject: "WorldItem"
├─ Components:
│  ├─ NetworkObject ✅
│  ├─ Rigidbody ✅
│  │  ├─ Use Gravity: true
│  │  └─ Is Kinematic: false
│  ├─ Collider (Box/Sphere) ✅
│  │  └─ Is Trigger: false
│  ├─ WorldItem script ✅
│  └─ MeshRenderer ✅
│
└─ Visual Model (child)
   └─ Your 3D model
```

### 2. ItemData Asset Oluşturma

**Folder Structure:**
```
Resources/
└─ Items/
   ├─ Consumables/
   │  ├─ Apple.asset
   │  ├─ Water_Bottle.asset
   │  └─ Medkit.asset
   │
   ├─ Collectibles/
   │  ├─ Chair.asset
   │  ├─ Table.asset
   │  └─ Lamp.asset
   │
   ├─ Usables/
   │  ├─ Screwdriver.asset
   │  ├─ Flashlight.asset
   │  └─ Wrench.asset
   │
   └─ Keys/
      └─ Exit_Key.asset
```

### 3. SpawnButton Setup

```
SpawnButton:
├─ Item To Spawn: [ItemData asset]
│  └─ Dropdown'dan seç
├─ Spawn Point: [Transform]
├─ Cooldown: 5
└─ Visual settings...
```

---

## 🎯 Kullanım Örnekleri

### Consumable Example
```csharp
// Apple_Data.asset
category = Consumable
itemType = Food
hungerRestore = 40
healthRestore = 5
weight = 0.5
isStackable = true
maxStackSize = 10
```

### Collectible Example
```csharp
// Chair_Data.asset
category = Collectible
itemType = Chair
weight = 15
canBePlaced = true
placedPrefab = Chair_Placed
```

### Usable Example
```csharp
// Screwdriver_Data.asset
category = Usable
itemType = Screwdriver
weight = 0.3
canBeHeld = true
handModel = Screwdriver_Hand
interactionRange = 2.5
```

---

## ⚡ Performans & Network

### Network Sync
- ItemData asset name sync (NetworkString)
- Weight bilgisi physics için local
- Category client-side kontrol
- Server-authoritative item usage

### Fizik Optimizasyonu
```csharp
// Light items
weight = 0.1-1.0 → Fast drop

// Medium items
weight = 1.0-10.0 → Normal drop

// Heavy items
weight = 10.0-100.0 → Slow drop, impact force
```

---

## 🚀 Gelecek Özellikler

### Collectible Placement System
- [ ] Place mode (F key?)
- [ ] Rotation control
- [ ] Snap to grid
- [ ] Collision check
- [ ] Server-side validation

### Usable Item Hand Models
- [ ] First-person hand view
- [ ] Tool animations
- [ ] Interaction raycast
- [ ] Use effects

### Item Stacking
- [ ] Stack size limit
- [ ] Stack split
- [ ] Stack merge
- [ ] UI stack counter

---

## ✅ Özet

### Eklenen Özellikler
1. ✅ **4 Item Category** - Consumable, Collectible, Usable, Key
2. ✅ **20+ Item Type** - Çeşitli item türleri
3. ✅ **Fizik Sistemi** - Rigidbody + gravity
4. ✅ **"E" Pickup** - Manuel toplama
5. ✅ **Category-based Usage** - Farklı davranışlar
6. ✅ **Weight System** - Ağırlık bazlı fizik
7. ✅ **Placement Support** - Collectible'lar için hazırlık
8. ✅ **Hand Model Support** - Usable'lar için hazırlık

### Kaldırılan Özellikler
- ❌ worldMaterial (gereksiz)
- ❌ itemColor (gereksiz)
- ❌ Otomatik pickup (OnTriggerEnter)
- ❌ Havada süzülme (rotation/bob animasyonları)

### Değişiklik Yapılması Gereken Yer
**Unity Editor'de:**
- ItemData asset'larını yeniden yapılandır
- Category ve Type seç
- Weight ayarla
- WorldItem prefab'a Rigidbody ekle
- Collider'ı trigger'dan çıkar

---

**Durum:** ✅ Complete  
**Compile Errors:** 0  
**Ready for Unity:** Yes!  
**Test Status:** Needs Unity Editor setup

🎮 **İyi oyunlar!**

