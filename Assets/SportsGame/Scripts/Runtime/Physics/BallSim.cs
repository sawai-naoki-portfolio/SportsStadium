using System.Collections.Generic;
using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// 決定論的なボール物理。重力を解析解で積分するため BallMath の軌道計算と結果が一致する。
    /// AI は Clone() で先読みして着地点・打点を予測する。
    /// </summary>
    public class BallSim
    {
        const float MinBounceSpeed = 0.35f;
        const float RestingSpeed = 0.01f;

        public Vector3 pos, vel;
        public float radius = 0.12f;
        public float gravity = 9.81f;
        /// <summary>スピンによる見かけの重力倍率(次のバウンドでリセット)。</summary>
        public float gravityScale = 1f;
        public float restitution = 0.78f;
        public float friction = 0.86f;
        /// <summary>次のバウンドだけに掛かる係数(スピン効果)。</summary>
        public float nextBounceXZ = 1f, nextBounceY = 1f;
        public float rollDecay = 1.2f;
        public bool frozen;
        public int substeps = 4;
        public readonly List<IBallCollider> colliders = new List<IBallCollider>();
        public System.Action<BallContact> onContact;

        public bool OnGround => pos.y <= radius + 0.01f && Mathf.Abs(vel.y) < 0.05f;

        public void Launch(Vector3 position, Vector3 velocity, float gScale = 1f, float bounceXZ = 1f, float bounceY = 1f)
        {
            if (!IsFinite(position) || !IsFinite(velocity))
            {
                GameLog.Warn("BallSim", $"Invalid launch ignored: pos={position} vel={velocity}");
                return;
            }
            frozen = false;
            pos = position;
            vel = velocity;
            gravityScale = gScale;
            nextBounceXZ = bounceXZ;
            nextBounceY = bounceY;
        }

        public void Hold(Vector3 position)
        {
            frozen = true;
            pos = position;
            vel = Vector3.zero;
            gravityScale = 1f;
        }

        public void Step(float dt)
        {
            if (frozen || dt <= 0f) return;
            float h = dt / substeps;
            for (int s = 0; s < substeps; s++) SubStep(h);
        }

        void SubStep(float h)
        {
            Vector3 prev = pos;
            Vector3 g = new Vector3(0f, -gravity * gravityScale, 0f);
            pos += vel * h + 0.5f * h * h * g;
            vel += g * h;

            if (pos.y < radius)
            {
                pos.y = radius;
                if (vel.y < 0f) Bounce(-vel.y);
            }
            if (pos.y <= radius + 0.0005f && Mathf.Abs(vel.y) < RestingSpeed)
            {
                float k = Mathf.Max(0f, 1f - rollDecay * h);
                vel.x *= k;
                vel.z *= k;
            }

            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i].Collide(ref pos, ref vel, prev, radius, out var c))
                    onContact?.Invoke(c);
            }
        }

        void Bounce(float impact)
        {
            gravityScale = 1f;
            if (impact > MinBounceSpeed)
            {
                vel.y = impact * restitution * nextBounceY;
                vel.x *= friction * nextBounceXZ;
                vel.z *= friction * nextBounceXZ;
                nextBounceXZ = nextBounceY = 1f;
            }
            else vel.y = 0f;
            if (impact > 0.05f) onContact?.Invoke(new BallContact { kind = ContactKind.Ground, point = pos, speed = impact });
        }

        /// <summary>先読み用の複製(イベント通知なし・粗い分割)。</summary>
        public BallSim Clone()
        {
            var b = new BallSim
            {
                pos = pos, vel = vel, radius = radius, gravity = gravity, gravityScale = gravityScale,
                restitution = restitution, friction = friction, nextBounceXZ = nextBounceXZ, nextBounceY = nextBounceY,
                rollDecay = rollDecay, frozen = frozen, substeps = 2
            };
            b.colliders.AddRange(colliders);
            return b;
        }

        static bool IsFinite(Vector3 v) =>
            !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}
