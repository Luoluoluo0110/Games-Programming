using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A class which controls aiming and shooting - used by both the player and enemies.
///
/// Architecture role:
/// When player controlled, it manages its own fire InputAction lifecycle
/// (enable in OnEnable, disable in OnDisable). When enemy controlled, Enemy.cs
/// calls Fire() directly, so the same component serves both without coupling.
/// </summary>
public class ShootingController : MonoBehaviour
{
    [Header("GameObject/Component References")]
    [Tooltip("The projectile to be fired.")]
    public GameObject projectilePrefab = null;
    [Tooltip("The transform in the hierarchy which holds projectiles, if any")]
    public Transform projectileHolder = null;

    [Header("Input Settings, Actions, & Controls")]
    [Tooltip("Whether this shooting controller is controlled by the player")]
    public bool isPlayerControlled = false;
    [Tooltip("The input action that maps to firing")]
    public InputAction fireAction;

    [Header("Firing Settings")]
    [Tooltip("The minimum time between projectiles being fired.")]
    [Range(0.01f, 5f)]
    public float fireRate = 0.05f;
    [Tooltip("The maximum difference (in degrees) between the direction this " +
        "controller faces and the direction projectiles are launched.")]
    [Range(0f, 45f)]
    public float projectileSpread = 1.0f;

    [Header("Effects")]
    [Tooltip("The effect to create when this fires")]
    public GameObject fireEffect;

    // The last time this component was fired
    private float lastFired = Mathf.NegativeInfinity;

    /// <summary>
    /// Description:
    /// Standard Unity function called whenever the attached gameobject is enabled.
    /// INPUT LIFECYCLE: only enable the fire action if this controller actually
    /// reads player input - enemy controllers never touch the input system.
    /// </summary>
    private void OnEnable()
    {
        if (isPlayerControlled)
        {
            Debug.Log("[ShootingController] OnEnable - enabling fire action.");
            fireAction.Enable();
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called whenever the attached gameobject is disabled.
    /// INPUT LIFECYCLE: cleanly disable the fire action to avoid lingering input events.
    /// </summary>
    private void OnDisable()
    {
        if (isPlayerControlled)
        {
            Debug.Log("[ShootingController] OnDisable - disabling fire action.");
            fireAction.Disable();
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update.
    /// </summary>
    private void Start()
    {
        if (fireAction.bindings.Count == 0 && isPlayerControlled)
        {
            Debug.LogWarning("The Fire Input Action does not have a binding set but is set to be " +
                "player controlled! Make sure it has a binding or the shooting controller will not shoot!");
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called every frame. Kept lightweight - it only
    /// delegates to the ProcessInput() helper.
    /// </summary>
    private void Update()
    {
        ProcessInput();
    }

    /// <summary>
    /// Description:
    /// Helper that reads the fire input (player controlled only) and fires.
    /// </summary>
    private void ProcessInput()
    {
        if (!isPlayerControlled)
        {
            return;
        }

        if (fireAction.bindings.Count == 0)
        {
            Debug.LogError("The Fire Input Action does not have a binding set! It must have a " +
                "binding set in order to fire!");
            return;
        }

        if (fireAction.ReadValue<float>() >= 1)
        {
            Fire();
        }
    }

    /// <summary>
    /// Description:
    /// Fires a projectile if the fire-rate cooldown has elapsed.
    /// Public so enemies can drive it directly via Enemy.TryToShoot().
    /// </summary>
    public void Fire()
    {
        // Respect the cooldown.
        if ((Time.timeSinceLevelLoad - lastFired) <= fireRate)
        {
            return;
        }

        SpawnProjectile();

        if (fireEffect != null)
        {
            Instantiate(fireEffect, transform.position, transform.rotation, null);
        }

        lastFired = Time.timeSinceLevelLoad;
    }

    /// <summary>
    /// Description:
    /// Helper that instantiates and configures a single projectile.
    /// </summary>
    public void SpawnProjectile()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        // Create the projectile.
        GameObject projectileGameObject = Instantiate(projectilePrefab, transform.position,
            transform.rotation, null);

        // Account for spread.
        Vector3 rotationEulerAngles = projectileGameObject.transform.rotation.eulerAngles;
        rotationEulerAngles.z += Random.Range(-projectileSpread, projectileSpread);
        projectileGameObject.transform.rotation = Quaternion.Euler(rotationEulerAngles);

        // Keep the hierarchy organised.
        if (projectileHolder == null && GameObject.Find("ProjectileHolder") != null)
        {
            projectileHolder = GameObject.Find("ProjectileHolder").transform;
        }
        if (projectileHolder != null)
        {
            projectileGameObject.transform.SetParent(projectileHolder);
        }
    }
}
