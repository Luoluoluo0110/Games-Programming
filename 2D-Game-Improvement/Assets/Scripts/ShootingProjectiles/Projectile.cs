using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class to make projectiles move and to clean themselves up after a fixed lifespan.
///
/// Architecture role:
/// The projectile only knows how to MOVE itself and how long it should live.
/// It carries a Damage component (separate script) that handles hitting things,
/// so movement and damage stay fully decoupled.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("The distance this projectile will move each second.")]
    [Range(0f, 50f)]
    public float projectileSpeed = 3.0f;

    [Header("Lifespan")]
    [Tooltip("Seconds before this projectile is automatically destroyed, so projectiles " +
        "that never hit anything cannot leak and pile up off-screen.")]
    [Range(0.1f, 30f)]
    [SerializeField] private float lifeTime = 5.0f;

    /// <summary>
    /// Description:
    /// Standard Unity function called when the script instance is loaded.
    /// Lifecycle split: Awake registers the self-destruct so the lifespan timer
    /// begins the instant the projectile exists, before any frame is drawn.
    /// </summary>
    private void Awake()
    {
        Debug.Log($"[Projectile] Awake on '{gameObject.name}' - lifespan {lifeTime}s.");
        // Schedule cleanup once. No manual timer / Update countdown needed.
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once per frame. Kept lightweight - it only
    /// delegates to the MoveProjectile() helper, never inlines movement maths.
    /// </summary>
    private void Update()
    {
        MoveProjectile();
    }

    /// <summary>
    /// Description:
    /// Helper function that moves the projectile forward in the direction it faces.
    /// Multiplied by Time.deltaTime to stay framerate-independent.
    /// </summary>
    private void MoveProjectile()
    {
        transform.position = transform.position + transform.up * projectileSpeed * Time.deltaTime;
    }
}
