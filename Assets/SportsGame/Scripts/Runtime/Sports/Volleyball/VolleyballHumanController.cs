using UnityEngine;

namespace SportsGame
{
    /// <summary>プレイヤーの操作(A=レシーブ/トス/フェイント/サーブ、B=ジャンプ(スパイク・ブロック))。</summary>
    public class VolleyballHumanController : IVolleyballController
    {
        const float AttackLunge = 1.2f;

        readonly IVolleyballView view;
        readonly VolleyballConfig cfg;

        public VolleyballHumanController(IVolleyballView view, VolleyballConfig cfg)
        {
            this.view = view;
            this.cfg = cfg;
        }

        public bool IsHuman => true;
        public float PassError => cfg.playerPassError;
        public float SpikeError => cfg.playerSpikeError;
        public float ServeError => cfg.playerServeError;

        public void TickRally(VolleyballPlayer self, float dt)
        {
            var plan = view.PlanFor(self.Team);
            bool myTouch = plan.valid && plan.toucher == self.Index;
            if (GameInput.A.Down) self.SetIntent(VbIntent.Pass);
            if (GameInput.B.Down && !self.Airborne)
            {
                self.Jump(cfg.jumpSpeed, myTouch && plan.touch == VbTouch.Attack ? AttackLunge : 0f);
                self.SetIntent(VbIntent.Spike);
            }
            if (self.Intent == VbIntent.Pass && Time.time - self.IntentTime > cfg.intentWindow) self.ClearIntent();

            Vector3 move = GameInput.MoveWorld(view.CameraTransform);
            if (move.sqrMagnitude < 0.01f && GameSettings.MoveAssist && myTouch) self.MoveTo(plan.spot, dt, view.Ball.pos, cfg.assistSpeed);
            else self.Move(move, dt, view.Ball.pos);
        }

        public Vector2 AttackAim(VolleyballPlayer self, out bool feint)
        {
            feint = self.Intent == VbIntent.Pass;
            return GameInput.Move;
        }

        public float SpikePower(float quality) => Mathf.Lerp(0.45f, 1f, quality);

        public void OnBallCrossed(VolleyballPlayer self) { }

        public Vector3 PreServeMove(VolleyballPlayer self) => GameInput.MoveWorld(view.CameraTransform) * 0.5f;

        public bool WantsToss(VolleyballPlayer self, float dt) => GameInput.A.Down || GameInput.B.Down;

        public bool WantsServeHit(VolleyballPlayer self, out float quality, out Vector2 aim)
        {
            aim = GameInput.Move;
            quality = 0f;
            if (!GameInput.A.Down && !GameInput.B.Down) return false;
            var ball = view.Ball;
            quality = 1f - Mathf.Clamp01(Mathf.Abs(ball.pos.y - (view.TossApex - 0.1f)) / 0.9f);
            return true;
        }
    }
}
