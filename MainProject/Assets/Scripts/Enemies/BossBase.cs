using UnityEngine;
using TowerGame.Combat;
using TowerGame.Core;

namespace TowerGame.Enemies
{
    /// <summary>
    /// Boss-specific behaviour on top of <see cref="EnemyBase"/>: raises an event so
    /// <see cref="TowerManager"/> knows to drop rewards when it dies.
    /// </summary>
    public abstract class BossBase : EnemyBase
    {
        public FloorConfigSO config;
        public string displayName = "Boss";

        public event System.Action<BossBase> OnDeath;
        public event System.Action<float, float> OnHealthChanged;

        public float MaxHp => _damageable != null ? _damageable.maxHp : 0f;
        public float CurrentHp => _damageable != null ? _damageable.hp : 0f;

        public void Configure(FloorConfigSO cfg)
        {
            config = cfg;
            displayName = cfg.bossName;
            moveSpeed = cfg.bossMoveSpeed;
            contactDamage = cfg.bossContactDamage;
            if (_damageable == null) _damageable = GetComponent<Damageable>();
            if (_damageable != null)
            {
                _damageable.maxHp = cfg.bossMaxHp;
                _damageable.hp = cfg.bossMaxHp;
                _damageable.team = Team.Enemy;
                _damageable.OnDamaged += HandleDamaged;
            }
        }

        private void HandleDamaged(float amt, Transform src)
        {
            OnHealthChanged?.Invoke(CurrentHp, MaxHp);
        }

        protected override void HandleKilled()
        {
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
