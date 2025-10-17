# 🎲 Button Density Feature (Yoğunluk Ayarı)

## 🎯 Feature

Her oyun başladığında duvar button'larının **rastgele yoğunluğu** belirlenir!

**Örnek:**
- Oyun 1: %20 button → 100 duvar pozisyonundan 20'si button
- Oyun 2: %35 button → 100 duvar pozisyonundan 35'i button  
- Oyun 3: %50 button → 100 duvar pozisyonundan 50'si button

## ✨ Özellikler

### 1️⃣ RoomConfiguration'da Ayarlar

**Yeni Parametreler:**
```csharp
[Header("Button Density")]
[Range(0f, 100f)]
public float minButtonDensityPercent = 20f; // Minimum %20

[Range(0f, 100f)]
public float maxButtonDensityPercent = 50f; // Maximum %50

[Header("Structure Prefabs")]
public GameObject plainWallCubePrefab; // Button olmayan sade duvar küpü
```

**Unity Inspector'da:**
```
Button Density
├─ Min Button Density Percent: 20 [slider: 0-100]
└─ Max Button Density Percent: 50 [slider: 0-100]

Structure Prefabs
├─ ...
├─ Plain Wall Cube Prefab: [Prefab] ← YENİ!
└─ ...
```

### 2️⃣ Rastgele Yoğunluk Hesaplaması

Her oyun başlangıcında:
```csharp
// Random yoğunluk seç (min-max arasında)
float densityPercent = Random.Range(minButtonDensityPercent, maxButtonDensityPercent);

// Button sayısını hesapla
int totalWallPositions = 288; // Örnek: 10x10 oda
int targetButtonCount = (int)(totalWallPositions * (densityPercent / 100f));

// Örnek: %35 → 288 * 0.35 = 100 button
```

### 3️⃣ Zorunlu Item Koruması

**Önemli:** Required items (anahtar, vb.) **her zaman spawn olur!**

```csharp
int requiredItemCount = 5; // Zorunlu item sayısı

if (targetButtonCount < requiredItemCount)
{
    // Yoğunluk çok düşük! Minimum zorunlu item sayısına yükselt
    targetButtonCount = requiredItemCount;
}
```

**Örnek Senaryo:**
- Min density: %5, Max density: %10
- Total positions: 288
- Random density: %7 → 20 button
- Required items: 5
- ✅ Result: 20 button (5 required + 15 random)

**Ama eğer:**
- Random density: %1 → 3 button
- Required items: 5
- ⚠️ Warning: "Button density too low!"
- ✅ Result: 5 button (minimum, hepsi required)

### 4️⃣ Button Pozisyon Seçimi

**Rastgele Pozisyonlar:**
```csharp
// Tüm duvar pozisyonlarını numaralandır: 0, 1, 2, ..., 287
List<int> allPositions = [0, 1, 2, ..., 287];

// Button olacak pozisyonları rastgele seç
List<int> buttonPositions = RandomSample(allPositions, targetButtonCount);

// Shuffle et
Shuffle(buttonPositions);

// Örnek: [5, 12, 47, 88, 101, 145, ...]
```

**Duvar Oluştururken:**
```csharp
for (int i = 0; i < totalPositions; i++)
{
    if (buttonPositions.Contains(i))
    {
        // Button'lu küp yerleştir
        SpawnWallCubeWithButton(position, itemData);
    }
    else
    {
        // Sade duvar küpü yerleştir (button YOK)
        Instantiate(plainWallCubePrefab, position, rotation);
    }
}
```

## 📐 Matematiksel Açıklama

### Örnek 1: 10x10 Oda, %30 Density

**Hesaplama:**
```
Room: 10 width × 10 depth × 10 height
Wall positions (corners excluded):
  North: (10-2) × 9 = 72
  South: (10-2) × 9 = 72
  East:  (10-2) × 9 = 72
  West:  (10-2) × 9 = 72
  Total: 288 positions

Button density: 30%
Target buttons: 288 × 0.30 = 86 buttons

Required items: 3
Random buttons: 86 - 3 = 83
```

**Sonuç:**
- 86 pozisyonda button
- 202 pozisyonda plain wall cube
- Toplam: 288 küp

### Örnek 2: 15x15 Oda, %50 Density

**Hesaplama:**
```
Room: 15 width × 15 depth × 10 height
Wall positions:
  North: (15-2) × 9 = 117
  South: (15-2) × 9 = 117
  East:  (15-2) × 9 = 117
  West:  (15-2) × 9 = 117
  Total: 468 positions

Button density: 50%
Target buttons: 468 × 0.50 = 234 buttons
```

**Sonuç:**
- 234 pozisyonda button
- 234 pozisyonda plain wall cube
- Toplam: 468 küp
- **Dolu duvarlara göre %50 daha az button!**

## 🎮 Oynanış Etkisi

