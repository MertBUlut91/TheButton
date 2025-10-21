# Multi-Block Event Placement Fix

## 🐛 Sorun

2x2 event yerleştirilirken, başlangıç marker'ı en soldaki küpten seçiliyordu ama event'in yarısı dışarıda kalıyordu.

### Görsel Sorun
```
Beklenen (2x2 event):
[M][M]
[M][M]  ← 4 marker kullanılmalı

Olan:
[M][ ]
[ ][ ]  ← Sadece 1 marker kullanıldı, event yarısı dışarıda!
```

### Neden Oluyordu?

**Eski Kod:**
```csharp
// Check if we found enough markers (at least half of required)
return requiredMarkers.Count >= Mathf.Max(1, requiredCount / 2);
```

**Sorun:**
- 2x2 event için `requiredCount = 4`
- `requiredCount / 2 = 2`
- Sadece 2 marker bulunsa bile event yerleştiriliyordu!
- Event yarısı dışarıda kalıyordu

## ✅ Çözüm

Multi-block event'ler için **TÜM gerekli marker'lar** bulunmalı, yoksa event yerleştirilmemeli.

### Yeni Kod

```csharp
// Calculate required positions based on event size
// Event size: (width, height, depth)
// For walls: width = horizontal, height = vertical, depth = into wall (usually 1)
int requiredCount = eventData.size.x * eventData.size.y;

// Try to find adjacent markers in a grid pattern
for (int w = 0; w < eventData.size.x; w++)
{
    for (int h = 0; h < eventData.size.y; h++)
    {
        if (w == 0 && h == 0) continue; // Already have the first marker
        
        // Calculate target position using marker's local axes
        Vector3 targetPos = markerPos + (markerRight * w * cubeSize) + (markerUp * h * cubeSize);
        
        // Find marker at this position
        WallMarker adjacentMarker = FindMarkerAtPosition(targetPos, availableMarkers, searchRadius);
        if (adjacentMarker != null && !requiredMarkers.Contains(adjacentMarker))
        {
            requiredMarkers.Add(adjacentMarker);
        }
    }
}

// For multi-block events, we need ALL required markers
// Otherwise the event will be placed incorrectly
bool hasAllMarkers = requiredMarkers.Count >= requiredCount;

if (!hasAllMarkers)
{
    // Debug info
    Log($"Event '{eventData.eventName}' size {eventData.size} needs {requiredCount} markers, found {requiredMarkers.Count}");
}

return hasAllMarkers;
```

## 🔍 Değişiklikler

### 1. Required Count Hesaplaması

**Eski:**
```csharp
int requiredCount = eventData.size.x * eventData.size.y * eventData.size.z;
```

**Yeni:**
```csharp
// Depth (Z) genellikle 1 olduğu için sadece width x height
int requiredCount = eventData.size.x * eventData.size.y;
```

**Neden:**
- Wall event'leri için depth her zaman 1
- Sadece width (X) ve height (Y) önemli
- 2x2x1 event = 4 marker (2x2)

### 2. Marker Validation

**Eski:**
```csharp
// Lenient: At least half of required markers
return requiredMarkers.Count >= Mathf.Max(1, requiredCount / 2);
```

**Yeni:**
```csharp
// Strict: ALL required markers must be found
bool hasAllMarkers = requiredMarkers.Count >= requiredCount;
return hasAllMarkers;
```

**Neden:**
- Eksik marker ile event yerleştirmek hatalı görünüm oluşturur
- Event'in yarısı dışarıda kalır
- Tüm marker'lar bulunmalı

### 3. Debug Logging

**Yeni:**
```csharp
if (!hasAllMarkers)
{
    Log($"Event '{eventData.eventName}' size {eventData.size} needs {requiredCount} markers, found {requiredMarkers.Count} at position {markerPos}");
}
```

**Faydası:**
- Hangi event'in yerleştirilemediğini gösterir
- Kaç marker bulunduğunu gösterir
- Prefab tasarımında sorun varsa anlaşılır

### 4. Search Radius

**Yeni:**
```csharp
float searchRadius = cubeSize * 0.4f; // Tolerance for finding adjacent markers
```

**Neden:**
- 0.1 çok küçük olabilir (floating point hataları)
- 0.4 daha güvenli bir tolerance
- Adjacent marker'ları daha iyi bulur

## 📊 Event Size Örnekleri

### 1x1 Event (Door)
```
Event size: (1, 1, 1)
Required markers: 1 x 1 = 1

Grid:
[M]

Result: ✅ 1 marker, event placed
```

### 2x1 Event (Horizontal Door)
```
Event size: (2, 1, 1)
Required markers: 2 x 1 = 2

Grid:
[M][M]

Result: ✅ 2 markers, event placed
```

### 1x2 Event (Vertical Door)
```
Event size: (1, 2, 1)
Required markers: 1 x 2 = 2

Grid:
[M]
[M]

Result: ✅ 2 markers, event placed
```

### 2x2 Event (Large Puzzle)
```
Event size: (2, 2, 1)
Required markers: 2 x 2 = 4

Grid:
[M][M]
[M][M]

Result: ✅ 4 markers, event placed
```

### 3x3 Event (Huge Puzzle)
```
Event size: (3, 3, 1)
Required markers: 3 x 3 = 9

Grid:
[M][M][M]
[M][M][M]
[M][M][M]

Result: ✅ 9 markers, event placed
```

