using UnityEngine;

namespace SportsGame
{
    /// <summary>ボールを箱の内側に閉じ込める壁(体育館の壁など)。床は BallSim が扱う。</summary>
    public class BoundsCollider : IBallCollider
    {
        readonly Vector3 min, max;
        public float restitution = 0.5f;

        public BoundsCollider(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Collide(ref Vector3 pos, ref Vector3 vel, Vector3 prev, float r, out BallContact contact)
        {
            contact = default;
            bool hit = Reflect(ref pos.x, ref vel.x, min.x + r, max.x - r);
            hit |= Reflect(ref pos.z, ref vel.z, min.z + r, max.z - r);
            if (pos.y > max.y - r)
            {
                pos.y = max.y - r;
                if (vel.y > 0f) { vel.y = -vel.y * restitution; hit = true; }
            }
            if (hit) contact = new BallContact { kind = ContactKind.Wall, point = pos, speed = vel.magnitude };
            return hit;
        }

        bool Reflect(ref float p, ref float v, float lo, float hi)
        {
            if (p < lo)
            {
                p = lo;
                if (v < 0f) { v = -v * restitution; return true; }
            }
            else if (p > hi)
            {
                p = hi;
                if (v > 0f) { v = -v * restitution; return true; }
            }
            return false;
        }
    }
}
