using System;
using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// ビーチバレーの調整値。Vector2 は (かんたん, むずかしい) で、難易度に応じて補間する。
    /// VolleyballGame の Inspector から変更できる。
    /// </summary>
    [Serializable]
    public class VolleyballConfig
    {
        [Header("CPU (x=かんたん, y=むずかしい)")]
        public Vector2 aiSpeed = new Vector2(4.2f, 5.3f);
        public Vector2 aiReaction = new Vector2(0.35f, 0.12f);
        [Tooltip("レシーブ・トスの狙いのブレ(m)")] public Vector2 aiPassError = new Vector2(1.1f, 0.35f);
        public Vector2 aiSpikePower = new Vector2(0.35f, 0.95f);
        [Tooltip("スパイクの狙いのブレ(m)")] public Vector2 aiSpikeError = new Vector2(1.0f, 0.45f);
        public Vector2 aiServePower = new Vector2(0.3f, 0.9f);
        public Vector2 aiServeError = new Vector2(1.0f, 0.5f);
        [Tooltip("相手スパイクに合わせて正しく跳べる確率")] public Vector2 aiBlockSkill = new Vector2(0.3f, 0.75f);
        [Tooltip("アウトになるボールを見送る確率")] public Vector2 aiJudge = new Vector2(0.3f, 0.85f);
        [Tooltip("味方 CPU の最低スキル(味方は頼れるように)")] public float partnerMinSkill = 0.6f;

        [Header("プレイヤー")]
        public float playerSpeed = 5.0f;
        [Tooltip("ボタンを先押ししておける秒数")] public float intentWindow = 0.8f;
        public float playerPassError = 0.55f;
        public float playerSpikeError = 0.5f;
        public float playerServeError = 0.5f;
        [Tooltip("移動アシスト時の速度倍率")] public float assistSpeed = 0.85f;

        [Header("届く範囲(m)")]
        public float passReach = 1.05f;
        public float assistReachBonus = 0.25f;
        public float spikeReach = 0.85f;

        [Header("ボール")]
        public Vector2 receiveApex = new Vector2(4.0f, 5.0f);
        public float setApex = 4.6f;
        [Tooltip("スパイクを打つ高さ")] public float hitHeight = 2.95f;
        public Vector2 spikeSpeed = new Vector2(12f, 20f);
        public Vector2 serveSpeed = new Vector2(11f, 17f);
        public float freeBallApex = 4.3f;
        public float jumpSpeed = 3.7f;

        public static float ForSkill(Vector2 range, float skill) => Mathf.Lerp(range.x, range.y, skill);
    }
}
