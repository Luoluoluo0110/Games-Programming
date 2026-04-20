using UnityEngine;
using TowerGame.Combat;
using TowerGame.Utilities;

namespace TowerGame.Player
{
    /// <summary>Builds the Player GameObject entirely from code so the scene can stay empty.</summary>
    public static class PlayerFactory
    {
        public static GameObject CreatePlayer()
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.layer = 0;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CircleSprite(new Color(0.35f, 0.75f, 1f), 48);
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.drag = 12f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            go.AddComponent<PlayerStats>();
            go.AddComponent<PlayerCombat>();
            go.AddComponent<PlayerInventory>();

            var damageable = go.AddComponent<Damageable>();
            damageable.team = Team.Player;

            go.AddComponent<PlayerController>();

            return go;
        }
    }
}
