using UnityEngine;
using TowerGame.Utilities;

namespace TowerGame.Combat
{
    /// <summary>Simple linear projectile. Self-destructs on wall hit or timeout.</summary>
    public class Projectile : MonoBehaviour
    {
        public Vector2 velocity;
        public float damage;
        public Team ownerTeam;
        public float lifetime = 3f;
        private float _dieAt;

        public static Projectile Spawn(Vector2 pos, Vector2 dir, float speed, float damage, Team team, Color color)
        {
            var go = new GameObject("Projectile");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CircleSprite(color, 24);
            sr.sortingOrder = 6;
            go.transform.localScale = Vector3.one * 0.4f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.isTrigger = true;

            var proj = go.AddComponent<Projectile>();
            proj.velocity = dir.normalized * speed;
            proj.damage = damage;
            proj.ownerTeam = team;
            rb.velocity = proj.velocity;

            return proj;
        }

        private void Start() { _dieAt = Time.time + lifetime; }
        private void Update() { if (Time.time > _dieAt) Destroy(gameObject); }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var dmg = other.GetComponent<Damageable>();
            if (dmg != null && dmg.team != ownerTeam && dmg.team != Team.Neutral)
            {
                dmg.TakeDamage(damage, transform);
                Destroy(gameObject);
                return;
            }

            if (other.GetComponent<BoxCollider2D>() != null && dmg == null)
            {
                Destroy(gameObject);
            }
        }
    }
}
