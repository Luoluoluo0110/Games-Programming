using UnityEngine;
using TowerGame.Player;

namespace TowerGame.Items
{
    public enum ItemUsage { Passive, Active, Key }

    /// <summary>
    /// Base class for all rewards.  Concrete behaviour lives in <see cref="OnAcquired"/>
    /// (passive effects applied once when picked up) and <see cref="Use"/> (e.g. stamina potion via F3).
    /// active items).
    /// </summary>
    public abstract class ItemSO : ScriptableObject
    {
        public string displayName = "Item";
        public string description = "";
        public Color spriteColor = Color.white;
        public ItemUsage usage = ItemUsage.Passive;
        public bool consumable = false;

        public virtual void OnAcquired(PlayerStats stats) { }

        public virtual bool Use(PlayerStats stats, PlayerCombat combat) => false;
    }

    public class StaminaPotionItem : ItemSO
    {
        public float restoreAmount = 60f;
        public override bool Use(PlayerStats stats, PlayerCombat combat)
        {
            if (stats == null) return false;
            stats.RestoreStamina(restoreAmount);
            stats.Heal(25f);
            return true;
        }
    }

    public class IronSwordItem : ItemSO
    {
        public float bonus = 0.25f;
        public override void OnAcquired(PlayerStats stats)
        {
            if (stats != null) stats.attackBonus += bonus;
        }
    }

    public class FireboltRuneItem : ItemSO
    {
        public override void OnAcquired(PlayerStats stats)
        {
            if (stats != null) stats.hasFirebolt = true;
        }
    }

    public class DashBootsItem : ItemSO
    {
        public override void OnAcquired(PlayerStats stats)
        {
            if (stats != null) stats.canDash = true;
        }
    }

    public class TowerAmuletItem : ItemSO
    {
        public float bonus = 0.5f;
        public override void OnAcquired(PlayerStats stats)
        {
            if (stats != null)
            {
                stats.attackBonus += bonus;
                stats.maxHp += 50f;
                stats.Heal(50f);
            }
        }
    }

    public class VitalityCharmItem : ItemSO
    {
        public float hpBonus = 35f;
        public override void OnAcquired(PlayerStats stats)
        {
            if (stats != null)
            {
                stats.maxHp += hpBonus;
                stats.Heal(hpBonus);
            }
        }
    }

    public class GritBandItem : ItemSO
    {
        public float regenBonus = 12f;
        public override void OnAcquired(PlayerStats stats)
        {
            if (stats != null) stats.staminaRegen += regenBonus;
        }
    }

    public class PredatorCharmItem : ItemSO
    {
        public float bonus = 0.15f;
        public override void OnAcquired(PlayerStats stats)
        {
            if (stats != null) stats.attackBonus += bonus;
        }
    }

    /// <summary>Floor key – consumed implicitly when walking through the exit.</summary>
    public class FloorKeyItem : ItemSO
    {
        public int floorNumber;
    }
}
