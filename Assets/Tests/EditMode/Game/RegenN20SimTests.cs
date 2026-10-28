// NIGHT20 U3-34.5 (builder E owns this file; tests first): design.regen.n20 in the sim (Game/SurvivalSim.Regen20.cs):
//   1. with the run's regen block on, the run hands Regen the block's rules (UseN20) and `roundClear` starts the refill to
//      full over refillTime (roundHeal); with it off nothing changes (NIGHT19's regen, no refill)
//   2. a player's kill heals by RegenN20.KillHeal: +closeKillHeal within closeKillRange m of the player's eye, +punchKillHeal
//      for a punch kill, nothing farther or for another owner (earnedHeal)
//   3. HEAT's NO REGEN (regen disabled): no refill at roundClear, the earned heals stay
// The Core rules are RegenN20Tests'.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class RegenN20SimTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Run(bool regenBlock)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg, Map());
            sim.noRecords = sim.noLab = true;
            sim.n20Override = new N20Blocks { regen = regenBlock };
            sim.StartRun(1, null, null);
            return sim;
        }

        static void Steps(SurvivalSim sim, double seconds)
        {
            int n = MovementMath.StepsFor(seconds, sim.cfg.Dt);
            for (int i = 0; i < n; i++) sim.Step();
        }

        static Vec3 Ahead(SurvivalSim sim, double metres)
        {
            Vec3 p = sim.mover.position;
            return new Vec3(p.x + metres, p.y + Cfg.movement.body.eyeHeight, p.z);
        }

        static void Kill(SurvivalSim sim, string weapon, double metres, string owner = "player") =>
            sim.events.Emit("kill", new KillEvent { owner = owner, weapon = weapon, kind = "husk", enemy = 999, position = Ahead(sim, metres), distance = metres });

        [Test]
        public void TheBlockOn_HandsRegenItsRules_AndRoundClearRefills()
        {
            SurvivalSim sim = Run(true);
            Assert.That(sim.regen.N20, Is.True, "the run's regen block reaches Regen");
            Assert.That(sim.regen.SegmentHp, Is.EqualTo(Cfg.Bool("design.regen.n20.passive") ? Cfg.Num("design.regen.n20.segment") : 0), "STRICT draws no segments (MEND would)");
            var heals = new List<double>();
            sim.events.On<RoundHealEvent>(N20Events.RoundHeal, e => heals.Add(e.amount));
            sim.health.hp = 40;
            sim.events.Emit("roundClear", new RoundEvent { number = 1 });
            Assert.That(heals, Is.EqualTo(new[] { sim.health.max - 40 }), "roundHeal with the missing HP");
            Assert.That(sim.regen.RefillLeft, Is.GreaterThan(0));
            Steps(sim, Cfg.Num("design.regen.n20.refillTime") + 0.05);
            Assert.That(sim.health.hp, Is.EqualTo(sim.health.max), "full after refillTime");
            Assert.That(sim.regen.RefillLeft, Is.EqualTo(0));
        }

        [Test]
        public void TheBlockOff_RoundClearChangesNothing()
        {
            SurvivalSim sim = Run(false);
            Assert.That(sim.regen.N20, Is.False);
            var heals = new List<double>();
            sim.events.On<RoundHealEvent>(N20Events.RoundHeal, e => heals.Add(e.amount));
            sim.health.hp = 40;
            sim.events.Emit("roundClear", new RoundEvent { number = 1 });
            Assert.That(heals, Is.Empty);
            Assert.That(sim.regen.RefillLeft, Is.EqualTo(0));
            Kill(sim, "punch", 1);
            Assert.That(sim.health.hp, Is.EqualTo(40), "no earned heals with the block off");
        }

        [Test]
        public void APlayersKill_HealsByItsDistance_AndThePunch()
        {
            SurvivalSim sim = Run(true);
            var earned = new List<string>();
            sim.events.On<RoundHealEvent>(N20Events.EarnedHeal, e => earned.Add(e.kind + " " + e.amount));
            sim.health.hp = 50;
            double close = Cfg.Num("design.regen.n20.closeKillHeal"), punch = Cfg.Num("design.regen.n20.punchKillHeal");
            Kill(sim, "revolver", 3);
            Assert.That(sim.health.hp, Is.EqualTo(50 + close), "+closeKillHeal within closeKillRange of the eye");
            Kill(sim, "revolver", 20);
            Assert.That(sim.health.hp, Is.EqualTo(50 + close), "nothing for a far kill");
            Kill(sim, "punch", 1);
            Assert.That(sim.health.hp, Is.EqualTo(50 + close + punch), "+punchKillHeal for a punch kill");
            Kill(sim, "revolver", 2, "rival");
            Assert.That(sim.health.hp, Is.EqualTo(50 + close + punch), "only the player's kills");
            Assert.That(earned, Is.EqualTo(new[] { "closeKill " + close, "punchKill " + punch }));
        }

        [Test]
        public void NoRegen_NoRefill_TheEarnedHealsStay()
        {
            SurvivalSim sim = Run(true);
            sim.regen.SetEnabled(false); // HEAT's NO REGEN (mutators.js apply)
            sim.health.hp = 40;
            sim.events.Emit("roundClear", new RoundEvent { number = 1 });
            Steps(sim, 3);
            Assert.That(sim.health.hp, Is.EqualTo(40), "no refill");
            Kill(sim, "punch", 1);
            Assert.That(sim.health.hp, Is.EqualTo(50));
        }
    }
}
