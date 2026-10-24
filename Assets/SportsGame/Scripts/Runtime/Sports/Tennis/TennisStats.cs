using System.Collections.Generic;
using UnityEngine;

namespace SportsGame
{
    /// <summary>テニスの試合統計(リザルト画面用)。添字 0 = プレイヤー, 1 = CPU。</summary>
    public class TennisStats
    {
        public readonly int[] pointsWon = new int[2], aces = new int[2], doubleFaults = new int[2], winners = new int[2], errors = new int[2];
        public int firstServes, firstServesIn, longestRally;

        public void RecordPoint(int winner, PointReason reason)
        {
            pointsWon[winner]++;
            int loser = 1 - winner;
            switch (reason)
            {
                case PointReason.Ace: aces[winner]++; break;
                case PointReason.Winner: winners[winner]++; break;
                case PointReason.DoubleFault: doubleFaults[loser]++; errors[loser]++; break;
                case PointReason.Out:
                case PointReason.Net: errors[loser]++; break;
            }
        }

        public List<string> Lines()
        {
            int pct = firstServes > 0 ? Mathf.RoundToInt(100f * firstServesIn / firstServes) : 0;
            return new List<string>
            {
                $"獲得ポイント   {pointsWon[0]} - {pointsWon[1]}",
                $"サービスエース   {aces[0]} - {aces[1]}",
                $"ウィナー   {winners[0]} - {winners[1]}",
                $"ミス   {errors[0]} - {errors[1]}",
                $"ダブルフォルト   {doubleFaults[0]} - {doubleFaults[1]}",
                $"1stサーブ成功率   {pct}%",
                $"最長ラリー   {longestRally}回",
                $"難易度   {SportCatalog.DifficultyLabel(GameSettings.Difficulty)}",
            };
        }
    }
}
