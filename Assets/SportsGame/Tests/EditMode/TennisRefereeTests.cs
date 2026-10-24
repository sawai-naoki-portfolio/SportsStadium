using NUnit.Framework;
using UnityEngine;

namespace SportsGame.Tests
{
    public class TennisRefereeTests
    {
        // 手前(side 0)の選手が x>0 からサーブ → 奥のコートの x<0 側ボックスへ
        static TennisReferee ServingReferee()
        {
            var r = new TennisReferee();
            r.BeginPoint();
            r.BeginServe(0, -1f);
            return r;
        }

        static readonly Vector3 InBox = new Vector3(-2f, 0f, 4f);
        static readonly Vector3 WrongBox = new Vector3(2f, 0f, 4f);

        [Test]
        public void ServeIntoCorrectBoxIsGood()
        {
            var r = ServingReferee();
            Assert.AreEqual(RallyCall.ServeGood, r.RegisterBounce(InBox).call);
            Assert.IsFalse(r.IsServe);
            Assert.IsTrue(r.CanHit(1));
            Assert.IsFalse(r.CanHit(0));
        }

        [Test]
        public void ReceiverCannotHitServeBeforeBounce()
        {
            var r = ServingReferee();
            Assert.IsFalse(r.CanHit(1));
        }

        [Test]
        public void TwoFaultsGiveDoubleFault()
        {
            var r = ServingReferee();
            Assert.AreEqual(RallyCall.Fault, r.RegisterBounce(WrongBox).call);
            Assert.IsTrue(r.IsSecondServe);
            r.BeginServe(0, -1f);
            var d = r.RegisterBounce(new Vector3(-2f, 0f, 8f));
            Assert.AreEqual(RallyCall.Point, d.call);
            Assert.AreEqual(PointReason.DoubleFault, d.reason);
            Assert.AreEqual(1, d.winner);
        }

        [Test]
        public void NetCordServeThatLandsInIsLet()
        {
            var r = ServingReferee();
            r.RegisterNetTouch();
            Assert.AreEqual(RallyCall.Let, r.RegisterBounce(InBox).call);
            Assert.AreEqual(0, r.Faults, "レットはフォルトに数えない");
        }

        [Test]
        public void UntouchedServeIsAce()
        {
            var r = ServingReferee();
            r.RegisterBounce(InBox);
            var d = r.RegisterBounce(new Vector3(-2f, 0f, 9f));
            Assert.AreEqual(PointReason.Ace, d.reason);
            Assert.AreEqual(0, d.winner);
        }

        [Test]
        public void RallyOutNetAndWinner()
        {
            var r = ServingReferee();
            r.RegisterBounce(InBox);
            r.RegisterHit(1);
            // 奥の選手の打球がサイドラインの外
            var outBall = r.RegisterBounce(new Vector3(5f, 0f, -6f));
            Assert.AreEqual(PointReason.Out, outBall.reason);
            Assert.AreEqual(0, outBall.winner);

            r = ServingReferee();
            r.RegisterBounce(InBox);
            r.RegisterHit(1);
            // 自分のコート側に落ちた = ネット
            var net = r.RegisterBounce(new Vector3(0f, 0f, 1f));
            Assert.AreEqual(PointReason.Net, net.reason);
            Assert.AreEqual(0, net.winner);

            r = ServingReferee();
            r.RegisterBounce(InBox);
            r.RegisterHit(1);
            Assert.AreEqual(RallyCall.None, r.RegisterBounce(new Vector3(1f, 0f, -8f)).call);
            var winner = r.RegisterBounce(new Vector3(1f, 0f, -12f));
            Assert.AreEqual(PointReason.Winner, winner.reason);
            Assert.AreEqual(1, winner.winner);
        }

        [Test]
        public void LinesAreIn()
        {
            Assert.IsTrue(TennisDims.InSingles(new Vector3(TennisDims.SinglesHalfWidth, 0f, -TennisDims.HalfLength), 0));
            Assert.IsFalse(TennisDims.InSingles(new Vector3(TennisDims.SinglesHalfWidth + 0.1f, 0f, -5f), 0));
            Assert.IsTrue(TennisDims.InServiceBox(new Vector3(-0.01f, 0f, TennisDims.ServiceLine), 1, -1f));
        }
    }
}
