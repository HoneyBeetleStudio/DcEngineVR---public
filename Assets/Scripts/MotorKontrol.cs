using UnityEngine;
using UnityEngine.InputSystem; // Tıklama için
using TMPro; // TextMeshPro için

public class MotorKontrol : MonoBehaviour
{
    [Header("--- 1. ANA PARÇA ---")]
    public Transform pervaneObjesi; 

    [Header("--- 2. GÖRSEL AYARLAR ---")]
    public TextMeshPro durumYazisi; 
    public Renderer butonRenderer;  
    public Material kirmiziMat;     
    public Material yesilMat;       

    [Header("--- 3. HIZ AYARLARI ---")]
    public float donusHizi = 1000f;
    public Vector3 donusYonVektoru = new Vector3(0, 0, 1);

    // Özel Değişkenler
    private bool motorAcik = false;
    private bool uzerineBakiliyor = false;

    void Start()
    {
        GorselleriGuncelle();
    }

    void Update()
    {
        // Pervane Dönüşü
        if (motorAcik && pervaneObjesi != null)
        {
            pervaneObjesi.Rotate(donusYonVektoru * donusHizi * Time.deltaTime, Space.Self);
        }

        // Tıklama (Sol Tık)
        if (uzerineBakiliyor && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) 
        {
            DurumuDegistir();
        }
    }

    void DurumuDegistir()
    {
        motorAcik = !motorAcik; 
        GorselleriGuncelle();   
    }

    void GorselleriGuncelle()
    {
        // ÖNCE YAZI RENGİNİ SABİTLE (Her zaman Siyah)
        if (durumYazisi != null)
        {
            durumYazisi.color = Color.black; 
        }

        if (motorAcik)
        {
            // --- AÇIK DURUMU ---
            if(durumYazisi != null) durumYazisi.text = "ÇALIŞIYOR";
            
            // Sadece butonun malzemesi yeşil olsun (Yazı siyah kalır)
            if(butonRenderer != null) butonRenderer.material = yesilMat;

            Debug.Log("DURUM: Motor Çalışıyor");
        }
        else
        {
            // --- KAPALI DURUMU ---
            if(durumYazisi != null) durumYazisi.text = "DURDU";

            // Sadece butonun malzemesi kırmızı olsun
            if(butonRenderer != null) butonRenderer.material = kirmiziMat;

            Debug.Log("DURUM: Motor Durdu");
        }
    }

    // --- XR Olayları ---
    public void HoverGiris() { uzerineBakiliyor = true; }
    public void HoverCikis() { uzerineBakiliyor = false; }
}