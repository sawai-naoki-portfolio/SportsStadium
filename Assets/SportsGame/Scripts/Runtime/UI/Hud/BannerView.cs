using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>画面中央に出るメッセージ。Large は得点などの大きな出来事、Small は状況の通知。</summary>
    public enum BannerSize { Large, Small }

    /// <summary>画面中央に大きく出るメッセージ(「ACE!」「3 POINTS!」など)。</summary>
    public class BannerView : MonoBehaviour
    {
        const float CloseTime = 0.2f;
        const int LargeFont = 108, SmallFont = 64;

        Text title, sub;
        Coroutine routine;

        public static BannerView Create(RectTransform parent)
        {
            var rt = UIKit.Place(UIKit.Rect(parent, "Banner"), new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 190f));
            var b = rt.gameObject.AddComponent<BannerView>();
            UIKit.Fill(UIKit.RoundPanel(rt, "Bg", new Color(0f, 0f, 0f, 0.45f), 40).rectTransform);
            b.title = UIKit.Label(rt, "Text", "", 108, Color.white);
            UIKit.Fill(b.title.rectTransform, 0f, 40f, 0f, 0f);
            b.title.horizontalOverflow = HorizontalWrapMode.Overflow;
            b.title.GetComponent<Outline>().effectDistance = new Vector2(4f, -4f);
            b.sub = UIKit.Label(rt, "Sub", "", 40, new Color(1f, 1f, 1f, 0.9f));
            UIKit.Place(b.sub.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(1400f, 50f));
            rt.gameObject.SetActive(false);
            return b;
        }

        public void Show(string text, Color color, float duration, string subText = null, BannerSize size = BannerSize.Large)
        {
            if (routine != null) StopCoroutine(routine);
            gameObject.SetActive(true);
            title.fontSize = size == BannerSize.Large ? LargeFont : SmallFont;
            routine = StartCoroutine(Run(text, color, duration, subText));
        }

        /// <summary>状況の通知用の小さめのメッセージ。</summary>
        public void Notice(string text, Color color, float duration = 0.7f, string subText = null)
            => Show(text, color, duration, subText, BannerSize.Small);

        public void Hide()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            gameObject.SetActive(false);
        }

        IEnumerator Run(string text, Color color, float duration, string subText)
        {
            title.text = text;
            title.color = color;
            sub.text = subText ?? "";
            sub.gameObject.SetActive(!string.IsNullOrEmpty(subText));
            transform.localScale = Vector3.one;
            yield return UITween.Pop(title.transform, 1.8f, 1f, 0.25f);
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime) yield return null;
            for (float t = 0f; t < CloseTime; t += Time.unscaledDeltaTime)
            {
                transform.localScale = new Vector3(1f, Mathf.Max(0.01f, 1f - t / CloseTime), 1f);
                yield return null;
            }
            gameObject.SetActive(false);
            routine = null;
        }
    }
}
