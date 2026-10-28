// NIGHT19 U3-33.2 (the lead owns this file): THE RANGE and THE CHASE tell the truth.
//   1. a judge is deterministic: the same episode twice gives the same numbers (THE RANGE: a cell's episode; THE CHASE: a
//      KITE run)
//   2. the hand-checked cell (NIGHT19-LOG, the baseline's hand checks): with every NIGHT19 block off, the KESTREL clears C2
//      (3 husks at 4 m, round 5: 40 HP each) with three 50-point headshots 0.4 s apart, so in 97 steps (0.808 s) with no
//      damage taken; the THUNDERCLAP needs three blasts 0.9 s apart (217 steps)
//   3. a kind with no port makes its cell n/a (never a husk standing in for it)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class SandboxHarnessTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SandboxSpec Spec() => SandboxSpec.Load(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "sandbox-cells.json")));

        [TearDown]
        public void Restore() => N19Blocks.ForceOff = N20Blocks.ForceOff = N21Blocks.ForceOff = N24Blocks.ForceOff = false;

        [Test]
        public void TheRange_IsDeterministic_AndTheHandCheckedCellHolds()
        {
            N19Blocks.ForceOff = N20Blocks.ForceOff = N21Blocks.ForceOff = N24Blocks.ForceOff = true; // NIGHT18's game: the baseline the hand checks read (NIGHT20: the NIGHT20 blocks off too, as --off19 does; NIGHT24: and the NIGHT24 blocks, whose shotgunTune makes the blasts 141 steps apart, not 108)
            SandboxSpec spec = Spec();
            var range = new SandboxRange(Cfg(), Map(), spec);
            Assert.That(range.spotClear, Is.GreaterThan(24), "the open spot sees past the farthest cell (22 m)");
            SandboxCell c2 = spec.Cell("C2");
            SandboxRange.Result a = range.Run(c2, new List<string> { "revolver" }, 1);
            SandboxRange.Result b = range.Run(c2, new List<string> { "revolver" }, 1);
            Assert.That(b.steps, Is.EqualTo(a.steps));
            Assert.That(b.score, Is.EqualTo(a.score));
            Assert.That(a.cleared, Is.True);
            Assert.That(a.steps, Is.EqualTo(97), "three headshots at steps 1, 49, 97 (0.4 s = 48 steps)");
            Assert.That(a.hurt, Is.EqualTo(0));
            Assert.That(a.kills, Is.EqualTo(3));
            SandboxRange.Result s = range.Run(c2, new List<string> { "shotgun" }, 1);
            Assert.That(s.steps, Is.EqualTo(217), "three blasts at steps 1, 109, 217 (0.9 s = 108 steps)");
            Assert.That(s.score, Is.EqualTo(s.seconds + spec.damageWeight * s.hurt).Within(1e-12));
        }

        [Test]
        public void TheChase_IsDeterministic()
        {
            N19Blocks.ForceOff = N20Blocks.ForceOff = N21Blocks.ForceOff = N24Blocks.ForceOff = true; // NIGHT20: as --off19
            SandboxChase.Result a = SandboxChase.Run(Cfg(), Map(), "kite", 10, 3);
            SandboxChase.Result b = SandboxChase.Run(Cfg(), Map(), "kite", 10, 3);
            Assert.That(b.survival, Is.EqualTo(a.survival));
            Assert.That(b.hurt, Is.EqualTo(a.hurt));
            Assert.That(a.died, Is.True);
            Assert.That(a.survival, Is.EqualTo(26.683).Within(0.001), "the hand check (NIGHT19-LOG): KITE, round 10, seed 3");
        }

        [Test]
        public void AKindWithNoPort_MakesItsCellNotAvailable()
        {
            SandboxSpec spec = Spec();
            SandboxCell c8 = spec.Cell("C8");
            if (Director.Ported("charger")) Assert.That(SandboxRange.NotAvailable(c8), Is.Null);
            else Assert.That(SandboxRange.NotAvailable(c8), Does.Contain("CHARGER"));
        }
    }
}
