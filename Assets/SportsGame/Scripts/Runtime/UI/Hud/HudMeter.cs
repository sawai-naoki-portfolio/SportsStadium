using UnityEngine;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>ワールド座標の近くに出す縦型ゲージ(シュート・サーブのタイミング)。</summary>
    public class HudMeter : MonoBehaviour
    {
        const float Height = 220f;
        const float MaxValue = 1.25f;
        static readonly Color ZoneColor = new Color(0.3f, 1f, 0.45f, 0.85f);

        RectTransform rt, fill, zone, marker;
        Image fillImage;

        public static HudMeter Create(RectTransform parent, Color fillColor)
        {
            var bg = UIKit.RoundPanel(parent, "Meter", new Color(0f, 0f, 0f, 0.6f), 14);
            var m = bg.gameObject.AddComponent<HudMeter>();
            m.rt = UIKit.Place(bg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(46f, Height + 12f));
            m.zone = BottomAnchored(UIKit.Image(m.rt, "Zone", ZoneColor, UISprites.RoundedRect(6)).rectTransform, new Vector2(0.5f, 0.5f));
            m.fillImage = UIKit.Image(m.rt, "Fill", fillColor, UISprites.RoundedRect(8));
            m.fill = BottomAnchored(m.fillImage.rectTransform, new Vector2(0.5f, 0f));
            m.fill.anchoredPosition = new Vector2(0f, 6f);
            m.marker = BottomAnchored(UIKit.Image(m.rt, "Marker", Color.white).rectTransform, new Vector2(0.5f, 0.5f));
            m.marker.sizeDelta = new Vector2(58f, 6f);
            bg.gameObject.SetActive(false);
            return m;
        }

        static RectTransform BottomAnchored(RectTransform r, Vector2 pivot)
        {
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f);
            r.pivot = pivot;
            return r;
        }

        /// <summary>value: 0〜1.25。zoneCenter/zoneWidth: 理想タイミングの範囲。</summary>
        public void Show(Vector2 hudPos, float value, float zoneCenter, float zoneWidth, Color fillColor)
        {
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            rt.anchoredPosition = hudPos;
            float v = Mathf.Clamp(value, 0f, MaxValue) / MaxValue;
            fill.sizeDelta = new Vector2(30f, Mathf.Max(4f, v * Height));
            zone.anchoredPosition = new Vector2(0f, 6f + zoneCenter / MaxValue * Height);
            zone.sizeDelta = new Vector2(40f, Mathf.Max(6f, zoneWidth / MaxValue * Height));
            marker.anchoredPosition = new Vector2(0f, 6f + v * Height);
            fillImage.color = fillColor;
        }

        public void Hide()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }
    }
}
