using UnityEngine;
using TowerGame.Core;

namespace TowerGame.Player
{
    /// <summary>
    /// Twin-stick style: WASD moves, mouse aims. Left-click (hold) melee, right-click
    /// (hold) ranged — independent cooldowns for weaving while kiting. F3 drinks a
    /// stamina potion; Space dashes when unlocked.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        public float moveSpeed = 5.8f;
        public float dashSpeed = 15f;
        public float dashDuration = 0.18f;
        public float dashCooldown = 0.6f;
        public float dashStaminaCost = 22f;

        public PlayerStats Stats { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public PlayerInventory Inventory { get; private set; }

        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public Vector2 MoveDirection { get; private set; } = Vector2.zero;

        private Rigidbody2D _rb;
        private float _dashEndTime;
        private float _nextDashTime;
        private Vector2 _dashDir;
        private Camera _cam;

        private void Awake()
        {
            Instance = this;
            _rb = GetComponent<Rigidbody2D>();
            Stats = GetComponent<PlayerStats>();
            Combat = GetComponent<PlayerCombat>();
            Inventory = GetComponent<PlayerInventory>();
            Stats.OnDeath += HandleDeath;
        }

        private void Start()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if ((gm.State == GameState.Playing || gm.State == GameState.Paused)
                && Input.GetKeyDown(KeyCode.Escape))
            {
                gm.TogglePause();
                return;
            }

            if (gm.State != GameState.Playing) return;

            if (_cam == null) _cam = Camera.main;
            if (_cam != null)
            {
                Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 aim = (Vector2)(mouseWorld - transform.position);
                if (aim.sqrMagnitude > 0.01f) AimDirection = aim.normalized;
            }

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f) input.Normalize();
            MoveDirection = input;

            bool dashing = Time.time < _dashEndTime;

            if (!dashing && Input.GetKeyDown(KeyCode.Space) && Stats.canDash
                && Time.time >= _nextDashTime && input.sqrMagnitude > 0.01f
                && Stats.TrySpendStamina(dashStaminaCost))
            {
                _dashEndTime = Time.time + dashDuration;
                _nextDashTime = Time.time + dashCooldown;
                _dashDir = input.normalized;
                dashing = true;
            }

            Vector2 velocity = dashing ? _dashDir * dashSpeed : input * moveSpeed;
            _rb.velocity = velocity;

            if (Input.GetMouseButton(0)) Combat.TryMelee();
            if (Input.GetMouseButton(1)) Combat.TryRanged();
            if (Input.GetKeyDown(KeyCode.F3)) Inventory.UseStaminaPotion();
        }

        private void HandleDeath()
        {
            _rb.velocity = Vector2.zero;
            GameManager.Instance?.SetState(GameState.GameOver);
        }
    }
}
