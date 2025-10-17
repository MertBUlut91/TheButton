# Mesh-Based Placement System

## 📋 Genel Bakış

Oyunda artık objeleri yerleştirirken **mesh tabanlı çakışma kontrolü** yapılmaktadır. Bu sistem, objenin gerçek collider geometrisini kullanarak başka objelerle çakışıp çakışmadığını kontrol eder.

## 🎮 Nasıl Çalışır?

### Önceki Sistem (Basit Raycast)
- ❌ Sadece kameranın baktığı yere raycast gönderir
- ❌ Yere temas varsa yeşil, yoksa kırmızı
- ❌ Objenin diğer objelerle çakışmasını kontrol etmez
- ❌ Objeler üst üste yerleştirilebilir

### Yeni Sistem (Mesh-Based)
- ✅ Objenin gerçek collider'larını kullanır (Box, Sphere, Capsule, Mesh)
- ✅ Başka objelerle çakışma olup olmadığını kontrol eder
- ✅ Her collider tipine özel optimizasyon yapar
- ✅ Minimum mesafe (clearance) ayarı ile objelerin birbirine yaklaşmasını engeller
- ✅ İki mod: Mesh tabanlı veya basit raycast

## 🔧 Unity Inspector Ayarları

`PlayerItemUsage` component'inde aşağıdaki ayarları bulabilirsin:

### Placement Settings

| Ayar | Açıklama | Varsayılan |
|------|----------|------------|
| **Placement Distance** | Objeyi kameradan ne kadar uzağa yerleştirebileceğin | 3m |
| **Placement Layer Mask** | Raycast için hangi layer'ları kontrol edecek (zemin vb.) | Everything |
| **Collision Check Layer Mask** | Çakışma kontrolü için hangi layer'ları kontrol edecek | Everything |
| **Use Mesh Collision Check** | Mesh tabanlı çakışma kontrolünü aktif et | ✅ True |
| **Minimum Clearance** | Objeler arası minimum mesafe (metre) | 0.1m |
| **Valid Placement Color** | Yerleştirilebilir olduğunda gösterilen renk | Yeşil (0.5 alpha) |
| **Invalid Placement Color** | Yerleştirilemez olduğunda gösterilen renk | Kırmızı (0.5 alpha) |

## 🎯 Collider Tipleri Desteği

Sistem farklı collider tiplerini otomatik olarak algılar:

### 1. BoxCollider
```csharp
// Box collider için OverlapBox kullanır
// Objenin scale'ini dikkate alır
// Clearance miktarı kadar küçültür
```

### 2. SphereCollider
```csharp
// Sphere collider için OverlapSphere kullanır
// En büyük scale değerini radius için kullanır
// Clearance miktarı kadar radius'u azaltır
```

### 3. CapsuleCollider
```csharp
// Capsule collider için OverlapCapsule kullanır
// Height ve radius'u scale'e göre hesaplar
// İki nokta arasında capsule kontrolü yapar
```

### 4. MeshCollider
```csharp
// Mesh collider için bounds-based OverlapBox kullanır
// Performans için approximation yapar
// Karmaşık mesh'ler için yeterli doğruluk sağlar
```

## 🔄 Çalışma Mantığı

### Adım 1: Preview Oluşturma
```
Player "E" tuşuna bastığında:
1. Item prefab'ı instantiate edilir
2. Collider'lar bulunur ve trigger olarak ayarlanır
3. Physics devre dışı bırakılır (preview için)
4. Network componentleri disable edilir
```

### Adım 2: Her Frame Kontrol
```
Her frame'de:
1. Kameradan raycast gönderilir
2. Yere temas varsa placement position hesaplanır
3. CheckMeshCollision() çağrılır
   - Her collider için Physics.Overlap* kullanılır
   - Bulunan collider'lar filtrelenir (kendi collider'larımızı çıkar)
   - Başka bir objeyle çakışma varsa FALSE döner
4. Renk güncellenir (yeşil/kırmızı)
```

### Adım 3: Yerleştirme
```
Player tekrar "E" tuşuna bastığında:
1. canPlaceAtCurrentPosition kontrol edilir
2. TRUE ise server'a placement request gönderilir
3. Server gerçek objeyi spawn eder
4. Preview silinir ve placement mode kapanır
```

## 🎨 Görsel Feedback

Preview objesi iki renkte görünür:

- **🟢 Yeşil (Valid)**: 
  - Yere temas var
  - Hiçbir objeyle çakışma yok
  - Yerleştirilebilir

