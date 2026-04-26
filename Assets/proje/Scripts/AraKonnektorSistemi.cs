using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AraKonnektorSistemi : MonoBehaviour
{
    private XRGrabInteractable _cihazGrab;
    public SnapTarget[] pinYuvalari; 

    private void Awake()
    {
        _cihazGrab = GetComponent<XRGrabInteractable>();
    }

    private void Update()
    {
        bool herhangiBirPinTakiliMi = false;

        foreach (var yuva in pinYuvalari)
        {
            if (yuva.isConnected)
            {
                herhangiBirPinTakiliMi = true;
                break;
            }
        }

        if (herhangiBirPinTakiliMi)
        {
            _cihazGrab.enabled = false;
        }
        else
        {
            _cihazGrab.enabled = true;
        }
    }
}