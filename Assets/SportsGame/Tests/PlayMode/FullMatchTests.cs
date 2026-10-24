using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SportsGame.Tests
{
    /// <summary>
    /// 各種目を自動対戦で最後まで戦わせ、試合終了 → リザルト画面までの流れを確認する。
    /// フレーム時間を固定して描画が追いつく限り高速に進める。
    /// </summary>
    public class FullMatchTests
    {
        const float FixedStep = 1f / 20f;
        const float TimeoutSeconds = 280f;
        const int TimeoutMs = 300000;

        Difficulty savedDifficulty;
        int savedLength;

        [SetUp]
        public void SetUp()
        {
            savedDifficulty = GameSettings.Difficulty;
            savedLength = GameSettings.MatchLength;
        }

        [TearDown]
        public void TearDown()
        {
            Time.captureDeltaTime = 0f;
            Time.timeScale = 1f;
            DebugOptions.AutoPlay = false;
            GameSettings.Difficulty = savedDifficulty;
            GameSettings.MatchLength = savedLength;
        }

        [UnityTest, Timeout(TimeoutMs)]
        public IEnumerator BasketballEasyPlaysToResult() => PlayToResult("Basketball", Difficulty.Easy);

        [UnityTest, Timeout(TimeoutMs)]
        public IEnumerator TennisNormalPlaysToResult() => PlayToResult("Tennis", Difficulty.Normal);

        [UnityTest, Timeout(TimeoutMs)]
        public IEnumerator VolleyballHardPlaysToResult() => PlayToResult("Volleyball", Difficulty.Hard);

        static IEnumerator PlayToResult(string scene, Difficulty difficulty)
        {
            GameSettings.Difficulty = difficulty;
            GameSettings.MatchLength = 0;
            DebugOptions.AutoPlay = true;
            SceneManager.LoadScene(scene);
            yield return null;
            yield return null;
            var hud = Object.FindAnyObjectByType<MatchHud>();
            Assert.IsNotNull(hud, "HUD がない");
            Time.captureDeltaTime = FixedStep;
            float end = Time.realtimeSinceStartup + TimeoutSeconds;
            while (!hud.ResultShown && Time.realtimeSinceStartup < end) yield return null;
            Assert.IsTrue(hud.ResultShown, $"{scene} の試合が時間内に終わらない");
            Assert.IsNotNull(GameObject.Find("HUD/Modal/Panel/Again"), "リザルトの「もう一度」ボタンがない");
            Assert.IsNotNull(GameObject.Find("HUD/Modal/Panel/Menu"), "リザルトの「種目選択へ」ボタンがない");
        }
    }
}
