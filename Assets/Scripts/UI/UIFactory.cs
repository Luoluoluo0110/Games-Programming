using UnityEngine;
using UnityEngine.UI;
using TowerGame.Utilities;

namespace TowerGame.UI
{
    /// <summary>
    /// Builds the entire HUD / menu hierarchy from code so no .prefab or scene wiring
    /// is required.  Other UI controllers (HUD, inventory, etc.) are attached as
    /// components and grab the references they need via <see cref="UIFactory"/>.
    /// </summary>
    public static class UIFactory
    {
        public static GameObject CreateUIRoot()
        {
            var canvasGo = new GameObject("UI_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            canvasGo.AddComponent<HUDController>();
            canvasGo.AddComponent<InventoryUI>();
            canvasGo.AddComponent<MinimapController>();
            canvasGo.AddComponent<FloorIndicator>();
            canvasGo.AddComponent<BossHealthBar>();
            canvasGo.AddComponent<PauseMenu>();
            canvasGo.AddComponent<MainMenuUI>();
            canvasGo.AddComponent<GameOverUI>();
            canvasGo.AddComponent<ToastNotice>();
            canvasGo.AddComponent<Crosshair>();

            return canvasGo;
        }

        // ---------- Helpers used by every controller ----------

        public static RectTransform AddPanel(Transform parent, string name, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            var img = go.GetComponent<Image>();
            img.sprite = SpriteFactory.SolidSprite(Color.white, 8, 8);
            img.color = color;
            return rt;
        }

        public static Text AddText(Transform parent, string name, string content, int fontSize,
            Color color, TextAnchor align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var txt = go.GetComponent<Text>();
            txt.text = content;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = align;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            // Critical: default raycastTarget on Text blocks clicks reaching Button Images underneath.
            txt.raycastTarget = false;
            return txt;
        }

        public static Image AddImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = SpriteFactory.SolidSprite(Color.white, 8, 8);
            img.color = color;
            img.raycastTarget = true;
            return img;
        }

        public static Button AddButton(Transform parent, string name, string label, Vector2 size,
            Vector2 anchoredPos, Vector2 anchor, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var btn = go.GetComponent<Button>();
            var bgImg = go.GetComponent<Image>();
            bgImg.sprite = SpriteFactory.SolidSprite(Color.white, 8, 8);
            bgImg.color = bg;
            btn.targetGraphic = bgImg;
            AddText(go.transform, "Label", label, 28, Color.white, TextAnchor.MiddleCenter);
            return btn;
        }
    }
}
