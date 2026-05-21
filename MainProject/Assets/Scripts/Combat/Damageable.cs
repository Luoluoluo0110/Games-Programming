using UnityEngine;

namespace TowerGame.Combat
{
    public enum Team { Neutral, Player, Enemy }

    /// <summary>
    /// Unified damage receiver. Player and enemies both attach this; the attacker
    /// just needs to call <see cref="TakeDamage"/>.
    /// </summary>
    public class Damageable : MonoBehaviour
    {
        public Team team = Team.Neutral;
        public float maxHp = 100f;
        public float hp = 100f;
        public bool invincible = false;

        public event System.Action<float, Transform> OnDamaged;
        public event System.Action OnKilled;

        private void OnEnable() { if (hp <= 0f) hp = maxHp; }

        public void TakeDamage(float amount, Transform source)
        {
            if (invincible || hp <= 0f) return;

            // Forward damage to PlayerStats when this is the player so HP/UI stay in sync.
            if (team == Team.Player)
            {
                var stats = GetComponent<Player.PlayerStats>();
                if (stats != null) stats.Damage(amount);
                OnDamaged?.Invoke(amount, source);
                return;
            }

            hp = Mathf.Max(0f, hp - amount);
            OnDamaged?.Invoke(amount, source);
            if (hp <= 0f) OnKilled?.Invoke();
        }
    }
}
