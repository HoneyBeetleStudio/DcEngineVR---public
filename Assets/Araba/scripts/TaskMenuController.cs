using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

[RequireComponent(typeof(Canvas))]
public class TaskMenuController : MonoBehaviour
{
    [Header("Visibility")]
    public bool startVisible = false;

    [Header("Toggle input")]
    public string[] toggleBindings =
    {
        "<XRController>{LeftHand}/menuButton",
        "<XRController>{LeftHand}/primaryButton",
        "<XRController>{LeftHand}/secondaryButton",
        "<Keyboard>/m"
    };

    Canvas _canvas;
    GraphicRaycaster _raycaster;
    TrackedDeviceGraphicRaycaster _xrRaycaster;
    InputAction _toggleAction;

    public bool IsVisible => _canvas != null && _canvas.enabled;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _raycaster = GetComponent<GraphicRaycaster>();
        _xrRaycaster = GetComponent<TrackedDeviceGraphicRaycaster>();

        _toggleAction = new InputAction("ToggleTaskMenu", InputActionType.Button);
        foreach (var binding in toggleBindings)
        {
            if (!string.IsNullOrWhiteSpace(binding))
                _toggleAction.AddBinding(binding);
        }
        _toggleAction.performed += OnTogglePerformed;

        SetVisible(startVisible);
    }

    void OnEnable()
    {
        _toggleAction?.Enable();
    }

    void OnDisable()
    {
        _toggleAction?.Disable();
    }

    void OnDestroy()
    {
        if (_toggleAction == null)
            return;
        _toggleAction.performed -= OnTogglePerformed;
        _toggleAction.Dispose();
    }

    void OnTogglePerformed(InputAction.CallbackContext context)
    {
        SetVisible(!IsVisible);
    }

    public void SetVisible(bool visible)
    {
        if (_canvas != null)
            _canvas.enabled = visible;
        if (_raycaster != null)
            _raycaster.enabled = visible;
        if (_xrRaycaster != null)
            _xrRaycaster.enabled = visible;
    }
}
