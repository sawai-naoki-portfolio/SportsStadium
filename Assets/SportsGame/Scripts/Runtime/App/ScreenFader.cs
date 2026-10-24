using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>シーン遷移時のフェード。DontDestroyOnLoad で常駐する。</summary>
    public class ScreenFader : MonoBehaviour
    {
        const float FadeOutTime = 0.28f;
        const float FadeInTime = 0.35f;

        static ScreenFader instance;
        Image image;
        bool busy;

        public static ScreenFader Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[ScreenFader]");
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<ScreenFader>();
                    instance.Build();
                }
                return instance;
            }
        }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            gameObject.AddComponent<GraphicRaycaster>();
            var rt = new GameObject("Fade", typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            image = rt.gameObject.AddComponent<Image>();
            image.color = new Color(0.02f, 0.03f, 0.06f, 0f);
            image.raycastTarget = false;
        }

        public void FadeToScene(string scene)
        {
            if (busy) return;
            StartCoroutine(FadeRoutine(scene));
        }

        IEnumerator FadeRoutine(string scene)
        {
            busy = true;
            image.raycastTarget = true;
            yield return Fade(0f, 1f, FadeOutTime);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            var op = SceneManager.LoadSceneAsync(scene);
            while (op != null && !op.isDone) yield return null;
            // 前のシーンで生成したメッシュ・マテリアル等を解放する
            yield return Resources.UnloadUnusedAssets();
            yield return Fade(1f, 0f, FadeInTime);
            image.raycastTarget = false;
            busy = false;
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            var c = image.color;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
                image.color = c;
                yield return null;
            }
            c.a = to;
            image.color = c;
        }
    }
}
