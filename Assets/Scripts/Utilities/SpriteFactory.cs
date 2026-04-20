using System.Collections.Generic;
using UnityEngine;

namespace TowerGame.Utilities
{
    /// <summary>
    /// Generates placeholder <see cref="Sprite"/>s at runtime so the project works
    /// out of the box without any imported art assets.  All results are cached and
    /// reused between callers.
    /// </summary>
    public static class SpriteFactory
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        private const float PixelsPerUnit = 32f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { _cache.Clear(); }

        private static Sprite Cache(string key, System.Func<Sprite> make)
        {
            if (!_cache.TryGetValue(key, out var s) || s == null)
            {
                s = make();
                _cache[key] = s;
            }
            return s;
        }

        private static Sprite FromTexture(Texture2D tex)
        {
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), PixelsPerUnit);
        }

        public static Sprite SolidSprite(Color color, int w, int h)
        {
            string key = $"solid|{w}x{h}|{color}";
            return Cache(key, () =>
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                var pixels = new Color[w * h];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
                tex.SetPixels(pixels);
                return FromTexture(tex);
            });
        }

        public static Sprite SquareSprite(Color fill, int w, int h, int border = 2)
        {
            string key = $"square|{w}x{h}|{fill}|b{border}";
            return Cache(key, () =>
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Color edge = fill * 0.55f; edge.a = 1f;
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool isEdge = x < border || y < border || x >= w - border || y >= h - border;
                    tex.SetPixel(x, y, isEdge ? edge : fill);
                }
                return FromTexture(tex);
            });
        }

        public static Sprite CircleSprite(Color fill, int size)
        {
            string key = $"circle|{size}|{fill}";
            return Cache(key, () =>
            {
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                float radius = size * 0.5f;
                Color edge = fill * 0.55f; edge.a = 1f;
                Color clear = new Color(0, 0, 0, 0);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius + 0.5f;
                    float dy = y - radius + 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > radius) tex.SetPixel(x, y, clear);
                    else if (d > radius - 2f) tex.SetPixel(x, y, edge);
                    else tex.SetPixel(x, y, fill);
                }
                return FromTexture(tex);
            });
        }

        public static Sprite RingSprite(Color outer, Color inner, int w, int h)
        {
            string key = $"ring|{w}x{h}|{outer}|{inner}";
            return Cache(key, () =>
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                float r = Mathf.Min(w, h) * 0.5f;
                Color clear = new Color(0, 0, 0, 0);
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = x - w * 0.5f + 0.5f;
                    float dy = y - h * 0.5f + 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > r) tex.SetPixel(x, y, clear);
                    else if (d > r - 4f) tex.SetPixel(x, y, outer);
                    else tex.SetPixel(x, y, inner);
                }
                return FromTexture(tex);
            });
        }

        public static Sprite CheckerSprite(Color a, Color b, int w, int h, int cell)
        {
            string key = $"checker|{w}x{h}|{a}|{b}|{cell}";
            return Cache(key, () =>
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    bool on = ((x / cell) + (y / cell)) % 2 == 0;
                    tex.SetPixel(x, y, on ? a : b);
                }
                return FromTexture(tex);
            });
        }

        public static Sprite BarSprite(Color color)
        {
            return SolidSprite(color, 8, 8);
        }
    }
}
