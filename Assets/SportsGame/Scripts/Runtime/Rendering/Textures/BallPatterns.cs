using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// 球面上の方向から各競技ボールの柄の色を求める。3D ボールのテクスチャとメニューアイコンで共通に使う。
    /// </summary>
    public static class BallPatterns
    {
        static readonly Color BasketballOrange = new Color(0.9f, 0.42f, 0.13f);
        static readonly Color BasketballLine = new Color(0.12f, 0.06f, 0.04f);
        static readonly Color TennisFelt = new Color(0.83f, 0.94f, 0.19f);
        static readonly Color TennisSeam = new Color(0.97f, 0.97f, 0.92f);
        static readonly Color[] VolleyPanels = { new Color(0.98f, 0.85f, 0.15f), new Color(0.13f, 0.37f, 0.84f), new Color(0.97f, 0.97f, 0.97f) };

        static Vector3[] tennisSeam;

        /// <summary>lineScale: 線の太さの倍率(小さなアイコンでは太くする)。</summary>
        public static Color Sample(Sport sport, Vector3 dir, float lineScale = 1f)
        {
            switch (sport)
            {
                case Sport.Basketball: return Basketball(dir, lineScale);
                case Sport.Tennis: return Tennis(dir, lineScale);
                default: return Volleyball(dir, lineScale);
            }
        }

        static Color Basketball(Vector3 d, float lineScale)
        {
            float w = 0.022f * lineScale;
            float line = Mathf.Min(Mathf.Min(Mathf.Abs(d.y), Mathf.Abs(d.x)), Mathf.Abs(Mathf.Abs(d.z) - 0.72f));
            float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(w, w * 0.4f, line));
            return Color.Lerp(BasketballOrange, BasketballLine, k);
        }

        static Color Tennis(Vector3 d, float lineScale)
        {
            if (tennisSeam == null) tennisSeam = BuildTennisSeam(160);
            float best = 1f;
            foreach (var p in tennisSeam) best = Mathf.Min(best, 1f - Vector3.Dot(d, p));
            float w = 0.0016f * lineScale * lineScale;
            return Color.Lerp(TennisFelt, TennisSeam, Mathf.InverseLerp(w, w * 0.35f, best));
        }

        /// <summary>テニスボールの縫い目(野球ボール型の閉曲線)。</summary>
        static Vector3[] BuildTennisSeam(int count)
        {
            var seam = new Vector3[count];
            const float a = 0.72f, b = 0.28f;
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count * Mathf.PI * 2f;
                seam[i] = new Vector3(a * Mathf.Cos(t) + b * Mathf.Cos(3f * t), a * Mathf.Sin(t) - b * Mathf.Sin(3f * t), 2f * Mathf.Sqrt(a * b) * Mathf.Sin(2f * t)).normalized;
            }
            return seam;
        }

        static Color Volleyball(Vector3 d, float lineScale)
        {
            Vector3 ad = new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z));
            int axis = ad.x > ad.y ? (ad.x > ad.z ? 0 : 2) : (ad.y > ad.z ? 1 : 2);
            float main = axis == 0 ? d.x : axis == 1 ? d.y : d.z;
            float second = axis == 0 ? d.y : axis == 1 ? d.z : d.x;
            float stripe = (second / Mathf.Abs(main) + 1f) * 1.5f;
            Color c = VolleyPanels[(Mathf.Clamp(Mathf.FloorToInt(stripe), 0, 2) + axis) % 3];
            float edge = Mathf.Abs(stripe - Mathf.Round(stripe)) * Mathf.Abs(main);
            float faceEdge = Mathf.Min(Mathf.Abs(ad.x - ad.y), Mathf.Min(Mathf.Abs(ad.y - ad.z), Mathf.Abs(ad.x - ad.z)));
            float seam = Mathf.Max(Mathf.InverseLerp(0.025f * lineScale, 0.01f * lineScale, edge), Mathf.InverseLerp(0.02f * lineScale, 0.008f * lineScale, faceEdge));
            return Color.Lerp(c, c * 0.55f, seam);
        }
    }
}
