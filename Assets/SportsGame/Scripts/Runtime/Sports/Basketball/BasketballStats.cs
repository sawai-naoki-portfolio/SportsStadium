using System.Collections.Generic;

namespace SportsGame
{
    /// <summary>バスケットボールの試合統計。添字 0 = プレイヤー, 1 = CPU。</summary>
    public class BasketballStats
    {
        public readonly int[] attempts = new int[2], makes = new int[2], threeAttempts = new int[2], threeMakes = new int[2];
        public readonly int[] dunks = new int[2], rebounds = new int[2], steals = new int[2], blocks = new int[2];

        public void RecordAttempt(int team, bool three)
        {
            attempts[team]++;
            if (three) threeAttempts[team]++;
        }

        public void RecordMake(int team, bool three, bool dunk)
        {
            makes[team]++;
            if (three) threeMakes[team]++;
            if (dunk) dunks[team]++;
        }

        static string Pct(int made, int att) => att > 0 ? $"{made}/{att} ({100 * made / att}%)" : "0/0";

        public List<string> Lines() => new List<string>
        {
            $"FG  {Pct(makes[0], attempts[0])}",
            $"CPU FG  {Pct(makes[1], attempts[1])}",
            $"3P  {Pct(threeMakes[0], threeAttempts[0])}",
            $"CPU 3P  {Pct(threeMakes[1], threeAttempts[1])}",
            $"ダンク   {dunks[0]} - {dunks[1]}",
            $"リバウンド   {rebounds[0]} - {rebounds[1]}",
            $"スティール   {steals[0]} - {steals[1]}",
            $"ブロック   {blocks[0]} - {blocks[1]}",
        };
    }
}
