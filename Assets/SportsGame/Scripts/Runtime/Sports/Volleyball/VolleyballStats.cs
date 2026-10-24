using System.Collections.Generic;

namespace SportsGame
{
    /// <summary>ビーチバレーの試合統計。添字 0 = プレイヤーのチーム, 1 = CPU チーム。</summary>
    public class VolleyballStats
    {
        public readonly int[] points = new int[2], kills = new int[2], aces = new int[2], blocks = new int[2], errors = new int[2], digs = new int[2];
        public int longestRally, humanSpikes, humanKills;

        public void RecordPoint(int winner, RallyEnd reason)
        {
            points[winner]++;
            switch (reason)
            {
                case RallyEnd.Kill: kills[winner]++; break;
                case RallyEnd.Ace: aces[winner]++; break;
                case RallyEnd.Blocked: blocks[winner]++; break;
                default: errors[1 - winner]++; break;
            }
        }

        public List<string> Lines() => new List<string>
        {
            $"獲得ポイント   {points[0]} - {points[1]}",
            $"アタック決定   {kills[0]} - {kills[1]}",
            $"サービスエース   {aces[0]} - {aces[1]}",
            $"ブロック決定   {blocks[0]} - {blocks[1]}",
            $"ミス   {errors[0]} - {errors[1]}",
            $"ディグ(スパイクレシーブ)   {digs[0]} - {digs[1]}",
            $"YOU のスパイク決定   {humanKills}/{humanSpikes}",
            $"最長ラリー   {longestRally}回",
        };
    }
}
