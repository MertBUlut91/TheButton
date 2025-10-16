# Oyun Mekanikleri - Implementasyon Tamamlandı! 🎉

## ✅ Tamamlanan Scriptler

Tüm oyun mekanikleri scriptleri başarıyla oluşturuldu. İşte eklenen dosyalar:

### Item Sistemi (5 dosya)
1. ✅ `Assets/Scripts/Items/ItemType.cs` - Item türleri enum
2. ✅ `Assets/Scripts/Items/ItemData.cs` - ScriptableObject item tanımı
3. ✅ `Assets/Scripts/Items/ItemDatabase.cs` - Item veritabanı yöneticisi
4. ✅ `Assets/Scripts/Items/WorldItem.cs` - Dünyada görünen item (NetworkObject)
5. ✅ `Assets/Scripts/Items/ItemSpawner.cs` - Server-side item spawning
6. ✅ `Assets/Scripts/Items/ItemSpawnPoint.cs` - Spawn noktası marker

### Interaktif Sistemler (3 dosya)
7. ✅ `Assets/Scripts/Interactables/IInteractable.cs` - Interactable interface
8. ✅ `Assets/Scripts/Interactables/SpawnButton.cs` - Item spawn butonu
9. ✅ `Assets/Scripts/Interactables/ExitDoor.cs` - Çıkış kapısı

### Player Güncellemeleri (2 dosya)
10. ✅ `Assets/Scripts/Player/PlayerInteraction.cs` - Raycast interaction sistemi
11. ✅ `Assets/Scripts/Player/PlayerInventory.cs` - ✨ Güncellendi (item kullanım logic)
12. ✅ `Assets/Scripts/Player/PlayerNetwork.cs` - ✨ Güncellendi (death detection, reset)

### Game Management (1 dosya)
13. ✅ `Assets/Scripts/Game/GameManager.cs` - Oyun durumu yönetimi

### UI Güncellemeleri (3 dosya)
14. ✅ `Assets/Scripts/UI/InventoryUI.cs` - ✨ Güncellendi (icon gösterimi)
15. ✅ `Assets/Scripts/UI/InteractionPromptUI.cs` - Interaction prompts
16. ✅ `Assets/Scripts/UI/GameStateUI.cs` - Win/Lose ekranları

**Toplam**: 16 yeni/güncellenmiş script dosyası

---

## 🎮 Özellikler

### ✨ Yeni Özellikler

1. **Item Sistemi**
   - ScriptableObject tabanlı item database
   - 5 item tipi: Key, Medkit, Food, Water, Hazard
   - Her item kendi icon, color ve özelliklerine sahip
   - Network synchronized pickup ve usage

2. **Interaktif Butonlar**
   - Duvarlara yerleştirilebilir spawn butonları
   - Cooldown sistemi (spam engelleme)
   - Visual feedback (renk değişimi)
   - Her buton farklı item spawn edebilir

3. **Item Spawning**
   - Server-authoritative spawning
   - Designated spawn points
   - Networked item replication

4. **Item Kullanımı**
   - Medkit → Health restore
   - Food → Hunger restore
   - Water → Thirst restore
   - Key → Door unlock
   - Hazard → Damage

5. **Çıkış Kapısı**
   - Key ile açılabilir
   - Visual feedback (kırmızı/yeşil)
   - Kapıdan geçince oyunu kazanma

6. **Oyun Durumu Yönetimi**
   - Win/Lose koşulları
   - Player death detection
   - Game restart
   - Return to lobby

7. **UI Geliştirmeleri**
   - Inventory item iconları
   - Interaction prompts ("Press E to...")
   - Win/Lose ekranları
   - Game timer (opsiyonel)

---

## 📋 Unity Editor'de Yapılması Gerekenler

Scriptler hazır ancak Unity Editor'de manuel kurulum yapmanız gerekiyor:

### 1. Item Database Oluşturma (15 dakika)

#### 1.1 Resources Klasörü
```
Assets/
└── Resources/
    └── ItemDatabase.asset  (buraya oluşturacaksınız)
```

1. `Assets/` altında `Resources` klasörü oluşturun
2. Resources içinde sağ tık → `Create > TheButton > Item Database`
3. İsim: `ItemDatabase`

#### 1.2 Item Data'ları Oluşturma

`Assets/Items/` klasörü oluşturun, içine itemleri ekleyin:

**Item 0 - Key**
- Sağ tık → `Create > TheButton > Item Data`
- Item ID: `0`
- Item Name: `Key`
- Item Type: `Key`
- Icon: (sarı bir icon sprite)
- Item Color: Yellow

**Item 1 - Medkit**
- Item ID: `1`
- Item Name: `Medkit`
- Item Type: `Medkit`
- Health Restore: `50`
- Icon: (yeşil bir icon sprite)
- Item Color: Green

**Item 2 - Food**
- Item ID: `2`
- Item Name: `Food`
- Item Type: `Food`
- Hunger Restore: `40`
- Icon: (turuncu bir icon sprite)
- Item Color: Orange

