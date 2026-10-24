using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SportsGame
{
    /// <summary>
    /// 高画質設定のときだけポストエフェクト(トーンマッピング・ブルーム・色調補正)を有効にする。
    /// プロファイルは Resources のアセット(ビルド時のシェーダー除外対策)を使う。
    /// </summary>
    public static class PostEffects
    {
        public const string ProfilePath = "SportsGame/PostFX";
        const float VolumePriority = 10f;

        public static void Apply(Camera cam)
        {
            if (cam == null) return;
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = false;
            if (!GameSettings.HighQuality) return;
            var profile = Resources.Load<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                GameLog.Warn("PostEffects", "PostFX profile is missing. Run SportsGame > Setup Project.");
                return;
            }
            var volume = new GameObject("[PostFX]").AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = VolumePriority;
            volume.sharedProfile = profile;
            data.renderPostProcessing = true;
        }
    }
}
