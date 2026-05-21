using UnityEngine;
using UnityEngine.UI;
using TowerGame.Core;
using TowerGame.Enemies;
using TowerGame.Items;
using TowerGame.Player;
using TowerGame.Utilities;

namespace TowerGame.UI
{
    /// <summary>Top-right square minimap rendered as a simple schematic, no RenderTexture.</summary>
    public class MinimapController : MonoBehaviour
    {
        private RectTransform _arena;
        private RectTransform _playerDot;
        private RectTransform _bossDot;
        private RectTransform _exitDot;
        private Button _menuButton;

        private void Start() { Build(); }

        private void Update()
        {
            var tm = TowerManager.Instance;
            if (tm == null || _arena == null) return;

            Vector2 min = tm.ArenaMin;
            Vector2 max = tm.ArenaMax;
            Vector2 range = max - min;

            var player = PlayerController.Instance;
            if (player != null)
                _playerDot.anchoredPosition = WorldToMap(player.transform.position, min, range);

            var boss = Object.FindObjectOfType<BossBase>();
            if (boss != null)
            {
                _bossDot.gameObject.SetActive(true);
                _bossDot.anchoredPosition = WorldToMap(boss.transform.position, min, range);
            }
            else
            {
                _bossDot.gameObject.SetActive(false);
            }

            var exit = Object.FindObjectOfType<FloorExit>();
            if (exit != null)
            {
                _exitDot.gameObject.SetActive(true);
                _exitDot.anchoredPosition = WorldToMap(exit.transform.position, min, range);
            }
            else
            {
                _exitDot.gameObject.SetActive(false);
            }
        }

        private Vector2 WorldToMap(Vector3 world, Vector2 min, Vector2 range)
        {
            Vector2 size = _arena.rect.size;
            float nx = Mathf.Clamp01((world.x - min.x) / range.x);
            float ny = Mathf.Clamp01((world.y - min.y) / range.y);
            return new Vector2(nx * size.x - size.x * 0.5f, ny * size.y - size.y * 0.5f);
        }

        private void Build()
        {
            _arena = UIFactory.AddPanel(transform, "Minimap",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(1, 1), new Vector2(180, 180),
                new Vector2(-16, -16), new Color(0.05f, 0.05f, 0.08f, 0.85f));

            // Border.
            var border = UIFactory.AddImage(_arena, "Border", new Color(1f, 0.95f, 0.6f, 0.6f));
            var brt = (RectTransform)border.transform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = Vector2.zero;
            brt.offsetMax = Vector2.zero;
            border.sprite = SpriteFactory.RingSprite(new Color(1f, 0.95f, 0.6f, 0.8f),
                new Color(1f, 1f, 1f, 0f), 64, 64);

            _playerDot = Dot("Player", new Color(0.35f, 0.75f, 1f), 10);
            _bossDot = Dot("Boss", new Color(1f, 0.4f, 0.4f), 12);
            _exitDot = Dot("Exit", new Color(1f, 0.95f, 0.5f), 10);

            _menuButton = UIFactory.AddButton(transform, "MenuBtn", "Esc",
                new Vector2(70, 40), new Vector2(-116, -220),
                new Vector2(1, 1), new Color(0.15f, 0.15f, 0.2f, 0.85f));
            _menuButton.onClick.AddListener(() => GameManager.Instance?.TogglePause());
        }

        private RectTransform Dot(string name, Color color, int size)
        {
            var img = UIFactory.AddImage(_arena, name, color);
            var rt = (RectTransform)img.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            img.sprite = SpriteFactory.CircleSprite(color, 32);
            return rt;
        }
    }
}