**Item 3 - Water**
- Item ID: `3`
- Item Name: `Water`
- Item Type: `Water`
- Thirst Restore: `40`
- Icon: (mavi bir icon sprite)
- Item Color: Cyan

**Item 4 - Hazard**
- Item ID: `4`
- Item Name: `Poison`
- Item Type: `Hazard`
- Damage Amount: `30`
- Icon: (kırmızı bir icon sprite)
- Item Color: Red

#### 1.3 Database'e Ekle

1. `ItemDatabase.asset`'i aç
2. `Items` listesine yukarıda oluşturduğunuz 5 item'i sürükle
3. Save

---

### 2. World Item Prefab Oluşturma (10 dakika)

1. **Hierarchy'de yeni GameObject**:
   - Sağ tık → `3D Object > Cube`
   - İsim: `WorldItem`

2. **Components ekle**:
   - `Network Object` (Netcode)
   - `WorldItem` script (bizim)
   - `Box Collider` → `Is Trigger = true`
   - `Mesh Renderer` (otomatik gelir)

3. **Visual Ayarları**:
   - Scale: `(0.5, 0.5, 0.5)`
   - Yeni material oluştur (parlayan, emissive)

4. **WorldItem script ayarları**:
   - Mesh Renderer → Cube'un renderer'ını ata
   - Rotation Speed: `50`
   - Bob Speed: `2`
   - Bob Height: `0.2`

5. **Prefab yap**:
   - `Assets/Prefabs/` klasörüne sürükle
   - Hierarchy'den sil

---

### 3. GameRoom Scene Setup (30 dakika)

#### 3.1 GameManager GameObject

1. Hierarchy'de boş GameObject oluştur: `GameManager`
2. Components ekle:
   - `Network Object`
   - `GameManager` script
3. Settings:
   - Game Time Limit: `600` (10 dakika, 0 = limit yok)

#### 3.2 ItemSpawner GameObject

1. Boş GameObject: `ItemSpawner`
2. Components:
   - `Network Object`
   - `ItemSpawner` script
3. Settings:
   - Item Prefab → `WorldItem` prefab'ını ata

#### 3.3 Spawn Points Oluşturma (5 adet)

1. Boş GameObject: `SpawnPoint1`
2. Component ekle: `ItemSpawnPoint` script
3. Settings:
   - Spawn Point ID: `1`
   - Gizmo Color: Yellow
4. Odanın merkezine yakın bir yere yerleştir
5. 4 tane daha oluştur (SpawnPoint2-5)

#### 3.4 Spawn Buttons Oluşturma (5 adet)

Her buton için:

1. **Cube oluştur**: `SpawnButton_Medkit`
2. **Components**:
   - `Network Object`
   - `SpawnButton` script
   - `Box Collider` (trigger DEĞİL)
3. **SpawnButton Settings**:
   - Item ID To Spawn: `1` (Medkit)
   - Spawn Point: `SpawnPoint1`'i ata
   - Cooldown Time: `5`
   - Button Renderer: Cube'un renderer'ını ata
   - Normal Color: Green
   - Cooldown Color: Red
   - Pressed Color: Yellow
4. **Konumlandır**: Duvara yerleştir
5. **Scale**: `(0.5, 0.5, 0.2)` gibi düz bir buton

Diğer 4 buton için tekrarla:
- `SpawnButton_Food` → Item ID: `2`, SpawnPoint2
- `SpawnButton_Water` → Item ID: `3`, SpawnPoint3
- `SpawnButton_Key` → Item ID: `0`, SpawnPoint4
- `SpawnButton_Hazard` → Item ID: `4`, SpawnPoint5

#### 3.5 Exit Door

1. **Cube oluştur**: `ExitDoor`
2. **Components**:
   - `Network Object`
   - `ExitDoor` script
   - `Box Collider` (trigger DEĞİL)
3. **ExitDoor Settings**:
   - Start Locked: `true`
   - Door Renderer: Cube'un renderer'ını ata
   - Locked Color: Red
   - Unlocked Color: Green
4. **Scale**: `(2, 3, 0.2)` - Kapı gibi
5. **Konumlandır**: Odanın bir köşesine

#### 3.6 Player Prefab'a PlayerInteraction Ekle

1. `Assets/Prefabs/Player.prefab`'ı aç
2. `PlayerInteraction` script ekle
3. Settings:
   - Interaction Range: `3`
   - Interact Key: `E`
   - Camera Transform: `PlayerCamera`'yı ata (otomatik bulur ama elle de atabilirsiniz)

---

### 4. GameRoom UI Setup (20 dakika)

GameRoom scene'deki Canvas'a (veya yeni Canvas oluştur):

#### 4.1 Interaction Prompt

1. **Panel oluştur**: `InteractionPromptPanel`
2. **Position**: Ekranın üst ortasında
3. **TextMeshPro ekle**: `PromptText`
   - Font Size: `24`
   - Alignment: Center
   - Text: "Press E to interact"
