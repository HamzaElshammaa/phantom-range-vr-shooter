using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed    = 30f;
    public float lifetime = 3f;

    void Start() { Destroy(gameObject, lifetime); }

    void Update() { transform.Translate(Vector3.forward * speed * Time.deltaTime); }

    void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.Die();
            Destroy(gameObject);
        }
    }
}
