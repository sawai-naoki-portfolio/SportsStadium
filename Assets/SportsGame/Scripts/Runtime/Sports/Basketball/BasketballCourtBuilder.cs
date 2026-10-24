using UnityEngine;

namespace SportsGame
{
    /// <summary>体育館・ハーフコート・ゴールの見た目を作る(当たり判定は BasketballDims.CreateColliders)。</summary>
    public static class BasketballCourtBuilder
    {
        const float LineWidth = 0.05f;

        public struct Result
        {
            public Transform net;
            public Transform[] crowds;
        }

        public static Result Build(Transform root, Color teamColor)
        {
            var r = new Result();
            var env = Geo.Empty("Environment", root).transform;
            Color paint = Color.Lerp(teamColor, new Color(0.1f, 0.1f, 0.2f), 0.45f);
            BuildFloor(env, paint);
            BuildLines(env);
            BuildHoop(env);
            BuildGym(env, teamColor);
            Geo.MakeStatic(env.gameObject);

            // ネットと観客は動くので静的バッチから外す
            var dyn = Geo.Empty("Dynamic", root).transform;
            r.net = BuildNet(dyn);
            r.crowds = new[]
            {
                StandsBuilder.Build(dyn, new Vector3(-9.6f, 0f, -5.5f), Vector3.right, 24f, 8, new Color(0.25f, 0.28f, 0.4f), 21),
                StandsBuilder.Build(dyn, new Vector3(9.6f, 0f, -5.5f), Vector3.left, 24f, 8, new Color(0.25f, 0.28f, 0.4f), 22),
                StandsBuilder.Build(dyn, new Vector3(0f, 0f, -15.5f), Vector3.forward, 16f, 6, new Color(0.4f, 0.22f, 0.22f), 23),
            };
            return r;
        }

        static void BuildFloor(Transform env, Color paint)
        {
            var floor = new MeshBuilder(0f);
            floor.FilledRect(-16f, -22f, 16f, 8f);
            floor.Build("Floor", env, Mats.LitTex(ProcTextures.WoodFloor(), new Color(0.85f, 0.78f, 0.7f), 0.55f, new Vector2(6f, 6f)));
            var court = new MeshBuilder(0.002f);
            court.FilledRect(-BasketballDims.HalfWidth, BasketballDims.HalfLineZ - 1.5f, BasketballDims.HalfWidth, BasketballDims.BaselineZ);
            court.Build("CourtWood", env, Mats.LitTex(ProcTextures.WoodFloor(), Color.white, 0.6f, new Vector2(3f, 3f)));
            var key = new MeshBuilder(0.004f);
            key.FilledRect(-BasketballDims.KeyHalfWidth, BasketballDims.FreeThrowZ, BasketballDims.KeyHalfWidth, BasketballDims.BaselineZ);
            key.Disc(new Vector2(0f, BasketballDims.HalfLineZ), 1.8f, 32, 0f, 180f);
            key.Build("Paint", env, Mats.Lit(paint, 0.5f));
        }

        static void BuildLines(Transform env)
        {
            const float W = BasketballDims.HalfWidth, B = BasketballDims.BaselineZ, H = BasketballDims.HalfLineZ, K = BasketballDims.KeyHalfWidth;
            var lines = new MeshBuilder(0.006f);
            lines.Line(-W, B, W, B, LineWidth);
            lines.Line(-W, H, W, H, LineWidth);
            lines.Line(-W, H, -W, B, LineWidth);
            lines.Line(W, H, W, B, LineWidth);
            lines.RectOutline(-K, BasketballDims.FreeThrowZ, K, B, LineWidth);
            lines.Arc(new Vector2(0f, BasketballDims.FreeThrowZ), 1.8f, 0f, 360f, LineWidth, 48);
            float cornerAngle = Mathf.Atan2(BasketballDims.CornerZ, BasketballDims.CornerX) * Mathf.Rad2Deg + 360f;
            lines.Arc(Vector2.zero, BasketballDims.ThreeRadius, 540f - cornerAngle, cornerAngle, LineWidth, 64);
            lines.Line(-BasketballDims.CornerX, BasketballDims.CornerZ, -BasketballDims.CornerX, B, LineWidth);
            lines.Line(BasketballDims.CornerX, BasketballDims.CornerZ, BasketballDims.CornerX, B, LineWidth);
            lines.Arc(Vector2.zero, 1.25f, 180f, 360f, LineWidth, 24);
            lines.Arc(new Vector2(0f, H), 1.8f, 0f, 180f, LineWidth, 32);
            for (int i = 0; i < 4; i++)
            {
                float z = B - 1.75f - i * 0.85f;
                lines.Line(-K - 0.1f, z, -K, z, LineWidth * 2f);
                lines.Line(K, z, K + 0.1f, z, LineWidth * 2f);
            }
            lines.Build("Lines", env, Mats.Unlit(new Color(0.97f, 0.97f, 0.97f)));
        }

