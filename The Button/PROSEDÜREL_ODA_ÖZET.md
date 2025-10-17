# Prosedürel Oda Sistemi - Özet

## ✅ Tamamlanan İşlemler

### 1. Script'ler Oluşturuldu

#### Ana Script'ler
- **RoomConfiguration.cs**: Oda boyutu, materyal ve ayarları tutan ScriptableObject
- **RoomItemPool.cs**: Spawn olabilecek itemları yöneten ScriptableObject  
- **ProceduralRoomGenerator.cs**: Odayı prosedürel olarak üreten ana sistem

#### Güncellenen Script'ler
- **SpawnButton.cs**: `SetItemData()` metodu eklendi (prosedürel kurulum için)
- **NetworkManagerSetup.cs**: Oyun başladığında oda üretimi entegre edildi
- **GameManager.cs**: Restart'ta yeni oda üretimi eklendi

### 2. Sistem Nasıl Çalışıyor?

```
1. Host "Start Game" butonuna basar
2. GameRoom scene yüklenir
3. ProceduralRoomGenerator otomatik çalışır
4. Sistem şunları üretir:
   ├─ Zemin (küpler, button yok)
   ├─ Tavan (küpler, button yok)
   ├─ 4 Duvar (her küpte potansiyel button pozisyonu)
   ├─ Zorunlu buttonlar yerleştirilir (anahtar vb.)
   └─ Rastgele buttonlar geri kalan yerlere yerleştirilir
5. Oyuncular oda merkezinde spawn olur
6. Oyun başlar
```

### 3. Network Senkronizasyonu

- **Seed**: Tüm clientlarda aynı odanın üretilmesi için NetworkVariable kullanıldı
- **Buttonlar**: Her button bir NetworkObject olarak server tarafından spawn ediliyor
- **Deterministik**: Aynı seed = tüm oyuncularda aynı oda

### 4. Özellikler

✅ Her oyunda farklı oda
✅ Duvarlar küplerden oluşuyor
✅ Her küpte button olabilir
✅ Buttonlar rastgele item spawn ediyor
✅ Zorunlu itemlar her odada garantili (anahtar, kapı vb.)
✅ Multiplayer'da tüm oyuncular aynı odayı görüyor
✅ Game restart'ta yeni oda üretiliyor

## 🎮 Unity'de Yapılması Gerekenler

### ÖNEMLİ: Asset'leri Unity Editor'de Oluşturmalısınız

Kod hazır ama Unity asset'lerini elle oluşturmanız gerekiyor:

### 1. WallButton Prefab Oluştur

**Konum**: `Assets/Prefabs/Item Prefabs/WallButton.prefab`

**Adımlar**:
1. `Assets/Toon Suburban Pack/Button.fbx` dosyasını scene'e sürükle
2. Şu component'leri ekle:
   - **NetworkObject** (Unity Netcode)
   - **SpawnButton** script
   - **BoxCollider** (trigger değil!)
3. SpawnButton ayarları:
   - `itemToSpawn`: Boş bırak (kod set ediyor)
   - `spawnPoint`: Boş bırak (kod set ediyor)
   - `cooldownTime`: 5
   - `buttonRenderer`: MeshRenderer'ı ata
   - Renkler: Yeşil (normal), Kırmızı (cooldown), Sarı (pressed)
4. Scale: (0.3, 0.3, 0.2) - duvara uygun boyut
5. Prefab olarak kaydet
6. **ÇOK ÖNEMLİ**: NetworkManager'ın NetworkPrefabs listesine ekle!

### 2. RoomItemPool Asset Oluştur

**Konum**: `Assets/Resources/RoomItemPool.asset`

**Adımlar**:
1. Project'te sağ tıkla: `Create > The Button > Room Item Pool`
2. Inspector'da ayarla:
   - **Required Items**: Zorunlu itemlar (anahtar vb.) - şimdilik boş bırakabilirsin
   - **Random Item Pool**: Rastgele spawn olacak itemlar
     - Chair.asset
     - Lamp.asset  
     - Stair.asset
     - Tv.asset
     - (İstediğin kadar ekleyebilirsin)

### 3. DefaultRoomConfiguration Asset Oluştur

**Konum**: `Assets/Resources/DefaultRoomConfiguration.asset`

