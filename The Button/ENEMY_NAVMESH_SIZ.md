# Enemy Sistemi - NavMesh'siz Versiyon

## 🎯 NavMesh Kaldırıldı!

Enemy artık NavMesh kullanmıyor. Direkt hareket ediyor!

### ✅ Avantajlar

- ❌ NavMesh bake etmeye gerek yok
- ✅ Düz zeminde direkt hareket
- ✅ Oyun içinde hareket alanı değiştirilebilir
- ✅ Daha basit ve hızlı
- ✅ Dinamik alan boyutu

---

## 🚀 Kullanım

### Basit Kullanım (Sınırsız Hareket)

1. **Enemy oluştur** (Capsule)
2. **Script'leri ekle**:
   - EnemyHealth ✓
   - EnemyAI ✓
   - Network Object ✓
3. **Ayarlar**:
   ```
   Use Movement Bounds: ☐ (kapalı)
   ```

Enemy her yere gidebilir!

### Sınırlı Alan (Bounds)

1. **Inspector'da**:
   ```
   Use Movement Bounds: ✓ (açık)
   Bounds Center: (0, 0, 0)
   Bounds Size: (20, 10, 20)  ← 20x10x20 metrelik alan
   ```

Enemy sadece bu alan içinde hareket eder!

---

## 🎮 Dinamik Alan Değiştirme

Oyun içinde alanı değiştirebilirsin!

### Kod ile Alan Ayarla

```csharp
// Enemy'yi bul
EnemyAI enemy = FindObjectOfType<EnemyAI>();

// Yeni alan belirle
Vector3 center = new Vector3(0, 0, 0);
Vector3 size = new Vector3(30, 10, 30);  // 30x10x30 metre
enemy.SetMovementBounds(center, size);
```

### Alan Büyüt/Küçült

```csharp
// Oyun ilerledikçe alan küçülsün
void ShrinkArea()
{
    float newSize = currentSize - 5f;  // 5 metre küçült
    enemy.SetMovementBounds(
        Vector3.zero, 
        new Vector3(newSize, 10, newSize)
    );
}
```

### Alanı Kapat

```csharp
// Sınırsız hareket
enemy.DisableMovementBounds();
```

---

## ⚙️ Inspector Ayarları

### Detection (Tespit)
```
Detection Range: 15     ← 15 metre içindeki oyuncuları görür
Attack Range: 2         ← 2 metre yakınsa saldırır
Player Layer: Default   ← Oyuncu layer'ı
```

### Combat (Savaş)
```
Attack Damage: 10       ← Saldırı hasarı
Attack Cooldown: 1.5    ← Saldırılar arası süre (saniye)
```

### Movement (Hareket)
```
Move Speed: 3.5         ← Hareket hızı (metre/saniye)
Rotation Speed: 5       ← Dönüş hızı
```

### Movement Bounds (Hareket Alanı)
```
Use Movement Bounds: ☐/✓
Bounds Center: (0, 0, 0)      ← Alan merkezi
Bounds Size: (20, 10, 20)     ← Alan boyutu (X, Y, Z)
```

---

## 🎨 Görselleştirme (Gizmos)

Scene view'da enemy'yi seçince:
- 🟡 Sarı daire: Detection range (tespit alanı)
- 🔴 Kırmızı daire: Attack range (saldırı alanı)
- 🔵 Mavi kutu: Movement bounds (hareket alanı)
- 🟢 Yeşil çizgi: Hedef oyuncuya bağlantı

---

## 💡 Örnek Senaryolar

### Senaryo 1: Battle Royale Tarzı Küçülen Alan

```csharp
public class AreaShrinker : MonoBehaviour
{
    public EnemyAI[] enemies;
    public float shrinkInterval = 30f;  // 30 saniyede bir
    public float shrinkAmount = 5f;     // 5 metre küçült
    
    private float currentSize = 50f;
    
    void Start()
    {
        InvokeRepeating(nameof(ShrinkArea), shrinkInterval, shrinkInterval);
    }
    
    void ShrinkArea()
    {
        currentSize -= shrinkAmount;
        currentSize = Mathf.Max(10f, currentSize);  // Min 10 metre
        
        foreach (var enemy in enemies)
        {
            enemy.SetMovementBounds(
                Vector3.zero, 
                new Vector3(currentSize, 10, currentSize)
            );
        }
        
        Debug.Log($"Area shrunk to {currentSize}m");
    }
}
```

### Senaryo 2: Oda Bazlı Hareket

```csharp
public class RoomManager : MonoBehaviour
{
    public void SetEnemyRoom(EnemyAI enemy, Transform room)
    {
        // Odanın merkezini ve boyutunu al
        Bounds roomBounds = room.GetComponent<Collider>().bounds;
        
        enemy.SetMovementBounds(
            roomBounds.center,
            roomBounds.size
        );
        
        Debug.Log($"Enemy confined to room: {room.name}");
    }
}
```

### Senaryo 3: Dinamik Arena

```csharp
public class DynamicArena : MonoBehaviour
{
    public EnemyAI[] enemies;
    public Transform arenaCenter;
    public float minSize = 10f;
    public float maxSize = 50f;
    
    void Update()
    {
        // Oyuncu sayısına göre alan boyutu
        int playerCount = FindObjectsOfType<PlayerNetwork>().Length;
        float size = Mathf.Lerp(minSize, maxSize, playerCount / 10f);
        
        foreach (var enemy in enemies)
        {
            enemy.SetMovementBounds(
                arenaCenter.position,
                new Vector3(size, 10, size)
            );
        }
    }
}
```

---

## 🐛 Sorun Giderme

### Enemy Hareket Etmiyor

**Kontrol Et**:
- CharacterController var mı? (Otomatik eklenir)
- Move Speed > 0 mı?
- Enemy ölü mü?

### Enemy Sınırları Aşıyor

**Kontrol Et**:
- Use Movement Bounds işaretli mi?
- Bounds Size yeterince büyük mü?
- Enemy spawn pozisyonu bounds içinde mi?

### Enemy Havada Kalıyor

**Çözüm**: Gravity otomatik uygulanıyor ama zemin collider'ı olmalı.

---

## 🎯 Özet

**Artık**:
- ✅ NavMesh yok
- ✅ Direkt hareket
- ✅ Dinamik alan
- ✅ Oyun içinde değiştirilebilir
- ✅ Daha basit

**Kullanım**:
1. Enemy oluştur
2. Script'leri ekle
3. İstersen bounds ayarla
4. Test et!

**Dinamik Alan**:
```csharp
enemy.SetMovementBounds(center, size);  // Alan ayarla
enemy.DisableMovementBounds();          // Sınırsız yap
```

İyi oyunlar! 🎮

