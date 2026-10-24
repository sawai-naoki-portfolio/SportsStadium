using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// アプリ起動時の初期化(フレームレート・画面向き・常駐オブジェクトの生成)。
    /// </summary>
    public static class AppBootstrap
    {
        static GameObject appRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            GameSettings.Load();
            ConfigureDevice();
            if (appRoot != null) return;
            appRoot = new GameObject("[App]");
            Object.DontDestroyOnLoad(appRoot);
            appRoot.AddComponent<GameInputDriver>();
            appRoot.AddComponent<AudioManager>();
        }

        static void ConfigureDevice()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}