**Adımlar**:
1. Project'te sağ tıkla: `Create > The Button > Room Configuration`
2. Inspector'da ayarla:
   - **Room Dimensions**:
     - Room Width: 15
     - Room Height: 10
     - Room Depth: 15
     - Cube Size: 1
   - **Structure Prefabs**:
     - **Button Prefab**: WallButton prefab'ı ata (ÖNEMLİ!)
     - Diğerleri opsiyonel
   - **Button Generation**:
     - Button Density: 0.3 (duvarların %30'unda button)
     - Min Random Buttons: 10
     - Max Random Buttons: 30
   - **Spawn Settings**:
     - Player Spawn Offset: (0, 1, 0)
     - Item Spawn Offset: 0.5

### 4. GameRoom Scene'e ProceduralRoomGenerator Ekle

**Adımlar**:
1. `Assets/Scenes/GameRoom.unity` scene'ini aç
2. Boş GameObject oluştur: `ProceduralRoomGenerator`
3. Component'leri ekle:
   - **NetworkObject**
   - **ProceduralRoomGenerator** script
4. Inspector'da ayarla:
   - **Room Config**: DefaultRoomConfiguration asset'ini ata
   - **Item Pool**: RoomItemPool asset'ini ata
   - **Show Debug Logs**: ✓ (test için)
5. Scene'i kaydet

### 5. NetworkManager Kontrolü

1. GameRoom scene'inde NetworkManager'ı bul
2. NetworkPrefabs listesini kontrol et:
   - WallButton prefab listede olmalı
   - Yoksa manuel olarak ekle

## 🧪 Test Etme

### Tek Oyunculu Test
1. Unity'de Play'e bas
2. Host olarak lobby oluştur
3. "Start Game" butonuna bas
4. Oda üretilmeli:
   - Zemin ve tavan görünmeli
   - 4 duvar küplerden oluşmalı
   - Duvarlarda buttonlar olmalı
5. Button'a yaklaş, E'ye bas
6. Item spawn olmalı

### Çok Oyunculu Test
1. Build al veya iki Unity instance aç
2. Biri host, diğeri client olsun
3. Aynı lobby'e katıl
4. Host start game desin
5. **İki oyuncu da aynı odayı görmeli**
6. Buttonlar her iki tarafta da çalışmalı

## 📊 Sistem Parametreleri

### Oda Boyutları (Değiştirilebilir)
- **Genişlik**: 15 küp (1-30 arası)
- **Yükseklik**: 10 küp (3-15 arası)  
- **Derinlik**: 15 küp (1-30 arası)
- **Küp Boyutu**: 1 metre

### Button Ayarları
- **Density**: 0.3 (duvarların %30'unda button)
- **Min Buttons**: 10
- **Max Buttons**: 30
- **Cooldown**: 5 saniye

### Item Pool
- **Required Items**: Her odada olması gereken itemlar
- **Random Pool**: Rastgele seçilebilecek itemlar

## 🔧 Özelleştirme

### Oda Boyutunu Değiştir
`DefaultRoomConfiguration` asset'inde:
- `roomWidth`, `roomHeight`, `roomDepth` değerlerini değiştir

### Daha Fazla Button
`DefaultRoomConfiguration` asset'inde:
- `buttonDensity`: 0.5'e çıkar (%50 button)
- `maxRandomButtons`: 50'ye çıkar

### Zorunlu Item Ekle (Anahtar vb.)
1. Item'ın ItemData asset'ini oluştur
2. `RoomItemPool` asset'inde `requiredItems` listesine ekle
3. Her odada garantili spawn olacak

### Farklı Button Modeli Kullan
1. Kendi 3D modelini import et
2. NetworkObject ve SpawnButton ekle
3. `DefaultRoomConfiguration.buttonPrefab`'a ata

## 📁 Oluşturulan Dosyalar

### Yeni Script'ler
- ✅ `Assets/Scripts/Game/RoomConfiguration.cs`
- ✅ `Assets/Scripts/Game/RoomItemPool.cs`
- ✅ `Assets/Scripts/Game/ProceduralRoomGenerator.cs`

### Güncellenen Script'ler  
- ✅ `Assets/Scripts/Interactables/SpawnButton.cs`
- ✅ `Assets/Scripts/Network/NetworkManagerSetup.cs`
- ✅ `Assets/Scripts/Game/GameManager.cs`

### Unity'de Oluşturulacak Asset'ler (Manuel)
- ⚠️ `Assets/Prefabs/Item Prefabs/WallButton.prefab`
- ⚠️ `Assets/Resources/RoomItemPool.asset`
- ⚠️ `Assets/Resources/DefaultRoomConfiguration.asset`
- ⚠️ GameRoom scene'ine ProceduralRoomGenerator eklenmeli

## 📚 Dokümantasyon

Detaylı İngilizce setup guide:
- `PROCEDURAL_ROOM_SETUP.md` (adım adım talimatlar)

## ⚠️ Önemli Notlar

1. **NetworkPrefabs**: WallButton prefab'ı mutlaka NetworkManager'a ekle!
2. **Resources Klasörü**: Asset'ler `Assets/Resources/` altında olmalı
3. **Scene'de Generator**: ProceduralRoomGenerator GameRoom scene'inde olmalı
4. **ItemSpawner**: GameRoom'da ItemSpawner GameObject'i olmalı
5. **Test**: Önce tek oyunculu, sonra çok oyunculu test et

## 🐛 Sorun Giderme

### Oda oluşmuyor
- Console'da hata mesajlarını kontrol et
- RoomConfiguration ve RoomItemPool atandığını kontrol et
- ProceduralRoomGenerator scene'de veya spawn olabiliyor mu kontrol et

### Buttonlar çalışmıyor
- SpawnButton script button prefab'da mı?
- BoxCollider trigger değil mi?
- NetworkObject var mı?
- NetworkPrefabs listesinde mi?

### "Failed to spawn NetworkObject" hatası
- WallButton prefab'ı NetworkPrefabs listesine ekle
- Tüm prefab'lar NetworkObject component'ine sahip mi kontrol et

### İtemler zemine düşüyor
- ItemData.itemPrefab'da Rigidbody ve Collider var mı?
- Zemin collider'ları doğru mu?

## 🎯 Sonraki Adımlar

1. ✅ Asset'leri Unity Editor'de oluştur (yukarıdaki adımları takip et)
2. ✅ Tek oyunculu test et
3. ✅ Çok oyunculu test et
4. 🔲 Zorunlu itemlar ekle (anahtar, kapı vb.)
5. 🔲 Görsel özelleştirme (materyal, model)
6. 🔲 Gameplay dengeleme (oda boyutu, button sayısı)

## 💡 İpuçları

- Küçük bir oda ile başla (10x8x10) - test daha hızlı
- Debug log'ları aç, console'u izle
- Önce tek oyunculu test et, sorun yoksa multiplayer dene
- Button density'yi düşük tut (0.2-0.3), çok button performansı düşürür
- Required items listesini sonra doldur, önce sistemi çalıştır

---

**Durum**: Kod tamam ✅ | Unity setup gerekli ⚠️  
**Test**: Önce Unity'de asset'leri oluştur, sonra test et

