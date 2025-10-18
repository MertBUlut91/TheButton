# Player Animation System Setup Guide

## 📋 Genel Bakış

Player için Idle, Walk ve Attack animasyonları entegre edilmiştir. Bu sistem Unity Animator Controller kullanarak animasyonları yönetir ve network üzerinden senkronize eder.

## 🎬 Mevcut Animasyonlar

Assets/Animations klasöründe şu animasyonlar bulunmaktadır:

1. **Neutral Idle.fbx** - Idle (durma) animasyonu
2. **Standard Walk.fbx** - Yürüme animasyonu  
3. **Standing Melee Attack Downward.fbx** - Saldırı animasyonu

## 🎮 Sistem Bileşenleri

### 1. PlayerAnimationController.cs
Yeni oluşturulan script, player animasyonlarını yönetir:

- **Hareket Animasyonları**: Player'ın hızına göre Idle/Walk arasında geçiş yapar
- **Saldırı Animasyonları**: PlayerWeaponSystem ile entegre, saldırı anında tetiklenir
- **Network Senkronizasyonu**: Tüm animasyonlar network üzerinden senkronize edilir

### 2. Animator Controller Parametreleri

Animator Controller'da şu parametreler kullanılır:

| Parametre | Tip | Açıklama |
|-----------|-----|----------|
| `Speed` | Float | Player'ın anlık hızı (0 = Idle, >0 = Walk) |
| `Attack` | Trigger | Saldırı animasyonunu tetikler |

## 🔧 Unity'de Yapılması Gerekenler

### Adım 1: Animator Controller'ı Yapılandır

1. **Character.controller** dosyasını aç (Assets/Animations/Character.controller)

2. **Parameters** sekmesinde şu parametreleri ekle:
   - `Speed` (Float)
   - `Attack` (Trigger)

3. **Layers** sekmesinde animasyon state'lerini oluştur:

#### Base Layer (Using Blend Tree)

```
Entry → Movement (Blend Tree)
Any State → Attack
```

**State'ler:**
- **Movement (Blend Tree)**: Idle/Walk blend based on Speed
- **Attack**: Standing Melee Attack Downward animasyonu

#### Creating the Blend Tree:

1. Right-click in Base Layer → **Create State** → **From New Blend Tree**
2. Name it: **"Movement"**
3. Double-click the Movement state to enter the Blend Tree
4. In Inspector:
   - Blend Type: **1D**
   - Parameter: **Speed**
5. Click **+** button → **Add Motion Field** (twice)
6. First Motion:
   - Motion: **Neutral Idle**
   - Threshold: **0**
7. Second Motion:
   - Motion: **Standard Walk**
   - Threshold: **5**

**Transition'lar:**

**Any State → Attack:**
- Condition: `Attack` (trigger)
- Has Exit Time: false
- Transition Duration: 0.05s

**Attack → Movement:**
- Has Exit Time: true
- Exit Time: 0.9 (90% of animation complete)
- Transition Duration: 0.1s

### Adım 2: Player Prefab'ı Yapılandır

1. **Assets/Prefabs/Player.prefab** dosyasını aç

2. Player GameObject'ine **Animator** component'i ekle (yoksa):
   - Controller: Character.controller
   - Avatar: (FBX'lerden birinin Avatar'ını kullan)
   - Apply Root Motion: false (CharacterController kullandığımız için)

3. Player GameObject'ine **PlayerAnimationController** component'ini ekle:
   - Animator: Animator component'ini ata
   - Player Controller: PlayerController component'ini ata
   - Weapon System: PlayerWeaponSystem component'ini ata
   - Animation Smooth Time: 0.1
   - Speed Multiplier: 1.0 (adjust based on character speed)

