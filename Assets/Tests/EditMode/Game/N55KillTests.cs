// NIGHT55 K kill55: the chain ladder's rules (Game/Kill55.cs), pure. Pinned:
//   1. the ladder: the 1st kill asks nothing (the impact rules' freeze stands), the 2nd secondS, the 3rd and later chainS, never over the cap
//   2. the chain: kills within chainGapS of play time chain; a354's spacing (0.483 s) does not; the KESTREL's 0.4 s does
//   3. a granted freeze shorter than the ladder is extended after it ends (one contiguous hold = the ladder); a skipped chain kill holds itself
//      (the loop stops stepping); the rolling-second budget cuts or skips; HIT FREEZE OFF holds nothing
//   4. the config's numbers are the ones the night measured against (NIGHT56 G2: chainGapS >= the KESTREL's live 0.483 s interval and under a355's
//      spaced pass; chainS >= 0.08, under move54's 0.12 cap)
using NUnit.Framework;
using Skygrind.Core;
using Skygrind.Game;

namespace Skygrind.Game.Tests
{
    public class N55KillTests
    {
        static Kill55Hold Make() => new Kill55Hold(0.45, 0.065, 0.08, 0.25, 0.04, 0.12);

        [Test]
        public void Ladder_FirstNothing_SecondThenChain_Capped()
        {
            Assert.That(Kill55.Ladder(1, 0.065, 0.08, 0.12), Is.EqualTo(0));
            Assert.That(Kill55.Ladder(2, 0.065, 0.08, 0.12), Is.EqualTo(0.065));
            Assert.That(Kill55.Ladder(3, 0.065, 0.08, 0.12), Is.EqualTo(0.08));
            Assert.That(Kill55.Ladder(9, 0.065, 0.08, 0.12), Is.EqualTo(0.08));
            Assert.That(Kill55.Ladder(5, 0.065, 0.2, 0.12), Is.EqualTo(0.12), "the cap");
        }

        [Test]
        public void Chain_GapOfPlayTime()
        {
            Assert.That(Kill55.NextChain(0, double.NegativeInfinity, 1, 0.45), Is.EqualTo(1));
            Assert.That(Kill55.NextChain(1, 1, 1.4, 0.45), Is.EqualTo(2), "the KESTREL's 0.4 s");
            Assert.That(Kill55.NextChain(2, 1.4, 1.4 + 0.483, 0.45), Is.EqualTo(1), "a354's 0.483 s: off a chain");
        }

        [Test]
        public void Granted_Extended_Then_Held()
        {
            Kill55Hold h = Make();
            // kill 1 at play 0: the rules freeze 0.05: nothing added
            h.Kill(0);
            Assert.That(h.StepEnded(0, 0, true, 0.05, "kill", 0, 1), Is.False);
            Assert.That(h.HoldLeft, Is.EqualTo(0));
            // kill 2 at play 0.4: the rules skipped it (the gap): kill55 holds 0.065 itself
            h.Kill(0.4);
            Assert.That(h.StepEnded(0.45, 0.4, false, 0, null, 0.05, 1), Is.True, "the loop stops stepping");
            Assert.That(h.HoldLeft, Is.EqualTo(0.065).Within(1e-12));
            double s = h.Frame(1 / 60.0, 0, 1);
            Assert.That(s, Is.EqualTo(0), "a held frame");
            for (int i = 0; i < 10 && h.HoldLeft > 0; i++) h.Frame(1 / 60.0, 0, 1);
            Assert.That(h.HoldLeft, Is.EqualTo(0));
            // kill 3 at play 0.8: the rules granted 0.05: extended by 0.03 after it ends
            h.Kill(0.8);
            Assert.That(h.StepEnded(0.915, 0.8, true, 0.05, "kill", 0.05, 1), Is.False);
            Assert.That(h.HoldLeft, Is.EqualTo(0.03).Within(1e-12));
            Assert.That(h.Frame(1 / 60.0, 0.05 - 1 / 60.0, 0), Is.EqualTo(0), "the rules still frozen: untouched");
            Assert.That(h.HoldLeft, Is.EqualTo(0.03).Within(1e-12));
            Kill55Hold.KillRec r = h.ProbeAt(h.RecCount - 1);
            Assert.That(r.chain, Is.EqualTo(3));
            Assert.That(r.rulesS + r.extraS, Is.EqualTo(0.08).Within(1e-12), "one hold: the ladder's");
        }

