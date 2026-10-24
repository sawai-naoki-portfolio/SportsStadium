namespace SportsGame
{
    /// <summary>キャラクターが再生する動作。</summary>
    public enum HAction
    {
        None,
        // バスケットボール
        Shoot, Layup, Dunk, Block, Steal, Crossover,
        // テニス
        Forehand, Backhand, Serve, Smash,
        // バレーボール
        Bump, Set, Spike, Dive,
        // 共通
        Celebrate, Dejected
    }
}
