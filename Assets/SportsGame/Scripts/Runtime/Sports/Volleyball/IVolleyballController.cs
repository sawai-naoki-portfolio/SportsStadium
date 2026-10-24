using UnityEngine;

namespace SportsGame
{
    /// <summary>操作側(人/CPU)が参照できる試合の状態。</summary>
    public interface IVolleyballView
    {
        BallSim Ball { get; }
        VolleyballReferee Referee { get; }
        Transform CameraTransform { get; }
        float TossApex { get; }
        /// <summary>そのチームが次にボールを触る計画(相手の番なら valid=false)。</summary>
        VolleyballPlan PlanFor(int team);
        VolleyballPlayer Partner(VolleyballPlayer self);
        bool IsHumanControlled(VolleyballPlayer player);
    }

    /// <summary>ビーチバレー選手の操作。人の入力と CPU の判断を同じ形で扱う。</summary>
    public interface IVolleyballController
    {
        bool IsHuman { get; }
        float PassError { get; }
        float SpikeError { get; }
        float ServeError { get; }

        /// <summary>ラリー中の移動・先押し・ジャンプ。</summary>
        void TickRally(VolleyballPlayer self, float dt);

        /// <summary>アタックの狙い(-1〜1)。feint はフェイント(軟打)にするか。</summary>
        Vector2 AttackAim(VolleyballPlayer self, out bool feint);

        float SpikePower(float quality);

        void OnBallCrossed(VolleyballPlayer self);

        Vector3 PreServeMove(VolleyballPlayer self);
        bool WantsToss(VolleyballPlayer self, float dt);
        bool WantsServeHit(VolleyballPlayer self, out float quality, out Vector2 aim);
    }
}
