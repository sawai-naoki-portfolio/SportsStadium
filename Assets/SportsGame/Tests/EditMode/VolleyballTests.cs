using NUnit.Framework;
using UnityEngine;

namespace SportsGame.Tests
{
    public class VolleyballTests
    {
        static VolleyballScoreEvent Award(VolleyballScore s, int team, int times)
        {
            var e = VolleyballScoreEvent.None;
            for (int i = 0; i < times; i++) e = s.AwardRally(team);
            return e;
        }

        [Test]
        public void SetNeedsTwoPointLead()
        {
            var s = new VolleyballScore(15, 1, 15, 0);
            Award(s, 0, 14);
            Award(s, 1, 14);
            Assert.AreEqual(VolleyballScoreEvent.None, s.AwardRally(0)); // 15-14
            s.AwardRally(1);                                              // 15-15
            s.AwardRally(0);                                              // 16-15
            Assert.AreEqual(VolleyballScoreEvent.Match, s.AwardRally(0)); // 17-15
            Assert.AreEqual(0, s.Winner);
            Assert.AreEqual("17-15", s.Summary());
        }

        [Test]
        public void SideOutAlternatesServer()
        {
            var s = new VolleyballScore(21, 1, 15, 0);
            Assert.AreEqual(0, s.ServingTeam);
            Assert.AreEqual(0, s.ServerSlot);
            s.AwardRally(0);
            Assert.AreEqual(0, s.ServerSlot, "得点したチームは同じ選手がサーブを続ける");
            s.AwardRally(1);
            Assert.AreEqual(1, s.ServingTeam);
            int first = s.ServerSlot;
            s.AwardRally(0);
            Assert.AreEqual(1, s.ServerSlot, "サーブ権を取り返したら前回と別の選手");
            s.AwardRally(1);
            Assert.AreEqual(1 - first, s.ServerSlot);
        }

        [Test]
        public void FinalSetUsesFinalSetPoints()
        {
            var s = new VolleyballScore(3, 2, 2, 0);
            Assert.AreEqual(VolleyballScoreEvent.Set, Award(s, 0, 3));
            Assert.AreEqual(VolleyballScoreEvent.Set, Award(s, 1, 3));
            Assert.AreEqual(2, s.Target);
            Assert.AreEqual(VolleyballScoreEvent.Match, Award(s, 1, 2));
            Assert.AreEqual(1, s.Winner);
        }

        [Test]
        public void GameAndMatchPoint()
        {
            var s = new VolleyballScore(15, 1, 15, 0);
            Award(s, 0, 14);
            Assert.AreEqual(0, s.GamePointTeam);
            Assert.IsTrue(s.IsMatchPoint);
        }

        [Test]
        public void FourthTouchIsFault()
        {
            var r = new VolleyballReferee();
            r.BeginServe(1, 2);
            Assert.IsFalse(r.RegisterTouch(0, 0, false).point);
            Assert.IsFalse(r.RegisterTouch(0, 1, false).point);
            Assert.IsFalse(r.RegisterTouch(0, 0, false).point);
            var d = r.RegisterTouch(0, 1, false);
            Assert.IsTrue(d.point);
            Assert.AreEqual(RallyEnd.FourHits, d.reason);
            Assert.AreEqual(1, d.winner);
        }

        [Test]
        public void SamePlayerTwiceIsDoubleTouchUnlessAfterBlock()
        {
            var r = new VolleyballReferee();
            r.BeginServe(1, 2);
            r.RegisterTouch(0, 0, false);
            Assert.AreEqual(RallyEnd.DoubleTouch, r.RegisterTouch(0, 0, false).reason);

            r = new VolleyballReferee();
            r.BeginServe(1, 2);
            r.RegisterTouch(0, 0, true);
            Assert.IsFalse(r.RegisterTouch(0, 0, false).point, "ブロック直後は同じ選手が触れる");
        }

        [Test]
        public void TouchIndexResetsWhenBallChangesSide()
        {
            var r = new VolleyballReferee();
            r.BeginServe(1, 2);
            Assert.AreEqual(0, r.TouchIndexFor(0));
            r.RegisterTouch(0, 0, false);
            Assert.AreEqual(1, r.TouchIndexFor(0));
            Assert.AreEqual(0, r.TouchIndexFor(1));
        }

        [Test]
        public void GroundAndNetJudgement()
        {
            var r = new VolleyballReferee();
            r.BeginServe(0, 0);
            var ace = r.RegisterGround(new Vector3(1f, 0f, 5f));
            Assert.AreEqual(RallyEnd.Ace, ace.reason);
            Assert.AreEqual(0, ace.winner);

            r = new VolleyballReferee();
            r.BeginServe(0, 0);
            var outBall = r.RegisterGround(new Vector3(5f, 0f, 5f));
            Assert.AreEqual(RallyEnd.Out, outBall.reason);
            Assert.AreEqual(1, outBall.winner);

            r = new VolleyballReferee();
            r.BeginServe(0, 0);
            Assert.AreEqual(RallyEnd.Antenna, r.RegisterNetCrossing(new Vector3(4.5f, 3f, 0f)).reason);
            Assert.AreEqual(RallyEnd.UnderNet, r.RegisterNetCrossing(new Vector3(0f, 1f, 0f)).reason);
            Assert.IsFalse(r.RegisterNetCrossing(new Vector3(0f, 3f, 0f)).point);
        }

        [Test]
        public void KillBlockIsCreditedToBlockers()
        {
            var r = new VolleyballReferee();
            r.BeginServe(0, 0);
            r.RegisterTouch(1, 2, false);
            r.RegisterTouch(1, 3, false);
            r.RegisterTouch(1, 2, false);   // スパイク
            r.RegisterTouch(0, 1, true);    // ブロック
            var d = r.RegisterGround(new Vector3(0f, 0f, 2f));
            Assert.AreEqual(RallyEnd.Blocked, d.reason);
            Assert.AreEqual(0, d.winner);
        }
    }
}
