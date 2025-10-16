# 🏷️ ItemCategory vs ItemType - Fark Nedir?

## 🎯 Kısa Cevap

- **ItemCategory** → **Davranışı** belirler (nasıl kullanılır?)
- **ItemType** → **Detayı** belirler (ne tür bir şey?) - **Opsiyonel**

---

## 📊 Karşılaştırma

| Özellik | ItemCategory | ItemType |
|---------|--------------|----------|
| **Amaç** | Davranış kontrolü | Detay/sınıflandırma |
| **Zorunluluk** | ✅ Zorunlu | ⚠️ Opsiyonel |
| **Kullanım** | Active (kod içinde) | Passive (metadata) |
| **Örnek** | Consumable | Food, Water, Medkit |
| **Varsayılan** | - | Generic |

---

## 🔍 Detaylı Açıklama

### ItemCategory (Ana Sınıf)

**4 Kategori:**
```csharp
public enum ItemCategory
{
    Consumable,  // Tüketilir
    Collectible, // Yerleştirilebilir
    Usable,      // Elle kullanılır
    Key          // Özel (kapı açar)
}
```

**Belirlediği Şeyler:**
- ✅ Kullanıldığında ne olur? (consume, place, equip)
- ✅ Envanterde kalır mı? (yes/no)
- ✅ Hangi mekanik çağrılır? (UseConsumable, UseKey, etc.)
- ✅ UI rengi ne olur? (green, cyan, yellow, gold)

**Kod Kullanımı:**
```csharp
// PlayerInventory.cs
switch (itemData.category)
{
    case ItemCategory.Consumable:
        UseConsumable(itemData);  // Stat restore
        consumeItem = true;        // Remove from inventory
        break;
        
    case ItemCategory.Key:
        UseKey(itemData);          // Unlock door
        consumeItem = true;
        break;
        
    case ItemCategory.Usable:
        UseUsable(itemData);       // Equip tool
        consumeItem = false;       // Keep in inventory
        break;
        
    case ItemCategory.Collectible:
        // Can't use from inventory
        // Need placement mode
        break;
}
```

---

### ItemType (Alt Sınıf - Detay)

**20+ Tip:**
```csharp
public enum ItemType
{
    // Consumables
    Food, Water, Medkit, Bandage, EnergyDrink, Poison,
    
    // Collectibles
    Chair, Table, Lamp, Picture, Plant, Box, Barrel,
    
    // Usables
    Key, Screwdriver, Pen, Flashlight, Wrench, Hammer,
    
    // Default
    Generic
}
```

**Belirlediği Şeyler:**
- ⚠️ **Şu anda hiçbir şey!** (future için)
- 🔮 Gelecekte: Crafting recipes
- 🔮 Gelecekte: Quest requirements
- 🔮 Gelecekte: Special interactions

**Kod Kullanımı:**
```csharp
// ŞU ANDA kullanılmıyor (sadece metadata)

// GELECEKTE:

// Crafting example
if (HasItemOfType(ItemType.Hammer) && HasItemOfType(ItemType.Wood))
{
    Craft("Wooden_Crate");
}

// Quest example
quest.Require(ItemType.Food, count: 3);

// Special interaction example
if (itemType == ItemType.Flashlight)
{
    ToggleLight();
}
```

---

## 🎮 Pratik Örnekler

### Örnek 1: Apple (Elma)

```csharp
ItemData: "Apple_Data"
├─ itemName: "Apple"
├─ category: Consumable     ← Davranış (tüketilir)
├─ itemType: Food           ← Detay (yiyecek türü)
├─ hungerRestore: 40
└─ healthRestore: 5

// Kullanım:
category → UseConsumable() → Restore hunger & health
itemType → (şimdilik kullanılmıyor)
```

### Örnek 2: Wooden Chair (Sandalye)

```csharp
ItemData: "Chair_Data"
├─ itemName: "Wooden Chair"
├─ category: Collectible    ← Davranış (yerleştirilir)
├─ itemType: Chair          ← Detay (mobilya türü)
├─ canBePlaced: true
└─ placedPrefab: [Prefab]

// Kullanım:
category → Placement mode required
itemType → (şimdilik kullanılmıyor)
```

