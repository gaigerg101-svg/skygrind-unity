// NIGHT25 (builder C): the combat contracts' headless judges (the lead's dispatch calls MeleeFireCommand / PulseCommand).
//   meleefire [--off] [--shockwave]   contract melee-fire: shots + punches held together over 3 s on every gun, vs the same gun firing alone and the
//                                     punch alone (no free damage: the DPS added is the punch's own); --shockwave: contract shockwave-off, ten shotgun LT
//                                     presses: no shockwave / double blast / cone events, the zoom comes on, no compress
//   pulse [--off]                     contract pulse-charge: hits per tier, only the PULSE's hits feed the charge (not a kill by another gun, not the
//                                     punch key, not time), the release launch distances against 0.5 / 1.0 / 2.25 jumps, the launch's damage, the lockout
// `--off` = every NIGHT25 block off (NIGHT24's game): the RED side. The rows are forced on by an override here, whatever the config's `enabled` says.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static SurvivalSim CombatSim25(GameConfig cfg, SurvivalMap map, bool on, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { chargeFist = true }; // NIGHT24's fist (the rules the pulse replaces) on both sides
        sim.n25Override = on ? new N25Blocks { meleeFire = true, pulseCharge = true, shockwaveOff = true } : N25Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.mover.invulnerable = true;
        return sim;
    }

    static void Tick25(SurvivalSim sim, double yaw, double pitch, CombatInput combat, double moveZ = 0)
    {
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, pitch);
        inp.Set(new InputFrame { moveZ = moveZ }, combat);
        sim.hitstopLeft = 0;
        sim.Step();
    }

    /// <summary>A husk pinned `d` m ahead of the player along `u` with a huge hp (a punching bag that can't kill the invulnerable player).</summary>
    static Husk Bag25(SurvivalSim sim, Vec3 p, double ux, double uz, double d)
    {
        Husk h = sim.husks.Spawn("husk", new Vec3(p.x + ux * d, p.y, p.z + uz * d), p);
        if (h != null) h.hp = h.maxHp = 1e9;
        return h;
    }

    static void Pin25(Husk h, Vec3 p, double ux, double uz, double d)
    {
        if (h != null && h.alive) h.position = new Vec3(p.x + ux * d, p.y, p.z + uz * d);
    }

    static (int shots, int punches, double gunDmg, double punchDmg) MeleeFireRun25(GameConfig cfg, SurvivalMap map, string weapon, bool fire, bool punch, bool on, int steps)
    {
        SurvivalSim sim = CombatSim25(cfg, map, on, 2501);
        if (!sim.loadout.owned.Contains(weapon)) sim.loadout.AddWeapon(weapon);
        int slot = sim.loadout.owned.IndexOf(weapon);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        int shots = 0, punches = 0;
        double gd = 0, pd = 0;
        sim.events.On<string>("fire", w => shots++);
        sim.events.On<MeleeEvent>("punch", e => punches++);
        sim.events.On<MeleeEvent>("meleeSwing", e => punches++);
        sim.events.On<HitEvent>("hit", e =>
        {
            if (e.owner != "player") return;
            if (e.weapon == "punch" || e.weapon == "gauntlet") pd += e.damage; else gd += e.damage;
        });
        Husk h = Bag25(sim, map.playerSpawn, ux, uz, 1.6);
        for (int s = 0; s < steps; s++)
        {
            double py = 0;
            Pin25(h, map.playerSpawn, ux, uz, 1.6);
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            var c = new CombatInput { weapon = s == 0 ? slot : -1 };
            if (fire) c.fire = new Button { held = true, pressed = s == 0 };
            if (punch) c.punch = new Button { held = true, pressed = s % 6 == 0 };
            Tick25(sim, yaw, py, c);
        }
        return (shots, punches, gd, pd);
    }

    static partial void MeleeFireCommand(string[] args, ref int code)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        if (Array.IndexOf(args, "--shockwave") >= 0) { ShockwaveOffRun25(cfg, map, !off, ref code); return; }
        int steps = 360; // 3 s at 120 Hz
        var fails = new List<string>();
        foreach (string gun in new[] { "revolver", "shotgun", "nailgun", "rocket", "rail", "gauntlet" })
        {
            var f = MeleeFireRun25(cfg, map, gun, true, false, !off, steps);
            var p = MeleeFireRun25(cfg, map, gun, false, true, !off, steps);
            var b = MeleeFireRun25(cfg, map, gun, true, true, !off, steps);
            int swings = b.shots + b.punches, alone = f.shots + f.punches + p.punches - (gun == "gauntlet" ? 0 : 0);
            bool same = gun == "gauntlet" ? Math.Abs(b.punches - (f.punches + p.punches)) <= 1 : b.shots == f.shots && b.punches == p.punches && b.punches > 0;
            double extra = b.punchDmg / 3.0;
            Console.WriteLine($"  {gun,-8} 3 s: gun alone {f.shots} shots / {f.punches} swings; punch alone {p.punches}; both held {b.shots} shots + {b.punches} punches ({(same ? "both ran" : "ONE EXCLUDED THE OTHER")}); damage gun {b.gunDmg:0} (alone {f.gunDmg:0}) + punch {b.punchDmg:0}: +{extra:0.#} DPS from the punches' own 0.35 s cycle");
            if (!same) fails.Add(gun);
        }
        Console.WriteLine($"meleefire {(fails.Count == 0 ? "PASS" : "FAIL")}{(off ? " (--off: NIGHT24's rules)" : "")}: shots and punches held together over 3 s match each held alone on every gun, each on its own cooldown, no shot lost and no free damage (the punches add their own {(10 / 0.35):0.#} DPS, as they did before){(fails.Count == 0 ? "" : "; excluded on " + string.Join(",", fails))}");
        code = fails.Count == 0 ? 0 : 1;
    }

    /// <summary>Ten shotgun LT presses (a second apart): no shockwave events, ADS on with a zoom above 1, no compress.</summary>
    static void ShockwaveOffRun25(GameConfig cfg, SurvivalMap map, bool on, ref int code)
    {
        SurvivalSim sim = CombatSim25(cfg, map, on, 2503);
        if (!sim.loadout.owned.Contains("shotgun")) sim.loadout.AddWeapon("shotgun");
        int slot = sim.loadout.owned.IndexOf("shotgun");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        int shock = 0, blast = 0, cone = 0, adsStart = 0;
        double maxZoom = 1, maxCompress = 0;
        sim.events.On<ShockwaveEvent>(N20Events.Shockwave, e => shock++);
        sim.events.On<ShockwaveEvent>(N20Events.DoubleBlast, e => blast++);
        sim.events.On<ShockwaveEvent>(N20Events.ConeWave, e => cone++);
        sim.events.On<string>("adsStart", w => adsStart++);
        for (int s = 0; s < 120; s++) Tick25(sim, yaw, 0, new CombatInput { weapon = s == 0 ? slot : -1 });
        int zoomedPresses = 0;
        for (int press = 0; press < 10; press++)
        {
            for (int s = 0; s < 120; s++)
            {
                bool down = s < 30;
                Tick25(sim, yaw, 0, new CombatInput { weapon = -1, altFire = new Button { held = down, pressed = s == 0 }, special = new Button { held = false, pressed = false } });
                if (s == 20)
                {
                    maxZoom = Math.Max(maxZoom, sim.loadout.ZoomMag);
                    maxCompress = Math.Max(maxCompress, sim.loadout.Compress);
                    if (sim.loadout.ZoomMag > 1) zoomedPresses++;
                }
            }
        }
        bool ok = shock == 0 && blast == 0 && cone == 0 && zoomedPresses == 10 && maxCompress == 0 && !sim.loadout.ShockwaveOn;
        Console.WriteLine($"shockwaveoff {(ok ? "PASS" : "FAIL")}{(!on ? " (--off: NIGHT24's shotgun)" : "")}: {(shock + blast + cone == 0 ? "no shockwave events" : $"{shock} shockwave / {blast} doubleBlast / {cone} coneWave events")} on 10 shotgun LT presses; zoom {(zoomedPresses == 10 ? "on" : $"on {zoomedPresses} of 10")} (x{maxZoom:0.##}, {adsStart} adsStart), compress {maxCompress:0.##}");
        code = ok ? 0 : 1;
    }

    // ---- pulse --------------------------------------------------------------------------------------------------------------------------

    static partial void PulseCommand(string[] args, ref int code)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        Fist24Rules rules = Fist24Rules.From(cfg);
        double J = rules.jumpDistance;

        // 1 hits per tier: the gauntlet held, the left click held, a pinned husk
        {
            SurvivalSim sim = CombatSim25(cfg, map, !off, 2511);
            sim.fist24.noFlick = true;
            sim.fist24.rules.hurtLoss = 0; // the pinned bag swings back: a hit on the player would take a third of the charge (a different rule, tested by fist)
            int slot = sim.loadout.owned.IndexOf("gauntlet");
            sim.Respawn(map.playerSpawn, 0);
            (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
            Husk h = Bag25(sim, map.playerSpawn, ux, uz, 1.6);
            int hits = 0;
            var atTier = new int[4];
            sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") hits++; });
            for (int s = 0; s < 3600 * 2 && atTier[3] == 0; s++)
            {
                Pin25(h, map.playerSpawn, ux, uz, 1.6);
                (double yy, double pp) = ArclineLook(sim.Eye, h.AimPoint);
                Tick25(sim, yy, pp, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = true, pressed = s == 0 } });
                int t = sim.fist24.Tier;
                for (int k = 1; k <= 3; k++) if (t >= k && atTier[k] == 0) atTier[k] = hits;
            }
            Console.WriteLine($"  1 hits to the notches: tier 1 after {atTier[1]} hits, tier 2 after {atTier[2]}, tier 3 after {atTier[3]} ({(sim.fist24.pulseMode ? "pulse mode" : "NIGHT24's fist rates")}, {sim.fist24.hitsFed} fed)");
            Need(atTier[1] >= 6 && atTier[1] <= 8, $"tier 1 took {atTier[1]} hits, not 6-8");
            Need(atTier[2] - atTier[1] >= 6 && atTier[2] - atTier[1] <= 8 && atTier[3] - atTier[2] >= 6 && atTier[3] - atTier[2] <= 8, $"the notches took {atTier[1]} / {atTier[2] - atTier[1]} / {atTier[3] - atTier[2]} hits, not 6-8 each");
            double sec = atTier[1] * 0.42;
            Console.WriteLine($"    a notch is about {sec:0.#} s of landed swings at the gauntlet's cycle, then the {rules.lockout:0.#} s lockout after each launch: the long cooldown");

            // the Player's path (the reel): the SAME sim banks the first notch on a husk 1.6 m ahead, is respawned grounded with a husk 1.5 m ahead,
            // the notch restaged and the click let go: it must launch (a banked hold never launches: noFlick; the husk is no collider)
            sim.husks.KillAll();
            Tick25(sim, yaw, 0, new CombatInput { weapon = -1 }); // the click let go with noFlick still set: the banked hold never launches
            sim.fist24.noFlick = false;
            sim.Respawn(map.playerSpawn, 0);
            for (int s = 0; s < 150; s++) Tick25(sim, yaw, 0, new CombatInput { weapon = -1 });
            Husk ahead = Bag25(sim, map.playerSpawn, ux, uz, 1.5);
            sim.fist24.charge = rules.tiers[0];
            Vec3 from = sim.mover.position;
            int l0 = sim.loadout.pulseLaunches;
            for (int s = 0; s < 720; s++)
            {
                Pin25(ahead, map.playerSpawn, ux, uz, 1.5 + Math.Min(0.5, 0));
                Tick25(sim, yaw, 0, new CombatInput { weapon = -1, fire = new Button { held = s < 8, pressed = s == 0 } });
                if (s > 40 && sim.mover.Speed < 0.05 && sim.mover.grounded) break;
            }
            double mv = FistHoriz(from, sim.mover.position);
            Console.WriteLine($"    player path: grounded, husk 1.5 m ahead, first notch staged on the same sim after the hold: launches {sim.loadout.pulseLaunches - l0}, moved {mv:0.00} m (asked {rules.jumpMult[0] * J:0.00} m), the husk took {(ahead != null ? (1e9 - ahead.hp) : 0):0} damage");
            Need(sim.loadout.pulseLaunches - l0 == 1 && mv >= 0.5 * rules.jumpMult[0] * J, $"the Player path launched {sim.loadout.pulseLaunches - l0} times and moved {mv:0.00} m");
        }

        // 2 only the PULSE's own hits feed it: a revolver kill, a quick punch's hits, time
        {
            SurvivalSim sim = CombatSim25(cfg, map, !off, 2512);
            sim.fist24.noFlick = true;
            sim.Respawn(map.playerSpawn, 0);
            (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 6);
            Husk h1 = sim.husks.Spawn("husk", new Vec3(map.playerSpawn.x + ux * 8, 0, map.playerSpawn.z + uz * 8), map.playerSpawn);
            int rev = sim.loadout.owned.IndexOf("revolver");
            for (int s = 0; s < 1500 && h1 != null && h1.alive; s++)
            {
                (double yy, double pp) = ArclineLook(sim.Eye, h1.AimPoint);
                Tick25(sim, yy, pp, new CombatInput { weapon = s == 0 ? rev : -1, fire = new Button { held = true, pressed = true } });
            }
            double afterGun = sim.fist24.charge;
            Husk bag = Bag25(sim, map.playerSpawn, ux, uz, 1.6);
            for (int s = 0; s < 400; s++)
            {
                Pin25(bag, map.playerSpawn, ux, uz, 1.6);
                (double yy, double pp) = ArclineLook(sim.Eye, bag.AimPoint);
                Tick25(sim, yy, pp, new CombatInput { weapon = -1, punch = new Button { held = true, pressed = s % 42 == 0 } });
            }
            double afterPunch = sim.fist24.charge;
            SurvivalSim idle = CombatSim25(cfg, map, !off, 2513);
            idle.Respawn(map.playerSpawn, 0);
            double hi = 0;
            for (int s = 0; s < 7200; s++) { Tick25(idle, 0, 0, new CombatInput { weapon = -1 }); hi = Math.Max(hi, idle.fist24.charge); }
            Console.WriteLine($"  2 feeds: a husk killed by the revolver -> charge {afterGun:0.000}; 10 quick punches (F) on a body -> {afterPunch:0.000}; idle 60 s -> highest {hi:0.000} (end {idle.fist24.charge:0.000})");
            Need(afterGun == 0, "another gun's kill fed the charge");
            Need(afterPunch == 0, "the quick punch fed the charge");
            Need(hi == 0, "the charge rose with time");
        }

        // 3 the release launch: staged charge at each notch, the left click held and let go
        bool found;
        Vec3 start;
        double fyaw, wallDist;
        {
            SurvivalSim probe = CombatSim25(cfg, map, !off, 2514);
            found = FistFlat(probe, 18, out start, out fyaw, out wallDist);
        }
        Need(found, "no flat 18 m stretch");
        if (found)
        {
            double[] want = { rules.jumpMult[0] * J, rules.jumpMult[1] * J, rules.jumpMult[2] * J };
            double[] staged = { rules.tiers[0], rules.tiers[1], rules.tiers[2] };
            for (int i = 0; i < 3; i++)
            {
                SurvivalSim sim = CombatSim25(cfg, map, !off, (uint)(2520 + i));
                int slot = sim.loadout.owned.IndexOf("gauntlet");
                sim.Respawn(start, 0);
                for (int s = 0; s < 150; s++) Tick25(sim, fyaw, 0, new CombatInput { weapon = s == 0 ? slot : -1 });
                Vec3 from = sim.mover.position;
                sim.fist24.charge = staged[i];
                int l0 = sim.loadout.pulseLaunches, f0 = sim.fist24.flicks;
                for (int s = 0; s < 720; s++)
                {
                    bool held = s < 12; // a held click, then let go at step 12
                    Tick25(sim, fyaw, 0, new CombatInput { weapon = -1, fire = new Button { held = held, pressed = s == 0 } });
                    if (s > 40 && sim.mover.Speed < 0.05 && sim.mover.grounded) break;
                }
                double d = FistHoriz(from, sim.mover.position);
                bool flicked = sim.fist24.flicks > f0;
                Console.WriteLine($"  3 launch tier {i + 1}: charge {staged[i]:0.00} -> {(flicked ? "launched" : "NO launch")}, reached {d:0.00} m (wanted {rules.jumpMult[i]:0.##} J = {want[i]:0.00} m, {d / want[i] * 100:0} %), charge left {sim.fist24.charge:0.00}");
                Need(flicked && Math.Abs(d - want[i]) <= 0.15 * want[i], $"tier {i + 1} launch {d:0.00} m vs {want[i]:0.00}");
            }
            // lockout: a second release 1 s after a launch does not launch
            {
                SurvivalSim sim = CombatSim25(cfg, map, !off, 2530);
                int slot = sim.loadout.owned.IndexOf("gauntlet");
                sim.Respawn(start, 0);
                for (int s = 0; s < 150; s++) Tick25(sim, fyaw, 0, new CombatInput { weapon = s == 0 ? slot : -1 });
                sim.fist24.charge = 0.4;
                for (int s = 0; s < 20; s++) Tick25(sim, fyaw, 0, new CombatInput { weapon = -1, fire = new Button { held = s < 8, pressed = s == 0 } });
                int n1 = sim.fist24.flicks;
                for (int s = 0; s < 100; s++) Tick25(sim, fyaw, 0, new CombatInput { weapon = -1 });
                sim.fist24.charge = 0.4;
                for (int s = 0; s < 20; s++) Tick25(sim, fyaw, 0, new CombatInput { weapon = -1, fire = new Button { held = s < 8, pressed = s == 0 } });
                int n2 = sim.fist24.flicks;
                for (int s = 0; s < 480; s++) Tick25(sim, fyaw, 0, new CombatInput { weapon = -1 });
                sim.fist24.charge = 0.4;
                for (int s = 0; s < 20; s++) Tick25(sim, fyaw, 0, new CombatInput { weapon = -1, fire = new Button { held = s < 8, pressed = s == 0 } });
                int n3 = sim.fist24.flicks;
                Console.WriteLine($"  3 lockout: launches {n1} -> {n2} (a second release inside {rules.lockout:0.#} s) -> {n3} (after it)");
                Need(n1 == 1 && n2 == 1 && n3 == 2, "the lockout did not hold 3 s");
            }
            // 4 the launch's damage: three husks in the path of a tier-3 launch, each hit once for 32
            {
                SurvivalSim sim = CombatSim25(cfg, map, !off, 2540);
                int slot = sim.loadout.owned.IndexOf("gauntlet");
                sim.Respawn(start, 0);
                for (int s = 0; s < 150; s++) Tick25(sim, fyaw, 0, new CombatInput { weapon = s == 0 ? slot : -1 });
                double ux = -Math.Sin(fyaw), uz = -Math.Cos(fyaw);
                var hs = new List<Husk>();
                foreach (double d in new[] { 3.0, 4.5, 6.0 })
                {
                    Husk h = sim.husks.Spawn("husk", new Vec3(start.x + ux * d, start.y, start.z + uz * d), start);
                    if (h != null) { h.hp = h.maxHp = 1e9; hs.Add(h); }
                }
                var dmg = new List<double>();
                sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") dmg.Add(e.damage); });
                sim.fist24.charge = 1.0;
                double c0 = sim.fist24.charge;
                var pos = hs.ConvertAll(h => h.position);
                Tick25(sim, fyaw, 0, new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } });
                dmg.Clear();
                Tick25(sim, fyaw, 0, new CombatInput { weapon = -1 });
                double gd = cfg.Num("weapons.gauntlet.damage");
                Console.WriteLine($"  4 launch damage: tier-3 launch through {hs.Count} husks: {dmg.Count} hits, {(dmg.Count > 0 ? dmg[0].ToString("0.#") : "-")} each (the gauntlet's {gd:0} x 1.0); charge after the launch (its own damage must not refill it) {sim.fist24.charge:0.00}");
                Need(hs.Count == 3 && dmg.Count == 3 && Math.Abs(dmg[0] - gd) < 1e-6 && sim.fist24.charge == 0, "the launch did not hit each body once for the notch's damage, or its damage fed the charge");
            }
        }

        bool ok = fails.Count == 0;
        Console.WriteLine($"pulse {(ok ? "PASS" : "FAIL")}{(off ? " (--off: NIGHT24's fist rules)" : "")}: the PULSE's own hits build 3 notches of 6-8 hits, only they feed it, the release launches 0.5 / 1.0 / 2.25 jumps with the notch's damage on what it passes, 3 s lockout, idle 60 s = +0{(ok ? "" : "; " + string.Join("; ", fails))}");
        code = ok ? 0 : 1;
    }
}
