# Enemy Room Integration - Prosedürel Oda Sistemi

## 🎯 Ne Yaptık?

Room Configuration sistemine **Enemy Spawn Button** desteği ekledik. Artık prosedürel olarak oluşturulan odalarda hem item spawn button'ları hem de enemy spawn button'ları rastgele yerleşecek!

## 📦 Yeni/Güncellenen Dosyalar

### 1. EnemyPool.cs (YENİ)
**Konum:** `Assets/Scripts/Enemy/EnemyPool.cs`

**Ne İşe Yarar:**
- Enemy türlerini havuzda tutar (ItemPool gibi)
- Random enemy seçimi yapar
- Weighted random destekler (opsiyonel)

**Özellikler:**
```csharp
public List<EnemyData> availableEnemies;  // Spawn olabilecek tüm enemy'ler
public List<float> spawnWeights;          // Spawn ağırlıkları (opsiyonel)

public EnemyData GetRandomEnemy();        // Rastgele enemy al
public List<EnemyData> GetRandomEnemies(int count); // Birden fazla enemy al
```

### 2. RoomConfiguration.cs (GÜNCELLENDİ)
**Eklenen Alanlar:**

```csharp
[Header("Enemy Spawn Button")]
public GameObject wallCubeWithEnemyButtonPrefab;  // Enemy button prefab
public float minEnemyButtonDensityPercent = 5f;   // Min %5
public float maxEnemyButtonDensityPercent = 15f;  // Max %15

[Header("Enemies")]
public EnemyPool enemyPool;  // Enemy havuzu
```

### 3. ProceduralRoomGenerator.cs (GÜNCELLENDİ)
**Eklenen Metodlar:**

```csharp
private void GenerateEnemySpawnButtons()
// Kalan duvar pozisyonlarına enemy button'ları yerleştirir

private void SpawnWallCubeWithEnemyButton(WallPosition wallPos, EnemyData enemyData)
// Tek bir enemy button spawn eder
```

## 🔄 Sistem Akışı

### Prosedürel Oda Oluşturma Sırası:

```
1. Floor & Ceiling oluştur
    ↓
2. Events yerleştir (door, puzzle, etc.)
    ↓
3. Item Spawn Button'ları yerleştir
   - Density: %20-50 (ayarlanabilir)
   - Required items önce
   - Random items sonra
   - Kullanılan pozisyonlar "usedWallPositions" listesine eklenir
    ↓
4. Enemy Spawn Button'ları yerleştir ⭐ YENİ!
   - Density: %5-15 (ayarlanabilir)
   - KALAN duvar pozisyonlarından seçer
   - Random enemy'ler EnemyPool'dan
    ↓
5. Ceiling spawn point oluştur
    ↓
6. Tamamlandı! 🎉
```

### Enemy Button Density Hesaplaması:

```csharp
// Örnek: 100 duvar pozisyonu var
// Item button'lar 40 tanesini kullandı
// Kalan: 60 pozisyon

int remainingWallPositions = 60;

// Random density seç (%5-%15 arası)
float enemyDensityPercent = Random.Range(5f, 15f); // Örn: %10

// Enemy button sayısı hesapla
int targetEnemyButtonCount = 60 * 0.10 = 6 enemy button
```

## 🎮 Kurulum Adımları

### Adım 1: EnemyPool Asset Oluştur

1. **Resources/Enemies/ klasöründe:**
   - Sağ tık > Create > The Button > Enemy Pool
   - İsim: `DefaultEnemyPool.asset`

2. **EnemyPool ayarları:**
   ```
   Available Enemies:
   ├─ BasicEnemy (EnemyData asset)
   ├─ FastEnemy (EnemyData asset)
   └─ TankEnemy (EnemyData asset)
   
   Spawn Weights (Opsiyonel):
   ├─ 50 (BasicEnemy - %50 şans)
   ├─ 30 (FastEnemy - %30 şans)
   └─ 20 (TankEnemy - %20 şans)
   ```

