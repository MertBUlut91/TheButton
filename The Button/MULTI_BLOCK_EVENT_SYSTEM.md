# Multi-Block Event System Documentation

## 🎯 Overview

Grid tabanlı oda sistemine multi-block event'ler ekleyen kapsamlı bir sistem. Event'ler farklı boyutlarda olabilir (1x1, 1x2, 2x2, vb.) ve duvar/zemin/tavana yerleştirilebilir. Her event'in açılması için gerekli item'lar otomatik olarak button'lara atanır.

## ✨ Features

- ✅ **Multi-block placement** - Event'ler birden fazla grid bloğu kaplayabilir
- ✅ **Flexible placement** - Duvar, zemin, tavan veya hepsi
- ✅ **Automatic item assignment** - Event için gerekli item'lar otomatik button'lara atanır
- ✅ **Required & random events** - Bazı event'ler her odada spawn olur, bazıları rastgele
- ✅ **Network synchronized** - Tüm event'ler multiplayer'da senkronize
- ✅ **Extensible base class** - Yeni event türleri kolayca eklenir
- ✅ **Smart collision** - Event'ler duvar/button placement'ını etkilemez

## 📦 New Files Created

### Core System
1. **`PlacementType.cs`** - Event yerleşim türleri enum (Wall, Floor, Ceiling, Any)
2. **`EventData.cs`** - Event tanımları için ScriptableObject
3. **`RoomEventPool.cs`** - Event pool yönetimi
4. **`InteractableEvent.cs`** - Event interaction base class

### Example Events
5. **`ValveEvent.cs`** - Vana event örneği (İngiliz anahtarı gerektirir)
6. **`PuzzlePanelEvent.cs`** - Puzzle panel event örneği (Tornavida gerektirir)

### Modified Files
- **`RoomConfiguration.cs`** - Event pool referansı eklendi
- **`ProceduralRoomGenerator.cs`** - Multi-block placement algoritması eklendi

## 🎮 How It Works

### Event Placement Algorithm

```
1. Floor & Ceiling Generation
   └─ Normal oda yapısı oluşturulur

2. Event Placement (NEW!)
   ├─ Required event'ler yerleştirilir
   ├─ Random event'ler yerleştirilir
   ├─ Her event için uygun alan bulunur (size'a göre)
   ├─ Event spawn edilir
   ├─ Event'in kapladığı bloklar işaretlenir
   └─ Event'in required item'ları RoomItemPool'a eklenir

3. Wall & Button Generation
   ├─ Event'lerin kapladığı pozisyonlar skip edilir
   ├─ Kalan pozisyonlara duvar/button yerleştirilir
   └─ Event required item'ları button'larda spawn olur
```

### Grid System

Event'ler grid tabanlı çalışır:
- Her blok 1x1x1 birim (varsayılan cubeSize = 1)
- Event'ler `Vector3Int size` ile tanımlanır
- Örnek: 
  - Kapı: `(1, 2, 1)` - 1 genişlik, 2 yükseklik, 1 derinlik
  - Vana: `(1, 1, 1)` - tek blok
  - Panel: `(2, 2, 1)` - 2x2 panel

## 🔧 Unity Setup Guide

### 1. Create Event Prefabs

#### Example: Exit Door Event

```
1. Create GameObject: "ExitDoor"
   ├─ Add NetworkObject
   ├─ Add ExitDoor script (veya InteractableEvent child class)
   ├─ Add BoxCollider
   └─ Add your 3D model

2. Size: Eğer 2 blok yüksekliğinde ise scale ayarla
   - Transform.localScale = (1, 2, 1)

3. Pivot: Objenin pivot'u CENTER'da olmalı
   - Sistem otomatik olarak multi-block event'lerin merkezini hesaplar
   - Unity default cube zaten center pivot'ta
   - Custom model'ler için: Import Settings → Pivot: Center

4. Save as Prefab: Assets/Prefabs/Events/ExitDoor.prefab
```

#### Example: Valve Event

```
1. Create GameObject: "Valve"
   ├─ Add NetworkObject
   ├─ Add ValveEvent script
   ├─ Add BoxCollider
   ├─ Add valve 3D model
   └─ (Optional) Child object "ValveHandle" for rotation

2. Size: Single block (1x1x1)

3. Save as Prefab: Assets/Prefabs/Events/Valve.prefab
```

