using UnityEngine;

namespace SportsGame
{
    /// <summary>効果音を実行時に合成する(外部アセット不要)。</summary>
    public static class SfxSynth
    {
        public static AudioClip Create(Sfx s)
        {
            float[] d;
            switch (s)
            {
                case Sfx.Dribble:
                    d = Dsp.Buffer(0.16f);
                    Dsp.Tone(d, 160f, 70f, 0.05f, 1f);
                    Dsp.NoiseBurst(d, 0.012f, 0.5f, 0.5f, 0f);
                    break;
                case Sfx.BallFloor:
                    d = Dsp.Buffer(0.2f);
                    Dsp.Tone(d, 140f, 60f, 0.07f, 1f);
                    Dsp.NoiseBurst(d, 0.015f, 0.4f, 0.4f, 0f);
                    break;
                case Sfx.TennisBounce:
                    d = Dsp.Buffer(0.1f);
                    Dsp.Tone(d, 420f, 250f, 0.02f, 0.7f);
                    Dsp.NoiseBurst(d, 0.008f, 0.8f, 0.6f, 0f);
                    break;
                case Sfx.Racket:
                    d = Dsp.Buffer(0.14f);
                    Dsp.Tone(d, 900f, 520f, 0.025f, 0.9f);
                    Dsp.Tone(d, 1700f, 1300f, 0.012f, 0.3f);
                    Dsp.NoiseBurst(d, 0.01f, 0.7f, 0.9f, 0.9f);
                    break;
                case Sfx.RacketStrong:
                    d = Dsp.Buffer(0.2f);
                    Dsp.Tone(d, 760f, 430f, 0.035f, 1f);
                    Dsp.Tone(d, 1500f, 1100f, 0.02f, 0.35f);
                    Dsp.NoiseBurst(d, 0.018f, 0.9f, 0.9f, 0.85f);
                    Dsp.Tone(d, 160f, 90f, 0.04f, 0.5f);
                    break;
                case Sfx.Bump:
                    d = Dsp.Buffer(0.16f);
                    Dsp.Tone(d, 260f, 150f, 0.04f, 1f);
                    Dsp.NoiseBurst(d, 0.02f, 0.6f, 0.5f, 0f);
                    break;
                case Sfx.Spike:
                    d = Dsp.Buffer(0.3f);
                    Dsp.NoiseBurst(d, 0.04f, 1f, 0.8f, 0.7f);
                    Dsp.Tone(d, 200f, 80f, 0.07f, 0.9f);
                    Dsp.NoiseBurst(d, 0.1f, 0.3f, 0.2f, 0f, 0.01f);
                    break;
                case Sfx.SandHit:
                    d = Dsp.Buffer(0.35f);
                    Dsp.NoiseBurst(d, 0.09f, 1f, 0.15f, 0f);
                    Dsp.Tone(d, 110f, 55f, 0.06f, 0.6f);
                    break;
                case Sfx.Swish:
                    d = Swish();
                    break;
                case Sfx.Rim:
                    d = Dsp.Buffer(0.6f);
                    Dsp.Tone(d, 540f, 520f, 0.18f, 0.6f);
                    Dsp.Tone(d, 1240f, 1230f, 0.12f, 0.45f);
                    Dsp.Tone(d, 2110f, 2100f, 0.07f, 0.3f);
                    Dsp.Tone(d, 3350f, 3340f, 0.04f, 0.2f);
                    Dsp.NoiseBurst(d, 0.006f, 0.6f, 0.9f, 0.8f);
                    break;
                case Sfx.Board:
                    d = Dsp.Buffer(0.22f);
                    Dsp.Tone(d, 210f, 120f, 0.05f, 1f);
                    Dsp.NoiseBurst(d, 0.03f, 0.5f, 0.3f, 0f);
                    Dsp.Tone(d, 900f, 800f, 0.02f, 0.2f);
                    break;
                case Sfx.NetHit:
                    d = Dsp.Buffer(0.3f);
                    Dsp.NoiseBurst(d, 0.08f, 1f, 0.25f, 0f);
                    break;
                case Sfx.Whistle:
                    d = Whistle(0.38f);
                    break;
                case Sfx.Buzzer:
                    d = Buzzer();
                    break;
                case Sfx.Cheer:
                case Sfx.Applause:
                case Sfx.Groan:
                    d = Crowd(s);
                    break;
                case Sfx.Squeak:
                    d = Squeak();
                    break;
                case Sfx.Click:
                    d = Dsp.Buffer(0.05f);
                    Dsp.Tone(d, 1500f, 1200f, 0.012f, 1f);
                    break;
                case Sfx.Confirm:
                    d = Dsp.Buffer(0.3f);
                    Dsp.Tone(d, 880f, 880f, 0.08f, 0.8f);
                    Dsp.Tone(d, 1320f, 1320f, 0.12f, 0.8f, 0.08f);
                    break;
                case Sfx.Back:
                    d = Dsp.Buffer(0.22f);
                    Dsp.Tone(d, 990f, 990f, 0.06f, 0.8f);
                    Dsp.Tone(d, 660f, 660f, 0.08f, 0.8f, 0.07f);
                    break;
                case Sfx.Point:
                    d = Dsp.Buffer(0.5f);
                    Dsp.Tone(d, 784f, 784f, 0.1f, 0.7f);
                    Dsp.Tone(d, 1046f, 1046f, 0.12f, 0.7f, 0.09f);
                    Dsp.Tone(d, 1568f, 1568f, 0.2f, 0.6f, 0.18f);
                    break;
                case Sfx.WinJingle:
                    d = Jingle(new[] { 523.25f, 659.25f, 783.99f, 1046.5f, 783.99f, 1046.5f }, new[] { 0.14f, 0.14f, 0.14f, 0.3f, 0.14f, 0.7f });
                    break;
                case Sfx.LoseJingle:
                    d = Jingle(new[] { 392f, 369.99f, 349.23f, 293.66f }, new[] { 0.3f, 0.3f, 0.3f, 0.9f });
                    break;
                default:
                    d = Dsp.Buffer(0.05f);
                    break;
            }
            return Dsp.ToClip(s.ToString(), d);
        }

