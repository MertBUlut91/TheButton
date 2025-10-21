# Enemy Namespace Hatası - Çözüm

## 🐛 Hata

```
error CS0234: The type or namespace name 'Enemy' does not exist in the namespace 'TheButton'
```

## ✅ Çözüm

### Adım 1: Unity'yi Yenile

1. **Unity'ye dön**
2. **Ctrl+R** (veya Cmd+R) - Refresh
3. Veya **Assets → Refresh**

### Adım 2: Script'leri Kontrol Et

Şu dosyaların olduğundan emin ol:
```
Assets/Scripts/Enemy/
├── EnemyHealth.cs
├── EnemyHealth.cs.meta
├── EnemyAI.cs
└── EnemyAI.cs.meta
```

### Adım 3: Unity Yeniden Derlesin

1. **Herhangi bir script'i aç**
2. **Boş bir satır ekle ve kaydet**
3. **Unity otomatik derleyecek**

### Adım 4: Hala Hata Varsa

**Console'da**:
```
Assets → Reimport All
```

Veya Unity'yi kapat ve tekrar aç.

---

## 🔍 Dosyaların Doğru Yerde Olduğunu Kontrol Et

### Terminal'de:
```bash
ls -la "Assets/Scripts/Enemy/"
```

**Görmeli**:
- EnemyHealth.cs
- EnemyHealth.cs.meta
- EnemyAI.cs
- EnemyAI.cs.meta

---

## 💡 Alternatif Çözüm

Eğer hala hata varsa, namespace'i tam yaz:

### PlayerWeaponSystem.cs'de:

**Şu anki**:
```csharp
var targetEnemy = hit.collider.GetComponent<TheButton.Enemy.EnemyHealth>();
```

**Alternatif** (using ekle):
```csharp
// Dosyanın başına ekle
using TheButton.Enemy;

// Sonra kullan
var targetEnemy = hit.collider.GetComponent<EnemyHealth>();
```

---

## 🎯 Özet

**Yapman Gerekenler**:
1. ✅ Unity'yi yenile (Ctrl+R)
2. ✅ Dosyaların yerini kontrol et
3. ✅ Unity'nin derlemesini bekle
4. ✅ Hata gitmeli!

**Hala Hata Varsa**:
- Unity'yi kapat ve tekrar aç
- Assets → Reimport All

İyi oyunlar! 🎮


