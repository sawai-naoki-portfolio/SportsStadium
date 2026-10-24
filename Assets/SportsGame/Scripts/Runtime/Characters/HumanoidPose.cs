using UnityEngine;

namespace SportsGame
{
    /// <summary>各関節のローカル回転(度)と腰の上下オフセット。</summary>
    public struct HumanoidPose
    {
        public float hipsY;
        public Vector3 spine, head, shL, elL, shR, elR, hipL, knL, hipR, knR;

        public static HumanoidPose Lerp(HumanoidPose a, HumanoidPose b, float t)
        {
            return new HumanoidPose
            {
                hipsY = Mathf.Lerp(a.hipsY, b.hipsY, t),
                spine = Vector3.Lerp(a.spine, b.spine, t),
                head = Vector3.Lerp(a.head, b.head, t),
                shL = Vector3.Lerp(a.shL, b.shL, t),
                elL = Vector3.Lerp(a.elL, b.elL, t),
                shR = Vector3.Lerp(a.shR, b.shR, t),
                elR = Vector3.Lerp(a.elR, b.elR, t),
                hipL = Vector3.Lerp(a.hipL, b.hipL, t),
                knL = Vector3.Lerp(a.knL, b.knL, t),
                hipR = Vector3.Lerp(a.hipR, b.hipR, t),
                knR = Vector3.Lerp(a.knR, b.knR, t),
            };
        }
    }

    /// <summary>移動状態から求める基本姿勢の入力。</summary>
    public struct LocomotionState
    {
        public float speed, crouch, runPhase, dribblePhase;
        public bool airborne, dribbling, dribbleLeft, holdBall, readyRacket;
    }

    /// <summary>
    /// 手続き的アニメーションのポーズ定義。関節の回転規約: x 負 = 前へ振る(腕・脚)、肘は負で曲がり、膝は正で曲がる。
    /// </summary>
    public static class HumanoidPoses
    {
        public const float RunReferenceSpeed = 6.5f;

        public static HumanoidPose Locomotion(in LocomotionState s)
        {
            var p = new HumanoidPose();
            float run = Mathf.Clamp01(s.speed / RunReferenceSpeed);
            float sin = Mathf.Sin(s.runPhase), cos = Mathf.Cos(s.runPhase);
            float stride = s.speed > 0.15f ? Mathf.Lerp(18f, 48f, run) : 0f;
            float cr = Mathf.Clamp01(s.crouch);

            p.hipL = new Vector3(-sin * stride - cr * 40f, 0f, -3f);
            p.hipR = new Vector3(sin * stride - cr * 40f, 0f, 3f);
            p.knL = new Vector3(cr * 70f + stride * 0.9f * Mathf.Max(0f, cos), 0f, 0f);
            p.knR = new Vector3(cr * 70f + stride * 0.9f * Mathf.Max(0f, -cos), 0f, 0f);
            p.shL = new Vector3(sin * stride * 0.9f - cr * 20f, 0f, -10f);
            p.shR = new Vector3(-sin * stride * 0.9f - cr * 20f, 0f, 10f);
            p.elL = p.elR = new Vector3(-25f - run * 55f - cr * 30f, 0f, 0f);
            p.spine = new Vector3(run * 12f + cr * 22f, 0f, 0f);
            p.head = new Vector3(-run * 6f - cr * 12f, 0f, 0f);
            p.hipsY = -cr * 0.16f + Mathf.Abs(sin) * 0.035f * run;

            if (s.airborne)
            {
                p.hipL = new Vector3(-30f, 0f, -4f);
                p.hipR = new Vector3(-10f, 0f, 4f);
                p.knL = new Vector3(55f, 0f, 0f);
                p.knR = new Vector3(30f, 0f, 0f);
                p.hipsY = 0f;
            }

            if (s.dribbling) Dribble(ref p, s.dribbleLeft, Mathf.Sin(s.dribblePhase));
            else if (s.holdBall)
            {
                p.shL = new Vector3(-45f, 0f, 8f);
                p.shR = new Vector3(-45f, 0f, -8f);
                p.elL = p.elR = new Vector3(-70f, 0f, 0f);
            }
            else if (s.readyRacket)
            {
                p.shR = new Vector3(-35f, 0f, 30f);
                p.elR = new Vector3(-55f, 0f, 0f);
                p.shL = new Vector3(-40f, 0f, -10f);
                p.elL = new Vector3(-70f, 0f, 0f);
            }
            return p;
        }

