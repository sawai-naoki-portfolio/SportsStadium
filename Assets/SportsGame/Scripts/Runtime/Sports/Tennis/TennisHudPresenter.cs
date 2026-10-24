using System.Text;
using UnityEngine;

namespace SportsGame
{
    /// <summary>テニスの状態を HUD 表示(スコアボード・ボタン・メッセージ文言)へ変換する。</summary>
    public class TennisHudPresenter
    {
        static readonly string[] Names = { "YOU", "CPU" };
        readonly MatchHud hud;
        readonly StringBuilder sb = new StringBuilder();

        public TennisHudPresenter(MatchHud hud) { this.hud = hud; }

        public static string Name(int player) => Names[player];

        public void ShowScore(TennisScore score, bool showServe)
        {
            for (int i = 0; i < 2; i++)
            {
                sb.Clear();
                foreach (var s in score.SetHistory) sb.Append("<color=#8F9BB3>").Append(s[i]).Append("</color> ");
                sb.Append(score.Games(i));
                hud.Scoreboard.SetRow(i, sb.ToString(), score.PointLabel(i));
            }
            hud.Scoreboard.SetServe(showServe ? score.CurrentServer : -1);
            string info = score.Tiebreak ? "タイブレーク (7ポイント先取)"
                : score.IsDeuce ? "デュース"
                : $"セット {score.SetHistory.Count + 1} ・ {score.GamesPerSet}ゲーム先取";
            hud.Scoreboard.SetInfo(info);
        }

        public void ShowServeButtons(bool tossed)
        {
            string label = tossed ? "サーブ" : "トス";
            hud.Controls.Set(0, label);
            hud.Controls.Set(1, label);
            hud.Controls.Set(2, "", false);
            hud.Controls.Highlight(-1);
        }

        public void ShowRallyButtons(SwingIntent swing)
        {
            hud.Controls.Set(0, "トップ\nスピン");
            hud.Controls.Set(1, "スライス");
            hud.Controls.Set(2, "ロブ");
            hud.Controls.Highlight(swing.active ? ButtonFor(swing.shot) : -1);
        }

        static int ButtonFor(TShot shot) => shot == TShot.Slice ? 1 : shot == TShot.Lob ? 2 : 0;

        public static string ReasonText(PointReason r)
        {
            switch (r)
            {
                case PointReason.Ace: return "ACE!";
                case PointReason.Winner: return "WINNER!";
                case PointReason.Net: return "NET";
                case PointReason.DoubleFault: return "DOUBLE FAULT";
                default: return "OUT";
            }
        }

        public static string EventText(TennisScoreEvent e, int gameWinner)
        {
            switch (e)
            {
                case TennisScoreEvent.Set: return "SET " + Name(gameWinner);
                case TennisScoreEvent.TiebreakStart: return "タイブレーク";
                case TennisScoreEvent.Match: return "GAME SET";
                default: return "GAME " + Name(gameWinner);
            }
        }

        /// <summary>デュース・アドバンテージのコール(無ければ null)。</summary>
        public static string CallText(TennisScore s)
        {
            if (s.IsDeuce) return "DEUCE";
            return s.Advantage >= 0 ? "ADVANTAGE " + Name(s.Advantage) : null;
        }

        public static Color PointColor(int winner) => winner == 0 ? UITheme.Gold : new Color(0.6f, 0.8f, 1f);
    }
}
