# Prefab Room System - Hızlı Başlangıç

## 🚀 5 Dakikada İlk Oda Prefabınızı Oluşturun!

### 1. Root GameObject Oluştur (30 saniye)

```
Hierarchy > Sağ tık > Create Empty
İsim: "TestRoom"
Position: (0, 0, 0)

Inspector > Add Component > Room Prefab Manager
Room Name: "Test Room"
```

### 2. Duvar Küpleri Oluştur (2 dakika)

```
Hierarchy > Sağ tık > 3D Object > Cube
İsim: "WallCube_001"
Position: (0, 0, 0)
Scale: (1, 1, 1)

Inspector > Add Component > Wall Marker

Kopyala (Ctrl+D) ve yerleştir:
- WallCube_002 → (1, 0, 0)
- WallCube_003 → (2, 0, 0)
- WallCube_004 → (3, 0, 0)
- WallCube_005 → (4, 0, 0)
... (20 küp oluştur, 5x4 duvar)

Hepsini TestRoom'un child'ı yap (sürükle)
```

### 3. Marker'ları Topla (10 saniye)

```
TestRoom seç > Inspector > RoomPrefabManager
Sağ tık > Collect All Markers From Children

✅ Wall Markers: 20 marker bulundu!
```

### 4. Prefab Olarak Kaydet (20 saniye)

```
TestRoom'u Project'e sürükle
Konum: Assets/Prefabs/Rooms/
İsim: TestRoom.prefab
```

### 5. RoomConfiguration'a Ata (30 saniye)

```
Assets/Resources/RoomConfiguration.asset aç

Room Prefab System:
└─ Room Prefab: TestRoom

Kaydet (Ctrl+S)
```

### 6. Test Et! (1 dakika)

```
Play butonuna bas
Oda oluşturulacak
Marker'ların button'a dönüştüğünü gör! 🎉
```

## 🎯 Sonuç

✅ İlk oda prefabınız hazır!
✅ Marker'lar button'a dönüşüyor!
✅ Random placement çalışıyor!

**Sonraki Adım:** Daha büyük bir oda oluştur (10x10, 80 marker)

## 📚 Detaylı Bilgi

Daha fazla bilgi için: `PREFAB_ROOM_SYSTEM_GUIDE.md`

