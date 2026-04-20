using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TowerGame.Core;
using TowerGame.Items;
using TowerGame.Player;
using TowerGame.Utilities;

namespace TowerGame.UI
{
    /// <summary>
    /// Bottom-left inventory grid + control hints: LMB melee, RMB ranged, F3 potion.
    /// Hot slots pulse green while the corresponding mouse button is held in-game.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        private PlayerInventory _inv;
        private PlayerCombat _combat;
        private RectTransform _gridParent;
        private readonly List<Image> _slotImages = new List<Image>();
        private readonly List<Text> _slotTexts = new List<Text>();

        private Image _lmbBg;
        private Image _rmbBg;
        private Image _f3Bg;
        private Image _f3Icon;
        private Text _f3Count;
        private Image _f3CooldownOverlay;

        private bool _eventsBound;

        private Color _hotBgIdle = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        private Color _hotBgActive = new Color(0.2f, 0.55f, 0.35f, 0.95f);
        private Color _hotBgDisabled = new Color(0.08f, 0.08f, 0.1f, 0.9f);

        private void Start()
        {
            Build();
            TryBind();
        }

        private void Update()
        {
            if (_inv == null || _combat == null) TryBind();
            UpdateHotkeys();
        }

        private void TryBind()
        {
            if (_eventsBound) return;
            var pc = PlayerController.Instance;
            if (pc == null) return;
            _inv = pc.Inventory;
            _combat = pc.Combat;
            _inv.OnInventoryChanged += Refresh;
            _eventsBound = true;
            Refresh();
            UpdateHotkeys();
        }

        private void Build()
        {
            var panel = UIFactory.AddPanel(transform, "Inventory",
                new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(0, 0), new Vector2(500, 220), new Vector2(16, 16),
                new Color(0, 0, 0, 0.45f));

            UIFactory.AddText(panel, "Title", "Inventory",
                16, new Color(0.9f, 0.9f, 1f), TextAnchor.UpperLeft)
                .rectTransform.offsetMin = new Vector2(12, 12);

            _gridParent = UIFactory.AddPanel(panel, "Grid",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 1f), new Vector2(-24, -110),
                new Vector2(0, -32), new Color(0, 0, 0, 0));
            var grid = _gridParent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(44, 44);
            grid.spacing = new Vector2(6, 6);
            grid.padding = new RectOffset(6, 6, 6, 6);
            grid.childAlignment = TextAnchor.UpperLeft;

