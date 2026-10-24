using UnityEngine;

namespace SportsGame
{
    /// <summary>バスケットボールの状態を HUD 表示へ変換する。</summary>
    public class BasketballHudPresenter
    {
        public enum Role { Offense, Defense, LooseBall }

        static readonly string[] Names = { "YOU", "CPU" };
        readonly MatchHud hud;

        public BasketballHudPresenter(MatchHud hud) { this.hud = hud; }

        public static string Name(int team) => Names[team];

        public void ShowScore(BasketballScore score)
        {
            for (int i = 0; i < 2; i++) hud.Scoreboard.SetRow(i, "", score.Points(i).ToString());
            string info;
            if (score.SuddenDeath) info = "延長戦 ・ 次のゴールで決着";
            else if (score.Timed)
            {
                int s = Mathf.CeilToInt(score.TimeLeft);
                info = $"残り {s / 60}:{s % 60:00}";
            }
            else info = $"{score.TargetScore}点先取";
            hud.Scoreboard.SetInfo(info + " ・ 3Pは3点");
        }

        public void ShowShotClock(float seconds, bool running)
        {
            float s = Mathf.Max(0f, seconds);
            hud.Center.Show(Mathf.CeilToInt(s).ToString(), "SHOT CLOCK", running && s <= 5f ? new Color(1f, 0.35f, 0.3f) : Color.white);
        }

        public void ShowButtons(Role role, bool canShoot)
        {
            switch (role)
            {
                case Role.Offense:
                    hud.Controls.Set(0, "シュート", true, !canShoot);
                    hud.Controls.Set(1, "クロス");
                    break;
                case Role.Defense:
                    hud.Controls.Set(0, "ブロック");
                    hud.Controls.Set(1, "スティール");
                    break;
                default:
                    hud.Controls.Set(0, "ジャンプ");
                    hud.Controls.Set(1, "スティール", true, true);
                    break;
            }
            hud.Controls.Set(2, "ダッシュ");
        }
    }
}
