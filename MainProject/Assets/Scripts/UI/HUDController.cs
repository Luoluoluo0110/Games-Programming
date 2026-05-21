using UnityEngine;
using UnityEngine.UI;
using TowerGame.Player;
using TowerGame.Utilities;

namespace TowerGame.UI
{
    /// <summary>Top-left panel: circular portrait, HP bar, stamina bar, level readout.</summary>
    public class HUDController : MonoBehaviour
    {
        private Image _hpFill;
        private Image _staFill;
        private Text _hpText;
        private Text _staText;
        private Text _levelText;
        private PlayerStats _stats;

        private void Start()
        {
            Build();
            TryBindPlayer();
        }

        private void Update()
        {
            if (_stats == null) TryBindPlayer();
        }

        private void TryBindPlayer()
        {
            var pc = PlayerController.Instance;
            if (pc == null) return;
            _stats = pc.Stats;
            _stats.OnChanged += Refresh;
            Refresh();
        }

        private void Build()
        {
            var panel = UIFactory.AddPanel(transform, "HUD", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1), new Vector2(360, 140), new Vector2(16, -16),
                new Color(0, 0, 0, 0.45f));

            var portrait = UIFactory.AddImage(panel, "Portrait", new Color(0.35f, 0.75f, 1f));
            var pr = (RectTransform)portrait.transform;
            pr.anchorMin = new Vector2(0, 0.5f);
            pr.anchorMax = new Vector2(0, 0.5f);
            pr.pivot = new Vector2(0, 0.5f);
            pr.sizeDelta = new Vector2(96, 96);
            pr.anchoredPosition = new Vector2(16, 0);
            portrait.sprite = SpriteFactory.CircleSprite(new Color(0.35f, 0.75f, 1f), 128);

            _hpFill = BuildBar(panel, new Vector2(128, -20), new Vector2(210, 22),
                new Color(0.9f, 0.2f, 0.2f));
            _hpText = UIFactory.AddText(_hpFill.transform.parent, "HpText", "100 / 100",
                18, Color.white, TextAnchor.MiddleCenter);

            _staFill = BuildBar(panel, new Vector2(128, -54), new Vector2(210, 18),
                new Color(0.95f, 0.85f, 0.2f));
            _staText = UIFactory.AddText(_staFill.transform.parent, "StaText", "100 / 100",
                14, Color.white, TextAnchor.MiddleCenter);

            _levelText = UIFactory.AddText(panel, "Level", "Lv. 1",
                16, new Color(0.9f, 0.9f, 1f), TextAnchor.LowerLeft);
            var lrt = (RectTransform)_levelText.transform;
            lrt.anchorMin = new Vector2(0, 0);
            lrt.anchorMax = new Vector2(0, 0);
            lrt.pivot = new Vector2(0, 0);
            lrt.offsetMin = new Vector2(128, 10);
            lrt.offsetMax = new Vector2(300, 30);
        }

        private Image BuildBar(RectTransform parent, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var bg = UIFactory.AddImage(parent, "BarBg", new Color(0, 0, 0, 0.5f));
            var bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = new Vector2(0, 1);
            bgRt.anchorMax = new Vector2(0, 1);
            bgRt.pivot = new Vector2(0, 1);
            bgRt.sizeDelta = size;
            bgRt.anchoredPosition = anchoredPos;

            var fill = UIFactory.AddImage(bg.transform, "BarFill", color);
            var frt = (RectTransform)fill.transform;
            frt.anchorMin = new Vector2(0, 0);
            frt.anchorMax = new Vector2(1, 1);
            frt.offsetMin = new Vector2(2, 2);
            frt.offsetMax = new Vector2(-2, -2);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.sprite = SpriteFactory.SolidSprite(Color.white, 8, 8);
            return fill;
        }

        private void Refresh()
        {
            if (_stats == null) return;
            _hpFill.fillAmount = _stats.maxHp > 0 ? _stats.hp / _stats.maxHp : 0f;
            _staFill.fillAmount = _stats.maxStamina > 0 ? _stats.stamina / _stats.maxStamina : 0f;
            _hpText.text = $"{Mathf.CeilToInt(_stats.hp)} / {Mathf.CeilToInt(_stats.maxHp)}";
            _staText.text = $"{Mathf.CeilToInt(_stats.stamina)} / {Mathf.CeilToInt(_stats.maxStamina)}";
            _levelText.text = $"Lv. {_stats.level}";
        }
    }
}
