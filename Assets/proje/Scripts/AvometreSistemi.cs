using UnityEngine;
using TMPro;

public class AvometreSistemi : MonoBehaviour
{
    public TextMeshProUGUI anaEkranText; 

    private PinKimligi prob1Pin;
    private PinKimligi prob2Pin;

    public bool kalibrasyonTamamlandi = false;
    public int tamamlananTestSayisi = 0;

    public void BaglantiGuncelle(int probNo, PinKimligi pin)
    {
        if (probNo == 1) prob1Pin = pin;
        else prob2Pin = pin;

       
        string renk = (probNo == 1) ? "cyan" : "orange"; 
        Debug.Log($"<color={renk}><b>[PROB {probNo} TAKILDI]</b></color>\n" +
                  $"<b>Obje Adı:</b> {pin.gameObject.name}\n" +
                  $"<b>Grup Adı:</b> {pin.grupAdi}");

        KontrolEt();
    }

    public void BaglantiKopart(int probNo)
    {
        string kopanObjeAdi = "";
        if (probNo == 1) { kopanObjeAdi = prob1Pin != null ? prob1Pin.name : "Bilinmiyor"; prob1Pin = null; }
        else { kopanObjeAdi = prob2Pin != null ? prob2Pin.name : "Bilinmiyor"; prob2Pin = null; }

        Debug.Log($"<color=red><b>[PROB {probNo} AYRILDI]</b></color> -> Ayrılan Obje: {kopanObjeAdi}");
        
        KontrolEt();
    }

    private void KontrolEt()
    {

        if (prob1Pin == null || prob2Pin == null)
        {
            if (anaEkranText != null)
            {
                
                anaEkranText.text = kalibrasyonTamamlandi ? "O.L" : "---";
                anaEkranText.color = Color.white;
            }
            return;
        }

        if (prob1Pin.grupAdi == "KalibrasyonAvo" && prob2Pin.grupAdi == "KalibrasyonAvo")
        {
            if (!kalibrasyonTamamlandi)
            {
                KalibrasyonuGerceklestir();
            }
            else
            {
                anaEkranText.text = "0.000";
                anaEkranText.color = Color.green;
            }
        
            return; 
        }


        if (prob1Pin.grupAdi.Contains("Kalibrasyon") || prob2Pin.grupAdi.Contains("Kalibrasyon"))
    {
        
        return; 
    }

        
        if (prob1Pin.grupAdi == prob2Pin.grupAdi)
        {
            anaEkranText.text = "Doğru";
            anaEkranText.color = Color.green;        
            
         
            tamamlananTestSayisi++;
            Debug.Log("<color=green><b>[BAŞARILI]</b></color> Bir çift test edildi. Toplam: " + tamamlananTestSayisi);
            
            if(tamamlananTestSayisi >= 3) 
            {
                Debug.Log("Tüm testler başarıyla tamamlandı!");
                anaEkranText.text = "Test Tamamlandı";
            }
        }
        else 
        {
           
            anaEkranText.text = "HAYIR";
            anaEkranText.color = Color.red;
        }
    }

    private void KalibrasyonuGerceklestir()
    {
        kalibrasyonTamamlandi = true;
        anaEkranText.text = "0.000";
        anaEkranText.color = Color.green;
        
        Debug.Log("<color=yellow><b>[SİSTEM]</b></color> Cihaz kalibre edildi. Artık ölçüme hazırsınız.");        
        
       
        Invoke("HazirModunaGec", 2.0f);
    }

    private void HazirModunaGec()
    {      
       
        if (prob1Pin != null && prob2Pin != null && prob1Pin.grupAdi == "KalibrasyonAvo")
        {
            anaEkranText.text = "0.000";
            anaEkranText.color = Color.green;
        }
        else
        {
            anaEkranText.text = "O.L";
            anaEkranText.color = Color.white;
        }
    }
}