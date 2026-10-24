using NUnit.Framework;
using UnityEngine;

namespace SportsGame.Tests
{
    public class BasketballTests
    {
        [Test]
        public void TargetScoreEndsGame()
        {
            var s = new BasketballScore(11, 0f);
            Assert.IsFalse(s.Add(0, 9));
            Assert.IsFalse(s.Add(1, 3));
            Assert.IsTrue(s.Add(0, 2));
            Assert.AreEqual(0, s.Winner);
            Assert.IsFalse(s.Add(1, 3), "決着後の得点は無効");
        }

        [Test]
        public void TimeUpWithLeadEndsGame()
        {
            var s = new BasketballScore(0, 10f);
            s.Add(1, 2);
            Assert.IsFalse(s.Tick(9f));
            Assert.IsTrue(s.Tick(2f));
            Assert.AreEqual(1, s.Winner);
        }

        [Test]
        public void TimeUpWhenTiedGoesToSuddenDeath()
        {
            var s = new BasketballScore(0, 5f);
            Assert.IsFalse(s.Tick(6f));
            Assert.IsTrue(s.SuddenDeath);
            Assert.IsTrue(s.Add(0, 2));
            Assert.AreEqual(0, s.Winner);
        }

        [Test]
        public void ThreePointLine()
        {
            Assert.IsTrue(BasketballDims.IsThree(new Vector3(0f, 0f, -7f)));
            Assert.IsFalse(BasketballDims.IsThree(new Vector3(0f, 0f, -6.5f)));
            Assert.IsTrue(BasketballDims.IsThree(new Vector3(6.8f, 0f, 0.5f)), "コーナー");
            Assert.IsFalse(BasketballDims.IsThree(new Vector3(6.4f, 0f, 0.5f)));
            Assert.AreEqual(2, BasketballShotModel.PointsFor(ShotKind.Layup, new Vector3(0f, 0f, -8f)), "レイアップは常に2点");
        }

        [Test]
        public void MakeChanceRewardsTimingAndPunishesContest()
        {
            float open = BasketballShotModel.MakeChance(ShotKind.Jumper, 5f, 1f, false, 0f, 1f);
            float bad = BasketballShotModel.MakeChance(ShotKind.Jumper, 5f, 0f, false, 0f, 1f);
            float contested = BasketballShotModel.MakeChance(ShotKind.Jumper, 5f, 1f, false, 1f, 1f);
            float perfect = BasketballShotModel.MakeChance(ShotKind.Jumper, 5f, 1f, true, 0f, 1f);
            Assert.Greater(open, bad);
            Assert.Greater(open, contested);
            Assert.GreaterOrEqual(perfect, 0.92f);
            Assert.That(BasketballShotModel.MakeChance(ShotKind.Jumper, 30f, 0f, false, 1f, 0.1f), Is.GreaterThanOrEqualTo(0.03f));
        }

        [Test]
        public void TimingQualityWindow()
        {
            Assert.AreEqual(1f, BasketballShotModel.TimingQuality(1.05f, 0.1f, out bool perfect));
            Assert.IsTrue(perfect);
            Assert.Less(BasketballShotModel.TimingQuality(1.4f, 0.1f, out perfect), 0.5f);
            Assert.IsFalse(perfect);
        }

        [Test]
        public void ContestIsStrongerInFront()
        {
            Vector3 shooter = new Vector3(0f, 0f, -5f);
            float front = BasketballShotModel.Contest(shooter, shooter + Vector3.forward, true);
            float behind = BasketballShotModel.Contest(shooter, shooter + Vector3.back, true);
            Assert.Greater(front, behind);
        }
    }
}
