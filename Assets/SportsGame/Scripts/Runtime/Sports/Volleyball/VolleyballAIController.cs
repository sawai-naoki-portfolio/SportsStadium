using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// CPU(相手チームと味方)の判断。チームの計画に従って、触る番ならボールへ、そうでなければ次のプレーや守備の位置へ動く。
    /// </summary>
    public class VolleyballAIController : IVolleyballController
    {
        const float AttackLunge = 1f;
        const float BlockLead = 0.05f;

        readonly IVolleyballView view;
        readonly VolleyballConfig cfg;
        readonly float reactionTime, judge, blockSkill, spikePower, jumpJitter, servePower;

        float reaction, serveTimer, serveHitVy, jumpLead;
        bool leaveDecided, leaveBall, blockDecided, willBlock, jumpPlanned;

        public VolleyballAIController(IVolleyballView view, VolleyballConfig cfg, float skill)
        {
            this.view = view;
            this.cfg = cfg;
            reactionTime = VolleyballConfig.ForSkill(cfg.aiReaction, skill);
            judge = VolleyballConfig.ForSkill(cfg.aiJudge, skill);
            blockSkill = VolleyballConfig.ForSkill(cfg.aiBlockSkill, skill);
            spikePower = VolleyballConfig.ForSkill(cfg.aiSpikePower, skill);
            servePower = VolleyballConfig.ForSkill(cfg.aiServePower, skill);
            PassError = VolleyballConfig.ForSkill(cfg.aiPassError, skill);
            SpikeError = VolleyballConfig.ForSkill(cfg.aiSpikeError, skill);
            ServeError = VolleyballConfig.ForSkill(cfg.aiServeError, skill);
            jumpJitter = Mathf.Lerp(0.09f, 0.03f, skill);
            ResetServe();
        }

        public bool IsHuman => false;
        public float PassError { get; }
        public float SpikeError { get; }
        public float ServeError { get; }

        void ResetServe()
        {
            serveTimer = Random.Range(1f, 1.7f);
            serveHitVy = Random.Range(-1.2f, 0.2f);
        }

        public void OnBallCrossed(VolleyballPlayer self)
        {
            reaction = reactionTime;
            leaveDecided = leaveBall = false;
        }

        public void TickRally(VolleyballPlayer self, float dt)
        {
            var plan = view.PlanFor(self.Team);
            var opp = view.PlanFor(1 - self.Team);
            Vector3 look = view.Ball.pos;
            if (plan.valid && plan.toucher == self.Index)
            {
                PlayBall(self, plan, dt);
                return;
            }
            jumpPlanned = false;
            if (self.Intent == VbIntent.Pass) self.ClearIntent();
            if (plan.valid) self.MoveTo(SupportSpot(self, plan), dt, look);
            else if (opp.valid) Defend(self, opp, dt);
            else self.MoveTo(BaseSpot(self), dt, look, 0.7f);
        }

        void PlayBall(VolleyballPlayer self, VolleyballPlan plan, float dt)
        {
            Vector3 look = view.Ball.pos;
            if (reaction > 0f)
            {
                reaction -= dt;
                self.Move(Vector3.zero, dt, look);
                return;
            }
            if (plan.touch == VbTouch.Receive && !leaveDecided)
            {
                leaveDecided = true;
                leaveBall = !plan.landingIn && Random.value < judge;
            }
            if (leaveBall)
            {
                self.ClearIntent();
                self.Move(Vector3.zero, dt, look);
                return;
            }
            self.MoveTo(plan.spot, dt, look);
            if (plan.touch != VbTouch.Attack || plan.lowBall)
            {
                self.SetIntent(VbIntent.Pass);
                return;
            }
            if (!jumpPlanned)
            {
                jumpPlanned = true;
                jumpLead = Random.Range(-jumpJitter, jumpJitter);
            }
            float rise = VolleyballPlayer.RiseTime(cfg.jumpSpeed);
            if (!self.Airborne && plan.contactTime - rise <= jumpLead)
            {
                self.Jump(cfg.jumpSpeed, AttackLunge);
                self.SetIntent(VbIntent.Spike);
            }
        }

        /// <summary>味方が触る番のときの次の準備位置。</summary>
        Vector3 SupportSpot(VolleyballPlayer self, VolleyballPlan plan)
        {
            switch (plan.touch)
            {
                case VbTouch.Receive: return VolleyballShots.SetSpot(self);
                case VbTouch.Set: return new Vector3(Mathf.Clamp(self.Pos.x, -2.8f, 2.8f), 0f, -self.Forward * 3f);
                default: return new Vector3(self.Pos.x * 0.5f, 0f, -self.Forward * 3.5f);
            }
        }

        /// <summary>相手の攻撃中: ブロック役はネット際、もう一人は後方で守る。</summary>
        void Defend(VolleyballPlayer self, VolleyballPlan opp, float dt)
        {
            Vector3 look = view.Ball.pos;
            if (!IsBlocker(self) || opp.touch == VbTouch.Receive)
            {
                blockDecided = false;
                float x = opp.touch == VbTouch.Attack ? -opp.contactPoint.x * 0.3f : 0f;
                self.MoveTo(new Vector3(Mathf.Clamp(x + (self.Index % 2 == 0 ? -1.5f : 1.5f), -3f, 3f), 0f, -self.Forward * 5.2f), dt, look);
                return;
            }
            float bx = opp.touch == VbTouch.Attack ? opp.contactPoint.x : 0f;
            self.MoveTo(new Vector3(Mathf.Clamp(bx, -3.5f, 3.5f), 0f, -self.Forward * 0.45f), dt, look);
            if (opp.touch != VbTouch.Attack) return;
            if (!blockDecided)
            {
                blockDecided = true;
                willBlock = Random.value < blockSkill;
            }
            if (willBlock && !self.Airborne && opp.contactTime - VolleyballPlayer.RiseTime(cfg.jumpSpeed) <= BlockLead)
                self.Jump(cfg.jumpSpeed);
        }

        bool IsBlocker(VolleyballPlayer self)
        {
            var partner = view.Partner(self);
            float mine = Mathf.Abs(self.Pos.z), theirs = Mathf.Abs(partner.Pos.z);
            if (view.IsHumanControlled(partner)) return theirs > 3f; // 味方: プレイヤーが後ろにいるときだけブロックに出る
            return mine < theirs || (Mathf.Approximately(mine, theirs) && self.Index < partner.Index);
        }

        Vector3 BaseSpot(VolleyballPlayer self) => new Vector3(self.Index % 2 == 0 ? -2f : 2f, 0f, -self.Forward * 4.5f);

        public Vector2 AttackAim(VolleyballPlayer self, out bool feint)
        {
            feint = Random.value < 0.15f;
            float side = Random.value < 0.5f ? -1f : 1f;
            return new Vector2(side * Random.Range(0.35f, 1f), Random.Range(-0.3f, 1f));
        }

        public float SpikePower(float quality) => spikePower * (0.7f + 0.3f * quality);

        public Vector3 PreServeMove(VolleyballPlayer self) => Vector3.zero;

        public bool WantsToss(VolleyballPlayer self, float dt)
        {
            serveTimer -= dt;
            return serveTimer <= 0f;
        }

        public bool WantsServeHit(VolleyballPlayer self, out float quality, out Vector2 aim)
        {
            quality = Random.Range(0.55f, 1f) * (0.6f + 0.4f * servePower);
            aim = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.2f, 1f));
            if (view.Ball.vel.y > serveHitVy) return false;
            ResetServe();
            return true;
        }
    }
}
