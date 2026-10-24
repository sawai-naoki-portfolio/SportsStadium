using UnityEngine;

namespace SportsGame
{
    public static class RandomUtil
    {
        /// <summary>標準正規分布の乱数(Box-Muller)。</summary>
        public static float Gauss()
        {
            float u1 = Mathf.Max(1e-6f, Random.value), u2 = Random.value;
            return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
        }
    }
}
