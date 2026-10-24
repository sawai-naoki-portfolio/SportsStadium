using UnityEngine;

namespace SportsGame
{
    /// <summary>ゲームロジックより先に毎フレーム入力を更新する。</summary>
    [DefaultExecutionOrder(-500)]
    public class GameInputDriver : MonoBehaviour
    {
        void Update() => GameInput.Tick(Time.unscaledDeltaTime);
    }
}
