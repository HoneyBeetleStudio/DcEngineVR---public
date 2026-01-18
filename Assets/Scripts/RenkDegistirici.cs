using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRI 3.x
using System.Collections.Generic;

public class RenkDegistirici : MonoBehaviour
{
    [Tooltip("Vurgu yapıldığında nesne tamamen bu materyale dönüşecek.")]
    public Material parlamaMateryali;

    private XRGrabInteractable grabInteractable;

    // Her parçanın orjinal materyallerini saklayacağımız hafıza
    private Dictionary<Renderer, Material[]> orjinalMateryalKayitlari = new Dictionary<Renderer, Material[]>();

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        // 1. Tüm çocuk parçaları (Renderer) bul
        Renderer[] tumParcalar = GetComponentsInChildren<Renderer>();

        // 2. Her parçanın orjinal halini kaydet
        foreach (Renderer parca in tumParcalar)
        {
            orjinalMateryalKayitlari.Add(parca, parca.sharedMaterials);
        }

        // Olayları dinle
        grabInteractable.hoverEntered.AddListener(MateryaliDegistir);
        grabInteractable.hoverExited.AddListener(EskiyeDon);
    }

    // El değince çalışır: ESKİYİ SİL, YENİYİ KOY
    private void MateryaliDegistir(HoverEnterEventArgs args)
    {
        if (parlamaMateryali == null) return;

        foreach (var kayit in orjinalMateryalKayitlari)
        {
            Renderer parca = kayit.Key;
            
            // Parçanın kaç tane materyal slotu varsa (örn: 2 malzemeli bir gövde),
            // hepsini bizim parlama materyali ile dolduracağız ki açık yer kalmasın.
            int slotSayisi = kayit.Value.Length;
            Material[] yeniDizi = new Material[slotSayisi];

            for (int i = 0; i < slotSayisi; i++)
            {
                yeniDizi[i] = parlamaMateryali;
            }

            // Yeni diziyi uygula (Eskiler tamamen gider)
            parca.materials = yeniDizi;
        }
    }

    // El çekilince çalışır: ESKİYİ GERİ YÜKLE
    private void EskiyeDon(HoverExitEventArgs args)
    {
        foreach (var kayit in orjinalMateryalKayitlari)
        {
            Renderer parca = kayit.Key;
            // Hafızadaki orjinal diziyi geri koy
            parca.materials = kayit.Value;
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.hoverEntered.RemoveListener(MateryaliDegistir);
            grabInteractable.hoverExited.RemoveListener(EskiyeDon);
        }
    }
}