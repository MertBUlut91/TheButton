# Enemy System - Kılavuz

## 🎯 Enemy Sistemi Oluşturuldu!

Tam özellikli bir enemy sistemi hazır! Düşmanlar:
- ✅ Can sistemi (health)
- ✅ Oyuncuları tespit ediyor
- ✅ Takip ediyor (chase)
- ✅ Saldırıyor (attack)
- ✅ Ölüyor ve despawn oluyor
- ✅ Network senkronize
- ✅ NavMesh ile hareket ediyor

---

## 📁 Oluşturulan Dosyalar

### 1. EnemyHealth.cs
- Can sistemi
- Hasar alma
- Ölüm mekaniği
- Network senkronize

### 2. EnemyAI.cs
- Oyuncu tespiti
- Takip sistemi
- Saldırı sistemi
- State machine (Idle, Chase, Attack)

---

## 🚀 Enemy Oluşturma (Adım Adım)

### Adım 1: Basit Enemy Modeli Oluştur

1. **Hierarchy'de sağ tık** → 3D Object → **Capsule**
2. **İsim**: "Enemy"
3. **Transform**:
   ```
   Position: (0, 1, 5)  ← Oyuncunun önünde
   Rotation: (0, 0, 0)
   Scale: (1, 1, 1)
   ```

### Adım 2: NavMesh Agent Ekle

1. **Enemy'yi seç**
2. **Add Component** → Navigation → **Nav Mesh Agent**
3. **Ayarlar**:
   ```
   Speed: 3.5
   Angular Speed: 120
   Acceleration: 8
   Stopping Distance: 2
   ```

### Adım 3: Enemy Script'lerini Ekle

1. **Add Component** → Search: **EnemyHealth**
2. **Add Component** → Search: **EnemyAI**
3. **Ayarlar**:
   ```
   [EnemyHealth]
   ├─ Max Health: 100
   └─ Despawn Delay: 5
   
   [EnemyAI]
   ├─ Detection Range: 15
   ├─ Attack Range: 2
   ├─ Attack Damage: 10
   ├─ Attack Cooldown: 1.5
   ├─ Move Speed: 3.5
   └─ Rotation Speed: 5
   ```

### Adım 4: Collider Ayarla

1. **Enemy'nin Capsule Collider'ını kontrol et**
2. **Ayarlar**:
   ```
   Center: (0, 0, 0)
   Radius: 0.5
   Height: 2
   ```

### Adım 5: NetworkObject Ekle

1. **Add Component** → **Network Object**
2. **Ayarlar**:
   ```
   ☐ Is Player Object
   ☐ Destroy With Scene
   ```

### Adım 6: Prefab Yap

1. **Enemy'yi Project'e sürükle**
2. **Prefab oluştur**
3. **Hierarchy'den orijinali sil**

### Adım 7: NetworkPrefabs'a Ekle

1. **Project'te** `DefaultNetworkPrefabs.asset` dosyasını bul
2. **Aç**
3. **Prefabs List'e Enemy prefab'ını ekle**

---

## 🗺️ NavMesh Oluşturma

Enemy'ler hareket edebilmesi için NavMesh gerekli!

### Adım 1: Zemin Ayarla

1. **Zemin objesini seç** (Plane veya Floor)
2. **Inspector'da** → Static → **Navigation Static** işaretle

### Adım 2: NavMesh Bake Et

1. **Window** → AI → **Navigation**
2. **Bake sekmesine git**
3. **Ayarlar**:
   ```
   Agent Radius: 0.5
   Agent Height: 2
   Max Slope: 45
   Step Height: 0.4
   ```
4. **Bake** butonuna tıkla

### Adım 3: NavMesh Kontrol Et

- Scene view'da mavi alan görünmeli
- Bu alan enemy'lerin hareket edebileceği yer

---

## 🎮 Enemy Spawn Etme

### Yöntem 1: Manuel Spawn (Test İçin)

```csharp
// Server tarafında
if (IsServer)
{
    GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy");
    GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    enemy.GetComponent<NetworkObject>().Spawn();
}
```

### Yöntem 2: Spawn Script Oluştur

```csharp
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 10f;
    
    private void Start()
    {
        if (IsServer)
        {
            InvokeRepeating(nameof(SpawnEnemy), 2f, spawnInterval);
        }
    }
    
    private void SpawnEnemy()
    {
        if (!IsServer) return;
        
        // Random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.GetComponent<NetworkObject>().Spawn();
        
        Debug.Log($"[EnemySpawner] Spawned enemy at {spawnPoint.position}");
    }
}
```

---

## 🎨 Enemy Görselleştirme

### Basit Renk

1. **Material oluştur**
2. **Kırmızı renk ver**
3. **Enemy'ye ata**

### Health Bar Ekle

