using UnityEngine;

namespace SportsGame
{
    /// <summary>ビーチ会場(砂浜・海・ネット・アンテナ・ヤシの木・観客席)を作る。</summary>
    public static class VolleyballCourtBuilder
    {
        const float LineWidth = 0.06f;
        const float PostX = 4.9f;

        public static Transform[] Build(Transform root, NetCollider net)
        {
            var env = Geo.Empty("Environment", root).transform;
            BuildBeach(env);
            BuildLines(env);
            BuildNet(env, net);
            BuildPalms(env);
            Geo.MakeStatic(env.gameObject);

            var dyn = Geo.Empty("Dynamic", root).transform;
            return new[]
            {
                StandsBuilder.Build(dyn, new Vector3(-10.5f, 0f, 0f), Vector3.right, 22f, 7, new Color(0.2f, 0.45f, 0.65f), 31),
                StandsBuilder.Build(dyn, new Vector3(10.5f, 0f, 0f), Vector3.left, 22f, 7, new Color(0.2f, 0.45f, 0.65f), 32),
                StandsBuilder.Build(dyn, new Vector3(0f, 0f, 14f), Vector3.back, 16f, 6, new Color(0.85f, 0.5f, 0.2f), 33),
            };
        }

        static void BuildBeach(Transform env)
        {
            var sand = new MeshBuilder(0f);
            sand.FilledRect(-40f, -30f, 40f, 30f);
            sand.Build("Sand", env, Mats.LitTex(ProcTextures.Noise("sand", new Color(0.86f, 0.76f, 0.56f), new Color(0.93f, 0.85f, 0.66f), 14f, 0.12f), Color.white, 0.1f, new Vector2(16f, 12f)));
            var sea = new MeshBuilder(0.02f);
            sea.FilledRect(-120f, 30f, 120f, 160f);
            sea.Build("Sea", env, Mats.Lit(new Color(0.1f, 0.45f, 0.7f), 0.85f));
            var shore = new MeshBuilder(0.015f);
            shore.FilledRect(-120f, 28.5f, 120f, 31f);
            shore.Build("Shore", env, Mats.Unlit(new Color(0.95f, 0.97f, 1f)));
        }

        static void BuildLines(Transform env)
        {
            var lines = new MeshBuilder(0.01f);
            lines.RectOutline(-VolleyballDims.HalfWidth, -VolleyballDims.HalfLength, VolleyballDims.HalfWidth, VolleyballDims.HalfLength, LineWidth);
            lines.Build("Lines", env, Mats.Unlit(new Color(0.15f, 0.35f, 0.85f)));
        }

        static void BuildNet(Transform env, NetCollider net)
        {
            Material post = Mats.Lit(new Color(0.85f, 0.2f, 0.2f), 0.5f, 0.2f);
            float postHeight = VolleyballDims.NetTop + 0.1f;
            Geo.Cylinder(env, new Vector3(-PostX, postHeight * 0.5f, 0f), 0.12f, postHeight, post, "PostL");
            Geo.Cylinder(env, new Vector3(PostX, postHeight * 0.5f, 0f), 0.12f, postHeight, post, "PostR");
            Material cable = Mats.Lit(new Color(0.2f, 0.2f, 0.2f), 0.3f);
            Geo.Box(env, new Vector3(0f, VolleyballDims.NetTop, 0f), new Vector3(PostX * 2f, 0.015f, 0.015f), cable, "TopCable", false);

            float w = net.halfWidth;
            var body = MeshFactory.VerticalStrip(-w, w, 8, x => VolleyballDims.NetBottom, x => VolleyballDims.NetTop - 0.06f, 0.1f);
            Geo.MeshObject("Net", env, body, Mats.Cutout(ProcTextures.Grid(new Color(0.08f, 0.08f, 0.1f), 32, 3), Color.white, Vector2.one), true);
            var tape = MeshFactory.VerticalStrip(-w, w, 8, x => VolleyballDims.NetTop - 0.07f, x => VolleyballDims.NetTop, 1f);
            Geo.MeshObject("NetTape", env, tape, Mats.Unlit(new Color(0.15f, 0.35f, 0.85f)));

            // アンテナ(赤白の縞)
            Material red = Mats.Unlit(new Color(0.9f, 0.15f, 0.15f)), white = Mats.Unlit(Color.white);
            foreach (float x in new[] { -VolleyballDims.AntennaX, VolleyballDims.AntennaX })
                for (int i = 0; i < 8; i++)
                    Geo.Cylinder(env, new Vector3(x, VolleyballDims.NetBottom + 0.1f + i * 0.2f, 0f), 0.025f, 0.2f, i % 2 == 0 ? red : white, "Antenna", false);
        }

        static void BuildPalms(Transform env)
        {
            Material trunk = Mats.Lit(new Color(0.45f, 0.32f, 0.2f), 0.1f);
            Material leaf = Mats.Lit(new Color(0.2f, 0.55f, 0.25f), 0.2f);
            Vector3[] spots = { new Vector3(-16f, 0f, 20f), new Vector3(15f, 0f, 22f), new Vector3(-22f, 0f, 6f), new Vector3(22f, 0f, -4f), new Vector3(-20f, 0f, -14f), new Vector3(19f, 0f, 12f) };
            var rng = new System.Random(7);
            foreach (var s in spots)
            {
                float h = 5f + (float)rng.NextDouble() * 2f;
                var palm = Geo.Empty("Palm", env, s).transform;
                Geo.Cylinder(palm, new Vector3(0f, h * 0.5f, 0f), 0.35f, h, trunk, "Trunk");
                for (int i = 0; i < 6; i++)
                {
                    var l = Geo.Box(palm, new Vector3(0f, h, 0f), new Vector3(0.5f, 0.06f, 2.6f), leaf, "Leaf", false);
                    l.transform.localRotation = Quaternion.Euler(18f, i * 60f, 0f);
                    l.transform.localPosition = new Vector3(0f, h, 0f) + l.transform.localRotation * new Vector3(0f, 0f, 1.1f);
                }
            }
        }
    }
}
