using UnityEngine;

namespace SportsGame
{
    /// <summary>ボタンの先押し(ボールが届く範囲に来たら実行する)。</summary>
    public enum VbIntent { None, Pass, Spike }

    /// <summary>ビーチバレーの選手(砂上の移動・ジャンプ・先押し入力)。</summary>
    public class VolleyballPlayer : MonoBehaviour
    {
        const float Accel = 20f;
        const float Gravity = 9.81f;
        const float SideLimitX = 7f, BackLimit = 12f, NetLimit = 0.35f;
        const float ArmReach = 0.42f;

        Vector3 pos, vel;
        float jumpHeight, vy;

        public int Index { get; private set; }
        public int Team { get; private set; }
        public Humanoid Body { get; private set; }
        public float MaxSpeed { get; set; }
        public VbIntent Intent { get; private set; }
        public float IntentTime { get; private set; }

        public Vector3 Pos => pos;
        public float Forward => VolleyballDims.Forward(Team);
        public Vector3 ForwardDir => new Vector3(0f, 0f, Forward);
        public bool Airborne => jumpHeight > 0.001f || vy > 0f;
        /// <summary>腕を上げたときの手(スパイク・ブロックの打点)。</summary>
        public Vector3 HandPoint => pos + Vector3.up * (Body.Height + ArmReach + jumpHeight) + ForwardDir * 0.25f;
        /// <summary>ジャンプの頂点までの時間。</summary>
        public static float RiseTime(float jumpSpeed) => jumpSpeed / Gravity;

        public static VolleyballPlayer Create(Transform parent, int index, HumanoidLook look, string name, float speed)
        {
            var body = HumanoidBuilder.Build(parent, name, look);
            var p = body.gameObject.AddComponent<VolleyballPlayer>();
            p.Index = index;
            p.Team = index / 2;
            p.Body = body;
            p.MaxSpeed = speed;
            return p;
        }

        public void Teleport(Vector3 p)
        {
            pos = new Vector3(p.x, 0f, p.z);
            vel = Vector3.zero;
            jumpHeight = vy = 0f;
            Intent = VbIntent.None;
            Apply();
            Body.FaceInstant(ForwardDir);
        }

        public void SetIntent(VbIntent intent)
        {
            Intent = intent;
            IntentTime = Time.time;
        }

        public void ClearIntent() => Intent = VbIntent.None;

        public void Move(Vector3 dir, float dt, Vector3 lookAt, float speedScale = 1f)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            if (!Airborne) vel = Vector3.MoveTowards(vel, dir * MaxSpeed * speedScale, Accel * dt);
            pos += vel * dt;
            pos.x = Mathf.Clamp(pos.x, -SideLimitX, SideLimitX);
            pos.z = Team == 0 ? Mathf.Clamp(pos.z, -BackLimit, -NetLimit) : Mathf.Clamp(pos.z, NetLimit, BackLimit);
            Apply();
            Body.moveSpeed = Airborne ? 0f : new Vector2(vel.x, vel.z).magnitude;
            Vector3 face = lookAt - pos;
            face.y = 0f;
            Body.FaceTowards(face.sqrMagnitude > 0.01f ? (face.normalized + ForwardDir).normalized : ForwardDir, dt, 10f);
        }

        public void MoveTo(Vector3 target, float dt, Vector3 lookAt, float speedScale = 1f)
        {
            Vector3 d = target - pos;
            d.y = 0f;
            float dist = d.magnitude;
            Move(dist > 0.02f ? d / dist * Mathf.Clamp01(dist / 0.5f) : Vector3.zero, dt, lookAt, speedScale);
        }

        /// <summary>サーブ前はエンドラインの後ろを左右にだけ動ける。</summary>
        public void ConstrainToServeLine(float z, float maxX)
        {
            pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
            pos.z = z;
            Apply();
        }

        /// <summary>踏み切り。forwardSpeed で少し前へ跳ぶ(スパイクの助走)。</summary>
        public void Jump(float speed, float forwardSpeed = 0f)
        {
            if (Airborne) return;
            vy = speed;
            vel = new Vector3(vel.x * 0.3f, 0f, vel.z * 0.3f) + ForwardDir * forwardSpeed;
            AudioManager.Play(Sfx.SandHit, 0.3f, 1.3f);
        }

        public void Tick(float dt)
        {
            if (Airborne)
            {
                vy -= Gravity * dt;
                jumpHeight += vy * dt;
                if (jumpHeight <= 0f)
                {
                    jumpHeight = vy = 0f;
                    vel *= 0.3f;
                    if (Intent == VbIntent.Spike) Intent = VbIntent.None; // 着地したらスパイクの先押しは終わり
                }
            }
            Body.airborne = Airborne;
            Apply();
        }

        void Apply() => transform.position = new Vector3(pos.x, jumpHeight, pos.z);
    }
}
