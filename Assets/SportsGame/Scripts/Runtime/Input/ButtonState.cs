namespace SportsGame
{
    /// <summary>1フレーム分のボタン状態。</summary>
    public struct ButtonState
    {
        public bool Down;
        public bool Held;
        public bool Up;
        public float HeldTime;

        /// <summary>
        /// 現在の押下状態から次フレームの状態を求める。
        /// pressedEvent / releasedEvent はタッチのように1フレーム内で押下と解放が起き得る入力のイベント。
        /// </summary>
        public void Advance(bool held, bool pressedEvent, bool releasedEvent, float dt)
        {
            bool wasHeld = Held;
            Down = (held && !wasHeld) || pressedEvent;
            Up = (!held && wasHeld) || (releasedEvent && !held);
            Held = held;
            if (Down) HeldTime = 0f;
            if (held) HeldTime += dt;
        }
    }
}
