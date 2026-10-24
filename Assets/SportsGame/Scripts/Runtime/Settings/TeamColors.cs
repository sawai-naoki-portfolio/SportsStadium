using UnityEngine;

namespace SportsGame
{
    /// <summary>プレイヤー側が選べるチームカラー。CPU 側(赤)と見分けやすい色だけを並べる。</summary>
    public static class TeamColors
    {
        public static readonly string[] Names = { "ブルー", "グリーン", "パープル", "イエロー" };

        static readonly Color[] Colors =
        {
            new Color(0.2f, 0.5f, 0.95f),
            new Color(0.15f, 0.7f, 0.38f),
            new Color(0.58f, 0.36f, 0.92f),
            new Color(0.95f, 0.76f, 0.12f),
        };

        public static int Count => Colors.Length;

        /// <summary>範囲外の番号は先頭(ブルー)として扱う。</summary>
        public static Color Get(int index) => index >= 0 && index < Colors.Length ? Colors[index] : Colors[0];

        /// <summary>パンツなどに使う暗めの色。</summary>
        public static Color Dark(Color c) => Color.Lerp(c, new Color(0.05f, 0.06f, 0.12f), 0.5f);
    }
}
