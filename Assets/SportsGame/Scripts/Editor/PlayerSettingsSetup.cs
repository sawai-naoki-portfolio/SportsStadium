using UnityEditor;
using UnityEditor.Build;

namespace SportsGame.EditorTools
{
    /// <summary>スマホ向けのプレイヤー設定(アプリ名・横画面固定・バンドルID・64bit ビルド)。</summary>
    public static class PlayerSettingsSetup
    {
        public const string ProductName = "スポーツスタジアム";
        public const string BundleId = "com.DefaultCompany.SportsStadium";

        public static void Apply()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "1.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.runInBackground = false;

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            // ストア公開に必要な 64bit(ARM64)ビルドは IL2CPP が前提
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
        }
    }
}
