using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class handles the health state of a game object.
///
/// Architecture role:
/// Health is the END of the deterministic damage chain:
///     OnTriggerEnter2D() -> DealDamage() -> TakeDamage() -> CheckDeath() -> Die()
/// It owns NO knowledge of the UI, the score, or the audio system. Instead it
/// broadcasts state changes through UnityEvents so other systems can react
/// without Health ever referencing them directly (component decoupling).
///
/// Implementation Notes: 2D Rigidbodies must be set to never sleep for this to
/// interact with trigger-stay damage.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour
{
    [Header("Team Settings")]
    [Tooltip("The team associated with this health. Damage only applies between different teams.")]
    public int teamId = 0;

    [Header("Health Settings")]
    [Tooltip("The default health value")]
    [SerializeField] private int defaultHealth = 1;
    [Tooltip("The maximum health value")]
    [SerializeField] private int maximumHealth = 1;
    [Tooltip("The current in-game health value")]
    public int currentHealth = 1;
    [Tooltip("Invulnerability duration, in seconds, after taking damage")]
    [Range(0f, 10f)]
    public float invincibilityTime = 3f;
    [Tooltip("Whether or not this health is always invincible")]
    public bool isAlwaysInvincible = false;

    [Header("Lives settings")]
    [Tooltip("Whether or not to use lives")]
    public bool useLives = false;
    [Tooltip("Current number of lives this health has")]
    public int currentLives = 3;
    [Tooltip("The maximum number of lives this health can have")]
    public int maximumLives = 5;

    [Header("Effects & Polish")]
    [Tooltip("The effect to create when this health dies")]
    public GameObject deathEffect;
    [Tooltip("The effect to create when this health is damaged")]
    public GameObject hitEffect;

    [Header("Decoupled Events (broadcast state changes - no hard references)")]
    [Tooltip("Invoked any time current health changes (damage OR healing). Hook UI bars here.")]
    public UnityEvent onHealthChanged;
    [Tooltip("Invoked the moment damage is successfully applied. Hook hit SFX / screen shake here.")]
    public UnityEvent onTakeDamage;
    [Tooltip("Invoked when this health reaches zero. Hook death SFX / score / UI here.")]
    public UnityEvent onDeath;

    // The specific game time when the health can be damaged again
    private float timeToBecomeDamagableAgain = 0;
    // Whether or not the health is currently invincible from a recent hit
    private bool isInvincableFromDamage = false;
    // The position that this game object will respawn at if lives are being used
    private Vector3 respawnPosition;

    /// <summary>
    /// Description:
    /// Standard Unity function called when the script instance is loaded.
    /// Lifecycle split: Awake handles SELF-contained data validation that does not
    /// depend on any other object existing yet.
    /// </summary>
    private void Awake()
    {
        Debug.Log($"[Health] Awake on '{gameObject.name}' (teamId {teamId}).");
        // Guarantee the inspector values are internally consistent before play begins.
        maximumHealth = Mathf.Max(1, maximumHealth);
        defaultHealth = Mathf.Clamp(defaultHealth, 1, maximumHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maximumHealth);
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called before the first frame update.
    /// Lifecycle split: Start handles setup that depends on this object's final
    /// world position being settled (the respawn point) and pushes the first UI sync.
    /// </summary>
    private void Start()
    {
        Debug.Log($"[Health] Start on '{gameObject.name}'.");
        SetRespawnPoint(transform.position);
        // Broadcast the starting value so listeners (UI) initialise correctly.
        onHealthChanged.Invoke();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once per frame. Kept intentionally lightweight -
    /// it only delegates to a dedicated helper, never inlines logic.
    /// </summary>
    private void Update()
    {
        InvincibilityCheck();
    }

    /// <summary>
    /// Description:
    /// Checks the current time against the time the health can be damaged again,
    /// removing invincibility once the window has elapsed.
    /// </summary>
    private void InvincibilityCheck()
    {
        if (timeToBecomeDamagableAgain <= Time.time)
        {
            isInvincableFromDamage = false;
        }
    }

    /// <summary>
    /// Description:
    /// Changes the respawn position to a new position.
    /// </summary>
    /// <param name="newRespawnPosition">The new position to respawn at</param>
    public void SetRespawnPoint(Vector3 newRespawnPosition)
    {
        respawnPosition = newRespawnPosition;
    }

    /// <summary>
    /// Description:
    /// Repositions this game object to the respawn position and resets health.
    /// </summary>
    private void Respawn()
    {
        transform.position = respawnPosition;
        currentHealth = defaultHealth;
        onHealthChanged.Invoke();
    }

    /// <summary>
    /// Description:
    /// CHAIN STEP 3 - Applies damage to the health unless it is invincible.
    /// This is the helper function called by Damage.DealDamage(). It never
    /// touches the attacker; it only mutates its own state, then forwards to CheckDeath().
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take</param>
    public void TakeDamage(int damageAmount)
    {
        // Guard clause: invincible health absorbs the hit and the chain stops here.
        if (isInvincableFromDamage || isAlwaysInvincible)
        {
            return;
        }

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, transform.rotation, null);
        }

        // Start the invincibility window so a single overlap can't apply damage twice.
        timeToBecomeDamagableAgain = Time.time + invincibilityTime;
        isInvincableFromDamage = true;

        currentHealth -= damageAmount;

        // Broadcast: UI / audio react here WITHOUT Health knowing they exist.
        onTakeDamage.Invoke();
        onHealthChanged.Invoke();

        // CHAIN STEP 4 - hand off to the death check.
        CheckDeath();
    }

    /// <summary>
    /// Description:
    /// Applies healing to the health, capped at the maximum health.
    /// </summary>
    /// <param name="healingAmount">How much healing to apply</param>
    public void ReceiveHealing(int healingAmount)
    {
        currentHealth += healingAmount;
        if (currentHealth > maximumHealth)
        {
            currentHealth = maximumHealth;
        }
        onHealthChanged.Invoke();
        CheckDeath();
    }

    /// <summary>
    /// Description:
    /// CHAIN STEP 4 - Checks whether the health has dropped to zero.
    /// Calls Die() if so. Returns true when dead.
    /// </summary>
    /// <returns>bool: true if the health has died, false otherwise</returns>
    private bool CheckDeath()
    {
        if (currentHealth <= 0)
        {
            // CHAIN STEP 5 - the final step.
            Die();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Description:
    /// CHAIN STEP 5 - Handles the death of the health. Spawns the death effect,
    /// fires the decoupled onDeath event, then delegates to the lives-aware handlers.
    /// </summary>
    public void Die()
    {
        Debug.Log($"[Health] Die() on '{gameObject.name}'.");

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation, null);
        }

        // Broadcast death so score / SFX / UI react without a hard reference.
        onDeath.Invoke();

        if (useLives)
        {
            HandleDeathWithLives();
        }
        else
        {
            HandleDeathWithoutLives();
        }
    }

    /// <summary>
    /// Description:
    /// Handles death when lives are being used: respawn while lives remain,
    /// otherwise run the final destruction path.
    /// </summary>
    private void HandleDeathWithLives()
    {
        currentLives -= 1;
        if (currentLives > 0)
        {
            Respawn();
        }
        else
        {
            NotifyGameManagerAndDestroy();
        }
    }

    /// <summary>
    /// Description:
    /// Handles death when lives are not being used.
    /// </summary>
    private void HandleDeathWithoutLives()
    {
        NotifyGameManagerAndDestroy();
    }

    /// <summary>
    /// Description:
    /// Final destruction path. Notifies the GameManager singleton if this was the
    /// player, lets an Enemy bank its score before destruction, then destroys
    /// THIS game object (the one running the script) - never the attacker.
    /// </summary>
    private void NotifyGameManagerAndDestroy()
    {
        if (gameObject.name == "Player" && gameObject.tag != "Player")
        {
            Debug.LogWarning("It looks like you're trying to kill a player, but your player " +
                "hasn't been tagged as 'Player' in the inspector! \n Please tag your player.");
        }

        // SINGLETON ACCESS: route a player death through the global GameManager.
        if (gameObject.tag == "Player" && GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }

        // Let an enemy award score before it leaves the scene.
        Enemy enemyComponent = gameObject.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.DoBeforeDestroy();
        }

        // SAFETY: this.gameObject is the object running this script (the victim).
        Destroy(this.gameObject);
    }
}
