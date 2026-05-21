using System.Collections.Generic;
using UnityEngine;
using TowerGame.Enemies;
using TowerGame.Items;
using TowerGame.Player;
using TowerGame.Utilities;

namespace TowerGame.Core
{
    /// <summary>
    /// Owns the tower run state: which floor the player is on, which bosses/rewards
    /// have been defined, and how to rebuild the arena when the player climbs higher.
    /// </summary>
    public class TowerManager : MonoBehaviour
    {
        public static TowerManager Instance { get; private set; }

        public const int TotalFloors = 8;

        public int CurrentFloor { get; private set; } = 1;
        public FloorConfigSO CurrentConfig => _configs[CurrentFloor - 1];
        public IReadOnlyList<FloorConfigSO> AllConfigs => _configs;

        public event System.Action<int> OnFloorChanged;
        public event System.Action<FloorConfigSO> OnBossDefeated;

        private readonly List<FloorConfigSO> _configs = new List<FloorConfigSO>();
        private Transform _arenaRoot;
        private BossBase _activeBoss;

        public Vector2 ArenaMin => new Vector2(-9f, -5f);
        public Vector2 ArenaMax => new Vector2(9f, 5f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildConfigs();
        }

        private void BuildConfigs()
        {
            _configs.Add(MakeConfig(1, "Slime Pit", "Slime King", new Color(0.35f, 0.55f, 0.25f),
                new Color(0.45f, 0.85f, 0.35f), 180, 8, 1.8f, 0,
                ItemLibrary.StaminaPotion(), ItemLibrary.FloorKey(1)));
            _configs.Add(MakeConfig(2, "Bone Hall", "Skeleton Lord", new Color(0.30f, 0.30f, 0.35f),
                new Color(0.95f, 0.95f, 0.85f), 260, 12, 2.4f, 1,
                ItemLibrary.IronSword(), ItemLibrary.FloorKey(2)));
            _configs.Add(MakeConfig(3, "Ember Vault", "Fire Warden", new Color(0.45f, 0.18f, 0.15f),
                new Color(1.00f, 0.55f, 0.15f), 340, 14, 2.0f, 2,
                ItemLibrary.FireboltRune(), ItemLibrary.FloorKey(3)));
            _configs.Add(MakeConfig(4, "Shadow Deck", "Shadow Knight", new Color(0.15f, 0.15f, 0.25f),
                new Color(0.35f, 0.25f, 0.55f), 420, 16, 3.0f, 3,
                ItemLibrary.DashBoots(), ItemLibrary.FloorKey(4)));
            _configs.Add(MakeConfig(5, "Storm Gallery", "Storm Titan", new Color(0.22f, 0.28f, 0.42f),
                new Color(0.55f, 0.75f, 1f), 480, 17, 2.2f, 4,
                ItemLibrary.VitalityCharm(), ItemLibrary.FloorKey(5)));
            _configs.Add(MakeConfig(6, "Frost Sanctum", "Frost Matriarch", new Color(0.75f, 0.88f, 0.95f),
                new Color(0.35f, 0.55f, 0.85f), 540, 18, 2.0f, 5,
                ItemLibrary.GritBand(), ItemLibrary.FloorKey(6)));
            _configs.Add(MakeConfig(7, "Iron Foundry", "Iron Colossus", new Color(0.35f, 0.32f, 0.30f),
                new Color(0.65f, 0.62f, 0.58f), 620, 20, 1.5f, 6,
                ItemLibrary.PredatorCharm(), ItemLibrary.FloorKey(7)));
            _configs.Add(MakeConfig(8, "Lich Summit", "Tower Lich", new Color(0.10f, 0.05f, 0.20f),
                new Color(0.55f, 0.95f, 0.95f), 720, 22, 2.5f, 7,
                ItemLibrary.TowerAmulet(), ItemLibrary.FloorKey(8)));
        }

        private static FloorConfigSO MakeConfig(int n, string fName, string bName, Color floor, Color boss,
            float hp, float dmg, float speed, int archetype, ItemSO reward, ItemSO key)
        {
            var c = ScriptableObject.CreateInstance<FloorConfigSO>();
            c.floorNumber = n;
            c.floorName = fName;
            c.bossName = bName;
            c.floorTint = floor;
            c.bossTint = boss;
            c.bossMaxHp = hp;
            c.bossContactDamage = dmg;
            c.bossMoveSpeed = speed;
            c.bossArchetypeIndex = archetype;
            c.rewardItem = reward;
            c.floorKey = key;
            return c;
        }

