using UnityEngine;

namespace SportsGame
{
    /// <summary>アクションボタンの初期設定。</summary>
    public struct ButtonSpec
    {
        public string label;
        public Color color;

        public ButtonSpec(string label, Color color)
        {
            this.label = label;
            this.color = color;
        }
    }

    /// <summary>左の仮想スティックと右のアクションボタン(最大3つ)。</summary>
    public class ActionControls : MonoBehaviour
    {
        const float StickRadius = 125f;
        static readonly Vector2[] ButtonPositions = { new Vector2(-210f, 210f), new Vector2(-470f, 140f), new Vector2(-190f, 470f) };
        static readonly float[] ButtonSizes = { 240f, 190f, 175f };

        readonly TouchButton[] buttons = new TouchButton[GameInput.ButtonCount];
        int highlighted = -1;

        public static ActionControls Create(RectTransform parent, ButtonSpec[] specs)
        {
            var root = UIKit.Fill(UIKit.Rect(parent, "Controls"));
            root.SetAsFirstSibling();
            var c = root.gameObject.AddComponent<ActionControls>();
            c.BuildStick(root);
            for (int i = 0; i < GameInput.ButtonCount; i++)
            {
                bool used = specs != null && i < specs.Length && !string.IsNullOrEmpty(specs[i].label);
                c.buttons[i] = c.BuildButton(root, i, used ? specs[i] : new ButtonSpec("", UITheme.Accent));
                if (!used) c.buttons[i].gameObject.SetActive(false);
            }
            return c;
        }

        void BuildStick(RectTransform root)
        {
            var area = UIKit.Image(root, "StickArea", new Color(1f, 1f, 1f, 0f), null, true).rectTransform;
            area.anchorMin = Vector2.zero;
            area.anchorMax = new Vector2(0.45f, 0.78f);
            area.pivot = Vector2.zero;
            area.offsetMin = area.offsetMax = Vector2.zero;
            var stickBase = UIKit.Image(area, "Base", new Color(1f, 1f, 1f, 0.3f), UISprites.Circle(256, 10f));
            UIKit.Place(stickBase.rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(270f, 250f), new Vector2(290f, 290f));
            UIKit.Fill(UIKit.Image(stickBase.transform, "Fill", new Color(0f, 0f, 0f, 0.25f), UISprites.Circle(256)).rectTransform, 8f, 8f, 8f, 8f);
            var knob = UIKit.Image(stickBase.transform, "Knob", new Color(1f, 1f, 1f, 0.9f), UISprites.Circle(128));
            UIKit.Place(knob.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130f, 130f));
            area.gameObject.AddComponent<TouchJoystick>().Init(area, stickBase.rectTransform, knob.rectTransform, StickRadius);
        }

        TouchButton BuildButton(RectTransform root, int i, ButtonSpec spec)
        {
            var bg = UIKit.Image(root, "Btn" + i, spec.color, UISprites.Circle(256), true);
            UIKit.Place(bg.rectTransform, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), ButtonPositions[i], Vector2.one * ButtonSizes[i]);
            UIKit.Fill(UIKit.Image(bg.transform, "Ring", new Color(1f, 1f, 1f, 0.6f), UISprites.Circle(256, 8f)).rectTransform);
            var label = UIKit.Label(bg.transform, "Label", spec.label, i == 0 ? 42 : 36, Color.white);
            // 長いラベルは改行せずに縮小して収める
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 20;
            label.resizeTextMaxSize = label.fontSize;
            UIKit.Fill(label.rectTransform, 18f, 0f, 18f, 0f);
            var tb = bg.gameObject.AddComponent<TouchButton>();
            tb.Init(i, bg, label, spec.color);
            return tb;
        }

        public void Set(int index, string label, bool visible = true, bool dim = false)
        {
            if (index < 0 || index >= buttons.Length) return;
            var b = buttons[index];
            if (b.gameObject.activeSelf != visible) b.gameObject.SetActive(visible);
            b.SetLabel(label);
            b.SetDim(dim);
        }

        /// <summary>予約中の操作などを示すため、ボタンを脈打たせる(-1 で解除)。</summary>
        public void Highlight(int index) => highlighted = index;

        public void SetVisible(bool on)
        {
            if (gameObject.activeSelf != on) gameObject.SetActive(on);
        }

        void Update()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].IsHeld) continue;
                float s = i == highlighted ? 1.05f + Mathf.Sin(Time.unscaledTime * 18f) * 0.04f : 1f;
                buttons[i].transform.localScale = Vector3.one * s;
            }
        }
    }
}
