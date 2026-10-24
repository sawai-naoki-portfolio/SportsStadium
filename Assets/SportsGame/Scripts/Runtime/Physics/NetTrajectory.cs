using UnityEngine;

namespace SportsGame
{
    /// <summary>z=0 のネットを越える(または意図的にネットへかける)打球の初速度を求める。テニス・バレーで共用。</summary>
    public static class NetTrajectory
    {
        const int MaxIterations = 40;
        const float TimeGrowth = 1.05f;

        /// <summary>
        /// 水平速度 speed で target へ届く初速度。ネット上端 + ボール半径 + margin を越えるまで滞空時間を伸ばして山なりにする。
        /// </summary>
        public static Vector3 OverNet(Vector3 start, Vector3 target, float speed, float gravity, NetCollider net, float ballRadius, float margin)
        {
            float t = Mathf.Max(0.15f, HorizontalDistance(start, target) / Mathf.Max(0.1f, speed));
            Vector3 v = BallMath.SolveByTime(start, target, t, gravity);
            for (int i = 0; i < MaxIterations && !ClearsNet(start, v, gravity, net, ballRadius + margin); i++)
            {
                t *= TimeGrowth;
                v = BallMath.SolveByTime(start, target, t, gravity);
            }
            return v;
        }

        /// <summary>ミスショット: ネット上端より depth だけ下を通す。</summary>
        public static Vector3 IntoNet(Vector3 start, Vector3 target, float speed, float gravity, NetCollider net, float depth)
        {
            float frac = Mathf.Abs(start.z) / Mathf.Max(0.01f, Mathf.Abs(target.z - start.z));
            float xn = Mathf.Lerp(start.x, target.x, frac);
            Vector3 netPoint = new Vector3(xn, net.TopAt(xn) - depth, 0f);
            float t = Mathf.Max(0.1f, HorizontalDistance(start, netPoint) / Mathf.Max(0.1f, speed));
            return BallMath.SolveByTime(start, netPoint, t, gravity);
        }

        /// <summary>初速度 v の軌道が z=0 を通過するときネット上端 + clearance より高いか。</summary>
        public static bool ClearsNet(Vector3 start, Vector3 v, float gravity, NetCollider net, float clearance)
        {
            if (Mathf.Abs(v.z) < 1e-3f) return true;
            float tn = -start.z / v.z;
            if (tn <= 0f) return true;
            float xn = start.x + v.x * tn;
            float yn = start.y + v.y * tn - 0.5f * gravity * tn * tn;
            return yn >= net.TopAt(xn) + clearance;
        }

        static float HorizontalDistance(Vector3 a, Vector3 b) => new Vector2(b.x - a.x, b.z - a.z).magnitude;
    }
}
