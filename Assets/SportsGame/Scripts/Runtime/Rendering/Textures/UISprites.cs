using System.Collections.Generic;
using UnityEngine;

namespace SportsGame
{
    /// <summary>UI 用スプライト(角丸・円・競技アイコン)の生成とキャッシュ。</summary>
    public static class UISprites
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        static Sprite Cached(string key, int size, Vector4 border, bool mips, System.Func<int, int, Color> pixel)
        {
            if (cache.TryGetValue(key, out var sp) && sp != null) return sp;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, mips)
            {
                name = key,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = mips ? FilterMode.Trilinear : FilterMode.Bilinear
            };
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    px[y * size + x] = pixel(x, y);
            tex.SetPixels32(px);
            tex.Apply(mips, true);
            sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            sp.name = key;
            cache[key] = sp;
            return sp;
        }

        /// <summary>9スライス可能な角丸矩形。</summary>
        public static Sprite RoundedRect(int radius = 28)
        {
            int s = radius * 2 + 4;
            return Cached("rr" + radius, s, new Vector4(radius + 1, radius + 1, radius + 1, radius + 1), false, (x, y) =>
            {
                float cx = Mathf.Clamp(x + 0.5f, radius, s - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, s - radius);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                return new Color(1f, 1f, 1f, Mathf.Clamp01(radius - d + 0.5f));
            });
        }

        /// <summary>円。ringWidth &gt; 0 で輪っか。</summary>
        public static Sprite Circle(int size = 128, float ringWidth = 0f)
        {
            float r = size * 0.5f - 1f;
            return Cached($"circle{size}_{ringWidth}", size, Vector4.zero, false, (x, y) =>
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size * 0.5f, size * 0.5f));
                float a = Mathf.Clamp01(r - d + 0.5f);
                if (ringWidth > 0f) a *= Mathf.Clamp01(d - (r - ringWidth) + 0.5f);
                return new Color(1f, 1f, 1f, a);
            });
        }

        /// <summary>陰影付きのボールアイコン(メニュー用)。</summary>
        public static Sprite SportIcon(Sport sport)
        {
            const int S = 256;
            float r = S * 0.5f - 4f;
            Vector3 light = new Vector3(-0.4f, 0.5f, 0.75f).normalized;
            Quaternion tilt = Quaternion.Euler(20f, -25f, 15f);
            return Cached("icon" + sport, S, Vector4.zero, true, (x, y) =>
            {
                float dx = (x + 0.5f - S * 0.5f) / r, dy = (y + 0.5f - S * 0.5f) / r;
                float r2 = dx * dx + dy * dy;
                if (r2 > 1.02f) return new Color(0f, 0f, 0f, 0f);
                Vector3 n = new Vector3(dx, dy, Mathf.Sqrt(Mathf.Max(0f, 1f - r2)));
                Color c = BallPatterns.Sample(sport, tilt * n, 2f);
                float shade = 0.45f + 0.65f * Mathf.Max(0f, Vector3.Dot(n, light));
                float spec = Mathf.Pow(Mathf.Max(0f, Vector3.Dot(Vector3.Reflect(-light, n), Vector3.forward)), 24f) * 0.35f;
                c = c * shade + Color.white * spec;
                c.a = Mathf.Clamp01((1f - Mathf.Sqrt(r2)) * r);
                return c;
            });
        }
    }
}