#### Example: Puzzle Panel Event

```
1. Create GameObject: "PuzzlePanel"
   ├─ Add NetworkObject
   ├─ Add PuzzlePanelEvent script
   ├─ Add BoxCollider
   └─ Add panel model

2. Components:
   ├─ Child: "PanelDoor" (açılacak kapak)
   ├─ Child: Lights (puzzle ışıkları)
   └─ Indicator renderers

3. Size: 2x2 panel için scale (2, 2, 1)

4. Save as Prefab: Assets/Prefabs/Events/PuzzlePanel.prefab
```

### 2. Register Network Prefabs

Her event prefab'ı NetworkManager'a ekle:

```
1. Open GameRoom scene
2. Select NetworkManager GameObject
3. Add prefabs to Network Prefabs List:
   - ExitDoor
   - Valve
   - PuzzlePanel
   - (Diğer event'leriniz)
```

### 3. Create EventData Assets

#### Exit Door Event Data

```
1. Project > Right-click > Create > TheButton > Event Data
2. Name: "ExitDoor_EventData"
3. Inspector:
   ├─ Event Name: "Exit Door"
   ├─ Description: "Locked exit door requiring a key"
   ├─ Size: X=1, Y=2, Z=1
   ├─ Placement Type: Wall
   ├─ Event Prefab: ExitDoor prefab
   ├─ Required Items:
   │  └─ [0] Key_ItemData
   ├─ Is Required: ✓ (her odada spawn olmalı)
   └─ Spawn Weight: 100
```

#### Valve Event Data

```
1. Project > Create > TheButton > Event Data
2. Name: "Valve_EventData"
3. Inspector:
   ├─ Event Name: "Water Valve"
   ├─ Description: "Valve that controls water flow"
   ├─ Size: X=1, Y=1, Z=1
   ├─ Placement Type: Wall
   ├─ Event Prefab: Valve prefab
   ├─ Required Items:
   │  └─ [0] Wrench_ItemData
   ├─ Is Required: ☐ (rastgele)
   └─ Spawn Weight: 50
```

#### Puzzle Panel Event Data

```
1. Project > Create > TheButton > Event Data
2. Name: "PuzzlePanel_EventData"
3. Inspector:
   ├─ Event Name: "Control Panel"
   ├─ Description: "Electrical panel requiring screwdriver"
   ├─ Size: X=2, Y=2, Z=1
   ├─ Placement Type: Wall (or Floor)
   ├─ Event Prefab: PuzzlePanel prefab
   ├─ Required Items:
   │  └─ [0] Screwdriver_ItemData
   ├─ Is Required: ☐
   └─ Spawn Weight: 30
```

### 4. Create RoomEventPool Asset

```
1. Project > Create > TheButton > Room Event Pool
2. Name: "DefaultRoomEventPool"
3. Inspector:
   
   Required Events: (her odada spawn olacak)
   ├─ [0] ExitDoor_EventData
   
   Random Event Pool: (rastgele seçilecek)
   ├─ [0] Valve_EventData
   ├─ [1] PuzzlePanel_EventData
   
   Random Event Settings:
   ├─ Min Random Events: 0
   └─ Max Random Events: 2
```

### 5. Update RoomConfiguration

```
1. Assets > Resources > DefaultRoomConfiguration
2. Inspector:
   
   Events:
   └─ Event Pool: DefaultRoomEventPool
```

### 6. Create Required Item Assets

Event'lerin ihtiyaç duyduğu item'ları oluştur:

#### Key Item

```
1. Project > Create > TheButton > Item Data
2. Name: "Key_ItemData"
3. Inspector:
   ├─ Item Name: "Key"
   ├─ Category: Key
   ├─ Icon: Key sprite
   └─ Item Prefab: WorldItem_Key prefab
```

#### Wrench Item

```
1. Project > Create > TheButton > Item Data
2. Name: "Wrench_ItemData"
3. Inspector:
   ├─ Item Name: "Wrench"
   ├─ Category: Usable
   ├─ Item Type: Wrench
   └─ Item Prefab: WorldItem_Wrench prefab
```

