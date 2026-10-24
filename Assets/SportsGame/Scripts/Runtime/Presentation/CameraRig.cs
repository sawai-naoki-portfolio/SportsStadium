using UnityEngine;

namespace SportsGame
{
    /// <summary>試合用カメラ。目標位置・注視点へ滑らかに追従し、揺れ演出も行う。</summary>
    public class CameraRig : MonoBehaviour
    {
        const float PosSharpness = 3.5f;
        const float LookSharpness = 5f;

        public Camera Cam { get; private set; }
        Vector3 posTarget, lookTarget, curPos, curLook;
        float shakeAmp, shakeTime, shakeDur;

        public static CameraRig Create(float fov, Color background, bool skybox)
        {
            Camera c = Camera.main;
            if (c == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                c = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            c.fieldOfView = fov;
            c.nearClipPlane = 0.1f;
            c.farClipPlane = 400f;
            c.clearFlags = skybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            c.backgroundColor = background;
            PostEffects.Apply(c);
            var rig = c.GetComponent<CameraRig>();
            if (rig == null) rig = c.gameObject.AddComponent<CameraRig>();
            rig.Cam = c;
            return rig;
        }

        public void Snap(Vector3 pos, Vector3 look)
        {
            posTarget = curPos = pos;
            lookTarget = curLook = look;
            Apply(Vector2.zero);
        }

        public void SetTargets(Vector3 pos, Vector3 look)
        {
            posTarget = pos;
            lookTarget = look;
        }

        public void Shake(float amplitude, float duration)
        {
            shakeAmp = Mathf.Max(shakeAmp, amplitude);
            shakeDur = shakeTime = Mathf.Max(0.01f, duration);
        }

        void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            curPos = Vector3.Lerp(curPos, posTarget, 1f - Mathf.Exp(-PosSharpness * dt));
            curLook = Vector3.Lerp(curLook, lookTarget, 1f - Mathf.Exp(-LookSharpness * dt));
            Vector2 shake = Vector2.zero;
            if (shakeTime > 0f)
            {
                shakeTime -= dt;
                float k = shakeAmp * Mathf.Clamp01(shakeTime / shakeDur);
                shake = new Vector2(Mathf.PerlinNoise(Time.time * 25f, 0f) - 0.5f, Mathf.PerlinNoise(0f, Time.time * 25f) - 0.5f) * k;
                if (shakeTime <= 0f) shakeAmp = 0f;
            }
            Apply(shake);
        }

        void Apply(Vector2 shake)
        {
            transform.position = curPos;
            Vector3 d = curLook - curPos;
            if (d.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(d, Vector3.up);
            transform.position += transform.right * shake.x + transform.up * shake.y;
        }
    }
}
