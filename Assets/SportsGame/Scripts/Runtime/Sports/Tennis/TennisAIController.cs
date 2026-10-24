using UnityEngine;

namespace SportsGame
{
    /// <summary>CPU の判断(先読みで打点へ移動・ショット選択・アウトボールの見送り)。</summary>
    public class TennisAIController : ITennisController
    {
        const float ReplanInterval = 0.15f;
        const float HomeDepth = 12.4f;

        readonly ITennisView view;
        readonly TennisConfig cfg;
        readonly float skill;
        const float MissErrorScale = 3f;
        const float MissNetChance = 0.35f;

        readonly float reactionTime, error, netChance, missChance, powerMin, powerMax, judge, aimAway;

        float reaction, replanTimer, serveTimer, serveHitVy;
        Vector3 target;
        bool hasTarget, leaveDecided, leaveBall;

        public TennisAIController(ITennisView view, TennisConfig cfg, float skill)
        {
            this.view = view;
            this.cfg = cfg;
            this.skill = skill;
            reactionTime = TennisConfig.ForSkill(cfg.aiReaction, skill);
            error = TennisConfig.ForSkill(cfg.aiError, skill);
            netChance = TennisConfig.ForSkill(cfg.aiNetChance, skill);
            missChance = TennisConfig.ForSkill(cfg.aiMissChance, skill);
            powerMin = TennisConfig.ForSkill(cfg.aiPowerMin, skill);
            powerMax = TennisConfig.ForSkill(cfg.aiPowerMax, skill);
            judge = TennisConfig.ForSkill(cfg.aiJudge, skill);
            aimAway = TennisConfig.ForSkill(cfg.aiAimAway, skill);
            ResetServeTimer();
        }

        public bool IsHuman => false;
        public float ServeError => error * 0.5f;

        void ResetServeTimer()
        {
            serveTimer = Random.Range(0.9f, 1.6f);
            serveHitVy = Random.Range(-0.9f, 0.4f);
        }

        public void OnOpponentHit(TennisPlayer self)
        {
            reaction = reactionTime;
            replanTimer = 0f;
            hasTarget = false;
            leaveDecided = false;
            leaveBall = false;
        }

        public void TickRally(TennisPlayer self, float dt)
        {
            var ball = view.Ball;
            bool incoming = view.Referee.LastHitter == view.Opponent(self).Side;
            if (!incoming)
            {
                self.ClearSwing();
                Vector3 home = new Vector3(Mathf.Clamp(ball.pos.x * 0.3f, -2f, 2f), 0f, -self.Forward * HomeDepth);
                self.MoveTo(home, dt, ball.pos, 0.8f);
                return;
            }
            if (reaction > 0f)
            {
                reaction -= dt;
                self.Move(Vector3.zero, dt, ball.pos);
                return;
            }
            replanTimer -= dt;
            if (replanTimer <= 0f)
            {
                replanTimer = ReplanInterval;
                var plan = TennisIntercept.Compute(ball, self, view.Referee, 0f);
                target = plan.stand;
                hasTarget = true;
                DecideLeave(self, plan);
            }
            if (hasTarget) self.MoveTo(target, dt, ball.pos);
            if (leaveBall) self.ClearSwing();
            else if (!self.Swing.active) self.QueueSwing(TShot.Topspin);
        }

        /// <summary>まだバウンドしていないボールがアウトになりそうなら、確率で見送る。</summary>
        void DecideLeave(TennisPlayer self, TennisIntercept.Plan plan)
        {
            if (leaveDecided || !plan.hasBounce || view.Referee.BouncesSinceHit != 0 || view.Referee.IsServe) return;
            leaveDecided = true;
            if (TennisDims.InSingles(plan.firstBounce, self.Side)) return;
            float over = Mathf.Max(Mathf.Abs(plan.firstBounce.x) - TennisDims.SinglesHalfWidth, Mathf.Abs(plan.firstBounce.z) - TennisDims.HalfLength);
            leaveBall = Random.value < (over > 0.3f ? judge + 0.1f : judge * 0.5f);
        }

        public ShotRequest DecideShot(TennisPlayer self, HitGeometry geometry)
        {
            var opp = view.Opponent(self);
            TShot shot = ChooseShot(opp, geometry.height);
            // 体勢が悪いほどミスショットになりやすい(大きく狙いが外れるか、ネットにかける)
            bool miss = Random.value < missChance * (2f - geometry.Quality);
            return new ShotRequest
            {
                shot = shot,
                power = Random.Range(powerMin, powerMax),
                aim = ChooseAim(opp, shot),
                error = miss ? error * MissErrorScale : error,
                netChance = miss ? MissNetChance : netChance * (1.4f - geometry.Quality),
            };
        }

        TShot ChooseShot(TennisPlayer opp, float height)
        {
            if (height > 2.05f) return TShot.Smash;
            float r = Random.value;
            if (Mathf.Abs(opp.Pos.z) < 5.5f && r < 0.45f) return TShot.Lob;
            if (r < Mathf.Lerp(0.25f, 0.15f, skill)) return TShot.Slice;
            if (r < Mathf.Lerp(0.33f, 0.2f, skill)) return TShot.Lob;
            return TShot.Topspin;
        }

        Vector2 ChooseAim(TennisPlayer opp, TShot shot)
        {
            float away = -Mathf.Sign(opp.Pos.x + 0.01f);
            float x = Random.value < aimAway ? away * Random.Range(0.55f, 1f) : Random.Range(-0.8f, 0.8f);
            float y = Random.Range(-0.1f, 0.9f);
            if (shot == TShot.Slice && Random.value < 0.25f * skill) y = -1f;
            return new Vector2(x, y);
        }

        public Vector3 PreServeMove(TennisPlayer self) => Vector3.zero;

        public bool WantsToss(TennisPlayer self, float dt)
        {
            serveTimer -= dt;
            if (serveTimer > 0f) return false;
            ResetServeTimer();
            return true;
        }

        public bool WantsServeHit(TennisPlayer self, out float quality, out float aimX)
        {
            quality = Random.Range(Mathf.Lerp(0.45f, 0.8f, skill), 1f);
            aimX = Random.Range(-1f, 1f);
            return view.Ball.vel.y <= serveHitVy;
        }
    }
}
