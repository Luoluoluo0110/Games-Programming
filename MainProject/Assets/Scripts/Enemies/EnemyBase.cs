using UnityEngine;
using TowerGame.Combat;
using TowerGame.Player;

namespace TowerGame.Enemies
{
    /// <summary>Common behaviour for enemies: chase the player and deal contact damage.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        public float moveSpeed = 2f;
        public float contactDamage = 8f;
        public float contactCooldown = 0.8f;

        protected Rigidbody2D _rb;
        protected Damageable _damageable;
        protected Transform _targetTf;
        private float _nextHit;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _damageable = GetComponent<Damageable>();
            if (_damageable != null) _damageable.OnKilled += HandleKilled;
        }

        protected virtual void Start()
        {
            _targetTf = PlayerController.Instance != null ? PlayerController.Instance.transform : null;
        }

        protected virtual void FixedUpdate()
        {
            if (_targetTf == null) return;
            Vector2 dir = ((Vector2)_targetTf.position - (Vector2)transform.position).normalized;
            _rb.velocity = dir * moveSpeed;
        }

        private void OnCollisionStay2D(Collision2D other) { TryContactDamage(other.collider); }
        private void OnTriggerStay2D(Collider2D other) { TryContactDamage(other); }

        private void TryContactDamage(Collider2D other)
        {
            if (Time.time < _nextHit) return;
            var dmg = other.GetComponent<Damageable>();
            if (dmg != null && dmg.team == Team.Player)
            {
                dmg.TakeDamage(contactDamage, transform);
                _nextHit = Time.time + contactCooldown;
            }
        }

        protected virtual void HandleKilled() { Destroy(gameObject); }
    }
}
