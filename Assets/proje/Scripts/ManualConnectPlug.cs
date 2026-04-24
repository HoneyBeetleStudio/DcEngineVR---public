using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

/// <summary>
/// Kullanıcının VR'da manuel olarak iki ucu birleştirmesini sağlar.
/// Belirli mesafeye gelince "Snap" yapar ve ekrana bilgi yazısı basar.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
public class ManualConnectPlug : MonoBehaviour
{
    [Header("Bağlantı Ayarları")]
    public Transform targetSocket;
    public float connectDistance = 0.1f;
    public string connectionMessage = "Bağlantı Başarılı!";
    
    [Header("Görsel Geri Bildirim")]
    [Tooltip("Bağlanabilir olduğunu göstermek için bu obje tutulduğunda hedefte aktif edilecek görsel (Highlight/Outline).")]
    public GameObject targetHighlight;

    [Header("Highlight Renk Ayarları")]
    public Color highlightColor = new Color(0.8f, 1f, 0f, 0.5f); // Sarımsı/Yeşilimsi neon
    public bool emissionAcik = true;
    [Range(0f, 5f)] public float emissionSiddeti = 2f;

    [Header("UI Ayarları")]
    public TextMeshProUGUI uiTextOutput;
    public float messageDuration = 3f;

    private XRGrabInteractable _grab;
    private Rigidbody _rb;
    private bool _isConnected = false;
    private Coroutine _messageCoroutine;
    
    private Renderer[] _highlightRenderers;
    private Color[] _originalColors;
    private Color[] _originalEmissionColors;
    private bool[] _originalEmissionEnabled;

    private void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
        
        if (targetHighlight != null)
        {
            _highlightRenderers = targetHighlight.GetComponentsInChildren<Renderer>();
            StoreOriginalColors();
            targetHighlight.SetActive(false);
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

    private void ApplyHighlightColors(bool active)
    {
        if (_highlightRenderers == null) return;

        for (int i = 0; i < _highlightRenderers.Length; i++)
        {
            Material mat = _highlightRenderers[i].material;
            if (active)
            {
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", highlightColor);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", highlightColor);

                if (emissionAcik && mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", highlightColor * emissionSiddeti);
                }
            }
            else
            {
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

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (_isConnected) return;

        if (targetHighlight != null)
        {
            ApplyHighlightColors(true);
            targetHighlight.SetActive(true);
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (targetHighlight != null)
        {
            ApplyHighlightColors(false);
            targetHighlight.SetActive(false);
        }

        CheckForConnection();
    }

    private void Update()
    {
        if (_isConnected) return;

        // Eldeyken de sürekli mesafe kontrolü yapabiliriz (opsiyonel snap hissi için)
        if (_grab.isSelected)
        {
            float dist = Vector3.Distance(transform.position, targetSocket.position);
            // Çok yakınsa kullanıcı elindeyken de snap yapabiliriz veya sadece görsel ipucu verebiliriz
        }
    }

    private void CheckForConnection()
    {
        if (_isConnected || targetSocket == null) return;

        float dist = Vector3.Distance(transform.position, targetSocket.position);
        if (dist <= connectDistance)
        {
            ConnectToSocket();
        }
    }

    private void ConnectToSocket()
    {
        _isConnected = true;

        // Fiziği ve etkileşimi kapat
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        _grab.enabled = false;

        // Tam konuma yerleştir
        transform.SetParent(targetSocket);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Ekrana yazı yaz
        ShowMessage(connectionMessage);

        // Highlight kapat
        if (targetHighlight != null)
            targetHighlight.SetActive(false);
    }

    private void ShowMessage(string msg)
    {
        if (uiTextOutput == null) return;

        if (_messageCoroutine != null)
            StopCoroutine(_messageCoroutine);
        
        _messageCoroutine = StartCoroutine(DisplayMessageCoroutine(msg));
    }

    private IEnumerator DisplayMessageCoroutine(string msg)
    {
        uiTextOutput.text = msg;
        uiTextOutput.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        uiTextOutput.text = "";
        // uiTextOutput.gameObject.SetActive(false); // İstersen tamamen kapatabilirsin
    }
}
