using UnityEngine;

namespace SportsGame
{
    /// <summary>ノッチ等を避けるため RectTransform を Screen.safeArea に合わせる。</summary>
    public class SafeArea : MonoBehaviour
    {
        Rect applied;
        Vector2Int appliedScreen;

        void Start() => Apply();

        void Update()
        {
            if (Screen.safeArea != applied || appliedScreen.x != Screen.width || appliedScreen.y != Screen.height) Apply();
        }

        void Apply()
        {
            applied = Screen.safeArea;
            appliedScreen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Sanitize(applied, Screen.width, Screen.height);
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rt.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        /// <summary>エディタ等で画面外の値が返ることがあるため、画面に収まらない場合は全画面を使う。</summary>
        public static Rect Sanitize(Rect safe, float width, float height)
        {
            bool invalid = safe.width <= 0f || safe.height <= 0f || safe.xMin < 0f || safe.yMin < 0f
                           || safe.xMax > width + 0.5f || safe.yMax > height + 0.5f;
            return invalid ? new Rect(0f, 0f, width, height) : safe;
        }
    }
}
