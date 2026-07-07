using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class LiftButonu : MonoBehaviour
{
    public LiftController lift;
    public int yon = 1;
    public Transform butonGorseli;
    public float basilmaDerinligi = 0.006f;

    XRSimpleInteractable _interactable;
    Vector3 _gorselBaslangic;

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        if (butonGorseli != null)
            _gorselBaslangic = butonGorseli.localPosition;
    }

    void OnEnable()
    {
        _interactable.selectEntered.AddListener(Basildi);
        _interactable.selectExited.AddListener(Birakildi);
    }

    void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(Basildi);
        _interactable.selectExited.RemoveListener(Birakildi);
        if (lift != null)
            lift.YonAyarla(0);
    }

    void Basildi(SelectEnterEventArgs _)
    {
        if (lift != null)
            lift.YonAyarla(yon);
        if (butonGorseli != null)
            butonGorseli.localPosition = _gorselBaslangic + Vector3.down * basilmaDerinligi;
    }

    void Birakildi(SelectExitEventArgs _)
    {
        if (lift != null)
            lift.YonAyarla(0);
        if (butonGorseli != null)
            butonGorseli.localPosition = _gorselBaslangic;
    }
}
