using UnityEngine;

namespace SportsGame
{
    /// <summary>暗転背景+中央パネル+タイトルのモーダル。ポーズ・リザルト・設定画面で共通に使う。</summary>
    public class ModalPanel : MonoBehaviour
    {
        public RectTransform Body { get; private set; }

        public static ModalPanel Open(Transform parent, string title, Vector2 size, float dimAlpha = 0.6f)
        {
            var dim = UIKit.Dimmer(parent, dimAlpha);
            dim.name = "Modal";
            var modal = dim.gameObject.AddComponent<ModalPanel>();
            modal.Body = UIKit.Place(UIKit.RoundPanel(dim.transform, "Panel", UITheme.Panel, 36, true).rectTransform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            if (!string.IsNullOrEmpty(title))
            {
                var t = UIKit.Label(modal.Body, "Title", title, 64, Color.white);
                UIKit.Place(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(size.x - 60f, 90f));
            }
            modal.StartCoroutine(UITween.Pop(modal.Body, 0.75f, 1f, 0.25f));
            return modal;
        }

        public void Close() => Destroy(gameObject);
    }
}
