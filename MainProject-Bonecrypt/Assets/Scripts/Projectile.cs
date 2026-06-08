using UnityEngine;

// A single bullet. Flies straight with no gravity and almost no mass (so it won't shove
// enemies around), and self-destructs after `lifetime` if it hits nothing. On impact it
// damages any EnemyCube it touches and is destroyed; it never damages the player who fired it.
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 3f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;          // flies straight, no drop
        rb.mass = 0.01f;                // light enough not to shove enemies around
        Destroy(gameObject, lifetime);  // clean up shots that never hit anything
    }

    public void Launch(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Player")) return;   // never hurt the shooter

        // search parents too, in case the visible mesh collider sits on a child of the enemy
        EnemyCube target = col.collider.GetComponentInParent<EnemyCube>();
        if (target != null) target.TakeDamage(damage);

        Destroy(gameObject);   // one-shot: gone on first impact with anything but the player
    }
}
