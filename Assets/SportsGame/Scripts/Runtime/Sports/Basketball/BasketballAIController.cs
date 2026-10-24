using UnityEngine;

namespace SportsGame
{
    /// <summary>CPU の判断(攻撃の狙い・シュート判断・守備位置取り・ブロック・スティール・リバウンド)。</summary>
    public class BasketballAIController : IBasketballController
    {
        enum Intent { Drive, MidRange, Three }

        const float ThinkInterval = 0.22f;
        const float ClearRadius = 7.4f;

        readonly IBasketballView view;
        readonly BasketballConfig cfg;
        readonly float speedScale, reaction, timingSigma, stealRate, blockChance, crossChance;

        Intent intent;
        Vector3 spot, lagPos;
        float thinkTimer, releaseMeter = 1f, blockTimer = -1f;
        bool hadBall, blockRolled;

        public BasketballAIController(IBasketballView view, BasketballConfig cfg, float skill)
        {
            this.view = view;
            this.cfg = cfg;
            speedScale = BasketballConfig.ForSkill(cfg.aiSpeedScale, skill);
            reaction = BasketballConfig.ForSkill(cfg.aiReaction, skill);
            timingSigma = BasketballConfig.ForSkill(cfg.aiTimingSigma, skill);
            stealRate = BasketballConfig.ForSkill(cfg.aiStealRate, skill);
            blockChance = BasketballConfig.ForSkill(cfg.aiBlockChance, skill);
            crossChance = BasketballConfig.ForSkill(cfg.aiCrossChance, skill);
        }

        public bool IsHuman => false;

        public BBCommand Decide(BasketballPlayer self, float dt)
        {
            var opp = view.Opponent(self);
            if (view.Holder == self)
            {
                if (!hadBall) OnGainBall();
                hadBall = true;
                return Offense(self, opp, dt);
            }
            hadBall = false;
            return view.Holder == opp ? Defense(self, opp, dt) : ChaseLooseBall(self);
        }

        void OnGainBall()
        {
            float r = Random.value;
            ChooseSpot(r < 0.45f ? Intent.Drive : r < 0.75f ? Intent.MidRange : Intent.Three);
            thinkTimer = Random.Range(0.25f, 0.5f);
        }

        void ChooseSpot(Intent next)
        {
            intent = next;
            float angle = Random.Range(200f, 340f) * Mathf.Deg2Rad;
            float radius = intent == Intent.Three ? 7.3f : 4.6f;
            spot = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        BBCommand Offense(BasketballPlayer self, BasketballPlayer opp, float dt)
        {
            var c = new BBCommand();
            if (self.shooting)
            {
                c.actionHeld = self.shotTimer / cfg.shotPeakTime < releaseMeter;
                c.actionUp = !c.actionHeld;
                return c;
            }
            if (view.NeedsClear)
            {
                Vector3 radial = -BasketballDims.ToHoop(self.Pos);
                c.move = MoveDir(self.Pos, BasketballDims.Hoop + radial * ClearRadius);
                c.sprint = true;
                return c;
            }

            float d = BasketballDims.HoopDistance(self.Pos);
            float open = Flat(opp.Pos - self.Pos).magnitude;
            bool blocked = open < 1.6f && Vector3.Dot(Flat(opp.Pos - self.Pos).normalized, BasketballDims.ToHoop(self.Pos)) > 0.5f;
            thinkTimer -= dt;
            if (thinkTimer <= 0f)
            {
                thinkTimer = ThinkInterval;
                if (ShouldShoot(self, d, open))
                {
                    c.actionDown = c.actionHeld = true;
                    releaseMeter = 1f + RandomUtil.Gauss() * timingSigma;
                    return c;
                }
                if (blocked && self.crossCooldown <= 0f && Random.value < crossChance) c.skillDown = true;
                if (blocked && intent != Intent.Drive && Vector3.Distance(self.Pos, spot) < 1f) intent = Intent.Drive;
                else if (blocked && intent == Intent.Drive && d < 2.5f) ChooseSpot(Intent.MidRange); // ゴール下で詰まったら引いて打ち直す
            }
            Vector3 target = intent == Intent.Drive || d < 3f ? BasketballDims.Hoop : spot;
            c.move = MoveDir(self.Pos, target);
            c.sprint = intent == Intent.Drive && self.Stamina > 0.3f && d > 2f;
            return c;
        }

        bool ShouldShoot(BasketballPlayer self, float d, float open)
        {
            if (view.ShotClock < 2.5f) return true;
            // 厳しくチェックされている間は打たずに崩しにいく
            var opp = view.Opponent(self);
            if (BasketballShotModel.Contest(self.Pos, opp.Pos, opp.Airborne) > 0.7f) return false;
            if (d < BasketballShotModel.LayupRange) return true;
            if (intent == Intent.MidRange && d < 5.6f && open > 1.3f) return true;
            if (intent == Intent.Three && BasketballDims.IsThree(self.Pos) && open > 1.7f && Vector3.Distance(self.Pos, spot) < 1.2f) return true;
            return open > 2.5f && d < 7.5f && Random.value < 0.3f;
        }

        BBCommand Defense(BasketballPlayer self, BasketballPlayer holder, float dt)
        {
            var c = new BBCommand();
            // 反応の遅れを表現するため、相手の位置を遅れて追いかける
            lagPos = Vector3.Lerp(lagPos, holder.Pos, Mathf.Clamp01(dt / reaction));
            float hd = BasketballDims.HoopDistance(lagPos);
            float guard = hd > 8.5f ? 1.8f : hd < 3f ? 0.8f : 1.15f;
            Vector3 target = lagPos + BasketballDims.ToHoop(lagPos) * guard;
            c.move = MoveDir(self.Pos, target) * speedScale;
            c.sprint = Flat(target - self.Pos).magnitude > 2.5f;

            float dist = Flat(holder.Pos - self.Pos).magnitude;
            if (holder.shooting)
            {
                if (!blockRolled)
                {
                    blockRolled = true;
                    blockTimer = dist < 2.4f && Random.value < blockChance ? reaction * 0.8f + Random.Range(0f, 0.1f) : -1f;
                }
                if (blockTimer >= 0f)
                {
                    blockTimer -= dt;
                    if (blockTimer < 0f) c.actionDown = true;
                }
            }
            else
            {
                blockRolled = false;
                blockTimer = -1f;
                if (dist < 1.2f && self.stealCooldown <= 0f && Random.value < stealRate * dt) c.skillDown = true;
            }
            return c;
        }

        BBCommand ChaseLooseBall(BasketballPlayer self)
        {
            var c = new BBCommand { move = MoveDir(self.Pos, view.LooseBallTarget), sprint = true };
            var b = view.Ball;
            if (Flat(b.pos - self.Pos).magnitude < 1.2f && b.pos.y > self.Reach && b.pos.y < self.Reach + 0.9f && b.vel.y < 0f) c.actionDown = true;
            return c;
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        static Vector3 MoveDir(Vector3 from, Vector3 to)
        {
            Vector3 d = Flat(to - from);
            float dist = d.magnitude;
            return dist > 0.05f ? d / dist * Mathf.Clamp01(dist / 0.5f) : Vector3.zero;
        }
    }
}