            var hotRow = UIFactory.AddPanel(panel, "Hotkeys",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0.5f, 0), new Vector2(-20, 68),
                new Vector2(0, 12), new Color(0, 0, 0, 0));

            _lmbBg = BuildHotSlot(hotRow, 0, "LMB", "Melee", new Color(0.85f, 0.85f, 0.9f), out _, out _, out _);
            _rmbBg = BuildHotSlot(hotRow, 1, "RMB", "Ranged", new Color(0.55f, 0.75f, 1f), out _, out _, out _);
            _f3Bg = BuildHotSlot(hotRow, 2, "F3", "Potion", new Color(0.35f, 0.85f, 0.45f),
                out _f3Icon, out _f3Count, out _f3CooldownOverlay);
        }

        private Image BuildHotSlot(RectTransform parent, int index, string key, string label, Color iconColor,
            out Image iconOut, out Text countOut, out Image cooldownOut)
        {
            var slot = UIFactory.AddPanel(parent, $"Slot_{key}",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(0, 0.5f), new Vector2(88, 80),
                new Vector2(18 + index * 100, 0), _hotBgIdle);

            var icon = UIFactory.AddImage(slot, "Icon", iconColor);
            var irt = (RectTransform)icon.transform;
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(10, 24);
            irt.offsetMax = new Vector2(-10, -10);
            icon.sprite = SpriteFactory.SquareSprite(iconColor, 48, 48);
            iconOut = icon;

            var cooldown = UIFactory.AddImage(slot, "Cooldown", new Color(0, 0, 0, 0.6f));
            var crt = (RectTransform)cooldown.transform;
            crt.anchorMin = Vector2.zero;
            crt.anchorMax = Vector2.one;
            crt.offsetMin = Vector2.zero;
            crt.offsetMax = Vector2.zero;
            cooldown.raycastTarget = false;
            cooldown.type = Image.Type.Filled;
            cooldown.fillMethod = Image.FillMethod.Vertical;
            cooldown.fillOrigin = (int)Image.OriginVertical.Top;
            cooldown.fillAmount = 0f;
            cooldownOut = cooldown;

            var keyLabel = UIFactory.AddText(slot, "Key", key,
                16, new Color(1f, 0.95f, 0.6f), TextAnchor.UpperLeft);
            keyLabel.rectTransform.offsetMin = new Vector2(6, 0);

            var nameLabel = UIFactory.AddText(slot, "Name", label,
                11, new Color(0.85f, 0.85f, 0.9f), TextAnchor.LowerCenter);
            nameLabel.rectTransform.offsetMax = new Vector2(0, -2);

            var count = UIFactory.AddText(slot, "Count", "",
                16, new Color(1f, 0.95f, 0.6f), TextAnchor.UpperRight);
            count.rectTransform.offsetMax = new Vector2(-4, 0);
            countOut = count;

            return slot.GetComponent<Image>();
        }

        private void Refresh()
        {
            if (_inv == null) return;

            int desiredSlots = Mathf.Max(_inv.Items.Count, 6);
            while (_slotImages.Count < desiredSlots) CreateEmptySlot();

            for (int i = 0; i < _slotImages.Count; i++)
            {
                if (i < _inv.Items.Count)
                {
                    var item = _inv.Items[i];
                    _slotImages[i].color = item.spriteColor;
                    int count = _inv.GetCount(item);
                    _slotTexts[i].text = count > 1 ? count.ToString() : "";
                }
                else
                {
                    _slotImages[i].color = new Color(0, 0, 0, 0.3f);
                    _slotTexts[i].text = "";
                }
            }

            UpdateHotkeys();
        }

        private void UpdateHotkeys()
        {
            if (_combat == null || _lmbBg == null) return;

            var gm = GameManager.Instance;
            bool playing = gm != null && gm.State == GameState.Playing;

            _lmbBg.color = playing && Input.GetMouseButton(0) ? _hotBgActive : _hotBgIdle;
            _rmbBg.color = playing && Input.GetMouseButton(1) ? _hotBgActive : _hotBgIdle;

            int count = _inv != null ? _inv.StaminaPotionCount : 0;
            if (count > 0)
            {
                _f3Bg.color = _hotBgIdle;
                _f3Count.text = count.ToString();
                _f3Icon.color = new Color(0.35f, 0.85f, 0.45f);
            }
            else
            {
                _f3Bg.color = _hotBgDisabled;
                _f3Count.text = "";
                _f3Icon.color = new Color(0.2f, 0.3f, 0.2f, 0.6f);
            }

            if (_inv != null && _f3CooldownOverlay != null)
            {
                float remain = _inv.PotionCooldownRemaining;
                _f3CooldownOverlay.fillAmount = remain > 0f
                    ? Mathf.Clamp01(remain / Mathf.Max(0.01f, _inv.potionCooldown))
                    : 0f;
            }
        }

        private void CreateEmptySlot()
        {
            var slot = new GameObject("Slot", typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(_gridParent, false);
            var img = slot.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.3f);
            img.sprite = SpriteFactory.SolidSprite(Color.white, 8, 8);
            _slotImages.Add(img);

            var txt = UIFactory.AddText(slot.transform, "Count", "",
                14, new Color(1f, 0.95f, 0.6f), TextAnchor.LowerRight);
            _slotTexts.Add(txt);
        }
    }
}
