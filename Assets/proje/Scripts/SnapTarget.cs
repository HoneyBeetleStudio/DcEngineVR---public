using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SnapTarget : MonoBehaviour
{

    [Header("Avometre Ayarı")]
public int probNumarasi;

[Header("Snap Ayarları")]
public Vector3 snapLocalRotation = Vector3.zero; 

    public Transform snappableObject;
    public bool isConnected;
    public Vector3 snapLocalOffset = Vector3.zero;
    public float snapRange = 0.3f;
    [Header("UI Mesaj Ayarı")]
    public TMPro.TextMeshProUGUI mesajText;

    [Header("Magnet")]
    public bool magnetEffect = true;
    public float magnetDuration = 0.15f;

    [Header("Highlight")]
    public GameObject highlightObject;
    public bool emissionHighlight = true;
    public Color highlightColor = Color.green;
    [Range(0, 5)] public float emissionSiddeti = 2f;

    private Renderer[] _highlightRenderers;
    private Color[] _originalColors;
    private Color[] _originalEmissionColors;
    private bool[] _originalEmissionEnabled;

    private Transform _snappedObject;
    private XRGrabInteractable _snappedInteractable;
    private bool _isMagnetizing = false;

    private static KabloGrabHighlight[] _cachedKablolar;
    private static float _cacheZamani;

    private void Awake()
    {
        if (emissionHighlight)
        {
            _highlightRenderers = GetComponentsInChildren<Renderer>();
            StoreOriginalColors();
        }

        if (highlightObject != null)
        {
            if (highlightObject == gameObject)
                highlightObject = null;
            else
                highlightObject.SetActive(false);
        }
    }

    private void StoreOriginalColors()
    {
        if (_highlightRenderers == null) return;

        _originalColors = new Color[_highlightRenderers.Length];
        _originalEmissionColors = new Color[_highlightRenderers.Length];
        _originalEmissionEnabled = new bool[_highlightRenderers.Length];

        for (int i = 0; i < _highlightRenderers.Length; i++)
        {
            if (_highlightRenderers[i].material.HasProperty("_BaseColor"))
                _originalColors[i] = _highlightRenderers[i].material.GetColor("_BaseColor");
            else if (_highlightRenderers[i].material.HasProperty("_Color"))
                _originalColors[i] = _highlightRenderers[i].material.GetColor("_Color");

            if (_highlightRenderers[i].material.HasProperty("_EmissionColor"))
            {
                _originalEmissionColors[i] = _highlightRenderers[i].material.GetColor("_EmissionColor");
                _originalEmissionEnabled[i] = _highlightRenderers[i].material.IsKeywordEnabled("_EMISSION");
            }
        }
    }

   public void HighlightAc()
{
    if (isConnected) return;

  
    if (highlightObject != null && highlightObject != gameObject)
    {
        highlightObject.SetActive(true);
        
     
        if (_highlightRenderers == null || _highlightRenderers.Length == 0)
        {
            _highlightRenderers = highlightObject.GetComponentsInChildren<Renderer>(true);
        }
    }


    if (emissionHighlight && _highlightRenderers != null)
    {
        foreach (var rend in _highlightRenderers)
        {
            foreach (var mat in rend.materials) 
            {
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", highlightColor);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", highlightColor);

                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", highlightColor * emissionSiddeti);
                }
            }
        }
    }



    AvometreSistemi avo = FindObjectOfType<AvometreSistemi>();
    if (avo != null && avo.kalibrasyonTamamlandi && GetComponent<PinKimligi>().grupAdi == "KalibrasyonAvo") 
        return;

    if (!isConnected && highlightObject != null) 
        highlightObject.SetActive(true);
}

    public void HighlightKapat()
    {
        if (highlightObject != null)
            highlightObject.SetActive(false);

        if (emissionHighlight && _highlightRenderers != null)
        {
            for (int i = 0; i < _highlightRenderers.Length; i++)
            {
                Material mat = _highlightRenderers[i].material;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", _originalColors[i]);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", _originalColors[i]);

                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", _originalEmissionColors[i]);
                    if (!_originalEmissionEnabled[i]) mat.DisableKeyword("_EMISSION");
                }
            }
        }
    }

   private void Update()
{   
    if (_snappedObject != null)
    {        
        if (_snappedInteractable != null && _snappedInteractable.isSelected)
        {
            Ayir(_snappedObject);
            return;
        }
       Vector3 beklenenPozisyon = transform.TransformPoint(snapLocalOffset);
        
        Quaternion beklenenRotasyon = transform.rotation * Quaternion.Euler(snapLocalRotation);
       if (!_isMagnetizing)
        {
        
            if (Vector3.Distance(_snappedObject.position, beklenenPozisyon) > 0.001f)
                _snappedObject.position = beklenenPozisyon;

         
            if (Quaternion.Angle(_snappedObject.rotation, beklenenRotasyon) > 0.1f)
                _snappedObject.rotation = beklenenRotasyon;
            
           
            float distFromSnap = Vector3.Distance(_snappedObject.position, beklenenPozisyon);
            if (distFromSnap > 0.15f)
                Ayir(_snappedObject);
        }
        return;
    }

    if (snappableObject != null)
    {
        TrySnapObject(snappableObject);
    }
    else
    {      
        if (Time.time - _cacheZamani > 0.5f)
        {
            _cachedKablolar = FindObjectsByType<KabloGrabHighlight>(FindObjectsSortMode.None);
            _cacheZamani = Time.time;
        }

        if (_cachedKablolar != null)
        {
            foreach (var kablo in _cachedKablolar)
            {
                if (kablo == null) continue;
                if (TrySnapObject(kablo.transform)) break; 
            }
        }
    }
}


    private static List<SnapTarget> _tumSnapTargetlar = new List<SnapTarget>();

    private void OnEnable()
    {
        if (!_tumSnapTargetlar.Contains(this))
            _tumSnapTargetlar.Add(this);
    }

    private void OnDisable()
    {
        if (_tumSnapTargetlar.Contains(this))
            _tumSnapTargetlar.Remove(this);
    }

    private bool BaskaBirTargetaBagliMi(Transform obj)
    {
        foreach (var target in _tumSnapTargetlar)
        {
            if (target != null && target != this && target._snappedObject == obj)
                return true;
        }
        return false;
    }

    private bool TrySnapObject(Transform target)
    {
        var grab = target.GetComponent<XRGrabInteractable>();
        Rigidbody rb = target.GetComponent<Rigidbody>();

        if (grab == null || rb == null) return false;
        
        if (BaskaBirTargetaBagliMi(target)) 
        {       
            return false; 
        }

        
        if (!grab.isSelected)
        {
            float dist = Vector3.Distance(target.position, transform.position);
        if (dist <= snapRange)
        {
            SnapYap(target);
            return true;
        }
    }
    return false;
}

    private void SnapYap(Transform obj)
    {
        if (_snappedObject != null) return;
        if (obj.parent != null && obj.parent.GetComponent<SnapTarget>() != null) return;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        _snappedObject = obj;
        _snappedInteractable = obj.GetComponent<XRGrabInteractable>();
        isConnected = true;

        
    if (obj.name.ToLower().Contains("fis1") && mesajText != null)
    {      
        mesajText.gameObject.SetActive(true);      
        if (gameObject.name.ToLower().Contains("fisyeri1"))
        {
            mesajText.text = "Gerilim Var";           
        }
    }

        if (_snappedInteractable != null)
        {
            _snappedInteractable.selectEntered.AddListener(TutuluncaAyir);

            if (_snappedInteractable.isSelected)
            {
                var interactor = _snappedInteractable.firstInteractorSelecting;
                if (interactor != null)
                {
                    var mgr = _snappedInteractable.interactionManager;
                    if (mgr != null)
                        mgr.SelectCancel((IXRSelectInteractor)interactor, (IXRSelectInteractable)_snappedInteractable);
                }
            }

            _snappedInteractable.trackPosition = false;
            _snappedInteractable.trackRotation = false;
        }
        HighlightKapat();

        if (magnetEffect)
            StartCoroutine(MagnetRoutine(obj));
        else
        {
            obj.position = transform.TransformPoint(snapLocalOffset);
            obj.rotation = transform.rotation;
        }

       
AvometreSistemi avo = FindFirstObjectByType<AvometreSistemi>();
    PinKimligi pin = GetComponent<PinKimligi>();

    if (avo != null && pin != null)
    {        
        avo.BaglantiGuncelle(probNumarasi, pin);
    }

    
if (HVManager.Instance != null)
{
    HVManager.Instance.BaglantiDurumunuGuncelle();
}

      
    }

    private System.Collections.IEnumerator MagnetRoutine(Transform obj)
    {
        _isMagnetizing = true;

        Vector3 startPos = obj.position;
        Quaternion startRot = obj.rotation;
        float t = 0;

        while (t < magnetDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.SmoothStep(0, 1, t / magnetDuration);

            obj.position = Vector3.Lerp(startPos, transform.TransformPoint(snapLocalOffset), n);
            obj.rotation = Quaternion.Slerp(startRot, transform.rotation, n);

            yield return null;
        }

        obj.position = transform.TransformPoint(snapLocalOffset);
        obj.rotation = transform.rotation;

        _isMagnetizing = false;
    }

  public void Ayir(Transform snappedObj)
{
    if (_snappedObject == null || _snappedObject != snappedObj) return;
    
    AvometreSistemi avo = FindFirstObjectByType<AvometreSistemi>();
    if (avo != null) avo.BaglantiKopart(probNumarasi);
    
    if (mesajText != null)
    {
        mesajText.gameObject.SetActive(false);
    }

    var rb = snappedObj.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = false; 
        rb.useGravity = true;  
    }
   
    if (_snappedInteractable != null)
    {
        _snappedInteractable.selectEntered.RemoveListener(TutuluncaAyir);
        _snappedInteractable.trackPosition = true;
        _snappedInteractable.trackRotation = true;
    }

    _snappedObject = null;
    _snappedInteractable = null;
    isConnected = false;
    Debug.Log(gameObject.name + " pini tamamen serbest bıraktı.");



if (HVManager.Instance != null)
{
    HVManager.Instance.BaglantiDurumunuGuncelle();
}


}

    private void TutuluncaAyir(SelectEnterEventArgs args)
    {
        if (_snappedObject == null) return;
        Ayir(_snappedObject);
    }

    private void OnDestroy()
    {
        if (_snappedInteractable != null)
            _snappedInteractable.selectEntered.RemoveListener(TutuluncaAyir);
    }


    public PinKimligi GetSnappedPinKimligi()
    {
        return _snappedObject != null ? _snappedObject.GetComponent<PinKimligi>() : null;
    }
}
