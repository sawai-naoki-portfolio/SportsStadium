using UnityEngine;
using UnityEngine.SceneManagement;

namespace SportsGame
{
    /// <summary>シーン遷移の窓口。ビルド設定に無いシーンへの遷移はタイトルへ退避する。</summary>
    public static class SceneFlow
    {
        public const string TitleScene = "Title";

        /// <summary>次に開く Title シーンで、スタート画面を飛ばして種目選択から始めるか。</summary>
        static bool openSelectOnTitle;

        public static void GoTitle()
        {
            openSelectOnTitle = false;
            Load(TitleScene);
        }

        /// <summary>試合から戻るとき用。Title シーンを読み直し、ボタンの文言どおり種目選択から始める。</summary>
        public static void GoSportSelect()
        {
            openSelectOnTitle = true;
            Load(TitleScene);
        }

        /// <summary>種目選択から始めるかを返してフラグを戻す(TitleScreen が起動時に 1 回だけ読む)。</summary>
        public static bool ConsumeOpenSelect()
        {
            bool open = openSelectOnTitle;
            openSelectOnTitle = false;
            return open;
        }

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