        public void StartNewRun()
        {
            CurrentFloor = 1;
            LoadFloor(CurrentFloor);
        }

        public void AdvanceFloor()
        {
            if (CurrentFloor >= TotalFloors)
            {
                GameManager.Instance?.SetState(GameState.Victory);
                return;
            }
            CurrentFloor++;
            LoadFloor(CurrentFloor);
        }

        public void LoadFloor(int floor)
        {
            CurrentFloor = Mathf.Clamp(floor, 1, TotalFloors);
            RebuildArena();
            OnFloorChanged?.Invoke(CurrentFloor);
        }

        private void RebuildArena()
        {
            if (_arenaRoot != null) Destroy(_arenaRoot.gameObject);
            var config = CurrentConfig;

            var root = new GameObject($"Arena_Floor{config.floorNumber}");
            _arenaRoot = root.transform;

            BuildFloorTiles(root.transform, config);
            BuildWalls(root.transform, config);

            var player = PlayerController.Instance;
            if (player != null)
            {
                player.transform.position = new Vector3(ArenaMin.x + 1.5f, 0f, 0f);
                player.Stats.FullHeal();
            }

            _activeBoss = BossFactory.SpawnBoss(config, new Vector3(ArenaMax.x - 2f, 0f, 0f), root.transform);
            if (_activeBoss != null)
            {
                _activeBoss.OnDeath += HandleBossDefeated;
            }
        }

        private void BuildFloorTiles(Transform parent, FloorConfigSO config)
        {
            var size = ArenaMax - ArenaMin;
            var floor = new GameObject("Floor");
            floor.transform.SetParent(parent);
            floor.transform.position = (Vector3)((ArenaMin + ArenaMax) * 0.5f);
            var sr = floor.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.CheckerSprite(config.floorTint, config.floorTint * 0.75f, 128, 128, 8);
            sr.sortingOrder = -10;
            floor.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private void BuildWalls(Transform parent, FloorConfigSO config)
        {
            Color wallColor = config.floorTint * 0.4f;
            wallColor.a = 1f;
            CreateWall(parent, "WallTop",
                new Vector2((ArenaMin.x + ArenaMax.x) * 0.5f, ArenaMax.y + 0.5f),
                new Vector2(ArenaMax.x - ArenaMin.x + 2f, 1f), wallColor);
            CreateWall(parent, "WallBottom",
                new Vector2((ArenaMin.x + ArenaMax.x) * 0.5f, ArenaMin.y - 0.5f),
                new Vector2(ArenaMax.x - ArenaMin.x + 2f, 1f), wallColor);
            CreateWall(parent, "WallLeft",
                new Vector2(ArenaMin.x - 0.5f, (ArenaMin.y + ArenaMax.y) * 0.5f),
                new Vector2(1f, ArenaMax.y - ArenaMin.y), wallColor);
            CreateWall(parent, "WallRight",
                new Vector2(ArenaMax.x + 0.5f, (ArenaMin.y + ArenaMax.y) * 0.5f),
                new Vector2(1f, ArenaMax.y - ArenaMin.y), wallColor);
        }

        private static void CreateWall(Transform parent, string name, Vector2 pos, Vector2 scale, Color color)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            var sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.SolidSprite(color, 32, 32);
            sr.sortingOrder = -5;
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        private void HandleBossDefeated(BossBase boss)
        {
            var config = CurrentConfig;
            if (boss != _activeBoss) return;
            _activeBoss = null;

            // Drop reward + key at boss death position.
            if (config.rewardItem != null)
                ItemDrop.Spawn(boss.transform.position + new Vector3(-0.6f, 0f, 0f), config.rewardItem);
            if (config.floorKey != null)
                ItemDrop.Spawn(boss.transform.position + new Vector3(0.6f, 0f, 0f), config.floorKey);

            // Spawn exit portal in the top-right corner.
            FloorExit.Spawn(new Vector3(ArenaMax.x - 1f, ArenaMax.y - 1f, 0f), config, _arenaRoot);

            OnBossDefeated?.Invoke(config);
        }
    }
}
