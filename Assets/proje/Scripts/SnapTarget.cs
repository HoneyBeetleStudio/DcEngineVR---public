using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
public class SnapTarget : MonoBehaviour
{
    public Transform snappableObject;
    public bool isConnected;
    public Vector3 snapLocalOffset = Vector3.zero;
    public float snapRange = 0.5f;
    public bool debugLog;

    private HashSet<XRGrabInteractable> _triggerIcindekiler = new HashSet<XRGrabInteractable>();
    private HashSet<XRGrabInteractable> _dinlenenler = new HashSet<XRGrabInteractable>();
    private Transform _snappedObject;
    private XRGrabInteractable _snappedInteractable;

    private static bool SnappableEslesiyor(Transform snappable, Transform other)
    {
        if (snappable == null) return true;
        return other == snappable || other.IsChildOf(snappable);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!SnappableEslesiyor(snappableObject, other.transform)) return;
        var grab = other.GetComponentInParent<XRGrabInteractable>();
        if (grab == null) return;
        _triggerIcindekiler.Add(grab);
        if (!_dinlenenler.Contains(grab))
        {
            _dinlenenler.Add(grab);
            grab.selectExited.AddListener(ObjeyiBirakildi);
            if (debugLog) Debug.Log($"[SnapTarget] Trigger'a girdi: {grab.name}", this);
        }
    }

    private void Update()
    {
        if (_snappedObject != null)
        {
            Vector3 beklenenPozisyon = transform.TransformPoint(snapLocalOffset);
            float distFromSnap = Vector3.Distance(_snappedObject.position, beklenenPozisyon);
            if (distFromSnap > 0.15f)
            {
                if (debugLog) Debug.Log($"[SnapTarget] (Update) Obje zorla çekildi, ayrılıyor. Fark: {distFromSnap:F2}", this);
                Ayir(_snappedObject);
            }
            return;
        }

        if (snappableObject != null)
        {
            var grab = snappableObject.GetComponent<XRGrabInteractable>();
            if (grab != null && !grab.isSelected)
            {
                float dist = Vector3.Distance(snappableObject.position, transform.position);
                if (dist <= snapRange)
                {
                    if (debugLog) Debug.Log($"[SnapTarget] (Update) Mesafe uygun ve tutulmuyor, snap yapılıyor. Mesafe={dist:F2}", this);
                    SnapYap(snappableObject);
                    return;
                }
            }
        }

        foreach (var grab in _triggerIcindekiler)
        {
            if (grab != null && !grab.isSelected)
            {
                if (debugLog) Debug.Log($"[SnapTarget] (Update) Trigger içinde ve tutulmuyor, snap yapılıyor: {grab.name}", this);
                SnapYap(grab.transform);
                return;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!SnappableEslesiyor(snappableObject, other.transform)) return;
        var grab = other.GetComponentInParent<XRGrabInteractable>();
        if (grab == null) return;
        _triggerIcindekiler.Remove(grab);
    }

    private void ObjeyiBirakildi(SelectExitEventArgs args)
    {
        if (_snappedObject != null) return;

        Transform releasedTransform = null;
        XRGrabInteractable interactable = args.interactableObject as XRGrabInteractable;
        if (interactable != null)
            releasedTransform = interactable.transform;
        else if (args.interactableObject is Component c)
            releasedTransform = c.transform;

        if (releasedTransform == null)
        {
            if (debugLog) Debug.Log("[SnapTarget] Bırakıldı ama interactable transform bulunamadı.", this);
            return;
        }
        if (!SnappableEslesiyor(snappableObject, releasedTransform))
            return;

        float dist = Vector3.Distance(releasedTransform.position, transform.position);
        bool triggerIciydi = interactable != null && _dinlenenler.Contains(interactable);
        if (!triggerIciydi && dist > snapRange)
        {
            if (debugLog) Debug.Log($"[SnapTarget] Bırakıldı ama mesafe fazla: {dist:F2} > {snapRange}.", this);
            return;
        }

        if (debugLog) Debug.Log($"[SnapTarget] Bırakıldı, mesafe={dist:F2}, snap yapılıyor.", this);
        var grabToRemove = interactable ?? releasedTransform.GetComponent<XRGrabInteractable>();
        if (grabToRemove != null)
        {
            _dinlenenler.Remove(grabToRemove);
            grabToRemove.selectExited.RemoveListener(ObjeyiBirakildi);
        }
        SnapYap(releasedTransform);
    }

    private void SnapYap(Transform obj)
    {
        if (_snappedObject != null) return;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        _snappedObject = obj;
        _snappedInteractable = obj.GetComponent<XRGrabInteractable>();
        if (_snappedInteractable != null)
            _snappedInteractable.selectEntered.AddListener(TutuluncaAyir);

        obj.SetParent(transform);
        obj.localPosition = snapLocalOffset;
        obj.localRotation = Quaternion.identity;
        obj.localScale = Vector3.one;

        isConnected = true;
        if (debugLog) Debug.Log("[SnapTarget] Yapıştı: " + obj.name, this);
    }

    public void Ayir(Transform snappedObj)
    {
        if (_snappedObject != snappedObj) return;

        if (_snappedInteractable != null)
            _snappedInteractable.selectEntered.RemoveListener(TutuluncaAyir);

        var rb = snappedObj.GetComponent<Rigidbody>();
        if (rb != null && (_snappedInteractable == null || !_snappedInteractable.isSelected))
            rb.isKinematic = false;

        if (snappedObj.parent == transform)
            snappedObj.SetParent(null);

        _snappedObject = null;
        _snappedInteractable = null;
        isConnected = false;
        if (debugLog) Debug.Log("[SnapTarget] Ayrıldı: " + snappedObj.name, this);
    }

    private void TutuluncaAyir(SelectEnterEventArgs args)
    {
        if (_snappedObject == null) return;
        Ayir(_snappedObject);
    }

    private void OnDestroy()
    {
        foreach (var g in _dinlenenler)
        {
            if (g != null)
                g.selectExited.RemoveListener(ObjeyiBirakildi);
        }
        if (_snappedInteractable != null)
            _snappedInteractable.selectEntered.RemoveListener(TutuluncaAyir);
    }
}
