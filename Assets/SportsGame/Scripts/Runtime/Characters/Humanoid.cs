using UnityEngine;

namespace SportsGame
{
    /// <summary>
    /// 人型キャラクターの手続き的アニメーション。制御側は移動速度などの状態を毎フレーム設定し、
    /// 動作は Play() で再生する。ルートの足元が原点、+Z が正面。
    /// </summary>
    public class Humanoid : MonoBehaviour
    {
        const float PoseSharpness = 22f;

        HumanoidRig rig;
        HumanoidPose current;
        bool hasPose;
        HAction action;
        float actionTime, actionDuration;
        bool actionLoop;
        float runPhase;

        // ---- 制御側が設定する状態 ----
        public float moveSpeed;
        public float crouch;
        public bool airborne;
        public bool dribbling;
        public bool dribbleLeft;
        public float dribblePhase;
        public bool holdBall;
        public bool readyRacket;

        public float Height { get; private set; }

        public void Init(HumanoidRig r, float height)
        {
            rig = r;
            Height = height;
        }

        /// <summary>動作を再生する。duration 秒で進行度が 0→1 になる。</summary>
        public void Play(HAction a, float duration, bool loop = false)
        {
            action = a;
            actionTime = 0f;
            actionDuration = Mathf.Max(0.05f, duration);
            actionLoop = loop;
        }

        /// <summary>水平方向へ向きを滑らかに変える。</summary>
        public void FaceTowards(Vector3 dir, float dt, float sharpness = 12f)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-4f) return;
            Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 1f - Mathf.Exp(-sharpness * dt));
        }

        public void FaceInstant(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        void LateUpdate()
        {
            if (rig == null) return;
            float dt = Time.deltaTime;
            AdvanceRunPhase(dt);
            var target = HumanoidPoses.Locomotion(new LocomotionState
            {
                speed = moveSpeed, crouch = crouch, runPhase = runPhase, dribblePhase = dribblePhase,
                airborne = airborne, dribbling = dribbling, dribbleLeft = dribbleLeft, holdBall = holdBall, readyRacket = readyRacket
            });
            if (action != HAction.None) target = BlendAction(target, dt);
            current = hasPose ? HumanoidPose.Lerp(current, target, 1f - Mathf.Exp(-PoseSharpness * dt)) : target;
            hasPose = true;
            rig.Apply(current);
        }

        void AdvanceRunPhase(float dt)
        {
            if (moveSpeed > 0.15f)
            {
                float run = Mathf.Clamp01(moveSpeed / HumanoidPoses.RunReferenceSpeed);
                runPhase += Mathf.Lerp(1.3f, 2.6f, run) * Mathf.PI * 2f * dt;
            }
            else runPhase = Mathf.MoveTowards(runPhase, Mathf.Round(runPhase / Mathf.PI) * Mathf.PI, dt * 6f);
        }

        HumanoidPose BlendAction(HumanoidPose basePose, float dt)
        {
            actionTime += dt;
            if (actionTime >= actionDuration)
            {
                if (!actionLoop)
                {
                    action = HAction.None;
                    return basePose;
                }
                actionTime %= actionDuration;
            }
            float t = Mathf.Clamp01(actionTime / actionDuration);
            var acted = basePose;
            HumanoidPoses.Action(action, t, dribbleLeft, ref acted);
            float weight = actionLoop ? 1f : Mathf.Clamp01(Mathf.Min(t / 0.08f, (1f - t) / 0.15f));
            return HumanoidPose.Lerp(basePose, acted, weight);
        }
    }
}
