namespace SportsGame
{
    public enum Sport { Basketball, Tennis, Volleyball }

    public enum Difficulty { Easy, Normal, Hard }

    public static class DifficultyExtensions
    {
        /// <summary>難易度を 0 (かんたん) 〜 1 (むずかしい) の値にする。CPU パラメータの補間に使う。</summary>
        public static float Skill(this Difficulty d)
        {
            switch (d)
            {
                case Difficulty.Easy: return 0f;
                case Difficulty.Hard: return 1f;
                default: return 0.5f;
            }
        }
    }
}
