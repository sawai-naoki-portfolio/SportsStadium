using UnityEngine;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>左上の2段スコアボード(チーム名・補足・メインスコア・サーブ権)。</summary>
    public class Scoreboard : MonoBehaviour
    {
        const float RowHeight = 64f;

        readonly Text[] details = new Text[2];
        readonly Text[] mains = new Text[2];
        readonly Image[] serveDots = new Image[2];
        Text info;

        public static Scoreboard Create(RectTransform parent, string leftName, Color leftColor, string rightName, Color rightColor)
        {
            var rt = UIKit.Place(UIKit.RoundPanel(parent, "Scoreboard", UITheme.Dark, 22).rectTransform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -22f), new Vector2(600f, 182f));
            var sb = rt.gameObject.AddComponent<Scoreboard>();
            sb.BuildRow(rt, 0, leftName, leftColor);
            sb.BuildRow(rt, 1, rightName, rightColor);
            sb.info = UIKit.Label(rt, "Info", "", 26, new Color(0.75f, 0.8f, 0.9f), TextAnchor.MiddleLeft, false);
            UIKit.Place(sb.info.rectTransform, Vector2.zero, Vector2.zero, new Vector2(22f, 8f), new Vector2(560f, 38f));
            return sb;
        }

        void BuildRow(RectTransform board, int i, string name, Color color)
        {
            float y = -16f - i * RowHeight;
            Vector2 topLeft = new Vector2(0f, 1f), topRight = new Vector2(1f, 1f);
            UIKit.Place(UIKit.Image(board, "Chip", color, UISprites.RoundedRect(6)).rectTransform, topLeft, topLeft, new Vector2(16f, y), new Vector2(14f, 54f));
            var n = UIKit.Label(board, "Name", name, 34, Color.white, TextAnchor.MiddleLeft);
            n.horizontalOverflow = HorizontalWrapMode.Overflow;
            UIKit.Place(n.rectTransform, topLeft, topLeft, new Vector2(44f, y), new Vector2(250f, 54f));
            serveDots[i] = UIKit.Image(board, "Serve", new Color(0.85f, 1f, 0.3f), UISprites.Circle(64));
            UIKit.Place(serveDots[i].rectTransform, topLeft, new Vector2(0.5f, 0.5f), new Vector2(276f, y - 27f), new Vector2(20f, 20f));
            serveDots[i].enabled = false;
            details[i] = UIKit.Label(board, "Detail", "", 32, UITheme.SubText, TextAnchor.MiddleRight);
            UIKit.Place(details[i].rectTransform, topRight, topRight, new Vector2(-150f, y), new Vector2(170f, 54f));
            var mainBg = UIKit.Image(board, "MainBg", new Color(1f, 1f, 1f, 0.1f), UISprites.RoundedRect(10));
            UIKit.Place(mainBg.rectTransform, topRight, topRight, new Vector2(-14f, y), new Vector2(124f, 54f));
            mains[i] = UIKit.Label(mainBg.transform, "Main", "0", 44, UITheme.Gold);
            UIKit.Fill(mains[i].rectTransform);
        }

        public void SetRow(int row, string detail, string main)
        {
            if (details[row].text != detail) details[row].text = detail;
            if (mains[row].text != main) mains[row].text = main;
        }

        /// <summary>サーブ権の表示。-1 で非表示。</summary>
        public void SetServe(int row)
        {
            for (int i = 0; i < 2; i++) serveDots[i].enabled = i == row;
        }

        public void SetInfo(string text)
        {
            if (info.text != text) info.text = text;
        }

        public void Punch(int row) => StartCoroutine(UITween.Pop(mains[row].transform.parent, 1.5f, 1f, 0.35f));
    }
}
