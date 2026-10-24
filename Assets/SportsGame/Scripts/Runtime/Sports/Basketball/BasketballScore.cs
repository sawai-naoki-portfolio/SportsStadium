namespace SportsGame
{
    /// <summary>
    /// バスケットボールの得点と勝敗(先取点数 または 時間制)。時間切れで同点なら次のゴールで決着(サドンデス)。
    /// Unity に依存しない純粋なロジック。
    /// </summary>
    public class BasketballScore
    {
        readonly int[] points = new int[2];

        /// <summary>先取点数(0 なら時間制のみ)。</summary>
        public int TargetScore { get; }
        /// <summary>試合時間(秒、0 なら時間無制限)。</summary>
        public float TimeLimit { get; }
        public float TimeLeft { get; private set; }
        public bool SuddenDeath { get; private set; }
        public int Winner { get; private set; } = -1;

        public BasketballScore(int targetScore, float timeLimit)
        {
            TargetScore = System.Math.Max(0, targetScore);
            TimeLimit = System.Math.Max(0f, timeLimit);
            TimeLeft = TimeLimit;
        }

        public int Points(int team) => points[team];

        public bool Timed => TimeLimit > 0f;

        /// <summary>得点を加算し、試合が決着したら true。</summary>
        public bool Add(int team, int value)
        {
            if (Winner >= 0 || (team != 0 && team != 1) || value <= 0) return false;
            points[team] += value;
            if (SuddenDeath || (TargetScore > 0 && points[team] >= TargetScore)) Winner = team;
            return Winner >= 0;
        }

        /// <summary>試合時間を進め、時間切れで決着したら true。同点ならサドンデスに入る。</summary>
        public bool Tick(float dt)
        {
            if (!Timed || Winner >= 0 || SuddenDeath || dt <= 0f) return false;
            TimeLeft = System.Math.Max(0f, TimeLeft - dt);
            if (TimeLeft > 0f) return false;
            if (points[0] == points[1])
            {
                SuddenDeath = true;
                return false;
            }
            Winner = points[0] > points[1] ? 0 : 1;
            return true;
        }
    }
}