        static void Dribble(ref HumanoidPose p, bool left, float d)
        {
            Vector3 dribbleSh = new Vector3(-30f + d * 10f, 0f, left ? -18f : 18f);
            Vector3 dribbleEl = new Vector3(-35f - d * 25f, 0f, 0f);
            Vector3 guardSh = new Vector3(-45f, 0f, left ? 35f : -35f);
            Vector3 guardEl = new Vector3(-60f, 0f, 0f);
            if (left) { p.shL = dribbleSh; p.elL = dribbleEl; p.shR = guardSh; p.elR = guardEl; }
            else { p.shR = dribbleSh; p.elR = dribbleEl; p.shL = guardSh; p.elL = guardEl; }
            p.spine.x += 8f;
        }

        static float Ease(float t) => t * t * (3f - 2f * t);
        static float Seg(float t, float a, float b) => Ease(Mathf.Clamp01((t - a) / (b - a)));

        static void BendLegs(ref HumanoidPose p, float knee, float hip)
        {
            p.knL.x += knee;
            p.knR.x += knee;
            p.hipL.x -= hip;
            p.hipR.x -= hip;
        }

        /// <summary>動作 a の進行度 t (0〜1) のポーズを p に上書きする。</summary>
        public static void Action(HAction a, float t, bool dribbleLeft, ref HumanoidPose p)
        {
            switch (a)
            {
                case HAction.Shoot:
                {
                    float up = Seg(t, 0f, 0.45f), rel = Seg(t, 0.45f, 0.6f);
                    p.shR = new Vector3(Mathf.Lerp(-60f, -160f, up), 0f, 8f);
                    p.elR = new Vector3(Mathf.Lerp(-110f, -8f, rel), 0f, 0f);
                    p.shL = new Vector3(Mathf.Lerp(-60f, -135f, up) + rel * 25f, 0f, -12f);
                    p.elL = new Vector3(-100f + rel * 40f, 0f, 0f);
                    p.spine = new Vector3(-4f, 0f, 0f);
                    p.head = new Vector3(-12f, 0f, 0f);
                    break;
                }
                case HAction.Layup:
                {
                    float up = Seg(t, 0f, 0.5f);
                    p.shR = new Vector3(Mathf.Lerp(-60f, -175f, up), 0f, 5f);
                    p.elR = new Vector3(Mathf.Lerp(-90f, -5f, up), 0f, 0f);
                    p.shL = new Vector3(-60f, 0f, -20f);
                    p.elL = new Vector3(-50f, 0f, 0f);
                    p.hipL = new Vector3(-75f, 0f, 0f);
                    p.knL = new Vector3(85f, 0f, 0f);
                    p.hipR = new Vector3(5f, 0f, 0f);
                    p.knR = new Vector3(15f, 0f, 0f);
                    break;
                }
                case HAction.Dunk:
                {
                    float x = Mathf.Lerp(-60f, -175f, Seg(t, 0f, 0.5f)) + Seg(t, 0.5f, 0.7f) * 65f;
                    p.shR = new Vector3(x, 0f, 10f);
                    p.shL = new Vector3(x, 0f, -10f);
                    p.elR = p.elL = new Vector3(-20f, 0f, 0f);
                    p.hipL = new Vector3(-50f, 0f, 0f);
                    p.knL = new Vector3(70f, 0f, 0f);
                    p.knR = new Vector3(40f, 0f, 0f);
                    break;
                }
                case HAction.Block:
                {
                    float x = Mathf.Lerp(-40f, -172f, Seg(t, 0f, 0.3f));
                    p.shR = new Vector3(x, 0f, 12f);
                    p.shL = new Vector3(x, 0f, -12f);
                    p.elR = p.elL = new Vector3(-5f, 0f, 0f);
                    p.spine = new Vector3(-3f, 0f, 0f);
                    break;
                }
                case HAction.Steal:
                {
                    float r = Mathf.Sin(t * Mathf.PI);
                    p.spine = new Vector3(15f + 25f * r, 0f, 0f);
                    p.shR = new Vector3(-30f - 55f * r, 0f, -10f);
                    p.elR = new Vector3(-10f, 0f, 0f);
                    BendLegs(ref p, 30f * r, 25f * r);
                    break;
                }
                case HAction.Crossover:
                {
                    float r = Mathf.Sin(t * Mathf.PI);
                    p.spine = new Vector3(18f, 0f, (dribbleLeft ? 18f : -18f) * r);
                    BendLegs(ref p, 25f * r, 20f * r);
                    break;
                }
                case HAction.Forehand:
                {
                    float back = Seg(t, 0f, 0.15f), sw = Seg(t, 0.15f, 0.65f);
                    p.spine = new Vector3(10f, Mathf.Lerp(Mathf.Lerp(0f, 65f, back), -65f, sw), 0f);
                    p.shR = new Vector3(Mathf.Lerp(15f, -75f, sw), 0f, 72f);
                    p.elR = new Vector3(-30f + sw * 10f, 0f, 0f);
                    p.shL = new Vector3(-50f, 0f, -45f + sw * 20f);
                    p.elL = new Vector3(-60f, 0f, 0f);
                    BendLegs(ref p, 25f, 20f);
                    break;
                }
                case HAction.Backhand:
                {
                    float back = Seg(t, 0f, 0.15f), sw = Seg(t, 0.15f, 0.65f);
                    p.spine = new Vector3(10f, Mathf.Lerp(Mathf.Lerp(0f, -80f, back), 55f, sw), 0f);
                    p.shR = new Vector3(Mathf.Lerp(-20f, -70f, sw), 0f, Mathf.Lerp(-55f, 65f, sw));
                    p.elR = new Vector3(-25f, 0f, 0f);
                    p.shL = new Vector3(Mathf.Lerp(-30f, -50f, sw), 0f, Mathf.Lerp(-10f, -60f, sw));
                    p.elL = new Vector3(-40f, 0f, 0f);
                    BendLegs(ref p, 25f, 20f);
                    break;
                }
                case HAction.Serve:
                case HAction.Smash:
                {
                    bool serve = a == HAction.Serve;
                    float toss = serve ? Seg(t, 0f, 0.45f) : 1f;
                    float sw = serve ? Seg(t, 0.45f, 0.62f) : Seg(t, 0f, 0.35f);
                    float fol = serve ? Seg(t, 0.62f, 1f) : Seg(t, 0.35f, 1f);
                    p.shL = new Vector3(Mathf.Lerp(-20f, -165f, toss) + sw * 100f, 0f, -10f);
                    p.elL = new Vector3(-10f, 0f, 0f);
                    float shx = Mathf.Lerp(Mathf.Lerp(-10f, -140f, toss), -175f, sw);
                    p.shR = new Vector3(Mathf.Lerp(shx, -30f, fol), 0f, Mathf.Lerp(45f, -25f, fol));
                    p.elR = new Vector3(Mathf.Lerp(Mathf.Lerp(-20f, -125f, toss), -5f, sw), 0f, 0f);
                    p.spine = new Vector3(Mathf.Lerp(Mathf.Lerp(0f, -18f, toss), 28f, sw) - fol * 8f, Mathf.Lerp(25f, -15f, sw), 0f);
                    p.head = new Vector3(Mathf.Lerp(-30f, 0f, sw), 0f, 0f);
                    break;
                }
                case HAction.Bump:
                {
                    float r = Mathf.Sin(Mathf.Min(1f, t * 1.4f) * Mathf.PI * 0.5f);
                    p.shL = new Vector3(-60f - 15f * r, 0f, 14f);
                    p.shR = new Vector3(-60f - 15f * r, 0f, -14f);
                    p.elL = p.elR = new Vector3(-4f, 0f, 0f);
                    p.spine = new Vector3(28f - 10f * r, 0f, 0f);
                    p.hipL = new Vector3(-45f, 0f, -6f);
                    p.hipR = new Vector3(-45f, 0f, 6f);
                    p.knL = p.knR = new Vector3(75f - 25f * r, 0f, 0f);
                    p.hipsY = -0.2f + 0.08f * r;
                    break;
                }
                case HAction.Set:
                {
                    float r = Seg(t, 0.2f, 0.6f);
                    p.shL = new Vector3(-150f - 20f * r, 0f, -22f);
                    p.shR = new Vector3(-150f - 20f * r, 0f, 22f);
                    p.elL = p.elR = new Vector3(Mathf.Lerp(-85f, -10f, r), 0f, 0f);
                    p.head = new Vector3(-25f, 0f, 0f);
                    p.knL = p.knR = new Vector3(40f - 30f * r, 0f, 0f);
                    p.hipL = new Vector3(-25f + 20f * r, 0f, -4f);
                    p.hipR = new Vector3(-25f + 20f * r, 0f, 4f);
                    p.hipsY = -0.08f + 0.06f * r;
                    break;
                }
                case HAction.Spike:
                {
                    float cock = Seg(t, 0f, 0.4f), sw = Seg(t, 0.4f, 0.6f), fol = Seg(t, 0.6f, 1f);
                    p.shL = new Vector3(Mathf.Lerp(-40f, -160f, cock) + sw * 90f, 0f, -15f);
                    p.elL = new Vector3(-20f, 0f, 0f);
                    float shx = Mathf.Lerp(Mathf.Lerp(-30f, -165f, cock), -175f, sw);
                    p.shR = new Vector3(Mathf.Lerp(shx, -40f, fol), 0f, 30f - fol * 30f);
                    p.elR = new Vector3(Mathf.Lerp(Mathf.Lerp(-30f, -130f, cock), -5f, sw), 0f, 0f);
                    p.spine = new Vector3(Mathf.Lerp(-20f * cock, 30f, sw) - fol * 10f, Mathf.Lerp(20f, -10f, sw), 0f);
                    break;
                }
                case HAction.Dive:
                {
                    float r = Seg(t, 0f, 0.3f);
                    p.spine = new Vector3(55f * r, 0f, 0f);
                    p.shL = new Vector3(-90f, 0f, 10f);
                    p.shR = new Vector3(-90f, 0f, -10f);
                    p.elL = p.elR = Vector3.zero;
                    p.hipL = new Vector3(-50f, 0f, 0f);
                    p.knL = new Vector3(90f, 0f, 0f);
                    p.hipR = new Vector3(20f, 0f, 0f);
                    p.knR = new Vector3(20f, 0f, 0f);
                    p.hipsY = -0.35f * r;
                    break;
                }
                case HAction.Celebrate:
                {
                    float pump = Mathf.Sin(t * Mathf.PI * 4f);
                    p.shL = new Vector3(-165f + pump * 15f, 0f, -20f);
                    p.shR = new Vector3(-165f - pump * 15f, 0f, 20f);
                    p.elL = p.elR = new Vector3(-20f, 0f, 0f);
                    p.head = new Vector3(-15f, 0f, 0f);
                    p.hipsY = Mathf.Abs(pump) * 0.05f;
                    break;
                }
                case HAction.Dejected:
                    p.head = new Vector3(35f, 0f, 0f);
                    p.spine = new Vector3(18f, 0f, 0f);
                    p.shL = new Vector3(5f, 0f, -4f);
                    p.shR = new Vector3(5f, 0f, 4f);
                    p.elL = p.elR = new Vector3(-10f, 0f, 0f);
                    break;
            }
        }
    }
}
