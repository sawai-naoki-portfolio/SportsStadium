using UnityEngine;

namespace SportsGame
{
    public enum VbTouch { Receive, Set, Attack }

    /// <summary>チームの次のタッチの計画(誰が・どこで・いつ触るか)。</summary>
    public struct VolleyballPlan
    {
        public bool valid;
        public int team;
        public VbTouch touch;
        /// <summary>触る選手の index(0〜3)。</summary>
        public int toucher;
        /// <summary>触る選手が立つ位置。</summary>
        public Vector3 spot;
        /// <summary>ボールに触る予定の位置と、そこまでの時間。</summary>
        public Vector3 contactPoint;
        public float contactTime;
        /// <summary>何もしなければボールが落ちる地点。</summary>
        public Vector3 landing;
        public bool landingIn;
        /// <summary>アタックの番だがトスが低く、打てないのでつなぐしかない。</summary>
        public bool lowBall;
    }

    /// <summary>
    /// 飛んでくるボールを先読みして、チームの誰がどのタッチをするかを決める(CPU・味方・移動アシストで共用)。
    /// レシーブ → トス → アタックの順で、直前に触っていない選手が次を担当する。
    /// </summary>
    public static class VolleyballPlanner
    {
        const float LookAhead = 4f;
        const float ReceiveHeight = 0.9f, SetHeight = 2.1f;
        const float AttackBackOffset = 0.35f;

        public static VolleyballPlan Compute(BallSim ball, VolleyballReferee referee, VolleyballPlayer[] players, int team, VolleyballConfig cfg)
        {
            var plan = new VolleyballPlan { team = team, toucher = -1 };
            int touchIndex = referee.TouchIndexFor(team);
            if (touchIndex > 2) return plan;
            plan.touch = (VbTouch)touchIndex;
            float contactHeight = plan.touch == VbTouch.Receive ? ReceiveHeight : plan.touch == VbTouch.Set ? SetHeight : cfg.hitHeight;

            // 理想の高さで触れなければ、低い位置(レシーブの高さ)でつなぐ
            bool foundHigh = false, foundLow = false;
            Vector3 high = default, low = default;
            float highTime = 0f, lowTime = 0f;
            BallPredictor.Simulate(ball, LookAhead, s =>
            {
                bool descending = VolleyballDims.SideOf(s.pos) == team && s.vel.y < 0f;
                if (descending && !foundHigh && s.pos.y <= contactHeight) { foundHigh = true; high = s.pos; highTime = s.time; }
                if (descending && !foundLow && s.pos.y <= ReceiveHeight) { foundLow = true; low = s.pos; lowTime = s.time; }
                if (s.justBounced || s.pos.y <= ball.radius + 0.01f)
                {
                    plan.landing = s.pos;
                    plan.landingIn = VolleyballDims.InCourt(s.pos) && VolleyballDims.SideOf(s.pos) == team;
                    return true;
                }
                return false;
            });
            if (!foundHigh && !foundLow) return plan;
            plan.contactPoint = foundHigh ? high : low;
            plan.contactTime = foundHigh ? highTime : lowTime;
            plan.lowBall = !foundHigh && plan.touch == VbTouch.Attack;

            plan.toucher = ChooseToucher(players, team, referee, plan);
            if (plan.toucher < 0) return plan;
            Vector3 spot = plan.contactPoint;
            if (plan.touch == VbTouch.Attack && !plan.lowBall) spot -= players[plan.toucher].ForwardDir * AttackBackOffset;
            plan.spot = new Vector3(spot.x, 0f, spot.z);
            plan.valid = true;
            return plan;
        }

        /// <summary>1回目は間に合う選手(近い方)、2・3回目は直前に触っていない選手。</summary>
        static int ChooseToucher(VolleyballPlayer[] players, int team, VolleyballReferee referee, VolleyballPlan plan)
        {
            int a = team * 2, b = a + 1;
            if (plan.touch != VbTouch.Receive) return referee.LastPlayer == a ? b : a;
            float ta = Vector3.Distance(Flat(players[a].Pos), Flat(plan.contactPoint)) / players[a].MaxSpeed;
            float tb = Vector3.Distance(Flat(players[b].Pos), Flat(plan.contactPoint)) / players[b].MaxSpeed;
            return ta <= tb ? a : b;
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    }
}
