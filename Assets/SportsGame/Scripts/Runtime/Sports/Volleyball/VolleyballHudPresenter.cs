using UnityEngine;

namespace SportsGame
{
    /// <summary>ビーチバレーの状態を HUD 表示へ変換する。</summary>
    public class VolleyballHudPresenter
    {
        static readonly string[] Names = { "YOU", "CPU" };
        readonly MatchHud hud;

        public VolleyballHudPresenter(MatchHud hud) { this.hud = hud; }

        public static string Name(int team) => Names[team];

        public void ShowScore(VolleyballScore score, bool showServe)
        {
            for (int i = 0; i < 2; i++)
                hud.Scoreboard.SetRow(i, score.SetsToWin > 1 ? $"<color=#8F9BB3>セット</color> {score.Sets(i)}" : "", score.Points(i).ToString());
            hud.Scoreboard.SetServe(showServe ? score.ServingTeam : -1);
            string info = $"第{score.SetNumber}セット ・ {score.Target}点先取";
            if (score.IsMatchPoint) info += " ・ マッチポイント";
            else if (score.GamePointTeam >= 0) info += " ・ セットポイント";
            hud.Scoreboard.SetInfo(info);
        }

        public void ShowServeButtons(bool tossed)
        {
            hud.Controls.Set(0, tossed ? "サーブ" : "トス");
            hud.Controls.Set(1, "", false);
        }

        /// <summary>プレイヤーの状況に応じて A/B の役割を表示する。</summary>
        public void ShowRallyButtons(VolleyballPlan mine, VolleyballPlan theirs, int humanIndex)
        {
            bool myTouch = mine.valid && mine.toucher == humanIndex;
            if (myTouch && mine.touch == VbTouch.Attack)
            {
                hud.Controls.Set(0, "フェイント");
                hud.Controls.Set(1, "スパイク");
            }
            else if (myTouch)
            {
                hud.Controls.Set(0, mine.touch == VbTouch.Set ? "トス" : "レシーブ");
                hud.Controls.Set(1, "ジャンプ");
            }
            else
            {
                hud.Controls.Set(0, "レシーブ");
                hud.Controls.Set(1, theirs.valid && theirs.touch != VbTouch.Receive ? "ブロック" : "ジャンプ");
            }
        }

        public static string ReasonText(RallyEnd r)
        {
            switch (r)
            {
                case RallyEnd.Kill: return "NICE KILL!";
                case RallyEnd.Ace: return "ACE!";
                case RallyEnd.Blocked: return "BLOCK!";
                case RallyEnd.Antenna: return "アンテナ";
                case RallyEnd.UnderNet: return "ネットの下";
                case RallyEnd.FourHits: return "フォアヒット";
                case RallyEnd.DoubleTouch: return "ドリブル";
                default: return "OUT";
            }
        }

        public static Color PointColor(int winner) => winner == 0 ? UITheme.Gold : new Color(0.6f, 0.8f, 1f);
    }
}
