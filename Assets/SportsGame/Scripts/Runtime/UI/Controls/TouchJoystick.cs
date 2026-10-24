using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>
    /// 画面左側のどこを触っても出現するフローティング式の仮想スティック。
    /// </summary>
    public class TouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        const float DeadZone = 0.12f;
        const float IdleAlpha = 0.45f;
        const float ActiveAlpha = 0.95f;
        const int NoPointer = int.MinValue;

        RectTransform area, baseRect, knob;
        Image baseImage, knobImage;
        float radius;
        Vector2 defaultPos;
        int pointerId = NoPointer;

        public Vector2 Value { get; private set; }

        public void Init(RectTransform touchArea, RectTransform stickBase, RectTransform stickKnob, float stickRadius)
        {
            area = touchArea;
            baseRect = stickBase;
            knob = stickKnob;
            radius = Mathf.Max(1f, stickRadius);
            baseImage = baseRect.GetComponent<Image>();
            knobImage = knob.GetComponent<Image>();
            defaultPos = baseRect.anchoredPosition;
            SetAlpha(IdleAlpha);
            GameInput.Register(this);
        }

        void OnDestroy() => GameInput.Unregister(this);

        void OnDisable() => Release();

        public void OnPointerDown(PointerEventData e)
        {
            if (pointerId != NoPointer || area == null) return;
            pointerId = e.pointerId;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, e.position, e.pressEventCamera, out var lp))
                baseRect.anchoredPosition = lp - area.rect.min;
            knob.anchoredPosition = Vector2.zero;
            Value = Vector2.zero;
            SetAlpha(ActiveAlpha);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != pointerId) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, e.position, e.pressEventCamera, out var lp)) return;
            Vector2 center = baseRect.anchoredPosition + area.rect.min;
            Vector2 d = Vector2.ClampMagnitude(lp - center, radius);
            knob.anchoredPosition = d;
            Vector2 v = d / radius;
            float mag = v.magnitude;
            Value = mag < DeadZone ? Vector2.zero : v.normalized * Mathf.InverseLerp(DeadZone, 1f, mag);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId == pointerId) Release();
        }

        void Release()
        {
            pointerId = NoPointer;
            Value = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            if (baseRect != null) baseRect.anchoredPosition = defaultPos;
            SetAlpha(IdleAlpha);
        }

        void SetAlpha(float a)
        {
            if (baseImage != null) { var c = baseImage.color; c.a = a * 0.6f; baseImage.color = c; }
            if (knobImage != null) { var c = knobImage.color; c.a = a; knobImage.color = c; }
        }
    }
}
