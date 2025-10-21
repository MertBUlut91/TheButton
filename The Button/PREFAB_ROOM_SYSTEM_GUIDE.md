# Prefab-Based Room System - Kullanım Kılavuzu

## 🎯 Genel Bakış

Yeni prefab-based room sistemi, odaları önceden tasarlamanıza ve içeriklerini (buttonlar, eventlar) random olarak yerleştirmenize olanak tanır. Artık prosedürel generation yerine hazır oda prefab'ları kullanıyorsunuz!

## 📦 Sistem Bileşenleri

### 1. WallMarker Component
- Her duvar küpü üzerinde olur
- Sistem bu marker'ları tespit edip yerine button/event yerleştirir
- Marker'ın kendisi görünür bir duvar küpüdür
- Button/event yerleştirildiğinde otomatik olarak devre dışı kalır

### 2. RoomPrefabManager Component
- Oda prefab'ının root'unda olur
- Tüm marker'ları manuel olarak tutar (Inspector'dan atanır)
- Marker listesini ProceduralRoomGenerator'a sağlar

### 3. ProceduralRoomGenerator (Güncellenmiş)
- Artık hem eski sistem hem de yeni prefab sistemi destekler
- `roomConfig.roomPrefab` atanmışsa yeni sistem kullanılır
- Atanmamışsa eski prosedürel sistem çalışır (geriye uyumlu)

## 🛠️ Kurulum Adımları

### Adım 1: Duvar Küpü Oluşturma

1. **Yeni bir Cube oluştur:**
   - Hierarchy'de sağ tık > 3D Object > Cube
   - İsim: `WallCube_001`
   - Scale: (1, 1, 1)
   - Position: Grid snap kullanarak yerleştir

2. **WallMarker component'ini ekle:**
   - Inspector'da `Add Component` > `Wall Marker`
   - Mesh Renderer otomatik atanacak
   - Marker ID otomatik numaralanacak

3. **Material ata (opsiyonel):**
   - Duvar küpüne istediğiniz material'ı atayın
   - Bu material sadece button/event yerleşmediğinde görünecek

4. **Collider kontrol et:**
   - Box Collider olmalı
   - Is Trigger: false

### Adım 2: Oda Prefabı Oluşturma

1. **Root GameObject oluştur:**
   ```
   Hierarchy'de sağ tık > Create Empty
   İsim: "RoomPrefab"
   Position: (0, 0, 0)
   ```

2. **RoomPrefabManager component'ini ekle:**
   ```
   Inspector'da Add Component > Room Prefab Manager
   Room Name: "Test Room 5x5"
   Description: "A simple test room"
   ```

3. **Duvar küplerini yerleştir:**
   ```
   - WallCube_001'i kopyala (Ctrl+D)
   - Grid snap kullanarak yerleştir (Edit > Grid and Snap Settings)
   - Tüm duvarları oluştur
   - Hepsini RoomPrefab'ın child'ı yap
   ```

4. **Köşe ve yapısal duvarlar ekle (opsiyonel):**
   ```
   - WallMarker component'i OLMAYAN küpler ekle
   - Bunlar değişmeyecek, her zaman duvar olarak kalacak
   - Örnek: köşeler, kapı çerçeveleri, vs.
   ```

5. **Marker'ları manager'a ata:**
   ```
   Yöntem 1 (Otomatik):
   - RoomPrefabManager Inspector'da
   - Sağ tık > Collect All Markers From Children
   - Tüm marker'lar otomatik toplanacak
   
   Yöntem 2 (Manuel):
   - RoomPrefabManager > Wall Markers listesini aç
   - Size: 20 (örnek)
   - Her marker'ı sürükle bırak
   ```

6. **Prefab olarak kaydet:**
   ```
   - RoomPrefab'ı Project'e sürükle
   - Konum: Assets/Prefabs/Rooms/
   - İsim: TestRoom_5x5.prefab
   ```

### Adım 3: RoomConfiguration Ayarlama

1. **RoomConfiguration asset'ini aç:**
   ```
   Assets/Resources/RoomConfiguration.asset
   ```

2. **Room Prefab'ı ata:**
   ```
   Room Prefab System:
   └─ Room Prefab: TestRoom_5x5
   ```

3. **Button prefab'larını kontrol et:**
   ```
   Structure Prefabs:
   ├─ Wall Cube With Button Prefab: WallCubeWithButton
   └─ Wall Cube With Enemy Button Prefab: WallCubeWithEnemyButton
   ```

4. **Density ayarlarını kontrol et:**
   ```
   Button Density:
   ├─ Min Button Density Percent: 20
   └─ Max Button Density Percent: 50
   
   Enemy Spawn Button:
   ├─ Min Enemy Button Density Percent: 5
   └─ Max Enemy Button Density Percent: 15
   ```

### Adım 4: Test

1. **Oyunu başlat (Host)**
2. **Oda oluşturulacak**
3. **Kontrol et:**
   - ✅ Bazı marker'lar item button oldu (yeşil)
   - ✅ Bazı marker'lar enemy button oldu (turuncu)
   - ✅ Bazı marker'lar duvar olarak kaldı
   - ✅ Button'lara basınca item/enemy spawn oluyor

