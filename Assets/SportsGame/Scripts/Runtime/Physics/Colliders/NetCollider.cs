using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// z=0 に張られた垂直のネット(テニス・バレー)。上端の高さは中央と端で変えられる(テニスのたるみ)。
    /// 先読みと実際の挙動を一致させるため乱数は使わない。
    /// </summary>
    public class NetCollider : IBallCollider
    {
        public float halfWidth = 6.4f;
        public float bottom;
        public float topCenter = 0.914f;
        public float topEdge = 1.07f;
        public float restitution = 0.12f;

        public float TopAt(float x)
        {
            float k = Mathf.Clamp01(Mathf.Abs(x) / halfWidth);
            return Mathf.Lerp(topCenter, topEdge, k * k);
        }

        public bool Collide(ref Vector3 pos, ref Vector3 vel, Vector3 prev, float r, out BallContact contact)
        {
            contact = default;
            if (Mathf.Abs(pos.x) > halfWidth) return false;
            bool crossed = (prev.z < 0f && pos.z >= 0f) || (prev.z > 0f && pos.z <= 0f) || Mathf.Abs(pos.z) < r;
            if (!crossed) return false;
            float top = TopAt(pos.x);
            if (pos.y < bottom - r || pos.y > top + r) return false;

            if (pos.y > top)
            {
                // 白帯に当たった(ネットイン): 減速して上へ跳ね、たまに手前へ戻る
                float dir = Mathf.Sign(vel.z);
                if (Mathf.Repeat(pos.x * 13.37f + vel.magnitude, 1f) < 0.35f) dir = -dir;
                vel.z = Mathf.Abs(vel.z) * 0.35f * dir;
                vel.x *= 0.7f;
                vel.y = Mathf.Abs(vel.y) * 0.25f + 1.2f;
                pos.z = dir * (r + 0.002f);
                pos.y = top + r * 0.6f;
                contact = new BallContact { kind = ContactKind.NetTop, point = pos, speed = vel.magnitude };
                return true;
            }
            float side = prev.z != 0f ? Mathf.Sign(prev.z) : -Mathf.Sign(vel.z);
            pos.z = side * (r + 0.002f);
            vel.z = -vel.z * restitution;
            vel.x *= 0.45f;
            vel.y *= 0.35f;
            contact = new BallContact { kind = ContactKind.Net, point = pos, speed = vel.magnitude };
            return true;
        }
    }
}
