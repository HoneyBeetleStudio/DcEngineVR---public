using UnityEngine;
using UnityEngine.InputSystem; 
using TMPro; 

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
        
        if (motorAcik && pervaneObjesi != null)
        {
            pervaneObjesi.Rotate(donusYonVektoru * donusHizi * Time.deltaTime, Space.Self);
        }

       
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
        
        if (durumYazisi != null)
        {
            durumYazisi.color = Color.black; 
        }

        if (motorAcik)
        {
            
            if(durumYazisi != null) durumYazisi.text = "ÇALIŞIYOR";
            
            
            if(butonRenderer != null) butonRenderer.material = yesilMat;

            Debug.Log("DURUM: Motor Çalışıyor");
        }
        else
        {
         
            if(durumYazisi != null) durumYazisi.text = "DURDU";

           
            if(butonRenderer != null) butonRenderer.material = kirmiziMat;

            Debug.Log("DURUM: Motor Durdu");
        }
    }

    
    public void HoverGiris() { uzerineBakiliyor = true; }
    public void HoverCikis() { uzerineBakiliyor = false; }
}