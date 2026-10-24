using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>uGUI をコードで組み立てるためのファクトリ。</summary>
    public static class UIKit
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        public static Canvas CreateCanvas(string name, int order)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            // 横長端末は高さ基準、タブレット等の横幅が狭い端末は幅寄りに合わせて見切れを防ぐ
            float aspect = Screen.height > 0 ? Screen.width / (float)Screen.height : 16f / 9f;
            scaler.matchWidthOrHeight = aspect >= 16f / 9f ? 1f : 0.35f;
            go.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
            return canvas;
        }

        static void EnsureEventSystem()
        {
            if (EventSystem.current != null || Object.FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
            var module = es.AddComponent<InputSystemUIInputModule>();
            if (module.actionsAsset == null) module.AssignDefaultActions();
        }

        /// <summary>セーフエリア(ノッチ回避)に合わせたルートを作る。</summary>
        public static RectTransform SafeRoot(Canvas canvas)
        {
            var rt = Fill(Rect(canvas.transform, "SafeArea"));
            rt.gameObject.AddComponent<SafeArea>();
            return rt;
        }

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Fill(RectTransform rt, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static Image Image(Transform parent, string name, Color color, Sprite sprite = null, bool raycast = false)
        {
            var img = Rect(parent, name).gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null && sprite.border.sqrMagnitude > 0f) img.type = UnityEngine.UI.Image.Type.Sliced;
            return img;
        }

        public static Image RoundPanel(Transform parent, string name, Color color, int radius = 28, bool raycast = false)
            => Image(parent, name, color, UISprites.RoundedRect(radius), raycast);

        /// <summary>全画面の半透明背景(入力を遮る)。</summary>
        public static Image Dimmer(Transform parent, float alpha)
        {
            var dim = Image(parent, "Dim", new Color(0f, 0f, 0f, alpha), null, true);
            Fill(dim.rectTransform);
            return dim;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter, bool outline = true)
        {
            var t = Rect(parent, name).gameObject.AddComponent<Text>();
            t.font = UITheme.Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            if (outline)
            {
                var o = t.gameObject.AddComponent<Outline>();
                o.effectColor = new Color(0f, 0f, 0f, 0.55f);
                o.effectDistance = new Vector2(2f, -2f);
            }
            return t;
        }

        /// <summary>影・光沢付きの角丸ボタン。押下で効果音を鳴らす。</summary>
        public static Button Button(Transform parent, string name, string label, Color color, UnityAction onClick, int fontSize = 46, Sfx sound = Sfx.Click)
        {
            var rt = Rect(parent, name);
            var shadow = RoundPanel(rt, "Shadow", new Color(0f, 0f, 0f, 0.35f));
            Fill(shadow.rectTransform, 0f, -8f, 0f, 8f);
            var bg = RoundPanel(rt, "Bg", color, 28, true);
            Fill(bg.rectTransform);
            var shine = RoundPanel(bg.transform, "Shine", new Color(1f, 1f, 1f, 0.12f));
            shine.rectTransform.anchorMin = new Vector2(0f, 0.55f);
            shine.rectTransform.anchorMax = Vector2.one;
            shine.rectTransform.offsetMin = new Vector2(6f, 0f);
            shine.rectTransform.offsetMax = new Vector2(-6f, -5f);
            if (!string.IsNullOrEmpty(label)) Fill(Label(bg.transform, "Label", label, fontSize, Color.white).rectTransform, 10f, 4f, 10f, 4f);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            var colors = btn.colors;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.fadeDuration = 0.06f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(onClick);
            btn.onClick.AddListener(() => AudioManager.Play(sound, 0.8f));
            rt.gameObject.AddComponent<PressScale>();
            return btn;
        }

        public static Text ButtonLabel(Button b) => b.targetGraphic.GetComponentInChildren<Text>();

        /// <summary>ボタンの内容を配置するための背景(グラフィック)の Transform。</summary>
        public static Transform ButtonContent(Button b) => b.targetGraphic.transform;

        /// <summary>選択肢を横に並べるセグメントボタン。戻り値は並びの RectTransform。</summary>
        public static RectTransform Segmented(Transform parent, string[] labels, int selected, Color onColor, System.Action<int> onSelect, int fontSize = 34)
            => Segmented(parent, labels, selected, (i, on) => on ? onColor : UITheme.ToggleOff, onSelect, fontSize);

        /// <summary>選択肢ごとに色を変えるセグメントボタン。colorOf(番号, 選択中か) で色を決める。</summary>
        public static RectTransform Segmented(Transform parent, string[] labels, int selected, System.Func<int, bool, Color> colorOf, System.Action<int> onSelect, int fontSize = 34)
        {
            var row = Rect(parent, "Segmented");
            var buttons = new Button[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                int idx = i;
                var b = Button(row, "Opt" + i, labels[i], colorOf(i, i == selected), null, fontSize);
                var brt = (RectTransform)b.transform;
                brt.anchorMin = new Vector2(i / (float)labels.Length, 0f);
                brt.anchorMax = new Vector2((i + 1) / (float)labels.Length, 1f);
                brt.offsetMin = new Vector2(6f, 0f);
                brt.offsetMax = new Vector2(-6f, 0f);
                buttons[i] = b;
                b.onClick.AddListener(() =>
                {
                    for (int k = 0; k < buttons.Length; k++) buttons[k].targetGraphic.color = colorOf(k, k == idx);
                    onSelect?.Invoke(idx);
                });
            }
            return row;
        }

        public static Slider Slider(Transform parent, string name, float value, Color fillColor, UnityAction<float> onChange)
        {
            var rt = Rect(parent, name);
            Fill(RoundPanel(rt, "Bg", new Color(0.2f, 0.24f, 0.32f), 16).rectTransform, 0f, 18f, 0f, 18f);
            var fillArea = Fill(Rect(rt, "FillArea"), 6f, 22f, 6f, 22f);
            var fill = RoundPanel(fillArea, "Fill", fillColor, 12);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            var handleArea = Fill(Rect(rt, "HandleArea"), 20f, 0f, 20f, 0f);
            var handle = Image(handleArea, "Handle", Color.white, UISprites.Circle(), true);
            handle.rectTransform.sizeDelta = new Vector2(52f, 0f);
            var s = rt.gameObject.AddComponent<Slider>();
            s.fillRect = fill.rectTransform;
            s.handleRect = handle.rectTransform;
            s.targetGraphic = handle;
            s.minValue = 0f;
            s.maxValue = 1f;
            s.value = Mathf.Clamp01(value);
            if (onChange != null) s.onValueChanged.AddListener(onChange);
            return s;
        }
    }
}
