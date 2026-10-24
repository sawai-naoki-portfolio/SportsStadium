using UnityEngine;

namespace SportsGame
{
    /// <summary>テニス会場(ハードコート・ネット・フェンス・観客席・審判台)を作る。</summary>
    public static class TennisCourtBuilder
    {
        const float LineWidth = 0.05f;

        /// <summary>会場を作り、歓声演出用の観客 Transform を返す。</summary>
        public static Transform[] Build(Transform root, NetCollider net)
        {
            var env = Geo.Empty("Environment", root).transform;
            BuildSurface(env);
            BuildLines(env);
            BuildNet(env, net);
            BuildFences(env);
            Geo.MakeStatic(env.gameObject);

            // 観客と審判は動くので静的バッチから外す
            var dyn = Geo.Empty("Dynamic", root).transform;
            BuildUmpire(dyn);
            return new[]
            {
                StandsBuilder.Build(dyn, new Vector3(-12.2f, 0f, 0f), Vector3.right, 34f, 9, new Color(0.25f, 0.3f, 0.45f), 1),
                StandsBuilder.Build(dyn, new Vector3(12.2f, 0f, 0f), Vector3.left, 34f, 9, new Color(0.25f, 0.3f, 0.45f), 2),
                StandsBuilder.Build(dyn, new Vector3(0f, 0f, 19.6f), Vector3.back, 24f, 10, new Color(0.45f, 0.2f, 0.2f), 3),
            };
        }

        static void BuildSurface(Transform env)
        {
            var apron = new MeshBuilder(0f);
            apron.FilledRect(-22f, -32f, 22f, 32f);
            apron.Build("Apron", env, Mats.LitTex(ProcTextures.Noise("apron", new Color(0.16f, 0.42f, 0.3f), new Color(0.2f, 0.48f, 0.34f), 6f), Color.white, 0.25f, new Vector2(8f, 8f)));
            var court = new MeshBuilder(0.002f);
            court.FilledRect(-TennisDims.DoublesHalfWidth - 0.6f, -TennisDims.HalfLength - 0.9f, TennisDims.DoublesHalfWidth + 0.6f, TennisDims.HalfLength + 0.9f);
            court.Build("Court", env, Mats.LitTex(ProcTextures.Noise("hard", new Color(0.15f, 0.34f, 0.6f), new Color(0.18f, 0.38f, 0.65f), 10f, 0.1f), Color.white, 0.3f, new Vector2(6f, 10f)));
        }

        static void BuildLines(Transform env)
        {
            const float L = TennisDims.HalfLength, S = TennisDims.SinglesHalfWidth, D = TennisDims.DoublesHalfWidth, SL = TennisDims.ServiceLine;
            var lines = new MeshBuilder(0.005f);
            lines.RectOutline(-D, -L, D, L, LineWidth);
            lines.Line(-S, -L, -S, L, LineWidth);
            lines.Line(S, -L, S, L, LineWidth);
            lines.Line(-S, -SL, S, -SL, LineWidth);
            lines.Line(-S, SL, S, SL, LineWidth);
            lines.Line(0f, -SL, 0f, SL, LineWidth);
            lines.Line(0f, -L, 0f, -L + 0.15f, LineWidth);
            lines.Line(0f, L, 0f, L - 0.15f, LineWidth);
            lines.Build("Lines", env, Mats.Unlit(new Color(0.96f, 0.96f, 0.96f)));
        }

        static void BuildNet(Transform env, NetCollider net)
        {
            Material post = Mats.Lit(new Color(0.12f, 0.2f, 0.15f), 0.5f, 0.3f);
            float postX = TennisDims.NetHalfWidth + 0.05f;
            Geo.Cylinder(env, new Vector3(-postX, TennisDims.NetPostHeight * 0.5f + 0.02f, 0f), 0.1f, TennisDims.NetPostHeight + 0.04f, post, "PostL");
            Geo.Cylinder(env, new Vector3(postX, TennisDims.NetPostHeight * 0.5f + 0.02f, 0f), 0.1f, TennisDims.NetPostHeight + 0.04f, post, "PostR");

            float w = TennisDims.NetHalfWidth;
            var mesh = MeshFactory.VerticalStrip(-w, w, 32, x => 0.03f, x => net.TopAt(x) - 0.06f, 0.045f);
            Geo.MeshObject("Net", env, mesh, Mats.Cutout(ProcTextures.Grid(new Color(0.08f, 0.08f, 0.08f), 32, 4), Color.white, Vector2.one), true);
            var tape = MeshFactory.VerticalStrip(-w, w, 32, x => net.TopAt(x) - 0.06f, x => net.TopAt(x), 1f);
            Geo.MeshObject("NetTape", env, tape, Mats.Unlit(new Color(0.97f, 0.97f, 0.97f)));
            Geo.Box(env, new Vector3(0f, TennisDims.NetCenterHeight * 0.5f, 0f), new Vector3(0.05f, TennisDims.NetCenterHeight, 0.012f), Mats.Unlit(new Color(0.95f, 0.95f, 0.95f)), "CenterStrap", false);
        }

        static void BuildFences(Transform env)
        {
            Material fence = Mats.Lit(new Color(0.1f, 0.25f, 0.2f), 0.1f);
            Geo.Box(env, new Vector3(0f, 1.5f, 19f), new Vector3(30f, 3f, 0.2f), fence, "FenceFar", false);
            Geo.Box(env, new Vector3(0f, 1.5f, -21f), new Vector3(30f, 3f, 0.2f), fence, "FenceNear", false);
            Geo.Box(env, new Vector3(-11.5f, 0.6f, 0f), new Vector3(0.2f, 1.2f, 38f), fence, "FenceL", false);
            Geo.Box(env, new Vector3(11.5f, 0.6f, 0f), new Vector3(0.2f, 1.2f, 38f), fence, "FenceR", false);
            Color[] ads = { new Color(0.9f, 0.3f, 0.2f), new Color(0.95f, 0.8f, 0.2f), new Color(0.2f, 0.5f, 0.9f), new Color(0.95f, 0.95f, 0.95f) };
            for (int i = 0; i < 6; i++)
                Geo.Box(env, new Vector3(-12.5f + i * 5f, 0.5f, 18.85f), new Vector3(4.8f, 0.9f, 0.05f), Mats.Unlit(ads[i % ads.Length]), "Ad", false);
            Material bench = Mats.Lit(new Color(0.3f, 0.3f, 0.35f), 0.3f);
            Geo.Box(env, new Vector3(-8.2f, 0.45f, -2.2f), new Vector3(0.6f, 0.1f, 1.6f), bench, "Bench");
            Geo.Box(env, new Vector3(-8.2f, 0.45f, 2.2f), new Vector3(0.6f, 0.1f, 1.6f), bench, "Bench");
        }

        static void BuildUmpire(Transform parent)
        {
            Material chair = Mats.Lit(new Color(0.15f, 0.35f, 0.25f), 0.2f);
            var ch = Geo.Empty("UmpireChair", parent, new Vector3(-7.3f, 0f, 0f)).transform;
            Geo.Box(ch, new Vector3(0f, 1f, 0f), new Vector3(0.8f, 2f, 0.8f), chair, "Base");
            Geo.Box(ch, new Vector3(0f, 2.2f, 0f), new Vector3(0.9f, 0.1f, 0.9f), chair, "Seat");
            var umpire = HumanoidBuilder.Build(ch, "Umpire", HumanoidLook.Make(new Color(0.15f, 0.2f, 0.35f), new Color(0.85f, 0.85f, 0.8f), 99));
            umpire.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            umpire.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            umpire.crouch = 0.9f;
        }
    }
}
