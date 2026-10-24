using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// 開発用オプション。エディタと開発ビルド以外では常に無効になる。
    /// </summary>
    public static class DebugOptions
    {
        static bool autoPlay;

        /// <summary>プレイヤー側も CPU が操作する(動作確認・バランス調整用)。</summary>
        public static bool AutoPlay
        {
            get => autoPlay && (Application.isEditor || Debug.isDebugBuild);
            set => autoPlay = value;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => autoPlay = false;
    }
}
