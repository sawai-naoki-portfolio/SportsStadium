using UnityEngine;

namespace SportsGame
{
    public enum RallyEnd { None, Kill, Ace, Out, Antenna, UnderNet, FourHits, DoubleTouch, Blocked }

    public struct VolleyballDecision
    {
        public bool point;
        public int winner;
        public RallyEnd reason;

        public static readonly VolleyballDecision Continue = new VolleyballDecision { winner = -1 };
        public static VolleyballDecision PointTo(int winner, RallyEnd reason) => new VolleyballDecision { point = true, winner = winner, reason = reason };
    }

    /// <summary>
    /// ラリーの審判(タッチ数・連続タッチ・ネット通過・着地のイン/アウト)。Unity の描画等には依存しない。
    /// ビーチバレーのルールに従い、ブロックも1回のタッチに数える。
    /// </summary>
    public class VolleyballReferee
    {
        const int MaxTouches = 3;

        bool lastWasBlock;

        public int LastTeam { get; private set; } = -1;
        public int LastPlayer { get; private set; } = -1;
        /// <summary>LastTeam が自陣で触った回数。</summary>
        public int Touches { get; private set; }
        /// <summary>サーブが相手に触られずに飛んでいる。</summary>
        public bool IsServe { get; private set; }

        public void BeginServe(int team, int player)
        {
            LastTeam = team;
            LastPlayer = player;
            Touches = 0;
            IsServe = true;
            lastWasBlock = false;
        }

        /// <summary>team が次に触るとき何回目のタッチになるか(0 始まり)。</summary>
        public int TouchIndexFor(int team) => team == LastTeam && !IsServe ? Touches : 0;

        public VolleyballDecision RegisterTouch(int team, int player, bool block)
        {
            bool newSide = team != LastTeam || IsServe;
            IsServe = false;
            if (newSide)
            {
                LastTeam = team;
                LastPlayer = player;
                Touches = 1;
                lastWasBlock = block;
                return VolleyballDecision.Continue;
            }
            if (player == LastPlayer && !lastWasBlock) return VolleyballDecision.PointTo(1 - team, RallyEnd.DoubleTouch);
            Touches++;
            LastPlayer = player;
            lastWasBlock = block;
            return Touches > MaxTouches ? VolleyballDecision.PointTo(1 - team, RallyEnd.FourHits) : VolleyballDecision.Continue;
        }

        /// <summary>ボールが z=0 を横切った。アンテナの外・ネットの下を通ったら反則。</summary>
        public VolleyballDecision RegisterNetCrossing(Vector3 p)
        {
            if (LastTeam < 0) return VolleyballDecision.Continue;
            if (Mathf.Abs(p.x) > VolleyballDims.AntennaX + VolleyballDims.BallRadius) return VolleyballDecision.PointTo(1 - LastTeam, RallyEnd.Antenna);
            if (p.y < VolleyballDims.NetBottom) return VolleyballDecision.PointTo(1 - LastTeam, RallyEnd.UnderNet);
            return VolleyballDecision.Continue;
        }

        public VolleyballDecision RegisterGround(Vector3 p)
        {
            if (LastTeam < 0) return VolleyballDecision.Continue;
            int side = VolleyballDims.SideOf(p);
            if (VolleyballDims.InCourt(p))
            {
                RallyEnd reason = IsServe ? RallyEnd.Ace : lastWasBlock && side != LastTeam ? RallyEnd.Blocked : RallyEnd.Kill;
                return VolleyballDecision.PointTo(1 - side, reason);
            }
            return VolleyballDecision.PointTo(1 - LastTeam, RallyEnd.Out);
        }
    }
}
