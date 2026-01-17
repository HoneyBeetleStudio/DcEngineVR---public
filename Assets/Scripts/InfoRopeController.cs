using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InfoRopeController : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject infoCanvas;
    public LineRenderer ropeRenderer;
    public Transform ropeAnchor;
    public Transform canvasAnchor;
    public AudioSource audioSource;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        infoCanvas.SetActive(false); 
        ropeRenderer.enabled = false; 

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void Update()
    {
        if (isGrabbed)
        {
            ropeRenderer.SetPosition(0, ropeAnchor.position);
            ropeRenderer.SetPosition(1, canvasAnchor.position);
            
            infoCanvas.transform.LookAt(Camera.main.transform);
            infoCanvas.transform.Rotate(0, 180, 0);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        infoCanvas.SetActive(true);
        ropeRenderer.enabled = true;
        
        if (audioSource != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        infoCanvas.SetActive(false);
        ropeRenderer.enabled = false;
        
        if (audioSource != null)
            audioSource.Stop();
    }
}