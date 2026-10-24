using UnityEngine;

namespace SportsGame
{
    /// <summary>レシーブ・トス・スパイク・サーブなどの狙いと初速度を求める。</summary>
    public class VolleyballShots
    {
        const float SetterHeight = 2.1f;
        const float AttackDistance = 0.9f;
        const float SpikeNetMargin = 0.05f, ServeNetMargin = 0.35f;
        const float IntoNetDepth = 0.25f;

        readonly VolleyballConfig cfg;
        readonly NetCollider net;
        readonly float gravity;

        public VolleyballShots(VolleyballConfig cfg, NetCollider net, float gravity)
        {
            this.cfg = cfg;
            this.net = net;
            this.gravity = gravity;
        }

        /// <summary>セッターが待つ位置(ネット際の中央付近)。</summary>
        public static Vector3 SetSpot(VolleyballPlayer setter)
            => new Vector3(Mathf.Clamp(setter.Pos.x * 0.3f, -1f, 1f), 0f, -setter.Forward * 1.3f);

        /// <summary>アタッカーが打つ位置(高さ hitHeight)。</summary>
        public Vector3 AttackPoint(VolleyballPlayer attacker)
            => new Vector3(Mathf.Clamp(attacker.Pos.x, -2.8f, 2.8f), cfg.hitHeight, -attacker.Forward * AttackDistance);

        static Vector3 Scatter(Vector3 p, float sigma) => p + new Vector3(RandomUtil.Gauss() * sigma, 0f, RandomUtil.Gauss() * sigma);

        /// <summary>レシーブ: セッターの位置へ高く上げる。quality(0〜1)が低いほど乱れる。</summary>
        public Vector3 Receive(Vector3 start, Vector3 setSpot, float quality, float error, float incomingSpeed)
        {
            float sigma = error * (1.2f - quality) * (1f + incomingSpeed / 25f);
            Vector3 target = Scatter(setSpot, sigma);
            target.y = SetterHeight;
            float apex = Mathf.Max(start.y + 1f, Mathf.Lerp(cfg.receiveApex.x, cfg.receiveApex.y, quality));
            return BallMath.SolveByApex(start, target, apex, gravity, out _);
        }

        /// <summary>トス: アタッカーが打つ位置へ上げる。</summary>
        public Vector3 Set(Vector3 start, Vector3 attackPoint, float quality, float error)
        {
            Vector3 target = Scatter(attackPoint, error * (1.1f - quality) * 0.6f);
            target.y = attackPoint.y;
            return BallMath.SolveByApex(start, target, Mathf.Max(start.y + 1f, cfg.setApex), gravity, out _);
        }

        /// <summary>相手コートの狙い。aim.x: -1〜1(左右)、aim.y: -1(ネット際)〜1(エンドライン際)。</summary>
        public static Vector3 CourtTarget(int attackingTeam, Vector2 aim, float sigma)
        {
            float f = VolleyballDims.Forward(attackingTeam);
            float depth = Mathf.Lerp(2.5f, 7.2f, (Mathf.Clamp(aim.y, -1f, 1f) + 1f) * 0.5f);
            var p = new Vector3(Mathf.Clamp(aim.x, -1f, 1f) * 3.3f, VolleyballDims.BallRadius, f * depth);
            return Scatter(p, sigma);
        }

        public Vector3 Spike(Vector3 start, Vector3 target, float power, bool intoNet)
        {
            float speed = Mathf.Lerp(cfg.spikeSpeed.x, cfg.spikeSpeed.y, power);
            return intoNet
                ? NetTrajectory.IntoNet(start, target, speed, gravity, net, IntoNetDepth)
                : NetTrajectory.OverNet(start, target, speed, gravity, net, VolleyballDims.BallRadius, SpikeNetMargin);
        }

        /// <summary>山なりのつなぎ・フェイント。</summary>
        public Vector3 Lob(Vector3 start, Vector3 target, float apex)
            => BallMath.SolveByApex(start, target, Mathf.Max(apex, start.y + 0.8f, VolleyballDims.NetTop + 1f), gravity, out _);

        public Vector3 Serve(Vector3 start, Vector3 target, float power)
        {
            float speed = Mathf.Lerp(cfg.serveSpeed.x, cfg.serveSpeed.y, power);
            return NetTrajectory.OverNet(start, target, speed, gravity, net, VolleyballDims.BallRadius, ServeNetMargin);
        }

        /// <summary>ブロックで跳ね返したボール。kill なら相手コートへ叩き落とし、それ以外は自陣へ上げる。</summary>
        public static Vector3 BlockDeflection(Vector3 incoming, int blockerTeam, bool kill)
        {
            float back = -VolleyballDims.Forward(blockerTeam);
            return kill
                ? new Vector3(incoming.x * 0.3f, -2f, -back * Mathf.Abs(incoming.z) * 0.35f)
                : new Vector3(incoming.x * 0.2f, 3f, back * 1.5f);
        }
    }
}
