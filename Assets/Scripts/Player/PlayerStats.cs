using UnityEngine;

namespace TowerGame.Player
{
    /// <summary>
    /// HP + Stamina resource model for the player.  Emits change events so the HUD
    /// can redraw without polling every frame.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("HP")]
        public float maxHp = 100f;
        public float hp = 100f;

        [Header("Stamina")]
        public float maxStamina = 100f;
        public float stamina = 100f;
        public float staminaRegen = 26f;

        [Header("Level")]
        public int level = 1;
        public int xp = 0;

        public event System.Action OnChanged;
        public event System.Action OnDeath;

        public float attackBonus = 0f;
        public bool canDash = false;
        public bool hasFirebolt = false;

        private void Update()
        {
            if (stamina < maxStamina)
            {
                stamina = Mathf.Min(maxStamina, stamina + staminaRegen * Time.deltaTime);
                OnChanged?.Invoke();
            }
        }

        public bool TrySpendStamina(float amount)
        {
            if (stamina < amount) return false;
            stamina -= amount;
            OnChanged?.Invoke();
            return true;
        }

        public void Damage(float amount)
        {
            if (hp <= 0f) return;
            hp = Mathf.Max(0f, hp - amount);
            OnChanged?.Invoke();
            if (hp <= 0f) OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            hp = Mathf.Min(maxHp, hp + amount);
            OnChanged?.Invoke();
        }

        public void RestoreStamina(float amount)
        {
            stamina = Mathf.Min(maxStamina, stamina + amount);
            OnChanged?.Invoke();
        }

        public void FullHeal()
        {
            hp = maxHp;
            stamina = maxStamina;
            OnChanged?.Invoke();
        }
    }
}
