using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 3f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Player")) return;

        EnemyCube target = col.collider.GetComponentInParent<EnemyCube>();
        if (target != null) target.TakeDamage(damage);

        Destroy(gameObject);
    }
}
