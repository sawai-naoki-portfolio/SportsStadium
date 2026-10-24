using UnityEngine;

namespace SportsGame
{
    /// <summary>放物運動の解析解(初速度の逆算など)。空気抵抗なし。</summary>
    public static class BallMath
    {
        /// <summary>時間 t で from から to に到達する初速度。</summary>
        public static Vector3 SolveByTime(Vector3 from, Vector3 to, float t, float g)
        {
            t = Mathf.Max(0.05f, t);
            Vector3 d = to - from;
            return new Vector3(d.x / t, (d.y + 0.5f * g * t * t) / t, d.z / t);
        }

        /// <summary>最高到達点 apexY を通って、下降中に to へ到達する初速度。</summary>
        public static Vector3 SolveByApex(Vector3 from, Vector3 to, float apexY, float g, out float flightTime)
        {
            apexY = Mathf.Max(apexY, Mathf.Max(from.y, to.y) + 0.05f);
            float vy = Mathf.Sqrt(2f * g * (apexY - from.y));
            flightTime = vy / g + Mathf.Sqrt(2f * (apexY - to.y) / g);
            Vector3 d = to - from;
            return new Vector3(d.x / flightTime, vy, d.z / flightTime);
        }

        /// <summary>仰角 angleDeg で from から to を通過する初速度。到達不可能なら false。</summary>
        public static bool SolveByAngle(Vector3 from, Vector3 to, float angleDeg, float g, out Vector3 velocity)
        {
            Vector3 d = to - from;
            Vector3 dh = new Vector3(d.x, 0f, d.z);
            float x = dh.magnitude;
            float a = angleDeg * Mathf.Deg2Rad;
            float cos = Mathf.Cos(a);
            float denom = 2f * cos * cos * (x * Mathf.Tan(a) - d.y);
            velocity = Vector3.zero;
            if (denom <= 1e-4f || x < 1e-3f) return false;
            float v = Mathf.Sqrt(g * x * x / denom);
            velocity = dh / x * (v * cos) + Vector3.up * (v * Mathf.Sin(a));
            return true;
        }
    }
}
