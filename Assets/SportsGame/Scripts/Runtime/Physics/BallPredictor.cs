using UnityEngine;

namespace SportsGame
{
    /// <summary>ボールの未来の軌道を先読みする(AI の移動目標・落下地点マーカー用)。</summary>
    public static class BallPredictor
    {
        public const float Step = 1f / 60f;

        /// <summary>先読み中の1サンプル。bounces は先読み開始からのバウンド回数。</summary>
        public struct Sample
        {
            public Vector3 pos, vel;
            public float time;
            public int bounces;
            public bool justBounced;
        }

        /// <summary>visit が true を返すか maxTime に達するまで先読みする。</summary>
        public static void Simulate(BallSim ball, float maxTime, System.Func<Sample, bool> visit)
        {
            if (ball == null || visit == null) return;
            var sim = ball.Clone();
            sim.frozen = false;
            var s = new Sample();
            int steps = Mathf.CeilToInt(maxTime / Step);
            for (int i = 0; i < steps; i++)
            {
                float prevVy = sim.vel.y;
                sim.Step(Step);
                s.time += Step;
                s.justBounced = prevVy < 0f && sim.vel.y >= 0f && sim.pos.y < sim.radius + 0.2f;
                if (s.justBounced) s.bounces++;
                s.pos = sim.pos;
                s.vel = sim.vel;
                if (visit(s)) return;
            }
        }

        public static bool FirstBounce(BallSim ball, float maxTime, out Vector3 point, out float time)
        {
            Vector3 p = Vector3.zero;
            float t = 0f;
            bool found = false;
            Simulate(ball, maxTime, s =>
            {
                if (!s.justBounced) return false;
                p = s.pos;
                t = s.time;
                found = true;
                return true;
            });
            point = p;
            time = t;
            return found;
        }
    }
}
