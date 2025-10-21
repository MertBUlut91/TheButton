# Inverse Kinematics (IK) Kılavuzu

## 🤔 IK Nedir?

**Inverse Kinematics (Ters Kinematik)**, karakterin el veya ayaklarının belirli bir noktaya ulaşmasını sağlayan bir animasyon tekniğidir.

### Basit Açıklama

**Normal Animasyon (Forward Kinematics)**:
- Animator: "Omuz 30°, dirsek 45°, bilek 20° dön"
- Sonuç: El bir yere gider (nereye gideceği önceden belli değil)

**Inverse Kinematics**:
- Sen: "El şu noktaya uzansın"
- IK: "Tamam, omuz, dirsek ve bileği otomatik hesaplayıp ayarlıyorum"
- Sonuç: El tam istediğin yere gider!

---

## 🎮 Oyunlarda IK Kullanım Örnekleri

### 1. **Silah Tutma** 🔫
- Sol el silahın kabzasını tutar
- Sağ el tetiği tutar
- Silah hareket edince eller otomatik takip eder

### 2. **Zemin Adaptasyonu** 🏃
- Karakterin ayakları her zaman zemine basar
- Merdiven çıkarken ayaklar basamaklara uyum sağlar
- Eğimli yüzeylerde ayaklar kaybolmaz

### 3. **Nesne Tutma** 📦
- Karakter bir kutu taşırken
- Eller kutunun tutma yerlerinde kalır
- Kutu hareket edince eller takip eder

### 4. **Bakış Yönü** 👀
- Karakter başını belirli bir noktaya çevirir
- Düşmana bakar
- Önemli objelere odaklanır

---

## 🔧 IK Nasıl Çalışır?

### Forward Kinematics (Normal)
```
Omuz → Dirsek → Bilek → El
  30°     45°      20°     ?
  
Sonuç: El nereye giderse gitsin
```

### Inverse Kinematics (Ters)
```
El → ? → ? → ?
(0,1,2)  Bilek  Dirsek  Omuz
  
IK Hesaplar: Omuz 35°, Dirsek 50°, Bilek 15°
Sonuç: El tam (0,1,2) noktasında!
```

---

## 🎯 Unity'de IK Kullanımı

Unity'nin built-in IK sistemi var!

### 1. Animator IK (Humanoid Karakterler)

#### Setup:
```csharp
using UnityEngine;

public class WeaponIK : MonoBehaviour
{
    [Header("IK Targets")]
    public Transform leftHandTarget;  // Sol elin gideceği yer
    public Transform rightHandTarget; // Sağ elin gideceği yer
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float leftHandWeight = 1f;  // IK gücü (0=kapalı, 1=tam)
    
    [Range(0f, 1f)]
    public float rightHandWeight = 1f;
    
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    // IK her animasyon frame'inden SONRA çalışır
    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;
        
        // Sol el IK
        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
        
        // Sağ el IK
        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
        }
    }
}
```

#### Kullanım:
1. Karakterin Animator'ında IK Pass açık olmalı
2. Script'i karaktere ekle
3. Target objeleri oluştur (silahın kabzası, tetik, vs.)
4. Target'ları script'e ata
5. Eller otomatik target'ları takip eder!

---

## 🔫 Silah Sistemi İçin IK Örneği

### Senaryo:
- Sağ el silahı tutuyor (zaten var)
- Sol el silahın ön kabzasını tutmalı
- Silah hareket edince sol el takip etmeli

### Çözüm:

```csharp
using UnityEngine;

public class WeaponIKController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform currentWeapon;
    
    [Header("IK Targets")]
    public Transform leftHandGripTarget;  // Silahın ön kabzası
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float leftHandIKWeight = 1f;
    
    public bool useIK = true;
    
    void OnAnimatorIK(int layerIndex)
    {
        if (!useIK || animator == null) return;
        
        // Sol el IK (silahın ön kabzasını tut)
        if (leftHandGripTarget != null)
        {
            // IK weight ayarla
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
            
            // Sol eli target pozisyonuna getir
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandGripTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandGripTarget.rotation);
        }
        else
        {
            // IK target yoksa IK'yı kapat
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
    }
    
    // Silah değiştiğinde çağrılır
    public void SetWeapon(GameObject weapon)
    {
        if (weapon != null)
        {
            // Silahın içinde "LeftHandGrip" isimli objeyi bul
            leftHandGripTarget = weapon.transform.Find("LeftHandGrip");
            
            if (leftHandGripTarget == null)
            {
                Debug.LogWarning($"LeftHandGrip not found on {weapon.name}");
            }
        }
        else
        {
            leftHandGripTarget = null;
        }
    }
}
```

---

## 🎨 Silah Prefab'ına IK Target Ekleme

### Adım 1: Silah Prefab'ını Aç

1. BatModel (veya başka silah) prefab'ını aç
2. Hierarchy'de sağ tık → Create Empty
3. İsim: "LeftHandGrip"
4. Pozisyonu ayarla (sol elin tutacağı yer)

### Adım 2: Pozisyonu Ayarla

```
Sopa için:
Position: (0, 0.3, 0)  // Sopanın ortasında
Rotation: (0, 0, 0)

Tüfek için:
Position: (0, 0, 0.2)  // Namlu altında
Rotation: (0, 0, 0)

Tabanca için:
Position: (0, -0.05, 0.05)  // Kabza altında
Rotation: (0, 0, 0)
```

### Adım 3: Prefab'ı Kaydet

---

## 🎯 PlayerWeaponSystem'e IK Entegrasyonu

PlayerWeaponSystem'i güncelleyerek IK ekleyebiliriz:

