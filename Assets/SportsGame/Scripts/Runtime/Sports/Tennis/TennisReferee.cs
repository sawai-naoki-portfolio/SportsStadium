using UnityEngine;

namespace SportsGame
{
    public enum RallyCall { None, ServeGood, Fault, Let, Point }

    public enum PointReason { None, Out, Net, Winner, Ace, DoubleFault }

    public struct RallyDecision
    {
        public RallyCall call;
        public int winner;
        public PointReason reason;

        public static RallyDecision Of(RallyCall call) => new RallyDecision { call = call, winner = -1 };
        public static RallyDecision PointTo(int winner, PointReason reason) => new RallyDecision { call = RallyCall.Point, winner = winner, reason = reason };
    }

    /// <summary>
    /// ラリーの審判。誰が最後に打ったか・バウンド回数・サーブの判定(フォルト/レット)を管理する。Unity の描画等には依存しない。
    /// </summary>
    public class TennisReferee
    {
        bool serveTouchedNet;

        public bool IsServe { get; private set; }
        public int LastHitter { get; private set; } = -1;
        public int BouncesSinceHit { get; private set; }
        public int RallyHits { get; private set; }
        public int Faults { get; private set; }
        public int Server { get; private set; }
        public float ServeBoxSign { get; private set; }

        public bool IsSecondServe => Faults == 1;

        /// <summary>新しいポイントの開始(フォルト数もリセット)。</summary>
        public void BeginPoint()
        {
            Faults = 0;
            ResetRally();
        }

        void ResetRally()
        {
            IsServe = false;
            serveTouchedNet = false;
            LastHitter = -1;
            BouncesSinceHit = 0;
            RallyHits = 0;
        }

        /// <summary>サーブを打った。boxSign はサービスボックスが x の正側(+1)か負側(-1)か。</summary>
        public void BeginServe(int server, float boxSign)
        {
            Server = server;
            ServeBoxSign = boxSign;
            IsServe = true;
            serveTouchedNet = false;
            LastHitter = server;
            BouncesSinceHit = 0;
            RallyHits = 1;
        }

        /// <summary>その選手が今ボールを打ってよいか(連続打ち禁止・サーブはワンバウンド後)。</summary>
        public bool CanHit(int player) => !IsServe && LastHitter >= 0 && LastHitter != player;

        public void RegisterHit(int player)
        {
            LastHitter = player;
            BouncesSinceHit = 0;
            RallyHits++;
        }

        public void RegisterNetTouch()
        {
            if (IsServe) serveTouchedNet = true;
        }

        public RallyDecision RegisterBounce(Vector3 p)
        {
            if (LastHitter < 0) return RallyDecision.Of(RallyCall.None);
            int side = TennisDims.SideOf(p);
            if (IsServe) return JudgeServe(p, side);

            BouncesSinceHit++;
            if (BouncesSinceHit >= 2) return RallyDecision.PointTo(LastHitter, RallyHits == 1 ? PointReason.Ace : PointReason.Winner);
            int target = 1 - LastHitter;
            if (side != target) return RallyDecision.PointTo(target, PointReason.Net);
            if (!TennisDims.InSingles(p, target)) return RallyDecision.PointTo(target, PointReason.Out);
            return RallyDecision.Of(RallyCall.None);
        }

        RallyDecision JudgeServe(Vector3 p, int side)
        {
            int receiver = 1 - Server;
            if (side == receiver && TennisDims.InServiceBox(p, receiver, ServeBoxSign))
            {
                if (serveTouchedNet)
                {
                    ResetRally();
                    return RallyDecision.Of(RallyCall.Let);
                }
                IsServe = false;
                BouncesSinceHit = 1;
                return RallyDecision.Of(RallyCall.ServeGood);
            }
            Faults++;
            ResetRally();
            return Faults >= 2 ? RallyDecision.PointTo(receiver, PointReason.DoubleFault) : RallyDecision.Of(RallyCall.Fault);
        }

        /// <summary>ボールが止まった/場外へ消えたときの判定。</summary>
        public RallyDecision JudgeDeadBall(Vector3 ballPos)
        {
            if (LastHitter < 0) return RallyDecision.Of(RallyCall.None);
            bool onHitterSide = TennisDims.SideOf(ballPos) == LastHitter;
            return onHitterSide || BouncesSinceHit == 0
                ? RallyDecision.PointTo(1 - LastHitter, PointReason.Out)
                : RallyDecision.PointTo(LastHitter, PointReason.Winner);
        }
    }
}
