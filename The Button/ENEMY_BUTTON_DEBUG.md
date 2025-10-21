# Enemy Button Debug Kılavuzu

## 🐛 Sorun: Enemy Button'lar Spawn Olmuyor

### Debug Log'ları Eklendi

Sisteme detaylı debug log'ları eklendi. Artık Console'da şunları göreceksin:

## 📋 Debug Log Sırası

Oyunu başlattığında Console'da şu log'ları ara:

### 1. Enemy Button Generation Başlangıcı
```
[DEBUG] GenerateEnemySpawnButtons called!
```
✅ Bu log **VARSA** → Sistem çalışıyor  
❌ Bu log **YOKSA** → GenerateEnemySpawnButtons metodu hiç çağrılmıyor

### 2. Prefab Kontrolü
```
[DEBUG] Enemy button prefab assigned: WallCubeWithEnemyButton
```
✅ Bu log **VARSA** → Prefab atanmış  
❌ Bu log yerine **"Enemy spawn button prefab not assigned"** → Prefab atanmamış

### 3. Enemy Pool Kontrolü
```
[DEBUG] Enemy pool assigned: EnemyPool
```
✅ Bu log **VARSA** → Pool atanmış  
❌ Bu log yerine **"Enemy pool not assigned"** → Pool atanmamış

### 4. Enemy Pool Validation
```
[EnemyPool] Validating pool: EnemyPool
[EnemyPool] Found 1 enemies in pool
[EnemyPool] Enemy 0: Basic Enemy
[EnemyPool] Enemy 0 prefab: BasicEnemy
[EnemyPool] Validation PASSED!
```
✅ Bu log **VARSA** → Pool geçerli  
❌ **"Enemy pool validation FAILED!"** → Pool'da sorun var

### 5. Remaining Wall Positions
```
[DEBUG] Remaining wall positions: 150
```
✅ **Sayı > 0** → Duvar pozisyonu var  
❌ **Sayı = 0** → Tüm duvarlar kullanılmış!

### 6. Enemy Button Spawn
```
Enemy Button Density: 10.5% (15 enemy buttons out of 150 remaining positions)
Spawned 15 enemy spawn buttons
```
✅ Bu log **VARSA** → Enemy button'lar spawn oldu!

---

## 🔍 Olası Sorunlar ve Çözümler

### Sorun 1: "Enemy pool validation FAILED!"

**Sebep:** EnemyPool'daki enemy'lerin prefab'ı atanmamış.

**Çözüm:**
1. Unity'de `NewEnemy` asset'ini aç
2. **Enemy Prefab** alanına bir prefab ata
3. Prefab'ın içinde:
   - ✅ NetworkObject olmalı
   - ✅ EnemyHealth olmalı
   - ✅ EnemyAI olmalı
4. Kaydet

### Sorun 2: "Remaining wall positions: 0"

**Sebep:** Item button'lar tüm duvarları kullanmış.

**Çözüm:**
1. RoomConfiguration'ı aç
2. **Button Density** ayarlarını düşür:
   ```
   Min Button Density Percent: 15 → 10
   Max Button Density Percent: 30 → 25
   ```
3. Veya oda boyutunu büyüt:
   ```
   Room Width: 10 → 15
   Room Depth: 10 → 15
   ```

### Sorun 3: Enemy button spawn sayısı çok az

**Sebep:** Enemy button density çok düşük veya kalan pozisyon az.

**Çözüm:**
1. RoomConfiguration'ı aç
2. **Enemy Button Density** ayarlarını artır:
   ```
   Min Enemy Button Density Percent: 5 → 10
   Max Enemy Button Density Percent: 15 → 25
   ```

### Sorun 4: Console'da hiçbir log yok

**Sebep:** `showDebugLogs` kapalı olabilir.

**Çözüm:**
1. Hierarchy'de `ProceduralRoomGenerator` objesini bul
2. Inspector'da **Show Debug Logs** ✅ işaretle

---

## 📊 Örnek Debug Output (Başarılı)

```
[RoomGenerator] Starting room generation with seed: 123456789
[RoomGenerator] Generating floor and ceiling...
[RoomGenerator] Placing events...
[RoomGenerator] Generating walls with buttons...
[RoomGenerator] Button Density: 25.3% (73 buttons out of 288 wall positions)

[RoomGenerator] Generating enemy spawn buttons...
[DEBUG] GenerateEnemySpawnButtons called!
[DEBUG] Enemy button prefab assigned: WallCubeWithEnemyButton
[DEBUG] Enemy pool assigned: EnemyPool
[EnemyPool] Validating pool: EnemyPool
[EnemyPool] Found 1 enemies in pool
[EnemyPool] Enemy 0: Basic Enemy
[EnemyPool] Enemy 0 prefab: BasicEnemy
[EnemyPool] Validation PASSED!
[DEBUG] Enemy pool validation PASSED!
[DEBUG] Remaining wall positions: 215
Enemy Button Density: 12.7% (27 enemy buttons out of 215 remaining positions)
Spawned 27 enemy spawn buttons

[RoomGenerator] Creating ceiling spawn point...
[RoomGenerator] Room generation complete!
```