### Düşük Density (%20)
- ✅ Az button → daha zor
- ✅ Strateji gerekir (hangi buttona basılmalı?)
- ✅ Daha uzun oyun süresi

### Orta Density (%35)
- ✅ Dengeli deneyim
- ✅ Keşfetme + strateji
- ✅ Ortalama oyun süresi

### Yüksek Density (%50)
- ✅ Çok button → daha kolay
- ✅ Hızlı item bulma
- ✅ Kısa oyun süresi

## 🔧 Unity Setup

### 1. Plain Wall Cube Prefab Oluştur

**Assets/Prefabs/PlainWallCube.prefab:**
```
PlainWallCube (GameObject)
├─ MeshFilter (Cube)
├─ MeshRenderer (Material: Wall Material)
├─ BoxCollider
└─ NetworkObject ← Önemli!
```

**Veya Kopyala:**
- `WallCubeWithButton` prefab'ını kopyala
- Button child'ını sil
- İsmi `PlainWallCube` yap
- NetworkObject bırak

### 2. RoomConfiguration Asset Güncelle

**Assets/Resources/DefaultRoomConfiguration:**
```
Button Density:
├─ Min Button Density Percent: 20
└─ Max Button Density Percent: 50

Structure Prefabs:
├─ Wall Cube With Button Prefab: [WallCubeWithButton]
├─ Plain Wall Cube Prefab: [PlainWallCube] ← ATAMAYI UNUTMA!
└─ Corner Cube Prefab: [CornerCube]
```

### 3. Network Prefabs Listesine Ekle

**NetworkManager:**
```
Network Prefabs List:
├─ Player
├─ WallCubeWithButton
├─ PlainWallCube ← YENİ! EKLE!
├─ CornerCube
└─ ...
```

## 📊 Debug Logları

Kod şu log'ları üretir:

```
[RoomGenerator] Button Density: 35.7% (103 buttons out of 288 wall positions)

// Eğer density çok düşükse:
[RoomGenerator] Warning: Button density too low! 
  Required items: 5, target buttons: 3. Increasing to minimum.
```

## 🎯 Oyun Tasarımı Önerileri

### Zorluk Seviyeleri

**Kolay:**
```csharp
minButtonDensityPercent = 40f;
maxButtonDensityPercent = 60f;
// Çok button, kolay bulma
```

**Normal:**
```csharp
minButtonDensityPercent = 25f;
maxButtonDensityPercent = 40f;
// Dengeli
```

**Zor:**
```csharp
minButtonDensityPercent = 10f;
maxButtonDensityPercent = 25f;
// Az button, zor bulma
```

**Ekstrem:**
```csharp
minButtonDensityPercent = 5f;
maxButtonDensityPercent = 15f;
// Çok az button, çok zor!
```

### Progressif Zorluk

Her round'da density azalt:
```csharp
Round 1: %50-%60
Round 2: %40-%50
Round 3: %30-%40
Round 4: %20-%30
Round 5: %10-%20 (Final Boss!)
```

## ✅ Test Senaryoları

1. **Density %20:**
   - ✅ 288 pozisyondan ~57 button
   - ✅ Required items spawn oluyor
   - ✅ Geri kalan plain wall cube

2. **Density %50:**
   - ✅ 288 pozisyondan ~144 button
   - ✅ Yarısı button, yarısı duvar
   - ✅ Görsel denge iyi

3. **Çok Düşük Density (%1):**
   - ✅ Warning: "Button density too low!"
   - ✅ Minimum required item count'a yükseltiliyor
   - ✅ Oyun oynanabilir

4. **Network Sync:**
   - ✅ Her client aynı density görüyor
   - ✅ Aynı pozisyonlarda button
   - ✅ Seed sync çalışıyor

## 🚀 Performans

**Önceki Sistem:**
- 288 button = 288 NetworkObject + 288 SpawnButton script
- Her button sürekli update
- Yüksek CPU/Memory kullanımı

**Yeni Sistem (%30 density):**
- 86 button = 86 NetworkObject + 86 SpawnButton script
- 202 plain cube = 202 NetworkObject (static, no update)
- **%70 daha az button logic!**
- **Daha iyi performans!**

## ✅ Sonuç

**Önceki Sistem:**
- ❌ Tüm duvarlar button (sıkıcı)
- ❌ Çok kolay (her yerde item)
- ❌ Her oyun aynı

**Yeni Sistem:**
- ✅ Rastgele button yoğunluğu
- ✅ Her oyun farklı zorluk
- ✅ Daha stratejik oynanış
- ✅ Ayarlanabilir zorluk (min/max %)
- ✅ Required items korunuyor
- ✅ Performans artışı

**Kullanıcı İsteği:**
> "100 küplük bir alan oluşacak ben oradan min max gireceğim %20 ile %50 arasında button olsun"

**Tamamlandı!** 🎉 Artık her oyun farklı button yoğunluğuna sahip!

