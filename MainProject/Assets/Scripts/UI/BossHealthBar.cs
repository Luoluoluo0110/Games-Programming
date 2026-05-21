using UnityEngine;
using UnityEngine.UI;
using TowerGame.Core;
using TowerGame.Enemies;
using TowerGame.Utilities;

namespace TowerGame.UI
{
    /// <summary>Thin boss HP bar sitting just under the floor indicator.</summary>
    public class BossHealthBar : MonoBehaviour
    {
        private RectTransform _panel;
        private Image _fill;
        private Text _label;
        private BossBase _boss;

        private void Start()
        {
            Build();
            if (TowerManager.Instance != null)
                TowerManager.Instance.OnFloorChanged += _ => Rebind();
        }

        private void Update()
        {
            if (_boss == null) Rebind();
            if (_boss != null && _fill != null)
                _fill.fillAmount = _boss.MaxHp > 0 ? _boss.CurrentHp / _boss.MaxHp : 0f;
            if (_panel != null) _panel.gameObject.SetActive(_boss != null);
        }

        private void Rebind()
        {
            _boss = Object.FindObjectOfType<BossBase>();
            if (_boss != null && _label != null) _label.text = _boss.displayName;
        }

        private void Build()
        {
            _panel = UIFactory.AddPanel(transform, "BossHp",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0.5f, 1), new Vector2(600, 24),
                new Vector2(0, -90), new Color(0, 0, 0, 0.6f));
            _fill = UIFactory.AddImage(_panel, "Fill", new Color(0.85f, 0.25f, 0.25f));
            var rt = (RectTransform)_fill.transform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(2, 2);
            rt.offsetMax = new Vector2(-2, -2);
            _fill.type = Image.Type.Filled;
            _fill.fillMethod = Image.FillMethod.Horizontal;
            _fill.sprite = SpriteFactory.SolidSprite(Color.white, 8, 8);

            _label = UIFactory.AddText(_panel, "Label", "Boss",
                14, Color.white, TextAnchor.MiddleCenter);
        }
    }
}
