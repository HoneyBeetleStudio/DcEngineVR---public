using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class KabloGrabHighlight : MonoBehaviour
{
    public List<SnapTarget> snapTargetlar;
    public bool otomatikBul = true;
    public KabloYonOku yonOku;

    private XRGrabInteractable _grabInteractable;

    private void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            _grabInteractable.selectExited.RemoveListener(OnReleased);
            _grabInteractable.selectEntered.AddListener(OnGrabbed);
            _grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void Start()
    {
        if (otomatikBul && (snapTargetlar == null || snapTargetlar.Count == 0))
            snapTargetlar = new List<SnapTarget>(FindObjectsByType<SnapTarget>(FindObjectsSortMode.None));
    }

    private void OnEnable()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.AddListener(OnGrabbed);
            _grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            _grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (snapTargetlar != null)
        {
            foreach (var target in snapTargetlar)
            {
                if (target != null && !target.isConnected)
                    target.HighlightAc();
            }
        }

        if (yonOku != null)
            yonOku.KabloTutuldu(this, snapTargetlar);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (snapTargetlar != null)
        {
            foreach (var target in snapTargetlar)
            {
                if (target != null)
                    target.HighlightKapat();
            }
        }

        bool birSoketeBaglandi = false;
        if (snapTargetlar != null)
        {
            foreach (var target in snapTargetlar)
            {
                if (target != null && target.isConnected)
                {
                    birSoketeBaglandi = true;
                    break;
                }
            }
        }

        if (yonOku != null && !birSoketeBaglandi)
            yonOku.KabloBirakildi();
    }
}