### Adım 2: Enemy Button Prefab Oluştur

1. **WallCubeWithButton prefab'ını kopyala:**
   - `Assets/Prefabs/WallCubeWithButton.prefab`
   - Kopyala ve yeniden adlandır: `WallCubeWithEnemyButton.prefab`

2. **Button component'ini değiştir:**
   - Eski: `SpawnButton` component'ini sil
   - Yeni: `EnemySpawnButton` component'ini ekle

3. **Renk ayarları (opsiyonel):**
   ```
   Normal Color: Orange (1, 0.5, 0)
   Cooldown Color: Red
   Pressed Color: Yellow
   ```

4. **NetworkObject kontrolü:**
   - ✅ NetworkObject component var mı?
   - ✅ Is Spawnable = true

5. **DefaultNetworkPrefabs'a ekle:**
   - `Assets/DefaultNetworkPrefabs.asset`
   - Network Prefabs List'e ekle

### Adım 3: RoomConfiguration Ayarları

1. **RoomConfiguration asset'ini aç:**
   - `Assets/Resources/RoomConfiguration.asset`

2. **Enemy ayarlarını yap:**
   ```
   Enemy Spawn Button:
   ├─ Wall Cube With Enemy Button Prefab: WallCubeWithEnemyButton
   ├─ Min Enemy Button Density Percent: 5
   └─ Max Enemy Button Density Percent: 15
   
   Enemies:
   └─ Enemy Pool: DefaultEnemyPool
   ```

### Adım 4: Test Et!

1. **Oyunu başlat** (Host)
2. **Oda oluşturulacak**
3. **Duvarları kontrol et:**
   - ✅ Yeşil button'lar = Item spawn
   - ✅ Turuncu button'lar = Enemy spawn
4. **Enemy button'a bas**
5. **Enemy spawn olmalı!** 🎮

## 📊 Density Örnekleri

### Örnek 1: Küçük Oda (10x10)

```
Total wall positions: 288

Item Buttons:
- Density: %30 → 86 button
- Required items: 5
- Random items: 81

Remaining positions: 202

Enemy Buttons:
- Density: %10 → 20 button
- Enemies: 20 random enemy

Final:
- Item buttons: 86
- Enemy buttons: 20
- Plain walls: 182
```

### Örnek 2: Büyük Oda (20x20)

```
Total wall positions: 1152

Item Buttons:
- Density: %40 → 460 button

Remaining positions: 692

Enemy Buttons:
- Density: %15 → 103 button

Final:
- Item buttons: 460
- Enemy buttons: 103
- Plain walls: 589
```

## 🎨 Visual Farklar

| Button Tipi | Renk | Script | Spawn Eder |
|-------------|------|--------|------------|
| **Item Button** | 🟢 Green | SpawnButton | ItemData |
| **Enemy Button** | 🟠 Orange | EnemySpawnButton | EnemyData |

## 🔧 Ayarlar ve Özelleştirme

### Density Ayarları

**Item Button Density:**
```csharp
minButtonDensityPercent = 20f;  // Minimum %20
maxButtonDensityPercent = 50f;  // Maximum %50
```

**Enemy Button Density:**
```csharp
minEnemyButtonDensityPercent = 5f;   // Minimum %5
maxEnemyButtonDensityPercent = 15f;  // Maximum %15
```

### Weighted Spawn (Ağırlıklı Spawn)

Enemy'lerin spawn şansını ayarlamak için:

```
EnemyPool:
├─ Available Enemies: [BasicEnemy, FastEnemy, TankEnemy]
└─ Spawn Weights: [60, 30, 10]

Sonuç:
- BasicEnemy: %60 şans
- FastEnemy: %30 şans
- TankEnemy: %10 şans
```

**Not:** Spawn Weights boş bırakılırsa, tüm enemy'ler eşit şansa sahip olur.

### Enemy Button Cooldown

Her button'un kendi cooldown'u var:

```csharp
EnemySpawnButton:
└─ Cooldown Time: 10 saniye (ayarlanabilir)
```

