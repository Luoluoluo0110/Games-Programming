using System.Collections.Generic;
using UnityEngine;
using TowerGame.Items;

namespace TowerGame.Player
{
    /// <summary>
    /// Stores every item the player has picked up.  F3 drinks a stamina potion when
    /// the player has picked up at least one (see <see cref="StaminaPotionItem"/>).
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        private readonly List<ItemSO> _items = new List<ItemSO>();
        private readonly Dictionary<ItemSO, int> _counts = new Dictionary<ItemSO, int>();

        public float potionCooldown = 4f;
        private float _nextPotionTime;

        public IReadOnlyList<ItemSO> Items => _items;
        public int GetCount(ItemSO item) => (item != null && _counts.TryGetValue(item, out var n)) ? n : 0;

        public event System.Action<ItemSO> OnItemAdded;
        public event System.Action OnInventoryChanged;

        public bool HasItem(ItemSO item) => item != null && _counts.ContainsKey(item);

        public int StaminaPotionCount
        {
            get
            {
                var pot = ItemLibrary.StaminaPotion();
                return _counts.TryGetValue(pot, out var n) ? n : 0;
            }
        }

        public void AddItem(ItemSO item)
        {
            if (item == null) return;

            if (_counts.TryGetValue(item, out var existing))
            {
                _counts[item] = existing + 1;
            }
            else
            {
                _counts[item] = 1;
                _items.Add(item);
                item.OnAcquired(GetComponent<PlayerStats>());
            }

            OnItemAdded?.Invoke(item);
            OnInventoryChanged?.Invoke();
        }

        public bool UseStaminaPotion()
        {
            if (Time.time < _nextPotionTime) return false;

            var pot = ItemLibrary.StaminaPotion();
            if (!_counts.ContainsKey(pot) || _counts[pot] <= 0) return false;

            var stats = GetComponent<PlayerStats>();
            var combat = GetComponent<PlayerCombat>();
            if (!pot.Use(stats, combat)) return false;

            _nextPotionTime = Time.time + potionCooldown;
            if (pot.consumable) ConsumeOne(pot);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public float PotionCooldownRemaining => Mathf.Max(0f, _nextPotionTime - Time.time);

        private void ConsumeOne(ItemSO item)
        {
            if (!_counts.ContainsKey(item)) return;
            _counts[item]--;
            if (_counts[item] <= 0)
            {
                _counts.Remove(item);
                _items.Remove(item);
            }
        }

        public void Clear()
        {
            _items.Clear();
            _counts.Clear();
            OnInventoryChanged?.Invoke();
        }
    }
}
