using UnityEngine;

namespace SportsGame
{
    /// <summary>テニスの選手(移動・向き・打点の判定)。人か CPU かは ITennisController が決める。</summary>
    public class TennisPlayer : MonoBehaviour
    {
        const float Accel = 28f;
        const float CourtHalfX = 10f;
        const float BackLimit = 18f;
        const float NetLimit = 0.7f;

        Vector3 pos, vel;

        public int Side { get; private set; }
        public Humanoid Body { get; private set; }
        public float MaxSpeed { get; set; }
        public SwingIntent Swing { get; private set; }

        public Vector3 Pos => pos;
        public Vector3 Velocity => vel;
        public float Forward => TennisDims.Forward(Side);
        public Vector3 ForwardDir => new Vector3(0f, 0f, Forward);
        /// <summary>選手から見た右(フォアハンド側)。</summary>
        public Vector3 RightDir => new Vector3(Forward, 0f, 0f);

        public static TennisPlayer Create(Transform parent, int side, HumanoidLook look, string name, float speed)
        {
            look.racket = true;
            var body = HumanoidBuilder.Build(parent, name, look);
            body.readyRacket = true;
            var p = body.gameObject.AddComponent<TennisPlayer>();
            p.Side = side;
            p.Body = body;
            p.MaxSpeed = speed;
            return p;
        }

        public void Teleport(Vector3 p)
        {
            pos = new Vector3(p.x, 0f, p.z);
            vel = Vector3.zero;
            transform.position = pos;
            Body.FaceInstant(ForwardDir);
        }

        /// <summary>dir: 移動したい方向(大きさ 0〜1)。lookAt: 体を向ける対象(ボール)。</summary>
        public void Move(Vector3 dir, float dt, Vector3 lookAt, float speedScale = 1f)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            vel = Vector3.MoveTowards(vel, dir * MaxSpeed * speedScale, Accel * dt);
            pos += vel * dt;
            pos.x = Mathf.Clamp(pos.x, -CourtHalfX, CourtHalfX);
            pos.z = Side == 0 ? Mathf.Clamp(pos.z, -BackLimit, -NetLimit) : Mathf.Clamp(pos.z, NetLimit, BackLimit);
            transform.position = pos;

            float speed = vel.magnitude;
            Body.moveSpeed = speed;
            Body.crouch = Mathf.Lerp(Body.crouch, speed < 1f ? 0.35f : 0.15f, dt * 6f);
            Vector3 face = ForwardDir;
            Vector3 toLook = lookAt - pos;
            toLook.y = 0f;
            if (toLook.sqrMagnitude > 0.01f) face = (ForwardDir * 1.4f + toLook.normalized * 0.6f).normalized;
            if (speed > 3.5f) face = (face + vel.normalized * 0.6f).normalized;
            Body.FaceTowards(face, dt, 10f);
        }

        /// <summary>目標地点へ向かう(近づくと減速する)。</summary>
        public void MoveTo(Vector3 target, float dt, Vector3 lookAt, float speedScale = 1f)
        {
            Vector3 d = target - pos;
            d.y = 0f;
            float dist = d.magnitude;
            Move(dist > 0.01f ? d / dist * Mathf.Clamp01(dist / 0.6f) : Vector3.zero, dt, lookAt, speedScale);
        }

        /// <summary>サーブ前はベースライン上の自分のサイドだけを動ける。</summary>
        public void ConstrainToServePosition(float sideSign, float baselineZ)
        {
            pos.x = sideSign * Mathf.Clamp(Mathf.Abs(pos.x), 0.25f, 3.2f);
            pos.z = baselineZ;
            transform.position = pos;
        }

        public void QueueSwing(TShot shot) => Swing = new SwingIntent { active = true, shot = shot, since = Time.time };

        public void ClearSwing() => Swing = default;

        public HitGeometry GeometryTo(Vector3 ball) => new HitGeometry
        {
            lateral = (ball.x - pos.x) * Forward,
            forward = (ball.z - pos.z) * Forward,
            height = ball.y
        };

        /// <summary>ラケットが届く範囲か。</summary>
        public static bool InReach(HitGeometry g)
            => Mathf.Abs(g.lateral) < 1.75f && g.forward > -0.9f && g.forward < 1.3f && g.height > 0.05f && g.height < 2.75f;

        /// <summary>打つべきタイミングか(体の少し前・高い球・遠い球はその場で打つ)。</summary>
        public static bool IsContactMoment(HitGeometry g) => g.forward <= 0.6f || g.height > 2.1f || Mathf.Abs(g.lateral) > 1.45f;
    }
}
