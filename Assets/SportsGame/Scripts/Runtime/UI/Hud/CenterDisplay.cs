using UnityEngine;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>画面上部中央の数値表示(ショットクロック・残り時間)。</summary>
    public class CenterDisplay
    {
        readonly GameObject root;
        readonly Text value, sub;

        public CenterDisplay(RectTransform parent)
        {
            var rt = UIKit.Place(UIKit.RoundPanel(parent, "Center", UITheme.Dark, 22).rectTransform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(250f, 120f));
            root = rt.gameObject;
            value = UIKit.Label(rt, "Value", "", 64, Color.white);
            UIKit.Place(value.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(240f, 78f));
            sub = UIKit.Label(rt, "Sub", "", 24, new Color(0.75f, 0.8f, 0.9f), TextAnchor.MiddleCenter, false);
            UIKit.Place(sub.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(240f, 34f));
            root.SetActive(false);
        }

        public void Show(string valueText, string subText, Color color)
        {
            if (!root.activeSelf) root.SetActive(true);
            if (value.text != valueText) value.text = valueText;
            value.color = color;
            if (sub.text != subText) sub.text = subText;
        }
    }
}
