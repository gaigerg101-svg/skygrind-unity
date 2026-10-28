// NIGHT74 builder B (slate B2, design.n74.target74; Game/Target74.cs, Game/UiTarget74.cs, hooked in Game/UiBossBar43.cs). Pinned:
//   1. the row: in the config's design, read through NightRows.ViewOn both ways, a FrameRows row
//   2. the chevron's place: the point where the ray from the centre along the bearing leaves the inset rectangle (right, up, a diagonal, the zero bearing points left), and its angle
//   3. the tag: "OPEN 2.9" + " - " + the word; at most six words with every direction word; the bar's face is at least 22 px at 1080p with hud47 (Hud47BossLabelPx)
//   4. the direction word (Weak74Math.Direction, tested in N74BWeakTests) is the same one the reel prints
//   5. no planted turn without -skygrindTarget74Away
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using T = Skygrind.Game.Target74;
using TM = Skygrind.Game.Target74Math;

namespace Skygrind.Game.Tests
{
    public class N74BTargetTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        [SetUp]
        public void SetUp() { NightRows.ReelOverride.Clear(); NightRows.ForceOffFrom = NightRows.NoOff; }

        [TearDown]
        public void TearDown() { NightRows.ReelOverride.Clear(); NightRows.ForceOffFrom = NightRows.NoOff; }

        [Test]
        public void Row_InTheDesign_ReadBothWays_AFrameRow()
        {
            GameConfig c = Cfg();
            Assert.That(c.Has("design.n74.target74.enabled"), Is.True, "the shared commit placed the row");
            NightRows.ReelOverride[T.Row] = true;
            Assert.That(T.Decide(c), Is.True);
            NightRows.ReelOverride[T.Row] = false;
            Assert.That(T.Decide(c), Is.False);
            Assert.That(T.Decide(null), Is.False);
            Assert.That(SameRun63Math.KindOf(T.Row), Is.EqualTo(SameRun63Math.Kind.Frame));
        }

        [Test]
        public void Edge_TheRayFromTheCentreLeavesTheInsetRectangle()
        {
            TM.Edge(1, 0, 960, 540, 96, out double ex, out double ey, out double deg);
            Assert.That(ex, Is.EqualTo(864).Within(1e-9));
            Assert.That(ey, Is.EqualTo(0).Within(1e-9));
            Assert.That(deg, Is.EqualTo(0).Within(1e-9));
            TM.Edge(0, 1, 960, 540, 96, out ex, out ey, out deg);
            Assert.That(ex, Is.EqualTo(0).Within(1e-9));
            Assert.That(ey, Is.EqualTo(444).Within(1e-9), "up: the half height less the inset");
            Assert.That(deg, Is.EqualTo(90).Within(1e-9));
            TM.Edge(1, 1, 960, 540, 96, out ex, out ey, out deg);
            Assert.That(ex, Is.EqualTo(444).Within(1e-9), "a diagonal leaves by the nearer side (the top, 444)");
            Assert.That(ey, Is.EqualTo(444).Within(1e-9));
            Assert.That(deg, Is.EqualTo(45).Within(1e-9));
            TM.Edge(-3, -1, 960, 540, 96, out ex, out ey, out deg);
            Assert.That(ex, Is.EqualTo(-864).Within(1e-9), "a shallow bearing to the lower left leaves by the left side");
            Assert.That(ey, Is.EqualTo(-288).Within(1e-9));
            Assert.That(Math.Abs(deg), Is.EqualTo(180 - Math.Atan(1.0 / 3) * 180 / Math.PI).Within(1e-9));
            TM.Edge(0, 0, 960, 540, 96, out ex, out ey, out deg);
            Assert.That(ex, Is.EqualTo(-864).Within(1e-9), "no bearing (dead behind you): left");
            Assert.That(deg, Is.EqualTo(180).Within(1e-9));
            Assert.That(ex, Is.GreaterThanOrEqualTo(-864 - 1e-9));
        }

        [Test]
        public void Tag_OpenWithTheWay_AtMostSixWords_AtLeast22Px()
        {
            foreach (string dir in new[] { "ABOVE", "BELOW", "LEFT", "RIGHT", "BEHIND" })
            {
                string t = TM.Tag("OPEN 2.9", dir, UnityTuning.N74TargetSeparator);
                Assert.That(t, Is.EqualTo("OPEN 2.9 - " + dir));
                Assert.That(TM.Words(t), Is.LessThanOrEqualTo(6), t);
            }
            Assert.That(TM.Tag("OPEN", "ABOVE", UnityTuning.N74TargetSeparator), Is.EqualTo("OPEN - ABOVE"), "no seconds left: the word alone");
            Assert.That(TM.Tag("OPEN 2.9", null, UnityTuning.N74TargetSeparator), Is.EqualTo("OPEN 2.9"), "on screen: the tag unchanged");
            Assert.That(UnityTuning.N74TargetTextMinPx, Is.GreaterThanOrEqualTo(22f));
            Assert.That(UnityTuning.Hud47BossLabelPx, Is.GreaterThanOrEqualTo(UnityTuning.N74TargetTextMinPx), "the bar's tag face with hud47 on");
            Assert.That(UnityTuning.N74TargetPulseHz, Is.LessThan(3f), "WCAG 2.3.1");
        }

        [Test]
        public void Arrow_BoldEnoughToBeAnArrowNotACornerBracket()
        {
            // round 1's 40 px arms at 9 px and 42 degrees (84 open, no shaft) read as "a small cyan corner bracket" (the B2 spot)
            Assert.That(UnityTuning.N74TargetShaftPx, Is.GreaterThan(0f), "an arrow has a shaft");
            Assert.That(UnityTuning.N74TargetHalfAngleDeg * 2f, Is.LessThan(80f), "an acute head, not a right angle");
            Assert.That(UnityTuning.N74TargetThickPx, Is.GreaterThanOrEqualTo(14f), "thick enough to find across a busy frame");
            Assert.That(UnityTuning.N74TargetShaftPx + UnityTuning.N74TargetThickPx, Is.GreaterThanOrEqualTo(80f), "long");
            Assert.That(UnityTuning.N74TargetOutlinePx, Is.GreaterThan(0f), "outlined (it sits on pale walls and a dark ceiling alike)");
            // the whole arrow (shaft behind the tip, outline included) stays on the screen when its tip is at the inset
            Assert.That(UnityTuning.N74TargetInsetPx, Is.GreaterThanOrEqualTo(UnityTuning.N74TargetOutlinePx + 54f), "past the HUD's 54 px margin by the outline");
        }

        [Test]
        public void NoPlantedTurnWithoutItsArgument()
        {
            Assert.That(T.TurnAwayDeg(), Is.EqualTo(0.0), "the test process has no -skygrindTarget74Away");
        }
    }
}
