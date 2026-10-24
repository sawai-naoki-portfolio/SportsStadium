using UnityEngine;
using UnityEngine.InputSystem;

namespace SportsGame
{
    /// <summary>
    /// タッチ(仮想スティック+ボタン)・キーボード・ゲームパッドを統合した入力。
    /// キーボード: WASD/矢印=移動, J/Space=A, K=B, L=C, Esc=ポーズ
    /// </summary>
    public static class GameInput
    {
        public const int ButtonCount = 3;

        public static Vector2 Move { get; private set; }
        public static ButtonState A => buttons[0];
        public static ButtonState B => buttons[1];
        public static ButtonState C => buttons[2];
        public static bool PauseDown { get; private set; }

        static readonly ButtonState[] buttons = new ButtonState[ButtonCount];
        static TouchJoystick joystick;
        static readonly TouchButton[] touchButtons = new TouchButton[ButtonCount];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            joystick = null;
            for (int i = 0; i < ButtonCount; i++)
            {
                touchButtons[i] = null;
                buttons[i] = default;
            }
            Move = Vector2.zero;
            PauseDown = false;
        }

        public static void Register(TouchJoystick stick) => joystick = stick;

        public static void Unregister(TouchJoystick stick)
        {
            if (joystick == stick) joystick = null;
        }

        public static void Register(int index, TouchButton button)
        {
            if (index >= 0 && index < ButtonCount) touchButtons[index] = button;
        }

        public static void Unregister(TouchButton button)
        {
            for (int i = 0; i < ButtonCount; i++)
                if (touchButtons[i] == button) touchButtons[i] = null;
        }

        public static void Tick(float dt)
        {
            var kb = Keyboard.current;
            var gp = Gamepad.current;
            Move = Vector2.ClampMagnitude(ReadMove(kb, gp), 1f);

            AdvanceButton(0, kb != null && (kb.jKey.isPressed || kb.spaceKey.isPressed), gp != null && gp.buttonSouth.isPressed, dt);
            AdvanceButton(1, kb != null && kb.kKey.isPressed, gp != null && gp.buttonEast.isPressed, dt);
            AdvanceButton(2, kb != null && kb.lKey.isPressed, gp != null && (gp.buttonWest.isPressed || gp.rightTrigger.isPressed), dt);

            PauseDown = (kb != null && kb.escapeKey.wasPressedThisFrame) || (gp != null && gp.startButton.wasPressedThisFrame);
        }

        static Vector2 ReadMove(Keyboard kb, Gamepad gp)
        {
            Vector2 m = joystick != null && joystick.isActiveAndEnabled ? joystick.Value : Vector2.zero;
            if (kb != null)
            {
                float x = 0f, y = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
                if (x != 0f || y != 0f) m = new Vector2(x, y).normalized;
            }
            if (gp != null)
            {
                var s = gp.leftStick.ReadValue();
                if (s.sqrMagnitude > 0.04f) m = s;
            }
            return m;
        }

        static void AdvanceButton(int index, bool kbHeld, bool gpHeld, float dt)
        {
            bool touchHeld = false, touchDown = false, touchUp = false;
            var tb = touchButtons[index];
            if (tb != null)
            {
                touchHeld = tb.IsHeld;
                touchDown = tb.ConsumeDown();
                touchUp = tb.ConsumeUp();
            }
            buttons[index].Advance(kbHeld || gpHeld || touchHeld, touchDown, touchUp, dt);
        }

        /// <summary>カメラの向きに合わせてスティック入力をワールドXZ平面の方向へ変換する。</summary>
        public static Vector3 MoveWorld(Transform cam)
        {
            if (cam == null) return new Vector3(Move.x, 0f, Move.y);
            Vector3 f = cam.forward;
            f.y = 0f;
            if (f.sqrMagnitude < 1e-4f) f = Vector3.forward;
            f.Normalize();
            Vector3 r = new Vector3(f.z, 0f, -f.x);
            return r * Move.x + f * Move.y;
        }
    }
}
