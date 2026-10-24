namespace SportsGame
{
    /// <summary>プレイヤーの操作(スティック移動・A=シュート/ジャンプ・B=クロス/スティール・C=ダッシュ)。</summary>
    public class BasketballHumanController : IBasketballController
    {
        readonly IBasketballView view;

        public BasketballHumanController(IBasketballView view) { this.view = view; }

        public bool IsHuman => true;

        public BBCommand Decide(BasketballPlayer self, float dt) => new BBCommand
        {
            move = GameInput.MoveWorld(view.CameraTransform),
            sprint = GameInput.C.Held,
            actionDown = GameInput.A.Down,
            actionHeld = GameInput.A.Held,
            actionUp = GameInput.A.Up,
            skillDown = GameInput.B.Down,
        };
    }
}
