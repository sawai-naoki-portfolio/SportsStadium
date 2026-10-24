using UnityEngine;

namespace SportsGame
{
    /// <summary>種目ごとの表示名・シーン名・試合形式ラベルなどの静的情報。</summary>
    public static class SportCatalog
    {
        public const int MatchLengthCount = 3;

        public static readonly Sport[] All = { Sport.Basketball, Sport.Tennis, Sport.Volleyball };

        public static string SceneName(Sport s) => s.ToString();

        public static string Name(Sport s)
        {
            switch (s)
            {
                case Sport.Basketball: return "バスケットボール";
                case Sport.Tennis: return "テニス";
                default: return "バレーボール";
            }
        }

        public static string Subtitle(Sport s)
        {
            switch (s)
            {
                case Sport.Basketball: return "1on1 ハーフコート";
                case Sport.Tennis: return "シングルス";
                default: return "ビーチ 2on2";
            }
        }

        public static Color ThemeColor(Sport s)
        {
            switch (s)
            {
                case Sport.Basketball: return new Color(0.93f, 0.42f, 0.12f);
                case Sport.Tennis: return new Color(0.25f, 0.62f, 0.32f);
                default: return new Color(0.15f, 0.45f, 0.85f);
            }
        }

        public static string DifficultyLabel(Difficulty d)
        {
            switch (d)
            {
                case Difficulty.Easy: return "かんたん";
                case Difficulty.Hard: return "むずかしい";
                default: return "ふつう";
            }
        }

        public static string[] MatchLengthLabels(Sport s)
        {
            switch (s)
            {
                case Sport.Basketball: return new[] { "11点先取", "21点先取", "3分間" };
                case Sport.Tennis: return new[] { "1セット(3ゲーム)", "1セット(6ゲーム)", "3セットマッチ" };
                default: return new[] { "1セット(15点)", "1セット(21点)", "3セットマッチ" };
            }
        }

        public static string MatchLengthLabel(Sport s, int length) => MatchLengthLabels(s)[Mathf.Clamp(length, 0, MatchLengthCount - 1)];

        /// <summary>試合開始バナーの補足文(難易度・試合形式)。</summary>
        public static string IntroSubtitle(Sport s) => DifficultyLabel(GameSettings.Difficulty) + " ・ " + MatchLengthLabel(s, GameSettings.MatchLength);
    }
}
