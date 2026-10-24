using UnityEngine;

namespace SportsGame
{
    /// <summary>プレイヤーの操作(スティック移動・ボタンでスイング予約・移動アシスト)。</summary>
    public class TennisHumanController : ITennisController
    {
        const float SwingExpire = 1.8f;
        const float ReplanInterval = 0.12f;
        const float AimDeadZone = 0.25f;

        readonly ITennisView view;
        readonly TennisConfig cfg;
        float replanTimer;
        Vector3 assistTarget;
        bool hasAssist;

        public TennisHumanController(ITennisView view, TennisConfig cfg)
        {
            this.view = view;
            this.cfg = cfg;
        }

        public bool IsHuman => true;
        public float ServeError => cfg.playerServeError;

        public void TickRally(TennisPlayer self, float dt)
        {
            if (GameInput.A.Down) self.QueueSwing(TShot.Topspin);
            if (GameInput.B.Down) self.QueueSwing(TShot.Slice);
            if (GameInput.C.Down) self.QueueSwing(TShot.Lob);
            if (self.Swing.active && Time.time - self.Swing.since > SwingExpire) self.ClearSwing();

            Vector3 move = GameInput.MoveWorld(view.CameraTransform);
            bool incoming = view.Referee.LastHitter == view.Opponent(self).Side;
            if (move.sqrMagnitude < 0.01f && GameSettings.MoveAssist && incoming)
            {
                replanTimer -= dt;
                if (replanTimer <= 0f)
                {
                    replanTimer = ReplanInterval;
                    assistTarget = TennisIntercept.Compute(view.Ball, self, view.Referee, 0f).stand;
                    hasAssist = true;
                }
                if (hasAssist) self.MoveTo(assistTarget, dt, view.Ball.pos, cfg.assistSpeed);
                else self.Move(Vector3.zero, dt, view.Ball.pos);
                return;
            }
            replanTimer = 0f;
            self.Move(move, dt, view.Ball.pos);
        }

        public ShotRequest DecideShot(TennisPlayer self, HitGeometry geometry)
        {
            Vector2 stick = GameInput.Move;
            // スティックを倒していなければクロスへ打ち返す
            float aimX = Mathf.Abs(stick.x) > AimDeadZone ? stick.x
                : Mathf.Clamp(-self.Pos.x / TennisDims.SinglesHalfWidth, -1f, 1f) * 0.55f + Random.Range(-0.2f, 0.2f);
            return new ShotRequest
            {
                shot = self.Swing.shot,
                power = 0.2f + 0.8f * Mathf.Clamp01((Time.time - self.Swing.since) / cfg.chargeTime),
                aim = new Vector2(aimX, stick.y),
                error = cfg.playerError,
                netChance = geometry.Quality < 0.35f ? 0.16f : 0.015f,
            };
        }

        public void OnOpponentHit(TennisPlayer self) => hasAssist = false;

        public Vector3 PreServeMove(TennisPlayer self) => GameInput.MoveWorld(view.CameraTransform) * 0.5f;

        public bool WantsToss(TennisPlayer self, float dt) => GameInput.A.Down || GameInput.B.Down;

        public bool WantsServeHit(TennisPlayer self, out float quality, out float aimX)
        {
            quality = 0f;
            aimX = GameInput.Move.x;
            if (!GameInput.A.Down && !GameInput.B.Down) return false;
            var ball = view.Ball;
            quality = 1f - Mathf.Clamp01(Mathf.Abs(ball.pos.y - (view.TossApex - 0.08f)) / 0.85f);
            if (ball.vel.y > 0f) quality *= 0.9f;
            return true;
        }
    }
}
