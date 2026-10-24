using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// 試合中 HUD の窓口。スコアボード・バナー・ヒント・操作 UI・ポーズ・リザルトをまとめる。
    /// </summary>
    public class MatchHud : MonoBehaviour
    {
        public Scoreboard Scoreboard { get; private set; }
        public CenterDisplay Center { get; private set; }
        public BannerView Banner { get; private set; }
        public ActionControls Controls { get; private set; }
        RectTransform root;
        HintView hint;
        PauseMenu pause;
        bool resultShown;

        public bool Paused => pause.IsOpen;
        internal bool ResultShown => resultShown;

        public static MatchHud Create(string leftName, Color leftColor, string rightName, Color rightColor, ButtonSpec[] buttons)
        {
            var canvas = UIKit.CreateCanvas("HUD", 10);
            var hud = canvas.gameObject.AddComponent<MatchHud>();
            hud.root = UIKit.SafeRoot(canvas);
            hud.Controls = ActionControls.Create(hud.root, buttons);
            hud.Scoreboard = Scoreboard.Create(hud.root, leftName, leftColor, rightName, rightColor);
            hud.Center = new CenterDisplay(hud.root);
            hud.Banner = BannerView.Create(hud.root);
            hud.hint = new HintView(hud.root);
            hud.pause = new PauseMenu(canvas.transform);
            var pauseButton = UIKit.Button(hud.root, "Pause", "II", new Color(0.15f, 0.18f, 0.26f, 0.85f), hud.TogglePause, 44);
            UIKit.Place((RectTransform)pauseButton.transform, Vector2.one, Vector2.one, new Vector2(-28f, -26f), new Vector2(110f, 100f));
            return hud;
        }

        public void Hint(string message) => hint.Set(message);

        public HudMeter CreateMeter(Color fill) => HudMeter.Create(root, fill);

        public Vector2 WorldToHud(Vector3 world, Camera cam)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, cam.WorldToScreenPoint(world), null, out var lp);
            return lp;
        }

        public void ShowResult(MatchResult result)
        {
            if (resultShown) return;
            resultShown = true;
            pause.Close();
            Controls.SetVisible(false);
            Banner.Hide();
            Hint(null);
            ResultPanel.Show(transform, result);
        }

        void Update()
        {
            if (GameInput.PauseDown) TogglePause();
        }

        // アプリがバックグラウンドへ回ったら自動でポーズする
        void OnApplicationPause(bool paused)
        {
            if (paused && !resultShown) pause.Open();
        }

        void OnDestroy()
        {
            if (pause != null && pause.IsOpen) Time.timeScale = 1f;
        }

        void TogglePause()
        {
            if (resultShown) return;
            if (pause.IsOpen) pause.Close();
            else pause.Open();
        }
    }
}
