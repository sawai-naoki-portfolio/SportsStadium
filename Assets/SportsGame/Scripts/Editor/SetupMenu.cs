using UnityEditor;

namespace SportsGame.EditorTools
{
    /// <summary>
    /// プロジェクトのセットアップ用メニュー。初回やシーン・マテリアルを作り直したいときに実行する。
    /// </summary>
    public static class SetupMenu
    {
        const string Root = "SportsGame/";

        [MenuItem(Root + "Setup Project (Materials + Scenes + Settings)", priority = 0)]
        public static void SetupAll()
        {
            MaterialSetup.CreateBaseMaterials();
            PostEffectsSetup.CreateProfile();
            SceneSetup.CreateScenes();
            PlayerSettingsSetup.Apply();
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("[SportsGame] Setup complete.");
        }

        [MenuItem(Root + "Create Base Materials", priority = 20)]
        public static void Materials() => MaterialSetup.CreateBaseMaterials();

        [MenuItem(Root + "Create Post Effects Profile", priority = 20)]
        public static void PostEffects() => PostEffectsSetup.CreateProfile();

        [MenuItem(Root + "Create Scenes + Build Settings", priority = 21)]
        public static void Scenes() => SceneSetup.CreateScenes();

        [MenuItem(Root + "Configure Player Settings (Mobile)", priority = 22)]
        public static void Player() => PlayerSettingsSetup.Apply();
    }
}
