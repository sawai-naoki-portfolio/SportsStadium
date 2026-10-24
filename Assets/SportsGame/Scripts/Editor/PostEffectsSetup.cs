using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SportsGame.EditorTools
{
    /// <summary>ポストエフェクトのプロファイル(Resources/SportsGame/PostFX.asset)を作る。</summary>
    public static class PostEffectsSetup
    {
        const string Path = "Assets/SportsGame/Resources/" + PostEffects.ProfilePath + ".asset";

        public static void CreateProfile()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            AssetDatabase.DeleteAsset(Path);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, Path);

            var tone = Add<Tonemapping>(profile);
            tone.mode.Override(TonemappingMode.Neutral);
            var color = Add<ColorAdjustments>(profile);
            color.postExposure.Override(0.15f);
            color.contrast.Override(8f);
            color.saturation.Override(14f);
            var bloom = Add<Bloom>(profile);
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.35f);
            bloom.scatter.Override(0.6f);
            var vignette = Add<Vignette>(profile);
            vignette.intensity.Override(0.18f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        /// <summary>エフェクトを追加し、プロファイルのサブアセットとして保存する。</summary>
        static T Add<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var c = profile.Add<T>(true);
            c.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(c, profile);
            return c;
        }
    }
}
