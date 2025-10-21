# Enemy Spawn System - Özet

## 🎯 Ne Yaptık?

ItemData ve SpawnButton sistemine benzer şekilde, **EnemyData** ve **EnemySpawnButton** sistemi oluşturduk.

## 📦 Yeni Dosyalar

### 1. EnemyData.cs
**Konum:** `Assets/Scripts/Enemy/EnemyData.cs`

**Ne İşe Yarar:**
- Enemy türlerini tanımlar (ScriptableObject)
- Enemy prefab referansı tutar
- Enemy stat'larını saklar (health, speed, damage, etc.)

**Özellikler:**
```csharp
- enemyName: string
- description: string
- enemyPrefab: GameObject
- maxHealth: float
- moveSpeed: float
- detectionRange: float
- attackRange: float
- attackDamage: float
- attackCooldown: float
- icon: Sprite (optional)
```

### 2. EnemySpawnButton.cs
**Konum:** `Assets/Scripts/Interactables/EnemySpawnButton.cs`

**Ne İşe Yarar:**
- Butona basıldığında enemy spawn eder
- Network synchronized
- Cooldown sistemi
- Visual feedback (renk değişimi)

**Özellikler:**
- EnemyData referansı
- Spawn point
- Cooldown süresi
- Renk ayarları

### 3. EnemyHealth.cs - Güncellendi
**Eklenen Metod:**
```csharp
public void SetMaxHealth(float newMaxHealth)
```
- EnemySpawnButton tarafından çağrılır
- Spawn sırasında health'i ayarlar

### 4. EnemyAI.cs - Güncellendi
**Eklenen Metod:**
```csharp
public void SetStats(float speed, float detection, float attack, float damage, float cooldown)
```
- EnemySpawnButton tarafından çağrılır
- Spawn sırasında tüm stat'ları ayarlar

## 🔄 Sistem Akışı

### 1. Setup (Unity Editor'de)

```
1. Enemy Prefab Oluştur
   ├─ NetworkObject ekle
   ├─ EnemyHealth ekle
   ├─ EnemyAI ekle
   └─ DefaultNetworkPrefabs'a ekle

2. EnemyData Asset Oluştur
   ├─ Resources/Enemies/ klasöründe
   ├─ Enemy prefab'ı ata
   └─ Stat'ları ayarla

3. EnemySpawnButton Oluştur
   ├─ NetworkObject ekle
   ├─ EnemySpawnButton script ekle
   ├─ EnemyData'yı ata
   └─ DefaultNetworkPrefabs'a ekle
```

### 2. Runtime (Oyun sırasında)

```
Player Butona Basar
    ↓
[Client] Interact() çağrılır
    ↓
[Client → Server] PressButtonServerRpc()
    ↓
[Server] Cooldown kontrolü
    ↓
[Server] Enemy prefab'ı Instantiate
    ↓
[Server] EnemyHealth.SetMaxHealth()
    ↓
[Server] EnemyAI.SetStats()
    ↓
[Server] NetworkObject.Spawn()
    ↓
[Server → All Clients] Enemy senkronize edilir
    ↓
[Server → All Clients] PlayPressEffectClientRpc()
    ↓
[All Clients] Visual feedback (renk değişimi)
```

## 🎮 Kullanım Örnekleri

### Örnek 1: Basit Enemy Spawn

```csharp
// Unity Editor'de:
// 1. EnemyData oluştur (Resources/Enemies/BasicEnemy.asset)
// 2. EnemySpawnButton'a ata
// 3. Oyunu başlat, butona bas!
```

### Örnek 2: Runtime'da Enemy Data Değiştirme

```csharp
EnemySpawnButton button = GetComponent<EnemySpawnButton>();
EnemyData newEnemy = Resources.Load<EnemyData>("Enemies/FastEnemy");
button.SetEnemyData(newEnemy);
```

### Örnek 3: Prosedürel Oda ile Entegrasyon

```csharp
// Oda oluşturulurken random enemy button spawn et
GameObject buttonObj = Instantiate(enemySpawnButtonPrefab, position, rotation);
EnemySpawnButton button = buttonObj.GetComponent<EnemySpawnButton>();

// Random enemy seç
EnemyData[] allEnemies = Resources.LoadAll<EnemyData>("Enemies");
EnemyData randomEnemy = allEnemies[Random.Range(0, allEnemies.Length)];

button.SetEnemyData(randomEnemy);

// Network'e spawn et
NetworkObject netObj = buttonObj.GetComponent<NetworkObject>();
netObj.Spawn();
```

## 🔧 Teknik Detaylar

### Network Synchronization

