# Enemy Spawn Button Sistemi - Kurulum Kılavuzu

## 📋 Genel Bakış

Enemy Spawn Button sistemi, ItemData'daki SpawnButton'a benzer şekilde çalışır. Butona basıldığında network üzerinden enemy spawn eder.

## 🎯 Sistem Bileşenleri

### 1. EnemyData (ScriptableObject)
Enemy özelliklerini tanımlar:
- Enemy adı ve açıklaması
- Enemy prefab referansı
- Health, speed, damage gibi statlar
- Detection ve attack range'leri

### 2. EnemySpawnButton (NetworkBehaviour)
- Butona basıldığında enemy spawn eder
- Cooldown sistemi vardır
- Network synchronized
- EnemyData'dan stat bilgilerini alır

## 🚀 Hızlı Kurulum

### Adım 1: Enemy Prefab Hazırlama

1. **Sphere veya başka bir 3D obje oluştur**
2. **Gerekli component'leri ekle:**
   - `NetworkObject` (Add Component > Netcode > Network Object)
   - `EnemyHealth` script
   - `EnemyAI` script
   - `CharacterController` (otomatik eklenir)

3. **NetworkObject ayarları:**
   - ✅ Is Spawnable = true
   - ✅ Destroy With Scene = true (opsiyonel)

4. **Prefab olarak kaydet:**
   - Assets/Prefabs/ klasörüne sürükle
   - İsim: `BasicEnemy.prefab`

### Adım 2: Enemy Prefab'ı Network'e Kaydetme

1. **DefaultNetworkPrefabs asset'ini aç:**
   - Assets/DefaultNetworkPrefabs.asset

2. **Network Prefabs List'e ekle:**
   - Size'ı 1 artır
   - Yeni slota `BasicEnemy` prefab'ını sürükle

### Adım 3: EnemyData ScriptableObject Oluşturma

1. **Resources/Enemies/ klasöründe:**
   - Sağ tık > Create > The Button > Enemy Data

2. **EnemyData ayarları:**
   ```
   Enemy Name: Basic Enemy
   Description: A simple hostile enemy
   
   Enemy Prefab: BasicEnemy (prefab'ı sürükle)
   
   Max Health: 100
   Move Speed: 3.5
   Detection Range: 15
   Attack Range: 2
   Attack Damage: 10
   Attack Cooldown: 1.5
   ```

3. **Kaydet:**
   - İsim: `BasicEnemy.asset`
   - Konum: `Assets/Resources/Enemies/BasicEnemy.asset`

### Adım 4: Enemy Spawn Button Oluşturma

1. **Scene'de bir Cube oluştur:**
   - GameObject > 3D Object > Cube
   - İsim: `EnemySpawnButton`
   - Position: (0, 0.5, 0)
   - Scale: (1, 0.2, 1)

2. **Component'leri ekle:**
   - `NetworkObject`
   - `EnemySpawnButton` script

3. **EnemySpawnButton ayarları:**
   ```
   Enemy To Spawn: BasicEnemy (asset'i sürükle)
   Spawn Point: (boş bırakabilirsin, otomatik bulur)
   Cooldown Time: 10
   
   Button Renderer: Cube'un MeshRenderer'ını sürükle
   Normal Color: Orange (1, 0.5, 0)
   Cooldown Color: Red
   Pressed Color: Yellow
   ```

4. **NetworkObject ayarları:**
   - ✅ Is Spawnable = true

5. **Prefab olarak kaydet:**
   - Assets/Prefabs/ klasörüne sürükle

### Adım 5: Network'e Kaydetme

1. **DefaultNetworkPrefabs asset'ini aç**
2. **Network Prefabs List'e ekle:**
   - `EnemySpawnButton` prefab'ını ekle

### Adım 6: Spawn Point Oluşturma (Opsiyonel)

Enemy'lerin spawn olacağı nokta:

1. **Empty GameObject oluştur:**
   - GameObject > Create Empty
   - İsim: `EnemySpawnPoint`
   - Position: Button'ın önünde bir yere koy

2. **Tag ekle:**
   - Tag: `EnemySpawnPoint`
   - (Eğer yoksa: Add Tag... > + > EnemySpawnPoint)

> **Not:** Spawn Point belirtmezsen, button'ın kendi pozisyonunda spawn olur.

## 🎮 Kullanım

### Oyunda Test Etme

1. **Oyunu başlat** (Play)
2. **Host olarak başla** (Start Host)
3. **Button'a yaklaş** (E tuşu ile etkileşim)
4. **E tuşuna bas**
5. **Enemy spawn olmalı!**

### Beklenen Davranış

- ✅ Button turuncu renkte olmalı
- ✅ E tuşuna basınca enemy spawn olmalı
- ✅ Button kırmızıya dönmeli (cooldown)
- ✅ Enemy player'ı tespit edip kovalamaya başlamalı
- ✅ 10 saniye sonra button tekrar turuncu olmalı

## 🔧 Özelleştirme

### Farklı Enemy Tipleri

Farklı enemy tipleri için:

1. **Yeni EnemyData oluştur:**
   - Resources/Enemies/ > Create > Enemy Data
   - İsim: `FastEnemy.asset`

2. **Farklı statlar ver:**
   ```
   Max Health: 50
   Move Speed: 7.0  (daha hızlı!)
   Detection Range: 20
   Attack Range: 1.5
   Attack Damage: 5
   Attack Cooldown: 0.8
   ```

3. **Yeni button oluştur ve bu EnemyData'yı ata**

### Spawn Point Değiştirme

Runtime'da spawn point değiştirmek için:

```csharp
EnemySpawnButton button = GetComponent<EnemySpawnButton>();
button.SetEnemyData(myEnemyData);
```