## 🐛 Sorun Giderme

### Enemy Button Spawn Olmuyor

**Kontrol Et:**
1. ✅ `wallCubeWithEnemyButtonPrefab` atanmış mı?
2. ✅ `enemyPool` atanmış mı?
3. ✅ EnemyPool'da enemy var mı?
4. ✅ Density %0'dan büyük mü?
5. ✅ Kalan duvar pozisyonu var mı?

### Tüm Duvarlar Item Button Oldu

**Çözüm:**
- Item button density'yi düşür
- Örnek: Max %50 yerine %30 yap
- Böylece enemy button'lar için yer kalır

### Enemy Button Çok Az

**Çözüm:**
- Enemy button density'yi artır
- Örnek: Max %15 yerine %25 yap

### Hep Aynı Enemy Spawn Oluyor

**Çözüm:**
- EnemyPool'a daha fazla enemy ekle
- Veya spawn weights kullan

## 💡 İpuçları

### 1. Dengeli Density Ayarları

```
Kolay Oyun:
- Item buttons: %40-60
- Enemy buttons: %5-10

Orta Oyun:
- Item buttons: %30-40
- Enemy buttons: %10-15

Zor Oyun:
- Item buttons: %20-30
- Enemy buttons: %15-25
```

### 2. Enemy Çeşitliliği

En az 3-4 farklı enemy tipi kullan:
- Basic (normal)
- Fast (hızlı, düşük can)
- Tank (yavaş, yüksek can)
- Ranged (uzaktan saldırı)

### 3. Spawn Weights Kullanımı

```
Başlangıç Alanı:
- BasicEnemy: 70%
- FastEnemy: 20%
- TankEnemy: 10%

Son Bölge:
- BasicEnemy: 30%
- FastEnemy: 30%
- TankEnemy: 40%
```

### 4. Button Renkleri

Farklı renk kullanarak oyuncuya ipucu ver:
- 🟢 Yeşil = Item (yardımcı)
- 🟠 Turuncu = Enemy (tehlikeli)
- 🔵 Mavi = Puzzle (opsiyonel)

## 📝 Checklist

Room'a enemy button sistemi eklemek için:

- [ ] EnemyData asset'leri oluşturuldu
- [ ] EnemyPool asset oluşturuldu
- [ ] EnemyPool'a enemy'ler eklendi
- [ ] WallCubeWithEnemyButton prefab oluşturuldu
- [ ] EnemySpawnButton component eklendi
- [ ] Prefab DefaultNetworkPrefabs'a eklendi
- [ ] RoomConfiguration'a prefab atandı
- [ ] RoomConfiguration'a EnemyPool atandı
- [ ] Density ayarları yapıldı
- [ ] Oyunda test edildi
- [ ] Enemy button'lar spawn oluyor
- [ ] Enemy'ler spawn oluyor
- [ ] Cooldown çalışıyor

## 🎉 Sonuç

Artık prosedürel odalarınızda:

✅ **Item spawn button'ları** (yeşil)
✅ **Enemy spawn button'ları** (turuncu)
✅ **Plain wall'lar** (button yok)
✅ **Random density** (her oyun farklı)
✅ **Weighted spawn** (enemy çeşitliliği)

Hepsi otomatik ve network synchronized! 🚀

## 🔗 İlgili Dosyalar

- `Assets/Scripts/Enemy/EnemyPool.cs` - Enemy havuzu
- `Assets/Scripts/Game/RoomConfiguration.cs` - Oda ayarları
- `Assets/Scripts/Game/ProceduralRoomGenerator.cs` - Oda oluşturma
- `Assets/Scripts/Interactables/EnemySpawnButton.cs` - Enemy button
- `ENEMY_SPAWN_BUTTON_KILAVUZU.md` - Enemy button detaylı kılavuz
- `ENEMY_SPAWN_SYSTEM_SUMMARY.md` - Enemy sistem özeti

İyi oyunlar! 🎮

