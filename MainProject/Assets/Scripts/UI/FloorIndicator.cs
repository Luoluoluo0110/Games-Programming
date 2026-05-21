using UnityEngine;
using UnityEngine.UI;
using TowerGame.Core;

namespace TowerGame.UI
{
    /// <summary>Top-center floor progress banner and the Boss name readout.</summary>
    public class FloorIndicator : MonoBehaviour
    {
        private Text _floorText;
        private Text _floorName;

        private void Start()
        {
            Build();
            if (TowerManager.Instance != null)
                TowerManager.Instance.OnFloorChanged += _ => Refresh();
            Refresh();
        }

        private void Build()
        {
            var panel = UIFactory.AddPanel(transform, "FloorPanel",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0.5f, 1), new Vector2(360, 64),
                new Vector2(0, -12), new Color(0, 0, 0, 0.5f));

            _floorText = UIFactory.AddText(panel, "Floor", "Floor 1 / 8",
                28, new Color(1f, 0.95f, 0.6f), TextAnchor.UpperCenter);
            _floorText.rectTransform.offsetMin = new Vector2(0, 26);

            _floorName = UIFactory.AddText(panel, "Name", "",
                16, new Color(0.9f, 0.9f, 0.95f), TextAnchor.LowerCenter);
            _floorName.rectTransform.offsetMax = new Vector2(0, -28);
        }

        private void Refresh()
        {
            var tm = TowerManager.Instance;
            if (tm == null) return;
            var cfg = tm.CurrentConfig;
            _floorText.text = $"Floor {tm.CurrentFloor} / {TowerManager.TotalFloors}";
            _floorName.text = $"{cfg.floorName} – {cfg.bossName}";
        }
    }
}
