using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 45f;
    public float lifetime = 6f;
    [Tooltip("Uniform scale for the mesh so the round reads clearly in VR.")]
    public float visualScale = 22f;
    [Tooltip("World-space hit radius (generous arcade coverage).")]
    public float hitRadiusWorld = 0.45f;
    [Tooltip("Bright tint for the sphere so it stands out.")]
    public Color projectileColor = new Color(1f, 0.55f, 0.1f);

    Rigidbody _rb;

    void Awake()
    {
        float vis = Mathf.Max(0.01f, visualScale);
        transform.localScale = Vector3.one * vis;

        float worldR = Mathf.Max(0.02f, hitRadiusWorld);
        float localR = worldR / vis;

        var spheres = GetComponents<SphereCollider>();
        if (spheres.Length == 0)
        {
            var c = gameObject.AddComponent<SphereCollider>();
            c.isTrigger = true;
            c.radius = localR;
        }
        else
        {
            for (int i = 0; i < spheres.Length; i++)
            {
                if (i == 0)
                {
                    spheres[i].enabled = true;
                    spheres[i].isTrigger = true;
                    spheres[i].radius = localR;
                }
                else
                    spheres[i].enabled = false;
            }
        }

        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        ApplyVisualTint();
    }

    void ApplyVisualTint()
    {
        var rends = GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            if (r is ParticleSystemRenderer) continue;
            foreach (var m in r.materials)
            {
                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", projectileColor);
                if (m.HasProperty("_Color"))
                    m.SetColor("_Color", projectileColor);
                if (m.HasProperty("_EmissionColor"))
                {
                    m.SetColor("_EmissionColor", projectileColor * 3f);
                    m.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    void Start() { Destroy(gameObject, lifetime); }

    void FixedUpdate()
    {
        Vector3 step = transform.forward * (speed * Time.fixedDeltaTime);
        if (_rb != null)
            _rb.MovePosition(_rb.position + step);
        else
            transform.position += step;
    }

    void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            enemy.Die();
            Destroy(gameObject);
        }
    }
}