## 🎨 Oda Tasarım İpuçları

### 1. Grid Snap Kullanın
```
Edit > Grid and Snap Settings
Grid Size: 1
Snap All Axes: true
```

### 2. Marker Sayısı
```
Küçük oda (5x5): 20-40 marker
Orta oda (10x10): 80-120 marker
Büyük oda (15x15): 180-240 marker
```

### 3. Marker Yerleşimi
```
✅ İYİ:
- Duvarları eşit dağıtılmış marker'larla doldur
- Köşeleri marker OLMAYAN küplerle yap
- Kapı çerçevelerini marker OLMAYAN küplerle yap

❌ KÖTÜ:
- Tüm duvarları marker yap (köşeler dahil)
- Çok az marker kullan (density çalışmaz)
- Marker'ları düzensiz yerleştir
```

### 4. Zemin ve Tavan
```
Seçenek 1: Marker'larla doldur
- Zemin ve tavanda da marker küpleri kullan
- Sistem bunları da button'a çevirebilir

Seçenek 2: Düz plane kullan
- Zemin ve tavan için plane mesh kullan
- Marker YOK, sadece görsel

Seçenek 3: Hiç kullanma
- Zemin ve tavan yok
- Sadece duvarlar
```

## 🔧 Component Detayları

### WallMarker.cs

```csharp
public class WallMarker : MonoBehaviour
{
    // Marker'ın mesh renderer'ı
    public MeshRenderer markerRenderer;
    
    // Marker ID (debug için)
    public int markerId;
    
    // Marker'ı devre dışı bırak
    public void DisableMarker();
    
    // Marker'ı tekrar aktif et
    public void EnableMarker();
}
```

**Gizmos:**
- Sarı wireframe küp: Normal marker
- Cyan wireframe küp: Seçili marker
- Label: Marker ID

### RoomPrefabManager.cs

```csharp
public class RoomPrefabManager : MonoBehaviour
{
    // Manuel marker listesi
    public List<WallMarker> wallMarkers;
    
    // Oda bilgileri
    public string roomName;
    public string description;
    
    // Marker'ları al
    public List<WallMarker> GetAllMarkers();
    
    // Marker sayısı
    public int GetMarkerCount();
    
    // Validasyon
    public bool Validate();
}
```

**Context Menu Komutları:**
- `Auto-Number Markers`: Marker'ları otomatik numarala
- `Collect All Markers From Children`: Tüm child marker'ları topla

**Gizmos:**
- Yeşil küp: Manager pozisyonu
- Yeşil çizgiler: Manager'dan marker'lara
- Label: Oda ismi ve marker sayısı

## 📊 Sistem Akışı

```
1. Oyun başlar (Host)
   ↓
2. ProceduralRoomGenerator.GenerateRoom() çağrılır
   ↓
3. roomConfig.roomPrefab kontrol edilir
   ↓
4a. Prefab VARSA (YENİ SİSTEM):
    - LoadRoomPrefab() → Prefab instantiate edilir
    - GetMarkersFromManager() → Marker listesi alınır
    - CalculateRoomCenterFromPrefab() → Merkez hesaplanır
    - PlaceEventsInPrefabRoom() → Event'ler yerleştirilir:
      • Required event'ler
      • Random event'ler
      • Marker'lar event için kullanılır
    - ProcessMarkers() → Marker'lar işlenir:
      • Shuffle (karıştır)
      • Item button density hesapla
      • Required item'ları yerleştir
      • Random item'ları yerleştir
      • Enemy button density hesapla
      • Enemy button'ları yerleştir
      • Kalan marker'lar duvar olarak kalır
   ↓
4b. Prefab YOKSA (ESKİ SİSTEM):
    - GenerateFloorAndCeiling()
    - PlaceEvents()
    - GenerateWallsWithButtons()
    - GenerateEnemySpawnButtons()
   ↓
5. CreateCeilingSpawnPoint()
   ↓
6. Tamamlandı! 🎉
```

## 🎮 Density Sistemi

### Item Button Density
```csharp
// Örnek: 100 marker var
int totalMarkers = 100;

// Random density seç (%20-50)
float density = Random.Range(20f, 50f); // Örn: 35%

// Button sayısı hesapla
int buttonCount = 100 * 0.35 = 35 button

// Required item'lar önce
int requiredItems = 5;
int randomItems = 30;

Sonuç: 35 item button
```

### Enemy Button Density
```csharp
// Kalan marker'lar
int remaining = 100 - 35 = 65 marker

// Random density seç (%5-15)
float density = Random.Range(5f, 15f); // Örn: 10%

// Enemy button sayısı hesapla
int enemyButtonCount = 65 * 0.10 = 6 button

Sonuç: 6 enemy button
```

### Kalan Marker'lar
```csharp
// Kalan marker'lar duvar olarak kalır
int remainingWalls = 100 - 35 - 6 = 59 duvar

Sonuç: 59 marker duvar olarak kalır (değişmez)
```

