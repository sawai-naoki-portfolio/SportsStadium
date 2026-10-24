namespace SportsGame
{
    /// <summary>実行時に合成する効果音の種類。</summary>
    public enum Sfx
    {
        // ボール
        Dribble, BallFloor, TennisBounce, Racket, RacketStrong, Bump, Spike, SandHit,
        Swish, Rim, Board, NetHit,
        // 審判・会場
        Whistle, Buzzer, Cheer, Applause, Groan, Squeak,
        // UI
        Click, Confirm, Back, Point, WinJingle, LoseJingle
    }
}
