using System;
using UnityEngine;

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

    public int CurrentHP { get; private set; }
    public event Action OnDeath;

    private Transform player;
    private Rigidbody rb;
    private float nextAttackTime;
    private bool dead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        CurrentHP = maxHP;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (dead || player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(Quaternion.LookRotation(dir, Vector3.up));
    }

    void OnCollisionStay(Collision col)
    {
        if (dead || Time.time < nextAttackTime) return;
        if (!col.collider.CompareTag("Player")) return;

        PlayerHealth ph = col.collider.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(contactDamage);
            nextAttackTime = Time.time + attackCooldown;
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
            Destroy(gameObject);
        }
    }
}
