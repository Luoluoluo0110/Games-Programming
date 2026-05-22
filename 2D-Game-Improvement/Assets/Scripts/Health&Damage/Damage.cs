using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles dealing damage to Health components.
///
/// Architecture role:
/// Damage is the START of the deterministic damage chain. It listens to Unity's
/// physics loop and forwards the result into the chain:
///     OnTriggerEnter2D() -> DealDamage() -> Health.TakeDamage() -> CheckDeath() -> Die()
///
/// OBJECT IDENTITY (critical):
///   * this.gameObject      -> the object running THIS script (e.g. the projectile / hazard).
///   * collision.gameObject -> the EXTERNAL object that was hit (e.g. the enemy / player).
/// Damage is applied to collision.gameObject. Self-destruction (destroyAfterDamage)
/// targets this.gameObject and only ever runs AFTER damage has been dealt.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Damage : MonoBehaviour
{
    [Header("Team Settings")]
    [Tooltip("The team associated with this damage. Damage only applies to a DIFFERENT team.")]
    public int teamId = 0;

    [Header("Damage Settings")]
    [Tooltip("How much damage to deal")]
    [Range(0, 100)]
    public int damageAmount = 1;
    [Tooltip("Prefab to spawn after dealing damage")]
    public GameObject hitEffect = null;
    [Tooltip("Whether or not to destroy the attached game object after dealing damage")]
    public bool destroyAfterDamage = true;
    [Tooltip("Whether or not to apply damage when triggers collide")]
    public bool dealDamageOnTriggerEnter = false;
    [Tooltip("Whether or not to apply damage while triggers stay overlapped (damage over time)")]
    public bool dealDamageOnTriggerStay = false;
    [Tooltip("Whether or not to apply damage on non-trigger collider collisions")]
    public bool dealDamageOnCollision = false;

    /// <summary>
    /// Description:
    /// CHAIN STEP 1 (entry point) - Unity physics callback fired when a Collider2D
    /// enters an attached 2D trigger collider.
    /// </summary>
    /// <param name="collision">The Collider2D that triggered the callback</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dealDamageOnTriggerEnter)
        {
            // collision.gameObject is the EXTERNAL object that was hit.
            DealDamage(collision.gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// CHAIN STEP 1 (entry point) - Unity physics callback fired every frame a
    /// Collider2D stays inside an attached 2D trigger collider.
    /// </summary>
    /// <param name="collision">The Collider2D that triggered the callback</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (dealDamageOnTriggerStay)
        {
            DealDamage(collision.gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// CHAIN STEP 1 (entry point) - Unity physics callback fired when a non-trigger
    /// Collider2D collides with another non-trigger Collider2D.
    /// </summary>
    /// <param name="collision">The Collision2D that triggered the callback</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dealDamageOnCollision)
        {
            DealDamage(collision.gameObject);
        }
    }

    /// <summary>
    /// Description:
    /// CHAIN STEP 2 - Helper function (NOT a Unity callback). Deals damage to the
    /// hit object if it has a Health component on a different team.
    /// </summary>
    /// <param name="collisionGameObject">The external object that was collided with</param>
    private void DealDamage(GameObject collisionGameObject)
    {
        Health collidedHealth = collisionGameObject.GetComponent<Health>();

        // Guard clauses keep the chain deterministic: bail out cleanly if there is
        // nothing to damage, or the target is a friendly (same team).
        if (collidedHealth == null)
        {
            return;
        }
        if (collidedHealth.teamId == this.teamId)
        {
            return;
        }

        Debug.Log($"[Damage] '{gameObject.name}' dealing {damageAmount} damage to " +
                  $"'{collisionGameObject.name}'.");

        // CHAIN STEP 3 - hand off into the victim's Health component.
        collidedHealth.TakeDamage(damageAmount);

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation, null);
        }

        // SAFETY: only NOW, after damage is dealt, do we optionally remove ourselves.
        // Destroy targets this.gameObject (the projectile), never collision.gameObject.
        if (destroyAfterDamage)
        {
            Enemy enemyComponent = gameObject.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.DoBeforeDestroy();
            }
            Destroy(this.gameObject);
        }
    }
}
