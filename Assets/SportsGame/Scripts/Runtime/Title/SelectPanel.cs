using UnityEngine;

namespace SportsGame
{
    /// <summary>種目選択(3枚のカードと戦績)。</summary>
    public static class SelectPanel
    {
        public static GameObject Build(RectTransform parent, System.Action<Sport> onSelect, System.Action onBack, System.Action onOptions)
        {
            var p = UIKit.Fill(UIKit.Rect(parent, "Select"));
            var header = UIKit.Label(p, "Header", "種目をえらぼう", 72, Color.white);
            UIKit.Place(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1200f, 110f));

            var back = UIKit.Button(p, "Back", "◀ タイトル", new Color(0.3f, 0.34f, 0.45f), () => onBack(), 36, Sfx.Back);
            UIKit.Place((RectTransform)back.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -34f), new Vector2(280f, 96f));
            var opt = UIKit.Button(p, "Options", "設定", new Color(0.3f, 0.34f, 0.45f), () => onOptions(), 36);
            UIKit.Place((RectTransform)opt.transform, Vector2.one, Vector2.one, new Vector2(-30f, -34f), new Vector2(240f, 96f));

            var row = UIKit.Place(UIKit.Rect(p, "Cards"), new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1560f, 700f));
            for (int i = 0; i < SportCatalog.All.Length; i++) BuildCard(row, i, SportCatalog.All[i], onSelect);
            return p.gameObject;
        }

        static void BuildCard(RectTransform row, int i, Sport s, System.Action<Sport> onSelect)
        {
            float x = i / (float)(SportCatalog.All.Length - 1);
            var card = UIKit.Button(row, "Card" + s, null, SportCatalog.ThemeColor(s), () => onSelect(s), 40);
            UIKit.Place((RectTransform)card.transform, new Vector2(x, 0.5f), new Vector2(x, 0.5f), Vector2.zero, new Vector2(480f, 680f));
            var content = UIKit.ButtonContent(card);
            Vector2 top = new Vector2(0.5f, 1f), bottom = new Vector2(0.5f, 0f);
            UIKit.Place(UIKit.Image(content, "Icon", Color.white, UISprites.SportIcon(s)).rectTransform, top, top, new Vector2(0f, -50f), new Vector2(260f, 260f));
            var name = UIKit.Label(content, "Name", SportCatalog.Name(s), 56, Color.white);
            name.horizontalOverflow = HorizontalWrapMode.Overflow;
            UIKit.Place(name.rectTransform, top, top, new Vector2(0f, -330f), new Vector2(460f, 80f));
            UIKit.Place(UIKit.Label(content, "Sub", SportCatalog.Subtitle(s), 38, new Color(1f, 1f, 1f, 0.9f)).rectTransform, top, top, new Vector2(0f, -410f), new Vector2(460f, 60f));
            var recBg = UIKit.RoundPanel(content, "RecordBg", new Color(0f, 0f, 0f, 0.25f), 20);
            UIKit.Place(recBg.rectTransform, bottom, bottom, new Vector2(0f, 120f), new Vector2(400f, 70f));
            UIKit.Fill(UIKit.Label(recBg.transform, "Record", $"通算 {MatchRecords.Wins(s)}勝 {MatchRecords.Losses(s)}敗", 34, Color.white).rectTransform);
            UIKit.Place(UIKit.Label(content, "Play", "▶ プレイ", 44, UITheme.Gold).rectTransform, bottom, bottom, new Vector2(0f, 30f), new Vector2(400f, 70f));
        }
    }
}
