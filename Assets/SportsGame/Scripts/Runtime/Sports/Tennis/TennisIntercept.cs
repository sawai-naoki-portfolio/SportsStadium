using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// 飛んでくるボールを先読みし、間に合う打点への立ち位置を求める(CPU と移動アシストで共用)。
    /// </summary>
    public static class TennisIntercept
    {
        const float LookAhead = 2.8f;
        const float MinHeight = 0.3f, MaxHeight = 1.75f;
        const float SideOffset = 0.85f, BackOffset = 0.35f;
        const float ForehandBias = 0.4f;

        public struct Plan
        {
            public Vector3 stand;
            public bool reachable;
            public bool hasBounce;
            public Vector3 firstBounce;
        }

        public static Plan Compute(BallSim ball, TennisPlayer p, TennisReferee referee, float reactionLeft)
        {
            var plan = new Plan { stand = p.Pos };
            float bestSlack = float.NegativeInfinity;
            int startBounces = referee.BouncesSinceHit;
            bool canVolley = !referee.IsServe && Mathf.Abs(p.Pos.z) < 7f;
            float speed = Mathf.Max(0.1f, p.MaxSpeed * 0.9f);

            BallPredictor.Simulate(ball, LookAhead, s =>
            {
                if (s.justBounced && !plan.hasBounce)
                {
                    plan.hasBounce = true;
                    plan.firstBounce = s.pos;
                }
                int bounces = startBounces + s.bounces;
                if (bounces >= 2) return true;
                Vector3 b = s.pos;
                if (b.z * p.Forward > 0.2f) return false;
                if (bounces == 0 && !canVolley) return false;
                if (b.y < MinHeight || b.y > MaxHeight) return false;

                Vector3 fh = b - p.RightDir * SideOffset - p.ForwardDir * BackOffset;
                Vector3 bh = b + p.RightDir * SideOffset - p.ForwardDir * BackOffset;
                fh.y = bh.y = 0f;
                float dF = Vector3.Distance(p.Pos, fh), dB = Vector3.Distance(p.Pos, bh);
                Vector3 stand = dF <= dB + ForehandBias ? fh : bh;
                float slack = s.time - (Mathf.Min(dF, dB) / speed + reactionLeft);
                if (slack > bestSlack)
                {
                    bestSlack = slack;
                    plan.stand = stand;
                }
                if (slack < 0f) return false;
                plan.reachable = true;
                return true;
            });
            return plan;
        }
    }
}
