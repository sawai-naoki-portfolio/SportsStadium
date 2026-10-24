using System.Collections.Generic;
using UnityEngine;

namespace SportsGame
{
    /// <summary>手続き的に生成する3D用テクスチャ(ボール柄・床・ネット・影)。生成結果はキャッシュする。</summary>
    public static class ProcTextures
    {
        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        static Texture2D Cached(string key, int w, int h, bool alpha, TextureWrapMode wrap, System.Func<int, int, Color> pixel)
        {
            if (cache.TryGetValue(key, out var tex) && tex != null) return tex;
            tex = new Texture2D(w, h, alpha ? TextureFormat.RGBA32 : TextureFormat.RGB24, true)
            {
                name = key,
                wrapMode = wrap,
                filterMode = FilterMode.Bilinear,
                anisoLevel = 4
            };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = pixel(x, y);
            tex.SetPixels32(px);
            tex.Apply(true, true);
            cache[key] = tex;
            return tex;
        }

        public static Texture2D Ball(Sport sport)
        {
            int w = sport == Sport.Tennis ? 256 : 512, h = w / 2;
            float grainScale = sport == Sport.Tennis ? 0.6f : 0.35f;
            float grain = sport == Sport.Volleyball ? 0f : sport == Sport.Tennis ? 0.1f : 0.12f;
            return Cached("ball_" + sport, w, h, false, TextureWrapMode.Repeat, (x, y) =>
            {
                Color c = BallPatterns.Sample(sport, MeshFactory.SphereDirection((x + 0.5f) / w, (y + 0.5f) / h));
                c *= 1f + (Mathf.PerlinNoise(x * grainScale, y * grainScale) - 0.5f) * grain;
                c.a = 1f;
                return c;
            });
        }

        /// <summary>木製フローリング(Z方向に板目)。</summary>
        public static Texture2D WoodFloor()
        {
            const int S = 512, Planks = 8, PlankLen = 170;
            Color baseA = new Color(0.80f, 0.60f, 0.38f), baseB = new Color(0.86f, 0.67f, 0.44f);
            return Cached("wood", S, S, false, TextureWrapMode.Repeat, (x, y) =>
            {
                int plank = x * Planks / S;
                int along = y + plank * 97;
                Color c = Color.Lerp(baseA, baseB, Mathf.PerlinNoise(plank * 3.7f, Mathf.Floor(along / (float)PlankLen) * 5.3f));
                c *= 1f + Mathf.PerlinNoise(x * 0.08f + plank * 13f, y * 0.006f) * 0.14f - 0.07f;
                if (x % (S / Planks) < 2) c *= 0.82f;
                if (along % PlankLen < 2) c *= 0.85f;
                c.a = 1f;
                return c;
            });
        }

        /// <summary>タイル可能な汎用ノイズ(砂・ハードコート・芝など)。</summary>
        public static Texture2D Noise(string key, Color a, Color b, float scale, float grain = 0.08f)
        {
            const int S = 256;
            var rng = new System.Random(key.GetHashCode());
            return Cached("noise_" + key, S, S, false, TextureWrapMode.Repeat, (x, y) =>
            {
                float u = x / (float)S, v = y / (float)S;
                float n = TileNoise(u, v, scale) * 0.7f + TileNoise(u, v, scale * 4f) * 0.3f;
                Color c = Color.Lerp(a, b, n) * (1f + ((float)rng.NextDouble() - 0.5f) * grain);
                c.a = 1f;
                return c;
            });
        }

        static float TileNoise(float u, float v, float scale)
        {
            float n00 = Mathf.PerlinNoise(u * scale, v * scale);
            float n10 = Mathf.PerlinNoise((u - 1f) * scale, v * scale);
            float n01 = Mathf.PerlinNoise(u * scale, (v - 1f) * scale);
            float n11 = Mathf.PerlinNoise((u - 1f) * scale, (v - 1f) * scale);
            return Mathf.Lerp(Mathf.Lerp(n00, n10, u), Mathf.Lerp(n01, n11, u), v);
        }

        /// <summary>ネット・ガットの格子(アルファ付き)。</summary>
        public static Texture2D Grid(Color line, int size, int thickness)
        {
            return Cached($"grid_{ColorUtility.ToHtmlStringRGBA(line)}_{size}_{thickness}", size, size, true, TextureWrapMode.Repeat,
                (x, y) => x < thickness || y < thickness ? line : new Color(0f, 0f, 0f, 0f));
        }

        /// <summary>バスケットネット用の菱形パターン。</summary>
        public static Texture2D DiamondNet()
        {
            const int S = 64;
            return Cached("diamond", S, S, true, TextureWrapMode.Repeat, (x, y) =>
                (x + y) % 32 < 4 || (x - y + S) % 32 < 4 ? new Color(0.98f, 0.98f, 0.98f, 1f) : new Color(0f, 0f, 0f, 0f));
        }

        /// <summary>中心が濃い円形グラデーション(ボールの影)。</summary>
        public static Texture2D RadialSoft()
        {
            const int S = 64;
            return Cached("radial", S, S, true, TextureWrapMode.Clamp, (x, y) =>
            {
                float dx = (x + 0.5f) / S * 2f - 1f, dy = (y + 0.5f) / S * 2f - 1f;
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                return new Color(1f, 1f, 1f, a * a * (3f - 2f * a));
            });
        }
    }
}
