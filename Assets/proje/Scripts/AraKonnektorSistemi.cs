using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AraKonnektorSistemi : MonoBehaviour
{
    private XRGrabInteractable _cihazGrab;
    public SnapTarget[] pinYuvalari; 

    [Header("Kalibrasyon Ayarları")]
    public string kalibrasyonGrupAdi = "KalibrasyonKonnektor";
    public bool kalibrasyonTamamlandi = false;

    [Header("Bağlantı Durumu")]
    public bool bataryayaBagli = false;

    private AvometreSistemi _avo;

    private void Awake()
    {
        _cihazGrab = GetComponent<XRGrabInteractable>();
        _avo = Object.FindAnyObjectByType<AvometreSistemi>();
    }

    private void Update()
    {
        bool herhangiBirPinTakiliMi = false;
        int kalibrasyonPinSayisi = 0;

        foreach (var yuva in pinYuvalari)
        {
            if (yuva.isConnected)
            {
                herhangiBirPinTakiliMi = true;

       
                PinKimligi pin = yuva.snappableObject != null ? yuva.snappableObject.GetComponent<PinKimligi>() : null;
                if (!kalibrasyonTamamlandi && pin != null && pin.grupAdi == kalibrasyonGrupAdi)
                {
                    kalibrasyonPinSayisi++;
                }
            }
        }

        
        _cihazGrab.enabled = !herhangiBirPinTakiliMi;

        
        if (!kalibrasyonTamamlandi && kalibrasyonPinSayisi == 2)
        {
            KonnektorKalibreEt();
        }
    }

    private void KonnektorKalibreEt()
    {
        kalibrasyonTamamlandi = true;

        if (_avo != null)
        {
      
            _avo.anaEkranText.text = "0.000";
            _avo.anaEkranText.color = Color.green;
            _avo.Invoke("HazirModunaGec", 2.0f);
        }

        Debug.Log("Ara Konnektör Kalibrasyonu Tamamlandı.");
    }

  

public void BataryayaTakildi() { bataryayaBagli = true; }
public void BataryadanCikarildi() { bataryayaBagli = false; }
}