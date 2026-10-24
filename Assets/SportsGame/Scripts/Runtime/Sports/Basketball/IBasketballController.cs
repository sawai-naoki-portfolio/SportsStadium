using UnityEngine;

namespace SportsGame
{
    /// <summary>1フレーム分の操作。move はワールド XZ 方向(大きさ 0〜1)。</summary>
    public struct BBCommand
    {
        public Vector3 move;
        public bool sprint;
        /// <summary>A ボタン: ボール保持時はシュート、それ以外はジャンプ(ブロック・リバウンド)。</summary>
        public bool actionDown, actionHeld, actionUp;
        /// <summary>B ボタン: ボール保持時はクロスオーバー、それ以外はスティール。</summary>
        public bool skillDown;
    }

    /// <summary>操作側(人/CPU)が参照できる試合の状態。</summary>
    public interface IBasketballView
    {
        BallSim Ball { get; }
        BasketballPlayer Holder { get; }
        bool NeedsClear { get; }
        float ShotClock { get; }
        /// <summary>こぼれ球を拾いに行く目標地点。</summary>
        Vector3 LooseBallTarget { get; }
        Transform CameraTransform { get; }
        BasketballPlayer Opponent(BasketballPlayer self);
    }

    public interface IBasketballController
    {
        bool IsHuman { get; }
        BBCommand Decide(BasketballPlayer self, float dt);
    }
}
