using UnityEngine;

// Player firing logic. While Fire1 is held it spawns a projectile from the muzzle at the
// configured fire rate (shots per second) and plays the optional fire SFX, muzzle flash and
// weapon animation. Firing is disabled once the round is over so clicks on the end screen
// don't spawn projectiles.
public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float fireRate = 5f;            // shots per second
    [SerializeField] private float projectileSpeed = 30f;

    [Header("FX (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSfx;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator weaponAnimator;

    private float cooldown;

    void Update()
    {
        // don't shoot once the round is over, or clicks on the end screen would spawn projectiles
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.State.Playing)
            return;

        cooldown -= Time.deltaTime;
        if (Input.GetButton("Fire1") && cooldown <= 0f)
        {
            Fire();
            cooldown = 1f / Mathf.Max(0.0001f, fireRate);   // seconds between shots
        }
    }

    void Fire()
    {
        if (projectilePrefab == null || muzzle == null) return;
        GameObject proj = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.Launch(muzzle.forward * projectileSpeed);

        if (audioSource != null && fireSfx != null) audioSource.PlayOneShot(fireSfx);
        if (muzzleFlash != null) muzzleFlash.Play();
        if (weaponAnimator != null) weaponAnimator.SetTrigger("Fire");
    }
}
