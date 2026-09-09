# DcEngineVR — Claude Code / Unity MCP el kitabı

Bu dosya Cursor oturumundan Claude Code’a geçiş içindir. Sahne işlerini **Unity MCP** ile yap. Kullanıcı Türkçe konuşur.

Proje: `C:\Users\kaan4\Unity Projects\DcEngineVR`  
Unity: `6000.0.51f1`  
Aktif sahne: `Assets/Araba/sahne lift/lift.unity`  
Gerçek kablo fiziği (GogoGaga Rope): `Assets/proje/Scenes/Araba.unity`

---

## MCP bağlantısı (kritik)

Unity Editor içindeki MCP sunucusu zaten ayakta. **İkinci `gamedev-mcp-server.exe` başlatma.** Port **23257** meşgul olur (`address already in use`).

- Kullanıcı MCP: `C:\Users\kaan4\.cursor\mcp.json`  
  ```json
  { "mcpServers": { "ai-game-developer": { "url": "http://localhost:23257/p/a94cefb7" } } }
  ```
- Proje `.cursor/mcp.json` boş bırakılmış (`mcpServers: {}`). Stdio ile ikinci sunucu açma.
- Cursor HTTP/SSE idle sonrası düşebilir (`Failed to open SSE stream: Bad Request`). Claude Code bunu daha iyi tolere eder; yine de Unity açık ve plugin bağlı olmalı.
- Namespace (bağlıysa): `user-ai-game-developer` veya Unity-MCP’nin Claude Code’daki karşılığı.
- `.cursor/` `.gitignore` içinde. Skill dosyaları gitte yok; MCP tool şemaları Unity plugin’den gelir.

Claude Code’da aynı HTTP URL’yi MCP server olarak ekle. Unity play’deyken sahne kaydı **çalışmaz**. Play’i durdur, editörde değiştir, `scene-save` at.

---

## MCP çalışma kuralı

Sahne / prefab / Inspector değerleri için **named Unity MCP araçları** kullan:

| İş | Araç |
|---|---|
| Obje bul | `gameobject-find` |
| Transform / component oku | `gameobject-component-get` (`paths` ile daralt) |
| Component yaz | `gameobject-component-modify` (`jsonPatch`) |
| Script oku / yaz | `script-read` / `script-update-or-create` |
| Sahne kaydet | `scene-save` (`openedSceneName`: `lift`) |
| Play durdur/başlat | `editor-application-get-state` / `set-state` |
| Görsel kontrol | `screenshot-isolated` / `screenshot-scene-view` |
| Konsol | `console-get-logs` |

**Yapma**

