using System.Collections.Generic;

namespace SportsGame
{
    public enum VolleyballScoreEvent { None, Set, Match }

    /// <summary>
    /// ビーチバレーの得点(ラリーポイント制・2点差)とサーブ権。Unity に依存しない純粋なロジック。
    /// サーブ権を取り返したチームは、前回とは別の選手がサーブする。
    /// </summary>
    public class VolleyballScore
    {
        readonly int[] points = new int[2], sets = new int[2];
        readonly int[] lastServer = { -1, -1 };
        readonly List<int[]> setHistory = new List<int[]>();

        public int PointsPerSet { get; }
        public int SetsToWin { get; }
        public int FinalSetPoints { get; }
        public int ServingTeam { get; private set; }
        /// <summary>サーブするチーム内の選手(0 または 1)。</summary>
        public int ServerSlot { get; private set; }
        public int Winner { get; private set; } = -1;
        public IReadOnlyList<int[]> SetHistory => setHistory;

        public VolleyballScore(int pointsPerSet, int setsToWin, int finalSetPoints, int firstServingTeam)
        {
            PointsPerSet = System.Math.Max(1, pointsPerSet);
            SetsToWin = System.Math.Max(1, setsToWin);
            FinalSetPoints = System.Math.Max(1, finalSetPoints);
            ServingTeam = firstServingTeam == 1 ? 1 : 0;
            ServerSlot = 0;
            lastServer[ServingTeam] = 0;
        }

        public int Points(int team) => points[team];
        public int Sets(int team) => sets[team];
        public int SetNumber => setHistory.Count + 1;
        bool IsFinalSet => SetsToWin > 1 && sets[0] == SetsToWin - 1 && sets[1] == SetsToWin - 1;
        public int Target => IsFinalSet ? FinalSetPoints : PointsPerSet;

        /// <summary>あと1点でセット(または試合)を取れるチーム(無ければ -1)。</summary>
        public int GamePointTeam
        {
            get
            {
                for (int t = 0; t < 2; t++)
                    if (points[t] + 1 >= Target && points[t] + 1 - points[1 - t] >= 2) return t;
                return -1;
            }
        }

        public bool IsMatchPoint => GamePointTeam >= 0 && sets[GamePointTeam] == SetsToWin - 1;

        public VolleyballScoreEvent AwardRally(int team)
        {
            if (Winner >= 0 || (team != 0 && team != 1)) return VolleyballScoreEvent.None;
            points[team]++;
            if (team != ServingTeam) GainServe(team);
            if (points[team] < Target || points[team] - points[1 - team] < 2) return VolleyballScoreEvent.None;

            setHistory.Add(new[] { points[0], points[1] });
            sets[team]++;
            points[0] = points[1] = 0;
            if (sets[team] >= SetsToWin)
            {
                Winner = team;
                return VolleyballScoreEvent.Match;
            }
            // 次のセットはセットを落としたチームのサーブから
            GainServe(1 - team);
            return VolleyballScoreEvent.Set;
        }

        void GainServe(int team)
        {
            ServingTeam = team;
            ServerSlot = lastServer[team] == 0 ? 1 : 0;
            lastServer[team] = ServerSlot;
        }

        public string Summary()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var s in setHistory) sb.Append(s[0]).Append('-').Append(s[1]).Append("  ");
            if (Winner < 0) sb.Append(points[0]).Append('-').Append(points[1]);
            return sb.ToString().Trim();
        }
    }
}
