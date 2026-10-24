using UnityEngine;

namespace SportsGame
{
    /// <summary>観客席の歓声演出(観客を上下に弾ませる)。</summary>
    public class CrowdAnimator : MonoBehaviour
    {
        const float JumpHeight = 0.18f;
        Transform[] crowds = new Transform[0];
        float remaining;

        public static CrowdAnimator Attach(GameObject host, Transform[] crowds)
        {
            var c = host.AddComponent<CrowdAnimator>();
            c.crowds = crowds ?? new Transform[0];
            return c;
        }

        public void Cheer(float seconds) => remaining = Mathf.Max(remaining, seconds);

        void Update()
        {
            remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            float k = Mathf.Clamp01(remaining);
            for (int i = 0; i < crowds.Length; i++)
            {
                if (crowds[i] == null) continue;
                crowds[i].localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 11f + i)) * JumpHeight * k, 0f);
            }
        }
    }
}
