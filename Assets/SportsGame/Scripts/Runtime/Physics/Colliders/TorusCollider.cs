using UnityEngine;

namespace SportsGame
{
    /// <summary>水平なリング(バスケットのリム)。</summary>
    public class TorusCollider : IBallCollider
    {
        readonly Vector3 center;
        readonly float ringRadius, tube;
        public float restitution = 0.55f;
        public float friction = 0.8f;

        public TorusCollider(Vector3 center, float ringRadius, float tube)
        {
            this.center = center;
            this.ringRadius = ringRadius;
            this.tube = tube;
        }

        public bool Collide(ref Vector3 pos, ref Vector3 vel, Vector3 prev, float r, out BallContact contact)
        {
            contact = default;
            Vector3 rel = pos - center;
            Vector3 planar = new Vector3(rel.x, 0f, rel.z);
            if (planar.sqrMagnitude < 1e-8f) return false;
            Vector3 q = center + planar.normalized * ringRadius;
            Vector3 d = pos - q;
            float dist = d.magnitude;
            float minD = r + tube;
            if (dist >= minD || dist < 1e-6f) return false;
            Vector3 n = d / dist;
            pos = q + n * (minD + 0.0005f);
            float vn = Vector3.Dot(vel, n);
            if (vn >= 0f) return false;
            Vector3 vN = n * vn;
            vel = (vel - vN) * friction - vN * restitution;
            contact = new BallContact { kind = ContactKind.Rim, point = q, speed = -vn };
            return true;
        }
    }
}
