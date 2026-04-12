using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Grab the gun, then press J to fire (keyboard / XR Device Simulator).
public class GunShooter : MonoBehaviour
{
    public Transform muzzlePoint;
    [Tooltip("Projectile spawned at muzzle. Assign Bullet prefab for visible shots.")]
    public GameObject bulletPrefab;
    [Tooltip("If no prefab, falls back to spherecast hit test.")]
    public float shootRange = 200f;
    [Tooltip("Thickens the fallback ray (world units).")]
    public float shotCoverageRadius = 0.18f;
    public LayerMask shootMask = Physics.DefaultRaycastLayers;
    public ParticleSystem muzzleFlash;
    public AudioSource shootSound;
    [Tooltip("Minimum seconds between shots (stops XR + keyboard double-firing).")]
    public float minShotInterval = 0.12f;

    XRGrabInteractable _grab;
    bool _held;
    float _lastShotTime = -999f;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        if (!muzzlePoint)
        {
            Transform mp = transform.Find("MuzzlePoint");
            if (mp) muzzlePoint = mp;
        }
    }

    void OnEnable()
    {
        if (!_grab) return;
        _grab.selectEntered.AddListener(OnGrabEnter);
        _grab.selectExited.AddListener(OnGrabExit);
    }

    void OnDisable()
    {
        if (!_grab) return;
        _grab.selectEntered.RemoveListener(OnGrabEnter);
        _grab.selectExited.RemoveListener(OnGrabExit);
    }

    void OnGrabEnter(SelectEnterEventArgs a)
    {
        _held = true;
        CombatZoneDebugLog.Log("Gun grabbed", a.interactorObject.transform.name);
    }

    void OnGrabExit(SelectExitEventArgs a)
    {
        _held = false;
        CombatZoneDebugLog.Log("Gun released", a.interactorObject.transform.name);
    }

    void Update()
    {
        if (!_held) return;
        if (Keyboard.current != null && Keyboard.current[Key.J].wasPressedThisFrame)
            TryFire("keyboard_J");
    }

    void TryFire(string source)
    {
        if (Time.unscaledTime - _lastShotTime < minShotInterval) return;
        _lastShotTime = Time.unscaledTime;

        if (bulletPrefab != null && muzzlePoint != null)
        {
            CombatZoneAudioManager.Instance?.PlayGunshot();
            if (muzzleFlash) muzzleFlash.Play();
            if (shootSound) shootSound.Play();
            // Use UnityEngine.Object overload so the engine does not pick Instantiate<GameObject>,
            // which can throw InvalidCastException when the asset reference is not a root GameObject.
            Quaternion shotRot = GetShotRotation();
            UnityEngine.Object spawned = UnityEngine.Object.Instantiate(
                (UnityEngine.Object)bulletPrefab,
                muzzlePoint.position,
                shotRot);
            GameObject spawnedGo = spawned as GameObject;
            if (spawnedGo == null && spawned is Component c)
                spawnedGo = c.gameObject;
            if (spawnedGo == null)
            {
                CombatZoneDebugLog.Log("Shot fired", $"{source} — bulletPrefab must be a GameObject prefab root");
                if (spawned != null)
                    Destroy(spawned);
                RaycastShot(source);
                return;
            }
            CombatZoneDebugLog.Log("Shot fired", $"{source} — projectile");
            return;
        }

        RaycastShot(source);
    }

    void RaycastShot(string source)
    {
        CombatZoneAudioManager.Instance?.PlayGunshot();
        if (muzzleFlash) muzzleFlash.Play();
        if (shootSound) shootSound.Play();
        Vector3 orig = muzzlePoint ? muzzlePoint.position : transform.position;
        Vector3 fwd = GetShotDirection();
        float castDist = Mathf.Max(0.01f, shootRange);
        float r = Mathf.Max(0f, shotCoverageRadius);
        Vector3 castStart = orig + fwd * Mathf.Min(0.08f, castDist * 0.5f);
        float castLen = Mathf.Max(0.01f, castDist - 0.08f);
        Debug.DrawRay(castStart, fwd * castLen, Color.red, 0.35f);

        RaycastHit hit;
        bool hitSomething = r > 0.0001f
            ? Physics.SphereCast(castStart, r, fwd, out hit, castLen, shootMask, QueryTriggerInteraction.Collide)
            : Physics.Raycast(castStart, fwd, out hit, castLen, shootMask, QueryTriggerInteraction.Collide);

        if (!hitSomething)
        {
            CombatZoneDebugLog.Log("Shot fired", $"{source} — miss (no hit, range={shootRange})");
            return;
        }

        var ec = hit.collider.GetComponentInParent<EnemyController>();
        if (ec != null)
        {
            CombatZoneDebugLog.Log("Shot fired", $"{source} — hit enemy {hit.collider.name}");
            ec.Die();
        }
        else
            CombatZoneDebugLog.Log("Shot fired", $"{source} — hit {hit.collider.name} (not enemy)");
    }

    /// <summary>
    /// Many weapon models parent MuzzlePoint with local +Z pointing back into the grip.
    /// Use gun-root → muzzle line (bore) and keep it in the same hemisphere as the weapon forward.
    /// Final direction is flipped 180° for this M1911 layout (barrel vs. transform axes).
    /// </summary>
    Vector3 GetShotDirection()
    {
        Vector3 dir;
        if (!muzzlePoint)
            dir = transform.forward;
        else
        {
            Vector3 gripToMuzzle = muzzlePoint.position - transform.position;
            if (gripToMuzzle.sqrMagnitude > 1e-4f)
            {
                dir = gripToMuzzle.normalized;
                if (Vector3.Dot(dir, transform.forward) < 0f)
                    dir = -dir;
            }
            else
            {
                dir = muzzlePoint.forward;
                if (Vector3.Dot(dir, transform.forward) < 0f)
                    dir = -dir;
            }
        }

        return -dir;
    }

    Quaternion GetShotRotation()
    {
        Vector3 dir = GetShotDirection();
        Vector3 up = muzzlePoint ? muzzlePoint.up : transform.up;
        if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.99f)
            up = Vector3.Cross(dir, transform.right).sqrMagnitude > 1e-4f
                ? Vector3.Cross(dir, transform.right).normalized
                : transform.up;
        return Quaternion.LookRotation(dir, up);
    }
}