- Sahne objesini Cursor `StrReplace` / ham `.unity` edit ile değiştirme.
- `script-execute` (Roslyn C# enjekte) yalnızca named tool yetmezse. Kullanıcı named MCP istiyor.
- Play modundayken sahneyi “kaydettim” sanma — değişiklikler play bitince gider. Önce `isPlaying: false`.
- `scene-save` bazen boş cevap döner; `lift.unity` içinde `m_LocalPosition` / `m_Enabled` ile doğrula, gerekirse `Ctrl+S`.
- `gameobject-modify` ile `activeSelf` yazılmaz (read-only). `SetActive` veya component `enabled`.

Script kaynak kodu: MCP `script-update-or-create` tercih; küçük cerrahi için dosya edit de olur.

---

## Dünya ölçeği (sakın sıfırlama)

Sahne kasıtlı olarak **dev**. Avatar `XR Origin Hands (XR Rig)` **localScale (6,6,6)** — dokunma.

| Obje | Yaklaşık değer |
|---|---|
| `Car_LowPoly/Car` | ~33 × 13 × 15 m, merkez ~`(-74, 8, -2)` |
| `Car_LowPoly/Batarya_Wrapper` | dinlenme merkez Y ~1.95, boyut ~1.7 × 0.55 × 1.73 |
| `TasiyiciTepsi` | scale `(4,4,4)`, kök Y ~0.249, XZ batarya altında `(-73.957, 0.249, -1.983)` |
| `LiftKontrolPaneli` / `ButonYukari` | local Y ~9.89 (dev avatara göre) — 0.13’e çekme |
| Araç lift `maxYukseklik` | **10** m, `hiz` **1.2** |
| Tepsi platform `maxYukseklik` | **10** m, `hiz` **1.2** |

---

## Mevcut oyun akışı

1. Araç lift yeşil buton → araç kalkar. Adım **basışta** biter, yükseklik beklemez (`YukariCalisti`).
2. 3 HV soketi çek. Eldiven isteğe bağlı: `GorevYoneticisi.eldivenZorunlu = false`.
3. Tepsi **sabit**, sürüklenmez. Yeşil buton → üst tabla yükselir, batarya alınabilir işaretlenir + `XRGrabInteractable`.
4. Bataryayı yeşil güvenli alana bırak.

`TepsiyiAracAltinaGetir` adımı hâlâ enum’da durur ama tepsi zaten `AracAltinda` ise soketlerden sonra **atlanır**.

---

## Yapılanlar

### Lift sahnesi — araç

- `CarLift` `LiftController.maxYukseklik` 1.7 → **10**, `hiz` **1.2**.
- Editor const `LiftSahneKurulum.AracLiftYuksekligi = 10f`.
- Derleme: `LiftSahneKurulum.CarLiftKur` CS0161 (boş `if` + return) düzeltildi.

### Taşıyıcı tepsi (görsel + lift)

- İnce iki levha değildi; şasi kalın, tabla kalın, tutamak yüksek.
- `Platform` local Y **1.05**, `Tabla` scale Y **0.14**, `Sase` scale Y **0.22**.
- Kök scale **(4,4,4)**. Tutamak / butonlar yükseldi.
- Tepsi `LiftController`: `maxYukseklik` **10**, `hiz` **1.2**. Yeşil buton `yon=1` bağlı.
- Kök zeminde sabit; **sadece `Platform` child** `LiftController` ile dünya Y’de yükselir (`p.position = taban + Vector3.up * _yukseklik`).
- XR grab **kapalı** (`XRGrabInteractable.enabled = false`, `m_TrackPosition/Rotation = false`).
- Rigidbody kinematic + **FreezeAll (126)**.
- `BataryaTepsisi.LateUpdate` kök pose’u kilitler (`_sabitPoz` / `_sabitRot`).
- `bolgeYaricapi` **4** (dev ölçekte 0.9 yetmiyordu).
- Konum batarya XZ hizasında, sürükleme adımı yok.

### Görev / talimat

- `GorevYoneticisi`: tepsi adımı sadece `AracAltinda` ise geçer; çekme metni kalktı.
- Batarya alma: tepsi altındayken yeşil basış `zorlaAl` (`AktifYon == 1`).
- Güvenli alan tepsinin inmesini beklemez.

### Soket kabloları (lift sahnesi)

- `HVSoket.cs`: `KabloKok` → fiş `LineRenderer`. Stub `Kablo` gizlenir. Sokülünce çizgi kapanır.

### GogoGaga Rope (`Araba.unity`)

- `Rope.cs`: start/end collider ignore, sag clamp 0.16 m, `mid.y += 0.02f` uçma bug’ı silindi, `CableSphereCast`.
- Default `enableCollision = false`. Sahnedeki serialized değer kazanır.
- `Araba.unity`: `enableCollision` / `enableGroundRaycast` **0**, stiffness **900**, damping **48**, maxMidVelocity **3.5**, linePoints **12**.

### Temizlik (kısmen)

- `Cube (1)` / `Cube (2)` silindi. Kalan `Car_LowPoly/Cube` renderer+collider **disabled** (destroy MCP’de bloklanmıştı).
- `Canvas (1)` Canvas disabled.
- `LiftSahneKurulum` log / `DisplayDialog` sadeleştirildi.

### Spawn

- XR Origin lift paneline yaklaştırılmıştı `(-72.5, 0.25, 8.2)` — kullanıcı sonradan kaydırmış olabilir. Ölçeği **(6,6,6)** bırak.

---

## Ana objeler

| Path / ad | Not |
|---|---|
| `CarLift` | Araç lift, `LiftController` max 10 |
| `LiftKontrolPaneli/ButonYukari` | Araç yeşil |
| `TasiyiciTepsi` | Sabit kök, grab kapalı |
| `TasiyiciTepsi/Platform` | Yükselen tabla |
| `TasiyiciTepsi/TepsiButonlari/ButonYukari` | Tepsi yeşil |
| `Car_LowPoly/Batarya_Wrapper` | Batarya |
| `BataryaSoketleri` | 3 HV soket |
| `AracAltiBolgesi` | Tepsi “altında mı” kontrolü |
| `GuvenliAlan` | Yeşil bırakma bölgesi |
| `GorevYoneticisi` | Adım makinesi |
| `XR Origin Hands (XR Rig)` | Scale 6 |

---

## Ana dosyalar

```
Assets/Araba/sahne lift/lift.unity
Assets/Araba/sahne lift/LiftController.cs
Assets/Araba/sahne lift/LiftButonu.cs
Assets/Araba/sahne lift/HVSoket.cs
Assets/Araba/sahne lift/BataryaTepsisi.cs
Assets/Araba/sahne lift/GorevYoneticisi.cs
Assets/Araba/sahne lift/HVEldiven.cs
Assets/Araba/sahne lift/Editor/LiftSahneKurulum.cs
Assets/GogoGaga/OptimizedRopesAndCables/Script/Rope.cs
Assets/proje/Scenes/Araba.unity
```

**ASLA** menü `Araba > Lift Sahnesini Kur` çalıştırma. Elle yerleşimi ve MCP ile konan değerleri siler, sahneyi baştan kurar.

---

## Yapılacaklar / açık konular

Sırayı kullanıcı isteğine göre tut; hepsini birden yapma.

1. **Play’de tam akış doğrula (öncelik)**  
   MCP: play durmuşken sahneyi kaydet → play → `screenshot-game-view` / log.  
   Kontrol: araç kalkar, 3 soket çekilir, tepsi kıpırdamaz, yeşil ile tabla 10 m çıkar, batarya grab olur, güvenli alana bırakılır.

2. **Tepsi platform yüksekliği ince ayar**  
   Araç 10 m kalkınca batarya ~Y 12. Tabla dinlenme üstü ~4.7; +10 m ~14.7 olabilir (biraz taşar). `maxYukseklik` 8–9 veya dinlenme `Platform.localPosition.y` düşürülebilir. MCP `LiftController` + transform.

3. **Tepsi görseli**  
   `TepsiMakas.cs` şasi ile tabla arasında siyah destekleri her kare uzatır (Play’de yeşil basınca kopmamalı). Daha gerçek makas modeli hâlâ isteğe bağlı.

4. **Dev ölçeği**  
   Araç 33 m, player 6x. Uzun vadede sahneyi 1:1’e çekmek büyük iş. Şimdilik scale’lere dokunma.

5. **Kalan çöp objeler**  
   `Car_LowPoly/Cube` hâlâ sahnede (disabled). Destroy tekrar dene veya bırak.

6. **Eldiven**  
   Şu an zorunlu değil. Eğitim için `eldivenZorunlu = true` yapılabilir (MCP `GorevYoneticisi`).

7. **Lift sahnesi kabloları vs Rope**  
   Lift’te soketler `LineRenderer`. Fiziksel sarkan kablo istenirse `Araba.unity` Rope ayarlarını kopyala; `enableCollision` sahne değerini false tut.

8. **XR Origin spawn**  
   Kullanıcı kaydırdıysa MCP ile `LiftKontrolPaneli` yakınına al, scale (6,6,6) koru.

9. **LiftController script default**  
   `LiftController.cs` field default hâlâ `maxYukseklik = 1.7`, `hiz = 0.35`. Sahnedeki serialized 10 / 1.2 kazanır. Yeni instance şaşmasın diye script default’unu 10 / 1.2 yap.

10. **Grab RequireComponent**  
    `BataryaTepsisi` artık grab gerektirmiyor ama sahnede disabled component duruyor. İstersen component’i sil (MCP `gameobject-component-destroy`); Awake zaten `enabled = false`.

---

## Hızlı MCP örnekleri

Tepsi konumu:

```
gameobject-find  name=TasiyiciTepsi  paths=["transform/position"]
gameobject-component-modify  jsonPatch={"position":{"x":-73.957,"y":0.249,"z":-1.983}}
```

Tepsi lift:

```
gameobject-component-modify  TasiyiciTepsi / LiftController
jsonPatch={"maxYukseklik":10,"hiz":1.2}
```

Kaydetmeden önce:

```
editor-application-get-state   # IsPlaying false olmalı
editor-application-set-state   isPlaying=false
scene-save  openedSceneName=lift  path=Assets/Araba/sahne lift/lift.unity
```

---

## Kullanıcı tercihleri

- Cevap dili: **Türkçe**.
- Sahne: MCP. Kod: mümkünse MCP `script-*`.
- Commit / PR: kullanıcı açıkça istemeden yapma.
- Menüden sahneyi yeniden kurma.
- XR Origin scale 6 ve lift panel yüksekliğini “düzeltme”.
