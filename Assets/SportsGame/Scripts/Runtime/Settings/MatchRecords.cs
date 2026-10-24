using UnityEngine;

namespace SportsGame
{
    /// <summary>種目ごとの通算戦績(PlayerPrefs)。</summary>
    public static class MatchRecords
    {
        static string WinKey(Sport s) => "sg.win." + s;
        static string LoseKey(Sport s) => "sg.lose." + s;

        public static int Wins(Sport s) => Mathf.Max(0, PlayerPrefs.GetInt(WinKey(s), 0));
        public static int Losses(Sport s) => Mathf.Max(0, PlayerPrefs.GetInt(LoseKey(s), 0));

        public static void Record(Sport s, bool win)
        {
            string key = win ? WinKey(s) : LoseKey(s);
            PlayerPrefs.SetInt(key, Mathf.Max(0, PlayerPrefs.GetInt(key, 0)) + 1);
            PlayerPrefs.Save();
        }

        public static void ResetAll()
        {
            foreach (var s in SportCatalog.All)
            {
                PlayerPrefs.DeleteKey(WinKey(s));
                PlayerPrefs.DeleteKey(LoseKey(s));
            }
            PlayerPrefs.Save();
        }
    }
}
