# Manuel Bone Takip - Kullanım Kılavuzu

## 🎯 Yeni Sistem

Artık sistem otomatik arama yapmıyor. Sen el bone'unu Inspector'dan atıyorsun, sistem onu sürekli takip ediyor!

---

## 🚀 Kurulum (3 Basit Adım)

### Adım 1: El Bone'unu Bul

1. **Player prefab'ını aç**
2. **Hierarchy'de karakterini genişlet**
3. **Armature/Skeleton'u genişlet**
4. **Sağ el bone'unu bul**
   - Örnek isimler:
     - `RightHand`
     - `Hand_R`
     - `mixamorig:RightHand`
     - `Bip01_R_Hand`

### Adım 2: Bone'u Ata

1. **PlayerWeaponSystem komponentini seç**
2. **Inspector'da "Hand Bone" alanını bul**
3. **Hierarchy'den el bone'unu bu alana sürükle**

```
[Weapon Display]
├─ Weapon Holder: (WeaponHolder objesi)
├─ Hand Bone: (Sağ el bone'unu buraya sürükle!) ← ÖNEMLİ
└─ Use Constraint System: ✓
```

### Adım 3: Test Et

1. **Prefab'ı kaydet** (Ctrl+S)
2. **Oyunu başlat**
3. **Console'da kontrol et**:
   ```
   [PlayerWeaponSystem] Constraint system enabled. WeaponHolder will follow: RightHand
   ```
4. **Silahı donat** (1 tuşu)
5. **Silah el ile birlikte hareket etmeli!** ✅

---

## 🎨 Pozisyon Ayarları (İsteğe Bağlı)

Silahın pozisyonu tam doğru değilse Inspector'dan ayarlayabilirsin!

### Inspector'da:

```
[Position Offset (Optional)]
├─ Position Offset: (0, 0.05, 0)  ← 5cm yukarı
└─ Rotation Offset: (0, 90, 0)    ← 90 derece dön
```

### Yaygın Ayarlar:

**Position Offset**:
```
(0, 0, 0)      → Tam el bone pozisyonunda
(0, 0.05, 0)   → 5cm yukarı
(0.05, 0, 0)   → 5cm sağa
(0, 0, 0.05)   → 5cm ileri
```

**Rotation Offset**:
```
(0, 0, 0)      → Normal
(0, 90, 0)     → 90 derece sağa dön
(0, -90, 0)    → 90 derece sola dön
(90, 0, 0)     → Öne eğ
```

---

## 🔧 Constraint System vs Direct Parenting

### Constraint System (Önerilen) ✅

**Inspector'da**:
```
Use Constraint System: ✓ İşaretli
```

