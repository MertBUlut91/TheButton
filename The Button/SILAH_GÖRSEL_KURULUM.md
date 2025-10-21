# Silah Görsel Kurulum Kılavuzu

## 🎯 Silahın Elde Görünmesi İçin Adımlar

### Adım 1: Weapon Holder Ayarla

1. **Player prefab'ını aç**
2. Karakterinin el pozisyonunda oluşturduğun boş objeyi bul
3. Bu objenin ismini **"WeaponHolder"** yap
4. Pozisyonunu ayarla (örnek: sağ elde olsun)
   - Local Position: (0.3, -0.2, 0.5) gibi
   - Local Rotation: (0, 0, 0)
5. Player prefab'ında **PlayerWeaponSystem** komponentini bul
6. **Weapon Holder** alanına bu objeyi sürükle

### Adım 2: Sopa 3D Modeli Oluştur

#### Yöntem A: Basit Test Modeli (Hızlı)

1. **Hierarchy'de sağ tık** → 3D Object → Cylinder
2. İsmini **"BatModel"** yap
3. Transform ayarları:
   - Scale: (0.05, 0.5, 0.05) - ince uzun sopa
   - Rotation: (0, 0, 90) - yatay pozisyon
4. Material ekle (isteğe bağlı):
   - Kahverengi veya gri material
5. **Prefab yap**:
   - Hierarchy'den **BatModel**'i Project'e sürükle
   - Prefab oluştur
6. Hierarchy'den orijinal BatModel'i sil

#### Yöntem B: Kendi Modelini Kullan

1. Sopa 3D modelini Unity'ye import et
2. Model'i prefab yap
3. Prefab'ı kullan

### Adım 3: ItemData'ya Model Ata

1. **Project'te sopanın ItemData'sını bul**
   - Örnek: `Assets/Resources/Items/Bat.asset`
2. **Inspector'da şu alanları doldur**:
   - **Hand Model**: Oluşturduğun sopa prefab'ını sürükle
   - **Item Prefab**: Dünya için sopa prefab'ı (bırakma için)
   - **Can Be Held**: ✓ İşaretle
   - **Category**: Weapon
   - **Item Type**: Bat
   - **Weapon Damage**: 20
   - **Attack Range**: 2.5
   - **Attack Speed**: 0.8
   - **Is Melee Weapon**: ✓ İşaretle

### Adım 4: Test Et

1. **Oyunu başlat**
2. **Console'da test kodu çalıştır**:
```csharp
PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
inventory.AddItemServerRpc("Bat"); // Sopanın asset ismini kullan
```
3. **1 tuşuna bas** - Sopa donatılmalı
4. **Elde görünmeli** - WeaponHolder pozisyonunda
5. **Sol tık** - Saldır!

### Adım 5: Pozisyon Ayarları (İsteğe Bağlı)

Silah elde iyi görünmüyorsa:

#### PlayerWeaponSystem'de Ayar
Script'te pozisyon ayarını değiştirebilirsin:

```csharp
// PlayerWeaponSystem.cs, satır 149 civarı
weaponHolder.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
```

Bu değerleri ayarla:
- **X**: Sağ/Sol (0.3 = sağda)
- **Y**: Yukarı/Aşağı (-0.2 = biraz aşağıda)
- **Z**: İleri/Geri (0.5 = kamera önünde)

#### Prefab'ta Ayar
Veya silah prefab'ının kendi pozisyonunu ayarla:
- Prefab'ı aç
- Root objesinin Transform'unu ayarla
- Local Position ve Rotation değiştir

### Adım 6: Her Silah İçin Tekrarla

Her silah için:
1. 3D model oluştur/import et
2. Prefab yap
3. ItemData'ya ata
4. Test et

## 🎨 Örnek Silah Modelleri

### Sopa (Bat)
```
Cylinder:
- Scale: (0.05, 0.5, 0.05)
- Rotation: (0, 0, 90)
- Material: Kahverengi
```

