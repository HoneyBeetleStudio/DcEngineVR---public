using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class HVEldiven : MonoBehaviour
{
    public static bool Giyildi { get; private set; }

    public HVEldiven esEldiven;
    public Color eldivenRengi = new Color(0.85f, 0.05f, 0.05f);
    public UnityEvent giyildi;

    XRSimpleInteractable _interactable;
    static readonly HashSet<Renderer> _boyananlar = new HashSet<Renderer>();

    void Awake()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
    }

    void OnEnable()
    {
        _interactable.selectEntered.AddListener(Tutuldu);
    }

    void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(Tutuldu);
    }

    void Tutuldu(SelectEnterEventArgs _)
    {
        Giy();
    }

    public void Giy()
    {
        if (Giyildi)
            return;
        Giyildi = true;
        giyildi?.Invoke();

        StartCoroutine(ElleriBoyaRutini());

        if (esEldiven != null)
            esEldiven.gameObject.SetActive(false);
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    IEnumerator ElleriBoyaRutini()
    {
        for (int i = 0; i < 15; i++)
        {
            ElleriBoya();
            yield return new WaitForSeconds(1f);
        }
    }

    void ElleriBoya()
    {
        foreach (var origin in FindObjectsByType<XROrigin>(FindObjectsSortMode.None))
        {
            foreach (var r in origin.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                if (r is LineRenderer || _boyananlar.Contains(r))
                    continue;
                bool elIleIlgili = r is SkinnedMeshRenderer;
                if (!elIleIlgili)
                {
                    for (Transform t = r.transform; t != null && t != origin.transform; t = t.parent)
                    {
                        string ad = t.name.ToLowerInvariant();
                        if (ad.Contains("hand") || ad.Contains("controller") || ad.Contains("glove"))
                        {
                            elIleIlgili = true;
                            break;
                        }
                    }
                }
                if (!elIleIlgili)
                    continue;

                var mpb = new MaterialPropertyBlock();
                r.GetPropertyBlock(mpb);
                mpb.SetColor("_BaseColor", eldivenRengi);
                mpb.SetColor("_Color", eldivenRengi);
                r.SetPropertyBlock(mpb);
                _boyananlar.Add(r);
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Sifirla()
    {
        Giyildi = false;
        _boyananlar.Clear();
    }
}
