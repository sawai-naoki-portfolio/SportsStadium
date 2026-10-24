using UnityEngine;

namespace SportsGame
{
    /// <summary>バスケットボールの選手(移動・ジャンプ・スタミナ・行動のクールダウン・シュート中の状態)。</summary>
    public class BasketballPlayer : MonoBehaviour
    {
        const float Accel = 26f;
        const float Gravity = 9.81f;
        const float CourtMargin = 0.2f;
        const float StunSpeedScale = 0.35f;
        /// <summary>腕を伸ばしたときの手の高さ(身長に対する加算)。</summary>
        const float ArmReach = 0.45f;

        Vector3 pos, vel;
        float jumpHeight, vy;
        float burstTime;
        Vector3 burstVelocity;

        public int Team { get; private set; }
        public Humanoid Body { get; private set; }
        public float Stamina { get; private set; } = 1f;
        public bool Sprinting { get; private set; }

        public float stun, stealCooldown, crossCooldown;
        public bool dribbleLeft;

        // シュート中の状態
        public bool shooting;
        public ShotKind shotKind;
        public float shotTimer;
        public bool shotJumped;
        public Vector3 shotFeet;

        public Vector3 Pos => pos;
        public Vector3 Velocity => vel;
        public float JumpHeight => jumpHeight;
        public bool Airborne => jumpHeight > 0.001f || vy > 0f;
        public bool Rising => vy > 0f;
        public Vector3 Facing => transform.forward;
        /// <summary>手を上げたときの指先の位置(ブロック・リバウンド判定用)。</summary>
        public Vector3 HandTop => pos + Vector3.up * (Body.Height + ArmReach + jumpHeight) + Facing * 0.15f;
        public float Reach => Body.Height + ArmReach - 0.2f + jumpHeight;

        public static BasketballPlayer Create(Transform parent, int team, HumanoidLook look, string name)
        {
            var body = HumanoidBuilder.Build(parent, name, look);
            var p = body.gameObject.AddComponent<BasketballPlayer>();
            p.Team = team;
            p.Body = body;
            return p;
        }

        public void Teleport(Vector3 p, Vector3 face)
        {
            pos = new Vector3(p.x, 0f, p.z);
            vel = Vector3.zero;
            jumpHeight = vy = 0f;
            burstTime = 0f;
            stun = 0f;
            shooting = false;
            Apply();
            Body.FaceInstant(face);
        }

        /// <summary>dir の大きさ(0〜1)×speed で移動。空中では向きを変えられない。</summary>
        public void Move(Vector3 dir, float speed, bool sprint, BasketballConfig cfg, float dt)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            Sprinting = sprint && Stamina > 0.05f && dir.sqrMagnitude > 0.1f && !Airborne;
            Stamina = Mathf.Clamp01(Stamina + (Sprinting ? -cfg.staminaDrain : cfg.staminaRegen) * dt);
            Vector3 target = dir * speed * (Sprinting ? cfg.sprintMultiplier : 1f) * (stun > 0f ? StunSpeedScale : 1f);
            if (burstTime > 0f) target = burstVelocity;
            if (!Airborne) vel = Vector3.MoveTowards(vel, target, Accel * dt);
            pos += vel * dt;
            ClampToCourt();
            Apply();
            Body.moveSpeed = Airborne ? 0f : new Vector2(vel.x, vel.z).magnitude;
        }

        /// <summary>空中での移動(レイアップ・ダンクの踏み切り後など)。</summary>
        public void Drift(Vector3 velocity)
        {
            vel = new Vector3(velocity.x, 0f, velocity.z);
        }

        public void Burst(Vector3 velocity, float time)
        {
            burstVelocity = new Vector3(velocity.x, 0f, velocity.z);
            burstTime = time;
        }

        public void Jump(float speed)
        {
            if (Airborne) return;
            vy = speed;
            AudioManager.Play(Sfx.Squeak, 0.25f, Random.Range(0.9f, 1.1f));
        }

        public void Face(Vector3 dir, float dt) => Body.FaceTowards(dir, dt, 12f);

        /// <summary>ジャンプの物理とクールダウンを進める。</summary>
        public void Tick(float dt)
        {
            if (Airborne)
            {
                vy -= Gravity * dt;
                jumpHeight += vy * dt;
                if (jumpHeight <= 0f)
                {
                    jumpHeight = 0f;
                    vy = 0f;
                }
            }
            burstTime = Mathf.Max(0f, burstTime - dt);
            stun = Mathf.Max(0f, stun - dt);
            stealCooldown = Mathf.Max(0f, stealCooldown - dt);
            crossCooldown = Mathf.Max(0f, crossCooldown - dt);
            Body.airborne = Airborne;
            Apply();
        }

        /// <summary>2人が重ならないよう押し離す(ディフェンスが壁になる)。</summary>
        public static void Separate(BasketballPlayer a, BasketballPlayer b, float minDistance)
        {
            Vector3 d = b.pos - a.pos;
            d.y = 0f;
            float dist = d.magnitude;
            if (dist >= minDistance) return;
            Vector3 n = dist > 1e-4f ? d / dist : Vector3.right;
            Vector3 push = n * (minDistance - dist) * 0.5f;
            a.pos -= push;
            b.pos += push;
            a.ClampToCourt();
            b.ClampToCourt();
            a.Apply();
            b.Apply();
        }

        void ClampToCourt()
        {
            pos.x = Mathf.Clamp(pos.x, -BasketballDims.HalfWidth + CourtMargin, BasketballDims.HalfWidth - CourtMargin);
            pos.z = Mathf.Clamp(pos.z, BasketballDims.HalfLineZ + CourtMargin, BasketballDims.BaselineZ - CourtMargin);
        }

        void Apply() => transform.position = new Vector3(pos.x, jumpHeight, pos.z);
    }
}
