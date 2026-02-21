using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; 
using System.Collections.Generic;

public class RenkDegistirici : MonoBehaviour
{
    
    public Material parlamaMateryali;

    private XRGrabInteractable grabInteractable;

    
    private Dictionary<Renderer, Material[]> orjinalMateryalKayitlari = new Dictionary<Renderer, Material[]>();

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        
        Renderer[] tumParcalar = GetComponentsInChildren<Renderer>();

        
        foreach (Renderer parca in tumParcalar)
        {
            orjinalMateryalKayitlari.Add(parca, parca.sharedMaterials);
        }

        
        grabInteractable.hoverEntered.AddListener(MateryaliDegistir);
        grabInteractable.hoverExited.AddListener(EskiyeDon);
    }


    private void MateryaliDegistir(HoverEnterEventArgs args)
    {
        if (parlamaMateryali == null) return;

        foreach (var kayit in orjinalMateryalKayitlari)
        {
            Renderer parca = kayit.Key;            
         
            int slotSayisi = kayit.Value.Length;
            Material[] yeniDizi = new Material[slotSayisi];

            for (int i = 0; i < slotSayisi; i++)
            {
                yeniDizi[i] = parlamaMateryali;
            }
            parca.materials = yeniDizi;
        }
    }

    private void EskiyeDon(HoverExitEventArgs args)
    {
        foreach (var kayit in orjinalMateryalKayitlari)
        {
            Renderer parca = kayit.Key;    
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