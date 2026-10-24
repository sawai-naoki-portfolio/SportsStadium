using NUnit.Framework;
using UnityEngine;

namespace SportsGame.Tests
{
    public class BallPhysicsTests
    {
        const float G = 9.81f;

        /// <summary>解いた初速度でシミュレーションすると目標を通過する(決定論的な積分の確認)。</summary>
        static float MissDistance(Vector3 from, Vector3 to, Vector3 v, float flightTime)
        {
            var sim = new BallSim { radius = 0.01f, gravity = G };
            sim.Launch(from, v);
            float dt = 1f / 240f;
            float best = float.MaxValue;
            for (float t = 0f; t < flightTime + 0.2f; t += dt)
            {
                sim.Step(dt);
                best = Mathf.Min(best, Vector3.Distance(sim.pos, to));
            }
            return best;
        }

        [Test]
        public void SolveByTimeHitsTarget()
        {
            Vector3 from = new Vector3(0f, 1f, -10f), to = new Vector3(2f, 1.5f, 8f);
            Vector3 v = BallMath.SolveByTime(from, to, 1.2f, G);
            Assert.Less(MissDistance(from, to, v, 1.2f), 0.05f);
        }

        [Test]
        public void SolveByAngleHitsTarget()
        {
            Vector3 from = new Vector3(0f, 2.4f, -6f), to = BasketballDims.Hoop;
            Assert.IsTrue(BallMath.SolveByAngle(from, to, 52f, G, out var v));
            Assert.Less(MissDistance(from, to, v, 2f), 0.05f);
        }

        [Test]
        public void SolveByApexReachesApex()
        {
            Vector3 from = new Vector3(0f, 0.8f, -5f), to = new Vector3(1f, 2f, -1f);
            Vector3 v = BallMath.SolveByApex(from, to, 4.5f, G, out float t);
            Assert.AreEqual(4.5f, from.y + v.y * v.y / (2f * G), 0.01f);
            Assert.Less(MissDistance(from, to, v, t), 0.05f);
        }

        [Test]
        public void ImpossibleAngleReturnsFalse()
        {
            Assert.IsFalse(BallMath.SolveByAngle(Vector3.zero, new Vector3(1f, 10f, 0f), 30f, G, out _));
        }

        [Test]
        public void BounceKeepsBallAboveGround()
        {
            var sim = new BallSim { radius = 0.1f };
            int bounces = 0;
            sim.onContact = c => { if (c.kind == ContactKind.Ground) bounces++; };
            sim.Launch(new Vector3(0f, 2f, 0f), Vector3.zero);
            for (int i = 0; i < 600; i++)
            {
                sim.Step(1f / 60f);
                Assert.GreaterOrEqual(sim.pos.y, sim.radius - 1e-4f);
            }
            Assert.Greater(bounces, 1);
        }

        [Test]
        public void InvalidLaunchIsIgnored()
        {
            var sim = new BallSim();
            sim.Hold(Vector3.one);
            sim.Launch(Vector3.zero, new Vector3(float.NaN, 0f, 0f));
            Assert.IsTrue(sim.frozen);
            Assert.AreEqual(Vector3.one, sim.pos);
        }

        [Test]
        public void NetStopsLowBall()
        {
            var net = TennisDims.CreateNet();
            var sim = new BallSim { radius = TennisDims.BallRadius };
            sim.colliders.Add(net);
            sim.Launch(new Vector3(0f, 0.5f, -3f), new Vector3(0f, 0f, 20f), 0f);
            for (int i = 0; i < 60; i++) sim.Step(1f / 60f);
            Assert.Less(sim.pos.z, 0.05f, "ネットより低いボールは相手コートへ抜けない");
        }
    }
}
