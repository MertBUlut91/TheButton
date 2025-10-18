# Fake Cover System - Kullanım Rehberi

## 🎯 Genel Bakış

Fake Cover (Sahte Kapak) sistemi, event'leri gizlemek için kullanılır. Oyuncu önce kapağa tıklar, kapak kalkar ve altındaki gerçek event ortaya çıkar.

## ✨ Özellikler

- ✅ Event'leri gizleyen kapaklar
- ✅ Tek tıklama ile kapak kaldırma
- ✅ Özelleştirilebilir kapak prompt metni
- ✅ Kapak kaldırma ses efekti
- ✅ Kapak kaldırma parçacık efekti
- ✅ Kapak yok etme veya devre dışı bırakma seçeneği
- ✅ Multiplayer senkronizasyonu

## 🎮 Nasıl Kullanılır?

### 1. Temel Kurulum

1. **GameObject Oluştur:**
   - Event için ana GameObject oluştur
   - `GeneralInteractableEvent` component'ini ekle
   - `NetworkObject` component'i olduğundan emin ol

2. **Kapak Objesi Oluştur:**
   - Kapak için GameObject oluştur (panel, kapı, kutu, vb.)
   - Event'in child'ı veya ayrı obje olabilir
   - Collider ekle (etkileşim için)

3. **Inspector Ayarları:**
   ```
   Fake Cover System:
   ├─ Use Fake Cover: ✅ (aktif et)
   ├─ Cover Objects: [Kapak GameObject'i ekle]
   ├─ Cover Prompt Text: "Press E to remove cover"
   ├─ Cover Remove Sound: (ses efekti)
   ├─ Cover Remove Effect: (parçacık efekti)
   └─ Destroy Cover: ❌ (sadece gizle) veya ✅ (yok et)
   ```

### 2. Örnek Konfigürasyonlar

#### Örnek 1: Basit Panel Kapağı

```
Senaryo: Bir elektrik panelinin önünde metal kapak var

Setup:
├─ Event GameObject: "ElectricPanel"
│  ├─ GeneralInteractableEvent component
│  ├─ NetworkObject component
│  └─ Event mesh (panel içi)
│
└─ Cover GameObject: "PanelCover"
   ├─ Mesh (metal kapak)
   └─ Collider

Inspector (GeneralInteractableEvent):
- Use Fake Cover: ✅
- Cover Objects: [PanelCover]
- Cover Prompt Text: "Press E to open panel cover"
- Cover Remove Sound: metal_slide.wav
- Destroy Cover: ❌ (tekrar kullanılabilir)

Event Settings:
- Required Items: [Screwdriver]
- Hold Duration: 2 seconds
```

**Sonuç:**
1. Oyuncu kapağa yaklaşır → "Press E to open panel cover"
2. E'ye basar → Kapak kaybolur, ses çalar
3. Altındaki event görünür → "Hold E for 2s (needs: Screwdriver)"
4. Tornavida varsa → Event'i kullanabilir

---

#### Örnek 2: Vana Kapağı

```
Senaryo: Vananın üzerinde koruyucu kapak var

Setup:
├─ Event GameObject: "Valve"
│  ├─ GeneralInteractableEvent component
│  ├─ NetworkObject component
│  ├─ Valve mesh
│  └─ Valve handle (dönen kısım)
│
└─ Cover GameObject: "ValveCover"
   ├─ Mesh (koruyucu kapak)
   └─ Collider

Inspector (GeneralInteractableEvent):
- Use Fake Cover: ✅
- Cover Objects: [ValveCover]
- Cover Prompt Text: "Press E to remove protective cover"
- Cover Remove Sound: plastic_snap.wav
- Cover Remove Effect: Dust particles
- Destroy Cover: ✅ (bir kere kaldırılınca yok olsun)

Event Settings:
- Required Items: [Wrench]
- Hold Duration: 3 seconds
- Rotating Objects: [Valve Handle]
- Rotation Speed: 180
```

**Sonuç:**
1. Oyuncu kapağa yaklaşır → "Press E to remove protective cover"
2. E'ye basar → Kapak yok olur, ses ve parçacık efekti
3. Vana görünür → "Hold E for 3s (needs: Wrench)"
4. İngiliz anahtarı varsa → Vanayı çevirebilir

---

#### Örnek 3: Çoklu Kapak

