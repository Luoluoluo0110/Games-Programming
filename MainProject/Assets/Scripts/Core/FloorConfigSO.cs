using UnityEngine;
using TowerGame.Items;

namespace TowerGame.Core
{
    /// <summary>
    /// Data definition for a single floor: colors used for procedural art, the boss
    /// archetype to spawn, and the unique reward dropped on victory.
    /// </summary>
    [CreateAssetMenu(menuName = "TowerGame/Floor Config", fileName = "FloorConfig")]
    public class FloorConfigSO : ScriptableObject
    {
        public int floorNumber = 1;
        public string floorName = "Floor 1";
        public string bossName = "Boss";
        public Color floorTint = new Color(0.18f, 0.18f, 0.22f);
        public Color bossTint = new Color(0.85f, 0.25f, 0.25f);

        [Header("Stats")]
        public float bossMaxHp = 200f;
        public float bossContactDamage = 10f;
        public float bossMoveSpeed = 2.2f;

        [Header("Rewards")]
        public ItemSO rewardItem;
        public ItemSO floorKey;

        [Header("Spawn")]
        public int bossArchetypeIndex = 0;
    }
}
