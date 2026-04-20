using UnityEngine;
using TowerGame.Player;
using TowerGame.Utilities;

namespace TowerGame.Items
{
    /// <summary>
    /// World pickup. When the player walks over it the item gets added to the
    /// inventory and the drop destroys itself.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class ItemDrop : MonoBehaviour
    {
        public ItemSO item;

        public static ItemDrop Spawn(Vector3 position, ItemSO item)
        {
            var go = new GameObject($"Drop_{item.displayName}");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.SquareSprite(item.spriteColor, 24, 24);
            sr.sortingOrder = 3;
            go.transform.localScale = Vector3.one * 0.6f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;
            col.isTrigger = true;

            var drop = go.AddComponent<ItemDrop>();
            drop.item = item;
            return drop;
        }

        private void Update()
        {
            transform.position += new Vector3(0f, Mathf.Sin(Time.time * 3f) * Time.deltaTime * 0.15f, 0f);
            transform.Rotate(0f, 0f, 40f * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var inv = other.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.AddItem(item);
                Destroy(gameObject);
            }
        }
    }
}
