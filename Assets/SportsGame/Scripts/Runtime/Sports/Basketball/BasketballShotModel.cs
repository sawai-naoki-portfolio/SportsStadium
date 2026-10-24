using UnityEngine;

namespace SportsGame
{
    public enum ShotKind { Jumper, Layup, Dunk }

    /// <summary>
    /// シュートの成功率と狙いの計算。成功と判定したらリング中心へ、失敗ならリングに当たる位置へ投げ、結果は物理に任せる。
    /// </summary>
    public static class BasketballShotModel
    {
        public const float LayupRange = 1.9f;
        public const float DunkRange = 1.7f;

        /// <summary>距離ごとの基本成功率。</summary>
        public static float BaseChance(ShotKind kind, float distance)
        {
            switch (kind)
            {
                case ShotKind.Dunk: return 0.94f;
                case ShotKind.Layup: return 0.8f;
            }
            if (distance < 3f) return 0.66f;
            if (distance < BasketballDims.ThreeRadius) return Mathf.Lerp(0.6f, 0.46f, (distance - 3f) / (BasketballDims.ThreeRadius - 3f));
            return Mathf.Max(0.05f, Mathf.Lerp(0.42f, 0.18f, (distance - BasketballDims.ThreeRadius) / 4f));
        }

        /// <summary>ディフェンスのチェックの強さ(0〜1)。近く・前方・ジャンプ中ほど強い。</summary>
        public static float Contest(Vector3 shooter, Vector3 defender, bool defenderAirborne)
        {
            Vector3 toDef = defender - shooter;
            toDef.y = 0f;
            float d = toDef.magnitude;
            bool front = d > 1e-3f && Vector3.Dot(toDef / d, BasketballDims.ToHoop(shooter)) > 0.3f;
            float c = Mathf.Clamp01(1f - (d - 0.7f) / 1.5f) * (front ? 1f : 0.45f) * (defenderAirborne ? 1.25f : 0.8f);
            return Mathf.Clamp01(c);
        }

        /// <summary>リリースのタイミング品質(ゲージ値 1.0 が理想)。window 内なら PERFECT。</summary>
        public static float TimingQuality(float meter, float window, out bool perfect)
        {
            float miss = Mathf.Abs(meter - 1f);
            perfect = miss <= window;
            return perfect ? 1f : Mathf.Clamp01(1f - (miss - window) / 0.45f);
        }

        public static float MakeChance(ShotKind kind, float distance, float timing, bool perfect, float contest, float skill)
        {
            float timingMult = kind == ShotKind.Jumper ? (perfect ? 1.45f : Mathf.Lerp(0.35f, 1.1f, timing)) : 1f;
            float contestMult = 1f - (kind == ShotKind.Dunk ? 0.5f : 0.45f) * contest;
            float p = BaseChance(kind, distance) * timingMult * contestMult * skill;
            if (kind == ShotKind.Jumper && perfect && contest < 0.25f) p = Mathf.Max(p, 0.92f);
            return Mathf.Clamp(p, 0.03f, 0.97f);
        }

        /// <summary>リング面上の狙い。成功ならほぼ中心、失敗ならリングの手前・奥・左右に当たる位置。</summary>
        public static Vector3 AimPoint(bool made, Vector3 shooter)
        {
            Vector3 dir = BasketballDims.ToHoop(shooter);
            Vector3 side = Vector3.Cross(Vector3.up, dir);
            Vector3 hoop = BasketballDims.Hoop;
            if (made) return hoop + dir * Random.Range(-0.03f, 0.05f) + side * Random.Range(-0.04f, 0.04f);
            switch (Random.Range(0, 4))
            {
                case 0: return hoop - dir * Random.Range(0.2f, 0.3f);
                case 1: return hoop + dir * Random.Range(0.22f, 0.32f);
                default: return hoop + side * (Random.value < 0.5f ? -1f : 1f) * Random.Range(0.21f, 0.3f) + dir * Random.Range(-0.05f, 0.1f);
            }
        }

        /// <summary>ブロックの手がボールにどれだけ近いか(0〜radius)から、ブロック成功率を求める。</summary>
        public static float BlockChance(float handDistance, float radius, ShotKind kind)
        {
            float k = Mathf.Lerp(0.6f, 0.2f, Mathf.Clamp01(handDistance / radius));
            return kind == ShotKind.Dunk ? k * 0.6f : k;
        }

        public static float LaunchAngle(ShotKind kind, float distance)
            => kind == ShotKind.Jumper ? Mathf.Lerp(50f, 56f, Mathf.Clamp01(distance / 8f)) : 62f;

        public static int PointsFor(ShotKind kind, Vector3 feet) => kind == ShotKind.Jumper && BasketballDims.IsThree(feet) ? 3 : 2;
    }
}