### Bıçak (Knife)
```
Cube + Cube (sap + bıçak):
- Sap: Scale (0.02, 0.15, 0.02)
- Bıçak: Scale (0.01, 0.2, 0.05)
- Material: Gri/Metal
```

### Tabanca (Pistol)
```
Cube + Cube (namlu + kabza):
- Namlu: Scale (0.03, 0.15, 0.03)
- Kabza: Scale (0.04, 0.08, 0.06)
- Material: Siyah/Metal
```

## 🔧 Sorun Giderme

### Silah Görünmüyor
**Kontrol Et**:
- [ ] Hand Model atandı mı?
- [ ] WeaponHolder atandı mı?
- [ ] PlayerWeaponSystem komponenti var mı?
- [ ] Silah donatıldı mı? (Console'da log var mı?)

**Çözüm**:
```csharp
// Console'da kontrol et
PlayerWeaponSystem ws = FindObjectOfType<PlayerWeaponSystem>();
Debug.Log($"Has weapon: {ws.HasWeaponEquipped()}");
Debug.Log($"Weapon holder: {ws.weaponHolder != null}");
```

### Silah Yanlış Pozisyonda
**Çözüm 1**: WeaponHolder pozisyonunu ayarla
- Player prefab'ında WeaponHolder'ı seç
- Local Position değiştir
- Test et

**Çözüm 2**: Script'te pozisyonu değiştir
- PlayerWeaponSystem.cs'yi aç
- Satır 149'u bul
- `weaponHolder.localPosition` değerini değiştir

### Silah Çok Büyük/Küçük
**Çözüm**: Prefab'ın scale'ini ayarla
- Silah prefab'ını aç
- Scale değerini değiştir
- Örnek: (0.5, 0.5, 0.5) - yarı boyut

### Silah Yanlış Yönde
**Çözüm**: Prefab'ın rotation'ını ayarla
- Silah prefab'ını aç
- Rotation değerini değiştir
- Örnek: (0, 90, 0) - 90 derece dön

## 📝 Hızlı Başlangıç Kodu

### Test İçin Sopa Ekle
```csharp
// Console'da çalıştır
PlayerInventory inv = FindObjectOfType<PlayerInventory>();
inv.AddItemServerRpc("Bat");
```

### Silah Durumunu Kontrol Et
```csharp
PlayerWeaponSystem ws = FindObjectOfType<PlayerWeaponSystem>();
if (ws.HasWeaponEquipped())
{
    ItemData weapon = ws.GetCurrentWeapon();
    Debug.Log($"Silah: {weapon.itemName}, Hasar: {weapon.weaponDamage}");
}
```

### WeaponHolder'ı Bul
```csharp
// Player'ın child'larını kontrol et
Transform player = FindObjectOfType<PlayerWeaponSystem>().transform;
foreach (Transform child in player.GetComponentsInChildren<Transform>())
{
    Debug.Log($"Child: {child.name}");
}
```

## 🎯 Özet

1. ✅ WeaponHolder objesini oluştur (karakterin elinde)
2. ✅ PlayerWeaponSystem'e WeaponHolder'ı ata
3. ✅ Silah 3D modeli oluştur (Cylinder ile basit test)
4. ✅ Model'i prefab yap
5. ✅ ItemData'da Hand Model alanına prefab'ı ata
6. ✅ Test et: Silah ekle → 1'e bas → Elde görünmeli!

## 💡 İpuçları

- **Basit başla**: İlk önce Cylinder ile test et
- **Pozisyon ayarla**: WeaponHolder'ın pozisyonunu karakterine göre ayarla
- **Scale önemli**: Silah modelinin scale'i çok önemli (çok büyük olmasın)
- **Test et**: Her değişiklikten sonra test et
- **Console logları**: Silahın donatıldığını console'dan kontrol et

## 🎮 Sonuç

Artık silahlar elde görsel olarak görünecek! Sopanı donatınca elinde sopayı göreceksin ve sol tık ile saldırabileceksin. İyi oyunlar! 🎉


