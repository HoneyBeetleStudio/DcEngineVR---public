using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class LiftSahneKurulum
{
    const string SahneYolu = "Assets/Araba/sahne lift/lift.unity";
    const string KlasorYolu = "Assets/Araba/sahne lift";
    const string MateryalKlasoru = "Assets/Araba/sahne lift/Materials";
    const string CarLiftFbx = "Assets/Araba/sahne lift/car-lift/carLift.fbx";
    const string TexKlasoru = "Assets/Araba/sahne lift/car-lift/textures";
    const string BataryaGlb = "Assets/Araba/textures/wb/Battery Bank .glb";

    const float AracLiftYuksekligi = 1.7f;

    static readonly string[] SilinecekAdlar =
    {
        "my_motor", "EngineBlock", "HVManager", "Kalibrasyon_1", "Kalibrasyon_2",
        "Kalibrasyon_Soket_1", "Kalibrasyon_Soket_2", "CanvasManager", "avometre",
        "cables", "cables (1)", "araKonnektor (1)", "arakonnektör alanları",
        "a (1)", "Elevator Buttons", "Electric motor 1", "whiteboard",
        "soketler", "Batarya_cikis", "SasiControl", "AvoCanvas", "pop-up",
        "my_motor (1)", "hvText"
    };

    static readonly string[] KurulumdaOlusanlar =
    {
        "CarLift", "TasiyiciTepsi", "GuvenliAlan", "TalimatPanel",
        "GorevYoneticisi", "BataryaSoketleri", "Eldivenler", "AracAltiBolgesi",
        "Batarya_Wrapper", "LiftKontrolPaneli"
    };

    [MenuItem("Araba/Lift Sahnesini Kur")]
    public static void Kur()
    {
        var aktifSahne = SceneManager.GetActiveScene();
        if (aktifSahne.path != SahneYolu)
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(SahneYolu);
        }

        NormalMaplariAyarla();

        EskiObjeleriSil();

        GameObject arac = GameObject.Find("Car_LowPoly");
        if (arac == null)
        {
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Hata", "Sahnede 'Car_LowPoly' bulunamadi.", "Tamam");
            return;
        }

        Bounds aracSinir = SinirHesapla(arac.transform);
        float zeminY = aracSinir.min.y;

        Vector3 uzunEksen = aracSinir.size.x >= aracSinir.size.z ? Vector3.right : Vector3.forward;
        Vector3 genisEksen = uzunEksen == Vector3.right ? Vector3.forward : Vector3.right;
        float aracGenisligi = uzunEksen == Vector3.right ? aracSinir.size.z : aracSinir.size.x;
        float aracUzunlugu = uzunEksen == Vector3.right ? aracSinir.size.x : aracSinir.size.z;

        var materyaller = MateryalleriOlustur();

        GameObject lift = CarLiftKur(aracSinir, genisEksen, aracGenisligi, zeminY, materyaller, out Transform liftPlatformu);
        GameObject panel = LiftKontrolPaneliKur(lift, aracSinir, genisEksen, aracGenisligi, zeminY, materyaller, out LiftController aracLift, arac.transform, liftPlatformu);
        Transform batarya = BataryayiFitEt(arac.transform, aracSinir, uzunEksen, zeminY);
        HVSoket[] soketler = SoketleriKur(arac.transform, batarya, aracSinir, uzunEksen, materyaller);
        HVEldiven eldiven = EldivenleriKur(aracSinir, genisEksen, zeminY, materyaller, out GameObject eldivenGrubu);
        BataryaTepsisi tepsi = TepsiKur(aracSinir, genisEksen, uzunEksen, zeminY, batarya, materyaller, out GameObject tepsiObj, out GameObject guvenliAlanObj, out GameObject aracAltiObj);
        GorevYoneticisiKur(aracLift, soketler, eldiven, tepsi, panel, eldivenGrubu, tepsiObj, guvenliAlanObj, soketGrubu: soketler.Length > 0 ? soketler[0].transform.parent.gameObject : null);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }


    static void EskiObjeleriSil()
    {
        var silinecek = new List<GameObject>();
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t == null || t.gameObject == null)
                continue;
            string ad = t.gameObject.name;
            if (SilinecekAdlar.Contains(ad) || KurulumdaOlusanlar.Contains(ad))
                silinecek.Add(t.gameObject);
        }

        foreach (var c in Object.FindObjectsByType<VRChecklistManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            silinecek.Add(c.gameObject);
        foreach (var c in Object.FindObjectsByType<VRChecklistItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            silinecek.Add(c.gameObject);
        foreach (var c in Object.FindObjectsByType<KabloGrabHighlight>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            silinecek.Add(c.gameObject);
        foreach (var c in Object.FindObjectsByType<KabloYonOku>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            silinecek.Add(c.gameObject);
        foreach (var c in Object.FindObjectsByType<MotorParcalari>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            silinecek.Add(c.gameObject);
        foreach (var c in Object.FindObjectsByType<SnapTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            silinecek.Add(c.gameObject);
        foreach (var c in Object.FindObjectsByType<GogoGaga.OptimizedRopesAndCables.Rope>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            silinecek.Add(c.gameObject);
        foreach (var go in silinecek)
        {
            if (go == null)
                continue;
            if (PrefabUtility.IsPartOfPrefabInstance(go) && !PrefabUtility.IsOutermostPrefabInstanceRoot(go))
            {
                var kok = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
                if (kok != null)
                    PrefabUtility.UnpackPrefabInstance(kok, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            if (go != null)
                Object.DestroyImmediate(go);
        }
    }


    static void NormalMaplariAyarla()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { TexKlasoru }))
        {
            string yol = AssetDatabase.GUIDToAssetPath(guid);
            if (!yol.Contains("_Normal"))
                continue;
            var imp = (TextureImporter)AssetImporter.GetAtPath(yol);
            if (imp != null && imp.textureType != TextureImporterType.NormalMap)
            {
                imp.textureType = TextureImporterType.NormalMap;
                imp.SaveAndReimport();
            }
        }
    }

    static Dictionary<string, Material> MateryalleriOlustur()
    {
        if (!AssetDatabase.IsValidFolder(MateryalKlasoru))
            AssetDatabase.CreateFolder(KlasorYolu, "Materials");

        var sonuc = new Dictionary<string, Material>();
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");

        foreach (string set in new[] { "lift", "rack", "arrival", "switch" })
        {
            var mat = MateryalYukleVeyaOlustur($"CarLift_{set}", lit);
            mat.SetTexture("_BaseMap", TextureYukle($"carLift_{set}_BaseColor"));
            var normal = TextureYukle($"carLift_{set}_Normal");
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.EnableKeyword("_NORMALMAP");
            }
            var metal = TextureYukle($"carLift_{set}_Metallic");
            if (metal != null)
            {
                mat.SetTexture("_MetallicGlossMap", metal);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            mat.SetFloat("_Smoothness", 0.45f);
            sonuc[set] = mat;
        }

        sonuc["eldiven"] = DuzMateryal("Eldiven_Kirmizi", lit, new Color(0.8f, 0.06f, 0.06f), 0.35f);
        sonuc["tepsiSari"] = DuzMateryal("Tepsi_Sari", lit, new Color(0.95f, 0.78f, 0.05f), 0.4f);
        sonuc["tepsiKoyu"] = DuzMateryal("Tepsi_Koyu", lit, new Color(0.15f, 0.15f, 0.16f), 0.3f);
        sonuc["soketTuruncu"] = DuzMateryal("Soket_Turuncu", lit, new Color(0.95f, 0.4f, 0.05f), 0.5f);
        sonuc["soketKoyu"] = DuzMateryal("Soket_Koyu", lit, new Color(0.12f, 0.12f, 0.13f), 0.4f);
        sonuc["butonYesil"] = DuzMateryal("Buton_Yesil", lit, new Color(0.1f, 0.75f, 0.15f), 0.55f);
        sonuc["butonKirmizi"] = DuzMateryal("Buton_Kirmizi", lit, new Color(0.85f, 0.1f, 0.1f), 0.55f);

        var yesil = DuzMateryal("GuvenliAlan_Yesil", lit, new Color(0.1f, 0.85f, 0.25f, 0.45f), 0.1f);
        SeffafYap(yesil);
        sonuc["guvenliAlan"] = yesil;

        AssetDatabase.SaveAssets();
        return sonuc;
    }

    static Material MateryalYukleVeyaOlustur(string ad, Shader shader)
    {
        string yol = $"{MateryalKlasoru}/{ad}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, yol);
        }
        else
        {
            mat.shader = shader;
        }
        return mat;
    }

    static Material DuzMateryal(string ad, Shader shader, Color renk, float smoothness)
    {
        var mat = MateryalYukleVeyaOlustur(ad, shader);
        mat.SetColor("_BaseColor", renk);
        mat.SetFloat("_Smoothness", smoothness);
        return mat;
    }

    static void SeffafYap(Material mat)
    {
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = (int)RenderQueue.Transparent;
    }

    static Texture2D TextureYukle(string ad)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexKlasoru}/{ad}.png");
    }


    static GameObject CarLiftKur(Bounds aracSinir, Vector3 genisEksen, float aracGenisligi, float zeminY,
        Dictionary<string, Material> materyaller, out Transform liftPlatformu)
    {
        liftPlatformu = null;
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(CarLiftFbx);
        if (model == null)
            return null;

        var lift = (GameObject)PrefabUtility.InstantiatePrefab(model);
        lift.name = "CarLift";

        foreach (var r in lift.GetComponentsInChildren<Renderer>())
        {
            var yeni = r.sharedMaterials.ToArray();
            for (int i = 0; i < yeni.Length; i++)
            {
                if (yeni[i] == null)
                    continue;
                string ad = yeni[i].name.ToLowerInvariant();
                foreach (string set in new[] { "arrival", "switch", "rack", "lift" })
                {
                    if (ad.StartsWith(set))
                    {
                        yeni[i] = materyaller[set];
                        break;
                    }
                }
            }
            r.sharedMaterials = yeni;
        }

        Transform rack1 = CocukBul(lift.transform, "rack");
        Transform rack2 = CocukBul(lift.transform, "rack.001");
        Vector3 aracMerkez = new Vector3(aracSinir.center.x, zeminY, aracSinir.center.z);

        if (rack1 != null && rack2 != null)
        {
            Vector3 ayrac = rack2.position - rack1.position;
            ayrac.y = 0f;
            if (ayrac.sqrMagnitude > 0.0001f)
            {
                float aci = Vector3.SignedAngle(ayrac.normalized, genisEksen, Vector3.up);
                if (aci > 90f) aci -= 180f;
                if (aci < -90f) aci += 180f;
                lift.transform.Rotate(Vector3.up, aci, Space.World);

                ayrac = rack2.position - rack1.position;
                ayrac.y = 0f;
                float mevcutAralik = Mathf.Max(ayrac.magnitude, 0.001f);
                float mevcutYukseklik = Mathf.Max(SinirHesapla(lift.transform).size.y, 0.001f);
                float aralikOlcek = (aracGenisligi + 1.3f) / mevcutAralik;
                float yukseklikOlcek = 2.3f / mevcutYukseklik;
                float olcek = Mathf.Max(aralikOlcek, yukseklikOlcek);
                lift.transform.localScale = lift.transform.localScale * olcek;
            }

            Bounds liftSinir = SinirHesapla(lift.transform);
            Vector3 sutunOrta = (rack1.position + rack2.position) * 0.5f;
            Vector3 kaydirma = aracMerkez - new Vector3(sutunOrta.x, liftSinir.min.y, sutunOrta.z);
            lift.transform.position += kaydirma;

            Bounds sonSinir = SinirHesapla(lift.transform);
        }
        else
        {
            Bounds liftSinir = SinirHesapla(lift.transform);
            lift.transform.position += aracMerkez - new Vector3(liftSinir.center.x, liftSinir.min.y, liftSinir.center.z);
        }

        liftPlatformu = CocukBul(lift.transform, "Bone");
        if (liftPlatformu == null)

        return lift;
    }

    static GameObject LiftKontrolPaneliKur(GameObject lift, Bounds aracSinir, Vector3 genisEksen,
        float aracGenisligi, float zeminY, Dictionary<string, Material> materyaller,
        out LiftController aracLift, Transform arac, Transform liftPlatformu)
    {
        GameObject controllerObj = lift != null ? lift : new GameObject("CarLift");
        aracLift = controllerObj.AddComponent<LiftController>();
        var platformlar = new List<Transform> { arac };
        if (liftPlatformu != null)
            platformlar.Insert(0, liftPlatformu);
        aracLift.platformlar = platformlar.ToArray();
        aracLift.maxYukseklik = AracLiftYuksekligi;
        aracLift.hiz = 0.4f;

        var panel = new GameObject("LiftKontrolPaneli");
        Vector3 panelPoz = new Vector3(aracSinir.center.x, zeminY, aracSinir.center.z)
            + genisEksen * (aracGenisligi * 0.5f + 1.0f)
            + Vector3.up * 1.15f;
        panel.transform.position = panelPoz;
        panel.transform.rotation = Quaternion.LookRotation(genisEksen);

        var kutu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kutu.name = "PanelKutusu";
        kutu.transform.SetParent(panel.transform, false);
        kutu.transform.localScale = new Vector3(0.34f, 0.52f, 0.08f);
        kutu.GetComponent<Renderer>().sharedMaterial = materyaller["tepsiKoyu"];

        ButonOlustur(panel.transform, aracLift, +1, new Vector3(0f, 0.13f, -0.05f), materyaller["butonYesil"]);
        ButonOlustur(panel.transform, aracLift, -1, new Vector3(0f, -0.13f, -0.05f), materyaller["butonKirmizi"]);

        return panel;
    }

    static void ButonOlustur(Transform parent, LiftController lift, int yon, Vector3 lokalPoz, Material mat)
    {
        var buton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        buton.name = yon > 0 ? "ButonYukari" : "ButonAsagi";
        buton.transform.SetParent(parent, false);
        buton.transform.localPosition = lokalPoz;
        buton.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        buton.transform.localScale = new Vector3(0.13f, 0.025f, 0.13f);
        buton.GetComponent<Renderer>().sharedMaterial = mat;

        Object.DestroyImmediate(buton.GetComponent<Collider>());
        var col = buton.AddComponent<BoxCollider>();
        col.size = new Vector3(1.2f, 3f, 1.2f);

        buton.AddComponent<XRSimpleInteractable>();
        var lb = buton.AddComponent<LiftButonu>();
        lb.lift = lift;
        lb.yon = yon;
        lb.butonGorseli = buton.transform;
        lb.basilmaDerinligi = 0.012f;
    }


    static Transform BataryayiFitEt(Transform arac, Bounds aracSinir, Vector3 uzunEksen, float zeminY)
    {
        GameObject batarya = null;
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name.StartsWith("Battery Bank"))
            {
                batarya = t.gameObject;
                break;
            }
        }
        if (batarya == null)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(BataryaGlb);
            if (asset == null)
                return null;
            batarya = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            batarya.name = "Battery Bank";
        }

        var wrapper = new GameObject("Batarya_Wrapper");
        wrapper.transform.position = SinirHesapla(batarya.transform).center;
        batarya.transform.SetParent(wrapper.transform, true);

        float aracUzunlugu = uzunEksen == Vector3.right ? aracSinir.size.x : aracSinir.size.z;
        float aracGenisligi = uzunEksen == Vector3.right ? aracSinir.size.z : aracSinir.size.x;

        Bounds mevcut = SinirHesapla(batarya.transform);
        Vector3 hedefBoyut =
            uzunEksen == Vector3.right
                ? new Vector3(aracUzunlugu * 0.55f, 0.28f, aracGenisligi * 0.75f)
                : new Vector3(aracGenisligi * 0.75f, 0.28f, aracUzunlugu * 0.55f);

        wrapper.transform.localScale = new Vector3(
            hedefBoyut.x / Mathf.Max(mevcut.size.x, 0.001f),
            hedefBoyut.y / Mathf.Max(mevcut.size.y, 0.001f),
            hedefBoyut.z / Mathf.Max(mevcut.size.z, 0.001f));

        Bounds yeni = SinirHesapla(batarya.transform);
        float altY = zeminY + 0.42f;
        Vector3 hedefMerkez = new Vector3(aracSinir.center.x, altY + yeni.size.y * 0.5f, aracSinir.center.z);
        wrapper.transform.position += hedefMerkez - yeni.center;

        wrapper.transform.SetParent(arac, true);
        return wrapper.transform;
    }

    static HVSoket[] SoketleriKur(Transform arac, Transform batarya, Bounds aracSinir, Vector3 uzunEksen, Dictionary<string, Material> materyaller)
    {
        if (batarya == null)
            return new HVSoket[0];

        Bounds bSinir = SinirHesapla(batarya);
        var grup = new GameObject("BataryaSoketleri");
        Vector3 uc = bSinir.center + uzunEksen * (bSinir.size.magnitude > 0 ? Vector3.Scale(bSinir.size, uzunEksen).magnitude * 0.5f : 0.5f);
        grup.transform.position = uc;
        grup.transform.rotation = Quaternion.LookRotation(uzunEksen);
        grup.transform.SetParent(arac, true);

        Vector3 genisEksen = uzunEksen == Vector3.right ? Vector3.forward : Vector3.right;
        float aralik = 0.22f;
        var liste = new List<HVSoket>();

        for (int i = 0; i < 3; i++)
        {
            float ofset = (i - 1) * aralik;
            var soket = new GameObject($"HVSoket_{i + 1}");
            soket.transform.position = uc + genisEksen * ofset + Vector3.down * (bSinir.size.y * 0.15f);
            soket.transform.rotation = Quaternion.LookRotation(uzunEksen);
            soket.transform.SetParent(grup.transform, true);

            var govde = GameObject.CreatePrimitive(PrimitiveType.Cube);
            govde.name = "Govde";
            govde.transform.SetParent(soket.transform, false);
            govde.transform.localScale = new Vector3(0.07f, 0.06f, 0.1f);
            govde.GetComponent<Renderer>().sharedMaterial = materyaller["soketTuruncu"];
            Object.DestroyImmediate(govde.GetComponent<Collider>());

            var kapak = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            kapak.name = "Kapak";
            kapak.transform.SetParent(soket.transform, false);
            kapak.transform.localPosition = new Vector3(0f, 0f, 0.06f);
            kapak.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            kapak.transform.localScale = new Vector3(0.05f, 0.02f, 0.05f);
            kapak.GetComponent<Renderer>().sharedMaterial = materyaller["soketKoyu"];
            Object.DestroyImmediate(kapak.GetComponent<Collider>());

            var kablo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            kablo.name = "Kablo";
            kablo.transform.SetParent(soket.transform, false);
            kablo.transform.localPosition = new Vector3(0f, 0.07f, 0.02f);
            kablo.transform.localScale = new Vector3(0.02f, 0.05f, 0.02f);
            kablo.GetComponent<Renderer>().sharedMaterial = materyaller["soketTuruncu"];
            Object.DestroyImmediate(kablo.GetComponent<Collider>());

            var col = soket.AddComponent<BoxCollider>();
            col.size = new Vector3(0.12f, 0.14f, 0.16f);

            var rb = soket.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var grab = soket.AddComponent<XRGrabInteractable>();
            grab.throwOnDetach = false;
            grab.useDynamicAttach = true;

            liste.Add(soket.AddComponent<HVSoket>());
        }
        return liste.ToArray();
    }


    static HVEldiven EldivenleriKur(Bounds aracSinir, Vector3 genisEksen, float zeminY,
        Dictionary<string, Material> materyaller, out GameObject eldivenGrubu)
    {
        eldivenGrubu = new GameObject("Eldivenler");

        Vector3 sehpaPoz = new Vector3(aracSinir.center.x, zeminY, aracSinir.center.z)
            - genisEksen * (Vector3.Scale(aracSinir.size, genisEksen).magnitude * 0.5f + 1.6f);
        var sehpa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sehpa.name = "EldivenSehpasi";
        sehpa.transform.SetParent(eldivenGrubu.transform, false);
        sehpa.transform.position = sehpaPoz + Vector3.up * 0.45f;
        sehpa.transform.localScale = new Vector3(0.6f, 0.9f, 0.4f);
        sehpa.GetComponent<Renderer>().sharedMaterial = materyaller["tepsiKoyu"];

        HVEldiven ilk = null, onceki = null;
        for (int i = 0; i < 2; i++)
        {
            var eldiven = ElYap($"Eldiven_{(i == 0 ? "Sol" : "Sag")}", materyaller["eldiven"], i == 0);
            eldiven.transform.SetParent(eldivenGrubu.transform, false);
            eldiven.transform.position = sehpaPoz + Vector3.up * 0.95f
                + Vector3.Cross(Vector3.up, genisEksen) * (i == 0 ? -0.14f : 0.14f);

            var col = eldiven.AddComponent<BoxCollider>();
            col.size = new Vector3(0.16f, 0.12f, 0.3f);

            eldiven.AddComponent<XRSimpleInteractable>();
            var hv = eldiven.AddComponent<HVEldiven>();
            if (ilk == null) ilk = hv;
            if (onceki != null)
            {
                onceki.esEldiven = hv;
                hv.esEldiven = onceki;
            }
            onceki = hv;
        }
        return ilk;
    }

    static GameObject ElYap(string ad, Material mat, bool sol)
    {
        var kok = new GameObject(ad);

        var avuc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        avuc.name = "Avuc";
        avuc.transform.SetParent(kok.transform, false);
        avuc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        avuc.transform.localScale = new Vector3(0.09f, 0.1f, 0.05f);
        avuc.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(avuc.GetComponent<Collider>());

        var basparmak = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        basparmak.name = "BasParmak";
        basparmak.transform.SetParent(kok.transform, false);
        basparmak.transform.localPosition = new Vector3(sol ? -0.05f : 0.05f, 0f, 0.03f);
        basparmak.transform.localRotation = Quaternion.Euler(60f, sol ? 30f : -30f, 0f);
        basparmak.transform.localScale = new Vector3(0.028f, 0.045f, 0.028f);
        basparmak.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(basparmak.GetComponent<Collider>());

        var manset = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        manset.name = "Manset";
        manset.transform.SetParent(kok.transform, false);
        manset.transform.localPosition = new Vector3(0f, 0f, -0.13f);
        manset.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        manset.transform.localScale = new Vector3(0.11f, 0.05f, 0.11f);
        manset.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(manset.GetComponent<Collider>());

        return kok;
    }


    static BataryaTepsisi TepsiKur(Bounds aracSinir, Vector3 genisEksen, Vector3 uzunEksen, float zeminY,
        Transform batarya, Dictionary<string, Material> materyaller,
        out GameObject tepsiObj, out GameObject guvenliAlanObj, out GameObject aracAltiObj)
    {
        tepsiObj = new GameObject("TasiyiciTepsi");
        Vector3 baslangic = new Vector3(aracSinir.center.x, zeminY, aracSinir.center.z)
            + genisEksen * (Vector3.Scale(aracSinir.size, genisEksen).magnitude * 0.5f + 3.0f);
        tepsiObj.transform.position = baslangic;
        tepsiObj.transform.rotation = Quaternion.LookRotation(-genisEksen);

        var sase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sase.name = "Sase";
        sase.transform.SetParent(tepsiObj.transform, false);
        sase.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        sase.transform.localScale = new Vector3(0.9f, 0.08f, 1.5f);
        sase.GetComponent<Renderer>().sharedMaterial = materyaller["tepsiSari"];

        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                var teker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                teker.name = "Teker";
                teker.transform.SetParent(tepsiObj.transform, false);
                teker.transform.localPosition = new Vector3(x * 0.38f, 0.06f, z * 0.62f);
                teker.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                teker.transform.localScale = new Vector3(0.12f, 0.03f, 0.12f);
                teker.GetComponent<Renderer>().sharedMaterial = materyaller["tepsiKoyu"];
                Object.DestroyImmediate(teker.GetComponent<Collider>());
            }
        }

        var platform = new GameObject("Platform");
        platform.transform.SetParent(tepsiObj.transform, false);
        platform.transform.localPosition = new Vector3(0f, 0.34f, 0f);

        var tabla = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tabla.name = "Tabla";
        tabla.transform.SetParent(platform.transform, false);
        tabla.transform.localScale = new Vector3(0.85f, 0.06f, 1.4f);
        tabla.GetComponent<Renderer>().sharedMaterial = materyaller["tepsiSari"];

        for (int i = -1; i <= 1; i += 2)
        {
            var makas = GameObject.CreatePrimitive(PrimitiveType.Cube);
            makas.name = "Makas";
            makas.transform.SetParent(platform.transform, false);
            makas.transform.localPosition = new Vector3(i * 0.35f, -0.11f, 0f);
            makas.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
            makas.transform.localScale = new Vector3(0.04f, 0.04f, 0.5f);
            makas.GetComponent<Renderer>().sharedMaterial = materyaller["tepsiKoyu"];
            Object.DestroyImmediate(makas.GetComponent<Collider>());
        }

        var tutamacDikey = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tutamacDikey.name = "TutamacDikey";
        tutamacDikey.transform.SetParent(tepsiObj.transform, false);
        tutamacDikey.transform.localPosition = new Vector3(0f, 0.6f, 0.78f);
        tutamacDikey.transform.localScale = new Vector3(0.03f, 0.5f, 0.03f);
        tutamacDikey.GetComponent<Renderer>().sharedMaterial = materyaller["tepsiSari"];
        Object.DestroyImmediate(tutamacDikey.GetComponent<Collider>());

        var tutamac = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tutamac.name = "Tutamac";
        tutamac.transform.SetParent(tepsiObj.transform, false);
        tutamac.transform.localPosition = new Vector3(0f, 1.1f, 0.78f);
        tutamac.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        tutamac.transform.localScale = new Vector3(0.035f, 0.3f, 0.035f);
        tutamac.GetComponent<Renderer>().sharedMaterial = materyaller["tepsiKoyu"];

        var govdeCol = tepsiObj.AddComponent<BoxCollider>();
        govdeCol.center = new Vector3(0f, 0.25f, 0f);
        govdeCol.size = new Vector3(0.95f, 0.45f, 1.6f);

        var rb = tepsiObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        var grab = tepsiObj.AddComponent<XRGrabInteractable>();
        grab.throwOnDetach = false;
        grab.useDynamicAttach = true;
        grab.colliders.Add(govdeCol);
        grab.colliders.Add(tutamac.GetComponent<Collider>());

        var platformLift = tepsiObj.AddComponent<LiftController>();
        platformLift.platformlar = new[] { platform.transform };
        platformLift.hiz = 0.35f;

        float bataryaAltiY = batarya != null ? SinirHesapla(batarya).min.y : zeminY + 0.42f;
        float platformUstuY = zeminY + 0.37f;
        platformLift.maxYukseklik = Mathf.Clamp(bataryaAltiY + AracLiftYuksekligi - platformUstuY - 0.02f, 0.5f, 2.6f);

        var butonPaneli = new GameObject("TepsiButonlari");
        butonPaneli.transform.SetParent(tepsiObj.transform, false);
        butonPaneli.transform.localPosition = new Vector3(0f, 0.98f, 0.74f);
        ButonOlustur(butonPaneli.transform, platformLift, +1, new Vector3(-0.1f, 0f, 0f), materyaller["butonYesil"]);
        ButonOlustur(butonPaneli.transform, platformLift, -1, new Vector3(0.1f, 0f, 0f), materyaller["butonKirmizi"]);

        aracAltiObj = new GameObject("AracAltiBolgesi");
        aracAltiObj.transform.position = new Vector3(aracSinir.center.x, zeminY, aracSinir.center.z);

        guvenliAlanObj = new GameObject("GuvenliAlan");
        Vector3 guvenliPoz = new Vector3(aracSinir.center.x, zeminY, aracSinir.center.z)
            + uzunEksen * (Vector3.Scale(aracSinir.size, uzunEksen).magnitude * 0.5f + 3.5f);
        guvenliAlanObj.transform.position = guvenliPoz;

        var alanGorsel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        alanGorsel.name = "AlanGorseli";
        alanGorsel.transform.SetParent(guvenliAlanObj.transform, false);
        alanGorsel.transform.localPosition = new Vector3(0f, 0.012f, 0f);
        alanGorsel.transform.localScale = new Vector3(2.4f, 0.01f, 2.4f);
        alanGorsel.GetComponent<Renderer>().sharedMaterial = materyaller["guvenliAlan"];
        Object.DestroyImmediate(alanGorsel.GetComponent<Collider>());

        var yazi = new GameObject("AlanYazisi");
        yazi.transform.SetParent(guvenliAlanObj.transform, false);
        yazi.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        yazi.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var tmp = yazi.AddComponent<TextMeshPro>();
        tmp.text = "GUVENLI ALAN";
        tmp.fontSize = 6f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.05f, 0.4f, 0.1f);
        var rt = yazi.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(3f, 1f);

        var tepsi = tepsiObj.AddComponent<BataryaTepsisi>();
        tepsi.platformLift = platformLift;
        tepsi.platform = platform.transform;
        tepsi.batarya = batarya;
        tepsi.aracAltiBolgesi = aracAltiObj.transform;
        tepsi.guvenliAlan = guvenliAlanObj.transform;

        return tepsi;
    }


    static void GorevYoneticisiKur(LiftController aracLift, HVSoket[] soketler, HVEldiven eldiven,
        BataryaTepsisi tepsi, GameObject liftPaneli, GameObject eldivenGrubu, GameObject tepsiObj,
        GameObject guvenliAlanObj, GameObject soketGrubu)
    {
        var panelObj = new GameObject("TalimatPanel");
        var canvas = panelObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var canvasRt = panelObj.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(1000f, 260f);
        panelObj.transform.localScale = Vector3.one * 0.0011f;

        var arkaplan = new GameObject("Arkaplan");
        arkaplan.transform.SetParent(panelObj.transform, false);
        var img = arkaplan.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0f, 0f, 0f, 0.55f);
        var arkaRt = arkaplan.GetComponent<RectTransform>();
        arkaRt.anchorMin = new Vector2(0f, 0f);
        arkaRt.anchorMax = new Vector2(1f, 0.6f);
        arkaRt.offsetMin = Vector2.zero;
        arkaRt.offsetMax = Vector2.zero;

        var talimatObj = new GameObject("TalimatText");
        talimatObj.transform.SetParent(arkaplan.transform, false);
        var talimat = talimatObj.AddComponent<TextMeshProUGUI>();
        talimat.fontSize = 44f;
        talimat.alignment = TextAlignmentOptions.Center;
        talimat.color = Color.white;
        talimat.textWrappingMode = TextWrappingModes.Normal;
        var tRt = talimatObj.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = new Vector2(20f, 10f);
        tRt.offsetMax = new Vector2(-20f, -10f);

        var uyariObj = new GameObject("UyariText");
        uyariObj.transform.SetParent(panelObj.transform, false);
        var uyari = uyariObj.AddComponent<TextMeshProUGUI>();
        uyari.fontSize = 40f;
        uyari.alignment = TextAlignmentOptions.Center;
        uyari.color = new Color(1f, 0.55f, 0.1f);
        uyari.fontStyle = FontStyles.Bold;
        var uRt = uyariObj.GetComponent<RectTransform>();
        uRt.anchorMin = new Vector2(0f, 0.65f);
        uRt.anchorMax = new Vector2(1f, 1f);
        uRt.offsetMin = Vector2.zero;
        uRt.offsetMax = Vector2.zero;
        uyariObj.SetActive(false);

        var yonetici = new GameObject("GorevYoneticisi").AddComponent<GorevYoneticisi>();
        yonetici.aracLifti = aracLift;
        yonetici.soketler = soketler;
        yonetici.eldiven = eldiven;
        yonetici.tepsi = tepsi;
        yonetici.talimatText = talimat;
        yonetici.uyariText = uyari;
        yonetici.aracLiftButonlari = liftPaneli;
        yonetici.eldivenObjesi = eldivenGrubu;
        yonetici.soketGrubu = soketGrubu;
        yonetici.tepsiObjesi = tepsiObj;
        yonetici.guvenliAlanObjesi = guvenliAlanObj;
    }


    static Transform CocukBul(Transform kok, string ad)
    {
        foreach (var t in kok.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == ad)
                return t;
        }
        foreach (var t in kok.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.StartsWith(ad))
                return t;
        }
        return null;
    }

    static Bounds SinirHesapla(Transform kok)
    {
        var rlar = kok.GetComponentsInChildren<Renderer>();
        if (rlar.Length == 0)
            return new Bounds(kok.position, Vector3.one * 0.1f);
        Bounds b = rlar[0].bounds;
        for (int i = 1; i < rlar.Length; i++)
            b.Encapsulate(rlar[i].bounds);
        return b;
    }
}
