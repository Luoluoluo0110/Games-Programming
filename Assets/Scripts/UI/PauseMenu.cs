using UnityEngine;
using TowerGame.Core;

namespace TowerGame.UI
{
    /// <summary>Semi-transparent overlay with Resume / Quit buttons, toggled by Esc.</summary>
    public class PauseMenu : MonoBehaviour
    {
        private GameObject _root;

        private void Start()
        {
            Build();
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            _root.SetActive(false);
        }

        private void HandleStateChanged(GameState s)
        {
            _root.SetActive(s == GameState.Paused);
            if (s == GameState.Paused && _root != null)
                _root.transform.SetAsLastSibling();
        }

        private void Build()
        {
            var panel = UIFactory.AddPanel(transform, "PauseMenu",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
                new Color(0, 0, 0, 0.65f));
            _root = panel.gameObject;

            UIFactory.AddText(panel, "Title", "PAUSED",
                72, new Color(1f, 0.95f, 0.6f), TextAnchor.MiddleCenter)
                .rectTransform.offsetMax = new Vector2(0, -120);

            var resume = UIFactory.AddButton(panel, "Resume", "Resume",
                new Vector2(240, 60), new Vector2(0, -20),
                new Vector2(0.5f, 0.5f), new Color(0.2f, 0.55f, 0.35f));
            resume.onClick.AddListener(() => GameManager.Instance.TogglePause());

            var quit = UIFactory.AddButton(panel, "Quit", "Back to Menu",
                new Vector2(240, 60), new Vector2(0, -100),
                new Vector2(0.5f, 0.5f), new Color(0.55f, 0.2f, 0.2f));
            quit.onClick.AddListener(() =>
            {
                GameManager.Instance.SetState(GameState.MainMenu);
            });
        }
    }
}
