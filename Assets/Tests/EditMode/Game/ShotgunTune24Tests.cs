// NIGHT24 shotgun-rate (builder R; design.n24.shotgunTune). The OFF case first: with the row off (the config default, a tape without the bit,
// or --off24) the multipliers are exactly 1 and the THUNDERCLAP's shot-to-shot gap is NIGHT22's. ON: the gap is 1.30 x (within a step) and
// the shot's damage 1.30 x, so the damage per second stays within 10 %. The same measurement runs outside Unity: SimHarness shotgun.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class ShotgunTune24Tests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        /// <summary>The median step gap between the THUNDERCLAP's shots with the trigger held (and the sim, for its multipliers).</summary>
        static int Gap(N24Blocks blocks, out SurvivalSim simOut)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.noRecords = sim.noLab = true;
            sim.n24Override = blocks;
            sim.StartRun(2424, null, null);
            sim.loadout.AddWeapon("shotgun");
            int slot = sim.loadout.owned.IndexOf("shotgun"), now = 0;
            var at = new List<int>();
            sim.events.On("fire", o => { if (o as string == "shotgun") at.Add(now); });
            var inp = (ScriptedInput)sim.input;
            for (int i = 0; i < 120 * 6; i++)
            {
                now = i;
                inp.SetLook(0, 0);
                inp.Set(default, new CombatInput { weapon = i == 0 ? slot : -1, fire = new Button { held = true, pressed = true } });
                sim.health.hp = sim.health.max;
                sim.hitstopLeft = 0;
                sim.Step();
            }
            var gaps = new List<int>();
            for (int i = 1; i < at.Count; i++) gaps.Add(at[i] - at[i - 1]);
            gaps.Sort();
            simOut = sim;
            return gaps[gaps.Count / 2];
        }

        [Test]
        public void Off_WithTheRowOff_TheMultipliersAreExactlyOne()
        {
            GameConfig cfg = Cfg(); // the row is forced off below (N24Blocks.Off as the override), whatever the config default is
            Assert.That(cfg.Has("design.n24.shotgunTune.intervalMult") && cfg.Has("design.n24.shotgunTune.damageMult"), Is.True);
            int off = Gap(N24Blocks.Off, out SurvivalSim sim);
            Assert.That((sim.loadout.sgIntervalMult24, sim.loadout.sgDamageMult24), Is.EqualTo((1.0, 1.0)));
            Assert.That(off, Is.EqualTo(MovementMath.StepsFor(cfg.Num("weapons.shotgun.fireInterval") * sim.loadout.TuneInterval("shotgun"), 1 / cfg.movement.loop.simHz)),
                "off: the web's interval times the NIGHT20 retune's knob, as NIGHT22 shipped it");
        }

        [Test]
        public void Off24_ForceOff_KeepsTheMultipliersAtOne()
        {
            bool was = N24Blocks.ForceOff;
            try
            {
                N24Blocks.ForceOff = true;
                SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
                sim.noRecords = sim.noLab = true;
                sim.StartRun(2424, null, null);
                Assert.That(sim.n24.shotgunTune, Is.False);
                Assert.That(sim.loadout.sgIntervalMult24, Is.EqualTo(1.0));
            }
            finally { N24Blocks.ForceOff = was; }
        }

        [Test]
        public void On_TheGapIsOnePointThreeTimes_TheShotHitsOnePointThreeTimesHarder_DpsWithinTenPercent()
        {
            GameConfig cfg = Cfg();
            int before = Gap(N24Blocks.Off, out _);
            int after = Gap(new N24Blocks { shotgunTune = true }, out SurvivalSim sim);
            double want = cfg.Num("design.n24.shotgunTune.intervalMult");
            Assert.That(want, Is.EqualTo(1.30).Within(1e-9));
            Assert.That(after, Is.EqualTo(before * want).Within(1.0), $"{before} -> {after} steps");
            Assert.That(sim.loadout.sgDamageMult24, Is.EqualTo(cfg.Num("design.n24.shotgunTune.damageMult")));
            double dps = sim.loadout.sgDamageMult24 / after, dps0 = 1.0 / before;
            Assert.That(dps / dps0, Is.EqualTo(1.0).Within(0.10), "slower, not weaker");
        }
    }
}