**Nasıl Çalışır**:
- WeaponHolder player'ın child'ı
- Her frame bone'un pozisyonunu kopyalar
- Scale her zaman 1,1,1 (bone'dan etkilenmez)

**Avantajlar**:
- ✅ Scale problemi yok
- ✅ Silah normal boyutta
- ✅ Offset'ler Inspector'dan ayarlanabilir

### Direct Parenting (Eski)

**Inspector'da**:
```
Use Constraint System: ☐ İşaretsiz
```

**Nasıl Çalışır**:
- WeaponHolder bone'un direkt child'ı
- Bone'un scale'ini miras alır

**Avantajlar**:
- ✅ Biraz daha basit
- ❌ Scale problemi olabilir

---

## 💡 İpuçları

### 1. Bone'u Kolayca Bul

**Yöntem 1**: Hierarchy'de ara
- Ctrl+F (veya Cmd+F) ile ara
- "hand" yaz
- Sağ el bone'unu bul

**Yöntem 2**: Inspector'da seç
- Karakteri seç
- Scene view'da el kemiğini seç
- Inspector'da ismi görünür

### 2. Play Mode'da Test Et

1. Play mode'a gir
2. Hierarchy'de WeaponHolder'ı seç
3. Position/Rotation'ı ayarla
4. Değerleri not et
5. Play mode'dan çık
6. Inspector'a değerleri gir

### 3. Offset'leri Runtime'da Değiştir

```csharp
// Console'da veya script'te
PlayerWeaponSystem ws = FindObjectOfType<PlayerWeaponSystem>();

// Pozisyon ayarla
ws.SetWeaponHolderOffset(new Vector3(0, 0.05f, 0));

// Rotation ayarla
ws.SetWeaponHolderRotationOffset(new Vector3(0, 90, 0));
```

---

## 🐛 Sorun Giderme

### ❌ "Hand bone not assigned" Uyarısı

**Sebep**: Hand Bone alanı boş

**Çözüm**:
1. Inspector'da Hand Bone alanını kontrol et
2. Hierarchy'den el bone'unu sürükle
3. Prefab'ı kaydet

### ❌ Silah Hareket Etmiyor

**Kontrol Et**:
- [ ] Hand Bone atandı mı?
- [ ] Use Constraint System işaretli mi?
- [ ] Animator çalışıyor mu?

**Çözüm**:
```csharp
// Console'da kontrol et
PlayerWeaponSystem ws = FindObjectOfType<PlayerWeaponSystem>();
Debug.Log($"Hand bone: {ws.handBone != null}");
Debug.Log($"Constraint: {ws.useConstraintSystem}");
```

### ❌ Silah Garip Pozisyonda

**Çözüm**: Offset'leri ayarla
1. Inspector'da Position Offset'i değiştir
2. Inspector'da Rotation Offset'i değiştir
3. Play mode'da test et

---

## 📊 Karşılaştırma

### Eski Sistem (Otomatik Arama)
```
❌ Bone ismini tahmin ediyor
❌ Bazen yanlış bone buluyor
❌ Debug zor
✅ Otomatik
```

### Yeni Sistem (Manuel Atama)
```
✅ Sen bone'u seçiyorsun
✅ Her zaman doğru bone
✅ Görsel olarak atama (sürükle-bırak)
✅ Debug kolay
✅ Daha kontrollü
```

---

## 🎯 Özet

### Ne Değişti?

**Önceki**:
```csharp
[SerializeField] private string rightHandBoneName = "RightHand";
// Script bone'u arıyor
```

**Şimdi**:
```csharp
[SerializeField] private Transform handBone;
// Sen bone'u Inspector'dan atıyorsun
```

### Avantajlar

1. **Daha Güvenilir**
   - Sen bone'u seçiyorsun
   - Yanlış bone seçilme riski yok

2. **Daha Kolay**
   - Sürükle-bırak
   - Bone ismini bilmene gerek yok

3. **Daha Esnek**
   - İstediğin bone'u seçebilirsin
   - Sol el, sağ el, başka yerler...

4. **Daha Görsel**
   - Inspector'da görüyorsun
   - Hangi bone olduğu belli

---

## 📋 Checklist

Kurulum için:
- [ ] Player prefab'ını açtım
- [ ] Hierarchy'de el bone'unu buldum
- [ ] PlayerWeaponSystem'de Hand Bone alanına bone'u sürükledim
- [ ] Use Constraint System işaretli
- [ ] Prefab'ı kaydettim
- [ ] Oyunu başlattım
- [ ] Console'da "WeaponHolder will follow" mesajı var
- [ ] Silahı donattım
- [ ] Silah el ile birlikte hareket ediyor ✓

---

## 🎉 Sonuç

Artık sistem çok daha basit ve güvenilir!

**Yapman Gerekenler**:
1. ✅ Hierarchy'den el bone'unu bul
2. ✅ Inspector'da Hand Bone alanına sürükle
3. ✅ Prefab'ı kaydet
4. ✅ Test et!

**Sistem**:
- ✅ Bone'u sürekli takip ediyor
- ✅ Scale bağımsız (her zaman 1,1,1)
- ✅ Offset'ler Inspector'dan ayarlanabilir
- ✅ Runtime'da da değiştirilebilir

İyi oyunlar! 🎮