### Örnek 3: Screwdriver (Tornavida)

```csharp
ItemData: "Screwdriver_Data"
├─ itemName: "Screwdriver"
├─ category: Usable         ← Davranış (elle kullanılır)
├─ itemType: Screwdriver    ← Detay (alet türü)
├─ canBeHeld: true
└─ handModel: [Prefab]

// Kullanım:
category → UseUsable() → Equip tool
itemType → (gelecekte özel etkileşim)
```

---

## ❓ Sık Sorulan Sorular

### "ItemType gerekli mi?"

**Cevap:** Şu anda hayır, gelecek için evet.

- **Şimdi:** Category yeterli (tüm davranış burada)
- **Gelecek:** Crafting, quests, özel etkileşimler için lazım

**Önerimiz:** Opsiyonel olarak bırak, default `Generic` kullan.

### "Hangisini ne zaman kullanmalıyım?"

**Category kullan:**
- ✅ Item kullanımı (consume/place/equip)
- ✅ UI color coding
- ✅ Inventory behavior
- ✅ Network sync logic

**Type kullan:**
- 🔮 Crafting recipes (gelecek)
- 🔮 Quest requirements (gelecek)
- 🔮 Special interactions (gelecek)
- 📝 Organization/metadata (şimdi)

### "İkisini de aynı yapmak mantıklı mı?"

**Örnek:**
```csharp
category: Key
itemType: Key
```

**Cevap:** Evet, mantıklı! Bazı durumlarda 1:1 mapping normal:
- Key → Key
- Food → Food
- Water → Water

Ama bazı durumlarda farklı:
- Consumable → Food, Water, Medkit (3 farklı type, 1 category)
- Collectible → Chair, Table, Lamp (çok type, 1 category)

---

## 📋 Best Practices

### 1. Category Her Zaman Ayarla ✅

```csharp
// ✅ DOĞRU
category: Consumable
itemType: Generic (veya Food)

// ❌ YANLIŞ
category: (none/default)
itemType: Food
```

### 2. Type Opsiyonel ⚠️

```csharp
// ✅ İyi
category: Consumable
itemType: Generic  // Default, sorun yok

// ✅ Daha iyi
category: Consumable
itemType: Food  // Daha açıklayıcı

// ✅ En iyi
category: Consumable
itemType: Food
description: "Restores hunger"  // Full context
```

### 3. Tutarlı Ol 🎯

```csharp
// ✅ Tutarlı - Mantıklı mapping
Consumable → Food, Water, Medkit
Collectible → Chair, Table, Lamp
Usable → Screwdriver, Hammer, Wrench
Key → Key

// ❌ Tutarsız - Karışık
Consumable → Chair (yanlış!)
Collectible → Food (yanlış!)
```

---

## 🔧 Unity Inspector'da Nasıl Görünür?

### ItemData Asset:

```
┌─────────────────────────────────────┐
│ Apple_Data (ItemData)               │
├─────────────────────────────────────┤
│ Item Name: "Apple"                  │
│                                     │
│ ━━ Item Category ━━━━━━━━━━━━━━━  │
│ Category:    Consumable ▼  ← ZORUNLU│
│ Item Type:   Food ▼        ← Opsiyonel│
│                                     │
│ ━━ Physical Properties ━━━━━━━━━━  │
│ Weight: 0.5                         │
│ ...                                 │
└─────────────────────────────────────┘
```

---

## 🎯 Özet

| Soru | Cevap |
|------|-------|
| Category gerekli mi? | ✅ Evet, zorunlu |
| Type gerekli mi? | ⚠️ Hayır, opsiyonel |
| Hangisi davranışı belirler? | Category |
| Hangisi gelecek için? | Type |
| Default değer? | Type = Generic |
| İkisi aynı olabilir mi? | Evet (Key-Key gibi) |

---

**Sonuç:** 
- **Category** → Must have (zorunlu)
- **Type** → Nice to have (opsiyonel, future-proof)

🎮 İyi oyunlar!

