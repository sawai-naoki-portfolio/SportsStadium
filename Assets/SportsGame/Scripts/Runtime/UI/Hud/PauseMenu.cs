using UnityEngine;

namespace SportsGame
{
    /// <summary>ポーズメニュー。開いている間は Time.timeScale を 0 にする。</summary>
    public class PauseMenu
    {
        readonly Transform canvas;
        ModalPanel modal;

        public bool IsOpen => modal != null;

        public PauseMenu(Transform canvas) { this.canvas = canvas; }

        public void Open()
        {
            if (IsOpen) return;
            Time.timeScale = 0f;
            modal = ModalPanel.Open(canvas, "ポーズ", new Vector2(760f, 560f));
            AddButton("Resume", "試合に戻る", UITheme.Green, -150f, Close, Sfx.Click, 44);
            AddButton("Restart", "最初からやり直す", UITheme.Blue, -285f, () => { Close(); SceneFlow.Reload(); }, Sfx.Click, 40);
            AddButton("Quit", "種目選択へ戻る", UITheme.Muted, -410f, () => { Close(); SceneFlow.GoTitle(); }, Sfx.Back, 40);
        }

        public void Close()
        {
            if (!IsOpen) return;
            Time.timeScale = 1f;
            modal.Close();
            modal = null;
        }

        void AddButton(string name, string label, Color color, float y, UnityEngine.Events.UnityAction action, Sfx sound, int fontSize)
        {
            var b = UIKit.Button(modal.Body, name, label, color, action, fontSize, sound);
            UIKit.Place((RectTransform)b.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(560f, 105f));
        }
    }
}
