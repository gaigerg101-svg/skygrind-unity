// NIGHT74 builder B (leftover 3, design.n74.spring74; Game/EnemyViews.Spring74.cs, hooked in Game/EnemyViews.Spring72.cs). Pinned:
//   1. the row: read through NightRows.ViewOn both ways (the reel's override); a FrameRows row only if the lead adds it (the spring runs either way, the application reads the row every frame)
//   2. Kick: no hit under Spring74MinShare of the maximum or inside Spring74GapS of the last kick, none for NaN / a zero maximum / a rise; the smallest hit kicks Spring74SmallMul of the squash, one of
//      Spring74BigShare or more all of it, between the two a straight line
//   3. the curve the numbers make (the same Spring72Math steps the view runs, at 60 fps): the squash is at its depth on the hit frame and held through hit + 2 (the strip's frames), then the spring lets it
//      go: -0.15-ish on the first free frame, a stretch under Spring74Max, quiet inside 0.25 s; the volume stays 1 whatever the squash
//   4. the caps: Applied holds the spring inside +-Spring74Max; the squash is deeper than crunch36's 0.15 by at least 2x, which is the point (spring72's 0.17 sat under it)
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using SM = Skygrind.Game.Spring74Math;

namespace Skygrind.Game.Tests
{
    public class N74BSpringTests
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
        public void Row_ReadBothWays()
        {
            GameConfig c = Cfg();
            NightRows.ReelOverride[EnemyLook.Spring74Row] = true;
            Assert.That(NightRows.ViewOn(c, EnemyLook.Spring74Row), Is.True);
            NightRows.ReelOverride[EnemyLook.Spring74Row] = false;
            Assert.That(NightRows.ViewOn(c, EnemyLook.Spring74Row), Is.False);
            NightRows.ReelOverride.Clear();
            Assert.That(NightRows.ViewOn(c, EnemyLook.Spring74Row), Is.EqualTo(c.Has("design.n74.spring74.enabled") && c.Design("n74.spring74.enabled")), "the config's own answer (false until the lead adds and flips it)");
        }

        [Test]
        public void Kick_ThresholdsAndScale()
        {
            float full = UnityTuning.Spring74Squash, small = full * UnityTuning.Spring74SmallMul;
            double max = 100, ok = 1.0; // a second since the last kick
            Assert.That(SM.Kick(100, 100, max, ok), Is.EqualTo(0f), "no damage");
            Assert.That(SM.Kick(100, 101, max, ok), Is.EqualTo(0f), "a rise (regeneration) is no hit");
            Assert.That(SM.Kick(100, 100 - max * (UnityTuning.Spring74MinShare - 0.01), max, ok), Is.EqualTo(0f), "a tick under the minimum share (a burn)");
            Assert.That(SM.Kick(100, 100 - max * UnityTuning.Spring74MinShare, max, ok), Is.EqualTo(small).Within(1e-5), "the smallest hit: the small multiple");
            Assert.That(SM.Kick(100, 100 - max * UnityTuning.Spring74BigShare, max, ok), Is.EqualTo(full).Within(1e-5), "a big hit: the full squash");
            Assert.That(SM.Kick(100, 0, max, ok), Is.EqualTo(full).Within(1e-5), "beyond it the same");
            float mid = SM.Kick(100, 100 - max * (UnityTuning.Spring74MinShare + UnityTuning.Spring74BigShare) * 0.5, max, ok);
            Assert.That(mid, Is.EqualTo((full + small) * 0.5f).Within(1e-4), "between the two a straight line");
            Assert.That(SM.Kick(100, 50, max, UnityTuning.Spring74GapS * 0.5), Is.EqualTo(0f), "inside the gap since the last kick: one hit, not a burst");
            Assert.That(SM.Kick(double.NaN, 50, max, ok), Is.EqualTo(0f), "the first frame of a body has no health to compare");
            Assert.That(SM.Kick(100, 50, 0, ok), Is.EqualTo(0f), "no maximum");
        }

        [Test]
        public void Curve_DeepOnTheHitFrameAndHeldThroughPlusTwo_ThenReleased()
        {
            float dt = 1f / 60f, x = 0f, v = 0f, hold = 0f, kick = SM.Kick(100, 40, 100, 1.0);
            Assert.That(kick, Is.GreaterThan(0f));
            var xs = new System.Collections.Generic.List<float>();
            // the view's own order on a kicked frame, then the following frames (EnemyViews.Spring74.cs)
            x = -kick; v = 0f; hold = UnityTuning.Spring74HoldS; xs.Add(x);
            for (int f = 1; f <= 20; f++)
            {
                if (hold > 0f) { hold -= dt; x = -kick; v = 0f; }
                else Spring72Math.Step(ref x, ref v, 0f, UnityTuning.Spring74Hz, UnityTuning.Spring74Zeta, dt);
                xs.Add(x);
            }
            Assert.That(xs[0], Is.EqualTo(-kick), "the hit frame shows the depth");
            Assert.That(xs[1], Is.EqualTo(-kick), "hit + 1: held");
            Assert.That(xs[2], Is.EqualTo(-kick), "hit + 2: held");
            Assert.That(Math.Abs(xs[3]), Is.LessThan(kick), "hit + 3: let go");
            float min = 0f, max = 0f;
            foreach (float s in xs) { if (s < min) min = s; if (s > max) max = s; }
            Assert.That(-min, Is.EqualTo(kick).Within(1e-6), "the deepest is the kick");
            Assert.That(max, Is.LessThan(UnityTuning.Spring74Max), "the stretch stays inside the cap");
            Assert.That(max, Is.GreaterThan(0.01f), "and there is one (an overshoot, a snap)");
            for (int f = 15; f <= 20; f++) Assert.That(Math.Abs(xs[f]), Is.LessThan(0.01f), "quiet by 0.25 s (frame " + f + ")");
            foreach (float s in xs)
            {
                float y = 1f + SM.Applied(s), xz = 1f / Mathf.Sqrt(y);
                Assert.That(y * xz * xz, Is.EqualTo(1f).Within(1e-5), "volume kept");
            }
        }

        [Test]
        public void Numbers_TheirReasons()
        {
            Assert.That(UnityTuning.Spring74Squash, Is.GreaterThanOrEqualTo(2f * UnityTuning.Crunch36Squash), "at least twice crunch36's pop: spring72's 0.17 sat under it and read as nothing");
            Assert.That(UnityTuning.Spring74Squash, Is.LessThanOrEqualTo(UnityTuning.Spring74Max));
            Assert.That(SM.Applied(-5f), Is.EqualTo(-UnityTuning.Spring74Max));
            Assert.That(SM.Applied(5f), Is.EqualTo(UnityTuning.Spring74Max));
            Assert.That(SM.Applied(0.1f), Is.EqualTo(0.1f));
            Assert.That(UnityTuning.Spring74HoldS, Is.InRange(0.02f, 0.06f), "a hitstop-style beat: two to three frames at 60 fps");
            Assert.That(UnityTuning.Spring74Zeta, Is.InRange(0.3f, 0.5f), "rings like jelly once (Spring72Math: 0.3 to 0.5)");
        }
    }
}
