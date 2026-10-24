using UnityEngine;

namespace SportsGame
{
    /// <summary>設定画面(音量・振動・画質・移動アシスト・戦績リセット)。</summary>
    public static class OptionsPanel
    {
        const float RowStep = 115f;

        public static ModalPanel Open(Transform parent, System.Action onClose, System.Action onRecordsReset)
        {
            var modal = ModalPanel.Open(parent, "設定", new Vector2(1200f, 980f), 0.5f);
            var body = modal.Body;
            float y = -150f;
            AddRow(body, "BGM 音量", y);
            AddSlider(body, y, GameSettings.BgmVolume, v => GameSettings.BgmVolume = v);
            y -= RowStep;
            AddRow(body, "効果音 音量", y);
            AddSlider(body, y, GameSettings.SeVolume, v => GameSettings.SeVolume = v);
            y -= RowStep;
            AddRow(body, "振動", y);
            AddToggle(body, y, GameSettings.Vibration, v => GameSettings.Vibration = v, "ON", "OFF");
            y -= RowStep;
            AddRow(body, "画質", y);
            AddToggle(body, y, GameSettings.HighQuality, v => GameSettings.HighQuality = v, "高画質", "省電力");
            y -= RowStep;
            AddRow(body, "移動アシスト", y);
            AddToggle(body, y, GameSettings.MoveAssist, v => GameSettings.MoveAssist = v, "ON", "OFF");

            AddResetButton(body, onRecordsReset);
            var close = UIKit.Button(body, "Close", "とじる", UITheme.Blue, () =>
            {
                GameSettings.Save();
                modal.Close();
                onClose();
            }, 44, Sfx.Back);
            UIKit.Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260f, 50f), new Vector2(460f, 110f));
            return modal;
        }

        /// <summary>誤操作防止のため2回押しでリセットする。</summary>
        static void AddResetButton(RectTransform body, System.Action onReset)
        {
            int taps = 0;
            UnityEngine.UI.Button reset = null;
            reset = UIKit.Button(body, "Reset", "戦績をリセット", new Color(0.6f, 0.25f, 0.25f), () =>
            {
                var label = UIKit.ButtonLabel(reset);
                if (++taps == 1)
                {
                    label.text = "もう一度押すとリセット";
                    return;
                }
                if (taps > 2) return;
                MatchRecords.ResetAll();
                label.text = "リセットしました";
                onReset();
            }, 36);
            UIKit.Place((RectTransform)reset.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-260f, 50f), new Vector2(460f, 110f));
        }

        static void AddRow(RectTransform body, string text, float y)
        {
            var l = UIKit.Label(body, text, text, 40, UITheme.SubText, TextAnchor.MiddleLeft, false);
            UIKit.Place(l.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 0.5f), new Vector2(-520f, y - 40f), new Vector2(380f, 80f));
        }

        static void AddSlider(RectTransform body, float y, float value, System.Action<float> set)
        {
            var s = UIKit.Slider(body, "Slider", value, UITheme.Accent, v => set(v));
            UIKit.Place((RectTransform)s.transform, new Vector2(0.5f, 1f), new Vector2(0f, 0.5f), new Vector2(-120f, y - 40f), new Vector2(620f, 80f));
        }

        static void AddToggle(RectTransform body, float y, bool value, System.Action<bool> set, string on, string off)
        {
            var seg = UIKit.Segmented(body, new[] { on, off }, value ? 0 : 1, UITheme.Accent, i => { set(i == 0); GameSettings.Save(); }, 36);
            UIKit.Place(seg, new Vector2(0.5f, 1f), new Vector2(0f, 0.5f), new Vector2(-120f, y - 40f), new Vector2(620f, 86f));
        }
    }
}
