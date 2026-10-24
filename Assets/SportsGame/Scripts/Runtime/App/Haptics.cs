using UnityEngine;

namespace SportsGame
{
    /// <summary>端末の振動。設定でオフにできる。</summary>
    public static class Haptics
    {
        public static void Vibrate()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (GameSettings.Vibration) Handheld.Vibrate();
#endif
        }
    }
}
