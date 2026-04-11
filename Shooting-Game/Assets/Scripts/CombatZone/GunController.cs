using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GunController : MonoBehaviour
{
    public Transform muzzlePoint;
    public GameObject bulletPrefab;
    public float fireRate = 0.3f;

    private XRGrabInteractable _grab;
    private bool _held;
    private float _nextFire;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _grab.activated.AddListener(OnTriggerPulled);
        _grab.selectEntered.AddListener(_ => _held = true);
        _grab.selectExited.AddListener(_ => _held = false);
    }

    void OnTriggerPulled(ActivateEventArgs args)
    {
        if (!_held || bulletPrefab == null || muzzlePoint == null) return;
        if (Time.time < _nextFire) return;
        _nextFire = Time.time + fireRate;
        Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
    }
}