        static void BuildHoop(Transform env)
        {
            Material padding = Mats.Lit(new Color(0.15f, 0.2f, 0.45f), 0.3f);
            Material metal = Mats.Lit(new Color(0.2f, 0.2f, 0.22f), 0.6f, 0.6f);
            Material orange = Mats.Lit(new Color(0.95f, 0.35f, 0.05f), 0.6f, 0.4f);

            Geo.Box(env, BasketballDims.BaseCenter, BasketballDims.BaseSize, padding, "StanchionBase");
            Geo.Box(env, BasketballDims.PoleCenter, BasketballDims.PoleSize, padding, "Pole");
            Geo.Box(env, new Vector3(0f, 3.85f, 1.9f), new Vector3(0.2f, 0.2f, 2.9f), metal, "Arm");

            // バックボード(下端 2.90m、幅 1.8m、高さ 1.05m)
            Geo.Box(env, BasketballDims.BoardCenter, BasketballDims.BoardSize, Mats.Lit(new Color(0.96f, 0.96f, 0.98f), 0.8f), "Backboard");
            var mb = new MeshBuilder(0f);
            mb.RectOutline(-0.87f, 2.93f, 0.87f, 3.92f, 0.05f);
            mb.RectOutline(-0.295f, 3.2f, 0.295f, 3.65f, 0.05f);
            var boardLines = mb.Build("BoardLines", env, Mats.Unlit(new Color(0.85f, 0.15f, 0.12f)));
            boardLines.transform.localPosition = new Vector3(0f, 0f, BasketballDims.BoardFrontZ - 0.003f);
            boardLines.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            var rim = Geo.MeshObject("Rim", env, MeshFactory.Torus(BasketballDims.RimRadius, BasketballDims.RimTube, 40, 8), orange, true);
            rim.transform.localPosition = BasketballDims.Hoop;
            float bracketLen = BasketballDims.BoardFrontZ - BasketballDims.RimRadius;
            Geo.Box(env, new Vector3(0f, BasketballDims.Hoop.y - 0.02f, BasketballDims.RimRadius + bracketLen * 0.5f), new Vector3(0.12f, 0.03f, bracketLen), orange, "RimBracket");
        }

        static Transform BuildNet(Transform parent)
        {
            var net = Geo.MeshObject("Net", parent, MeshFactory.Frustum(BasketballDims.RimRadius - 0.005f, 0.15f, 0.42f, 24),
                Mats.Cutout(ProcTextures.DiamondNet(), Color.white, new Vector2(6f, 2f)));
            net.transform.localPosition = BasketballDims.Hoop;
            return net.transform;
        }

        static void BuildGym(Transform env, Color teamColor)
        {
            Material wall = Mats.Lit(new Color(0.16f, 0.18f, 0.26f), 0.1f);
            Geo.Box(env, new Vector3(0f, 6f, 8f), new Vector3(32f, 12f, 0.3f), wall, "WallBack", false);
            Geo.Box(env, new Vector3(0f, 1f, 7.8f), new Vector3(32f, 2f, 0.2f), Mats.Lit(teamColor, 0.3f), "WallTrim", false);
            Geo.Box(env, new Vector3(0f, 6f, -22f), new Vector3(32f, 12f, 0.3f), wall, "WallFront", false);
            Geo.Box(env, new Vector3(-16f, 6f, -7f), new Vector3(0.3f, 12f, 30f), wall, "WallL", false);
            Geo.Box(env, new Vector3(16f, 6f, -7f), new Vector3(0.3f, 12f, 30f), wall, "WallR", false);
            Geo.Box(env, new Vector3(0f, 12f, -7f), new Vector3(32f, 0.2f, 30f), Mats.Lit(new Color(0.1f, 0.1f, 0.14f), 0f), "Ceiling", false);
            Material lamp = Mats.Unlit(new Color(1f, 0.98f, 0.9f));
            for (int x = -1; x <= 1; x++)
                for (int z = 0; z < 3; z++)
                    Geo.Box(env, new Vector3(x * 6f, 11.5f, -1f - z * 6f), new Vector3(2.4f, 0.15f, 1.2f), lamp, "Lamp", false);
            Color[] banners = { teamColor, new Color(0.95f, 0.8f, 0.2f), new Color(0.9f, 0.3f, 0.25f), new Color(0.95f, 0.95f, 0.95f) };
            for (int i = 0; i < 5; i++)
                Geo.Box(env, new Vector3(-10f + i * 5f, 7.5f, 7.8f), new Vector3(3.6f, 1.6f, 0.1f), Mats.Unlit(banners[i % banners.Length]), "Banner", false);
        }
    }
}