### Movement Bounds Ayarlama

Enemy'lerin hareket alanını sınırlamak için:

```csharp
// Spawn edilen enemy'e eriş
EnemyAI enemyAI = spawnedEnemy.GetComponent<EnemyAI>();

// Hareket alanını ayarla
enemyAI.SetMovementBounds(
    center: new Vector3(0, 0, 0),
    size: new Vector3(20, 10, 20)
);
```

## 🐛 Sorun Giderme

### Enemy Spawn Olmuyor

**Kontrol Et:**
1. ✅ Enemy prefab'ında `NetworkObject` var mı?
2. ✅ Enemy prefab `DefaultNetworkPrefabs`'a eklenmiş mi?
3. ✅ Button'da `NetworkObject` var mı?
4. ✅ Button `DefaultNetworkPrefabs`'a eklenmiş mi?
5. ✅ EnemyData'da prefab atanmış mı?
6. ✅ Host olarak mı başlattın?

### Enemy Hareket Etmiyor

**Kontrol Et:**
1. ✅ `EnemyAI` script eklenmiş mi?
2. ✅ `CharacterController` component var mı?
3. ✅ Player'ın `PlayerNetwork` component'i var mı?
4. ✅ Detection Range yeterince büyük mü?

### Button Etkileşim Vermiyor

**Kontrol Et:**
1. ✅ `IInteractable` interface implement edilmiş mi?
2. ✅ Player'da `PlayerInteraction` script var mı?
3. ✅ Button'un collider'ı var mı?
4. ✅ Cooldown bitti mi?

### Spawn Point Bulunamıyor

**Çözüm 1:** Tag'i kontrol et
```
GameObject > Tag > EnemySpawnPoint
```

**Çözüm 2:** Manuel ata
```
Button > Spawn Point > Empty GameObject'i sürükle
```

**Çözüm 3:** Boş bırak (button pozisyonunda spawn olur)

## 📊 Network Senkronizasyonu

### Nasıl Çalışır?

1. **Client** butona basar
2. **ServerRpc** server'a istek gönderir
3. **Server** enemy'yi spawn eder
4. **Server** enemy'yi tüm client'lara senkronize eder
5. **ClientRpc** tüm client'larda visual feedback oynatır

### Network Variables

- `isOnCooldown`: Button cooldown durumu
- `cooldownEndTime`: Cooldown bitiş zamanı
- `enemyDataAssetName`: Enemy asset adı (client'lar için)

## 🎨 Visual Feedback

### Renk Sistemi

- **🟠 Turuncu (Normal):** Kullanılabilir
- **🔴 Kırmızı (Cooldown):** Bekleniyor
- **🟡 Sarı (Pressed):** Basıldı (0.2 saniye)

### Gizmos (Editor'de)

- **🔴 Kırmızı Küre:** Spawn point konumu
- **⚪ Beyaz Çizgi:** Button'dan spawn point'e

## 💡 İpuçları

1. **Enemy Prefab'ı basit tut:**
   - Sadece gerekli component'ler
   - Fazla mesh/texture kullanma

2. **Cooldown süresini ayarla:**
   - Çok kısa = spam
   - Çok uzun = sıkıcı
   - Önerilen: 5-15 saniye

3. **Spawn point'i stratejik koy:**
   - Player'ın arkasında değil
   - Görüş alanında
   - Engellerin arkasında değil

4. **Enemy stat'larını dengele:**
   - Hızlı enemy = düşük health
   - Yavaş enemy = yüksek health
   - Uzun menzil = düşük damage

## 🔗 İlgili Dosyalar

- `Assets/Scripts/Enemy/EnemyData.cs` - ScriptableObject tanımı
- `Assets/Scripts/Interactables/EnemySpawnButton.cs` - Button logic
- `Assets/Scripts/Enemy/EnemyHealth.cs` - Health sistemi
- `Assets/Scripts/Enemy/EnemyAI.cs` - AI sistemi

## 📝 Örnek Kullanım Senaryoları

### Senaryo 1: Wave Sistemi

Farklı button'lar farklı wave'ler için:

```
Button 1: 3 Basic Enemy (kolay)
Button 2: 5 Fast Enemy (orta)
Button 3: 2 Tank Enemy + 3 Fast Enemy (zor)
```

### Senaryo 2: Prosedürel Oda

Oda oluşturulurken otomatik button spawn:

```csharp
// Oda oluşturulurken
GameObject buttonObj = Instantiate(enemySpawnButtonPrefab);
EnemySpawnButton button = buttonObj.GetComponent<EnemySpawnButton>();
button.SetEnemyData(randomEnemyData);
```

### Senaryo 3: Boss Fight

Özel boss button:

```
Enemy Name: Boss
Max Health: 500
Move Speed: 2.0
Attack Damage: 30
Detection Range: 30
```

## ✅ Checklist

Kurulum tamamlandı mı?

- [ ] Enemy prefab oluşturuldu
- [ ] NetworkObject eklendi
- [ ] EnemyHealth ve EnemyAI scriptleri eklendi
- [ ] Prefab DefaultNetworkPrefabs'a eklendi
- [ ] EnemyData asset'i oluşturuldu
- [ ] Resources/Enemies/ klasörüne kaydedildi
- [ ] EnemySpawnButton oluşturuldu
- [ ] Button NetworkObject'e sahip
- [ ] Button DefaultNetworkPrefabs'a eklendi
- [ ] Spawn point oluşturuldu (opsiyonel)
- [ ] Oyunda test edildi
- [ ] Enemy spawn oluyor
- [ ] Enemy hareket ediyor
- [ ] Cooldown çalışıyor

## 🎉 Tamamlandı!

Artık enemy spawn sisteminiz hazır! İyi oyunlar! 🎮

