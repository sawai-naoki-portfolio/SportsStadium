using UnityEngine;
using UnityEngine.SceneManagement;

namespace SportsGame
{
    /// <summary>シーン遷移の窓口。ビルド設定に無いシーンへの遷移はタイトルへ退避する。</summary>
    public static class SceneFlow
    {
        public const string TitleScene = "Title";

        public static void GoTitle() => Load(TitleScene);

        public static void GoSport(Sport sport)
        {
            GameSettings.SelectedSport = sport;
            Load(SportCatalog.SceneName(sport));
        }

        public static void Reload() => Load(SceneManager.GetActiveScene().name);

        static void Load(string scene)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                GameLog.Error("SceneFlow", $"Scene '{scene}' is not in Build Settings. (SportsGame > Setup Project)");
                if (scene == TitleScene || !Application.CanStreamedLevelBeLoaded(TitleScene)) return;
                scene = TitleScene;
            }
            ScreenFader.Instance.FadeToScene(scene);
        }
    }
}
