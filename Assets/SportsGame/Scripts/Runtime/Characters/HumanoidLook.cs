using UnityEngine;

namespace SportsGame
{
    /// <summary>キャラクターの外見。</summary>
    public struct HumanoidLook
    {
        public Color skin, hair, jersey, trim, shorts, shoes, accent;
        public float height;
        public bool racket, longHair, headband;

        static readonly Color[] Skins =
        {
            new Color(0.96f, 0.80f, 0.66f), new Color(0.87f, 0.68f, 0.52f), new Color(0.65f, 0.46f, 0.32f),
            new Color(0.45f, 0.30f, 0.20f), new Color(0.98f, 0.86f, 0.74f)
        };

        static readonly Color[] Hairs =
        {
            new Color(0.08f, 0.06f, 0.05f), new Color(0.25f, 0.15f, 0.08f), new Color(0.55f, 0.38f, 0.18f),
            new Color(0.85f, 0.7f, 0.4f), new Color(0.12f, 0.1f, 0.12f)
        };

        /// <summary>ユニフォーム色と乱数シードから外見を作る(同じシードなら同じ見た目)。</summary>
        public static HumanoidLook Make(Color jersey, Color shorts, int seed, float height = 1f)
        {
            var r = new System.Random(seed);
            return new HumanoidLook
            {
                skin = Skins[r.Next(Skins.Length)],
                hair = Hairs[r.Next(Hairs.Length)],
                jersey = jersey,
                trim = Color.Lerp(jersey, Color.white, 0.75f),
                shorts = shorts,
                shoes = Color.Lerp(Color.white, jersey, 0.25f),
                accent = jersey,
                height = height,
                longHair = r.Next(3) == 0,
                headband = r.Next(2) == 0,
            };
        }
    }
}
