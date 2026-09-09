using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class ControllerHandPoser : MonoBehaviour
{
    public enum Yon { Sol, Sag }

    [Header("El")]
    public Yon yon = Yon.Sol;

    [Header("Girdi (Input System yolları)")]
    public string gripPath = "<XRController>{LeftHand}/grip";
    public string triggerPath = "<XRController>{LeftHand}/trigger";

    [Header("Poz")]
    [Range(0f, 140f)] public float maxKivrim = 82f;
    [Range(0f, 140f)] public float basparmakKivrim = 32f;
    [Tooltip("Hiç girdi yokken parmakların rahat duruş miktarı (0-1).")]
    [Range(0f, 1f)] public float dinlenmeKivrimi = 0.15f;
    public float yumusatma = 14f;
    [Tooltip("Parmaklar ters yöne kıvrılıyorsa işaretle. Inspector'dan anında denenebilir.")]
    public bool kivrimYonunuTersCevir = false;

    [System.Serializable]
    class Eklem
    {
        public Transform t;
        public Quaternion rest;
        public Vector3 axis;     // in the joint's own space
        public float weight;     // per-joint share of the total curl
    }

    [System.Serializable]
    class Parmak
    {
        public string ad;
        public List<Eklem> eklemler = new List<Eklem>();
        public float hedef;
        public float suan;
        public float maxAci;
    }

    [SerializeField, HideInInspector] List<Parmak> _parmaklar = new List<Parmak>();
    InputControl<float> _grip;
    InputControl<float> _trigger;
    [SerializeField, HideInInspector] bool _hazir;

    void Kur()
    {
        _parmaklar.Clear();
        string p = yon == Yon.Sol ? "L_" : "R_";
        Transform bilek = Bul(p + "Wrist");
        Transform idxProx = Bul(p + "IndexProximal");
        Transform litProx = Bul(p + "LittleProximal");
        if (bilek == null || idxProx == null || litProx == null)
        {
            Debug.LogError($"[{name}] el kemikleri bulunamadı ({p}Wrist / {p}IndexProximal / {p}LittleProximal).", this);
            enabled = false;
            return;
        }

        // Plane of the palm. The winding of this cross product flips between the left and
        // right models, so the right hand needs the negation to keep the normal pointing
        // OUT of the palm (measured: left gives -Y, right gives +Y, palm is -Y on both).
        Vector3 avucNormali = Vector3.Cross(idxProx.position - bilek.position,
                                            litProx.position - bilek.position).normalized;
        if (yon == Yon.Sag)
            avucNormali = -avucNormali;

        foreach (var ad in new[] { "Index", "Middle", "Ring", "Little" })
        {
            var f = new Parmak { ad = ad, maxAci = maxKivrim };
            Ekle(f, p + ad + "Proximal",     p + ad + "Intermediate", avucNormali, 1.0f);
            Ekle(f, p + ad + "Intermediate", p + ad + "Distal",       avucNormali, 0.95f);
            Ekle(f, p + ad + "Distal",       p + ad + "Tip",          avucNormali, 0.55f);
            if (f.eklemler.Count > 0) _parmaklar.Add(f);
        }

        var bas = new Parmak { ad = "Thumb", maxAci = basparmakKivrim };
        Ekle(bas, p + "ThumbProximal", p + "ThumbDistal", avucNormali, 1.0f);
        Ekle(bas, p + "ThumbDistal",   p + "ThumbTip",    avucNormali, 0.7f);
        if (bas.eklemler.Count > 0) _parmaklar.Add(bas);

        _hazir = _parmaklar.Count > 0;
        if (!_hazir) Debug.LogError($"[{name}] parmak kemikleri bulunamadı.", this);
    }

    void Awake()
    {
        if (!_hazir || _parmaklar == null || _parmaklar.Count == 0)
            Kur();
    }

    /// <summary>
    /// The hinge axis is perpendicular to both the bone and the palm normal, so the joint
    /// swings inside the curl plane. The direction is chosen by testing which way actually
    /// carries the next joint toward the palm side.
    /// </summary>
    void Ekle(Parmak parmak, string eklemAd, string cocukAd, Vector3 avucNormali, float agirlik)
    {
        Transform j = Bul(eklemAd);
        Transform c = Bul(cocukAd);
        if (j == null || c == null) return;

        Vector3 kemik = (c.position - j.position).normalized;
        if (kemik.sqrMagnitude < 1e-8f) return;

        Vector3 eksenDunya = Vector3.Cross(kemik, avucNormali);
        if (eksenDunya.sqrMagnitude < 1e-6f) return;          // bone parallel to the normal: no usable hinge
        eksenDunya.Normalize();

        Quaternion rest = j.localRotation;
        Vector3 eksenYerel = Quaternion.Inverse(j.rotation) * eksenDunya;
        Vector3 restCocuk = c.position;

        j.localRotation = rest * Quaternion.AngleAxis(30f, eksenYerel);
        float arti = Vector3.Dot(c.position - restCocuk, -avucNormali);
        j.localRotation = rest * Quaternion.AngleAxis(-30f, eksenYerel);
        float eksi = Vector3.Dot(c.position - restCocuk, -avucNormali);
        j.localRotation = rest;

        if (eksi > arti) eksenYerel = -eksenYerel;

        parmak.eklemler.Add(new Eklem { t = j, rest = rest, axis = eksenYerel, weight = agirlik });
    }

    Transform Bul(string ad)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
            if (t.name == ad) return t;
        return null;
    }

    void OnEnable()
    {
        _grip = InputSystem.FindControl(gripPath) as InputControl<float>;
        _trigger = InputSystem.FindControl(triggerPath) as InputControl<float>;
    }

    void Update()
    {
        if (!_hazir || _parmaklar.Count == 0)
        {
            Kur();                       // a play-mode recompile clears these; rebuild instead of going dead
            if (!_hazir) return;
        }

        if (_grip == null) _grip = InputSystem.FindControl(gripPath) as InputControl<float>;
        if (_trigger == null) _trigger = InputSystem.FindControl(triggerPath) as InputControl<float>;

        float grip = _grip != null ? Mathf.Clamp01(_grip.ReadValue()) : 0f;
        float trigger = _trigger != null ? Mathf.Clamp01(_trigger.ReadValue()) : 0f;
        float k = dinlenmeKivrimi;

        foreach (var f in _parmaklar)
        {
            if (f.ad == "Index")      f.hedef = Mathf.Max(trigger, grip * 0.35f, k);
            else if (f.ad == "Thumb") f.hedef = Mathf.Max(grip, trigger * 0.25f, k);
            else                      f.hedef = Mathf.Max(grip, k);

            f.suan = yumusatma > 0f
                ? Mathf.Lerp(f.suan, f.hedef, 1f - Mathf.Exp(-yumusatma * Time.deltaTime))
                : f.hedef;

            float aci = f.suan * f.maxAci * (kivrimYonunuTersCevir ? -1f : 1f);
            foreach (var e in f.eklemler)
                e.t.localRotation = e.rest * Quaternion.AngleAxis(aci * e.weight, e.axis);
        }
    }
}
