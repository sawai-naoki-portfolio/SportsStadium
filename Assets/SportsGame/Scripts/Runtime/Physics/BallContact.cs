using UnityEngine;

namespace SportsGame
{
    public enum ContactKind { Ground, Rim, Board, Net, NetTop, Wall, Post }

    /// <summary>ボールが何かに当たったときの情報。</summary>
    public struct BallContact
    {
        public ContactKind kind;
        public Vector3 point;
        public float speed;
    }

    /// <summary>ボールと衝突する形状。衝突時は位置・速度を補正して true を返す。</summary>
    public interface IBallCollider
    {
        bool Collide(ref Vector3 pos, ref Vector3 vel, Vector3 prev, float radius, out BallContact contact);
    }
}
