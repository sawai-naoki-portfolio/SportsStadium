using NUnit.Framework;
using UnityEngine;

namespace SportsGame.Tests
{
    /// <summary>「成功」と判定したシュートが、リム・バックボードの物理を通しても実際に入ることを確認する。</summary>
    public class BasketballShotPhysicsTests
    {
        const float JumperReleaseHeight = 2.7f;
        const float LayupReleaseHeight = 2.9f;

        static bool Scores(Vector3 release, float angle)
        {
            var sim = new BallSim { radius = BasketballDims.BallRadius, restitution = 0.8f, friction = 0.9f };
            sim.colliders.AddRange(BasketballDims.CreateColliders());
            if (!BallMath.SolveByAngle(release, BasketballDims.Hoop, angle, sim.gravity, out var v)) return false;
            sim.Launch(release, v);
            for (int i = 0; i < 300; i++)
            {
                float prevY = sim.pos.y;
                sim.Step(1f / 60f);
                if (BasketballDims.PassedThroughHoop(prevY, sim.pos, sim.vel)) return true;
            }
            return false;
        }

        static Vector3 ReleaseFrom(float distance, float bearing, float height)
        {
            Vector3 dir = Quaternion.Euler(0f, bearing, 0f) * Vector3.back;
            Vector3 feet = new Vector3(BasketballDims.Hoop.x, 0f, BasketballDims.Hoop.z) + dir * distance;
            return feet + Vector3.up * height - dir * 0.2f;
        }

        [Test]
        public void CenteredJumpersScore([Values(2.5f, 4f, 5.5f, 7f, 8.5f)] float distance, [Values(-75f, -40f, 0f, 40f, 75f)] float bearing)
        {
            var release = ReleaseFrom(distance, bearing, JumperReleaseHeight);
            Assert.IsTrue(Scores(release, BasketballShotModel.LaunchAngle(ShotKind.Jumper, distance)), $"d={distance} bearing={bearing}");
        }

        [Test]
        public void CenteredLayupsScore([Values(-60f, 0f, 60f)] float bearing)
        {
            var release = ReleaseFrom(0.8f, bearing, LayupReleaseHeight);
            Assert.IsTrue(Scores(release, BasketballShotModel.LaunchAngle(ShotKind.Layup, 0.8f)), $"bearing={bearing}");
        }

        [Test]
        public void BlockChanceFallsWithDistance()
        {
            Assert.Greater(BasketballShotModel.BlockChance(0.1f, 0.55f, ShotKind.Jumper), BasketballShotModel.BlockChance(0.5f, 0.55f, ShotKind.Jumper));
            Assert.Less(BasketballShotModel.BlockChance(0.1f, 0.55f, ShotKind.Dunk), BasketballShotModel.BlockChance(0.1f, 0.55f, ShotKind.Layup));
        }
    }
}
