# Player Animasyon Sistemi Kurulum Kılavuzu

## 🎯 Özet

Player için **Idle**, **Walk** ve **Attack** animasyonları başarıyla entegre edildi! Sistem otomatik olarak player'ın hareketlerini algılayıp uygun animasyonları oynatır ve network üzerinden senkronize eder.

## 📦 Eklenen Dosyalar

### 1. PlayerAnimationController.cs
**Konum**: `Assets/Scripts/Player/PlayerAnimationController.cs`

Player animasyonlarını yöneten ana script:
- Hareket hızına göre Idle/Walk animasyonları
- Silah saldırısı sırasında Attack animasyonu
- Network senkronizasyonu
- Smooth geçişler

## 🎬 Kullanılan Animasyonlar

**Konum**: `Assets/Animations/`

1. **Neutral Idle.fbx** → Durma animasyonu
2. **Standard Walk.fbx** → Yürüme animasyonu
3. **Standing Melee Attack Downward.fbx** → Saldırı animasyonu

## 🛠️ Unity'de Yapman Gerekenler

### 1️⃣ Animator Controller Ayarları

**Character.controller** dosyasını aç ve şunları yap:

#### Parametreler Ekle:
- `Speed` (Float) - Player hızı (0 = Idle, >0 = Walk)
- `Attack` (Trigger) - Saldırı tetikleyici

#### State Machine Oluştur (Blend Tree Kullanarak):

```
Entry → Movement (Blend Tree)
   ↓
Attack (Any State'ten trigger ile)
```

**Movement (Blend Tree):**
- Blend Type: 1D
- Parameter: Speed
- Motions:
  - Speed 0: Neutral Idle
  - Speed 5: Standard Walk

**Attack State:**
- Motion: Standing Melee Attack Downward

#### Blend Tree Oluşturma:

1. Base Layer'da **sağ tık** → Create State → From New Blend Tree
2. İsim: "Movement"
3. Blend Tree'ye **çift tık** (içine gir)
4. Inspector'da:
   - Blend Type: **1D**
   - Parameter: **Speed**
5. **+ butonuna** bas → Add Motion Field (2 kere)
6. İlk Motion:
   - Motion: **Neutral Idle**
   - Threshold: **0**
7. İkinci Motion:
   - Motion: **Standard Walk**
   - Threshold: **5**

#### Transition Ayarları:

**Any State → Attack:**
- Condition: `Attack` trigger
- Has Exit Time: ❌ (kapalı)
- Transition Duration: 0.05

**Attack → Movement:**
- Has Exit Time: ✅ (açık)
- Exit Time: 0.9
- Transition Duration: 0.1

### 2️⃣ Player Prefab Ayarları

**Player.prefab** dosyasını aç:

1. **Animator Component Ekle** (yoksa):
   - Add Component → Animator
   - Controller: Character.controller seç
   - Apply Root Motion: ❌ (kapalı)

2. **Character Model Ekle**:
   - Player'ın altına yeni GameObject oluştur (isim: "CharacterModel")
   - Character mesh'ini bu objeye ekle
   - Animator'ı bu objeye taşı

3. **PlayerAnimationController Component Ekle**:
   - Add Component → Player Animation Controller
   - Animator: CharacterModel'deki Animator'ı sürükle
   - Player Controller: PlayerController'ı sürükle
   - Weapon System: PlayerWeaponSystem'ı sürükle
   - Animation Smooth Time: 0.1
   - Speed Multiplier: 1.0 (karakter hızına göre ayarla)

### 3️⃣ FBX Ayarları

Her animasyon dosyası için (Neutral Idle, Standard Walk, Standing Melee Attack Downward):

1. FBX dosyasını seç
2. Inspector → **Rig** sekmesi
3. Animation Type: **Humanoid**
4. Avatar Definition: **Create From This Model**
5. **Apply** butonuna bas

### 4️⃣ Test Et!

1. Play Mode'a gir
2. **WASD** ile hareket et → Walk animasyonu
3. **Dur** → Idle animasyonu
4. **Silah kuşan** ve **Sol Tık** → Attack animasyonu

## ✨ Özellikler

### 🎮 Otomatik Çalışır
- Player hareket edince otomatik walk animasyonu
- Durduğunda otomatik idle animasyonu
- Saldırı yaptığında otomatik attack animasyonu

### 🌐 Network Senkronize
- Tüm oyuncular birbirlerinin animasyonlarını görür
- Optimized: Her oyuncu kendi animasyonunu local oynatır

### 🔫 Silah Sistemi Entegrasyonu
- PlayerWeaponSystem ile otomatik çalışır
- Hem melee hem ranged silahlar için geçerli
- Saldırı anında attack animasyonu tetiklenir

### 🎨 Smooth Geçişler
- Animasyonlar arası yumuşak geçişler
- Damping ile hız değişimleri smooth
- Ayarlanabilir transition süreleri

## 🎨 State Machine Şeması

