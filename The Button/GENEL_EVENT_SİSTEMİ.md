# Genel Event Sistemi - Türkçe Özet

## Yapılan Değişiklikler

`ValveEvent` ve `PuzzlePanelEvent` gibi özel puzzle scriptleri yerine, tek bir genel ve esnek **`GeneralInteractableEvent`** sistemi oluşturuldu.

## Yeni Sistemin Özellikleri

### 🎯 Item Gereksinimleri
- Event'i kullanabilmek için envanterde belirli itemlar gerekli
- ItemData listesi ile yapılandırılabilir
- İsteğe bağlı item tüketimi (kullanınca yok olsun mu?)

### ⏱️ Basılı Tutma Sistemi
- E tuşuna basılı tutma süresi ayarlanabilir (0 = anında aktif)
- Gerçek zamanlı ilerleme göstergesi
- Tuşu bırakınca iptal olur
- Ekranda yüzde göstergesi: "Hold E (45%)"

### 🔄 Obje Rotasyonu
- Etkileşim sırasında birden fazla obje döndürülebilir
- Dönüş hızı ve yönü ayarlanabilir
- Tamamlandıktan sonra dönmeye devam etsin mi? (opsiyonel)
- Editor'de görsel ok göstergesi

### 🎬 Animasyon Desteği
- Başlangıç animasyonu
- Başarılı tamamlama animasyonu
- Başarısız/iptal animasyonu
- Tamamen opsiyonel

### 🔊 Ses Efektleri
- **Başlangıç sesi**: Etkileşim başladığında
- **Döngü sesi**: Basılı tutarken çalan loop ses
- **Başarı sesi**: Tamamlandığında
- **Başarısız sesi**: İptal edildiğinde
- **Reddedilme sesi**: Gerekli itemlar yoksa
- Hepsi opsiyonel

### ✨ Parçacık Efektleri
- Başlangıç efekti
- Başarı efekti
- Başarısızlık efekti
- Hepsi opsiyonel

### 🎨 Görsel Geri Bildirim
- Renk değişimleri:
  - **Kırmızı**: Kilitli/aktif değil
  - **Sarı**: Kullanılıyor
  - **Yeşil**: Aktif edildi
- Renkler özelleştirilebilir

### 🌐 Network Senkronizasyonu
- Tam multiplayer desteği
- Server otoriteli
- Aynı anda sadece bir oyuncu kullanabilir
- Diğer oyuncular "Someone is using this..." görür

### 🔒 Tek Kullanımlık veya Tekrar Kullanılabilir
- `oneTimeUse` ayarı ile kontrol edilir
- Tek seferlik veya sınırsız kullanım

## Kullanım Örnekleri

### Örnek 1: İngiliz Anahtarı ile Vana

```
Ayarlar:
- Required Items: [Wrench ItemData]
- Consume Items: ✅ (anahtar tüketilir)
- One Time Use: ✅ (sadece bir kere)
- Hold Duration: 3 saniye
- Rotating Objects: [Vana Kolu Transform]
- Rotation Speed: 180
- Rotation Axis: (0, 0, 1)
- Continue Rotation After Complete: ✅

Sonuç:
- Oyuncunun envanterinde İngiliz Anahtarı olmalı
- E tuşuna 3 saniye basılı tut
- Vana kolu dönerken gösterir
- İngiliz Anahtarı tüketilir
- Vana tamamlandıktan sonra dönmeye devam eder
```

### Örnek 2: Tornavida ile Panel

```
Ayarlar:
- Required Items: [Screwdriver ItemData]
- Consume Items: ❌ (tornavida kalır)
- One Time Use: ✅
- Hold Duration: 2 saniye
- Rotating Objects: [Panel Kapağı Transform]
- Rotation Speed: 90
- Rotation Axis: (0, 1, 0)

Sonuç:
- Oyuncunun envanterinde Tornavida olmalı
- E tuşuna 2 saniye basılı tut
- Panel kapağı açılır
- Tornavida envanterde kalır
```

### Örnek 3: Basit Buton (Gereksinim Yok)

```
Ayarlar:
- Required Items: (boş)
- One Time Use: ❌
- Hold Duration: 0 saniye

Sonuç:
- Item gerekmez
- E tuşuna bas, anında aktif olur
- Birden fazla kez kullanılabilir
```

### Örnek 4: Zamanlı Kol

