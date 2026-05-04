using UnityEngine;
using TMPro;

public class HVManager : MonoBehaviour
{
    public static HVManager Instance;

    [Header("Soket Referansları")]
    public SnapTarget soketArtı;
    public SnapTarget soketEksi;

    [Header("Doğrulama Ayarları")]
    public string beklenenPinIsmi = "HV_Pin"; 
    public string beklenenSoketGrubu = "HV_Test_Unitesi"; 

    [Header("UI")]
    public TextMeshProUGUI durumText;

    private void Awake() => Instance = this;

    public void BaglantiDurumunuGuncelle()
    {
        if (soketArtı == null || soketEksi == null) return;
        
        if (soketArtı.isConnected && soketEksi.isConnected)
        {          
            bool artıDogru = soketArtı.snappableObject.name.Contains(beklenenPinIsmi);
            bool eksiDogru = soketEksi.snappableObject.name.Contains(beklenenPinIsmi);
           
            PinKimligi pinArtı = soketArtı.GetComponent<PinKimligi>();
            PinKimligi pinEksi = soketEksi.GetComponent<PinKimligi>();

            if (artıDogru && eksiDogru && pinArtı.grupAdi == beklenenSoketGrubu && pinEksi.grupAdi == beklenenSoketGrubu)
            {
                CihaziCalistir();
            }
            else
            {
                CihaziKapat("Hatalı Bağlantı veya Cihaz!");
            }
        }
        else
        {
            CihaziKapat("Bağlantı Bekleniyor...");
        }
    }

    void CihaziCalistir()
    {
        durumText.text = "12V";
        durumText.color = Color.red;
    }

    void CihaziKapat(string mesaj)
    {
        durumText.text = mesaj;
        durumText.color = Color.white;
    }
}