```
Senaryo: Büyük bir kontrol panelinin birden fazla kapağı var

Setup:
├─ Event GameObject: "ControlPanel"
│  ├─ GeneralInteractableEvent component
│  └─ Panel mesh
│
├─ Cover1: "TopCover"
├─ Cover2: "BottomCover"
└─ Cover3: "SideCover"

Inspector (GeneralInteractableEvent):
- Use Fake Cover: ✅
- Cover Objects: [TopCover, BottomCover, SideCover]
- Cover Prompt Text: "Press E to open panel"
- Destroy Cover: ❌
```

**Sonuç:**
- Tek E tuşuna basışta tüm kapaklar birden kalkar
- Event ortaya çıkar

---

#### Örnek 4: Animasyonlu Kapak

```
Senaryo: Kapak animasyonlu bir şekilde açılıyor

Setup:
├─ Event GameObject: "SecretDoor"
│  └─ GeneralInteractableEvent component
│
└─ Cover GameObject: "DoorCover"
   ├─ Animator component
   └─ Animation: "DoorOpen"

Inspector (GeneralInteractableEvent):
- Use Fake Cover: ✅
- Cover Objects: [DoorCover]
- Cover Remove Sound: door_open.wav
- Destroy Cover: ❌

Cover Animator:
- "Open" trigger → Plays door opening animation
- Animation ends → SetActive(false) via Animation Event
```

**Not:** Animasyon için Animator'ı cover objesine ekle, GeneralInteractableEvent otomatik olarak SetActive(false) yapacak.

---

## 🎨 Görsel Tasarım İpuçları

### Kapak Tipleri

1. **Metal Panel:**
   - Mesh: Düz metal plaka
   - Material: Metallic shader
   - Sound: Metal_slide.wav
   - Effect: Metal sparks

2. **Plastik Kapak:**
   - Mesh: Hafif kavisli kapak
   - Material: Plastic shader
   - Sound: Plastic_snap.wav
   - Effect: Dust puff

3. **Ahşap Kutu:**
   - Mesh: Tahta kapak
   - Material: Wood shader
   - Sound: Wood_creak.wav
   - Effect: Wood splinters

4. **Cam Panel:**
   - Mesh: Şeffaf cam
   - Material: Glass shader (alpha)
   - Sound: Glass_break.wav
   - Effect: Glass shards

### Kapak Yerleşimi

```
Doğru Yerleşim:
┌─────────────┐
│   COVER     │ ← Oyuncu bunu görür
│  (visible)  │
└─────────────┘
      ↓ E tuşu
┌─────────────┐
│   EVENT     │ ← Kapak kalkınca görünür
│  (hidden)   │
└─────────────┘
```

## 🔊 Ses Efekti Önerileri

| Kapak Tipi | Ses Efekti | Süre |
|------------|------------|------|
| Metal Panel | Metal slide/scrape | 0.5-1s |
| Plastik Kapak | Snap/click | 0.2-0.5s |
| Ahşap Kapak | Creak/crack | 0.5-1s |
| Cam Panel | Glass break | 0.3-0.7s |
| Elektronik Panel | Beep/unlock | 0.2-0.5s |

## ✨ Parçacık Efekti Önerileri

| Kapak Tipi | Parçacık Efekti | Renk |
|------------|-----------------|------|
| Metal Panel | Sparks | Orange/Yellow |
| Plastik Kapak | Dust puff | Gray/White |
| Ahşap Kapak | Wood chips | Brown |
| Cam Panel | Glass shards | Clear/White |
| Elektronik Panel | Electric sparks | Blue/Cyan |

## 🎯 Kullanım Senaryoları

### Senaryo 1: Gizli Puzzle
```
Oyuncu bir odada dolaşıyor
→ Normal görünen bir panel görüyor
→ E'ye basıyor → Kapak kalkıyor
→ Altında puzzle event var
→ Sürpriz! 🎉
```

### Senaryo 2: Güvenlik Sistemi
```
Oyuncu güvenli bir alana girmek istiyor
→ Kapak altında kontrol paneli var
→ Önce kapağı kaldırmalı
→ Sonra doğru item ile paneli aktif etmeli
→ Çok aşamalı puzzle
```

### Senaryo 3: Hasar Görmüş Ekipman
```
Oyuncu kırık bir makine görüyor
→ Üzerinde hasar görmüş kapak var
→ Kapağı kaldırıyor
→ İçeride tamir edilmesi gereken event
→ Doğru tool ile tamir ediyor
```

## 🌐 Multiplayer Davranışı

- **Senkronize:** Bir oyuncu kapağı kaldırınca tüm oyuncular görür
- **Tek Kaldırma:** Kapak bir kere kaldırılır, tekrar kapanmaz
- **Server Authority:** Server kontrol eder
- **Network Optimized:** Minimal bandwidth kullanımı

## 🔧 Gelişmiş Ayarlar