#### Screwdriver Item

```
1. Project > Create > TheButton > Item Data
2. Name: "Screwdriver_ItemData"
3. Inspector:
   ├─ Item Name: "Screwdriver"
   ├─ Category: Usable
   ├─ Item Type: Screwdriver
   └─ Item Prefab: WorldItem_Screwdriver prefab
```

## 🎯 Usage Examples

### Example 1: Simple Valve (Single Block)

```csharp
EventData:
- eventName: "Water Valve"
- size: (1, 1, 1)
- placementType: Wall
- requiredItems: [Wrench_ItemData]
- isRequired: false
- spawnWeight: 50

Result:
- Vana duvarların birine yerleştirilir
- İngiliz anahtarı bir button'a spawn olur
- Player anahtarı bulup vanayı açar
```

### Example 2: Large Puzzle Panel (2x2)

```csharp
EventData:
- eventName: "Control Panel"
- size: (2, 2, 1)
- placementType: Floor
- requiredItems: [Screwdriver_ItemData]
- isRequired: false
- spawnWeight: 30

Result:
- Panel zemine 2x2 alan kaplayarak yerleştirilir
- Tornavida bir button'a spawn olur
- Player tornavida ile paneli açar
```

### Example 3: Multi-Item Event

```csharp
EventData:
- eventName: "Secure Vault"
- size: (2, 3, 1)
- placementType: Wall
- requiredItems: [Key_ItemData, Keycard_ItemData]
- isRequired: true
- spawnWeight: 100

Result:
- Vault duvara 2x3 alan kaplayarak yerleştirilir
- Hem key hem keycard farklı button'lara spawn olur
- Player her ikisini de bulmalı
```

## 🔌 Creating Custom Events

### Step 1: Create Event Script

```csharp
using Unity.Netcode;
using UnityEngine;
using TheButton.Interactables;

public class MyCustomEvent : InteractableEvent
{
    [Header("Custom Settings")]
    [SerializeField] private GameObject secretDoor;
    
    protected override void OnEventActivated(ulong clientId)
    {
        Debug.Log($"Custom event activated by player {clientId}!");
        
        // Your custom logic here
        if (secretDoor != null)
        {
            OpenSecretDoorClientRpc();
        }
    }
    
    [ClientRpc]
    private void OpenSecretDoorClientRpc()
    {
        // Animation, effects, etc.
        secretDoor.SetActive(true);
    }
    
    public override string GetInteractionPrompt()
    {
        if (isActivated.Value)
        {
            return "Secret door opened!";
        }
        
        return base.GetInteractionPrompt();
    }
}
```

### Step 2: Create Prefab

1. GameObject + NetworkObject + MyCustomEvent script
2. Add visuals, colliders
3. Save as prefab

### Step 3: Create EventData Asset

1. Create > TheButton > Event Data
2. Assign prefab, set size, placement type
3. Add required items

### Step 4: Add to Event Pool

1. Open RoomEventPool asset
2. Add to requiredEvents or randomEventPool

## 📊 System Architecture

```
ProceduralRoomGenerator
├─ RoomConfiguration
│  └─ RoomEventPool
│     ├─ Required Events (EventData[])
│     └─ Random Events (EventData[])
│
├─ Event Placement
│  ├─ Find available space (by size)
│  ├─ Mark grid positions as occupied
│  └─ Spawn event prefab
│
├─ Item Assignment
│  └─ Add event's required items to RoomItemPool
│
└─ Wall Generation
   └─ Skip occupied grid positions
```

## 🎨 Placement Types

### Wall Placement
- Event duvara yerleştirilir (kuzey/güney/doğu/batı)
- Otomatik rotation (duvara bakacak şekilde)
- Örnek: Kapı, vana, panel

### Floor Placement
- Event zemine yerleştirilir
- Rotation: 0 degrees
- Örnek: Trap door, floor panel, puzzle

### Ceiling Placement
- Event tavana yerleştirilir
- Rotation: 0 degrees (veya 180 for hanging)
- Örnek: Vent, hatch, ceiling panel

### Any Placement
- Event herhangi bir yere yerleşebilir
- Sistem en uygun yeri seçer
- Öncelik sırası rastgele