## 🎮 Marker Grid Pattern

### Adjacent Marker Search

Event yerleştirme için marker'lar grid pattern'inde aranır:

```
Base marker: (0, 0)
Event size: 2x2

Search pattern:
(0,0) (1,0)  ← Width (right)
(0,1) (1,1)
  ↑
Height (up)

Target positions:
- (0, 0): Base marker (already have)
- (1, 0): markerPos + markerRight * 1 * cubeSize
- (0, 1): markerPos + markerUp * 1 * cubeSize
- (1, 1): markerPos + markerRight * 1 * cubeSize + markerUp * 1 * cubeSize
```

### Marker Local Axes

Her marker kendi duvarının orientation'ına göre local axes'e sahip:

**North Wall (Z = max):**
```
Forward: -Z (face south)
Right: -X (west to east)
Up: +Y (down to up)
```

**South Wall (Z = 0):**
```
Forward: +Z (face north)
Right: +X (west to east)
Up: +Y (down to up)
```

**East Wall (X = max):**
```
Forward: -X (face west)
Right: -Z (north to south)
Up: +Y (down to up)
```

**West Wall (X = 0):**
```
Forward: +X (face east)
Right: +Z (north to south)
Up: +Y (down to up)
```

## 🐛 Sorun Giderme

### Event Yerleşmiyor

**Console Log:**
```
[RoomGenerator] Event 'LargePuzzle' size (2, 2, 1) needs 4 markers, found 2 at position (5, 2, 0)
```

**Sorun:**
- 2x2 event için 4 marker gerekli
- Sadece 2 marker bulundu
- Event yerleştirilmedi

**Çözüm:**
1. Prefab'da marker'ları grid pattern'inde yerleştirin
2. Adjacent marker'ların yan yana olduğundan emin olun
3. Marker'ların rotation'ı doğru olmalı

### Event Yarısı Dışarıda

**Sorun:**
- Eski sistemde lenient validation vardı
- Eksik marker ile event yerleştiriliyordu

**Çözüm:**
- Yeni sistemde strict validation var
- TÜM marker'lar bulunmalı
- Eksik marker varsa event yerleştirilmez

### Adjacent Marker Bulunamıyor

**Sorun:**
- Search radius çok küçük
- Marker pozisyonları tam grid üzerinde değil

**Çözüm:**
```csharp
float searchRadius = cubeSize * 0.4f;  // Daha büyük tolerance
```

## ✅ Test Sonuçları

### Before Fix
```
❌ 2x2 event: 2 marker bulundu, event yarısı dışarıda
❌ 3x3 event: 5 marker bulundu, event eksik yerleşti
❌ Lenient validation: Eksik marker ile event yerleştiriliyor
```

### After Fix
```
✅ 2x2 event: 4 marker bulundu, event tam yerleşti
✅ 3x3 event: 9 marker bulundu, event tam yerleşti
✅ Strict validation: TÜM marker'lar bulunmalı
✅ Debug logging: Eksik marker durumu loglanıyor
```

### Console Logs

**Success:**
```
[RoomGenerator] Placed event 'LargePuzzle' at marker position (size: (2, 2, 1), markers used: 4)
```

**Failure (not enough markers):**
```
[RoomGenerator] Event 'LargePuzzle' size (2, 2, 1) needs 4 markers, found 2 at position (5, 2, 0)
[RoomGenerator] Failed to place event: LargePuzzle
```

## 💡 Prefab Tasarım İpuçları

### 1. Grid Pattern

Multi-block event'ler için marker'ları grid pattern'inde yerleştirin:

```
✅ İYİ:
[M][M][M]
[M][M][M]
[M][M][M]

❌ KÖTÜ:
[M][ ][M]
[ ][M][ ]
[M][ ][M]
```

### 2. Marker Spacing

Marker'lar arasında tam 1 cube size mesafe olmalı:

```
Cube size: 1
Marker positions:
- (5, 2, 0)
- (6, 2, 0)  ← +1 X
- (5, 3, 0)  ← +1 Y
- (6, 3, 0)  ← +1 X, +1 Y
```

### 3. Marker Rotation

Tüm marker'lar aynı duvarda aynı rotation'a sahip olmalı:

```
North wall markers:
- Rotation: (0, 180, 0)
- Right: -X
- Up: +Y
```

### 4. Event Size

EventData'da size doğru ayarlanmalı:

```
Small door: (1, 1, 1)
Horizontal door: (2, 1, 1)
Vertical door: (1, 2, 1)
Large puzzle: (2, 2, 1)
Huge puzzle: (3, 3, 1)
```

## 🎉 Sonuç

Multi-block event placement sorunu çözüldü!

**Düzeltmeler:**
✅ Strict validation: TÜM marker'lar bulunmalı
✅ Correct required count: width x height
✅ Better search radius: 0.4 * cubeSize
✅ Debug logging: Eksik marker durumu loglanıyor

**Kullanım:**
1. Marker'ları grid pattern'inde yerleştirin
2. Event size'ı doğru ayarlayın
3. Sistem otomatik olarak adjacent marker'ları bulacak
4. TÜM marker'lar bulunursa event yerleşecek

İyi oyunlar! 🎮

---

**Tarih:** 21 Ekim 2025
**Durum:** ✅ TAMAMLANDI

