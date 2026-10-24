using UnityEngine;
using UnityEngine.Rendering;

namespace SportsGame
{
    /// <summary>ボールの見た目(転がり回転・接地影・軌跡)。物理は BallSim 側が持つ。</summary>
    public class BallView : MonoBehaviour
    {
        Transform model, shadow;
        TrailRenderer trail;
        Vector3 lastPos;
        float shadowBase, modelRadius;

        public static BallView Create(Transform parent, string name, float radius, Texture2D tex, float visualScale, Color? trailColor = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var v = go.AddComponent<BallView>();
            float d = radius * 2f * visualScale;
            v.modelRadius = d * 0.5f;
            v.model = Geo.DetailedSphere(go.transform, Vector3.zero, d, Mats.LitTex(tex, Color.white, 0.35f), "Model").transform;
            v.shadow = Geo.MeshObject("Shadow", parent, MeshFactory.Disc(0.5f, 24), Mats.Transparent(new Color(0f, 0f, 0f, 0.45f), ProcTextures.RadialSoft())).transform;
            v.shadowBase = d * 1.3f;
            if (trailColor.HasValue) v.trail = CreateTrail(go, d, trailColor.Value);
            return v;
        }

        static TrailRenderer CreateTrail(GameObject go, float diameter, Color c)
        {
            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.minVertexDistance = 0.05f;
            trail.widthMultiplier = diameter * 0.9f;
            trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            trail.material = Mats.Particle;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                      new[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(0f, 1f) });
            trail.colorGradient = g;
            trail.shadowCastingMode = ShadowCastingMode.Off;
            trail.emitting = false;
            return trail;
        }

        public void Sync(Vector3 pos)
        {
            transform.position = pos;
            Vector3 move = pos - lastPos;
            lastPos = pos;
            // テレポート(持ち替え等)では回転させない
            if (move.sqrMagnitude > 1e-6f && move.sqrMagnitude < 4f)
            {
                Vector3 axis = Vector3.Cross(Vector3.up, move.normalized);
                if (axis.sqrMagnitude < 1e-4f) axis = Vector3.right;
                model.Rotate(axis, move.magnitude / modelRadius * Mathf.Rad2Deg, Space.World);
            }
            float s = shadowBase * (1f + Mathf.Max(0f, pos.y) * 0.12f);
            shadow.position = new Vector3(pos.x, 0.012f, pos.z);
            shadow.localScale = new Vector3(s, 1f, s);
        }

        public void SetTrail(bool on)
        {
            if (trail != null) trail.emitting = on;
        }

        void OnDestroy()
        {
            if (shadow != null) Destroy(shadow.gameObject);
        }
    }
}
