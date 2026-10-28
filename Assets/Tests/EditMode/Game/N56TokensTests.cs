// NIGHT56 builder G (G3 tokens56; Core/HuskBrain.N56.cs, SurvivalSim.N56.Tokens.cs). Pinned:
//   1. the sim cone: half angle camera.fovDeg / 2 round the look (a point at 36 deg is in, at 38 deg out, behind out), from config, not a
//      viewer setting; the numbers exist and the row ships off
//   2. row off: the brain's rules are off (on56 false) in a campaign-less run and with the row forced off; row on in SURVIVAL: still off
//      (tokens56 is a campaign rule, as tokens41)
// The bot numbers (attack starts in the cone, the idle share, deaths) are n46flow's TOKENS / IDLE lines (Research/2026-10-20-wave56/G.md).
// No UnityEngine, so it also runs headless on the harness's mini NUnit:
//   dotnet run --project Tools/SimHarness -c Release -p:Extra=... -- n56 selftest N56TokensTests
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N56TokensTests
    {
        static string Root => Path.GetFullPath("PortExport");

        static Vec3 Dir(double yawDeg, double pitchDeg)
        {
            double y = yawDeg * Math.PI / 180, p = pitchDeg * Math.PI / 180;
            return new Vec3(-Math.Sin(y) * Math.Cos(p), Math.Sin(p), -Math.Cos(y) * Math.Cos(p));
        }

        [Test]
        public void TheCone_HalfTheConfigFov_RoundTheLook_TheRowShipsOff()
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            // NIGHT57 freeze (D-10): tokens56 ships ON; off unless a freeze turned it on with an "ON ..." reason
            Assert.That(!cfg.Design("n56.tokens56.enabled") || (cfg.Get("design.n56.tokens56.enabled_why") as string ?? "").StartsWith("ON", StringComparison.Ordinal), Is.True, "tokens56: off, or ON with its reason");
            Tokens56Params p = Tokens56Params.From(cfg);
            double half = cfg.Num("camera.fovDeg") * 0.5;
            Assert.That(Math.Abs(p.cosHalf - Math.Cos(half * Math.PI / 180)) < 1e-12, Is.True, "cos(camera.fovDeg / 2)");
            Vec3 eye = new Vec3(0, 1.6, 0), look = Dir(0, 0);
            Vec3 At(double yawDeg, double pitchDeg) => eye + Dir(yawDeg, pitchDeg) * 10;
            Assert.That(Tokens56.InCone(eye, look, At(0, 0), p.cosHalf), Is.True, "straight ahead");
            Assert.That(Tokens56.InCone(eye, look, At(half - 1, 0), p.cosHalf), Is.True, "1 deg inside the half angle (yaw)");
            Assert.That(Tokens56.InCone(eye, look, At(half + 1, 0), p.cosHalf), Is.False, "1 deg outside (yaw)");
            Assert.That(Tokens56.InCone(eye, look, At(0, half + 1), p.cosHalf), Is.False, "1 deg outside (pitch)");
            Assert.That(Tokens56.InCone(eye, look, At(180, 0), p.cosHalf), Is.False, "behind");
            Assert.That(Tokens56.InCone(eye, Dir(90, 0), At(90, 0), p.cosHalf), Is.True, "the cone turns with the look");
            Assert.That(p.waitS > 0 && p.idleS > 0 && p.idleSpeed > 0 && p.minM > 0 && p.arriveM > 0 && p.advanceMaxS > 0, Is.True, "the numbers");
        }

        static SurvivalSim Run(bool? tokens56)
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            if (tokens56.HasValue) sim.rowsOverride = sim.rowsRegistry.Defaults().Set("tokens56", tokens56.Value);
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(4711, null, null);
            for (int i = 0; i < 240; i++) sim.Step();
            return sim;
        }

        [Test]
        public void RowOff_TheBrainsRulesAreOff_AndSurvivalKeepsTheWebsBrain()
        {
            SurvivalSim plain = Run(null);
            Assert.That(plain.Tokens56On, Is.False, "the config ships the row off");
            Assert.That(plain.brain.on56, Is.False, "off: the brain's rules skipped");
            SurvivalSim off = Run(false);
            Assert.That(off.brain.on56, Is.False);
            SurvivalSim on = Run(true);
            Assert.That(on.Tokens56On, Is.True, "the bit is read");
            Assert.That(on.brain.on56, Is.False, "a SURVIVAL run keeps the web's brain (a campaign rule, as tokens41)");
            Assert.That(on.brain.coneWaits56 + on.brain.advances56, Is.EqualTo(0), "nothing counted in survival");
        }
    }
}
