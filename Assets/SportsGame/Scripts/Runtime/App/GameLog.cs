using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace SportsGame
{
    /// <summary>
    /// ログ出力の窓口。Info はエディタとデバッグビルドのみ出力され、リリースビルドでは呼び出しごと除去される。
    /// </summary>
    public static class GameLog
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
        public static void Info(string category, string message) => Debug.Log($"[{category}] {message}");

        public static void Warn(string category, string message) => Debug.LogWarning($"[{category}] {message}");

        public static void Error(string category, string message) => Debug.LogError($"[{category}] {message}");
    }
}
