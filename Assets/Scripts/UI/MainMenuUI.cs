using UnityEngine;
using TowerGame.Core;
using TowerGame.Player;

namespace TowerGame.UI
{
    /// <summary>Start screen shown before and between runs.</summary>
    public class MainMenuUI : MonoBehaviour
    {
        private GameObject _root;

        private void Start()
        {
            Build();
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            _root.SetActive(true);
        }

        private void HandleStateChanged(GameState s)
        {
            _root.SetActive(s == GameState.MainMenu);
            if (s == GameState.MainMenu && _root != null)
                _root.transform.SetAsLastSibling();
        }

        private void Build()
        {
            var panel = UIFactory.AddPanel(transform, "MainMenu",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.05f, 0.1f, 0.95f));
            _root = panel.gameObject;
            _root.transform.SetAsLastSibling();

            UIFactory.AddText(panel, "Title", "TOWER ASCENT",
                84, new Color(1f, 0.95f, 0.6f), TextAnchor.MiddleCenter)
                .rectTransform.offsetMax = new Vector2(0, -220);

            UIFactory.AddText(panel, "Sub", "Eight floors. Eight bosses. Left blade, right bolt — climb or fall.",
                24, new Color(0.85f, 0.85f, 0.95f), TextAnchor.MiddleCenter)
                .rectTransform.offsetMax = new Vector2(0, -320);

            UIFactory.AddText(panel, "Hint",
                "WASD: Move   Mouse: Aim   Left Click: Melee   Right Click: Ranged (costs stamina)\nF3: Potion   Space: Dash (after floor 4)   Esc: Pause",
                18, new Color(0.75f, 0.75f, 0.8f), TextAnchor.MiddleCenter)
                .rectTransform.offsetMin = new Vector2(0, 180);

            var start = UIFactory.AddButton(panel, "Start", "Enter the Tower",
                new Vector2(320, 70), new Vector2(0, -40),
                new Vector2(0.5f, 0.5f), new Color(0.25f, 0.55f, 0.35f));
            start.onClick.AddListener(() =>
            {
                var pc = PlayerController.Instance;
                if (pc != null)
                {
                    pc.Inventory.Clear();
                    pc.Stats.maxHp = 100f;
                    pc.Stats.maxStamina = 100f;
                    pc.Stats.attackBonus = 0f;
                    pc.Stats.canDash = false;
                    pc.Stats.hasFirebolt = false;
                    pc.Stats.level = 1;
                    pc.Stats.staminaRegen = 26f;
                    pc.Stats.FullHeal();
                }
                GameManager.Instance.StartNewRun();
            });

            var quit = UIFactory.AddButton(panel, "Quit", "Quit",
                new Vector2(320, 60), new Vector2(0, -120),
                new Vector2(0.5f, 0.5f), new Color(0.35f, 0.2f, 0.25f));
            quit.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }
    }
}
