using UnityEngine;
using TowerGame.Combat;
using TowerGame.Utilities;

namespace TowerGame.Enemies
{
    /// <summary>Floor 1 – slow chaser that occasionally surges toward the player.</summary>
    public class SlimeKingBoss : BossBase
    {
        private float _nextSurge;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_targetTf != null && Time.time > _nextSurge)
            {
                _nextSurge = Time.time + 3.5f;
                Vector2 dir = ((Vector2)_targetTf.position - (Vector2)transform.position).normalized;
                _rb.AddForce(dir * 28f, ForceMode2D.Impulse);
            }
        }
    }

    /// <summary>Floor 2 – circles the player and flings bone projectiles.</summary>
    public class SkeletonLordBoss : BossBase
    {
        private float _nextVolley;

        protected override void FixedUpdate()
        {
            if (_targetTf == null) return;
            Vector2 toPlayer = (Vector2)_targetTf.position - (Vector2)transform.position;
            Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;
            Vector2 approach = toPlayer.sqrMagnitude > 16f ? toPlayer.normalized : Vector2.zero;
            _rb.velocity = (tangent * 0.7f + approach * 0.6f) * moveSpeed;

            if (Time.time > _nextVolley)
            {
                _nextVolley = Time.time + 2.2f;
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 dir = Quaternion.Euler(0, 0, i * 15f) * toPlayer.normalized;
                    Projectile.Spawn(transform.position, dir, 5.5f, 8f, Team.Enemy,
                        new Color(0.9f, 0.9f, 0.75f));
                }
            }
        }
    }

    /// <summary>Floor 3 – stands still and rains fire rings.</summary>
    public class FireWardenBoss : BossBase
    {
        private float _nextRing;

        protected override void FixedUpdate()
        {
            if (_targetTf == null) return;
            Vector2 toPlayer = (Vector2)_targetTf.position - (Vector2)transform.position;
            if (toPlayer.sqrMagnitude > 25f)
                _rb.velocity = toPlayer.normalized * (moveSpeed * 0.6f);
            else
                _rb.velocity = Vector2.zero;

            if (Time.time > _nextRing)
            {
                _nextRing = Time.time + 2.0f;
                for (int i = 0; i < 8; i++)
                {
                    float a = i * (360f / 8f) * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    Projectile.Spawn(transform.position, dir, 4f, 10f, Team.Enemy,
                        new Color(1f, 0.45f, 0.1f));
                }
            }
        }
    }

    /// <summary>Floor 5 – keeps distance and throws fast lightning salvos toward the player.</summary>
    public class StormTitanBoss : BossBase
    {
        private float _nextBurst;

        protected override void FixedUpdate()
        {
            if (_targetTf == null) return;
            Vector2 toPlayer = (Vector2)_targetTf.position - (Vector2)transform.position;
            if (toPlayer.sqrMagnitude > 36f)
                _rb.velocity = toPlayer.normalized * moveSpeed;
            else
                _rb.velocity = -toPlayer.normalized * (moveSpeed * 0.85f);

            if (Time.time > _nextBurst)
            {
                _nextBurst = Time.time + 1.9f;
                Vector2 lead = toPlayer.normalized;
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 dir = Quaternion.Euler(0, 0, i * 12f) * lead;
                    Projectile.Spawn(transform.position, dir, 8.5f, 11f, Team.Enemy,
                        new Color(0.55f, 0.85f, 1f));
                }
            }
        }
    }

    /// <summary>Floor 6 – strafes sideways and fires ice shards in four cardinal directions.</summary>
    public class FrostMatriarchBoss : BossBase
    {
        private float _nextCross;

        protected override void FixedUpdate()
        {
            if (_targetTf == null) return;
            Vector2 toPlayer = (Vector2)_targetTf.position - (Vector2)transform.position;
            Vector2 tangent = new Vector2(-toPlayer.y, toPlayer.x).normalized;
            _rb.velocity = tangent * moveSpeed;

            if (Time.time > _nextCross)
            {
                _nextCross = Time.time + 1.6f;
                for (int i = 0; i < 4; i++)
                {
                    float a = i * 90f * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    Projectile.Spawn(transform.position, dir, 5f, 12f, Team.Enemy,
                        new Color(0.65f, 0.85f, 1f));
                }
            }
        }
    }

    /// <summary>Floor 7 – slow juggernaut that periodically fires a ring of heavy slugs.</summary>
    public class IronColossusBoss : BossBase
    {
        private float _nextRing;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_targetTf == null) return;

            if (Time.time > _nextRing)
            {
                _nextRing = Time.time + 2.8f;
                for (int i = 0; i < 10; i++)
                {
                    float a = i * (360f / 10f) * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    Projectile.Spawn(transform.position, dir, 3.2f, 14f, Team.Enemy,
                        new Color(0.55f, 0.52f, 0.48f));
                }
            }
        }
    }

    /// <summary>Floor 4 – teleports and dashes through the player.</summary>
    public class ShadowKnightBoss : BossBase
    {
        private float _nextDash;
        private float _nextBlink;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_targetTf == null) return;

            if (Time.time > _nextDash)
            {
                _nextDash = Time.time + 2.5f;
                Vector2 dir = ((Vector2)_targetTf.position - (Vector2)transform.position).normalized;
                _rb.AddForce(dir * 45f, ForceMode2D.Impulse);
            }

            if (Time.time > _nextBlink)
            {
                _nextBlink = Time.time + 5.0f;
                Vector2 min = Core.TowerManager.Instance.ArenaMin + new Vector2(1f, 1f);
                Vector2 max = Core.TowerManager.Instance.ArenaMax - new Vector2(1f, 1f);
                transform.position = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), 0f);
            }
        }
    }

    /// <summary>Floor 8 – final boss: chases, shoots spirals, and summons small adds.</summary>
    public class TowerLichBoss : BossBase
    {
        private float _nextSpiral;
        private float _spiralAngle;
        private float _nextAdd;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (_targetTf == null) return;

            if (Time.time > _nextSpiral)
            {
                _nextSpiral = Time.time + 0.15f;
                _spiralAngle += 25f;
                for (int i = 0; i < 3; i++)
                {
                    float a = (_spiralAngle + i * 120f) * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    Projectile.Spawn(transform.position, dir, 4.5f, 9f, Team.Enemy,
                        new Color(0.55f, 0.95f, 1f));
                }
            }

            if (Time.time > _nextAdd)
            {
                _nextAdd = Time.time + 6.0f;
                SpawnAdd();
            }
        }

        private void SpawnAdd()
        {
            var go = new GameObject("LichMinion");
            go.transform.position = transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.SquareSprite(new Color(0.35f, 0.25f, 0.55f), 28, 28);
            sr.sortingOrder = 3;
            go.transform.localScale = Vector3.one * 0.6f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.drag = 4f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;

            var dmg = go.AddComponent<Damageable>();
            dmg.team = Team.Enemy;
            dmg.maxHp = dmg.hp = 30f;

            var minion = go.AddComponent<LichMinion>();
            minion.moveSpeed = 3f;
            minion.contactDamage = 6f;
        }
    }

    /// <summary>Small add summoned by the Tower Lich.</summary>
    public class LichMinion : EnemyBase { }
}
