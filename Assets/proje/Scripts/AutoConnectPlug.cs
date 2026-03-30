using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
// Unity XR Interaction Toolkit 3.0 ve sonrası için eklenen yeni yollar (Hatanın Çözümü):
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AutoConnectPlug : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    [Tooltip("Fişin uçup gireceği hedef nokta (Hizalama objesi)")]
    public Transform targetSocket;

    [Header("Olaylar")] // YENİ EKLENDİ
    [Tooltip("Fiş yuvaya tam oturduğunda çalışacak şeyler")]
    public UnityEvent onConnected; // YENİ EKLENDİ
    
    [Tooltip("Uçuş animasyonunun kaç saniye süreceği")]
    public float flyDuration = 0.4f;

    private bool isConnected = false;
    private XRBaseInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
    }

    // Bu fonksiyonu XR Event'lerinden çağıracağız
    public void SendToSocket()
    {
        if (!isConnected && targetSocket != null)
        {
            StartCoroutine(FlyToSocketCoroutine());
        }
    }

    private IEnumerator FlyToSocketCoroutine()
    {
        isConnected = true;

        // 1. Fişi bir daha tutamasınlar diye interactable'ı kapat
        // (Bu işlem aynı zamanda eğer oyuncu o an fişi tutuyorsa otomatik olarak elinden düşmesini sağlar - XRIT 3.0 ile tam uyumludur)
        if (interactable != null) 
        {
            interactable.enabled = false;
        }

        // 2. Fizik motorunu kapat ki uçarken kablo havada çıldırmasın
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // --- YENİ EKLENEN KISIM (PATLAMAYI ÖNLER) ---
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true; 
        // --------------------------------------------

        // 3. Uçuş Animasyonu Başlıyor
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < flyDuration)
        {
            // Fişi başlangıç noktasından hedefe doğru yumuşakça kaydır (Lerp)
            transform.position = Vector3.Lerp(startPos, targetSocket.position, elapsedTime / flyDuration);
            transform.rotation = Quaternion.Lerp(startRot, targetSocket.rotation, elapsedTime / flyDuration);
            
            elapsedTime += Time.deltaTime;
            yield return null; // Bir sonraki kareyi bekle
        }

        // 4. Tam olarak yerine oturt ve kilitle
        transform.position = targetSocket.position;
        transform.rotation = targetSocket.rotation;
        transform.SetParent(targetSocket); // Yuvaya bağla
     onConnected.Invoke();
        Debug.Log("Kablo otomatik olarak yuvaya girdi!");
    }
}