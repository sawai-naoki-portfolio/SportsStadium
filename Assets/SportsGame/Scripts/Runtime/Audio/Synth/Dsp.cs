using UnityEngine;

namespace SportsGame
{
    /// <summary>効果音・BGM 合成のための基本的な波形処理。</summary>
    public static class Dsp
    {
        public const int Rate = 22050;

        static readonly System.Random rng = new System.Random(1234);

        public static float Noise() => (float)rng.NextDouble() * 2f - 1f;
        public static bool Chance(double p) => rng.NextDouble() < p;

        public static float[] Buffer(float seconds) => new float[Mathf.CeilToInt(seconds * Rate)];

        public static float Midi(int note) => 440f * Mathf.Pow(2f, (note - 69) / 12f);

        public static AudioClip ToClip(string name, float[] data, float peak = 0.9f)
        {
            Normalize(data, peak);
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static void Normalize(float[] d, float peak)
        {
            float m = 0f;
            for (int i = 0; i < d.Length; i++) m = Mathf.Max(m, Mathf.Abs(d[i]));
            if (m < 1e-5f) return;
            float k = peak / m;
            for (int i = 0; i < d.Length; i++) d[i] *= k;
        }

        /// <summary>周波数が f0→f1 へ指数的に変化する減衰サイン波を加算。</summary>
        public static void Tone(float[] d, float f0, float f1, float decay, float amp, float start = 0f, float attack = 0.002f)
        {
            int s0 = (int)(start * Rate);
            float phase = 0f;
            for (int i = s0; i < d.Length; i++)
            {
                float t = (i - s0) / (float)Rate;
                float f = f1 + (f0 - f1) * Mathf.Exp(-t * 18f);
                phase += 2f * Mathf.PI * f / Rate;
                float env = Mathf.Exp(-t / decay) * Mathf.Clamp01(t / attack);
                d[i] += Mathf.Sin(phase) * env * amp;
            }
        }

        /// <summary>フィルタ済みノイズを加算。lowpass/highpass は 0〜1 の係数(highpass=0 で無効)。</summary>
        public static void NoiseBurst(float[] d, float decay, float amp, float lowpass, float highpass, float start = 0f, float attack = 0.001f)
        {
            int s0 = (int)(start * Rate);
            float low = 0f, prev = 0f, high = 0f;
            for (int i = s0; i < d.Length; i++)
            {
                float t = (i - s0) / (float)Rate;
                low += (Noise() - low) * lowpass;
                high = highpass * (high + low - prev);
                prev = low;
                float env = Mathf.Exp(-t / decay) * Mathf.Clamp01(t / attack);
                d[i] += (highpass > 0f ? high : low) * env * amp;
            }
        }

        public enum Wave { Triangle, Bass, Pluck }

        public static void Note(float[] d, float f, float start, float dur, float amp, Wave wave)
        {
            int s0 = (int)(start * Rate);
            int len = (int)((dur + 0.08f) * Rate);
            for (int i = 0; i < len && s0 + i < d.Length; i++)
            {
                float t = i / (float)Rate;
                float env = Mathf.Clamp01(t / 0.008f) * Mathf.Clamp01((dur + 0.08f - t) / 0.08f) * Mathf.Exp(-t * (wave == Wave.Pluck ? 6f : 2.5f));
                float ph = 2f * Mathf.PI * f * t;
                float w;
                switch (wave)
                {
                    case Wave.Bass: w = Mathf.Sin(ph) + 0.3f * Mathf.Sin(ph * 2f); break;
                    case Wave.Pluck: w = Mathf.Sin(ph) * 0.7f + Mathf.Sin(ph * 3f) * 0.2f; break;
                    default: w = 2f * Mathf.Abs(2f * ((f * t) % 1f) - 1f) - 1f; break;
                }
                d[s0 + i] += w * env * amp;
            }
        }

        public static void Kick(float[] d, float start, float amp)
        {
            int s0 = (int)(start * Rate);
            float ph = 0f;
            for (int i = 0; i < Rate / 6 && s0 + i < d.Length; i++)
            {
                float t = i / (float)Rate;
                ph += 2f * Mathf.PI * (50f + 110f * Mathf.Exp(-t * 35f)) / Rate;
                d[s0 + i] += Mathf.Sin(ph) * Mathf.Exp(-t * 18f) * amp;
            }
        }

        public static void Snare(float[] d, float start, float amp)
        {
            int s0 = (int)(start * Rate);
            for (int i = 0; i < Rate / 6 && s0 + i < d.Length; i++)
            {
                float t = i / (float)Rate;
                d[s0 + i] += (Noise() * 0.8f + Mathf.Sin(2f * Mathf.PI * 190f * t) * 0.4f) * Mathf.Exp(-t * 22f) * amp;
            }
        }

        public static void Hat(float[] d, float start, float amp)
        {
            int s0 = (int)(start * Rate);
            float prev = 0f;
            for (int i = 0; i < Rate / 25 && s0 + i < d.Length; i++)
            {
                float t = i / (float)Rate;
                float n = Noise();
                d[s0 + i] += (n - prev) * Mathf.Exp(-t * 80f) * amp;
                prev = n;
            }
        }
    }
}
