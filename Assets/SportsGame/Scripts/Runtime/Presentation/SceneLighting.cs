using UnityEngine;
using UnityEngine.Rendering;

namespace SportsGame
{
    /// <summary>メインライト(太陽・照明)と環境光の設定。</summary>
    public static class SceneLighting
    {
        public struct Preset
        {
            public Vector3 sunEuler;
            public Color sun;
            public float intensity;
            public Color sky, equator, ground;
        }

        public static Light Apply(Preset p)
        {
            Light light = null;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude))
            {
                if (l.type == LightType.Directional) { light = l; break; }
            }
            if (light == null)
            {
                light = new GameObject("Directional Light").AddComponent<Light>();
                light.type = LightType.Directional;
            }
            light.transform.rotation = Quaternion.Euler(p.sunEuler);
            light.color = p.sun;
            light.intensity = p.intensity;
            light.shadows = GameSettings.HighQuality ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = 0.75f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = p.sky;
            RenderSettings.ambientEquatorColor = p.equator;
            RenderSettings.ambientGroundColor = p.ground;
            return light;
        }
    }
}
