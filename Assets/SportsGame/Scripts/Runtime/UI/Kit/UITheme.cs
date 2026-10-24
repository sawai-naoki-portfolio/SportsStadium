using UnityEngine;

namespace SportsGame
{
    /// <summary>UI 共通の配色とフォント。</summary>
    public static class UITheme
    {
        public static readonly Color Accent = new Color(1f, 0.56f, 0.12f);
        public static readonly Color Blue = new Color(0.18f, 0.48f, 0.95f);
        public static readonly Color Green = new Color(0.2f, 0.78f, 0.4f);
        public static readonly Color Gold = new Color(1f, 0.82f, 0.25f);
        public static readonly Color Muted = new Color(0.4f, 0.43f, 0.52f);
        public static readonly Color SubText = new Color(0.8f, 0.86f, 1f);
        public static readonly Color Dark = new Color(0.07f, 0.09f, 0.14f, 0.92f);
        public static readonly Color Panel = new Color(0.1f, 0.13f, 0.2f, 0.94f);
        public static readonly Color ToggleOff = new Color(0.22f, 0.26f, 0.36f);

        /// <summary>プレイヤー側(ホーム、設定で選んだ色)と CPU 側(アウェイ)のチームカラー。</summary>
        public static Color Home => TeamColors.Get(GameSettings.TeamColor);
        public static readonly Color Away = new Color(0.9f, 0.25f, 0.25f);

        static Font font;

        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.Load<Font>("Fonts/GameFont");
                if (font == null)
                {
                    GameLog.Warn("UI", "Fonts/GameFont not found. Falling back to the built-in font (Japanese may not render).");
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return font;
            }
        }
    }
}
