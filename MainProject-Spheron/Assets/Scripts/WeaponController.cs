using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float fireRate = 5f;            // shots per second
    [SerializeField] private float projectileSpeed = 30f;

    private float cooldown;

    void Update()
    {
        cooldown -= Time.deltaTime;
        if (Input.GetButton("Fire1") && cooldown <= 0f)
        {
            Fire();
            cooldown = 1f / Mathf.Max(0.0001f, fireRate);
        }
    }

    void Fire()
    {
        if (projectilePrefab == null || muzzle == null) return;
        GameObject proj = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.Launch(muzzle.forward * projectileSpeed);
    }
}
