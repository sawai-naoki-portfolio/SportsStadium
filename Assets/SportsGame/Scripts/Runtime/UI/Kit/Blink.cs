using UnityEngine;
using UnityEngine.UI;

namespace SportsGame
{
    /// <summary>グラフィックをゆっくり点滅させる(「タップしてスタート」など)。</summary>
    [RequireComponent(typeof(Graphic))]
    public class Blink : MonoBehaviour
    {
        const float Speed = 2.5f;
        Graphic graphic;

        void Awake() => graphic = GetComponent<Graphic>();

        void Update()
        {
            var c = graphic.color;
            c.a = 0.35f + 0.65f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * Speed * Mathf.PI));
            graphic.color = c;
        }
    }
}
