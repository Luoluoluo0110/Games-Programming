using UnityEngine;
using UnityEngine.UI;
using TowerGame.Core;

namespace TowerGame.UI
{
    /// <summary>Overlay shown on death or full tower victory.</summary>
    public class GameOverUI : MonoBehaviour
    {
        private GameObject _root;
        private Text _title;
        private Text _subtitle;

        private void Start()
        {
            Build();
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            _root.SetActive(false);
        }

        private void HandleStateChanged(GameState s)
        {
            if (s == GameState.Victory)
            {
                _title.text = "VICTORY";
                _title.color = new Color(1f, 0.95f, 0.6f);
                _subtitle.text = "The tower is yours. The amulet hums quietly.";
                _root.SetActive(true);
                _root.transform.SetAsLastSibling();
            }
            else if (s == GameState.GameOver)
            {
                _title.text = "YOU FELL";
                _title.color = new Color(1f, 0.45f, 0.45f);
                _subtitle.text = "The tower watches, patient and hungry.";
                _root.SetActive(true);
                _root.transform.SetAsLastSibling();
            }
            else
            {
                _root.SetActive(false);
            }
        }

        private void Build()
        {
            var panel = UIFactory.AddPanel(transform, "GameOver",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
                new Color(0, 0, 0, 0.8f));
            _root = panel.gameObject;

            _title = UIFactory.AddText(panel, "Title", "GAME OVER",
                84, Color.white, TextAnchor.MiddleCenter);
            _title.rectTransform.offsetMax = new Vector2(0, -220);

            _subtitle = UIFactory.AddText(panel, "Sub", "",
                24, new Color(0.85f, 0.85f, 0.95f), TextAnchor.MiddleCenter);
            _subtitle.rectTransform.offsetMax = new Vector2(0, -320);

            var restart = UIFactory.AddButton(panel, "Restart", "Return to Menu",
                new Vector2(320, 70), new Vector2(0, -40),
                new Vector2(0.5f, 0.5f), new Color(0.25f, 0.55f, 0.35f));
            restart.onClick.AddListener(() => GameManager.Instance.SetState(GameState.MainMenu));
        }
    }
}
