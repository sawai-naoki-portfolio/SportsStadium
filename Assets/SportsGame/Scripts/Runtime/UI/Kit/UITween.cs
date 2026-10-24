using System.Collections;
using UnityEngine;

namespace SportsGame
{
    /// <summary>UI の簡易アニメーション(時間停止中も動くよう unscaled time を使う)。</summary>
    public static class UITween
    {
        /// <summary>from から to へ、少し行き過ぎて戻る拡大縮小。</summary>
        public static IEnumerator Pop(Transform t, float from, float to, float duration)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / duration) - 1f;
                float eased = 1f + 2.2f * k * k * k + 1.2f * k * k;
                if (t == null) yield break;
                t.localScale = Vector3.one * Mathf.LerpUnclamped(from, to, eased);
                yield return null;
            }
            if (t != null) t.localScale = Vector3.one * to;
        }
    }
}