```csharp
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private EnemyHealth enemyHealth;
    
    private void Start()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged += UpdateHealthBar;
        }
    }
    
    private void UpdateHealthBar(float current, float max)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = current / max;
        }
    }
}
```

---

## ⚙️ Ayarlar ve Tuning

### Kolay Enemy
```
Max Health: 50
Detection Range: 10
Attack Range: 2
Attack Damage: 5
Move Speed: 2.5
Attack Cooldown: 2
```

### Normal Enemy
```
Max Health: 100
Detection Range: 15
Attack Range: 2
Attack Damage: 10
Move Speed: 3.5
Attack Cooldown: 1.5
```

### Zor Enemy
```
Max Health: 200
Detection Range: 20
Attack Range: 3
Attack Damage: 20
Move Speed: 5
Attack Cooldown: 1
```

---

## 🐛 Sorun Giderme

### Enemy Hareket Etmiyor

**Kontrol Et**:
- [ ] NavMesh bake edildi mi?
- [ ] NavMesh Agent var mı?
- [ ] Enemy NavMesh üzerinde mi?

**Çözüm**:
```csharp
// Console'da kontrol et
NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
Debug.Log($"Agent on NavMesh: {agent.isOnNavMesh}");
Debug.Log($"Agent enabled: {agent.enabled}");
```

### Enemy Oyuncuyu Görmüyor

**Kontrol Et**:
- [ ] Detection Range yeterli mi?
- [ ] Player Layer doğru mu?
- [ ] Oyuncu collider'ı var mı?

**Çözüm**:
```csharp
// EnemyAI'da detection range'i artır
detectionRange = 20f;
```

### Enemy Saldırmıyor

**Kontrol Et**:
- [ ] Attack Range içinde mi?
- [ ] Attack Cooldown geçti mi?
- [ ] PlayerNetwork komponenti var mı?

**Debug**:
```csharp
// Console'da
EnemyAI ai = enemy.GetComponent<EnemyAI>();
Debug.Log($"State: {ai.GetCurrentState()}");
Debug.Log($"Target: {ai.GetTarget()}");
```

### Enemy Ölmüyor

**Kontrol Et**:
- [ ] EnemyHealth komponenti var mı?
- [ ] Silah enemy'ye hasar veriyor mu?
- [ ] Console'da log var mı?

**Test**:
```csharp
// Console'da enemy'ye hasar ver
EnemyHealth health = enemy.GetComponent<EnemyHealth>();
health.TakeDamageServerRpc(50);
```

---

## 🎯 Test Senaryosu

1. **NavMesh oluştur**
2. **Enemy prefab oluştur**
3. **Enemy spawn et**
4. **Oyuncuya yaklaş**
5. **Enemy seni takip etmeli** ✓
6. **Yaklaşınca saldırmalı** ✓
7. **Silahla vur**
8. **Enemy hasar almalı** ✓
9. **Yeterince vur**
10. **Enemy ölmeli** ✓

---

## 📊 Sistem Özellikleri

### EnemyHealth
- ✅ Network senkronize can
- ✅ Hasar alma
- ✅ İyileşme
- ✅ Ölüm event'i
- ✅ Otomatik despawn
- ✅ Death effect

### EnemyAI
- ✅ Oyuncu tespiti
- ✅ En yakın oyuncuyu bulma
- ✅ NavMesh ile takip
- ✅ Saldırı sistemi
- ✅ State machine
- ✅ Gizmos (debug için)

### PlayerWeaponSystem (Güncellendi)
- ✅ Enemy'lere hasar verme
- ✅ Hem player hem enemy'ye çalışıyor
- ✅ Melee ve ranged destekli

---

## 💡 İleri Seviye Özellikler

### 1. Farklı Enemy Tipleri
- Tank: Yüksek can, yavaş
- Scout: Düşük can, hızlı
- Boss: Çok yüksek can, güçlü

### 2. Enemy Animasyonları
- Idle animasyonu
- Walk animasyonu
- Attack animasyonu
- Death animasyonu

### 3. Loot Sistemi
- Enemy öldüğünde item bırakma
- Random loot
- Rare items

### 4. Spawn Sistemi
- Wave sistemi
- Zorluk artışı
- Spawn noktaları

---

## 🎉 Özet

**Enemy sistemi hazır!**

**Yapman Gerekenler**:
1. ✅ NavMesh oluştur
2. ✅ Enemy prefab oluştur
3. ✅ Script'leri ekle
4. ✅ NetworkPrefabs'a ekle
5. ✅ Spawn et
6. ✅ Test et!

**Sonuç**:
- Enemy'ler oyuncuları takip ediyor ✓
- Saldırıyorlar ✓
- Hasar alıyorlar ✓
- Ölüyorlar ✓
- Network senkronize ✓

İyi oyunlar! 🎮

