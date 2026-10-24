using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// テニスコートの寸法(m)と判定。ネットは z=0、手前側(side 0 = プレイヤー)が z&lt;0。
    /// </summary>
    public static class TennisDims
    {
        public const float HalfLength = 11.885f;
        public const float SinglesHalfWidth = 4.115f;
        public const float DoublesHalfWidth = 5.485f;
        public const float ServiceLine = 6.40f;
        public const float NetCenterHeight = 0.914f;
        public const float NetPostHeight = 1.07f;
        public const float NetHalfWidth = 6.40f;
        public const float BallRadius = 0.035f;

        /// <summary>side 0 は z 負側、side 1 は z 正側。</summary>
        static bool OnSide(Vector3 p, int side) => side == 0 ? p.z <= 0f : p.z >= 0f;

        public static int SideOf(Vector3 p) => p.z < 0f ? 0 : 1;

        /// <summary>前方(ネット方向)の符号。side 0 は +1。</summary>
        public static float Forward(int side) => side == 0 ? 1f : -1f;

        public static bool InSingles(Vector3 p, int side)
            => OnSide(p, side) && Mathf.Abs(p.x) <= SinglesHalfWidth + BallRadius && Mathf.Abs(p.z) <= HalfLength + BallRadius;

        /// <summary>サービスボックス判定。boxXSign はボックスが x の正側(+1)か負側(-1)か。</summary>
        public static bool InServiceBox(Vector3 p, int side, float boxXSign)
        {
            if (!OnSide(p, side)) return false;
            float x = p.x * boxXSign;
            return x >= -BallRadius && x <= SinglesHalfWidth + BallRadius && Mathf.Abs(p.z) <= ServiceLine + BallRadius;
        }

        public static NetCollider CreateNet() => new NetCollider
        {
            halfWidth = NetHalfWidth, bottom = 0f, topCenter = NetCenterHeight, topEdge = NetPostHeight, restitution = 0.1f
        };
    }
}