### Destroy Cover vs SetActive(false)

**Destroy Cover = ✅ (Yok Et):**
- ✅ Bellek temizlenir
- ✅ Geri döndürülemez
- ❌ Tekrar kullanılamaz
- **Kullanım:** Tek seferlik event'ler için

**Destroy Cover = ❌ (Gizle):**
- ✅ Tekrar aktif edilebilir
- ✅ Bellek kullanımı devam eder
- ✅ Debug için yararlı
- **Kullanım:** Test veya tekrar kullanılabilir event'ler için

### Prompt Metni Örnekleri

```
İngilizce:
- "Press E to remove cover"
- "Press E to open panel"
- "Press E to break seal"
- "Press E to reveal"

Türkçe:
- "Kapağı kaldırmak için E'ye bas"
- "Paneli açmak için E'ye bas"
- "Mührü kırmak için E'ye bas"
- "Ortaya çıkarmak için E'ye bas"

Yaratıcı:
- "Press E to unveil the mystery"
- "Press E to discover what's hidden"
- "Press E to break the seal"
```

## 🐛 Sorun Giderme

### Kapak Kalkıyor Ama Event Görünmüyor
**Çözüm:** Event objesinin collider'ı olduğundan ve aktif olduğundan emin ol.

### Kapak Kalkmıyor
**Çözüm:** 
- Use Fake Cover işaretli mi?
- Cover Objects listesi dolu mu?
- Cover objeleri null değil mi?

### Ses Çalmıyor
**Çözüm:**
- Cover Remove Sound atandı mı?
- AudioSource component var mı?

### Multiplayer'da Senkronize Olmuyor
**Çözüm:**
- NetworkObject component var mı?
- Server'da mı test ediyorsun?

## 💡 Pro İpuçları

1. **Görsel İpucu Ver:** Kapağın farklı olduğunu göster (renk, desen, ışık)
2. **Ses Kullan:** Kapak kalkınca tatmin edici ses efekti
3. **Parçacık Ekle:** Görsel geri bildirim önemli
4. **Prompt Açıklayıcı Olsun:** "Press E" yerine "Press E to remove cover"
5. **Layer Kullan:** Kapak ve event farklı layer'larda olabilir
6. **Test Et:** Multiplayer'da mutlaka test et

## 📊 Performans

- **CPU:** Minimal (sadece state değişiminde)
- **Memory:** Kapak başına ~100-500KB (mesh'e bağlı)
- **Network:** Minimal (tek boolean sync)
- **Önerilen Kapak Sayısı:** Obje başına 1-5 kapak

## 🎓 Öğrenme Yolu

### Başlangıç (5 dakika)
1. Basit panel + kapak oluştur
2. Use Fake Cover aktif et
3. Cover Objects'e ekle
4. Test et

### Orta Seviye (15 dakika)
1. Ses efekti ekle
2. Parçacık efekti ekle
3. Prompt metnini özelleştir
4. Multiplayer'da test et

### İleri Seviye (30 dakika)
1. Çoklu kapak sistemi
2. Animasyonlu kapak
3. Event ile entegrasyon
4. Custom prompt'lar

## 🎉 Örnek Kullanım

### Tam Örnek: Elektrik Paneli

```
GameObject Hierarchy:
├─ ElectricPanel (GeneralInteractableEvent)
│  ├─ PanelBody (mesh)
│  ├─ PanelWires (mesh)
│  └─ PanelCover (mesh + collider)
│
└─ NetworkObject

Inspector Settings:
[Fake Cover System]
- Use Fake Cover: ✅
- Cover Objects: [PanelCover]
- Cover Prompt Text: "Press E to open electrical panel"
- Cover Remove Sound: metal_slide.wav
- Cover Remove Effect: Sparks particle system
- Destroy Cover: ❌

[Event Settings]
- Required Items: [Screwdriver]
- Hold Duration: 2 seconds
- Success Sound: panel_activate.wav

Oyun İçi:
1. Oyuncu panele yaklaşır
   → "Press E to open electrical panel"
   
2. E'ye basar
   → Metal ses çalar
   → Kıvılcım efekti
   → Kapak kaybolur
   
3. Panel görünür
   → "Hold E for 2s (needs: Screwdriver)"
   
4. Tornavida varsa
   → 2 saniye basılı tutar
   → Panel aktif olur
   → Başarı sesi çalar
```

---

**Sistem Durumu:** ✅ Kullanıma Hazır  
**Multiplayer:** ✅ Tam Destek  
**Dokümantasyon:** ✅ Tamamlandı  

**İyi eğlenceler! 🎮✨**