```
Ayarlar:
- Required Items: (boş)
- One Time Use: ❌
- Hold Duration: 5 saniye
- Rotating Objects: [Kol Transform]
- Rotation Speed: 45
- Rotation Axis: (1, 0, 0)

Sonuç:
- Item gerekmez
- E tuşuna 5 saniye basılı tut
- Kol döner
- Erken bırakırsan iptal olur (fail sesi)
- Tekrar kullanılabilir
```

## Kurulum Adımları

1. **GameObject'e Component Ekle**
   - `GeneralInteractableEvent` component'ini ekle
   - `NetworkObject` olduğundan emin ol
   - Collider ekle (etkileşim için)

2. **Item Gereksinimlerini Ayarla**
   - Required Items listesine ItemData'ları ekle
   - Consume Items'ı işaretle (tüketilsin mi?)

3. **Etkileşim Ayarları**
   - One Time Use: Tek kullanımlık mı?
   - Hold Duration: Kaç saniye basılı tutulacak?

4. **Rotasyon Ayarları**
   - Rotating Objects: Dönecek objeleri ekle
   - Rotation Speed: Dönüş hızı (derece/saniye)
   - Rotation Axis: Dönüş ekseni (X, Y, Z)
   - Continue Rotation: Sonra da dönsün mü?

5. **Animasyon (Opsiyonel)**
   - Animator component'i ata
   - Trigger isimlerini ayarla

6. **Ses Efektleri (Opsiyonel)**
   - AudioClip'leri ata
   - Start, Loop, Success, Fail, Denied

7. **Görsel Ayarlar**
   - Visual Renderer: Renk değişecek mesh
   - Locked/Unlocked/Interacting renkleri

8. **Parçacık Efektleri (Opsiyonel)**
   - Start/Success/Fail particle system'leri

## Eski Sistemden Farklar

### ✅ Avantajlar

1. **Tek Script**: Tüm event tipleri için tek script
2. **Daha Esnek**: Kod yazmadan ayarlama
3. **Daha İyi UX**: İlerleme göstergesi ile basılı tutma
4. **Daha Fazla Özellik**: Animasyon, çoklu ses, parçacıklar
5. **Kolay Kullanım**: Inspector'dan ayarlama
6. **Daha İyi Geri Bildirim**: Tüm durumlar için görsel ve ses
7. **Tekrar Kullanılabilir**: Birçok farklı puzzle tipi için

### 🔄 Eski Scriptleri Değiştirme

**ValveEvent yerine:**
1. `ValveEvent` component'ini kaldır
2. `GeneralInteractableEvent` ekle
3. Vana kolunu Rotating Objects'e ekle
4. İngiliz anahtarını Required Items'a ekle

**PuzzlePanelEvent yerine:**
1. `PuzzlePanelEvent` component'ini kaldır
2. `GeneralInteractableEvent` ekle
3. Panel kapağını Rotating Objects'e ekle
4. Tornavidayı Required Items'a ekle

## Önemli Notlar

- **Network Sync**: Tüm state multiplayer'da senkronize
- **Server Authority**: Server kontrol eder, client'lar sadece görür
- **Progress Feedback**: Oyuncu her zaman ne kadar ilerlediğini görür
- **Cancellable**: E tuşunu bırakınca iptal olur
- **Visual Gizmos**: Editor'da dönüş eksenini gösterir

## Sorun Giderme

### "Already activated" görünüyor ama tekrar kullanılabilir olmalı
**Çözüm:** "One Time Use" işaretini kaldır

### Rotasyon çalışmıyor
**Çözüm:** 
- Rotating Objects listesinde geçerli transform'lar var mı kontrol et
- Rotation Axis (0,0,0) değil mi kontrol et
- Rotation Speed 0 değil mi kontrol et

### Item'lar tüketilmiyor
**Çözüm:** "Consume Items" işaretini koy

### Animasyon oynatılmıyor
**Çözüm:**
- Animator component atandı mı kontrol et
- Trigger isimleri animator parametreleriyle eşleşiyor mu kontrol et

### Sesler çalmıyor
**Çözüm:**
- AudioClip'ler atandı mı kontrol et
- AudioSource ayarları doğru mu kontrol et

## Gelecek Geliştirmeler

Eklenebilecek özellikler:
- Unity Events ile custom callback'ler
- Çoklu basma aşamaları (bas, tut, bırak)
- Yakınlık bazlı otomatik başlatma
- Takım bazlı gereksinimler (birden fazla oyuncu)
- Cooldown sistemi
- Kaynak maliyeti (sadece item değil)

---

**Detaylı İngilizce dokümantasyon için:** `GENERAL_EVENT_SYSTEM.md` dosyasına bakın.

