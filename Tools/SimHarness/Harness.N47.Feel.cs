// NIGHT47 builder F (G20, the LT blast's weight): `n47feel`, F's own meters. Hooked without a Program.cs line (a module initializer runs the
// command and exits when argv[1] is "n47feel", Harness.N46.Flow.cs's pattern); it applies the off switches itself (--off47 / --off46 ...).
//   n47feel blast [--seeds N] [--dist d] [--rows r1,r2] [--trace]
//     The PULSE gauntlet's LT blast on one husk standing d m (default 3) ahead on the spawn's open line: a full glove (5 pips), the swap done, the
//     NIGHT30 action waited out, LT pressed once. Measured per seed: the husk's horizontal knockback (the farthest it gets from where it stood, in
//     the 2 s after the press, and where it is when its knockback steps end), the blast's damage, the player's own boost (peak speed), and the
//     husk's hp share taken. G20's done-when: knockback at least 2x today's (the RED number is this command with the config's rows, pulse47 off).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void F47FeelInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n47feel") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string sub = args.Length > 1 ? args[1] : "blast";
            code = sub == "blast" ? F47Blast(args) : sub == "overheal" ? F47Overheal(args) : 2;
            if (code == 2 && sub != "blast" && sub != "overheal") Console.WriteLine("n47feel: unknown subcommand " + sub + " (blast | overheal)");
        }
        catch (Exception e) { Console.WriteLine("n47feel: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string F47Opt(string[] args, string name, string dflt)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : dflt;
    }

    /// <summary>An open floor for the throw: a point on the map's grid (4 m) and a direction (32) where a ray 1 m up runs clear for `len` m and
    /// the floor under the line (every 2 m) is within 0.4 m of the start's height. The survival spawn has no open 4 m line (ArclineOpenDir).</summary>
    static bool F47OpenFloor(SurvivalSim sim, Vec3 near, double len, out Vec3 at, out double ux, out double uz)
    {
        at = near; ux = 0; uz = -1;
        for (int ring = 0; ring <= 12; ring++)
            for (int gx = -ring; gx <= ring; gx++)
                for (int gz = -ring; gz <= ring; gz++)
                {
                    if (Math.Max(Math.Abs(gx), Math.Abs(gz)) != ring) continue;
                    var top = new Vec3(near.x + gx * 4, near.y + 6, near.z + gz * 4);
                    if (!sim.world.RayIntersect(top, new Vec3(0, -1, 0), out RayHit down) || down.distance > 12) continue;
                    double fy = top.y - down.distance;
                    for (int k = 0; k < 32; k++)
                    {
                        double a = k * Math.PI / 16, dx = Math.Sin(a), dz = -Math.Cos(a);
                        var eye = new Vec3(top.x, fy + 1.0, top.z);
                        if (sim.world.RayIntersect(eye, new Vec3(dx, 0, dz), out RayHit h) && h.distance < len) continue;
                        bool flat = true;
                        for (double s = 2; s <= len && flat; s += 2)
                        {
                            var p = new Vec3(top.x + dx * s, top.y, top.z + dz * s);
                            flat = sim.world.RayIntersect(p, new Vec3(0, -1, 0), out RayHit d2) && Math.Abs(top.y - d2.distance - fy) < 0.4;
                        }
                        if (!flat) continue;
                        at = new Vec3(top.x, fy, top.z); ux = dx; uz = dz;
                        return true;
                    }
                }
        return false;
    }

    //   n47feel overheal [--rows overheal47]
    //     G17 / DECIDE47 D-4: a full-health player; a husk 2 m ahead killed every 0.5 s (Loadout.Damage: a player kill, so heal36's spill and the
    //     overheal's kill listener both see it) x 6; then 1 s idle; then 6 s idle more; then a 20-damage hit. Printed: hp and overheal after each
    //     kill and at each mark. RED (row off): the overheal stays 0 (hp at the max). GREEN (row on): +10 a kill to the cap (+50), it holds
    //     holdSeconds, drains decayPerSecond, and the hit takes the overheal first (hp stays 100).
    static int F47Overheal(string[] args)
    {
        string rowsArg = F47Opt(args, "--rows", "");
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        if (rowsArg.Length > 0)
        {
            var o = sim.rowsRegistry.Defaults();
            foreach (string r in rowsArg.Split(',')) o = o.Set(r.Trim(), true);
            sim.rowsOverride = o;
        }
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(4711, null, null);
        if (!F47OpenFloor(sim, map.playerSpawn, 6, out Vec3 sp, out double ux, out double uz)) sp = map.playerSpawn;
        sim.Respawn(sp, 0);
        sim.mover.invulnerable = true;
        Health hl = sim.health;
        var lines = new List<string>();
        void Idle(double s) { for (int t = 0; t < (int)Math.Round(s / sim.step); t++) Tick25(sim, Math.Atan2(-ux, -uz), 0, new CombatInput { weapon = -1 }); }
        Idle(0.5);
        hl.hp = hl.max;
        for (int k = 0; k < 6; k++)
        {
            Husk h = sim.husks.Spawn("husk", new Vec3(sp.x + ux * 2, sp.y, sp.z + uz * 2), sp);
            if (h != null) sim.loadout.Damage(h, 1e6, false, "revolver", sim.Eye);
            Idle(0.5);
            lines.Add($"kill {k + 1}: hp {hl.hp:0.0} over {hl.over:0.0}");
        }
        double atCap = hl.over;
        Idle(1.0);
        double held = hl.over;
        Idle(6.0);
        double drained = hl.over;
        double hpBefore = hl.hp;
        sim.mover.invulnerable = false; // the scripted hit only (the survival round's own enemies never reach the player: invulnerable till now)
        hl.Damage(20, "harness");
        sim.mover.invulnerable = true;
        Idle(0.02);
        double afterHit = hl.over, hpAfter = hl.hp;
        Console.WriteLine($"n47feel overheal rows [{(rowsArg.Length > 0 ? rowsArg : "config")}] on {sim.Overheal47On} max over {sim.Overheal47Max:0}: " + string.Join("; ", lines));
        Console.WriteLine($"n47feel overheal SUMMARY: after 6 close kills over {atCap:0.0}; 1 s later {held:0.0} (the hold); 6 s more {drained:0.0} (the drain); a 20 hit: over {afterHit:0.0}, hp {hpBefore:0.0} -> {hpAfter:0.0}; gained {sim.Overheal47Gained:0.0}, spent {sim.Overheal47Spent:0.0}");
        return 0;
    }

    static int F47Blast(string[] args)
    {
        int seeds = int.Parse(F47Opt(args, "--seeds", "5"), CultureInfo.InvariantCulture);
        double dist = double.Parse(F47Opt(args, "--dist", "3"), CultureInfo.InvariantCulture);
        string rowsArg = F47Opt(args, "--rows", "");
        bool trace = Flag(args, "--trace");
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var far = new List<double>();
        var atEnd = new List<double>();
        var dmgs = new List<double>();
        var peaks = new List<double>();
        int blasted = 0;
        for (int k = 0; k < seeds; k++)
        {
            uint seed = (uint)(4701 + k);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            if (rowsArg.Length > 0)
            {
                var o = sim.rowsRegistry.Defaults();
                foreach (string r in rowsArg.Split(',')) o = o.Set(r.Trim(), true);
                sim.rowsOverride = o;
            }
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            sim.mover.invulnerable = true;
            if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
            int slot = sim.loadout.owned.IndexOf("gauntlet");
            Vec3 sp = map.playerSpawn;
            double ux, uz;
            if (!F47OpenFloor(sim, map.playerSpawn, 22, out sp, out ux, out uz)) { Console.WriteLine("n47feel blast: no open floor of 22 m found"); return 1; }
            sim.Respawn(sp, 0);
            double yaw = Math.Atan2(-ux, -uz);
            if (k == 0) Console.WriteLine($"  open floor at ({sp.x:0.0}, {sp.y:0.0}, {sp.z:0.0}) dir ({ux:0.00}, {uz:0.00}): 22 m clear");
            PulseCharge27 pc = sim.pulse27;
            // the swap to the gauntlet, then 1.5 s idle (the swap and any action done)
            for (int t = 0; t < (int)(1.5 / sim.step); t++) Tick27(sim, yaw, 0, new CombatInput { weapon = t == 0 ? slot : -1 });
            if (pc != null) pc.pips = pc.p.pips;
            Husk h = Pin27(sim, null, sp, ux, uz, dist, 1e6);
            if (h == null) { Console.WriteLine($"n47feel blast seed {seed}: no husk spawned"); continue; }
            h = Pin27(sim, h, sp, ux, uz, dist, 1e6);
            Vec3 start = h.position;
            double hp0 = h.hp;
            int blasts0 = pc != null ? pc.blasts : 0;
            double peak = 0, farthest = 0, endDist = -1;
            bool knockSeen = false;
            for (int t = 0; t < (int)(2.0 / sim.step); t++)
            {
                double py = 0;
                if (t == 0 && dist > 0) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint); // a negative --dist: the husk stands behind, the aim stays on the open line (the ring)
                Tick27(sim, yaw, py, new CombatInput { weapon = -1, altFire = new Button { held = t < 10, pressed = t == 0 } });
                double dx = h.position.x - start.x, dz = h.position.z - start.z;
                double d = Math.Sqrt(dx * dx + dz * dz);
                farthest = Math.Max(farthest, d);
                if (h.knockSteps > 0) knockSeen = true;
                else if (knockSeen && endDist < 0) endDist = d;
                Vec3 v = sim.mover.velocity;
                peak = Math.Max(peak, Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z));
                if (trace && t % 6 == 0) Console.WriteLine($"  t {t * sim.step:0.000} husk d {d:0.00} knock {h.knockSteps} v ({h.velocity.x:0.0},{h.velocity.z:0.0}) player v {peak:0.0}");
            }
            bool ok = pc != null && pc.blasts == blasts0 + 1;
            if (ok) blasted++;
            far.Add(farthest);
            atEnd.Add(endDist < 0 ? farthest : endDist);
            dmgs.Add(hp0 - h.hp);
            peaks.Add(peak);
            Launch36 l36 = sim.loadout.launch36;
            string flight = l36 != null && l36.launches > 0 ? $", launch36 flight end '{l36.lastEnd}' thrown {l36.lastThrown:0.00} m" : ", no launch";
            Console.WriteLine($"n47feel blast seed {seed}: blasted {(ok ? "yes" : "NO")}, husk knockback farthest {farthest:0.00} m, at knock end {(endDist < 0 ? farthest : endDist):0.00} m, damage {hp0 - h.hp:0.0}, player peak speed {peak:0.0} m/s{flight}");
        }
        if (far.Count == 0) { Console.WriteLine("n47feel blast: no measurement"); return 1; }
        far.Sort(); atEnd.Sort();
        Console.WriteLine($"n47feel blast SUMMARY rows [{(rowsArg.Length > 0 ? rowsArg : "config")}] dist {dist:0.0} m: blasted {blasted}/{seeds}, knockback median {far[far.Count / 2]:0.00} m (min {far[0]:0.00}, max {far[far.Count - 1]:0.00}), at knock end median {atEnd[atEnd.Count / 2]:0.00} m, damage {dmgs.Average():0.0}, player peak {peaks.Average():0.0} m/s; gauntlet knockback {cfg.Num("weapons.gauntlet.knockback")} m/s for {cfg.Num("weapons.gauntlet.knockbackTime")} s, blastDamageMult {cfg.Num("design.n27.pulse27.blastDamageMult")}");
        return blasted == seeds ? 0 : 1;
    }
}
