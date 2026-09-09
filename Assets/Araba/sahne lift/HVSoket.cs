using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class HVSoket : MonoBehaviour
{
    public bool kilitli = true;
    public float sokmeMesafesi = 0.12f;
    public UnityEvent sokuldu;
    public string eldivenUyarisi = "You must put on the high voltage gloves first!";

    public bool Sokuldu { get; private set; }

    XRGrabInteractable _grab;
    Rigidbody _rb;
    Vector3 _yuvaPozisyonu;
    Quaternion _yuvaRotasyonu;
    Transform _yuvaParent;
    Transform _kabloKok;
    LineRenderer _kabloCizgi;
    Transform _kabloGorsel;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
        var kablo = transform.Find("Kablo");
        if (kablo != null)
            _kabloGorsel = kablo;
    }

    void Start()
    {
        _yuvaParent = transform.parent;
        _yuvaPozisyonu = transform.localPosition;
        _yuvaRotasyonu = transform.localRotation;
        _rb.isKinematic = true;
        _rb.useGravity = false;
        KabloCizgisiniKur();
    }

    void OnEnable()
    {
        _grab.selectEntered.AddListener(Tutuldu);
        _grab.selectExited.AddListener(Birakildi);
    }

    void OnDisable()
    {
        _grab.selectEntered.RemoveListener(Tutuldu);
        _grab.selectExited.RemoveListener(Birakildi);
    }

    void LateUpdate()
    {
        KabloCizgisiniGuncelle();
    }

    void Tutuldu(SelectEnterEventArgs args)
    {
        bool eldivenTamam = HVEldiven.Giyildi
            || GorevYoneticisi.Ornek == null
            || !GorevYoneticisi.Ornek.eldivenZorunlu;
        bool izinVar = !kilitli && eldivenTamam;
        if (izinVar)
            return;

        if (!eldivenTamam && !kilitli && GorevYoneticisi.Ornek != null)
            GorevYoneticisi.Ornek.UyariGoster(eldivenUyarisi);

        var interactor = args.interactorObject as IXRSelectInteractor;
        if (interactor != null && _grab.interactionManager != null)
            _grab.interactionManager.SelectExit(interactor, _grab);
    }

    void Update()
    {
        if (Sokuldu || !_grab.isSelected)
            return;

        Vector3 yuvaDunya = _yuvaParent != null
            ? _yuvaParent.TransformPoint(_yuvaPozisyonu)
            : _yuvaPozisyonu;

        if (Vector3.Distance(transform.position, yuvaDunya) >= sokmeMesafesi)
        {
            Sokuldu = true;
            sokuldu?.Invoke();
        }
    }

    void Birakildi(SelectExitEventArgs _)
    {
        if (Sokuldu)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            if (_kabloCizgi != null)
                _kabloCizgi.enabled = false;
        }
        else
        {
            transform.SetParent(_yuvaParent, true);
            transform.localPosition = _yuvaPozisyonu;
            transform.localRotation = _yuvaRotasyonu;
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    void KabloCizgisiniKur()
    {
        var kokObj = new GameObject("KabloKok");
        Vector3 kokPoz = _kabloGorsel != null
            ? _kabloGorsel.position
            : transform.position + Vector3.up * 0.08f;
        kokObj.transform.position = kokPoz;
        kokObj.transform.SetParent(_yuvaParent != null ? _yuvaParent : transform, true);
        _kabloKok = kokObj.transform;

        if (_kabloGorsel != null)
            _kabloGorsel.gameObject.SetActive(false);

        _kabloCizgi = gameObject.AddComponent<LineRenderer>();
        _kabloCizgi.positionCount = 2;
        _kabloCizgi.startWidth = 0.012f;
        _kabloCizgi.endWidth = 0.012f;
        _kabloCizgi.useWorldSpace = true;
        _kabloCizgi.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _kabloCizgi.receiveShadows = false;
        _kabloCizgi.numCapVertices = 4;
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        if (shader != null)
        {
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", new Color(0.12f, 0.12f, 0.12f, 1f));
            else
                mat.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            _kabloCizgi.sharedMaterial = mat;
        }
        KabloCizgisiniGuncelle();
    }

    void KabloCizgisiniGuncelle()
    {
        if (_kabloCizgi == null || !_kabloCizgi.enabled || _kabloKok == null)
            return;
        _kabloCizgi.SetPosition(0, _kabloKok.position);
        _kabloCizgi.SetPosition(1, transform.position);
    }
}
