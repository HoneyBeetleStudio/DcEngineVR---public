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

    [Header("Checklist entegrasyonu")]
    [Tooltip("Assign to let the checklist own the instruction text and receive auto-completion.")]
    public VRChecklistManager checklist;
    [Tooltip("Checklist task index for each Adim, in enum order. -1 = no matching task.")]
    public int[] adimGorevIndeksleri = { -1, -1, -1, -1, -1, -1, -1 };

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
        "Press the GREEN (up) button on the lift control to raise the vehicle.",
        "Pick up the high voltage gloves from the table and put them on.",
        "Grab and pull to disconnect the battery's 3 cable/socket connections. (0/3)",
        "The carrier tray is fixed under the vehicle. Press the GREEN (up) button.",
        "Press the tray's GREEN (up) button so the battery becomes movable.",
        "Grab the battery with your hand and drop it in the green SAFE ZONE.",
        "TASK COMPLETE! The battery has been safely removed."
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
                if (tepsi != null && tepsi.AracAltinda)
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
        if (MevcutAdim == Adim.SoketleriSok)
        {
            if (checklist != null)
                checklist.SetActiveSuffix($"({_sokulenSoket}/{soketler.Length})");
            else if (talimatText != null)
                talimatText.text = Talimatlar[(int)Adim.SoketleriSok].Replace("(0/3)", $"({_sokulenSoket}/{soketler.Length})");
        }

        if (_sokulenSoket >= soketler.Length && MevcutAdim == Adim.SoketleriSok)
        {
            if (tepsi != null && tepsi.AracAltinda)
                AdimaGec(Adim.BataryayiAl);
            else
                AdimaGec(Adim.TepsiyiAracAltinaGetir);
        }
    }

    void AdimaGec(Adim adim)
    {
        if (adim != MevcutAdim)
            GoreviTamamla(MevcutAdim);

        MevcutAdim = adim;

        if (talimatText != null && checklist == null)
            talimatText.text = Talimatlar[(int)adim];

        bool soketAdimi = adim == Adim.SoketleriSok;
        foreach (var s in soketler)
        {
            if (s != null)
                s.kilitli = !soketAdimi && !s.Sokuldu;
        }

        if (tepsi != null)
            tepsi.bataryaAlinabilir = adim >= Adim.TepsiyiAracAltinaGetir && adim != Adim.Tamamlandi;

        if (checklist != null)
            return;

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

    void GoreviTamamla(Adim adim)
    {
        if (checklist == null || adimGorevIndeksleri == null)
            return;
        int i = (int)adim;
        if (i < 0 || i >= adimGorevIndeksleri.Length)
            return;
        int gorev = adimGorevIndeksleri[i];
        if (gorev >= 0)
            checklist.CompleteTask(gorev);
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
