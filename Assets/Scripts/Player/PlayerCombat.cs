using UnityEngine;
using TowerGame.Combat;
using TowerGame.Utilities;

namespace TowerGame.Player
{
    /// <summary>
    /// Left-click melee, right-click ranged — both use the same mouse aim and have
    /// independent cooldowns so you can weave them while kiting.  The Firebolt Rune
    /// upgrades the ranged bolt.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Melee")]
        public float meleeRadius = 1.2f;
        public float meleeDamage = 22f;
        public float meleeCooldown = 0.28f;

        [Header("Ranged (basic bolt)")]
        public float boltBaseDamage = 12f;
        public float boltBaseSpeed = 10f;
        public float boltCooldown = 0.38f;
        public float boltStaminaCost = 8f;

        [Header("Ranged (Firebolt upgrade)")]
        public float fireboltDamage = 38f;
        public float fireboltSpeed = 13f;
        public float fireboltStaminaCost = 12f;

        public LayerMask enemyMask = ~0;

        private float _nextMelee;
        private float _nextBolt;
        private PlayerStats _stats;
        private PlayerController _controller;

        public float GetAttackMultiplier() => 1f + (_stats != null ? _stats.attackBonus : 0f);

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
        }

        private PlayerController Controller
        {
            get
            {
                if (_controller == null) _controller = GetComponent<PlayerController>();
                return _controller;
            }
        }

        public void TryMelee()
        {
            if (Time.time < _nextMelee) return;
            _nextMelee = Time.time + meleeCooldown;

            Vector2 aim = Controller.AimDirection;
            Vector2 origin = (Vector2)transform.position + aim * 0.7f;
            var hits = Physics2D.OverlapCircleAll(origin, meleeRadius, enemyMask);
            foreach (var hit in hits)
            {
                var dmg = hit.GetComponent<Damageable>();
                if (dmg != null && dmg.team != Team.Player)
                    dmg.TakeDamage(meleeDamage * GetAttackMultiplier(), transform);
            }

            SpawnSlashFx(origin);
        }

        public void TryRanged()
        {
            if (_stats == null) return;
            if (Time.time < _nextBolt) return;

            bool upgraded = _stats.hasFirebolt;
            float cost = upgraded ? fireboltStaminaCost : boltStaminaCost;
            if (!_stats.TrySpendStamina(cost)) return;

            _nextBolt = Time.time + boltCooldown;

            float damage = (upgraded ? fireboltDamage : boltBaseDamage) * GetAttackMultiplier();
            float speed = upgraded ? fireboltSpeed : boltBaseSpeed;
            Color color = upgraded ? new Color(1f, 0.55f, 0.15f) : new Color(0.55f, 0.75f, 1f);

            Vector2 origin = (Vector2)transform.position + Controller.AimDirection * 0.6f;
            Projectile.Spawn(origin, Controller.AimDirection, speed, damage, Team.Player, color);
        }

        private void SpawnSlashFx(Vector2 position)
        {
            var go = new GameObject("SlashFx");
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.SolidSprite(new Color(1f, 1f, 1f, 0.7f), 16, 16);
            sr.sortingOrder = 5;
            go.transform.localScale = new Vector3(meleeRadius * 2f, meleeRadius * 2f, 1f);
            Destroy(go, 0.08f);
        }
    }
}
