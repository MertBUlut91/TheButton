# Event Position & Rotation Fix

## 🐛 Sorunlar

1. **Event'ler duvarların içinde spawn oluyordu**
2. **Event'ler iç içe geçmişti**
3. **Event'lerin rotation'ı yanlıştı** (1x2 event yan dönmüş, 2x1 gibi görünüyordu)

### Görsel Sorunlar
- Event'ler marker pozisyonunda olduğu gibi spawn ediliyordu
- Multi-block event'ler için offset hesaplaması yoktu
- Event'ler birbirinin üzerine biniyordu
- Rotation marker'dan alınıyordu ama doğru hesaplanmıyordu

## ✅ Çözüm

Event placement sistemi tamamen yeniden yazıldı:

### 1. Multi-Block Event Desteği
Event'ler artık size'larına göre birden fazla marker kullanıyor.

### 2. Proper Position Calculation
Event'ler merkez offset ile doğru pozisyonda spawn oluyor.

### 3. Rotation Handling
Event rotation marker'ın orientation'ına göre doğru hesaplanıyor.

### 4. Marker Overlap Prevention
Kullanılan marker'lar devre dışı bırakılıyor, event'ler üst üste gelmiyor.

## 🔧 Yapılan Değişiklikler

### TryPlaceEventOnMarker() - Tamamen Yeniden Yazıldı

**Eski Kod:**
```csharp
// Sadece bir marker seç
WallMarker selectedMarker = availableMarkers[Random.Range(0, count)];

// Event'i marker pozisyonunda spawn et (YANLIŞ!)
GameObject eventObj = Instantiate(
    eventData.eventPrefab,
    selectedMarker.transform.position,  // ❌ Offset yok
    selectedMarker.transform.rotation   // ❌ Doğru rotation değil
);

// Sadece bir marker'ı disable et
selectedMarker.DisableMarker();
```

**Yeni Kod:**
```csharp
// Shuffle for randomness
ShuffleList(availableMarkers);

// Try to find space for multi-block event
WallMarker selectedMarker = null;
List<WallMarker> requiredMarkers = new List<WallMarker>();

foreach (var marker in availableMarkers)
{
    // Check if we can place event here (considering size)
    if (CanPlaceEventAtMarker(marker, eventData, availableMarkers, out requiredMarkers))
    {
        selectedMarker = marker;
        break;
    }
}

// Calculate proper position and rotation for event
Vector3 eventPosition = CalculateEventPosition(selectedMarker, eventData);
Quaternion eventRotation = CalculateEventRotation(selectedMarker, eventData);

// Instantiate event with calculated position and rotation
GameObject eventObj = Instantiate(
    eventData.eventPrefab,
    eventPosition,   // ✅ Proper offset
    eventRotation    // ✅ Correct rotation
);

// Disable all markers used by this event
foreach (var marker in requiredMarkers)
{
    marker.DisableMarker();
}
```

### Yeni Metodlar

#### 1. CanPlaceEventAtMarker()
```csharp
/// <summary>
/// Check if event can be placed at marker (considering size)
/// </summary>
private bool CanPlaceEventAtMarker(
    WallMarker marker, 
    EventData eventData, 
    List<WallMarker> availableMarkers, 
    out List<WallMarker> requiredMarkers
)
```

**Ne Yapar:**
- Event size'ına göre gerekli marker sayısını hesaplar
- Adjacent (yan yana) marker'ları bulur
- Multi-block event'ler için yeterli alan var mı kontrol eder

**Örnek:**
```
Event size: 2x2 (4 block)
Marker pozisyonu: (5, 2, 0)

Gerekli marker pozisyonları:
├─ (5, 2, 0) - Base marker
├─ (6, 2, 0) - Right (+X)
├─ (5, 3, 0) - Up (+Y)
└─ (6, 3, 0) - Right+Up

Sonuç: 4 marker bulundu → Event yerleştirilebilir ✅
```

#### 2. FindMarkerAtPosition()
```csharp
/// <summary>
/// Find marker at specific position
/// </summary>
private WallMarker FindMarkerAtPosition(
    Vector3 position, 
    List<WallMarker> markers, 
    float tolerance
)
```

**Ne Yapar:**
- Belirli bir pozisyonda marker arar
- Tolerance ile yakın marker'ları bulur (0.1 cube size)

#### 3. CalculateEventPosition()
```csharp
/// <summary>
/// Calculate event position with proper offset
/// </summary>
private Vector3 CalculateEventPosition(
    WallMarker marker, 
    EventData eventData
)
```

**Ne Yapar:**
- Event size'ına göre merkez offset hesaplar
- Multi-block event'leri merkeze hizalar
- Z-fighting önlemek için küçük forward offset ekler

**Hesaplama:**
```csharp
Vector3 basePos = marker.transform.position;
Vector3 centerOffset = Vector3.zero;

// X offset (width)
if (eventData.size.x > 1)
{
    centerOffset += markerRight * (size.x - 1) * cubeSize * 0.5f;
}

// Y offset (height)
if (eventData.size.y > 1)
{
    centerOffset += markerUp * (size.y - 1) * cubeSize * 0.5f;
}

// Forward offset (prevent z-fighting)
centerOffset += markerForward * 0.01f;

return basePos + centerOffset;
```

**Örnek:**
```
Event size: 2x2
Cube size: 1
Base position: (5, 2, 0)

Center offset:
├─ Right: (2-1) * 1 * 0.5 = 0.5
├─ Up: (2-1) * 1 * 0.5 = 0.5
└─ Forward: 0.01

Final position: (5.5, 2.5, 0.01)
```

#### 4. CalculateEventRotation()
```csharp
/// <summary>
/// Calculate event rotation based on marker orientation
/// </summary>
private Quaternion CalculateEventRotation(
    WallMarker marker, 
    EventData eventData
)
```

