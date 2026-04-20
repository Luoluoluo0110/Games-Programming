using UnityEngine;
using UnityEngine.UI;
using TowerGame.Core;
using TowerGame.Player;
using TowerGame.Utilities;

namespace TowerGame.UI
{
    /// <summary>Draws a small ring at the mouse position and a line from the player to it.</summary>
    public class Crosshair : MonoBehaviour
    {
        private RectTransform _ring;
        private RectTransform _aimDot;
        private Canvas _canvas;

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            Build();
        }

        private void Update()
        {
            if (_ring == null) return;

            var gm = GameManager.Instance;
            bool show = gm != null && gm.State == GameState.Playing;
            _ring.gameObject.SetActive(show);
            if (_aimDot != null) _aimDot.gameObject.SetActive(show);
            if (!show) return;

            Vector2 mousePos = Input.mousePosition;

            if (_canvas != null)
            {
                Vector2 canvasPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)_canvas.transform, mousePos, null, out canvasPos);
                _ring.anchoredPosition = canvasPos;
            }
            else
            {
                _ring.position = mousePos;
            }

            var pc = PlayerController.Instance;
            var cam = Camera.main;
            if (pc != null && cam != null && _aimDot != null && _canvas != null)
            {
                Vector3 forward = pc.transform.position + (Vector3)pc.AimDirection * 0.9f;
                Vector3 screen = cam.WorldToScreenPoint(forward);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)_canvas.transform, screen, null, out Vector2 canvasPos);
                _aimDot.anchoredPosition = canvasPos;
            }
        }

        private void Build()
        {
            var ringImg = UIFactory.AddImage(transform, "CrosshairRing", new Color(1f, 0.95f, 0.6f, 0.9f));
            ringImg.sprite = SpriteFactory.RingSprite(new Color(1f, 0.95f, 0.6f, 0.9f),
                new Color(1f, 1f, 1f, 0f), 48, 48);
            ringImg.raycastTarget = false;
            _ring = (RectTransform)ringImg.transform;
            _ring.sizeDelta = new Vector2(28, 28);
            _ring.anchorMin = _ring.anchorMax = _ring.pivot = new Vector2(0.5f, 0.5f);

            var dotImg = UIFactory.AddImage(transform, "AimDot", new Color(1f, 0.95f, 0.6f, 0.8f));
            dotImg.sprite = SpriteFactory.CircleSprite(new Color(1f, 0.95f, 0.6f, 0.8f), 16);
            dotImg.raycastTarget = false;
            _aimDot = (RectTransform)dotImg.transform;
            _aimDot.sizeDelta = new Vector2(10, 10);
            _aimDot.anchorMin = _aimDot.anchorMax = _aimDot.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
