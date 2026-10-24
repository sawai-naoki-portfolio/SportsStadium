using UnityEngine;

namespace SportsGame
{
    /// <summary>操作側(人/CPU)が参照できる試合の状態。</summary>
    public interface ITennisView
    {
        BallSim Ball { get; }
        TennisReferee Referee { get; }
        Transform CameraTransform { get; }
        float TossApex { get; }
        TennisPlayer Opponent(TennisPlayer self);
    }

    /// <summary>テニス選手の操作。人の入力と CPU の判断を同じ形で扱う。</summary>
    public interface ITennisController
    {
        /// <summary>人が操作しているか(ヒント・ボタン表示の切り替えに使う)。</summary>
        bool IsHuman { get; }

        /// <summary>サーブの狙いのブレの基準値(m)。</summary>
        float ServeError { get; }

        /// <summary>ラリー中の移動とスイング予約。</summary>
        void TickRally(TennisPlayer self, float dt);

        /// <summary>打つ瞬間に打球の内容を決める。</summary>
        ShotRequest DecideShot(TennisPlayer self, HitGeometry geometry);

        /// <summary>相手が打った直後に呼ばれる(反応時間・予測のリセット)。</summary>
        void OnOpponentHit(TennisPlayer self);

        /// <summary>サーブ前に動きたい方向(試合側が位置を制限する)。</summary>
        Vector3 PreServeMove(TennisPlayer self);

        bool WantsToss(TennisPlayer self, float dt);

        /// <summary>トスしたボールを打つか。quality は打点の良さ、aimX はコース(-1〜1)。</summary>
        bool WantsServeHit(TennisPlayer self, out float quality, out float aimX);
    }
}
