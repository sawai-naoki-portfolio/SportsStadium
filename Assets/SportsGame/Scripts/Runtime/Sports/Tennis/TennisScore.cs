using System.Collections.Generic;

namespace SportsGame
{
    public enum TennisScoreEvent { None, Game, Set, Match, TiebreakStart }

    /// <summary>
    /// テニスの得点計算(0-15-30-40・デュース・タイブレーク・セット)とサーブ権。Unity に依存しない純粋なロジック。
    /// セットは gamesPerSet ゲーム先取かつ2ゲーム差、gamesPerSet-gamesPerSet でタイブレーク(7ポイント先取・2点差)。
    /// </summary>
    public class TennisScore
    {
        const int GamePoints = 4;
        const int TiebreakPoints = 7;

        readonly int[] points = new int[2], games = new int[2], sets = new int[2];
        readonly List<int[]> setHistory = new List<int[]>();
        int server, tiebreakFirstServer;

        public int GamesPerSet { get; }
        public int SetsToWin { get; }
        public bool Tiebreak { get; private set; }
        /// <summary>試合の勝者(未決着は -1)。</summary>
        public int Winner { get; private set; } = -1;
        public IReadOnlyList<int[]> SetHistory => setHistory;

        public TennisScore(int gamesPerSet, int setsToWin, int firstServer)
        {
            GamesPerSet = System.Math.Max(1, gamesPerSet);
            SetsToWin = System.Math.Max(1, setsToWin);
            server = firstServer == 1 ? 1 : 0;
        }

        public int Points(int player) => points[player];
        public int Games(int player) => games[player];
        public int Sets(int player) => sets[player];

        /// <summary>このポイントのサーバー。タイブレークは1ポイント目の後、2ポイントごとに交代する。</summary>
        public int CurrentServer
        {
            get
            {
                if (!Tiebreak) return server;
                int n = points[0] + points[1];
                return ((n + 1) / 2) % 2 == 0 ? tiebreakFirstServer : 1 - tiebreakFirstServer;
            }
        }

        /// <summary>デュースサイド(右側)からのサーブか。</summary>
        public bool DeuceSide => (points[0] + points[1]) % 2 == 0;

        public bool IsDeuce => !Tiebreak && points[0] >= 3 && points[0] == points[1];

        /// <summary>アドバンテージを持つ選手(無ければ -1)。</summary>
        public int Advantage => !Tiebreak && points[0] >= 3 && points[1] >= 3 && points[0] != points[1] ? (points[0] > points[1] ? 0 : 1) : -1;

        public TennisScoreEvent AwardPoint(int winner)
        {
            if (Winner >= 0 || (winner != 0 && winner != 1)) return TennisScoreEvent.None;
            points[winner]++;
            int need = Tiebreak ? TiebreakPoints : GamePoints;
            if (points[winner] >= need && points[winner] - points[1 - winner] >= 2) return WinGame(winner);
            return TennisScoreEvent.None;
        }

        TennisScoreEvent WinGame(int w)
        {
            bool wasTiebreak = Tiebreak;
            games[w]++;
            points[0] = points[1] = 0;
            if (wasTiebreak)
            {
                Tiebreak = false;
                server = 1 - tiebreakFirstServer;
            }
            else server = 1 - server;

            if (wasTiebreak || (games[w] >= GamesPerSet && games[w] - games[1 - w] >= 2))
            {
                setHistory.Add(new[] { games[0], games[1] });
                sets[w]++;
                games[0] = games[1] = 0;
                if (sets[w] >= SetsToWin)
                {
                    Winner = w;
                    return TennisScoreEvent.Match;
                }
                return TennisScoreEvent.Set;
            }
            if (games[0] == GamesPerSet && games[1] == GamesPerSet)
            {
                Tiebreak = true;
                tiebreakFirstServer = server;
                return TennisScoreEvent.TiebreakStart;
            }
            return TennisScoreEvent.Game;
        }

        /// <summary>スコアボード用のポイント表記(0/15/30/40/AD、タイブレークは数字)。</summary>
        public string PointLabel(int player)
        {
            if (Tiebreak) return points[player].ToString();
            if (Advantage >= 0) return Advantage == player ? "AD" : "40";
            switch (points[player])
            {
                case 0: return "0";
                case 1: return "15";
                case 2: return "30";
                default: return "40";
            }
        }

        /// <summary>終了したセットと現在のゲーム数(例: "6-4 2-1")。</summary>
        public string Summary()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var s in setHistory) sb.Append(s[0]).Append('-').Append(s[1]).Append("  ");
            if (Winner < 0) sb.Append(games[0]).Append('-').Append(games[1]);
            return sb.ToString().Trim();
        }
    }
}
