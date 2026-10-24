using System.Collections;
using UnityEngine;

namespace SportsGame
{
    /// <summary>試合シーンの初期設定。</summary>
    public struct MatchSetup
    {
        public string title;
        public string homeName, awayName;
        public ButtonSpec[] buttons;
        public float fov;
        public Color background;
        public bool skybox;
        public SceneLighting.Preset lighting;
        public Transform[] crowds;
        public float ambienceVolume;
        public Sfx[] warmup;
    }

    /// <summary>
    /// 3競技に共通する試合シーンの土台(カメラ・HUD・観客・音・試合終了の流れ)。
    /// 各競技はこれを継承し、ルールと操作だけを実装する。
    /// </summary>
    public abstract class MatchBase : MonoBehaviour
    {
        const float MaxFrameDelta = 0.05f;
        const float ResultDelay = 2.8f;

        protected MatchHud Hud { get; private set; }
        protected CameraRig Rig { get; private set; }
        protected CrowdAnimator Crowd { get; private set; }
        protected bool MatchOver { get; private set; }

        protected abstract Sport Sport { get; }

        /// <summary>ポーズ中・リザルト表示中は試合を進めない。</summary>
        protected bool IsPaused => Hud == null || Hud.Paused;

        /// <summary>処理落ち時にボールがすり抜けないよう上限を設けたフレーム時間。</summary>
        protected static float FrameDelta => Mathf.Min(Time.deltaTime, MaxFrameDelta);

        protected void InitMatch(MatchSetup s)
        {
            SceneLighting.Apply(s.lighting);
            Rig = CameraRig.Create(s.fov, s.background, s.skybox);
            Hud = MatchHud.Create(s.homeName, UITheme.Home, s.awayName, UITheme.Away, s.buttons);
            Crowd = CrowdAnimator.Attach(gameObject, s.crowds);
            AudioManager.SetTitleMusic(false);
            AudioManager.SetAmbience(true, s.ambienceVolume);
            if (s.warmup != null) AudioManager.Warmup(s.warmup);
            AudioManager.Warmup(Sfx.Applause, Sfx.Cheer, Sfx.Groan, Sfx.Whistle, Sfx.Point, Sfx.WinJingle, Sfx.LoseJingle);
            Hud.Banner.Show(s.title, UITheme.Gold, 1.6f, SportCatalog.IntroSubtitle(Sport));
        }

        /// <summary>得点時の歓声・振動。</summary>
        protected void PointFeedback(bool humanScored, bool highlight)
        {
            AudioManager.Play(humanScored ? Sfx.Applause : Sfx.Groan, humanScored ? 0.7f : 0.45f);
            if (highlight)
            {
                AudioManager.Play(Sfx.Cheer, 0.6f);
                Crowd.Cheer(1.5f);
            }
            if (humanScored) Haptics.Vibrate();
        }

        /// <summary>試合終了の共通演出(戦績保存・ジングル・紙吹雪・リザルト表示)。</summary>
        protected void FinishMatch(bool humanWon, string bannerTitle, Vector3 confettiAt, System.Func<MatchResult> buildResult)
        {
            if (MatchOver) return;
            MatchOver = true;
            // 戦績は人が遊んだ試合だけ記録する(自動対戦・テストでは残さない)
            if (!DebugOptions.AutoPlay) MatchRecords.Record(Sport, humanWon);
            Hud.Banner.Show(bannerTitle, UITheme.Gold, 2f, humanWon ? "YOU の勝利！" : "CPU の勝利");
            Hud.Hint(null);
            AudioManager.Play(humanWon ? Sfx.WinJingle : Sfx.LoseJingle, 0.9f);
            AudioManager.Play(Sfx.Cheer, 0.8f);
            Crowd.Cheer(3f);
            if (humanWon) Fx.Confetti(confettiAt);
            After(ResultDelay, () => Hud.ShowResult(buildResult()));
        }

        /// <summary>seconds 秒後(ポーズ中は止まる)に action を実行する。</summary>
        protected Coroutine After(float seconds, System.Action action) => StartCoroutine(AfterRoutine(seconds, action));

        static IEnumerator AfterRoutine(float seconds, System.Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }

        /// <summary>ダンク・スマッシュなど決定的な瞬間の短いスローモーション(実時間 realSeconds)。</summary>
        protected void SlowMotion(float scale = 0.35f, float realSeconds = 0.35f)
        {
            // テストなどで時間を固定しているときは演出しない
            if (IsPaused || Time.captureDeltaTime > 0f || !Mathf.Approximately(Time.timeScale, 1f)) return;
            StartCoroutine(SlowMotionRoutine(scale, realSeconds));
        }

        IEnumerator SlowMotionRoutine(float scale, float realSeconds)
        {
            Time.timeScale = scale;
            yield return new WaitForSecondsRealtime(realSeconds);
            // ポーズ中はポーズ側が timeScale を管理するので戻さない
            if (!IsPaused) Time.timeScale = 1f;
        }

        void OnDisable()
        {
            if (!IsPaused && Time.timeScale < 1f && Time.timeScale > 0f) Time.timeScale = 1f;
        }
    }
}
