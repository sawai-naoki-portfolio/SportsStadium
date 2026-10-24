using UnityEngine;

namespace SportsGame
{
    /// <summary>人型キャラクターの関節 Transform 群。ポーズを関節へ書き込む。</summary>
    public class HumanoidRig
    {
        public const float LegLength = 0.92f;

        public Transform body, hips, spine, head;
        public Transform shL, elL, handL, shR, elR, handR;
        public Transform hipL, knL, footL, hipR, knR, footR;

        public void Apply(in HumanoidPose p)
        {
            hips.localPosition = new Vector3(0f, LegLength + p.hipsY, 0f);
            spine.localRotation = Quaternion.Euler(p.spine);
            head.localRotation = Quaternion.Euler(p.head);
            shL.localRotation = Quaternion.Euler(p.shL);
            elL.localRotation = Quaternion.Euler(p.elL);
            shR.localRotation = Quaternion.Euler(p.shR);
            elR.localRotation = Quaternion.Euler(p.elR);
            hipL.localRotation = Quaternion.Euler(p.hipL);
            knL.localRotation = Quaternion.Euler(p.knL);
            hipR.localRotation = Quaternion.Euler(p.hipR);
            knR.localRotation = Quaternion.Euler(p.knR);
            // 足裏を地面と平行に保つ
            footL.localRotation = Quaternion.Euler(-(p.hipL.x + p.knL.x) * 0.8f, 0f, 0f);
            footR.localRotation = Quaternion.Euler(-(p.hipR.x + p.knR.x) * 0.8f, 0f, 0f);
        }
    }
}