## 🐛 Troubleshooting

### Event spawn olmuyor

**Sorun:** Event'ler odada görünmüyor

**Çözüm:**
1. RoomConfiguration'da eventPool atandı mı kontrol et
2. EventData'da eventPrefab atandı mı kontrol et
3. Event prefab'ında NetworkObject var mı kontrol et
4. Network Prefabs listesinde kayıtlı mı kontrol et
5. Console'da hata mesajları var mı kontrol et

### Event için gereken item button'dan çıkmıyor

**Sorun:** Required item'lar spawn olmuyor

**Çözüm:**
1. EventData'da requiredItems listesi dolu mu kontrol et
2. ItemData asset'ları doğru mu kontrol et
3. RoomItemPool'da yeterince button var mı kontrol et
4. Console log: "Added required item..." mesajını ara

### Event yanlış pozisyonda spawn oluyor

**Sorun:** Event duvarın içinde veya havada

**Çözüm:**
1. Event prefab'ının pivot noktasını kontrol et (CENTER'da olmalı!)
2. Unity Cube kullan (default center pivot) veya custom model → Import Settings → Pivot: Center
3. EventData size değerlerini kontrol et
4. Event prefab scale'ini kontrol et
   - 1x1 event için scale: (1, 1, 1)
   - 1x2 event için scale: (1, 2, 1)
   - 2x2 event için scale: (2, 2, 1)

### Multiple event'ler üst üste spawn oluyor

**Sorun:** Event'ler birbirinin içine geçiyor

**Çözüm:**
- Bu olmamalı! occupiedGridPositions sistemi bunu engelliyor
- Eğer oluyor ise, console'da error olabilir
- MarkSpaceAsOccupied metodunu debug et

## 🎯 Best Practices

### Event Design
1. **Size ayarı:** Event boyutu grid ile uyumlu olsun
2. **Pivot point:** CENTER'da olmalı (Unity default)
3. **Scale:** EventData.size ile prefab scale'i eşleşmeli
4. **Size orientation (ÖNEMLİ!):**
   - **Wall events:** size.x = width (horizontal), size.y = height (vertical), size.z = depth (usually 1)
   - **Floor/Ceiling:** size.x = width, size.y = height, size.z = depth
   - Örnek: 2x2 wall door → size (2, 2, 1) = 2 wide, 2 tall, 1 deep
5. **Collider:** Event prefab'ında proper collider ekle
6. **Network sync:** Tüm visual değişiklikler ClientRpc ile

### Required Items
1. **Mantıklı item'lar:** Kapı → Key, Vana → Wrench
2. **Spawn rate:** Çok fazla required item button sayısını artırır
3. **Balance:** Her event için 1-2 item yeterli

### Placement
1. **Test different sizes:** 1x1, 1x2, 2x2 gibi farklı boyutları test et
2. **Room size:** Çok büyük event'ler küçük odalara sığmayabilir
3. **Density:** Çok fazla event oda'yı doldurabilir

## 📈 Performance

- **Grid tracking:** HashSet<Vector3Int> kullanımı çok hızlı (O(1) lookup)
- **Event placement:** Deterministik, seed-based random
- **Network:** Sadece spawn sırasında network trafiği
- **Memory:** Minimal overhead (sadece grid pozisyonları)

## 🚀 Future Enhancements

Sistem kolayca genişletilebilir:

1. **Event chains:** Bir event diğerini tetikleyebilir
2. **Timed events:** Belirli süre sonra kapanan event'ler
3. **Multi-step puzzles:** Birden fazla event birlikte çözülür
4. **Dynamic sizing:** Runtime'da event boyutu değişebilir
5. **Procedural events:** Random oluşturulan puzzle'lar

## ✅ System Complete

- ✅ Core event system
- ✅ Multi-block placement algorithm
- ✅ Automatic item assignment
- ✅ Example events (Valve, PuzzlePanel)
- ✅ Network synchronization
- ✅ Documentation
- ✅ No compiler errors
- ✅ Ready for Unity setup!

## 🎮 Ready to Use!

Sistem Unity'de setup edilmeye hazır. Yukarıdaki adımları takip ederek event'lerinizi oluşturun ve test edin!

**İyi oyunlar! 🎉**

