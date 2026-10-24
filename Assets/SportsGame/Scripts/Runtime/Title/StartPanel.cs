using UnityEngine;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>タイトルロゴと「タップしてスタート」。</summary>
    public static class StartPanel
    {
        public const string Version = "ver 1.0";

        public static GameObject Build(RectTransform parent, System.Action onStart)
        {
            var p = UIKit.Fill(UIKit.Rect(parent, "Start"));
            var tap = UIKit.Image(p, "Tap", new Color(0f, 0f, 0f, 0f), null, true);
            UIKit.Fill(tap.rectTransform);
            tap.gameObject.AddComponent<Button>().onClick.AddListener(() =>
            {
                AudioManager.Play(Sfx.Confirm);
                onStart();
            });

            UIKit.Place(UIKit.RoundPanel(p, "LogoBg", new Color(0f, 0f, 0f, 0.35f), 40).rectTransform,
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 330f));
            var logo = UIKit.Label(p, "Logo", "SPORTS STADIUM", 150, UITheme.Gold);
            UIKit.Place(logo.rectTransform, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1800f, 180f));
            logo.horizontalOverflow = HorizontalWrapMode.Overflow;
            logo.GetComponent<Outline>().effectDistance = new Vector2(6f, -6f);
            var shadow = logo.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.6f, 0.2f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(0f, -10f);

            var sub = UIKit.Label(p, "Sub", "スポーツスタジアム  ～ バスケ・テニス・バレー ～", 52, Color.white);
            UIKit.Place(sub.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600f, 80f));
            var start = UIKit.Label(p, "Start", "画面をタップしてスタート", 56, Color.white);
            UIKit.Place(start.rectTransform, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 90f));
            start.gameObject.AddComponent<Blink>();
            var ver = UIKit.Label(p, "Version", Version, 26, new Color(1f, 1f, 1f, 0.5f), TextAnchor.LowerRight, false);
            UIKit.Place(ver.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 20f), new Vector2(300f, 40f));
            return p.gameObject;
        }
    }
}