**NetworkVariables:**
- `isOnCooldown`: bool - Button kullanılabilir mi?
- `cooldownEndTime`: float - Ne zaman kullanılabilir olacak?
- `enemyDataAssetName`: NetworkString - Hangi enemy spawn olacak?

**RPCs:**
- `PressButtonServerRpc()`: Client → Server, button basıldı
- `PlayPressEffectClientRpc()`: Server → All Clients, visual feedback

### Resource Loading

Enemy asset'leri `Resources/Enemies/` klasöründen yüklenir:

```csharp
// Server
enemyDataAssetName.Value = new NetworkString(enemyData.name);

// Client
enemyToSpawn = Resources.Load<EnemyData>($"Enemies/{assetName}");
```

Bu sayede client'lar server'dan asset adını alır ve kendi local'lerinden yükler.

## 🎨 Visual System

### Button Renkleri

| Durum | Renk | Anlamı |
|-------|------|--------|
| Normal | 🟠 Orange (1, 0.5, 0) | Kullanılabilir |
| Cooldown | 🔴 Red | Bekleniyor |
| Pressed | 🟡 Yellow | Basıldı (0.2s) |

### Gizmos (Editor)

- **Spawn Point:** Kırmızı küre
- **Button → Spawn Point:** Beyaz çizgi

## 📊 Karşılaştırma: SpawnButton vs EnemySpawnButton

| Özellik | SpawnButton | EnemySpawnButton |
|---------|-------------|------------------|
| **Spawn Eder** | ItemData | EnemyData |
| **Prefab Tipi** | Item prefab | Enemy prefab |
| **Stat Ayarlama** | ❌ Yok | ✅ SetStats() |
| **Health Ayarlama** | ❌ Yok | ✅ SetMaxHealth() |
| **Cooldown** | ✅ Var | ✅ Var |
| **Network Sync** | ✅ Var | ✅ Var |
| **Visual Feedback** | ✅ Var | ✅ Var |
| **Resource Folder** | Items/ | Enemies/ |
| **Default Color** | 🟢 Green | 🟠 Orange |

## 🐛 Bilinen Sınırlamalar

1. **Enemy prefab mutlaka NetworkObject içermeli**
   - Yoksa spawn olmaz
   - Console'da error verir

2. **EnemyData mutlaka Resources/Enemies/ klasöründe olmalı**
   - Client'lar bu klasörden yükler
   - Başka yerde olursa client'lar bulamaz

3. **Button mutlaka DefaultNetworkPrefabs'da olmalı**
   - Prosedürel olarak spawn edilecekse
   - Manuel scene'de varsa gerekli değil

## 💡 Gelecek Geliştirmeler

### Önerilen Eklemeler

1. **Enemy Pool Sistemi:**
   ```csharp
   // Sürekli Instantiate yerine pool kullan
   EnemyPool.Instance.SpawnEnemy(enemyData, position);
   ```

2. **Wave Sistemi:**
   ```csharp
   // Belirli sayıda enemy spawn et
   button.SetWaveSize(5); // 5 enemy spawn et
   ```

3. **Spawn Animasyonu:**
   ```csharp
   // Enemy spawn olurken efekt göster
   button.spawnEffect = particleEffectPrefab;
   ```

4. **Spawn Limiti:**
   ```csharp
   // Maksimum enemy sayısı
   button.maxActiveEnemies = 10;
   ```

5. **Spawn Pattern:**
   ```csharp
   // Çember şeklinde spawn
   button.spawnPattern = SpawnPattern.Circle;
   button.spawnRadius = 5f;
   ```

## 📝 Checklist - Yeni Enemy Eklemek İçin

- [ ] Enemy prefab oluştur
- [ ] NetworkObject ekle
- [ ] EnemyHealth ekle
- [ ] EnemyAI ekle
- [ ] CharacterController ekle (otomatik)
- [ ] Prefab'ı kaydet
- [ ] DefaultNetworkPrefabs'a ekle
- [ ] EnemyData asset oluştur (Resources/Enemies/)
- [ ] Enemy prefab'ı EnemyData'ya ata
- [ ] Stat'ları ayarla
- [ ] Button'a EnemyData'yı ata
- [ ] Test et!

## 🎉 Sonuç

Artık ItemData gibi EnemyData sisteminiz var! 

**Avantajlar:**
- ✅ Kolay enemy ekleme (sadece asset oluştur)
- ✅ Network synchronized
- ✅ Stat'lar merkezi bir yerden yönetiliyor
- ✅ Prosedürel oda sistemi ile uyumlu
- ✅ Cooldown sistemi
- ✅ Visual feedback

**Kullanım:**
1. EnemyData asset oluştur
2. Button'a ata
3. Oyunu başlat
4. Butona bas
5. Enemy spawn olur! 🎮

İyi oyunlar! 🚀


