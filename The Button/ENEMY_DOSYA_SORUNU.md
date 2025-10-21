# Enemy Dosyaları Görünmüyor - Çözüm

## 🐛 Problem

Enemy klasörü Unity'de boş gözüküyor ama dosyalar var.

## ✅ Çözüm

### Yöntem 1: Unity'yi Yenile

1. **Unity'de Enemy klasörüne sağ tık**
2. **Reimport** seç
3. Veya **Ctrl+R** (Cmd+R) - Tüm asset'leri yenile

### Yöntem 2: Unity'yi Yeniden Başlat

1. **Unity'yi kapat**
2. **Unity'yi tekrar aç**
3. Dosyalar görünmeli

### Yöntem 3: Dosyaları Manuel Kontrol Et

Terminal'de:
```bash
ls -la "Assets/Scripts/Enemy/"
```

**Görmeli**:
- EnemyHealth.cs
- EnemyHealth.cs.meta
- EnemyAI.cs
- EnemyAI.cs.meta

### Yöntem 4: .meta Dosyalarını Kontrol Et

Eğer .meta dosyaları yoksa Unity bunları görmez.

**Çözüm**: Unity'yi yeniden başlat, otomatik oluşturur.

---

## 🔧 Eğer Hala Görünmüyorsa

### Dosyaları Yeniden Oluştur

1. **Unity'de Enemy klasörüne sağ tık**
2. **Create → C# Script**
3. **İsim**: EnemyHealth
4. **Script'i aç**
5. **İçeriği kopyala-yapıştır** (ENEMY_SYSTEM_GUIDE.md'den)

Aynısını EnemyAI için tekrarla.

---

## 💡 Neden Oluyor?

Unity bazen yeni dosyaları hemen görmez:
- Asset database güncellenmemiş
- .meta dosyaları eksik
- Unity cache problemi

**Çözüm**: Unity'yi yenile veya yeniden başlat!

---

## 🎯 Hızlı Çözüm

**En kolay**:
1. Unity'yi kapat
2. Unity'yi aç
3. Dosyalar görünmeli ✓

İyi oyunlar! 🎮


