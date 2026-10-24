using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SportsGame.Tests
{
    /// <summary>
    /// 各シーンを実際に起動し、エラーを出さずに試合が進むこと・タッチ操作が効くことを確認するスモークテスト。
    /// (エラーログが出るとテストは自動的に失敗する)
    /// </summary>
    public class SceneSmokeTests
    {
        const float SimSpeed = 8f;
        const float TimeoutSeconds = 60f;
        const int TestPointer = 42;

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            DebugOptions.AutoPlay = false;
        }

        static IEnumerator Load(string scene)
        {
            SceneManager.LoadScene(scene);
            yield return null;
            yield return null;
        }

        static IEnumerator WaitFor(System.Func<bool> condition, float realSeconds = TimeoutSeconds)
        {
            float end = Time.realtimeSinceStartup + realSeconds;
            while (!condition() && Time.realtimeSinceStartup < end) yield return null;
        }

        static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>();

        static void Click(string path)
        {
            var go = GameObject.Find(path);
            Assert.IsNotNull(go, path + " が見つからない");
            go.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        }

        /// <summary>画面のアクションボタンを指で押す/離す操作を再現する。</summary>
        static TouchButton ActionButton(int index)
        {
            var go = GameObject.Find("HUD/SafeArea/Controls/Btn" + index);
            Assert.IsNotNull(go, "アクションボタンが見つからない");
            return go.GetComponent<TouchButton>();
        }

        static PointerEventData Pointer() => new PointerEventData(EventSystem.current) { pointerId = TestPointer };

        static IEnumerator Tap(TouchButton b, float holdGameSeconds = 0.05f)
        {
            b.OnPointerDown(Pointer());
            yield return new WaitForSeconds(holdGameSeconds);
            b.OnPointerUp(Pointer());
            yield return null;
        }

        [UnityTest]
        public IEnumerator TitleNavigatesToMatchSetup()
        {
            yield return Load(SceneFlow.TitleScene);
            Assert.IsNotNull(Find<TitleScreen>());
            Click("TitleUI/SafeArea/Start/Tap");
            Click("TitleUI/SafeArea/Select/Cards/CardTennis");
            yield return null;
            Assert.IsNotNull(GameObject.Find("TitleUI/SafeArea/Setup/Panel/Start"), "試合設定画面が開かない");
        }

        [UnityTest]
        public IEnumerator TennisAutoPlayScoresPoints()
        {
            yield return AutoPlay("Tennis", () => { var s = Find<TennisGame>().Stats; return s.pointsWon[0] + s.pointsWon[1] >= 2; });
        }

        [UnityTest]
        public IEnumerator BasketballAutoPlayScoresPoints()
        {
            yield return AutoPlay("Basketball", () => { var s = Find<BasketballGame>().Score; return s.Points(0) + s.Points(1) >= 4; });
        }

        [UnityTest]
        public IEnumerator VolleyballAutoPlayScoresPoints()
        {
            yield return AutoPlay("Volleyball", () => { var s = Find<VolleyballGame>().Stats; return s.points[0] + s.points[1] >= 2; });
        }

        static IEnumerator AutoPlay(string scene, System.Func<bool> progressed)
        {
            DebugOptions.AutoPlay = true;
            yield return Load(scene);
            Time.timeScale = SimSpeed;
            yield return WaitFor(progressed);
            Assert.IsTrue(progressed(), scene + " の試合が進まない");
        }

        [UnityTest]
        public IEnumerator BasketballHumanCanShoot()
        {
            yield return Load("Basketball");
            var game = Find<BasketballGame>();
            Time.timeScale = 4f;
            yield return WaitFor(() => game.Holder == game.HumanPlayer && game.PhaseName == "Live" && !game.NeedsClear);
            Assert.AreEqual(game.HumanPlayer, game.Holder, "プレイヤーにボールが回ってこない");
            int before = game.Stats.attempts[0];
            yield return Tap(ActionButton(0), 0.4f);
            yield return WaitFor(() => game.Stats.attempts[0] > before, 5f);
            Assert.Greater(game.Stats.attempts[0], before, "シュートボタンでシュートできない");
        }

        [UnityTest]
        public IEnumerator TennisHumanCanServe()
        {
            yield return Load("Tennis");
            var game = Find<TennisGame>();
            Time.timeScale = 4f;
            yield return WaitFor(() => game.PointServer == 0 && game.PhaseName == "PreServe");
            Assert.AreEqual("PreServe", game.PhaseName, "プレイヤーのサーブ番が来ない");
            var a = ActionButton(0);
            yield return Tap(a);
            Assert.AreEqual("Toss", game.PhaseName, "ボタンでトスできない");
            yield return new WaitForSeconds(0.45f);
            yield return Tap(a);
            Assert.AreEqual("Rally", game.PhaseName, "トス中のボタンでサーブできない");
        }

        [UnityTest]
        public IEnumerator VolleyballHumanCanServe()
        {
            yield return Load("Volleyball");
            var game = Find<VolleyballGame>();
            Time.timeScale = 4f;
            yield return WaitFor(() => game.Score.ServingTeam == 0 && game.Score.ServerSlot == 0 && game.PhaseName == "PreServe");
            Assert.AreEqual("PreServe", game.PhaseName, "プレイヤーのサーブ番が来ない");
            var a = ActionButton(0);
            yield return Tap(a);
            Assert.AreEqual("Toss", game.PhaseName, "ボタンでトスできない");
            yield return new WaitForSeconds(0.4f);
            yield return Tap(a);
            Assert.AreEqual("Rally", game.PhaseName, "トス中のボタンでサーブできない");
        }
    }
}
