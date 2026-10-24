using System;
using UnityEngine;

namespace SportsGame
{
    /// <summary>打球の物理特性。</summary>
    [Serializable]
    public struct ShotProfile
    {
        [Tooltip("見かけの重力倍率(トップスピン > 1、スライス < 1)")] public float gravityScale;
        public float speedMin, speedMax;
        [Tooltip("バウンド時の水平速度の保持率倍率")] public float bounceXZ;
        [Tooltip("バウンド時の跳ね上がり倍率")] public float bounceY;
        [Tooltip("ネット上端からの余裕(m)")] public float netMargin;

        public ShotProfile(float gravityScale, float speedMin, float speedMax, float bounceXZ, float bounceY, float netMargin)
        {
            this.gravityScale = gravityScale;
            this.speedMin = speedMin;
            this.speedMax = speedMax;
            this.bounceXZ = bounceXZ;
            this.bounceY = bounceY;
            this.netMargin = netMargin;
        }

        public float Speed(float power) => Mathf.Lerp(speedMin, speedMax, Mathf.Clamp01(power));
    }

    /// <summary>
    /// テニスの調整値。Vector2 は (かんたん, むずかしい) で、難易度に応じて補間する。
    /// TennisGame の Inspector から変更できる。
    /// </summary>
    [Serializable]
    public class TennisConfig
    {
        [Header("CPU (x=かんたん, y=むずかしい)")]
        public Vector2 aiSpeed = new Vector2(5.0f, 6.7f);
        public Vector2 aiReaction = new Vector2(0.32f, 0.1f);
        [Tooltip("狙いのブレ(m)")] public Vector2 aiError = new Vector2(1.1f, 0.5f);
        public Vector2 aiNetChance = new Vector2(0.07f, 0.025f);
        [Tooltip("1打ごとのミスショット(大きく狙いが外れる)の確率")] public Vector2 aiMissChance = new Vector2(0.22f, 0.06f);
        public Vector2 aiPowerMin = new Vector2(0.15f, 0.6f);
        public Vector2 aiPowerMax = new Vector2(0.5f, 1f);
        [Tooltip("アウトになるボールを見送る確率")] public Vector2 aiJudge = new Vector2(0.35f, 0.9f);
        [Tooltip("相手のいない側を狙う確率")] public Vector2 aiAimAway = new Vector2(0.45f, 0.8f);

        [Header("プレイヤー")]
        public float playerSpeed = 6.3f;
        [Tooltip("狙いのブレ(m)")] public float playerError = 0.42f;
        [Tooltip("サーブの狙いのブレ(m)")] public float playerServeError = 0.34f;
        [Tooltip("移動アシスト時の速度倍率")] public float assistSpeed = 0.8f;
        [Tooltip("ボタンを早押しした時間に応じてパワーが最大になるまでの秒数")] public float chargeTime = 0.8f;

        [Header("打球")]
        public ShotProfile topspin = new ShotProfile(1.45f, 16f, 24f, 1.12f, 1f, 0.4f);
        public ShotProfile slice = new ShotProfile(0.82f, 13.5f, 17.5f, 0.82f, 0.7f, 0.25f);
        public ShotProfile smash = new ShotProfile(1f, 25f, 31f, 1f, 1f, 0.1f);
        public ShotProfile volley = new ShotProfile(1.1f, 13f, 18f, 1f, 1f, 0.15f);
        public ShotProfile firstServe = new ShotProfile(1.15f, 21f, 30.5f, 1f, 0.9f, 0.06f);
        public ShotProfile secondServe = new ShotProfile(1.55f, 16f, 20f, 1.1f, 1.1f, 0.3f);
        public Vector2 lobApex = new Vector2(5.5f, 7.5f);

        public static float ForSkill(Vector2 range, float skill) => Mathf.Lerp(range.x, range.y, skill);

        public ShotProfile Profile(TShot shot)
        {
            switch (shot)
            {
                case TShot.Slice: return slice;
                case TShot.Smash: return smash;
                case TShot.Volley: return volley;
                default: return topspin;
            }
        }
    }
}