        static float[] Swish()
        {
            var d = Dsp.Buffer(0.45f);
            float low = 0f, prev = 0f, high = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Dsp.Rate;
                low += (Dsp.Noise() - low) * 0.35f;
                high = 0.9f * (high + low - prev);
                prev = low;
                float env = Mathf.Sin(Mathf.Clamp01(t / 0.42f) * Mathf.PI);
                d[i] = high * env * env;
            }
            return d;
        }

        static float[] Whistle(float len)
        {
            var d = Dsp.Buffer(len + 0.05f);
            float ph = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Dsp.Rate;
                float trill = 0.6f + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 32f);
                ph += 2f * Mathf.PI * (2950f + 90f * Mathf.Sin(t * 2f * Mathf.PI * 32f)) / Dsp.Rate;
                float env = Mathf.Clamp01(t / 0.015f) * Mathf.Clamp01((len - t) / 0.04f);
                d[i] = ((Mathf.Sin(ph) + 0.15f * Mathf.Sin(ph * 2f)) * trill + Dsp.Noise() * 0.05f) * env;
            }
            return d;
        }

        static float[] Buzzer()
        {
            var d = Dsp.Buffer(0.9f);
            float ph = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Dsp.Rate;
                ph += 2f * Mathf.PI * 233f / Dsp.Rate;
                float sq = Mathf.Sign(Mathf.Sin(ph)) * 0.5f + Mathf.Sin(ph * 2.01f) * 0.3f + Mathf.Sin(ph * 3.02f) * 0.2f;
                d[i] = sq * Mathf.Clamp01(t / 0.01f) * Mathf.Clamp01((0.88f - t) / 0.05f);
            }
            return d;
        }

        static float[] Squeak()
        {
            var d = Dsp.Buffer(0.12f);
            float ph = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Dsp.Rate;
                ph += 2f * Mathf.PI * (2100f + 900f * t / 0.12f) / Dsp.Rate;
                d[i] = (Mathf.Sin(ph) + 0.4f * Mathf.Sin(ph * 2f)) * Mathf.Sin(Mathf.Clamp01(t / 0.12f) * Mathf.PI);
            }
            return d;
        }

        static float[] Crowd(Sfx kind)
        {
            float len = kind == Sfx.Groan ? 1.4f : 2.4f;
            var d = Dsp.Buffer(len);
            var low = new float[6];
            var prev = new float[6];
            var band = new float[6];
            float[] formants = { 0.08f, 0.12f, 0.18f, 0.25f, 0.33f, 0.42f };
            for (int i = 0; i < d.Length; i++)
            {
                float t = i / (float)Dsp.Rate;
                float env = kind == Sfx.Groan
                    ? Mathf.Clamp01(t / 0.1f) * Mathf.Clamp01((len - t) / 0.8f)
                    : Mathf.Clamp01(t / 0.25f) * Mathf.Clamp01((len - t) / 1.2f);
                float s = 0f;
                for (int k = 0; k < formants.Length; k++)
                {
                    low[k] += (Dsp.Noise() - low[k]) * formants[k];
                    band[k] = 0.97f * (band[k] + low[k] - prev[k]);
                    prev[k] = low[k];
                    s += band[k] * (0.6f + 0.4f * Mathf.Sin(t * (3f + k * 1.7f) + k));
                }
                if (kind == Sfx.Applause && Dsp.Chance(0.02)) s += Dsp.Noise() * 4f;
                if (kind == Sfx.Groan)
                    s = s * 0.5f + Mathf.Sin(t * 2f * Mathf.PI * (180f - 60f * t)) * 0.3f * (0.5f + 0.5f * Mathf.Sin(t * 20f));
                d[i] = s * env;
            }
            return d;
        }

        static float[] Jingle(float[] notes, float[] lens)
        {
            float total = 0f;
            foreach (var l in lens) total += l;
            var d = Dsp.Buffer(total + 0.6f);
            float start = 0f;
            for (int n = 0; n < notes.Length; n++)
            {
                float f = notes[n];
                int s0 = (int)(start * Dsp.Rate);
                int count = (int)((lens[n] + 0.4f) * Dsp.Rate);
                for (int i = s0; i < d.Length && i < s0 + count; i++)
                {
                    float t = (i - s0) / (float)Dsp.Rate;
                    float env = Mathf.Clamp01(t / 0.01f) * Mathf.Exp(-t / (lens[n] * 0.9f + 0.05f));
                    float w = Mathf.Sin(2f * Mathf.PI * f * t) * 0.6f + Mathf.Sin(4f * Mathf.PI * f * t) * 0.2f + Mathf.Sin(6f * Mathf.PI * f * t) * 0.1f;
                    d[i] += w * env;
                }
                start += lens[n];
            }
            return d;
        }
    }
}