        [Test]
        public void Budget_CutsOrSkips_And_HitFreezeOff()
        {
            Kill55Hold h = Make();
            h.Kill(0);
            h.StepEnded(0, 0, true, 0.05, "kill", 0, 1);
            h.Kill(0.3);
            // the rolling second already holds 0.2 of the rules' freezes: 0.05 left, cut to it
            Assert.That(h.StepEnded(0.35, 0.3, false, 0, null, 0.2, 1), Is.True);
            Assert.That(h.HoldLeft, Is.EqualTo(0.05).Within(1e-12));
            Kill55Hold o = Make();
            o.Kill(0);
            o.StepEnded(0, 0, true, 0.05, "kill", 0, 1);
            o.Kill(0.3);
            Assert.That(o.StepEnded(0.35, 0.3, false, 0, null, 0.23, 1), Is.False, "under minHoldS left: skipped");
            Kill55Hold off = Make();
            off.Kill(0);
            off.StepEnded(0, 0, false, 0, null, 0, 0);
            off.Kill(0.3);
            Assert.That(off.StepEnded(0.3, 0.3, false, 0, null, 0, 0), Is.False, "HIT FREEZE OFF");
            Assert.That(off.HoldLeft, Is.EqualTo(0));
        }

        /// <summary>NIGHT57 G (G3): a chain kill while RUNNING asks no extra hold (the rules' own freeze stands, as NIGHT54's), and the chain still
        /// counts; the same chain standing holds the ladder; the config's runSpeed sits between a355's stand (0) and a354's run (the walk speed).</summary>
        [Test]
        public void Running_TheChainCounts_NoExtraHold()
        {
            Kill55Hold h = new Kill55Hold(0.55, 0.065, 0.09, 0.25, 0.04, 0.12, 4);
            h.Kill(0);
            Assert.That(h.StepEnded(0, 0, true, 0.05, "kill", 0, 1, true), Is.False);
            h.Kill(0.483);
            Assert.That(h.StepEnded(0.6, 0.483, true, 0.05, "kill", 0.05, 1, true), Is.False, "running: no hold of its own");
            Assert.That(h.HoldLeft, Is.EqualTo(0), "running: nothing extended");
            Assert.That(h.Chain, Is.EqualTo(2), "the chain still counts");
            Assert.That(h.RunSkipped, Is.EqualTo(1));
            h.Kill(0.966);
            Assert.That(h.StepEnded(1.2, 0.966, true, 0.05, "kill", 0.05, 1, false), Is.False);
            Assert.That(h.HoldLeft, Is.EqualTo(0.04).Within(1e-12), "standing again: the 3rd kill's chainS 0.09 over the rules' 0.05");
            GameConfig c = GameConfig.Load(System.IO.Path.Combine(System.IO.Path.GetFullPath("PortExport"), "config.json"));
            double run = c.Num(Kill55.Numbers + "runSpeed");
            Assert.That(run > 0 && run < c.Num("ground.walkSpeed"), Is.True, "between a355's stand and a354's run at the walk speed");
            Assert.That(Kill55Hold.From(c, 0.12).runSpeed, Is.EqualTo(run), "read from the config");
        }

        [Test]
        public void Config_TheNumbersMeasuredAgainst()
        {
            GameConfig c = GameConfig.Load(System.IO.Path.Combine(System.IO.Path.GetFullPath("PortExport"), "config.json"));
            // NIGHT56 G (G2): the KESTREL's LIVE interval is weapons.revolver.fireInterval x design.n39.p39.intervalMult (0.4 x 1.2 = 58 steps,
            // 0.483 s: `n56 kestrel --scheme alt`), a354's own spacing; NIGHT55's 0.45 sat under it, so a KESTREL chain never formed (n56-r1 a355).
            double gap = c.Num(Kill55.Numbers + "chainGapS"), dt = c.Dt;
            double live = MovementMath.StepsFor(c.Num("weapons.revolver.fireInterval") * c.Num("design.n39.p39.intervalMult"), dt) * dt;
            Assert.That(gap >= live, Is.True, "the KESTREL at its own live rate chains (" + live + " s)");
            Assert.That(gap < UnityTuning.Kill55ReelSpacedEvery / 60.0 - 0.05, Is.True, "a355's spaced pass (play gap ~0.62 s) stays off a chain");
            Assert.That(c.Num(Kill55.Numbers + "chainS"), Is.GreaterThanOrEqualTo(0.08));
            Assert.That(c.Num(Kill55.Numbers + "chainS"), Is.LessThanOrEqualTo(c.Num("design.n54.move54.numbers.impact.killCapS")), "under move54's cap");
        }
    }
}
