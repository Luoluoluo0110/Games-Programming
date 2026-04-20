using UnityEngine;
using UnityEngine.UI;
using TowerGame.Core;
using TowerGame.Items;
using TowerGame.Player;

namespace TowerGame.UI
{
    /// <summary>Short-lived text that celebrates pickups and floor transitions.</summary>
    public class ToastNotice : MonoBehaviour
    {
        private Text _text;
        private float _hideAt;

        private void Start()
        {
            Build();
            if (TowerManager.Instance != null)
                TowerManager.Instance.OnBossDefeated += cfg =>
                    Show($"{cfg.bossName} defeated! Picked up {cfg.rewardItem?.displayName}");
            TryBindInventory();
        }

        private void Update()
        {
            if (_text == null) return;
            if (Time.time > _hideAt) _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 0f);
            else
            {
                float t = Mathf.Clamp01(_hideAt - Time.time);
                _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, Mathf.Clamp01(t / 1.5f));
            }
            if (_boundInventory == null) TryBindInventory();
        }

        private PlayerInventory _boundInventory;
        private void TryBindInventory()
        {
            var pc = PlayerController.Instance;
            if (pc == null) return;
            _boundInventory = pc.Inventory;
            _boundInventory.OnItemAdded += item => Show($"Acquired: {item.displayName}");
        }

        private void Show(string message)
        {
            _text.text = message;
            _hideAt = Time.time + 3f;
        }

        private void Build()
        {
            var panel = UIFactory.AddPanel(transform, "Toast",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0.5f, 1), new Vector2(800, 40),
                new Vector2(0, -150), new Color(0, 0, 0, 0));
            panel.GetComponent<Image>().raycastTarget = false;
            _text = UIFactory.AddText(panel, "Text", "",
                22, new Color(1f, 0.95f, 0.6f, 0f), TextAnchor.MiddleCenter);
        }
    }
}
