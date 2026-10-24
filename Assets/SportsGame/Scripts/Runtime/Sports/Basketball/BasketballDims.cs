using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// バスケットボール(FIBA)ハーフコートの寸法。リング中心が原点の真上、攻撃方向は +Z(ベースライン側)。
    /// </summary>
    public static class BasketballDims
    {
        public static readonly Vector3 Hoop = new Vector3(0f, 3.05f, 0f);
        public const float RimRadius = 0.235f;
        public const float RimTube = 0.013f;
        public const float BallRadius = 0.12f;
        public const float BaselineZ = 1.575f;
        public const float HalfLineZ = -12.425f;
        public const float HalfWidth = 7.5f;
        public const float ThreeRadius = 6.75f;
        public const float CornerX = 6.6f;
        public const float CornerZ = -1.416f;
        public const float KeyHalfWidth = 2.45f;
        public const float FreeThrowZ = -4.225f;
        public const float BoardFrontZ = 0.375f;

        /// <summary>3ポイントラインの外か。</summary>
        public static bool IsThree(Vector3 p)
        {
            if (p.z > CornerZ) return Mathf.Abs(p.x) > CornerX;
            return new Vector2(p.x, p.z).magnitude > ThreeRadius;
        }

        public static bool InBounds(Vector3 p) => Mathf.Abs(p.x) <= HalfWidth && p.z <= BaselineZ && p.z >= HalfLineZ;

        /// <summary>リング中心までの水平距離。</summary>
        public static float HoopDistance(Vector3 p) => new Vector2(p.x - Hoop.x, p.z - Hoop.z).magnitude;

        public static readonly Vector3 BoardCenter = new Vector3(0f, 3.425f, BoardFrontZ + 0.025f);
        public static readonly Vector3 BoardSize = new Vector3(1.8f, 1.05f, 0.05f);
        public static readonly Vector3 PoleCenter = new Vector3(0f, 2.3f, 3.3f);
        public static readonly Vector3 PoleSize = new Vector3(0.35f, 3.4f, 0.35f);
        public static readonly Vector3 BaseCenter = new Vector3(0f, 0.6f, 3.6f);
        public static readonly Vector3 BaseSize = new Vector3(1.4f, 1.2f, 1.8f);

        /// <summary>ゴール(リム・バックボード・支柱)と体育館の壁のコライダー。</summary>
        public static IBallCollider[] CreateColliders() => new IBallCollider[]
        {
            new TorusCollider(Hoop, RimRadius, RimTube) { restitution = 0.5f },
            new AabbCollider(BoardCenter, BoardSize, ContactKind.Board) { restitution = 0.62f },
            new AabbCollider(PoleCenter, PoleSize, ContactKind.Post) { restitution = 0.4f },
            new AabbCollider(BaseCenter, BaseSize, ContactKind.Post) { restitution = 0.4f },
            new BoundsCollider(new Vector3(-15.8f, 0f, -21.8f), new Vector3(15.8f, 11.8f, 7.8f)),
        };

        /// <summary>ボールがリングを上から下へ通過したか(得点判定)。</summary>
        public static bool PassedThroughHoop(float prevY, Vector3 pos, Vector3 vel)
            => prevY >= Hoop.y && pos.y < Hoop.y && vel.y < 0f && HoopDistance(pos) < RimRadius - 0.05f;

        /// <summary>リングへ向かう水平方向の単位ベクトル。</summary>
        public static Vector3 ToHoop(Vector3 p)
        {
            Vector3 d = new Vector3(Hoop.x - p.x, 0f, Hoop.z - p.z);
            return d.sqrMagnitude > 1e-6f ? d.normalized : Vector3.forward;
        }
    }
}
