using UnityEngine;

namespace SportsGame
{
    /// <summary>打球の狙い(着地点)と初速度を求める。ネットを越えない軌道は自動で山なりに補正する。</summary>
    public class TennisShotSolver
    {
        public struct Launch
        {
            public Vector3 velocity;
            public float gravityScale, bounceXZ, bounceY;
        }

        const float MaxDepth = 10.9f;
        const float IntoNetDepth = 0.22f;

        readonly TennisConfig cfg;
        readonly NetCollider net;
        readonly float gravity;

        public TennisShotSolver(TennisConfig cfg, NetCollider net, float gravity)
        {
            this.cfg = cfg;
            this.net = net;
            this.gravity = gravity;
        }

        /// <summary>ラリーの狙い。forward はネット方向の符号(手前の選手は +1)。sigma は狙いのブレ(m)。</summary>
        public Vector3 RallyTarget(float forward, TShot shot, Vector2 aim, float sigma)
        {
            float depth;
            switch (shot)
            {
                case TShot.Slice: depth = 7.6f; break;
                case TShot.Lob: depth = 9.9f; break;
                case TShot.Smash: depth = 6.8f; break;
                case TShot.Volley: depth = 7.2f; break;
                default: depth = 8.6f; break;
            }
            if (aim.y > 0.4f) depth += shot == TShot.Lob ? 0.6f : 1.6f;
            else if (aim.y < -0.4f) depth = shot == TShot.Slice || shot == TShot.Volley ? 3.6f : shot == TShot.Lob ? 8.6f : 5.6f;
            depth = Mathf.Min(depth, MaxDepth);
            float lateralMax = shot == TShot.Smash || shot == TShot.Volley ? 3.7f : 3.5f;
            float x = Mathf.Clamp(aim.x, -1f, 1f) * lateralMax + RandomUtil.Gauss() * sigma;
            float z = depth + RandomUtil.Gauss() * sigma * 1.25f;
            return new Vector3(x, TennisDims.BallRadius, forward * z);
        }

        /// <summary>サーブの狙い。aimX は -1(左)〜1(右)でボックス内の左右位置。</summary>
        public Vector3 ServeTarget(float forward, float boxSign, float aimX, float sigma)
        {
            float minX = boxSign < 0f ? -3.55f : 0.35f;
            float maxX = boxSign < 0f ? -0.35f : 3.55f;
            float x = Mathf.Lerp(minX, maxX, (Mathf.Clamp(aimX, -1f, 1f) + 1f) * 0.5f) + RandomUtil.Gauss() * sigma;
            float z = Random.Range(4.9f, 5.8f) + RandomUtil.Gauss() * sigma * 1.2f;
            return new Vector3(x, TennisDims.BallRadius, forward * z);
        }

        public Launch Rally(Vector3 start, Vector3 target, TShot shot, float power, bool intoNet)
        {
            if (shot == TShot.Lob)
            {
                // ロブのミスはネットではなくオーバー(アウト)にする
                if (intoNet) target.z += Mathf.Sign(target.z) * 2.5f;
                float apex = Mathf.Lerp(cfg.lobApex.x, cfg.lobApex.y, power);
                return new Launch { velocity = BallMath.SolveByApex(start, target, apex, gravity, out _), gravityScale = 1f, bounceXZ = 1f, bounceY = 1f };
            }
            return Solve(start, target, cfg.Profile(shot), power, intoNet);
        }

        public Launch Serve(Vector3 start, Vector3 target, bool first, float power, bool intoNet)
            => Solve(start, target, first ? cfg.firstServe : cfg.secondServe, power, intoNet);

        Launch Solve(Vector3 start, Vector3 target, ShotProfile p, float power, bool intoNet)
        {
            float g = gravity * p.gravityScale;
            float speed = p.Speed(power);
            Vector3 v = intoNet
                ? NetTrajectory.IntoNet(start, target, speed, g, net, IntoNetDepth)
                : NetTrajectory.OverNet(start, target, speed, g, net, TennisDims.BallRadius, p.netMargin);
            return new Launch { velocity = v, gravityScale = p.gravityScale, bounceXZ = p.bounceXZ, bounceY = p.bounceY };
        }
    }
}
