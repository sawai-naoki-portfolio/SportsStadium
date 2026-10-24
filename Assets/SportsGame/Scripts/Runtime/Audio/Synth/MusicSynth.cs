using UnityEngine;

namespace SportsGame
{
    /// <summary>BGM と環境音(ループ素材)の合成。</summary>
    public static class MusicSynth
    {
        /// <summary>観客のざわめき(継ぎ目をクロスフェードしたループ)。</summary>
        public static AudioClip CrowdLoop()
        {
            const float len = 6f;
            int n = (int)(len * Dsp.Rate);
            int fade = Dsp.Rate / 2;
            var d = new float[n];
            float l1 = 0f, l2 = 0f, prev = 0f, band = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)Dsp.Rate;
                l1 += (Dsp.Noise() - l1) * 0.12f;
                l2 += (l1 - l2) * 0.12f;
                band = 0.98f * (band + l2 - prev);
                prev = l2;
                float mod = 0.7f + 0.15f * Mathf.Sin(t * 2f * Mathf.PI / len * 3f) + 0.15f * Mathf.Sin(t * 2f * Mathf.PI / len * 7f + 1f);
                d[i] = band * mod;
            }
            var loop = new float[n - fade];
            for (int i = 0; i < loop.Length; i++)
            {
                float k = i < fade ? i / (float)fade : 1f;
                loop[i] = i < fade ? d[i] * k + d[n - fade + i] * (1f - k) : d[i];
            }
            return Dsp.ToClip("CrowdLoop", loop, 0.5f);
        }

        /// <summary>タイトル用の明るいBGMループ(C-G-Am-F)。</summary>
        public static AudioClip TitleMusic()
        {
            const float bpm = 124f;
            const int bars = 8;
            float beat = 60f / bpm;
            var d = new float[Mathf.CeilToInt(bars * 4 * beat * Dsp.Rate)];
            int[][] chords = { new[] { 60, 64, 67 }, new[] { 55, 59, 62 }, new[] { 57, 60, 64 }, new[] { 53, 57, 60 } };
            int[] bass = { 36, 43, 45, 41 };
            int[] melody = { 72, 74, 76, 79, 76, 74, 72, 74, 71, 72, 74, 76, 74, 72, 69, 71,
                             72, 76, 79, 81, 79, 76, 74, 72, 72, 74, 72, 69, 67, 69, 72, -1 };
            for (int bar = 0; bar < bars; bar++)
            {
                int ci = bar % chords.Length;
                float barStart = bar * 4 * beat;
                for (int e = 0; e < 8; e++)
                    Dsp.Note(d, Dsp.Midi(bass[ci] + (e % 2 == 1 ? 12 : 0)), barStart + e * beat * 0.5f, beat * 0.45f, 0.28f, Dsp.Wave.Bass);
                for (int s = 0; s < 16; s++)
                    Dsp.Note(d, Dsp.Midi(chords[ci][s % 3] + (s % 6 >= 3 ? 12 : 0)), barStart + s * beat * 0.25f, beat * 0.22f, 0.1f, Dsp.Wave.Pluck);
                for (int q = 0; q < 4; q++)
                {
                    int m = melody[(bar * 4 + q) % melody.Length];
                    if (m > 0) Dsp.Note(d, Dsp.Midi(m), barStart + q * beat, beat * 0.9f, 0.16f, Dsp.Wave.Triangle);
                    float t0 = barStart + q * beat;
                    Dsp.Kick(d, t0, 0.5f);
                    if (q % 2 == 1) Dsp.Snare(d, t0, 0.25f);
                    Dsp.Hat(d, t0 + beat * 0.5f, 0.07f);
                }
            }
            return Dsp.ToClip("TitleMusic", d, 0.6f);
        }
    }
}
