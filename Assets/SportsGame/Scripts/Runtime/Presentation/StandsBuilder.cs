using UnityEngine;

namespace SportsGame
{
    /// <summary>ひな壇の観客席と観客を作る。観客はマテリアルごとに結合してドローコールを抑える。</summary>
    public static class StandsBuilder
    {
        const float RowDepth = 0.85f;
        const float RowRise = 0.45f;
        const float SeatSpacing = 0.62f;

        static readonly Color[] ShirtColors =
        {
            new Color(0.85f, 0.2f, 0.2f), new Color(0.2f, 0.4f, 0.85f), new Color(0.95f, 0.85f, 0.25f),
            new Color(0.95f, 0.95f, 0.95f), new Color(0.2f, 0.7f, 0.35f), new Color(0.9f, 0.5f, 0.15f),
            new Color(0.15f, 0.15f, 0.18f), new Color(0.6f, 0.3f, 0.7f)
        };

        static readonly Color[] SkinColors = { new Color(0.95f, 0.8f, 0.65f), new Color(0.7f, 0.5f, 0.35f), new Color(0.45f, 0.3f, 0.2f) };

        /// <summary>origin は最前列の中央、facing は観客が向く方向。戻り値は観客(上下させて歓声を表現する)。</summary>
        public static Transform Build(Transform parent, Vector3 origin, Vector3 facing, float width, int rows, Color seatColor, int seed)
        {
            var root = Geo.Empty("Stands", parent).transform;
            root.position = origin;
            root.rotation = Quaternion.LookRotation(facing, Vector3.up);

            var structure = Geo.Empty("Structure", root).transform;
            Material seat = Mats.Lit(seatColor, 0.2f);
            Material step = Mats.Lit(Color.Lerp(seatColor, Color.gray, 0.6f), 0.1f);
            for (int r = 0; r < rows; r++)
                Geo.Box(structure, new Vector3(0f, r * RowRise + RowRise * 0.5f, -r * RowDepth - RowDepth * 0.5f), new Vector3(width, RowRise, RowDepth), r % 2 == 0 ? step : seat, "Row", false);
            Geo.Box(structure, new Vector3(0f, rows * RowRise * 0.5f + 0.6f, -rows * RowDepth - 0.2f), new Vector3(width, rows * RowRise + 1.2f, 0.4f), step, "Back", false);
            Geo.CombineByMaterial(structure);

            var crowd = Geo.Empty("Crowd", root).transform;
            var rng = new System.Random(seed);
            int perRow = Mathf.FloorToInt(width / SeatSpacing);
            double density = GameSettings.HighQuality ? 0.8 : 0.45;
            for (int r = 0; r < rows; r++)
            {
                for (int i = 0; i < perRow; i++)
                {
                    if (rng.NextDouble() > density) continue;
                    float x = -width * 0.5f + SeatSpacing * (i + 0.5f) + (float)(rng.NextDouble() - 0.5) * 0.15f;
                    float z = -r * RowDepth - RowDepth * 0.45f;
                    float y = (r + 1) * RowRise;
                    Geo.Box(crowd, new Vector3(x, y + 0.33f, z), new Vector3(0.42f, 0.62f, 0.3f), Mats.Lit(ShirtColors[rng.Next(ShirtColors.Length)], 0.15f), "P", false);
                    Geo.Box(crowd, new Vector3(x, y + 0.78f, z), new Vector3(0.24f, 0.26f, 0.24f), Mats.Lit(SkinColors[rng.Next(SkinColors.Length)], 0.3f), "H", false);
                }
            }
            Geo.CombineByMaterial(crowd);
            return crowd;
        }
    }
}
