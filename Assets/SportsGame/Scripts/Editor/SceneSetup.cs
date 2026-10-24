using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SportsGame.EditorTools
{
    /// <summary>
    /// タイトルと各競技のシーンを作り、ビルド設定に登録する。
    /// シーンにはカメラ・ライトと起動用コンポーネントだけを置き、会場は実行時に生成する。
    /// </summary>
    public static class SceneSetup
    {
        const string Folder = "Assets/SportsGame/Scenes";

        static readonly (string scene, System.Type component)[] Scenes =
        {
            (SceneFlow.TitleScene, typeof(TitleScreen)),
            (SportCatalog.SceneName(Sport.Basketball), typeof(BasketballGame)),
            (SportCatalog.SceneName(Sport.Tennis), typeof(TennisGame)),
            (SportCatalog.SceneName(Sport.Volleyball), typeof(VolleyballGame)),
        };

        public static void CreateScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Folder);
            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (var (sceneName, component) in Scenes)
            {
                string path = $"{Folder}/{sceneName}.unity";
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                new GameObject(sceneName + "Game").AddComponent(component);
                EditorSceneManager.SaveScene(scene, path);
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();
            EditorSceneManager.OpenScene($"{Folder}/{SceneFlow.TitleScene}.unity", OpenSceneMode.Single);
        }
    }
}
