using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GunShooter : MonoBehaviour
{
    [Header("Muzzle")] public Transform muzzlePoint;
    [Header("Ray")] public float shootRange = 200f;
    public LayerMask shootMask = Physics.DefaultRaycastLayers;
    [Header("FX")] public ParticleSystem muzzleFlash;
    public AudioSource shootSound;

    XRGrabInteractable _xr;
    bool _held;

    void Awake()
    {
        _xr = GetComponent<XRGrabInteractable>();
        if (!muzzlePoint) { var t = transform.Find("MuzzlePoint"); if (t) muzzlePoint = t; }
    }

    void OnEnable()
    {
        if (!_xr) return;
        _xr.activated.AddListener(OnActivate);
        _xr.selectEntered.AddListener(OnPickup);
        _xr.selectExited.AddListener(OnPutdown);
    }

    void OnDisable()
    {
        if (!_xr) return;
        _xr.activated.RemoveListener(OnActivate);
        _xr.selectEntered.RemoveListener(OnPickup);
        _xr.selectExited.RemoveListener(OnPutdown);
    }

    void OnPickup(SelectEnterEventArgs a)  { _held = true; }
    void OnPutdown(SelectExitEventArgs a)  { _held = false; }
    void OnActivate(ActivateEventArgs a)   { PullTrigger(); }

    void Update()
    {
        if (!_held || Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame) PullTrigger();
    }

    void PullTrigger()
    {
        if (muzzleFlash) muzzleFlash.Play();
        if (shootSound)  shootSound.Play();
        var o = muzzlePoint ? muzzlePoint.position : transform.position;
        var d = muzzlePoint ? muzzlePoint.forward  : transform.forward;
        Debug.DrawRay(o, d * shootRange, Color.red, 0.5f);
        if (Physics.Raycast(o, d, out RaycastHit h, shootRange, shootMask))
        {
            var e = h.collider.GetComponentInParent<EnemyController>();
            if (e) e.Die();
        }
    }
}