# Silah Sistemi Kılavuzu

## Genel Bakış
Silah sistemi, oyuncuların envanterlerinden silah donatıp kullanmalarını sağlar. Oyuncular 1-5 tuşlarıyla silah seçebilir ve sol tıklama ile saldırabilirler. Silahlar diğer oyunculara hasar verir ve damage, menzil, saldırı hızı gibi özelleştirilebilir özelliklere sahiptir.

## Özellikler
- ✅ Item sistemine Weapon kategorisi eklendi
- ✅ 6 silah tipi: Pistol, Rifle, Shotgun, Knife, Bat, Axe
- ✅ Envanter slotu seçildiğinde otomatik silah donatma
- ✅ Sol tık ile saldırı
- ✅ Yakın dövüş (melee) ve menzilli silah desteği
- ✅ PlayerNetwork ile entegre hasar sistemi
- ✅ Saldırı cooldown sistemi
- ✅ Elde görsel silah modelleri
- ✅ Network senkronize saldırılar

## Kullanım

### Oyuncular İçin
1. **Silah toplama**: Dünyadaki bir silahla etkileşime geç (E tuşu)
2. **Silah seçme**: Silahın bulunduğu envanter slotunu seçmek için 1-5 tuşlarına bas
3. **Otomatik donatma**: Silah otomatik olarak eline gelir
4. **Saldırı**: Sol tık ile saldır
5. **Silah değiştirme**: Başka bir numara tuşuna (1-5) basarak değiştir
6. **Silah bırakma**: Seçili silahı bırakmak için Q tuşuna bas

### Kontroller
- **1-5 Tuşları**: Envanter slotu seç (silah varsa otomatik donatır)
- **Sol Fare Tuşu**: Donatılmış silahla saldır
- **Q**: Seçili itemi bırak
- **E**: Kullan/etkileşim (silah olmayanlar için)

### Saldırı Sistemi
- **Yakın Dövüş Silahları (Melee)**: 
  - Kısa menzil (2-3 metre)
  - Anlık vuruş tespiti
  - Hızlı saldırı hızı
  - Yakın dövüş için iyi

- **Menzilli Silahlar (Ranged)**:
  - Uzun menzil (10-50 metre)
  - Anlık vuruş tespiti (hitscan)
  - Daha yavaş saldırı hızı
  - Mesafeli dövüş için iyi

## Kurulum Talimatları

### 1. PlayerWeaponSystem Komponenti Ekle
Player prefab'ına `PlayerWeaponSystem` komponentini ekle:
1. Player prefab'ını seç
2. Add Component → TheButton → Player → PlayerWeaponSystem
3. Komponent PlayerInventory, PlayerNetwork ve Camera'yı otomatik bulacak

### 2. Silah ItemData Oluştur
Yeni bir silah oluşturmak için:
1. Project'te sağ tık → Create → TheButton → Item Data
2. Şu özellikleri ayarla:
   - **Item Name**: örn. "Pistol"
   - **Category**: Weapon
   - **Item Type**: Pistol, Rifle, Shotgun, Knife, Bat, Axe'den birini seç
   - **Weapon Damage**: 1-100 (önerilen: 10-30 denge için)
   - **Attack Range**: 
     - Melee: 2-3 metre
     - Ranged: 10-50 metre
   - **Attack Speed**: 
     - Hızlı: 0.3-0.5 saniye
     - Orta: 0.8-1.2 saniye
     - Yavaş: 1.5-2.5 saniye
   - **Is Melee Weapon**: Melee için işaretle, ranged için işareti kaldır
   - **Hand Model**: Silah 3D model prefab'ını ata
   - **Item Prefab**: Dünya item prefab'ını ata (bırakma için)

### 3. Örnek Silah Konfigürasyonları

#### Pistol (Menzilli)
- Damage: 15
- Range: 30
- Attack Speed: 0.5
- Is Melee: false

#### Rifle (Menzilli)
- Damage: 25
- Range: 50
- Attack Speed: 1.0
- Is Melee: false

#### Shotgun (Menzilli)
- Damage: 40
- Range: 15
- Attack Speed: 2.0
- Is Melee: false

#### Knife (Yakın Dövüş)
- Damage: 10
- Range: 2
- Attack Speed: 0.3
- Is Melee: true

#### Bat (Yakın Dövüş)
- Damage: 20
- Range: 2.5
- Attack Speed: 0.8
- Is Melee: true

