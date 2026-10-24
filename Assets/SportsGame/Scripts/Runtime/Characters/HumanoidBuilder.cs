using UnityEngine;

namespace SportsGame
{
    /// <summary>プリミティブを組み合わせて人型キャラクターの階層を作る。</summary>
    public static class HumanoidBuilder
    {
        public const float StandingHeight = 1.86f;

        public static Humanoid Build(Transform parent, string name, HumanoidLook look)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            float scale = look.height <= 0f ? 1f : look.height;

            var m = new Materials(look);
            var rig = new HumanoidRig();
            rig.body = Geo.Empty("Body", root.transform).transform;
            rig.body.localScale = Vector3.one * scale;

            rig.hips = Geo.Empty("Hips", rig.body, new Vector3(0f, HumanoidRig.LegLength, 0f)).transform;
            Geo.Prim(PrimitiveType.Capsule, rig.hips, new Vector3(0f, -0.02f, 0f), new Vector3(0.36f, 0.12f, 0.24f), m.shorts, "Pelvis");

            rig.spine = Geo.Empty("Spine", rig.hips, new Vector3(0f, 0.04f, 0f)).transform;
            Geo.Prim(PrimitiveType.Capsule, rig.spine, new Vector3(0f, 0.27f, 0f), new Vector3(0.40f, 0.28f, 0.25f), m.jersey, "Torso");
            Geo.Prim(PrimitiveType.Cube, rig.spine, new Vector3(0f, 0.30f, 0.118f), new Vector3(0.05f, 0.30f, 0.01f), m.trim, "Stripe", false);
            Geo.Prim(PrimitiveType.Capsule, rig.spine, new Vector3(0f, 0.50f, 0f), new Vector3(0.46f, 0.08f, 0.24f), m.jersey, "Shoulders");

            rig.head = Geo.Empty("Head", rig.spine, new Vector3(0f, 0.6f, 0f)).transform;
            BuildHead(rig.head, look, m);

            BuildArm(rig.spine, -1f, m, out rig.shL, out rig.elL, out rig.handL);
            BuildArm(rig.spine, 1f, m, out rig.shR, out rig.elR, out rig.handR);
            BuildLeg(rig.hips, -1f, m, out rig.hipL, out rig.knL, out rig.footL);
            BuildLeg(rig.hips, 1f, m, out rig.hipR, out rig.knR, out rig.footR);
            if (look.racket) BuildRacket(rig.handR, look.accent);

            var h = root.AddComponent<Humanoid>();
            h.Init(rig, StandingHeight * scale);
            return h;
        }

        sealed class Materials
        {
            public readonly Material skin, jersey, trim, shorts, shoes, hair, dark, sock;

            public Materials(HumanoidLook look)
            {
                skin = Mats.Lit(look.skin, 0.35f);
                jersey = Mats.Lit(look.jersey, 0.3f);
                trim = Mats.Lit(look.trim, 0.3f);
                shorts = Mats.Lit(look.shorts, 0.3f);
                shoes = Mats.Lit(look.shoes, 0.45f);
                hair = Mats.Lit(look.hair, 0.15f);
                dark = Mats.Lit(new Color(0.08f, 0.08f, 0.1f), 0.6f);
                sock = Mats.Lit(Color.white, 0.2f);
            }
        }

        static void BuildHead(Transform head, HumanoidLook look, Materials m)
        {
            Geo.Prim(PrimitiveType.Cylinder, head, Vector3.zero, new Vector3(0.1f, 0.05f, 0.1f), m.skin, "Neck");
            Geo.Prim(PrimitiveType.Sphere, head, new Vector3(0f, 0.13f, 0.01f), new Vector3(0.22f, 0.25f, 0.23f), m.skin, "Face");
            Geo.Prim(PrimitiveType.Sphere, head, new Vector3(0f, 0.18f, -0.015f), new Vector3(0.235f, 0.2f, 0.235f), m.hair, "Hair");
            if (look.longHair)
                Geo.Prim(PrimitiveType.Capsule, head, new Vector3(0f, 0.08f, -0.12f), new Vector3(0.09f, 0.1f, 0.07f), m.hair, "Ponytail");
            if (look.headband)
                Geo.Prim(PrimitiveType.Cylinder, head, new Vector3(0f, 0.19f, 0f), new Vector3(0.24f, 0.018f, 0.24f), m.trim, "Headband", false);
            Geo.Prim(PrimitiveType.Sphere, head, new Vector3(-0.045f, 0.14f, 0.11f), Vector3.one * 0.03f, m.dark, "EyeL", false);
            Geo.Prim(PrimitiveType.Sphere, head, new Vector3(0.045f, 0.14f, 0.11f), Vector3.one * 0.03f, m.dark, "EyeR", false);
        }

