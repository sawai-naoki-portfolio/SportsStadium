using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// ビーチバレーのコート寸法(16m×8m)と判定。ネットは z=0、ホーム(team 0 = プレイヤー側)は z&lt;0。
    /// </summary>
    public static class VolleyballDims
    {
        public const float HalfLength = 8f;
        public const float HalfWidth = 4f;
        public const float NetTop = 2.43f;
        public const float NetBottom = 1.43f;
        public const float NetHalfWidth = 4.3f;
        /// <summary>アンテナの位置(サイドラインの真上)。この外側を通った返球は反則。</summary>
        public const float AntennaX = 4f;
        public const float BallRadius = 0.105f;

        public static int SideOf(Vector3 p) => p.z < 0f ? 0 : 1;

        /// <summary>ネット方向の符号(ホームは +1)。</summary>
        public static float Forward(int team) => team == 0 ? 1f : -1f;

        /// <summary>ラインに触れていればイン。</summary>
        public static bool InCourt(Vector3 p) => Mathf.Abs(p.x) <= HalfWidth + BallRadius && Mathf.Abs(p.z) <= HalfLength + BallRadius;

        public static NetCollider CreateNet() => new NetCollider
        {
            halfWidth = NetHalfWidth, bottom = NetBottom, topCenter = NetTop, topEdge = NetTop, restitution = 0.15f
        };
    }
}