#### Axe (Yakın Dövüş)
- Damage: 35
- Range: 2.5
- Attack Speed: 1.5
- Is Melee: true

## Teknik Detaylar

### Değiştirilen Dosyalar
1. **ItemCategory.cs**: `Weapon` kategorisi eklendi
2. **ItemType.cs**: 6 silah tipi eklendi (Pistol, Rifle, Shotgun, Knife, Bat, Axe)
3. **ItemData.cs**: Silah özellikleri eklendi (weaponDamage, attackRange, attackSpeed, isMeleeWeapon)
4. **PlayerWeaponSystem.cs**: YENİ - Silah donatma ve saldırı sistemi
5. **PlayerItemUsage.cs**: 5. slot desteği eklendi (Alpha5 tuşu)
6. **PlayerInventory.cs**: Weapon kategorisi için kullanım mantığı eklendi

### Yeni Özellikler
```csharp
// ItemData'ya eklenen özellikler
public float weaponDamage = 10f;        // Saldırı başına hasar
public float attackRange = 2f;          // Saldırı menzili (metre)
public float attackSpeed = 1f;          // Saldırılar arası süre (saniye)
public bool isMeleeWeapon = true;       // Melee mi Ranged mi
```

### Network Senkronizasyonu
Silah sistemi tamamen network senkronize:
- ✅ Silah donatma yerel (sadece görsel)
- ✅ Saldırılar sunucuda doğrulanır
- ✅ Hasar sunucu tarafında uygulanır
- ✅ Vuruş efektleri tüm clientlara gösterilir
- ✅ Envanter değişiklikleri senkronize

## Sorun Giderme

### Silah Donatılmıyor
1. Player prefab'ında PlayerWeaponSystem komponentinin olduğunu kontrol et
2. ItemData'nın category'sinin "Weapon" olduğunu doğrula
3. Envanter slotunda silah item'ı olduğundan emin ol
4. Console'da hata mesajlarını kontrol et

### Saldırılar Çalışmıyor
1. Silahın donatıldığını doğrula (console loglarını kontrol et)
2. Saldırı cooldown'unu kontrol et (attackSpeed süresi kadar bekle)
3. Hedefin saldırı menzilinde olduğundan emin ol
4. PlayerWeaponSystem'daki LayerMask ayarlarını kontrol et

### Hasar Uygulanmıyor
1. Hedefin PlayerNetwork komponentine sahip olduğunu doğrula
2. Hedefin NetworkObject olduğunu kontrol et
3. Sunucunun çalıştığından emin ol (host/dedicated server)
4. Sunucu tarafı logları için console'u kontrol et

### Silah Modeli Görünmüyor
1. ItemData'da handModel'i ata
2. handModel prefab'ının renderer'lara sahip olduğunu kontrol et
3. WeaponHolder'ın düzgün konumlandırıldığını doğrula
4. PlayerWeaponSystem'daki camera referansını kontrol et

## Özet

Silah sistemi artık tamamen uygulandı ve kullanıma hazır! Oyuncular:
1. Dünyadan silah toplayabilir
2. Silahları envanterinde saklayabilir (5 slot)
3. 1-5 tuşlarıyla silah seçebilir (otomatik donatma)
4. Sol tık ile saldırabilir
5. Diğer oyunculara hasar verebilir
6. Q tuşu ile silah bırakabilir

Tüm silah özellikleri ItemData ScriptableObject'leri üzerinden yapılandırılabilir, bu da kod değişikliği olmadan farklı silahlar oluşturmayı ve dengelemeyi kolaylaştırır.

## Gelecek Geliştirmeler

Olası eklemeler:
- 🔲 Menzilli silahlar için mermi sistemi
- 🔲 Silah dayanıklılığı/kırılma
- 🔲 Silah eklentileri/yükseltmeler
- 🔲 Farklı saldırı animasyonları
- 🔲 Kritik vuruşlar/headshot'lar
- 🔲 Silah geri tepmesi ve yayılma
- 🔲 Şarjör doldurma mekanikleri
- 🔲 Silah ses efektleri
- 🔲 Namlu alevi efektleri
- 🔲 Menzilli silahlar için mermi izleri
- 🔲 Vuruş işaretleri ve hasar göstergeleri
- 🔲 Silah yapım sistemi