## 🐛 Sorun Giderme

### Marker'lar Button'a Dönüşmüyor

**Kontrol Et:**
1. ✅ `roomConfig.roomPrefab` atanmış mı?
2. ✅ RoomPrefab'da `RoomPrefabManager` component'i var mı?
3. ✅ `RoomPrefabManager.wallMarkers` listesi dolu mu?
4. ✅ Marker'larda `WallMarker` component'i var mı?
5. ✅ `roomConfig.wallCubeWithButtonPrefab` atanmış mı?

**Console Log Kontrol:**
```
[RoomGenerator] Using prefab-based room system...
[RoomGenerator] Loading room prefab...
[RoomGenerator] Found X markers in room prefab 'Test Room'
[RoomGenerator] Item Button Density: 35.0% (35 buttons out of 100 markers)
[RoomGenerator] Replaced marker #0 with item button: Pistol
...
```

### Marker'lar Görünmüyor

**Sebep:** Marker'ın mesh renderer'ı devre dışı kalmış

**Çözüm:**
```csharp
// WallMarker script'inde:
marker.EnableMarker(); // Marker'ı tekrar aktif et
```

### Tüm Marker'lar Button Oldu

**Sebep:** Density çok yüksek

**Çözüm:**
```
RoomConfiguration:
- Max Button Density Percent: 50 → 30
- Max Enemy Button Density Percent: 15 → 10
```

### Hiç Enemy Button Yok

**Kontrol Et:**
1. ✅ `roomConfig.wallCubeWithEnemyButtonPrefab` atanmış mı?
2. ✅ `roomConfig.enemyPool` atanmış mı?
3. ✅ EnemyPool'da enemy var mı?
4. ✅ Enemy button density > 0 mı?

### Oda Merkezi Yanlış

**Sebep:** Prefab'ın renderer'ları yok veya yanlış pozisyonda

**Çözüm:**
```csharp
// Manuel olarak ayarla:
roomCenter = new Vector3(10, 5, 10); // Örnek
```

## 💡 İleri Seviye İpuçları

### 1. Farklı Marker Tipleri (Gelecek Özellik)

Şu anda tüm marker'lar aynı tip (random seçim). İleride farklı marker tipleri eklenebilir:

```csharp
public enum MarkerType {
    Any,           // Random (item/enemy/wall)
    ItemOnly,      // Sadece item button
    EnemyOnly,     // Sadece enemy button
    EventOnly,     // Sadece event
    NeverReplace   // Asla değişmez (her zaman duvar)
}
```

### 2. Birden Fazla Oda Prefabı

```csharp
// RoomConfiguration'a liste ekle:
public List<GameObject> roomPrefabs;

// Random seç:
GameObject selectedPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Count)];
```

### 3. Oda Boyutuna Göre Density

```csharp
// Küçük oda: Daha fazla button
if (markerCount < 50) {
    minButtonDensity = 40f;
    maxButtonDensity = 60f;
}
// Büyük oda: Daha az button
else if (markerCount > 150) {
    minButtonDensity = 15f;
    maxButtonDensity = 30f;
}
```

### 4. Event Placement (Gelecek Özellik)

Marker'lar event yerleştirme için de kullanılabilir:

```csharp
// Multi-block event'ler için birden fazla marker kullan
List<WallMarker> eventMarkers = GetAdjacentMarkers(marker, eventSize);
ReplaceMarkersWithEvent(eventMarkers, eventData);
```

## 📝 Checklist

Yeni bir oda prefabı oluştururken:

- [ ] Root GameObject oluşturuldu
- [ ] RoomPrefabManager component'i eklendi
- [ ] Duvar küpleri oluşturuldu
- [ ] Her duvar küpüne WallMarker eklendi
- [ ] Marker'lar RoomPrefabManager'a atandı
- [ ] Köşe ve yapısal duvarlar eklendi (marker YOK)
- [ ] Prefab olarak kaydedildi
- [ ] RoomConfiguration'a atandı
- [ ] Oyunda test edildi
- [ ] Button'lar doğru yerleşiyor
- [ ] Density ayarları uygun
- [ ] Network sync çalışıyor

## 🎉 Sonuç

Artık odalarınızı Unity Editor'de görsel olarak tasarlayabilir ve içeriklerini random olarak yerleştirebilirsiniz!

**Avantajlar:**
✅ Tam kontrol - Oda tasarımını görsel olarak yapın
✅ Hızlı - Prefab instantiate çok hızlı
✅ Esnek - Zemin, tavan, duvar hepsi aynı şekilde
✅ Kolay test - Scene'e koyup test edin
✅ Geriye uyumlu - Eski sistem hala çalışıyor

**Sonraki Adımlar:**
1. İlk test oda prefabınızı oluşturun (5x5, 20 marker)
2. RoomConfiguration'a atayın
3. Oyunu test edin
4. Daha büyük odalar oluşturun
5. Farklı oda tasarımları deneyin

İyi oyunlar! 🎮

