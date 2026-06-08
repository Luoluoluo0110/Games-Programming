using System;
using UnityEngine;

// An enemy (used for both regular mobs and the boss). Uses a Rigidbody to chase the player and
// face them, zeroing its velocity each physics step so knockback from hits can't make it drift
// away or push through walls. It deals contact damage to the player on a cooldown while touching
// them, and on death raises OnDeath, plays its death animation and destroys itself.
[RequireComponent(typeof(Rigidbody))]
public class EnemyCube : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 30;

    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Combat")]
    public int contactDamage = 10;
    public float attackCooldown = 1f;

    [Header("Visuals (optional)")]
    public Animator animator;
    public float deathDestroyDelay = 2f;

    public int CurrentHP { get; private set; }
    public event Action OnDeath;

    private Transform player;
    private Rigidbody rb;
    private float nextAttackTime;
    private bool dead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;                              // we drive rotation ourselves
        rb.mass = 100f;
        rb.useGravity = false;
        rb.constraints |= RigidbodyConstraints.FreezePositionY; // pin to the floor plane
        CurrentHP = maxHP;
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (dead || player == null) return;

        // cancel knockback from hits; this body has no drag, so impulses would otherwise
        // pile up and shove the enemy off course (drifting away or pushing through walls)
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.LookRotation(dir, Vector3.up));

        if (animator != null) animator.SetFloat("Speed", moveSpeed);
    }

    // deal contact damage while touching the player, throttled by attackCooldown
    void OnCollisionStay(Collision col)
    {
        if (dead || Time.time < nextAttackTime) return;
        if (!col.collider.CompareTag("Player")) return;

        PlayerHealth ph = col.collider.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(contactDamage);
            nextAttackTime = Time.time + attackCooldown;
            if (animator != null) animator.SetTrigger("Attack");
        }
    }

    public void TakeDamage(int amount)
    {
        if (dead || amount <= 0) return;
        CurrentHP -= amount;
        if (CurrentHP <= 0)
        {
            dead = true;
            OnDeath?.Invoke();
            if (animator != null)
            {
                animator.SetTrigger("Die");
                Destroy(gameObject, deathDestroyDelay);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
