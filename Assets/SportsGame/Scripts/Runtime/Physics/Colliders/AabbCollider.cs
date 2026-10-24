using UnityEngine;

namespace SportsGame
{
    /// <summary>軸平行の箱(バックボード・支柱など)。</summary>
    public class AabbCollider : IBallCollider
    {
        readonly Vector3 min, max;
        readonly ContactKind kind;
        public float restitution = 0.6f;
        public float friction = 0.85f;

        public AabbCollider(Vector3 center, Vector3 size, ContactKind kind)
        {
            min = center - size * 0.5f;
            max = center + size * 0.5f;
            this.kind = kind;
        }

        Vector3 Clamp(Vector3 p) => new Vector3(Mathf.Clamp(p.x, min.x, max.x), Mathf.Clamp(p.y, min.y, max.y), Mathf.Clamp(p.z, min.z, max.z));

        public bool Collide(ref Vector3 pos, ref Vector3 vel, Vector3 prev, float r, out BallContact contact)
        {
            contact = default;
            Vector3 c = Clamp(pos);
            Vector3 d = pos - c;
            float d2 = d.sqrMagnitude;
            if (d2 > r * r) return false;
            Vector3 n;
            if (d2 < 1e-10f)
            {
                // 中心が箱の内部に入った: 前フレームの位置から押し出す
                c = Clamp(prev);
                Vector3 back = prev - c;
                n = back.sqrMagnitude > 1e-10f ? back.normalized : Vector3.up;
            }
            else n = d / Mathf.Sqrt(d2);
            pos = c + n * (r + 0.0005f);
            float vn = Vector3.Dot(vel, n);
            if (vn >= 0f) return false;
            Vector3 vN = n * vn;
            vel = (vel - vN) * friction - vN * restitution;
            contact = new BallContact { kind = kind, point = c, speed = -vn };
            return true;
        }
    }
}
