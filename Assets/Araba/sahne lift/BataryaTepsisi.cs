using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class BataryaTepsisi : MonoBehaviour
{
    [Header("Bağlantılar")]
    public LiftController platformLift;
    public Transform platform;
    public Transform batarya;
    public Transform aracAltiBolgesi;
    public Transform guvenliAlan;

    [Header("Ayarlar")]
    public float bolgeYaricapi = 0.9f;
    public float bataryaAlmaMesafesi = 0.12f;

    [Header("Olaylar")]
    public UnityEvent bataryaAlindi;
    public UnityEvent guvenliAlanaVarildi;

    public bool AracAltinda => BolgedeMi(aracAltiBolgesi);
    public bool GuvenliAlanda => BolgedeMi(guvenliAlan);
    public bool BataryaAlindiMi { get; private set; }

    public bool BataryaGuvenliAlanda
    {
        get
        {
            if (!BataryaAlindiMi || batarya == null || guvenliAlan == null)
                return false;
            Vector3 fark = SinirHesapla(batarya).center - guvenliAlan.position;
            fark.y = 0f;
            return fark.magnitude <= bolgeYaricapi;
        }
    }

    public bool bataryaAlinabilir;

    XRGrabInteractable _grab;
    Rigidbody _rb;
    float _zeminY;
    bool _guvenliAlanBildirildi;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _zeminY = transform.position.y;
    }

    void LateUpdate()
    {
        Vector3 poz = transform.position;
        poz.y = _zeminY;
        transform.position = poz;

        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);

        BataryaAlmayiDene();
        GuvenliAlanKontrol();
    }

    void BataryaAlmayiDene()
    {
        if (BataryaAlindiMi || !bataryaAlinabilir || batarya == null || platform == null)
            return;

        Bounds platformSinir = SinirHesapla(platform);
        Bounds bataryaSinir = SinirHesapla(batarya);
        float dikeyMesafe = bataryaSinir.min.y - platformSinir.max.y;
        Vector3 fark = bataryaSinir.center - platformSinir.center;
        fark.y = 0f;

        bool zorlaAl = platformLift != null && platformLift.AktifYon == 1;

        if (zorlaAl || (dikeyMesafe <= bataryaAlmaMesafesi && fark.magnitude <= bolgeYaricapi))
        {
            BataryaAlindiMi = true;
            BataryayiElleTasinabilirYap();
            bataryaAlindi?.Invoke();
        }
    }

    void BataryayiElleTasinabilirYap()
    {
        if (batarya.GetComponent<XRGrabInteractable>() != null)
            return;

        if (batarya.GetComponentInChildren<Collider>() == null)
        {
            Bounds b = SinirHesapla(batarya);
            var col = batarya.gameObject.AddComponent<BoxCollider>();
            col.center = batarya.InverseTransformPoint(b.center);
            Vector3 olcek = batarya.lossyScale;
            col.size = new Vector3(
                b.size.x / Mathf.Max(Mathf.Abs(olcek.x), 0.0001f),
                b.size.y / Mathf.Max(Mathf.Abs(olcek.y), 0.0001f),
                b.size.z / Mathf.Max(Mathf.Abs(olcek.z), 0.0001f));
        }

        var rb = batarya.GetComponent<Rigidbody>();
        if (rb == null)
            rb = batarya.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        var grab = batarya.gameObject.AddComponent<XRGrabInteractable>();
        grab.throwOnDetach = false;
        grab.useDynamicAttach = true;
        grab.selectEntered.AddListener(_ => batarya.SetParent(null, true));
    }

    void GuvenliAlanKontrol()
    {
        if (!BataryaAlindiMi || _guvenliAlanBildirildi)
            return;
        if (GuvenliAlanda && platformLift != null && platformLift.AlttaMi)
        {
            _guvenliAlanBildirildi = true;
            guvenliAlanaVarildi?.Invoke();
        }
    }

    bool BolgedeMi(Transform bolge)
    {
        if (bolge == null)
            return false;
        Vector3 fark = transform.position - bolge.position;
        fark.y = 0f;
        return fark.magnitude <= bolgeYaricapi;
    }

    static Bounds SinirHesapla(Transform kok)
    {
        var rlar = kok.GetComponentsInChildren<Renderer>();
        if (rlar.Length == 0)
            return new Bounds(kok.position, Vector3.zero);
        Bounds b = rlar[0].bounds;
        for (int i = 1; i < rlar.Length; i++)
            b.Encapsulate(rlar[i].bounds);
        return b;
    }
}