- **🔴 Kırmızı (Invalid)**:
  - Yere temas yok VEYA
  - Başka bir objeyle çakışma var
  - Yerleştirilemez

## 🎮 Player Kontrolü

| Tuş | Aksiyon |
|-----|---------|
| **E** | Placement mode'a gir / Objeyi yerleştir |
| **R** | Objeyi 45° döndür |
| **Q** veya **ESC** | Placement mode'dan çık (iptal) |

## 📊 Performans

### Optimizasyon İpuçları

1. **Layer Mask Kullanımı**
   - Collision Check Layer Mask'i sadece önemli objelerle sınırla
   - Player layer'ını exclude et
   - Sadece Static Geometry ve Placed Items layer'larını kontrol et

2. **Minimum Clearance**
   - Daha büyük değer = daha performanslı (daha erken collision)
   - Önerilen: 0.05m - 0.15m arası
   - Çok küçük değerler (< 0.01m) floating point hatalarına yol açabilir

3. **Use Mesh Collision Check**
   - FALSE yaparak basit raycast moduna geçebilirsin
   - Performans kritikse bu seçeneği kullan
   - Ancak objeler üst üste yerleştirilebilir

## 🐛 Debugging

Collision tespit edildiğinde console'da log görürsün:

```
[PlayerItemUsage] Collision detected with [ObjectName]
```

### Debug İçin:

1. **Scene View'da Gizmos**
   - Physics Debug Visualization'ı aç
   - Collider'ları görsel olarak kontrol et

2. **Layer Mask Kontrol**
   - Collision Check Layer Mask'in doğru ayarlandığından emin ol
   - Preview objesinin layer'ını kontrol et

3. **Collider Kontrolü**
   - Prefab'da collider var mı?
   - Collider enabled mı?
   - Collider boyutları doğru mu?

## 🔮 Gelecek İyileştirmeler

Sisteme eklenebilecek özellikler:

1. **Grid Snapping** - Objeler belirli bir grid'e otursun
2. **Surface Normal Alignment** - Obje yüzeyin açısına göre dönsün
3. **Multi-Object Check** - Birden fazla objeyi aynı anda yerleştir
4. **Undo/Redo** - Yerleştirme işlemlerini geri al
5. **Placement Preview Animation** - Yumuşak geçişler

## 📝 Örnek Kullanım

### Basit Yerleştirme
```csharp
// Inspector'da:
Use Mesh Collision Check = true
Minimum Clearance = 0.1m
Collision Check Layer Mask = Everything
```

### Sadece Zemin Kontrolü (Eski Sistem)
```csharp
// Inspector'da:
Use Mesh Collision Check = false
Placement Layer Mask = Ground (sadece zemin layer'ı)
```

### Sıkı Yerleştirme (Objeler çok yakın)
```csharp
// Inspector'da:
Use Mesh Collision Check = true
Minimum Clearance = 0.02m  // Çok küçük
Collision Check Layer Mask = PlacedItems only
```

### Gevşek Yerleştirme (Objeler arası boşluk)
```csharp
// Inspector'da:
Use Mesh Collision Check = true
Minimum Clearance = 0.3m  // Büyük
Collision Check Layer Mask = Everything
```

## ⚠️ Önemli Notlar

1. **Prefab Gereksinimleri**
   - Prefab'da en az bir Collider olmalı
   - Collider boyutları mesh'i kapsamalı
   - NetworkObject, Rigidbody vb. preview için otomatik disable edilir

2. **Layer Ayarları**
   - Preview objeleri kendi collider'larını ignore eder
   - Oyuncu ile collision ignore edilmeli (layer mask ile)
   - Static geometry ve placed items kontrol edilmeli

3. **Network Sync**
   - Collision kontrolü sadece client-side
   - Server final placement'ı onaylar
   - Cheating önlenmeli (server-side validation eklenebilir)

## 🎓 Kod Örneği

```csharp
// CheckMeshCollision() metodu her collider tipi için:

if (col is BoxCollider boxCol)
{
    // Box için özel hesaplama
    Vector3 center = col.bounds.center;
    Vector3 halfExtents = boxCol.size * 0.5f;
    // Scale'i uygula
    halfExtents.x *= col.transform.lossyScale.x;
    // Clearance'ı uygula
    halfExtents -= Vector3.one * minimumClearance;
    // Overlap kontrolü
    overlaps = Physics.OverlapBox(center, halfExtents, ...);
}
```

---

**📌 Not**: Bu sistem `PlayerItemUsage.cs` dosyasında implement edilmiştir.

