using UnityEngine;
using TMPro;

public class AvometreSistemi : MonoBehaviour
{
    public TextMeshProUGUI anaEkranText; 

    private PinKimligi prob1Pin;
    private PinKimligi prob2Pin;

    public void BaglantiGuncelle(int probNo, PinKimligi pin)
    {
        if (probNo == 1) prob1Pin = pin;
        else prob2Pin = pin;

        // --- CONSOLE LOG ÇIKTISI ---
        string renk = (probNo == 1) ? "cyan" : "orange"; 
        Debug.Log($"<color={renk}><b>[PROB {probNo} TAKILDI]</b></color>\n" +
                  $"<b>Obje Adı:</b> {pin.gameObject.name}\n" +
                  $"<b>Grup Adı:</b> {pin.grupAdi}");
        // ---------------------------

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

  public int tamamlananTestSayisi = 0;

    private void KontrolEt()
    {
        if (prob1Pin == null || prob2Pin == null)
        {
            if (anaEkranText != null)
            {
                anaEkranText.text = "---";
                anaEkranText.color = Color.white;
            }
            return;
        }

        if (prob1Pin.grupAdi == prob2Pin.grupAdi)
    {
        anaEkranText.text = "EVET";
        anaEkranText.color = Color.green;        
        Debug.Log("Bir çift başarıyla test edildi. Toplam: " + (++tamamlananTestSayisi));
        
        if(tamamlananTestSayisi >= 3) {
            Debug.Log("Tüm testler başarıyla tamamlandı!");
            anaEkranText.text = "TEST OK";
        }
    }
    else {
        anaEkranText.text = "HAYIR";
    }
}
}