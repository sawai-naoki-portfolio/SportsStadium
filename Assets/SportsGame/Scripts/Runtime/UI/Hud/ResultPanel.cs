using System.Collections.Generic;
using UnityEngine;

namespace SportsGame
{
    /// <summary>試合結果の表示内容。</summary>
    public struct MatchResult
    {
        public bool humanWon;
        public string scoreLine;
        public IList<string> stats;
    }

    /// <summary>試合終了時のリザルト画面(勝敗・スコア・スタッツ・再戦ボタン)。</summary>
    public static class ResultPanel
    {
        const int StatColumns = 2;

        public static void Show(Transform canvas, MatchResult result)
        {
            var modal = ModalPanel.Open(canvas, null, new Vector2(1100f, 860f), 0.62f);
            var body = modal.Body;
            bool win = result.humanWon;

            var ribbon = UIKit.RoundPanel(body, "Ribbon", win ? new Color(0.95f, 0.65f, 0.1f) : new Color(0.35f, 0.4f, 0.55f), 30);
            UIKit.Place(ribbon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(760f, 150f));
            var title = UIKit.Label(ribbon.transform, "Title", win ? "YOU WIN!" : "YOU LOSE...", 104, Color.white);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            UIKit.Fill(title.rectTransform);

            AddLine(body, "Sub", win ? "勝利！ナイスゲーム！" : "おしい！次はきっと勝てる！", 40, UITheme.SubText, -110f, 60f);
            AddLine(body, "Score", result.scoreLine, 76, UITheme.Gold, -180f, 100f).horizontalOverflow = HorizontalWrapMode.Overflow;

            var stats = UIKit.Place(UIKit.RoundPanel(body, "Stats", new Color(0f, 0f, 0f, 0.25f), 24).rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(940f, 360f));
            int n = result.stats != null ? result.stats.Count : 0;
            for (int i = 0; i < n; i++)
            {
                var s = UIKit.Label(stats, "Stat" + i, result.stats[i], 34, Color.white, TextAnchor.MiddleLeft, false);
                s.horizontalOverflow = HorizontalWrapMode.Overflow;
                UIKit.Place(s.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f + i % StatColumns * 460f, -24f - i / StatColumns * 64f), new Vector2(440f, 56f));
            }

            var again = UIKit.Button(body, "Again", "もう一度", UITheme.Green, SceneFlow.Reload, 46, Sfx.Confirm);
            UIKit.Place((RectTransform)again.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-240f, 40f), new Vector2(440f, 120f));
            var menu = UIKit.Button(body, "Menu", "種目選択へ", UITheme.Blue, SceneFlow.GoSportSelect, 46, Sfx.Back);
            UIKit.Place((RectTransform)menu.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(240f, 40f), new Vector2(440f, 120f));
        }

        static UnityEngine.UI.Text AddLine(RectTransform body, string name, string text, int size, Color color, float y, float height)
        {
            var t = UIKit.Label(body, name, text, size, color);
            UIKit.Place(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(1000f, height));
            return t;
        }
    }
}