        static void BuildArm(Transform spine, float side, Materials m, out Transform sh, out Transform el, out Transform hand)
        {
            sh = Geo.Empty(side < 0f ? "ShoulderL" : "ShoulderR", spine, new Vector3(0.235f * side, 0.49f, 0f)).transform;
            Geo.Prim(PrimitiveType.Sphere, sh, Vector3.zero, Vector3.one * 0.13f, m.jersey, "Sleeve");
            Geo.Prim(PrimitiveType.Capsule, sh, new Vector3(0f, -0.14f, 0f), new Vector3(0.085f, 0.15f, 0.085f), m.skin, "UpperArm");
            el = Geo.Empty("Elbow", sh, new Vector3(0f, -0.29f, 0f)).transform;
            Geo.Prim(PrimitiveType.Capsule, el, new Vector3(0f, -0.13f, 0f), new Vector3(0.075f, 0.14f, 0.075f), m.skin, "Forearm");
            hand = Geo.Empty("Hand", el, new Vector3(0f, -0.28f, 0f)).transform;
            Geo.Prim(PrimitiveType.Sphere, hand, Vector3.zero, new Vector3(0.08f, 0.1f, 0.06f), m.skin, "Palm");
        }

        static void BuildLeg(Transform hips, float side, Materials m, out Transform hip, out Transform knee, out Transform foot)
        {
            hip = Geo.Empty(side < 0f ? "HipL" : "HipR", hips, new Vector3(0.1f * side, -0.02f, 0f)).transform;
            Geo.Prim(PrimitiveType.Capsule, hip, new Vector3(0f, -0.12f, 0f), new Vector3(0.155f, 0.14f, 0.155f), m.shorts, "Shorts");
            Geo.Prim(PrimitiveType.Capsule, hip, new Vector3(0f, -0.25f, 0f), new Vector3(0.12f, 0.2f, 0.12f), m.skin, "Thigh");
            knee = Geo.Empty("Knee", hip, new Vector3(0f, -0.44f, 0f)).transform;
            Geo.Prim(PrimitiveType.Capsule, knee, new Vector3(0f, -0.2f, 0f), new Vector3(0.1f, 0.21f, 0.1f), m.skin, "Shin");
            Geo.Prim(PrimitiveType.Cylinder, knee, new Vector3(0f, -0.33f, 0f), new Vector3(0.105f, 0.06f, 0.105f), m.sock, "Sock", false);
            foot = Geo.Empty("Foot", knee, new Vector3(0f, -0.43f, 0f)).transform;
            Geo.Prim(PrimitiveType.Cube, foot, new Vector3(0f, -0.02f, 0.05f), new Vector3(0.11f, 0.08f, 0.27f), m.shoes, "Shoe");
        }

        static void BuildRacket(Transform hand, Color accent)
        {
            var grip = Geo.Empty("Racket", hand).transform;
            Material frame = Mats.Lit(accent, 0.6f, 0.2f);
            Material black = Mats.Lit(new Color(0.1f, 0.1f, 0.1f), 0.3f);
            Geo.Prim(PrimitiveType.Cylinder, grip, new Vector3(0f, -0.1f, 0f), new Vector3(0.035f, 0.1f, 0.035f), black, "Grip");
            Geo.Prim(PrimitiveType.Cylinder, grip, new Vector3(0f, -0.27f, 0f), new Vector3(0.02f, 0.08f, 0.02f), frame, "Throat");
            var head = Geo.Empty("RacketHead", grip, new Vector3(0f, -0.48f, 0f)).transform;
            var ring = Geo.MeshObject("Frame", head, MeshFactory.Torus(0.13f, 0.012f, 24, 6), frame, true);
            ring.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            ring.transform.localScale = new Vector3(1f, 1f, 0.78f);
            var strings = Geo.Prim(PrimitiveType.Cylinder, head, Vector3.zero, new Vector3(0.25f, 0.003f, 0.195f),
                Mats.Cutout(ProcTextures.Grid(new Color(0.95f, 0.95f, 0.9f), 32, 3), Color.white, new Vector2(6f, 5f)), "Strings", false);
            strings.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }
}
