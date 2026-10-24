using System;
using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// バスケットボールの調整値。Vector2 は (かんたん, むずかしい) で、難易度に応じて補間する。
    /// BasketballGame の Inspector から変更できる。
    /// </summary>
    [Serializable]
    public class BasketballConfig
    {
        [Header("ルール")]
        public float shotClock = 12f;
        [Tooltip("チェックボール(攻撃開始前)の待ち時間")] public float checkTime = 1.1f;

        [Header("移動")]
        public float runSpeed = 5.1f;
        public float dribbleSpeed = 4.8f;
        public float sprintMultiplier = 1.28f;
        public float staminaDrain = 0.35f;
        public float staminaRegen = 0.2f;
        public float jumpSpeed = 3.6f;
        public float shotJumpSpeed = 3.0f;
        public float dunkJumpSpeed = 4.4f;
        public float crossoverSpeed = 7.5f;

        [Header("シュート")]
        [Tooltip("ジャンプの頂点(理想のリリース)までの秒数")] public float shotPeakTime = 0.43f;
        [Tooltip("PERFECT 判定の幅(ゲージ比)")] public Vector2 perfectWindow = new Vector2(0.18f, 0.08f);
        [Tooltip("プレイヤーのシュート成功率倍率")] public Vector2 playerShotBonus = new Vector2(1.12f, 0.95f);

        [Header("プレイヤーの守備")]
        public float playerStealChance = 0.35f;

        [Header("CPU (x=かんたん, y=むずかしい)")]
        public Vector2 aiSpeedScale = new Vector2(0.86f, 1.02f);
        public Vector2 aiReaction = new Vector2(0.35f, 0.12f);
        [Tooltip("リリースタイミングのブレ")] public Vector2 aiTimingSigma = new Vector2(0.2f, 0.07f);
        public Vector2 aiShotSkill = new Vector2(0.85f, 1.1f);
        [Tooltip("1秒あたりのスティール試行率")] public Vector2 aiStealRate = new Vector2(0.12f, 0.45f);
        public Vector2 aiStealChance = new Vector2(0.15f, 0.32f);
        [Tooltip("シュートに合わせてブロックに跳ぶ確率")] public Vector2 aiBlockChance = new Vector2(0.35f, 0.7f);
        public Vector2 aiCrossChance = new Vector2(0.25f, 0.45f);
        [Tooltip("プレイヤーのクロスオーバーに引っかかる確率")] public Vector2 aiAnkleBreak = new Vector2(0.5f, 0.22f);

        public static float ForSkill(Vector2 range, float skill) => Mathf.Lerp(range.x, range.y, skill);
    }
}