```csharp
[Header("IK Settings")]
[Tooltip("Use IK for left hand grip")]
[SerializeField] private bool useWeaponIK = true;

[Range(0f, 1f)]
[SerializeField] private float leftHandIKWeight = 1f;

private Animator animator;
private Transform leftHandGripTarget;

private void Awake()
{
    // ... existing code ...
    
    animator = GetComponent<Animator>();
}

private void EquipWeapon(ItemData weaponData)
{
    // ... existing code ...
    
    // Find left hand grip target on weapon
    if (currentWeaponModel != null)
    {
        leftHandGripTarget = currentWeaponModel.transform.Find("LeftHandGrip");
        
        if (leftHandGripTarget != null)
        {
            Debug.Log($"[PlayerWeaponSystem] Found left hand grip on {weaponData.itemName}");
        }
    }
}

private void UnequipWeapon()
{
    // ... existing code ...
    
    leftHandGripTarget = null;
}

void OnAnimatorIK(int layerIndex)
{
    if (!useWeaponIK || animator == null || !IsOwner) return;
    
    // Sol el IK
    if (leftHandGripTarget != null)
    {
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandGripTarget.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandGripTarget.rotation);
    }
    else
    {
        // IK'yı kapat
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
    }
}
```

---

## 🎮 IK Kullanım Alanları

### 1. **İki Elli Silah Tutma** ⭐⭐⭐
- Tüfek, pompalı, sopa gibi silahlar
- Sol el otomatik silahı tutar
- Çok profesyonel görünüm

### 2. **Ayak IK (Foot IK)** ⭐⭐
- Ayaklar her zaman zemine basar
- Merdiven, rampa adaptasyonu
- Daha gerçekçi hareket

### 3. **Bakış IK (Look At)** ⭐⭐⭐
- Karakter düşmana bakar
- Baş otomatik döner
- Daha canlı karakterler

### 4. **Dirsek/Diz Hint** ⭐
- Dirsek ve dizlerin yönünü kontrol et
- Daha doğal pozlar

---

## ⚖️ IK Avantaj ve Dezavantajları

### ✅ Avantajlar

1. **Gerçekçilik**
   - Eller her zaman doğru yerde
   - Animasyon hatalarını düzeltir

2. **Esneklik**
   - Farklı silahlar için aynı animasyon
   - Runtime'da ayarlanabilir

3. **Profesyonellik**
   - AAA oyunlarda kullanılır
   - Çok daha iyi görünüm

### ❌ Dezavantajlar

1. **Performans**
   - Her frame hesaplama yapar
   - Çok karakter varsa yavaşlayabilir

2. **Karmaşıklık**
   - Setup biraz zaman alır
   - Debug etmek zor olabilir

3. **Animasyon Çakışması**
   - Bazen animasyonla çakışabilir
   - Weight ayarı gerekebilir

---

## 🎯 Ne Zaman IK Kullanmalı?

### ✅ IK Kullan:
- İki elli silahlar (tüfek, pompalı, sopa)
- Gerçekçilik önemli
- Farklı silah tipleri var
- Profesyonel görünüm istiyorsun

### ❌ IK Kullanma:
- Tek elli silahlar (tabanca, bıçak)
- Basit oyun
- Performans kritik
- Animasyonlar zaten mükemmel

---

## 📊 Karşılaştırma

### Şu Anki Sistem (Bone Parent)
```
✅ Basit
✅ Hızlı
✅ Performanslı
❌ Tek el
❌ Silah sabit pozisyonda tutulur
```

### IK Sistemi
```
✅ İki el
✅ Gerçekçi
✅ Esnek
❌ Karmaşık
❌ Biraz daha yavaş
```

---

## 🚀 Hızlı Başlangıç (IK Eklemek İçin)

### Adım 1: Animator'da IK Pass Aç

1. Player prefab'ını aç
2. Animator komponentini seç
3. Controller'ı aç
4. Base Layer → Settings
5. **IK Pass: ✓ İşaretle**

### Adım 2: Script Ekle

Yukarıdaki IK kodunu PlayerWeaponSystem'e ekle

### Adım 3: Silaha Grip Target Ekle

Her silah prefab'ına "LeftHandGrip" objesi ekle

### Adım 4: Test Et

1. Oyunu başlat
2. Silahı donat
3. Sol el otomatik grip'i tutmalı!

---

## 💡 İpuçları

### 1. IK Weight Ayarla
```csharp
leftHandIKWeight = 1f;   // Tam IK
leftHandIKWeight = 0.5f; // Yarı IK (animasyon + IK karışımı)
leftHandIKWeight = 0f;   // IK kapalı
```

### 2. Animasyonla Blend Et
```csharp
// Attack animasyonu sırasında IK'yı azalt
if (isAttacking)
    leftHandIKWeight = 0.3f;
else
    leftHandIKWeight = 1f;
```

### 3. Smooth Transition
```csharp
// IK'yı yumuşak aç/kapat
currentIKWeight = Mathf.Lerp(currentIKWeight, targetIKWeight, Time.deltaTime * 5f);
animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, currentIKWeight);
```

---

## 🎯 Özet

**IK (Inverse Kinematics)**:
- Elin belirli bir noktaya ulaşmasını sağlar
- İki elli silah tutma için mükemmel
- Daha gerçekçi animasyonlar
- Biraz daha karmaşık ama çok profesyonel

**Şu Anki Sistem (Bone Parent)**:
- Basit ve hızlı
- Tek el için yeterli
- Performanslı
- Senin durumun için şimdilik yeterli

**Öneri**:
- Şimdilik bone parent sistemi kullan (zaten çalışıyor!)
- İleride iki elli silahlar eklemek istersen IK'ya geç
- IK daha gelişmiş bir özellik, acele etme

İyi oyunlar! 🎮

