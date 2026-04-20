using UnityEngine;
using TowerGame.Combat;
using TowerGame.Core;
using TowerGame.Utilities;

namespace TowerGame.Enemies
{
    /// <summary>Spawns the correct boss prefab for a given floor config entirely from code.</summary>
    public static class BossFactory
    {
        public static BossBase SpawnBoss(FloorConfigSO config, Vector3 position, Transform parent)
        {
            var go = new GameObject($"Boss_{config.bossName}");
            go.transform.SetParent(parent);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.SquareSprite(config.bossTint, 64, 64);
            sr.sortingOrder = 4;
            go.transform.localScale = Vector3.one * 1.1f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.drag = 6f;
            rb.mass = 5f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.55f;

            go.AddComponent<Damageable>();

            BossBase boss;
            switch (config.bossArchetypeIndex)
            {
                case 0: boss = go.AddComponent<SlimeKingBoss>(); break;
                case 1: boss = go.AddComponent<SkeletonLordBoss>(); break;
                case 2: boss = go.AddComponent<FireWardenBoss>(); break;
                case 3: boss = go.AddComponent<ShadowKnightBoss>(); break;
                case 4: boss = go.AddComponent<StormTitanBoss>(); break;
                case 5: boss = go.AddComponent<FrostMatriarchBoss>(); break;
                case 6: boss = go.AddComponent<IronColossusBoss>(); break;
                default: boss = go.AddComponent<TowerLichBoss>(); break;
            }
            boss.Configure(config);
            return boss;
        }
    }
}