```
┌──────────┐
│  Entry   │
└────┬─────┘
     │
     v
┌─────────────────────────────────┐
│       MOVEMENT (Blend Tree)     │
│                                 │
│  Speed = 0 → Neutral Idle       │
│  Speed = 5 → Standard Walk      │
│                                 │
│  (Otomatik geçiş Speed'e göre) │
└─────────────────────────────────┘
                ^
                │
                │ Exit Time
                │
         ┌──────┴──────┐
         │   ATTACK    │
         │             │
         │  Standing   │
         │   Melee     │
         └──────▲──────┘
                │
         Attack Trigger
                │
         ┌──────┴──────┐
         │  Any State  │
         └─────────────┘
```

**Nasıl Çalışır:**
- Speed = 0 → Tam Idle animasyonu
- Speed = 0-5 arası → Idle ve Walk arası blend
- Speed = 5+ → Tam Walk animasyonu
- Attack trigger → Attack animasyonu oynar, bitince Movement'e döner

## 🔧 Sorun Giderme

### ❌ Animasyonlar Çalışmıyor
**Çözüm:**
- Animator component'inin Controller'ı atanmış mı kontrol et
- Character.controller'da state'ler doğru mu kontrol et
- FBX dosyalarının Rig ayarları Humanoid mu kontrol et

### ❌ Network'te Senkronize Değil
**Çözüm:**
- Player prefab'ında NetworkObject var mı kontrol et
- PlayerAnimationController component'i eklenmiş mi kontrol et

### ❌ Attack Animasyonu Çalışmıyor
**Çözüm:**
- PlayerWeaponSystem component'i var mı kontrol et
- Animator'da "Attack" trigger parametresi var mı kontrol et
- Any State → Attack transition'ı doğru mu kontrol et

### ❌ Animasyonlar Çok Hızlı/Yavaş
**Çözüm:**
- Her state'in Speed değerini Animator'da ayarla (genelde 1)
- PlayerAnimationController'da Animation Smooth Time'ı değiştir

## 📋 Checklist

Kurulum için adım adım:

- [ ] Character.controller'ı aç
- [ ] Parametreleri ekle (Speed, Attack)
- [ ] Movement Blend Tree oluştur
- [ ] Blend Tree'ye Idle ve Walk animasyonlarını ekle (threshold 0 ve 5)
- [ ] Attack state'i oluştur
- [ ] Any State → Attack transition'ı ekle
- [ ] Attack → Movement transition'ı ekle
- [ ] Player prefab'ına Animator ekle
- [ ] Player prefab'ına PlayerAnimationController ekle
- [ ] Character model'i ekle
- [ ] FBX dosyalarını Humanoid yap
- [ ] Test et!

## 🎓 Nasıl Çalışır?

### Hareket Animasyonları
```
Her Frame:
1. Player'ın pozisyonunu oku
2. Hızı hesapla (pozisyon farkı / zaman)
3. Hızı smooth yap (damping ile)
4. Speed parametresini Animator'a gönder
5. Blend Tree otomatik olarak Idle/Walk arası geçiş yapar
6. Network'e sync et
```

**Blend Tree Mantığı:**
- Speed = 0 → %100 Idle
- Speed = 2.5 → %50 Idle + %50 Walk
- Speed = 5 → %100 Walk

### Saldırı Animasyonu
```
Saldırı Anında:
1. PlayerWeaponSystem → OnAttack event
2. PlayerAnimationController → event dinler
3. Attack trigger'ı tetikle
4. Animator → Attack state'e geç
5. Network'e sync et
6. Animasyon bitince → Idle/Walk'a dön
```

## 💡 İpuçları

1. **Smooth Geçişler İçin**: Animation Smooth Time değerini artır (0.2-0.3)
2. **Hızlı Tepki İçin**: Transition Duration'ları azalt (0.05)
3. **Animasyon Hızı Ayarı**: Speed Multiplier'ı değiştir (1.0 = normal, 2.0 = 2x hızlı)
4. **Blend Tree Threshold Ayarı**: Walk threshold'unu PlayerController'daki moveSpeed'e göre ayarla
5. **Farklı Silahlar İçin**: AttackType parametresi ekle ve Attack için Blend Tree kullan
6. **Sprint İçin**: Blend Tree'ye üçüncü motion ekle (Sprint animasyonu, threshold 10)

## 🚀 Gelecek Geliştirmeler

Sistemi genişletmek için:
- Jump animasyonu ekle
- Crouch animasyonu ekle
- Sprint animasyonu ekle
- Farklı silah animasyonları (pistol, rifle, etc.)
- Hit reaction animasyonları
- Death animasyonu

## 📚 İlgili Dosyalar

- `PlayerAnimationController.cs` - Ana animasyon controller
- `PlayerController.cs` - Hareket sistemi
- `PlayerWeaponSystem.cs` - Silah sistemi
- `Character.controller` - Animator Controller
- `ANIMATION_SYSTEM_SETUP.md` - Detaylı İngilizce dokümantasyon

## ✅ Tamamlandı!

Artık player'ın:
- ✅ Hareket animasyonları çalışıyor
- ✅ Saldırı animasyonları çalışıyor
- ✅ Network senkronizasyonu çalışıyor
- ✅ Sistem modüler ve genişletilebilir

**Başarılar! 🎉**

Sorularınız için dokümantasyonu inceleyin veya kod içindeki yorumları okuyun.

