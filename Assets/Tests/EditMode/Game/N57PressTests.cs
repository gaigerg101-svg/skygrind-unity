// NIGHT57 builder G (G2 press57; Core/Press57.cs, SurvivalSim.N57.Press.cs). Pinned:
//   1. the row ships off with its number (markR = spitter.puddleRadius, the centre glob's acid)
//   2. MARK -> STRIKE: a marked spitter's volley flies to the marked spot (not to where its target is at the throw), lands in the flight time to
//      the spot, and draws no miss; the mark lands at tell + flight; with on57 false the same throw aims at the target as NIGHT56's does
//   3. row off: Press57On false (the config) and with the row forced off; row on in SURVIVAL: the acid's on57 stays false (a campaign rule)
// The bot numbers (the stall share, deaths, clear time, damage taken moving / still) are n46flow's SUMMARY / PRESS57 lines
// (Research/2026-10-21-wave57/G.md). No UnityEngine, so it also runs headless on the harness's mini NUnit:
//   dotnet run --project Tools/SimHarness -c Release -p:Extra=../../Assets/Tests/EditMode/Game/N57PressTests.cs -- n57g selftest
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N57PressTests
    {
        static string Root => Path.GetFullPath("PortExport");

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalSim Sim(bool? press57, out GameConfig cfg)
        {
            cfg = Cfg();
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            if (press57.HasValue) sim.rowsOverride = sim.rowsRegistry.Defaults().Set("press57", press57.Value);
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(4711, null, null);
            return sim;
        }

        [Test]
        public void TheRowShipsOff_WithItsNumber()
        {
            GameConfig cfg = Cfg();
            Assert.That(cfg.Design("n57.press57.enabled"), Is.False, "press57 ships off");
            Press57Params p = Press57Params.From(cfg);
            Assert.That(p.markR, Is.EqualTo(cfg.Num("spitter.puddleRadius")), "markR = the centre glob's acid");
        }

        /// <summary>The horizontal point a volley's middle glob lands on (its launch and velocity, the acid's gravity: x / z are linear).</summary>
        static (double x, double z) Landing(SpitterAcid acid, int i, double time)
        {
            SpitterAcid.Glob g = acid.globs[i];
            return (g.position.x + g.velocity.x * time, g.position.z + g.velocity.z * time);
        }

        static int LiveGlob(SpitterAcid acid, int nth)
        {
            int n = 0;
            for (int i = 0; i < acid.globs.Length; i++)
                if (acid.globs[i].alive && n++ == nth) return i;
            return -1;
        }

        [Test]
        public void MarkThenStrike_TheVolleyGoesToTheSpot_OffItAimsAtTheTarget()
        {
            SurvivalSim sim = Sim(false, out GameConfig cfg);
            sim.husks.KillAll();
            SpitterAcid acid = sim.husks.acid;
            acid.Clear();
            Vec3 at = new Vec3(0, 0, 0), spot = new Vec3(4, 0, -12), there = new Vec3(-6, 0, -14);
            var sp = (Spitter)sim.husks.Spawn("spitter", at, spot);
            Assert.That(sp != null, Is.True, "a spitter");
            sp.ai = null; // no brain: no hit roll, no miss (the off throw's aim is the lead point)

            // ON: marked at `spot`, thrown while its target stands at `there`
            acid.on57 = true;
            acid.p57 = Press57Params.From(cfg);
            acid.Alloc57();
            acid.Clear57(null);
            SpitterAcid.Mark57 m = acid.Mark57At(sp, spot, 1.0, null);
            Assert.That(m != null, Is.True, "marked");
            double flight = Math.Max(acid.p.globTimeMin, Math.Min(acid.p.globTimeMax, JsMath.Hypot(spot.x - sp.position.x, spot.z - sp.position.z) / acid.p.globSpeed));
            Assert.That(Math.Abs(m.land - (1.0 + flight)) < 1e-9, Is.True, "lands at tell + the flight to the spot");
            sp.state = "tell";
            acid.ThrowVolley(sp, there);
            Assert.That(acid.struck57, Is.EqualTo(1), "a volley thrown at its mark");
            Assert.That(m.thrown, Is.True);
            int mid = LiveGlob(acid, 1);
            Assert.That(mid >= 0, Is.True, "three globs");
            (double lx, double lz) = Landing(acid, mid, flight);
            Assert.That(JsMath.Hypot(lx - spot.x, lz - spot.z) < 1e-6, Is.True, $"the middle glob lands on the spot ({lx:0.###}, {lz:0.###})");

            // OFF: the same throw aims at the target (no lead: the target's velocity is 0 in this run)
            acid.Clear();
            acid.on57 = false;
            acid.Clear57(null);
            acid.ThrowVolley(sp, there);
            Assert.That(acid.struck57, Is.EqualTo(0), "off: nothing struck at a mark");
            mid = LiveGlob(acid, 1);
            double tOff = Math.Max(acid.p.globTimeMin, Math.Min(acid.p.globTimeMax, JsMath.Hypot(there.x - sp.position.x, there.z - sp.position.z) / acid.p.globSpeed));
            (lx, lz) = Landing(acid, mid, tOff);
            Assert.That(JsMath.Hypot(lx - there.x, lz - there.z) < 1e-6, Is.True, $"off: the middle glob lands on the target ({lx:0.###}, {lz:0.###})");
        }

        [Test]
        public void RowOff_PressIsOff_AndSurvivalKeepsTheWebsSpitter()
        {
            SurvivalSim plain = Sim(null, out _);
            Assert.That(plain.Press57On, Is.False, "the config ships the row off");
            SurvivalSim off = Sim(false, out _);
            Assert.That(off.Press57On, Is.False);
            SurvivalSim on = Sim(true, out _);
            Assert.That(on.Press57On, Is.True, "the bit is read");
            for (int i = 0; i < 240; i++) on.Step();
            Assert.That(on.husks.acid.on57, Is.False, "a SURVIVAL run keeps the web's spitter (a campaign rule, as tokens56)");
            Assert.That(on.husks.acid.marked57, Is.EqualTo(0), "nothing marked in survival");
        }
    }
}
