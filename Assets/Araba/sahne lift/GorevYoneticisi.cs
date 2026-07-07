using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GorevYoneticisi : MonoBehaviour
{
    public static GorevYoneticisi Ornek { get; private set; }

    public enum Adim
    {
        AracKaldir,
        EldivenGiy,
        SoketleriSok,
        TepsiyiAracAltinaGetir,
        BataryayiAl,
        GuvenliAlanaCek,
        Tamamlandi
    }

    [Header("Bağlantılar")]
    public LiftController aracLifti;
    public HVEldiven eldiven;
    public HVSoket[] soketler;
    public BataryaTepsisi tepsi;

    [Header("Ayarlar")]
    public bool eldivenZorunlu = false;

    [Header("Talimat paneli")]
    public TMP_Text talimatText;
    public TMP_Text uyariText;
    public float panelMesafe = 1.6f;

    [Header("Vurgu")]
    public Color vurguRengi = new Color(1f, 0.85f, 0.1f);

    [Header("Adım hedefleri")]
    public GameObject aracLiftButonlari;
    public GameObject eldivenObjesi;
    public GameObject soketGrubu;
    public GameObject tepsiObjesi;
    public GameObject guvenliAlanObjesi;

    public Adim MevcutAdim { get; private set; } = Adim.AracKaldir;

    static readonly string[] Talimatlar =
    {
        "Aracı kaldırmak için lift kontrolündeki YEŞİL (yukarı) butona basılı tut.",
        "Masadaki yüksek gerilim eldivenlerini al ve giy.",
        "Bataryanın 3 soket bağlantısını tutup çekerek sök. (0/3)",
        "Taşıyıcı tepsiyi tutamacından tutup aracın altına, bataryanın hizasına çek.",
        "Tepsinin YEŞİL (yukarı) butonuyla platformu kaldır ve bataryayı al.",
        "Bataryayı elinle tut, indir ve yeşil GÜVENLİ ALANA bırak.",
        "GÖREV TAMAMLANDI! Batarya güvenle söküldü."
    };

    readonly List<Renderer> _vurgulular = new List<Renderer>();
    Coroutine _uyariRutini;
    int _sokulenSoket;
    Transform _panel;

    void Awake()
    {
        Ornek = this;
    }

    void Start()
    {
        if (talimatText != null)
            _panel = talimatText.canvas != null ? talimatText.canvas.transform : talimatText.transform;

        foreach (var s in soketler)
        {
            if (s != null)
                s.sokuldu.AddListener(SoketSokuldu);
        }
        AdimaGec(Adim.AracKaldir);
    }

    void Update()
    {
        switch (MevcutAdim)
        {
            case Adim.AracKaldir:
                if (aracLifti != null && aracLifti.YukariCalisti)
                    AdimaGec(eldivenZorunlu ? Adim.EldivenGiy : Adim.SoketleriSok);
                break;
            case Adim.EldivenGiy:
                if (HVEldiven.Giyildi)
                    AdimaGec(Adim.SoketleriSok);
                break;
            case Adim.TepsiyiAracAltinaGetir:
                if (tepsi != null && (tepsi.AracAltinda || tepsi.BataryaAlindiMi))
                    AdimaGec(Adim.BataryayiAl);
                break;
            case Adim.BataryayiAl:
                if (tepsi != null && tepsi.BataryaAlindiMi)
                    AdimaGec(Adim.GuvenliAlanaCek);
                break;
            case Adim.GuvenliAlanaCek:
                if (tepsi != null && tepsi.BataryaGuvenliAlanda)
                    AdimaGec(Adim.Tamamlandi);
                break;
        }
    }

    void LateUpdate()
    {
        PaneliKamerayaTasi();
        VurgulariCiz();
    }

    void SoketSokuldu()
    {
        _sokulenSoket++;
        if (MevcutAdim == Adim.SoketleriSok && talimatText != null)
            talimatText.text = Talimatlar[(int)Adim.SoketleriSok].Replace("(0/3)", $"({_sokulenSoket}/{soketler.Length})");

        if (_sokulenSoket >= soketler.Length && MevcutAdim == Adim.SoketleriSok)
            AdimaGec(Adim.TepsiyiAracAltinaGetir);
    }

    void AdimaGec(Adim adim)
    {
        MevcutAdim = adim;

        if (talimatText != null)
            talimatText.text = Talimatlar[(int)adim];

        bool soketAdimi = adim == Adim.SoketleriSok;
        foreach (var s in soketler)
        {
            if (s != null)
                s.kilitli = !soketAdimi && !s.Sokuldu;
        }

        if (tepsi != null)
            tepsi.bataryaAlinabilir = adim >= Adim.TepsiyiAracAltinaGetir && adim != Adim.Tamamlandi;

        VurguyuAyarla(adim switch
        {
            Adim.AracKaldir => aracLiftButonlari,
            Adim.EldivenGiy => eldivenObjesi,
            Adim.SoketleriSok => soketGrubu,
            Adim.TepsiyiAracAltinaGetir => tepsiObjesi,
            Adim.BataryayiAl => tepsiObjesi,
            Adim.GuvenliAlanaCek => guvenliAlanObjesi,
            _ => null
        });
    }

    public void UyariGoster(string mesaj)
    {
        if (uyariText == null)
            return;
        if (_uyariRutini != null)
            StopCoroutine(_uyariRutini);
        _uyariRutini = StartCoroutine(UyariRutini(mesaj));
    }

    IEnumerator UyariRutini(string mesaj)
    {
        uyariText.text = mesaj;
        uyariText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        uyariText.gameObject.SetActive(false);
        _uyariRutini = null;
    }

    void PaneliKamerayaTasi()
    {
        if (_panel == null)
            return;
        Camera kamera = Camera.main;
        if (kamera == null)
            return;

        Vector3 hedefPoz = kamera.transform.position
            + kamera.transform.forward * panelMesafe
            - kamera.transform.up * (panelMesafe * 0.35f);
        _panel.position = Vector3.Lerp(_panel.position, hedefPoz, Time.deltaTime * 6f);
        _panel.rotation = Quaternion.Slerp(
            _panel.rotation,
            Quaternion.LookRotation(_panel.position - kamera.transform.position),
            Time.deltaTime * 6f);
    }

    void VurguyuAyarla(GameObject hedef)
    {
        foreach (var r in _vurgulular)
        {
            if (r != null)
                r.SetPropertyBlock(null);
        }
        _vurgulular.Clear();

        if (hedef == null)
            return;
        foreach (var r in hedef.GetComponentsInChildren<Renderer>())
        {
            if (!(r is LineRenderer))
                _vurgulular.Add(r);
        }
    }

    void VurgulariCiz()
    {
        if (_vurgulular.Count == 0)
            return;
        float nabiz = 0.45f + 0.35f * Mathf.PingPong(Time.time * 1.6f, 1f);
        Color renk = Color.Lerp(Color.white, vurguRengi, nabiz);
        var mpb = new MaterialPropertyBlock();
        mpb.SetColor("_BaseColor", renk);
        mpb.SetColor("_Color", renk);
        foreach (var r in _vurgulular)
        {
            if (r != null)
                r.SetPropertyBlock(mpb);
        }
    }
}
