// NIGHT24 shotgun-rate (builder R): `shotgun [--tag t]`, the THUNDERCLAP's measured shot-to-shot time BEFORE (design.n24.shotgunTune off, the
// game NIGHT22 shipped) and AFTER (the row on), and the paper DPS of the shotgun and the revolver before and after.
// Shot-to-shot is MEASURED: a sim with the shotgun in hand and the trigger held every step, the "fire" events of the shotgun counted by run
// step (120 Hz), the gaps between them (the cooldown the loadout set: weapons.shotgun.fireInterval x the NIGHT20 retune's knob / the QUICK
// perks rate, rounded by stepsFor). weapons.shotgun.pumpDelay (0.45 s) is the viewmodels pump animation, after the shot; it does not gate the next
// shot (Combat.cs reads no pumpDelay), so the shot-to-shot time includes it by being longer than it: the line prints both.
// The web's number is 0.9 s (config.json); when BEFORE is not 0.9 s the line names what shortened it: here design.arsenal.n20.tune.shotgun
// (interval x0.45, damage x2.5; NIGHT20's retune), read from Loadout.TuneInterval / TuneDamage.
// Pass: AFTER = 1.30 x BEFORE (within one step), per-pellet damage x1.30, paper DPS within 10 % of BEFORE. RED (exit 1): the row's hook does not
// change the interval (the feature absent), which is what this prints before SurvivalSim.Shotgun24.cs exists.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class ShotgunMeasure
    {
        public int steps; // the gap between two shotgun shots, in 120 Hz steps (median)
        public double seconds, perShot, dps, tuneInterval, tuneDamage, intervalMult24, damageMult24;
        public int shots;
    }

    static ShotgunMeasure ShotgunRun(GameConfig cfg, SurvivalMap map, bool tune24, string weapon)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { shotgunTune = tune24 };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(2424, null, null);
        sim.loadout.AddWeapon(weapon);
        int slot = sim.loadout.owned.IndexOf(weapon);
        int stepNow = 0;
        var at = new List<int>();
        sim.events.On("fire", o => { if (o as string == weapon) at.Add(stepNow); });
        var inp = (ScriptedInput)sim.input;
        for (int i = 0; i < 120 * 12; i++)
        {
            stepNow = i;
            inp.SetLook(0, 0);
            inp.Set(default, new CombatInput { weapon = i == 0 ? slot : -1, fire = new Button { held = true, pressed = true } });
            sim.health.hp = sim.health.max;
            sim.hitstopLeft = 0;
            sim.Step();
        }
        var gaps = new List<int>();
        for (int i = 1; i < at.Count; i++) gaps.Add(at[i] - at[i - 1]);
        gaps.Sort();
        double hz = cfg.movement.loop.simHz;
        var m = new ShotgunMeasure { shots = at.Count, steps = gaps.Count == 0 ? 0 : gaps[gaps.Count / 2] };
        m.seconds = m.steps / hz;
        m.tuneInterval = sim.loadout.TuneInterval(weapon);
        m.tuneDamage = sim.loadout.TuneDamage(weapon);
        m.intervalMult24 = weapon == "shotgun" ? sim.loadout.sgIntervalMult24 : 1;
        m.damageMult24 = weapon == "shotgun" ? sim.loadout.sgDamageMult24 : 1;
        if (weapon == "shotgun")
            m.perShot = cfg.Num("weapons.shotgun.pellets") * cfg.Num("weapons.shotgun.pelletDamage") * m.tuneDamage * m.damageMult24;
        else
            m.perShot = cfg.Num("weapons.revolver.damage") * m.tuneDamage;
        m.dps = m.seconds > 0 ? m.perShot / m.seconds : 0;
        return m;
    }

    static partial void ShotgunCommand(string[] args, ref int code)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        ShotgunMeasure before = ShotgunRun(cfg, map, false, "shotgun");
        ShotgunMeasure after = ShotgunRun(cfg, map, true, "shotgun");
        ShotgunMeasure revB = ShotgunRun(cfg, map, false, "revolver");
        ShotgunMeasure revA = ShotgunRun(cfg, map, true, "revolver");
        double web = cfg.Num("weapons.shotgun.fireInterval"), pump = cfg.Num("weapons.shotgun.pumpDelay");
        string why = Math.Abs(before.seconds - web) < 1.5 / cfg.movement.loop.simHz
            ? "the web's interval"
            : $"shortened from the web's {web.ToString("0.###", ci)} s by design.arsenal.n20.tune.shotgun (interval x{before.tuneInterval.ToString("0.###", ci)}, damage x{before.tuneDamage.ToString("0.###", ci)})";
        Console.WriteLine($"shotgun BEFORE: shot-to-shot {before.seconds.ToString("0.000", ci)} s ({before.steps} steps; {before.shots} shots in 12 s; the viewmodel pump {pump.ToString("0.00", ci)} s after each shot is {(pump <= before.seconds ? "inside the gap" : "LONGER than the gap: it overlaps the next shot")}); {why}; per shot {before.perShot.ToString("0.0", ci)}, paper DPS {before.dps.ToString("0.0", ci)}");
        Console.WriteLine($"shotgun AFTER : shot-to-shot {after.seconds.ToString("0.000", ci)} s ({after.steps} steps; {after.shots} shots); interval x{after.intervalMult24.ToString("0.###", ci)}, per-pellet damage x{after.damageMult24.ToString("0.###", ci)}; per shot {after.perShot.ToString("0.0", ci)}, paper DPS {after.dps.ToString("0.0", ci)}");
        Console.WriteLine($"revolver paper DPS: before {revB.dps.ToString("0.0", ci)} ({revB.seconds.ToString("0.000", ci)} s), after {revA.dps.ToString("0.0", ci)} ({revA.seconds.ToString("0.000", ci)} s; the row does not touch it)");
        double want = cfg.Has("design.n24.shotgunTune.intervalMult") ? cfg.Num("design.n24.shotgunTune.intervalMult") : 1.30;
        bool interval = before.steps > 0 && Math.Abs(after.steps - before.steps * want) <= 1.0;
        double ratio = before.dps > 0 ? after.dps / before.dps : 0;
        bool dps = Math.Abs(ratio - 1) <= 0.10;
        bool damage = Math.Abs(after.damageMult24 - want) < 1e-9;
        bool rev = revB.steps == revA.steps && Math.Abs(revB.dps - revA.dps) < 1e-9;
        bool ok = interval && dps && damage && rev;
        Console.WriteLine($"shotgun {(ok ? "PASS" : "RED")}: AFTER/BEFORE interval {(before.steps > 0 ? (after.steps / (double)before.steps).ToString("0.000", ci) : "n/a")} (want {want.ToString("0.00", ci)} within a step: {(interval ? "ok" : "NO")}), damage mult {(damage ? "ok" : "NO")}, DPS ratio {ratio.ToString("0.000", ci)} (within 10 %: {(dps ? "ok" : "NO")}), revolver untouched {(rev ? "ok" : "NO")}");
        code = ok ? 0 : 1;
    }
}
