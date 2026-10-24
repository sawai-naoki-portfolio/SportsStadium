using UnityEngine;

namespace SportsGame
{
    /// <summary>タイトル背景の3Dシーン(台座の上で回る3つのボールとゆっくり動くカメラ)。</summary>
    public class TitleBackdrop : MonoBehaviour
    {
        static readonly float[] BallSizes = { 1.5f, 1.1f, 1.35f };

        readonly Transform[] balls = new Transform[3];
        readonly Vector3[] ballHome = new Vector3[3];
        Camera cam;
        float time;
        int focus = -1;

        public static TitleBackdrop Create()
        {
            var root = new GameObject("TitleBackdrop");
            var b = root.AddComponent<TitleBackdrop>();
            b.Build();
            return b;
        }

        /// <summary>-1 で全体、0〜2 でそのボールへカメラを寄せる。</summary>
        public void Focus(int index) => focus = Mathf.Clamp(index, -1, balls.Length - 1);

        void Build()
        {
            cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.12f);
            cam.fieldOfView = 40f;
            PostEffects.Apply(cam);

            SceneLighting.Apply(new SceneLighting.Preset
            {
                sunEuler = new Vector3(50f, -30f, 0f), sun = new Color(1f, 0.96f, 0.9f), intensity = 1.2f,
                sky = new Color(0.35f, 0.4f, 0.6f), equator = new Color(0.2f, 0.22f, 0.3f), ground = new Color(0.08f, 0.08f, 0.1f)
            });

            Geo.MeshObject("Floor", transform, MeshFactory.Disc(30f, 64), Mats.Lit(new Color(0.08f, 0.1f, 0.18f), 0.75f));
            var rings = new MeshBuilder(0.01f);
            for (int i = 1; i <= 6; i++) rings.Arc(Vector2.zero, i * 3f, 0f, 360f, 0.05f, 96);
            rings.Build("Rings", transform, Mats.Unlit(new Color(0.2f, 0.35f, 0.6f)));

            for (int i = 0; i < SportCatalog.All.Length; i++) BuildPedestal(i, SportCatalog.All[i]);
        }

        void BuildPedestal(int i, Sport sport)
        {
            Color color = SportCatalog.ThemeColor(sport);
            float a = Mathf.Deg2Rad * (90f - (i - 1) * 40f);
            Vector3 p = new Vector3(Mathf.Cos(a) * 6f, 0f, Mathf.Sin(a) * 6f - 4f);
            Geo.Cylinder(transform, p + Vector3.up * 0.4f, 2.2f, 0.8f, Mats.Lit(new Color(0.85f, 0.88f, 0.95f), 0.6f, 0.3f), "Pedestal");
            Geo.Cylinder(transform, p + Vector3.up * 0.82f, 2.0f, 0.04f, Mats.Unlit(color), "Glow", false);
            var ball = Geo.DetailedSphere(transform, p + Vector3.up * (0.95f + BallSizes[i] * 0.5f), BallSizes[i], Mats.LitTex(ProcTextures.Ball(sport), Color.white, 0.4f), sport.ToString());
            balls[i] = ball.transform;
            ballHome[i] = ball.transform.position;
            var light = new GameObject("Light").AddComponent<Light>();
            light.transform.SetParent(transform, false);
            light.transform.position = p + new Vector3(0f, 3.2f, -1.5f);
            light.type = LightType.Point;
            light.color = Color.Lerp(color, Color.white, 0.4f);
            light.range = 7f;
            light.intensity = 3f;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            time += dt;
            for (int i = 0; i < balls.Length; i++)
            {
                balls[i].Rotate(Vector3.up, (focus == i ? 90f : 35f) * dt, Space.World);
                balls[i].Rotate(Vector3.right, 12f * dt, Space.Self);
                balls[i].position = ballHome[i] + Vector3.up * (Mathf.Sin(time * 1.6f + i) * 0.12f + (focus == i ? 0.3f : 0f));
            }
            float yaw = Mathf.Sin(time * 0.15f) * 12f;
            // 右側に設定パネルが出るので、選んだボールは画面の左寄りに映す
            Vector3 look = focus >= 0 ? ballHome[focus] + Quaternion.Euler(0f, yaw * 0.5f, 0f) * Vector3.right * 2.2f : new Vector3(0f, 1.6f, 1.2f);
            Vector3 camPos = focus >= 0
                ? ballHome[focus] + Quaternion.Euler(0f, yaw * 0.5f, 0f) * new Vector3(0f, 1.6f, -6.5f)
                : Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 4.2f, -13f);
            var t = cam.transform;
            t.position = Vector3.Lerp(t.position, camPos, 1f - Mathf.Exp(-2.5f * dt));
            t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(look - t.position), 1f - Mathf.Exp(-3f * dt));
        }
    }
}
