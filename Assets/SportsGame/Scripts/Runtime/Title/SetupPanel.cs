using UnityEngine;

namespace SportsGame
{
    /// <summary>試合設定(難易度・試合の長さ・チームカラー)と開始ボタン。選択はすぐ GameSettings に保存する。</summary>
    public static class SetupPanel
    {
        public static GameObject Build(RectTransform parent, Sport s, System.Action onStart, System.Action onHowTo, System.Action onBack)
        {
            var p = UIKit.Fill(UIKit.Rect(parent, "Setup"));
            Color theme = SportCatalog.ThemeColor(s);
            var panel = UIKit.Place(UIKit.RoundPanel(p, "Panel", UITheme.Panel, 40, true).rectTransform,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-50f, 0f), new Vector2(980f, 940f));
            Vector2 top = new Vector2(0.5f, 1f);

            var head = UIKit.RoundPanel(panel, "Head", theme, 36);
            UIKit.Place(head.rectTransform, top, top, Vector2.zero, new Vector2(980f, 130f));
            UIKit.Fill(UIKit.Label(head.transform, "Title", SportCatalog.Name(s) + "  <size=36>" + SportCatalog.Subtitle(s) + "</size>", 60, Color.white).rectTransform);

            AddRow(panel, "難易度", -160f, UIKit.Segmented(panel, new[] { "かんたん", "ふつう", "むずかしい" }, (int)GameSettings.Difficulty, theme,
                i => { GameSettings.Difficulty = (Difficulty)i; GameSettings.Save(); }, 40));
            AddRow(panel, "試合の長さ", -305f, UIKit.Segmented(panel, SportCatalog.MatchLengthLabels(s), GameSettings.MatchLength, theme,
                i => { GameSettings.MatchLength = i; GameSettings.Save(); }, 32));
            // 選んでいない色は暗くして、選択中の色をはっきり見せる
            AddRow(panel, "チームカラー", -450f, UIKit.Segmented(panel, TeamColors.Names, GameSettings.TeamColor,
                (i, on) => on ? TeamColors.Get(i) : Color.Lerp(TeamColors.Get(i), UITheme.ToggleOff, 0.7f),
                i => { GameSettings.TeamColor = i; GameSettings.Save(); }, 36));

            var how = UIKit.Button(panel, "HowTo", "あそびかた", new Color(0.3f, 0.36f, 0.5f), () => onHowTo(), 40);
            UIKit.Place((RectTransform)how.transform, top, top, new Vector2(-230f, -630f), new Vector2(420f, 90f));
            var back = UIKit.Button(panel, "Back", "もどる", UITheme.Muted, () => onBack(), 40, Sfx.Back);
            UIKit.Place((RectTransform)back.transform, top, top, new Vector2(230f, -630f), new Vector2(420f, 90f));
            var start = UIKit.Button(panel, "Start", "試合開始！", UITheme.Green, () => onStart(), 60, Sfx.Confirm);
            UIKit.Place((RectTransform)start.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(880f, 140f));
            return p.gameObject;
        }

        /// <summary>見出し(captionY)とその下の選択肢を1行ぶん配置する。</summary>
        static void AddRow(RectTransform panel, string caption, float captionY, RectTransform control)
        {
            Vector2 top = new Vector2(0.5f, 1f);
            var l = UIKit.Label(panel, caption, caption, 40, UITheme.SubText, TextAnchor.MiddleLeft, false);
            UIKit.Place(l.rectTransform, top, top, new Vector2(0f, captionY), new Vector2(880f, 60f));
            UIKit.Place(control, top, top, new Vector2(0f, captionY - 60f), new Vector2(880f, 90f));
        }
    }
}
