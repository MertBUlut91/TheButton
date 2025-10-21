# Animation System - Quick Reference

## 🎯 Hızlı Başlangıç

### Unity'de Yapılacaklar (5 Dakika)

#### 1. Animator Controller Setup
```
Character.controller'ı aç:
├── Parameters
│   ├── Speed (Float)
│   └── Attack (Trigger)
└── States
    ├── Movement (Blend Tree) ← Entry
    │   ├── Idle (threshold: 0)
    │   └── Walk (threshold: 5)
    └── Attack
        └── Any State → Attack (trigger)
        └── Attack → Movement (exit time)
```

#### 2. Blend Tree Oluşturma
1. Base Layer → Sağ tık → **Create State** → **From New Blend Tree**
2. İsim: **Movement**
3. Çift tık (içine gir)
4. Inspector:
   - Blend Type: **1D**
   - Parameter: **Speed**
5. **+ → Add Motion Field** (2 kere)
   - Motion 1: **Neutral Idle** (Threshold: 0)
   - Motion 2: **Standard Walk** (Threshold: 5)

#### 3. Attack State
1. Base Layer → Sağ tık → **Create State** → **Empty**
2. İsim: **Attack**
3. Motion: **Standing Melee Attack Downward**
4. **Any State** → **Attack**:
   - Condition: Attack (trigger)
   - Has Exit Time: ❌
5. **Attack** → **Movement**:
   - Has Exit Time: ✅
   - Exit Time: 0.9

#### 4. Player Prefab
```
Player
├── Animator (Character.controller)
├── PlayerAnimationController
│   ├── Animator: ↑
│   ├── Player Controller: (auto)
│   ├── Weapon System: (auto)
│   ├── Animation Smooth Time: 0.1
│   └── Speed Multiplier: 1.0
└── CharacterModel (child)
    └── Mesh
```

#### 5. FBX Settings
Her FBX için:
- Rig → Animation Type: **Humanoid**
- Avatar Definition: **Create From This Model**
- **Apply**

---

## 📋 Animator Parameters

| Parameter | Type | Purpose | Values |
|-----------|------|---------|--------|
| `Speed` | Float | Movement speed | 0 = Idle, 5 = Walk |
| `Attack` | Trigger | Attack animation | One-time trigger |

---

## 🎬 How It Works

### Movement Animation
```
Speed = 0    → 100% Idle
Speed = 2.5  → 50% Idle + 50% Walk (blend)
Speed = 5+   → 100% Walk
```

### Attack Animation
```
Left Click → PlayerWeaponSystem → OnAttack event
          → PlayerAnimationController → Attack trigger
          → Animator → Attack state
          → Animation plays → Return to Movement
```

---

## 🔧 Common Adjustments

### Animasyon Çok Yavaş/Hızlı
```
PlayerAnimationController:
├── Speed Multiplier: 1.0 (default)
│   ├── 0.5 = Yarı hız
│   ├── 1.0 = Normal
│   └── 2.0 = 2x hız
```

### Geçişler Çok Ani
```
PlayerAnimationController:
└── Animation Smooth Time: 0.1 (default)
    ├── 0.05 = Hızlı geçiş
    ├── 0.1 = Normal
    └── 0.3 = Smooth geçiş
```

### Walk Animasyonu Erken/Geç Başlıyor
```
Blend Tree:
└── Walk Threshold: 5 (default)
    ├── Düşür = Daha erken walk
    └── Yükselt = Daha geç walk
```

---

## 🐛 Troubleshooting

| Problem | Çözüm |
|---------|-------|
| Animasyon yok | Animator Controller atandı mı? |
| Sadece Idle | Blend Tree doğru kuruldu mu? Speed parametresi var mı? |
| Attack çalışmıyor | PlayerWeaponSystem var mı? Attack trigger var mı? |
| Network'te senkronize değil | NetworkObject var mı? PlayerAnimationController NetworkBehaviour mi? |
| Animasyon çok hızlı | Speed Multiplier'ı düşür (0.5-1.0) |
| Animasyon çok yavaş | Speed Multiplier'ı artır (1.5-2.0) |

---

## 📊 State Machine Diagram

```
     Entry
       ↓
   Movement (Blend Tree)
   ┌─────────────┐
   │ Speed = 0   │ → Idle
   │ Speed = 2.5 │ → Blend
   │ Speed = 5   │ → Walk
   └─────────────┘
         ↑
         │ exit time
         │
      Attack ←─── Any State (trigger)
```

---

## 💻 Code Reference

### Get Current Speed
```csharp
var animController = GetComponent<PlayerAnimationController>();
float speed = animController.GetCurrentSpeed();
```

### Check if Walking
```csharp
bool isWalking = animController.IsPlayerWalking();
```

### Manual Speed Control
```csharp
animController.SetSpeed(3.0f); // Force speed value
```

### Manual Attack Trigger
```csharp
animController.PlayAttackAnimation();
```

---

## 📁 File Locations

```
Assets/
├── Animations/
│   ├── Character.controller          ← Animator Controller
│   ├── Neutral Idle.fbx             ← Idle animation
│   ├── Standard Walk.fbx            ← Walk animation
│   └── Standing Melee Attack Downward.fbx  ← Attack animation
├── Scripts/Player/
│   └── PlayerAnimationController.cs  ← Main script
└── Prefabs/
    └── Player.prefab                 ← Player prefab
```

---

## ✅ Quick Checklist

- [ ] Parameters: Speed (Float), Attack (Trigger)
- [ ] Movement Blend Tree (Idle @ 0, Walk @ 5)
- [ ] Attack State
- [ ] Any State → Attack transition
- [ ] Attack → Movement transition
- [ ] Player prefab: Animator + PlayerAnimationController
- [ ] FBX files: Humanoid rig
- [ ] Test: Walk, Idle, Attack

---

## 🎯 Key Points

1. **Blend Tree** otomatik olarak Idle/Walk arası geçiş yapar
2. **Speed parametresi** hareket hızını kontrol eder
3. **Attack trigger** silah sistemi ile otomatik çalışır
4. **Network senkronizasyonu** otomatik
5. **Speed Multiplier** ile hız ayarlanabilir

---

## 📚 Full Documentation

- **ANİMASYON_SİSTEMİ_KILAVUZU.md** - Türkçe detaylı kılavuz
- **ANIMATION_SYSTEM_SETUP.md** - English detailed guide
- **PlayerAnimationController.cs** - Code documentation

---

**Başarılar! 🎉**



