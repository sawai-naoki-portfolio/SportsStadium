using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>アクションボタン。押下/解放はラッチされ、GameInput が毎フレーム読み取る。</summary>
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        const int NoPointer = int.MinValue;

        Image background;
        Text label;
        Color color = Color.white;
        bool held, pendingDown, pendingUp, dim;
        int pointerId = NoPointer;

        public bool IsHeld => held && isActiveAndEnabled;

        public void Init(int index, Image bg, Text text, Color c)
        {
            background = bg;
            label = text;
            color = c;
            Refresh();
            GameInput.Register(index, this);
        }

        void OnDestroy() => GameInput.Unregister(this);

        public bool ConsumeDown()
        {
            bool d = pendingDown;
            pendingDown = false;
            return d;
        }

        public bool ConsumeUp()
        {
            bool u = pendingUp;
            pendingUp = false;
            return u;
        }

        public void SetLabel(string text)
        {
            if (label != null && label.text != text) label.text = text;
        }

        public void SetDim(bool value)
        {
            if (dim == value) return;
            dim = value;
            Refresh();
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (pointerId != NoPointer) return;
            pointerId = e.pointerId;
            held = true;
            pendingDown = true;
            transform.localScale = Vector3.one * 0.9f;
            Refresh();
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId == pointerId) Release();
        }

        void OnDisable()
        {
            if (held) Release();
        }

        void Release()
        {
            pointerId = NoPointer;
            held = false;
            pendingUp = true;
            transform.localScale = Vector3.one;
            Refresh();
        }

        void Refresh()
        {
            if (background == null) return;
            Color c = held ? Color.Lerp(color, Color.white, 0.35f) : color;
            c.a = dim ? 0.35f : 0.85f;
            background.color = c;
            if (label != null)
            {
                var lc = label.color;
                lc.a = dim ? 0.5f : 1f;
                label.color = lc;
            }
        }
    }
}
