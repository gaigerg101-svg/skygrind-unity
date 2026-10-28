// NIGHT54 builder M (move54, a view row): the camera smoothness pieces that run without a scene (Game/Move54.cs).
//   Move54Step: a grounded 0.3 m snap down is drawn with no frame jumping more than the snap's 40 %, and the camera is back within 2 % of
//               the snap at 0.15 s; a jump (the step's end not grounded) and a landing (its start not grounded) hold nothing.
//   Move54.ImpactConfig / QualityConfig: row off, the game's own config object; row on, a view whose listed paths read the row's numbers,
//               the game's config untouched, the player's settings object shared; the off switch (-skygrindOff54) gives the game's
//               own config even with the row on. The off switch is pinned in the test (T-126: an earlier test in an Editor batch leaves
//               NightRows.ForceOffFrom at 46, e.g. Lesson25Tests, which turned every NIGHT46+ view row off: the n54 run's 3/4).
//   Move54.SlowEase: a freeze passes through (0), a slow-mo eases down over frames, its end returns at once.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N54MoveTests
    {
        static string Root => Path.GetFullPath("PortExport");

        static GameConfig Cfg(bool on)
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            var row = (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)cfg.root["design"])["n54"])["move54"];
            row["enabled"] = on;
            return cfg;
        }

        [Test]
        public void SnapDownIsSpreadAndConverges()
        {
            var s = new Move54Step(Cfg(true));
            const double dt = 1.0 / 60, snap = -0.3;
            double y = 0, cam = y + s.Frame(y, dt, true), maxJump = 0, t = 0, at015 = double.NaN;
            s.StepEnded(true, 0, 0);
            for (int f = 0; f < 30; f++)
            {
                if (f == 5) { s.StepEnded(true, y, y + snap); y += snap; }
                double c = y + s.Frame(y, dt, true);
                if (f >= 5) maxJump = System.Math.Max(maxJump, System.Math.Abs(c - cam));
                cam = c;
                if (f >= 5) t += dt;
                if (f >= 5 && double.IsNaN(at015) && t >= 0.15) at015 = System.Math.Abs(s.Offset);
            }
            Assert.That(maxJump, Is.LessThanOrEqualTo(0.4 * System.Math.Abs(snap)), "no frame jumps the whole snap");
            Assert.That(at015, Is.LessThanOrEqualTo(0.02 * System.Math.Abs(snap) + 1e-9), "back within 2 % at 0.15 s");
            Assert.That(s.StepsSeen, Is.EqualTo(1));
        }

        [Test]
        public void JumpAndLandingHoldNothing()
        {
            var s = new Move54Step(Cfg(true));
            const double dt = 1.0 / 60;
            double y = 0;
            s.Frame(y, dt, true);
            s.StepEnded(true, 0, 0);
            s.StepEnded(false, y, y + 0.2); // a jump's first step: the end is in the air
            y += 0.2;
            Assert.That(s.Frame(y, dt, true), Is.EqualTo(0), "a jump is never held");
            s.StepEnded(true, y, y - 0.2); // a landing: the start was in the air
            y -= 0.2;
            Assert.That(s.Frame(y, dt, true), Is.EqualTo(0), "a landing is never held");
            Assert.That(s.StepsSeen, Is.EqualTo(0));
        }

        [Test]
        public void ConfigViewsOffAndOn()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff; // T-126: whatever an earlier test left
                ConfigViews();
                NightRows.ForceOffFrom = 54; // -skygrindOff54: the row off whatever the config says
                GameConfig forced = Cfg(true);
                Assert.That(ReferenceEquals(Move54.ImpactConfig(forced), forced), Is.True, "the off switch: the game's own config");
                Assert.That(ReferenceEquals(Move54.QualityConfig(forced), forced), Is.True);
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        static void ConfigViews()
        {
            GameConfig off = Cfg(false);
            Assert.That(ReferenceEquals(Move54.ImpactConfig(off), off), Is.True, "row off: the game's own config");
            Assert.That(ReferenceEquals(Move54.QualityConfig(off), off), Is.True);
            GameConfig on = Cfg(true);
            double kill = on.Num("design.n42.impact42.killS"), step = on.Num("quality.dynamic.step");
            GameConfig iv = Move54.ImpactConfig(on), qv = Move54.QualityConfig(on);
            Assert.That(iv.Num("design.n42.impact42.killS"), Is.EqualTo(on.Num(Move54.Numbers + "impact.killS")));
            Assert.That(iv.Num("design.impact.freeze.parry"), Is.EqualTo(on.Num("design.impact.freeze.parry")), "a parry keeps its freeze");
            Assert.That(qv.Num("quality.dynamic.step"), Is.EqualTo(on.Num(Move54.Numbers + "quality.step")));
            Assert.That(on.Num("design.n42.impact42.killS"), Is.EqualTo(kill), "the game's config untouched");
            Assert.That(on.Num("quality.dynamic.step"), Is.EqualTo(step));
            Assert.That(ReferenceEquals(iv.movement.settings, on.movement.settings), Is.True, "the player's settings object shared");
            Assert.That(Move54.ImpactPathsSet, Is.EqualTo(Move54.ImpactPaths.GetLength(0)));
        }

        [Test]
        public void SlowEaseInOnlyDown()
        {
            var e = new Move54.SlowEase(Cfg(true));
            const double dt = 1.0 / 60;
            Assert.That(e.Frame(dt, 0, 0.3), Is.EqualTo(0), "a freeze passes through");
            double a = e.Frame(dt, 1, 0.3), b = e.Frame(dt, 1, 0.3);
            Assert.That(a, Is.GreaterThan(0.3).And.LessThan(1), "the first slow frame is eased, not the whole drop");
            Assert.That(b, Is.LessThan(a), "it keeps going down");
            for (int i = 0; i < 60; i++) e.Frame(dt, 1, 0.3);
            Assert.That(e.Frame(dt, 1, 0.3), Is.EqualTo(0.3).Within(1e-6), "it reaches the slow-mo");
            Assert.That(e.Frame(dt, 1, 1), Is.EqualTo(1), "the end returns at once");
        }
    }
}
