using NUnit.Framework;

namespace SportsGame.Tests
{
    public class TennisScoreTests
    {
        static TennisScoreEvent Award(TennisScore s, int player, int times)
        {
            var e = TennisScoreEvent.None;
            for (int i = 0; i < times; i++) e = s.AwardPoint(player);
            return e;
        }

        [Test]
        public void FourStraightPointsWinGame()
        {
            var s = new TennisScore(3, 1, 0);
            Assert.AreEqual(TennisScoreEvent.None, Award(s, 0, 3));
            Assert.AreEqual("40", s.PointLabel(0));
            Assert.AreEqual(TennisScoreEvent.Game, s.AwardPoint(0));
            Assert.AreEqual(1, s.Games(0));
            Assert.AreEqual("0", s.PointLabel(0));
        }

        [Test]
        public void DeuceAndAdvantage()
        {
            var s = new TennisScore(3, 1, 0);
            Award(s, 0, 3);
            Award(s, 1, 3);
            Assert.IsTrue(s.IsDeuce);
            s.AwardPoint(1);
            Assert.AreEqual(1, s.Advantage);
            Assert.AreEqual("AD", s.PointLabel(1));
            Assert.AreEqual("40", s.PointLabel(0));
            s.AwardPoint(0);
            Assert.IsTrue(s.IsDeuce);
            Award(s, 0, 1);
            Assert.AreEqual(TennisScoreEvent.Game, s.AwardPoint(0));
            Assert.AreEqual(1, s.Games(0));
        }

        [Test]
        public void ServerAlternatesEachGame()
        {
            var s = new TennisScore(6, 1, 1);
            Assert.AreEqual(1, s.CurrentServer);
            Award(s, 0, 4);
            Assert.AreEqual(0, s.CurrentServer);
        }

        [Test]
        public void ServeSideAlternatesEachPoint()
        {
            var s = new TennisScore(3, 1, 0);
            Assert.IsTrue(s.DeuceSide);
            s.AwardPoint(0);
            Assert.IsFalse(s.DeuceSide);
        }

        [Test]
        public void SetNeedsTwoGameLead()
        {
            var s = new TennisScore(3, 1, 0);
            Award(s, 0, 8);  // 2-0
            Award(s, 1, 8);  // 2-2
            Award(s, 0, 4);  // 3-2 (2ゲーム差が必要)
            Assert.AreEqual(-1, s.Winner);
            Assert.AreEqual(TennisScoreEvent.Match, Award(s, 0, 4)); // 4-2
            Assert.AreEqual(0, s.Winner);
            Assert.AreEqual("4-2", s.Summary());
        }

        [Test]
        public void TiebreakStartsAtGamesAllAndRotatesServe()
        {
            var s = new TennisScore(3, 1, 0);
            Award(s, 0, 8);
            Award(s, 1, 8);
            Award(s, 0, 4);
            Assert.AreEqual(TennisScoreEvent.TiebreakStart, Award(s, 1, 4)); // 3-3
            Assert.IsTrue(s.Tiebreak);
            int first = s.CurrentServer;
            s.AwardPoint(0);
            Assert.AreEqual(1 - first, s.CurrentServer, "1ポイント目の後に交代");
            s.AwardPoint(0);
            Assert.AreEqual(1 - first, s.CurrentServer, "その後は2ポイントごとに交代");
            s.AwardPoint(0);
            Assert.AreEqual(first, s.CurrentServer);
            Assert.AreEqual("3", s.PointLabel(0));
        }

        [Test]
        public void TiebreakNeedsSevenWithTwoPointLead()
        {
            var s = new TennisScore(1, 1, 0);
            Award(s, 0, 4);
            Assert.AreEqual(TennisScoreEvent.TiebreakStart, Award(s, 1, 4)); // 1-1
            Award(s, 0, 6);
            Award(s, 1, 6);
            Assert.AreEqual(TennisScoreEvent.None, s.AwardPoint(0)); // 7-6
            s.AwardPoint(1);                                           // 7-7
            s.AwardPoint(1);                                           // 7-8
            Assert.AreEqual(TennisScoreEvent.Match, s.AwardPoint(1));  // 7-9
            Assert.AreEqual(1, s.Winner);
        }

        [Test]
        public void BestOfThreeSets()
        {
            var s = new TennisScore(1, 2, 0);
            Assert.AreEqual(TennisScoreEvent.Set, Award(s, 0, 8));   // 2-0 (2ゲーム差)
            Assert.AreEqual(TennisScoreEvent.Set, Award(s, 1, 8));
            Assert.AreEqual(TennisScoreEvent.Match, Award(s, 0, 8));
            Assert.AreEqual(2, s.Sets(0));
            Assert.AreEqual(3, s.SetHistory.Count);
        }

        [Test]
        public void PointsAfterMatchAreIgnored()
        {
            var s = new TennisScore(1, 1, 0);
            Award(s, 0, 8);
            Assert.AreEqual(0, s.Winner);
            Assert.AreEqual(TennisScoreEvent.None, s.AwardPoint(1));
            Assert.AreEqual(TennisScoreEvent.None, s.AwardPoint(5));
        }
    }
}
