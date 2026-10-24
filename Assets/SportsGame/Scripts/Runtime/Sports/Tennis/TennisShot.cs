using UnityEngine;

namespace SportsGame
{
    public enum TShot { Topspin, Slice, Lob, Smash, Volley }

    /// <summary>打つ瞬間のボールと選手の位置関係(選手の向きを基準にしたローカル座標)。</summary>
    public struct HitGeometry
    {
        /// <summary>+ がフォアハンド側。</summary>
        public float lateral;
        /// <summary>+ が体の前(ネット側)。</summary>
        public float forward;
        public float height;

        /// <summary>理想の打点(体の斜め前・腰の高さ)に近いほど 1。</summary>
        public float Quality => 1f - Mathf.Clamp01(Mathf.Abs(forward - 0.35f) / 1.1f) * 0.5f
                                   - Mathf.Clamp01(Mathf.Abs(Mathf.Abs(lateral) - 0.85f) / 0.9f) * 0.5f;
    }

    /// <summary>操作側(人/CPU)が決める打球の内容。</summary>
    public struct ShotRequest
    {
        public TShot shot;
        public float power;
        /// <summary>x: -1(左)〜1(右)のワールド X 方向, y: -1(短く)〜1(深く)。</summary>
        public Vector2 aim;
        /// <summary>狙いのブレの基準値(m)。</summary>
        public float error;
        public float netChance;
    }

    /// <summary>スイング予約(ボールが打点に来たら自動で打つ)。</summary>
    public struct SwingIntent
    {
        public bool active;
        public TShot shot;
        public float since;
    }
}