4. **InteractionPromptUI script ekle** (Panel'e)
5. **Referansları bağla**:
   - Prompt Text: `PromptText`
   - Prompt Container: `InteractionPromptPanel`

#### 4.2 Game State UI

Mevcut Canvas'a veya yeni GameObject'e:

1. **GameStateUI script ekle** (Canvas'a)

2. **Win Panel oluştur**:
   - Panel: `WinPanel`
   - Background: Yeşil-yarı transparan
   - TextMeshPro: "You Won!"
   - 2 Button: "Restart", "Return to Lobby"

3. **Lose Panel oluştur**:
   - Panel: `LosePanel`
   - Background: Kırmızı-yarı transparan
   - TextMeshPro: "Game Over!"
   - 2 Button: "Restart", "Return to Lobby"

4. **Timer Text** (opsiyonel):
   - TextMeshPro: "Time: 10:00"
   - Position: Sağ üst köşe

5. **GameStateUI Referansları**:
   - Win Panel: `WinPanel`
   - Lose Panel: `LosePanel`
   - HUD Panel: Mevcut stats panel'i
   - Win Message Text: Win panel'deki text
   - Restart Button: Win panel'deki restart button
   - Return To Lobby Button: Win panel'deki lobby button
   - Lose Message Text: Lose panel'deki text
   - Lose Restart Button: Lose panel'deki restart button
   - Lose Return To Lobby Button: Lose panel'deki lobby button
   - Timer Text: Timer text (opsiyonel)
   - Show Timer: `true/false`

---

### 5. NetworkManager Prefab List Güncelleme

MainMenu scene'deki NetworkSetup'ta:

1. **NetworkManager** component'ini aç
2. **Network Prefabs** listesine ekle:
   - `WorldItem` prefab
   - `SpawnButton` prefab'ları (eğer prefab yaptıysanız)
   - `ExitDoor` prefab (eğer prefab yaptıysanız)

**Not**: Scene'de direkt bulunan NetworkObject'ler otomatik register olur, ancak runtime'da spawn edeceğiniz prefab'ları eklemek zorundasınız (WorldItem gibi).

---

## 🧪 Test Etme

### Tek Oyuncu Test

1. GameRoom scene'ini aç
2. Play'e bas
3. Test et:
   - ✅ Butona bakınca "Press E" göstersin
   - ✅ E'ye basınca item spawn olsun
   - ✅ Item'a yaklaş, otomatik envantere alınsın
   - ✅ 1-5 tuşlarıyla item kullan
   - ✅ Stats değişsin (medkit, food, water)
   - ✅ Kapıya bak, "needs Key" desin
   - ✅ Key topla ve kullan
   - ✅ Kapıya bak, "Exit and Win" desin
   - ✅ E'ye bas, Win ekranı göster

### Multiplayer Test

1. Build yap veya Multiplayer Play Mode kullan
2. 2 client başlat
3. Test et:
   - ✅ Bir player buton basar
   - ✅ Diğer player item'i görür
   - ✅ İlk alan alır, diğeri alamaz
   - ✅ Item kullanımı her iki tarafta da görünsün
   - ✅ Kapı unlock her iki tarafta da görünsün
   - ✅ Bir player kazanınca her iki taraf da Win ekranı görsün

---

## 🎨 İyileştirme Önerileri

### Görsel İyileştirmeler
- Item'lara particle effect ekle (parlama)
- Butonlara animasyon ekle (basılınca içeri girmesi)
- Kapıya animasyon ekle (açılırken yana kayması)
- Item icon'lar için sprite'lar oluştur/bul

### Ses Efektleri
- Button press sound
- Item pickup sound
- Door unlock sound
- Win/lose sound
- Item usage sound

### Ek Özellikler
- Item drop (envanterden dünyaya atma)
- Item tooltips (mouse hover'da bilgi)
- Inventory drag & drop
- Drop noktalarına parça efekti
- Hazard item'da particle effect

---

## 📝 Önemli Notlar

### Server Authority
- Tüm gameplay logic server-side
- Client'lar sadece input gönderir
- State'ler server'dan sync edilir

### Network Object'ler
- Her spawned item NetworkObject olmalı
- NetworkManager prefab listesinde olmalı
- Server tarafında spawn edilmeli

### Test Sırası
1. Önce tek oyuncu test et
2. Sonra multiplayer test et
3. Her adımda console log'ları kontrol et

### Debugging
- `[ItemSpawner]`, `[WorldItem]`, `[Inventory]` gibi log prefix'leri var
- Unity Console'da filter kullan
- NetworkObject'lerin spawn/despawn'ını takip et

---

## 🚀 Sonraki Adımlar

1. **Unity Editor'de Setup** (yukarıdaki adımları takip et)
2. **Test Et** (tek ve multiplayer)
3. **Polish** (görsel, ses, animasyonlar)
4. **Ek Özellikler** (istediğin mekanikleri ekle)

---

**Tebrikler!** Oyununuzun temel mekanikleri artık hazır. Unity Editor'de setup'ı tamamladıktan sonra tamamen oynanabilir bir multiplayer survival oyununuz olacak! 🎮🎉

Herhangi bir sorun yaşarsanız console log'ları kontrol edin. Her script yeterli debug mesajları içeriyor.

İyi oyunlar! 🎯

