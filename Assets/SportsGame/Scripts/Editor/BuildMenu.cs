using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SportsGame.EditorTools
{
    /// <summary>ワンクリックでビルドするメニュー。出力先は Builds/(Git 管理外)。</summary>
    public static class BuildMenu
    {
        const string Root = "SportsGame/Build/";
        const string OutputFolder = "Builds";
        const string AndroidApk = "Android/SportsStadium.apk";

        [MenuItem(Root + "Android (APK)")]
        public static void Android() => Build(BuildTarget.Android, AndroidApk);

        /// <summary>APK を作り、USB でつないだ端末にインストールして起動する。</summary>
        [MenuItem(Root + "Android (APK) + Run on Device")]
        public static void AndroidRun() => Build(BuildTarget.Android, AndroidApk, BuildOptions.AutoRunPlayer);

        [MenuItem(Root + "Windows")]
        public static void Windows() => Build(BuildTarget.StandaloneWindows64, "Windows/SportsStadium.exe");

        /// <summary>シーンを登録済みのビルド設定でビルドする。成功したら true。</summary>
        public static bool Build(BuildTarget target, string relativePath, BuildOptions options = BuildOptions.None)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
            {
                Debug.LogError($"[SportsGame] {target} のビルドモジュールがありません。Unity Hub で追加してください。");
                return false;
            }
            PlayerSettingsSetup.Apply();
            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[SportsGame] ビルドするシーンがありません。SportsGame > Setup Project を実行してください。");
                return false;
            }
            string path = Path.Combine(OutputFolder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (target == BuildTarget.Android) EditorUserBuildSettings.buildAppBundle = false;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = target,
                options = options,
            });
            var summary = report.summary;
            bool ok = summary.result == BuildResult.Succeeded;
            string message = $"[SportsGame] Build {target}: {summary.result} ({summary.totalSize / (1024f * 1024f):F1} MB, {summary.totalTime.TotalSeconds:F0}s) -> {path}";
            if (ok) Debug.Log(message);
            else Debug.LogError(message);
            return ok;
        }
    }
}
