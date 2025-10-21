# Enemy Sistemi - Hızlı Başlangıç 🎮

## ✅ Ne Hazırlandı?

Enemy sistemi tamamen hazır! 2 script oluşturuldu:

1. **EnemyHealth.cs** - Can sistemi
2. **EnemyAI.cs** - Yapay zeka (takip + saldırı)

---

## 🚀 5 Dakikada Enemy Oluştur

### 1. Enemy Objesi Oluştur (1 dk)

1. **Hierarchy** → Sağ tık → 3D Object → **Capsule**
2. **İsim**: "Enemy"
3. **Kırmızı material ver** (düşman olduğu belli olsun)

### 2. Script'leri Ekle (1 dk)

1. **Enemy'yi seç**
2. **Add Component** → **Nav Mesh Agent**
3. **Add Component** → **EnemyHealth**
4. **Add Component** → **EnemyAI**
5. **Add Component** → **Network Object**

### 3. NavMesh Oluştur (2 dk)

1. **Zemin objesini seç** (Floor/Plane)
2. **Inspector** → Static → **Navigation Static** ✓
3. **Window** → AI → **Navigation**
4. **Bake** butonuna tıkla

### 4. Prefab Yap (30 sn)

1. **Enemy'yi Project'e sürükle**
2. **Prefab oluştur**

### 5. NetworkPrefabs'a Ekle (30 sn)

1. **Project'te** `DefaultNetworkPrefabs.asset` bul
2. **Aç**
3. **Enemy prefab'ını ekle**

**Bitti!** ✅

---

## 🎮 Test Et

1. **Oyunu başlat**
2. **Enemy'ye yaklaş**
3. **Enemy seni takip etmeli** ✓
4. **Silahını donat** (1 tuşu)
5. **Enemy'ye saldır** (sol tık)
6. **Enemy hasar almalı** ✓
7. **Yeterince vur, ölmeli** ✓

---

## ⚙️ Ayarlar

### EnemyHealth (Inspector'da)
```
Max Health: 100        ← Enemy canı
Despawn Delay: 5       ← Ölünce 5 saniye sonra yok olur
```

### EnemyAI (Inspector'da)
```
Detection Range: 15    ← 15 metre içindeki oyuncuları görür
Attack Range: 2        ← 2 metre yakınsa saldırır
Attack Damage: 10      ← Saldırı hasarı
Attack Cooldown: 1.5   ← Saldırılar arası süre
Move Speed: 3.5        ← Hareket hızı
```

---

## 💡 İpuçları

### Kolay Enemy
```
Max Health: 50
Attack Damage: 5
Move Speed: 2.5
```

### Zor Enemy
```
Max Health: 200
Attack Damage: 20
Move Speed: 5
```

### Enemy Spawn Etme

Console'da:
```csharp
// Test için enemy spawn et
GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemy");
GameObject enemy = Instantiate(prefab, new Vector3(0, 1, 5), Quaternion.identity);
enemy.GetComponent<NetworkObject>().Spawn();
```

---

## 🐛 Sorun mu Var?

### Enemy Hareket Etmiyor
- NavMesh bake edildi mi?
- Enemy NavMesh üzerinde mi?

### Enemy Saldırmıyor
- Detection Range yeterli mi? (15 yap)
- Attack Range içinde misin? (2 metre yakın ol)

### Enemy Ölmüyor
- Console'da log var mı?
- Silah enemy'ye hasar veriyor mu?

---

## 🎯 Özet

**Yapman Gerekenler**:
1. ✅ Capsule oluştur
2. ✅ Script'leri ekle (EnemyHealth + EnemyAI + NavMeshAgent + NetworkObject)
3. ✅ NavMesh bake et
4. ✅ Prefab yap
5. ✅ NetworkPrefabs'a ekle
6. ✅ Test et!

**Sonuç**: Enemy'ler seni takip ediyor, saldırıyor, hasar alıyor ve ölüyor! 🎉

Detaylı bilgi için: **ENEMY_SYSTEM_GUIDE.md**

İyi oyunlar! 🎮


