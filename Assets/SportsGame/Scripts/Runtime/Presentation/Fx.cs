using UnityEngine;
using UnityEngine.Rendering;

namespace SportsGame
{
    /// <summary>パーティクル演出(打球の火花・紙吹雪)。シーンごとに1つの ParticleSystem を使い回す。</summary>
    public static class Fx
    {
        static ParticleSystem ps;

        static readonly Color[] ConfettiColors =
        {
            new Color(1f, 0.3f, 0.3f), new Color(1f, 0.85f, 0.2f), new Color(0.3f, 0.8f, 1f), new Color(0.4f, 1f, 0.5f), new Color(1f, 0.5f, 1f)
        };

        static ParticleSystem Get()
        {
            if (ps != null) return ps;
            var go = new GameObject("[FX]");
            ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = 1.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 800;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.material = Mats.Particle;
            r.shadowCastingMode = ShadowCastingMode.Off;
            ps.Play();
            return ps;
        }

        public static void Burst(Vector3 pos, Color color, int count, float speed = 4f, float size = 0.1f)
        {
            if (!GameSettings.HighQuality) count = Mathf.Max(1, count / 2);
            var p = Get();
            var ep = new ParticleSystem.EmitParams { position = pos, startColor = color };
            for (int i = 0; i < count; i++)
            {
                ep.velocity = Random.onUnitSphere * speed * Random.Range(0.4f, 1f) + Vector3.up * speed * 0.4f;
                ep.startSize = size * Random.Range(0.6f, 1.4f);
                ep.startLifetime = Random.Range(0.35f, 0.9f);
                p.Emit(ep, 1);
            }
        }

        public static void Confetti(Vector3 center, float spread = 6f)
        {
            var p = Get();
            int n = GameSettings.HighQuality ? 160 : 70;
            for (int i = 0; i < n; i++)
            {
                p.Emit(new ParticleSystem.EmitParams
                {
                    position = center + new Vector3(Random.Range(-spread, spread), Random.Range(3f, 6f), Random.Range(-spread * 0.5f, spread * 0.5f)),
                    velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 3f), Random.Range(-1f, 1f)),
                    startColor = ConfettiColors[i % ConfettiColors.Length],
                    startSize = Random.Range(0.08f, 0.16f),
                    startLifetime = Random.Range(2f, 3.5f),
                }, 1);
            }
        }
    }
}
