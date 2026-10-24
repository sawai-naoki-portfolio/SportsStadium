using UnityEngine;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>画面下部の操作ヒント。</summary>
    public class HintView
    {
        readonly GameObject root;
        readonly Text text;

        public HintView(RectTransform parent)
        {
            var bg = UIKit.RoundPanel(parent, "Hint", new Color(0f, 0f, 0f, 0.4f), 22);
            UIKit.Place(bg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(900f, 64f));
            root = bg.gameObject;
            text = UIKit.Label(bg.transform, "Text", "", 32, Color.white);
            UIKit.Fill(text.rectTransform, 16f, 0f, 16f, 0f);
            root.SetActive(false);
        }

        /// <summary>null または空文字で非表示。</summary>
        public void Set(string message)
        {
            bool on = !string.IsNullOrEmpty(message);
            if (root.activeSelf != on) root.SetActive(on);
            if (on && text.text != message) text.text = message;
        }
    }
}
