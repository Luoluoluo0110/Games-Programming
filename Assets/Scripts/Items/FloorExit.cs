using UnityEngine;
using TowerGame.Core;
using TowerGame.Player;
using TowerGame.Utilities;

namespace TowerGame.Items
{
    /// <summary>
    /// Portal that appears after the boss dies.  Requires the floor key to use; on
    /// contact it advances the tower one floor.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class FloorExit : MonoBehaviour
    {
        public FloorConfigSO config;

        public static FloorExit Spawn(Vector3 position, FloorConfigSO config, Transform parent)
        {
            var go = new GameObject("FloorExit");
            go.transform.SetParent(parent);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.RingSprite(new Color(1f, 0.95f, 0.5f, 0.85f),
                new Color(1f, 0.95f, 0.5f, 0.25f), 96, 96);
            sr.sortingOrder = 2;
            go.transform.localScale = Vector3.one * 1.4f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.7f;
            col.isTrigger = true;

            var exit = go.AddComponent<FloorExit>();
            exit.config = config;
            return exit;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, 60f * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var inv = other.GetComponent<PlayerInventory>();
            if (inv == null) return;

            if (config != null && config.floorKey != null && !inv.HasItem(config.floorKey))
                return;

            TowerManager.Instance.AdvanceFloor();
        }
    }
}
