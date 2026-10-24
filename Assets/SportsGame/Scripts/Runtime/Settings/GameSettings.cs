using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// ユーザー設定と試合の選択状態。PlayerPrefs に保存し、読み込み時は値を検証して範囲内に収める。
    /// </summary>
    public static class GameSettings
    {
        const string KeyBgm = "sg.bgm", KeySe = "sg.se", KeyVibration = "sg.vib", KeyQuality = "sg.hq",
            KeyAssist = "sg.assist", KeySport = "sg.sport", KeyDifficulty = "sg.diff", KeyLength = "sg.len", KeyTeam = "sg.team";

        public static Sport SelectedSport = Sport.Basketball;
        public static Difficulty Difficulty = Difficulty.Normal;
        public static int MatchLength;
        public static int TeamColor;
        public static float BgmVolume = 0.55f;
        public static float SeVolume = 0.9f;
        public static bool Vibration = true;
        public static bool HighQuality = true;
        public static bool MoveAssist = true;

        static bool loaded;

        public static float Skill => Difficulty.Skill();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => loaded = false;

        public static void Load()
        {
            if (loaded) return;
            loaded = true;
            BgmVolume = ReadVolume(KeyBgm, 0.55f);
            SeVolume = ReadVolume(KeySe, 0.9f);
            Vibration = PlayerPrefs.GetInt(KeyVibration, 1) == 1;
            HighQuality = PlayerPrefs.GetInt(KeyQuality, 1) == 1;
            MoveAssist = PlayerPrefs.GetInt(KeyAssist, 1) == 1;
            SelectedSport = ReadEnum(KeySport, Sport.Basketball);
            Difficulty = ReadEnum(KeyDifficulty, Difficulty.Normal);
            MatchLength = Mathf.Clamp(PlayerPrefs.GetInt(KeyLength, 0), 0, SportCatalog.MatchLengthCount - 1);
            TeamColor = Mathf.Clamp(PlayerPrefs.GetInt(KeyTeam, 0), 0, TeamColors.Count - 1);
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(KeyBgm, Mathf.Clamp01(BgmVolume));
            PlayerPrefs.SetFloat(KeySe, Mathf.Clamp01(SeVolume));
            PlayerPrefs.SetInt(KeyVibration, Vibration ? 1 : 0);
            PlayerPrefs.SetInt(KeyQuality, HighQuality ? 1 : 0);
            PlayerPrefs.SetInt(KeyAssist, MoveAssist ? 1 : 0);
            PlayerPrefs.SetInt(KeySport, (int)SelectedSport);
            PlayerPrefs.SetInt(KeyDifficulty, (int)Difficulty);
            PlayerPrefs.SetInt(KeyLength, Mathf.Clamp(MatchLength, 0, SportCatalog.MatchLengthCount - 1));
            PlayerPrefs.SetInt(KeyTeam, Mathf.Clamp(TeamColor, 0, TeamColors.Count - 1));
            PlayerPrefs.Save();
        }

        static float ReadVolume(string key, float fallback)
        {
            float v = PlayerPrefs.GetFloat(key, fallback);
            return float.IsNaN(v) || float.IsInfinity(v) ? fallback : Mathf.Clamp01(v);
        }

        static T ReadEnum<T>(string key, T fallback) where T : System.Enum
        {
            int v = PlayerPrefs.GetInt(key, System.Convert.ToInt32(fallback));
            return System.Enum.IsDefined(typeof(T), v) ? (T)System.Enum.ToObject(typeof(T), v) : fallback;
        }
    }
}