---

## 📊 Örnek Debug Output (Hatalı - Prefab Yok)

```
[RoomGenerator] Generating enemy spawn buttons...
[DEBUG] GenerateEnemySpawnButtons called!
[DEBUG] Enemy button prefab assigned: WallCubeWithEnemyButton
[DEBUG] Enemy pool assigned: EnemyPool
[EnemyPool] Validating pool: EnemyPool
[EnemyPool] Found 1 enemies in pool
[EnemyPool] Enemy 0: Basic Enemy
[EnemyPool] Enemy 0 has no prefab assigned!  ← SORUN BURADA!
[RoomGenerator] Enemy pool validation failed!
[DEBUG] Enemy pool validation FAILED!
```

**Çözüm:** NewEnemy asset'ine prefab ata!

---

## 📊 Örnek Debug Output (Hatalı - Yer Yok)

```
[RoomGenerator] Generating enemy spawn buttons...
[DEBUG] GenerateEnemySpawnButtons called!
[DEBUG] Enemy button prefab assigned: WallCubeWithEnemyButton
[DEBUG] Enemy pool assigned: EnemyPool
[EnemyPool] Validating pool: EnemyPool
[EnemyPool] Found 1 enemies in pool
[EnemyPool] Enemy 0: Basic Enemy
[EnemyPool] Enemy 0 prefab: BasicEnemy
[EnemyPool] Validation PASSED!
[DEBUG] Enemy pool validation PASSED!
[DEBUG] Remaining wall positions: 0  ← SORUN BURADA!
[DEBUG] No available wall positions for enemy buttons.
```

**Çözüm:** Item button density'yi düşür veya oda boyutunu büyüt!

---

## ✅ Checklist - Sorun Giderme

Sırayla kontrol et:

1. **Console'u aç** (Unity > Window > General > Console)
2. **Oyunu başlat** (Play)
3. **Host olarak başla** (Start Host)
4. **Console'da ara:**
   - [ ] `[DEBUG] GenerateEnemySpawnButtons called!` var mı?
   - [ ] `[DEBUG] Enemy button prefab assigned` var mı?
   - [ ] `[DEBUG] Enemy pool assigned` var mı?
   - [ ] `[EnemyPool] Validation PASSED!` var mı?
   - [ ] `[DEBUG] Remaining wall positions: X` → X > 0 mı?
   - [ ] `Spawned X enemy spawn buttons` var mı?

5. **Eğer bir adımda takılıyorsan:**
   - O adımın çözümüne bak (yukarıda)
   - Düzelt
   - Oyunu tekrar başlat

---

## 🎯 Hızlı Çözüm (En Yaygın Sorun)

**%90 ihtimalle sorun şu:**

### NewEnemy asset'inde prefab atanmamış!

**Çözüm (30 saniye):**

1. **Enemy Prefab Oluştur:**
   ```
   Hierarchy > Create > 3D Object > Sphere
   → Add Component > Network Object
   → Add Component > Enemy Health
   → Add Component > Enemy AI
   → Sürükle Assets/Prefabs/ klasörüne
   → İsim: BasicEnemy
   ```

2. **DefaultNetworkPrefabs'a Ekle:**
   ```
   Assets/DefaultNetworkPrefabs.asset
   → Network Prefabs List > + butonu
   → BasicEnemy prefab'ını sürükle
   ```

3. **NewEnemy'ye Ata:**
   ```
   Assets/Resources/Enemies/NewEnemy.asset
   → Enemy Prefab: BasicEnemy'yi sürükle
   → Kaydet (Ctrl+S)
   ```

4. **Test Et:**
   ```
   Play → Start Host → Enemy button'lar spawn olmalı!
   ```

---

## 🔧 Debug Log'ları Kaldırma

Test bittikten sonra debug log'larını kaldırmak için:

1. `ProceduralRoomGenerator.cs` aç
2. `[DEBUG]` içeren tüm satırları sil
3. `EnemyPool.cs` aç
4. `Debug.Log` satırlarını sil veya yorum yap

Veya sadece `showDebugLogs = false` yap.

---

## 📞 Hala Çalışmıyor mu?

Console'daki **TAM** log'ları paylaş:
- `[DEBUG]` ile başlayan tüm satırlar
- `[EnemyPool]` ile başlayan tüm satırlar
- Herhangi bir **Warning** veya **Error**

---

## 🎉 Başarılı Olduğunda

Console'da şunu görmelisin:

```
Spawned 15 enemy spawn buttons  ← Bu satırı gör!
```

Ve oyunda:
- 🟢 Yeşil button'lar (item)
- 🟠 Turuncu button'lar (enemy)
- ⬜ Sade duvarlar

Turuncu button'a bas → Enemy spawn olur! 🎮


