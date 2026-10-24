using NUnit.Framework;
using UnityEngine;

namespace SportsGame.Tests
{
    public class InputAndUITests
    {
        [Test]
        public void ButtonStateEdges()
        {
            var s = new ButtonState();
            s.Advance(true, false, false, 0.1f);
            Assert.IsTrue(s.Down);
            Assert.IsTrue(s.Held);
            s.Advance(true, false, false, 0.1f);
            Assert.IsFalse(s.Down);
            Assert.AreEqual(0.2f, s.HeldTime, 1e-5f);
            s.Advance(false, false, false, 0.1f);
            Assert.IsTrue(s.Up);
            Assert.IsFalse(s.Held);
        }

        [Test]
        public void TapWithinOneFrameReportsDownAndUp()
        {
            var s = new ButtonState();
            s.Advance(false, true, true, 0.016f);
            Assert.IsTrue(s.Down);
            Assert.IsTrue(s.Up);
            Assert.IsFalse(s.Held);
        }

        [Test]
        public void SafeAreaFallsBackToFullScreenWhenInvalid()
        {
            Assert.AreEqual(new Rect(0f, 0f, 1920f, 1080f), SafeArea.Sanitize(new Rect(90f, -120f, 2600f, 1080f), 1920f, 1080f));
            var notch = new Rect(88f, 0f, 1744f, 1080f);
            Assert.AreEqual(notch, SafeArea.Sanitize(notch, 1920f, 1080f));
        }

        [Test]
        public void DifficultySkillRange()
        {
            Assert.AreEqual(0f, Difficulty.Easy.Skill());
            Assert.AreEqual(0.5f, Difficulty.Normal.Skill());
            Assert.AreEqual(1f, Difficulty.Hard.Skill());
        }

        [Test]
        public void EverySportHasThreeMatchLengths()
        {
            foreach (var s in SportCatalog.All)
                Assert.AreEqual(SportCatalog.MatchLengthCount, SportCatalog.MatchLengthLabels(s).Length);
            Assert.AreEqual(SportCatalog.MatchLengthLabels(Sport.Tennis)[2], SportCatalog.MatchLengthLabel(Sport.Tennis, 99), "範囲外は丸める");
        }

        [Test]
        public void TeamColorsStayDistinctFromAway()
        {
            Assert.AreEqual(TeamColors.Count, TeamColors.Names.Length);
            Color.RGBToHSV(UITheme.Away, out float awayHue, out _, out _);
            for (int i = 0; i < TeamColors.Count; i++)
            {
                Color.RGBToHSV(TeamColors.Get(i), out float hue, out _, out _);
                float diff = Mathf.Abs(Mathf.DeltaAngle(hue * 360f, awayHue * 360f));
                Assert.Greater(diff, 35f, TeamColors.Names[i] + " は CPU の赤と見分けにくい");
            }
            Assert.AreEqual(TeamColors.Get(0), TeamColors.Get(99), "範囲外は先頭の色");
        }
    }
}
