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
    public string eldivenUyarisi = "Önce yüksek gerilim eldivenlerini giymelisin!";

    public bool Sokuldu { get; private set; }

    XRGrabInteractable _grab;
    Rigidbody _rb;
    Vector3 _yuvaPozisyonu;
    Quaternion _yuvaRotasyonu;
    Transform _yuvaParent;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        _yuvaParent = transform.parent;
        _yuvaPozisyonu = transform.localPosition;
        _yuvaRotasyonu = transform.localRotation;
        _rb.isKinematic = true;
        _rb.useGravity = false;
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
}