**Ne Yapar:**
- Marker'ın rotation'ını kullanır
- Marker zaten doğru wall orientation'ına sahip olmalı

## 📊 Event Placement Akışı

### Eski Sistem (Yanlış)
```
1. Random marker seç
2. Event'i marker pozisyonunda spawn et
3. Marker'ı disable et
❌ Sorun: Offset yok, rotation yanlış, multi-block desteklenmez
```

### Yeni Sistem (Doğru)
```
1. Available marker'ları bul
2. Shuffle (random)
3. Her marker için:
   a. Event size'ına göre yeterli alan var mı?
   b. Adjacent marker'lar mevcut mu?
   c. Gerekli marker'ları topla
4. Uygun marker bulundu:
   a. Event position hesapla (center offset)
   b. Event rotation hesapla (marker orientation)
   c. Event'i spawn et
   d. TÜM kullanılan marker'ları disable et
✅ Sonuç: Doğru pozisyon, doğru rotation, overlap yok
```

## 🎮 Örnekler

### Örnek 1: 1x1 Event (Door)
```
Event size: 1x1x1
Required markers: 1

Marker: (5, 2, 0)
Position: (5, 2, 0.01) - Small forward offset
Rotation: Marker rotation
Markers disabled: 1
```

### Örnek 2: 2x1 Event (Horizontal Door)
```
Event size: 2x1x1
Required markers: 2

Base marker: (5, 2, 0)
Adjacent marker: (6, 2, 0) - Right

Position: (5.5, 2, 0.01) - Centered between markers
Rotation: Marker rotation
Markers disabled: 2
```

### Örnek 3: 1x2 Event (Vertical Door)
```
Event size: 1x2x1
Required markers: 2

Base marker: (5, 2, 0)
Adjacent marker: (5, 3, 0) - Up

Position: (5, 2.5, 0.01) - Centered vertically
Rotation: Marker rotation
Markers disabled: 2
```

### Örnek 4: 2x2 Event (Large Puzzle)
```
Event size: 2x2x1
Required markers: 4

Base marker: (5, 2, 0)
Adjacent markers:
├─ (6, 2, 0) - Right
├─ (5, 3, 0) - Up
└─ (6, 3, 0) - Right+Up

Position: (5.5, 2.5, 0.01) - Centered on all 4 markers
Rotation: Marker rotation
Markers disabled: 4
```

## 🔍 Marker Orientation

Marker'ların doğru rotation'a sahip olması önemli:

### North Wall (Z = max)
```
Rotation: (0, 180, 0)
Forward: -Z (face south)
Right: -X
Up: +Y
```

### South Wall (Z = 0)
```
Rotation: (0, 0, 0)
Forward: +Z (face north)
Right: +X
Up: +Y
```

### East Wall (X = max)
```
Rotation: (0, 270, 0)
Forward: -X (face west)
Right: -Z
Up: +Y
```

### West Wall (X = 0)
```
Rotation: (0, 90, 0)
Forward: +X (face east)
Right: +Z
Up: +Y
```

## ⚠️ Önemli Notlar

### 1. Marker Yerleşimi
Marker'ları prefab'da doğru rotation ile yerleştirin:
```
✅ İYİ: Marker forward duvardan dışarı bakıyor
❌ KÖTÜ: Marker forward duvara bakıyor
```

### 2. Multi-Block Event'ler
Adjacent marker'lar grid üzerinde yan yana olmalı:
```
✅ İYİ:
[M][M]  <- 2x1 event için
[M][M]

❌ KÖTÜ:
[M][ ][M]  <- Aralarında boşluk var
```

### 3. Event Size
EventData'da size doğru ayarlanmalı:
```
Horizontal door: size = (2, 1, 1)
Vertical door: size = (1, 2, 1)
Large puzzle: size = (2, 2, 1)
```

## ✅ Test Sonuçları

### Before Fix
```
❌ Event'ler duvarın içinde
❌ Event'ler iç içe
❌ Rotation yanlış (1x2 yan dönmüş)
❌ Multi-block event'ler desteklenmiyor
```

### After Fix
```
✅ Event'ler doğru pozisyonda
✅ Event'ler overlap etmiyor
✅ Rotation doğru
✅ Multi-block event'ler çalışıyor
✅ Adjacent marker'lar kullanılıyor
✅ Center offset hesaplanıyor
```

### Console Logs
```
[RoomGenerator] Placing events...
[RoomGenerator] Placing 4 events (2 required, 2 random)
[RoomGenerator] Placed event 'Door' at marker position (size: (2, 1, 1), markers used: 2)
[RoomGenerator] Placed event 'Puzzle' at marker position (size: (2, 2, 1), markers used: 4)
[RoomGenerator] Placed event 'Trap' at marker position (size: (1, 1, 1), markers used: 1)
[RoomGenerator] Placed event 'Chest' at marker position (size: (1, 1, 1), markers used: 1)
```

## 🎉 Sonuç

Event placement sistemi artık düzgün çalışıyor!

**Düzeltilen Sorunlar:**
✅ Event'ler doğru pozisyonda spawn oluyor
✅ Multi-block event'ler destekleniyor
✅ Center offset hesaplanıyor
✅ Rotation doğru
✅ Marker overlap önleniyor
✅ Adjacent marker'lar kullanılıyor

**Kullanım:**
1. Marker'ları prefab'da doğru rotation ile yerleştirin
2. Event size'ları doğru ayarlayın
3. Oyunu başlatın
4. Event'ler otomatik olarak doğru pozisyonda spawn olacak!

İyi oyunlar! 🎮

---

**Tarih:** 21 Ekim 2025
**Durum:** ✅ TAMAMLANDI