4. **Character Model'i Ekle**:
   - Player prefab'ının altına yeni bir child GameObject ekle (isim: "CharacterModel")
   - Bu GameObject'e character mesh'ini ekle
   - Animator component'ini bu GameObject'e taşı (root'tan)
   - PlayerAnimationController'da Animator referansını güncelle

### Adım 3: Animator Avatar Ayarları

Eğer animasyonlar çalışmazsa:

1. Her FBX dosyasını seç (Neutral Idle, Standard Walk, Standing Melee Attack Downward)
2. Inspector'da **Rig** sekmesine git
3. Animation Type: Humanoid
4. Avatar Definition: Create From This Model
5. Apply

### Adım 4: Test Et

1. Play Mode'a gir
2. WASD ile hareket et → Walk animasyonu oynamalı
3. Durduğunda → Idle animasyonuna geçmeli
4. Silah kuşan ve sol tıkla → Attack animasyonu oynamalı

## 🎯 Özellikler

### Otomatik Hareket Algılama
- Player'ın pozisyonunu her frame takip eder
- Hızı hesaplar ve animasyonu buna göre ayarlar
- Smooth geçişler için damping kullanır

### Network Senkronizasyonu
- Tüm animasyonlar ServerRpc/ClientRpc ile senkronize edilir
- Her client diğer player'ların animasyonlarını görür
- Local player için optimizasyon (kendi animasyonunu tekrar almaz)

### Silah Sistemi Entegrasyonu
- PlayerWeaponSystem'ın OnAttack event'ine subscribe olur
- Saldırı anında otomatik olarak attack animasyonunu tetikler
- Hem melee hem ranged silahlar için çalışır

## 📊 Animasyon State Machine Diyagramı

```
┌─────────────┐
│    Entry    │
└──────┬──────┘
       │
       v
┌─────────────────────────────────┐
│    MOVEMENT (Blend Tree)        │
│                                 │
│  Speed = 0 → Neutral Idle       │
│  Speed = 5 → Standard Walk      │
│                                 │
│  (Auto blend based on Speed)    │
└─────────────────────────────────┘
              ^
              │
              │ Exit Time
              │
       ┌──────┴──────┐
       │   Attack    │
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

**How it works:**
- Speed = 0 → 100% Idle animation
- Speed = 0-5 → Blend between Idle and Walk
- Speed = 5+ → 100% Walk animation
- Attack trigger → Play attack animation, return to Movement when done

## 🔍 Troubleshooting

### Animasyonlar Çalışmıyor
- Animator component'inin Controller'ı atanmış mı kontrol et
- Avatar'ın doğru yapılandırıldığından emin ol
- Console'da hata var mı kontrol et

### Animasyonlar Senkronize Değil
- PlayerAnimationController component'inin NetworkBehaviour olduğundan emin ol
- Player prefab'ının NetworkObject component'i var mı kontrol et

### Attack Animasyonu Çalışmıyor
- PlayerWeaponSystem component'inin eklendiğinden emin ol
- Animator'da "Attack" trigger parametresi var mı kontrol et

### Animasyonlar Çok Hızlı/Yavaş
- Animator'da her state'in Speed parametresini ayarla
- PlayerAnimationController'da animationSmoothTime değerini değiştir

## 🎨 Gelişmiş Özellikler (İsteğe Bağlı)

### Blend Tree Kullanımı
Walk animasyonunu Speed parametresine göre blend edebilirsin:
- Walk → Blend Tree
- Blend Type: 1D
- Parameter: Speed
- Motions: Idle (0) → Walk (5)

### Farklı Attack Animasyonları
Farklı silahlar için farklı attack animasyonları:
1. Animator'a yeni parametre ekle: `AttackType` (Int)
2. Attack state'ini Blend Tree yap
3. PlayerWeaponSystem'da silah tipine göre parametreyi ayarla

### Sprint Animasyonu
Koşma animasyonu eklemek için:
1. Sprint animasyonu ekle
2. `IsSprinting` (Bool) parametresi ekle
3. Walk → Sprint transition'ı oluştur

## 📝 Kod Örnekleri

### Manuel Animasyon Tetikleme

```csharp
// PlayerAnimationController'a erişim
var animController = GetComponent<PlayerAnimationController>();

// Attack animasyonunu manuel tetikle
animController.PlayAttackAnimation();

// Walking state'ini manuel ayarla
animController.SetWalking(true);

// Mevcut hızı al
float speed = animController.GetCurrentSpeed();

// Player yürüyor mu kontrol et
bool isWalking = animController.IsPlayerWalking();
```

### Custom Event Dinleme

```csharp
// PlayerWeaponSystem'ın OnAttack event'ine subscribe ol
weaponSystem.OnAttack += (damage) => {
    Debug.Log($"Attack animation played! Damage: {damage}");
};
```

## ✅ Checklist

Player animasyon sistemini kurmak için:

- [ ] Character.controller'da parametreleri oluştur (Speed, Attack)
- [ ] Movement Blend Tree oluştur
- [ ] Blend Tree'ye Idle ve Walk animasyonlarını ekle (threshold 0 ve 5)
- [ ] Attack state'i oluştur
- [ ] Any State → Attack transition'ı ekle
- [ ] Attack → Movement transition'ı ekle
- [ ] Player prefab'ına Animator component'i ekle
- [ ] Player prefab'ına PlayerAnimationController component'i ekle
- [ ] Character model'i player'ın child'ı olarak ekle
- [ ] FBX dosyalarının Rig ayarlarını kontrol et (Humanoid)
- [ ] Test et ve ayarla

## 🎓 Sonuç

Bu sistem sayesinde:
- ✅ Player hareket ederken otomatik walk animasyonu oynar
- ✅ Durduğunda idle animasyonuna geçer
- ✅ Saldırı yaptığında attack animasyonu oynar
- ✅ Tüm animasyonlar network üzerinden senkronize edilir
- ✅ Sistem modüler ve genişletilebilir

Sorularınız için dokümantasyonu inceleyin veya kod içindeki yorumları okuyun!

