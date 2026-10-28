// NIGHT29 builder B: `boss29 [lantern|glacier|hp] [--seeds n] [--first n] [--dbg]` the NIGHT29 boss rules, headless (Core/Enemies/Lantern29.cs, Glacier29.cs, SkateLane29.cs; design.n29.boss1 / boss2 / bossHp).
// Each fight is the COMPOSED sim and a scripted bot (LanternPilot27 for the wall rides, GlacierPilot28 for the chase) through ScriptedInput, ORGANIC kills only. Lines print RED (a precondition
// is printed with it) and GREEN per seed and the summary. DISPATCH (the lead's Program.cs, ONE line next to N28Boss): N29Boss(cmd, args, ref code);
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N29Boss(string cmd, string[] args, ref int code)
    {
        if (cmd != "boss29") return;
        string which = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = 10, first = 1;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") int.TryParse(args[i + 1], out seeds);
            if (args[i] == "--first") int.TryParse(args[i + 1], out first);
        }
        bool dbg = Array.IndexOf(args, "--dbg") >= 0;
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--set") sets29.Add(args[i + 1]);
        GlacierPilot29.DbgOn = Array.IndexOf(args, "--dodge") >= 0;
        god29 = Array.IndexOf(args, "--god") >= 0; // diagnostic only: the bot never dies (the kill time of the damage rule alone)
        code = 0;
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        if (which == "all" || which == "lantern") code |= Lantern29Command(seeds, first, dbg);
        if (which == "all" || which == "glacier") code |= Glacier29Command(seeds, first, dbg);
        if (which == "all" || which == "hp") code |= BossHp29Command(seeds, first, dbg);
        SurvivalSim.Wyrm24Dir = null;
    }

    /// <summary>The design config with design.n29.bossHp switched on and the three multipliers set (null = leave): the before / after lines of the HP cuts.</summary>
    static GameConfig Cfg29(double? lantern, double? glacier, double? worm, bool bossHpOn)
    {
        string design = File.ReadAllText(Path.Combine(Root, "..", "Design", "unity-config.json"));
        string head = "\"bossHp\": {";
        int at = design.IndexOf(head, StringComparison.Ordinal);
        if (at >= 0)
        {
            int end = design.IndexOf('}', at);
            string blk = design.Substring(at, end - at);
            string nb = blk;
            if (bossHpOn) nb = Regex.Replace(nb, "\"enabled\"\\s*:\\s*false", "\"enabled\": true");
            if (lantern.HasValue) nb = Regex.Replace(nb, "\"lanternMult\"\\s*:\\s*[0-9.]+", "\"lanternMult\": " + lantern.Value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture));
            if (glacier.HasValue) nb = Regex.Replace(nb, "\"glacierMult\"\\s*:\\s*[0-9.]+", "\"glacierMult\": " + glacier.Value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture));
            if (worm.HasValue) nb = Regex.Replace(nb, "\"wormMult\"\\s*:\\s*[0-9.]+", "\"wormMult\": " + worm.Value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture));
            design = design.Substring(0, at) + nb + design.Substring(end);
        }
        // --set block.key=value (a tuning probe: the first `"key": number` after the block's name inside design.n29)
        foreach (string kv in sets29)
        {
            string[] a = kv.Split('=');
            string[] bk = a[0].Split('.');
            int n29 = design.IndexOf("\"n29\": {", StringComparison.Ordinal);
            int b0 = design.IndexOf("\"" + bk[0] + "\": {", n29, StringComparison.Ordinal);
            var rx = new Regex("(\"" + bk[1] + "\"\\s*:\\s*)[-0-9.a-z]+");
            Match m = rx.Match(design, b0);
            if (m.Success) design = design.Substring(0, m.Index) + m.Groups[1].Value + a[1] + design.Substring(m.Index + m.Length);
        }
        return GameConfig.FromText(File.ReadAllText(Path.Combine(Root, "config.json")), design);
    }

    static List<string> sets29 = new List<string>();

    sealed class LanternRow29
    {
        public int minTellSteps = int.MaxValue, seed, steps, deaths, rides, fullHits, lightHits, cycles, floorTicks;
        public bool killed;
        public double hpEnd, floorDamage, minSlow = 1, maxHp;
    }

    /// <summary>mode 0: the wall-riding bot (LanternPilot27's movement, fires at the box on ANY face whenever the ray lands); 1: the floor-standing bot (never moves off the floor, fires all the time).</summary>
    static LanternRow29 LanternRun29(GameConfig cfg, uint seed, int capSteps, int mode, bool n29On, bool dbg)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n27Override = new N27Blocks { boss1 = true };
        sim.n29Override = new N29Blocks { boss1 = n29On };
        sim.n26Override = N26Blocks.Off;
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var row = new LanternRow29 { seed = (int)seed };
        if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yaw)) { row.steps = -1; return row; }
        sim.Respawn(spawn, yaw);
        Lantern27 l = sim.lantern27;
        row.maxHp = l.body.maxHp;
        var si = (ScriptedInput)sim.input;
        var pilot = new LanternPilot29 { hand = new Mulberry32(seed * 7919u + 13u) };
        double yawL = yaw * Math.PI / 180, pitchL = 0;
        int step = 0, lastStage = -1;
        while (l.Alive && step < capSteps)
        {
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            if (mode == 0)
            {
                pilot.Step(sim, yawL, step);
                yawL = pilot.Move.yaw; pitchL = pilot.Move.pitch;
                CombatInput cb = pilot.Combat;
                // the bot also shoots a face that is NOT lit (any face, any time the ray lands): the rule under test is what that does
                Vec3 fw = MovementMath.WishDirection(0, 1, yawL);
                double cp = JsMath.Cos(pitchL);
                Vec3 dir = new Vec3(fw.x * cp, JsMath.Sin(pitchL), fw.z * cp);
                double t = l.Raycast(eye, dir);
                if (t >= 0 && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < t)) cb.fire = new Skygrind.Core.Button { held = true, pressed = step % 2 == 0 };
                si.SetLook(yawL, pitchL);
                si.Set(pilot.Move, cb);
            }
            else
            {
                Vec3 d = l.Centre - eye;
                double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                si.SetLook(wy, wp);
                int gun = sim.loadout.owned.IndexOf("revolver");
                si.Set(new InputFrame { yaw = wy, pitch = wp }, new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Skygrind.Core.Button { held = true, pressed = step % 2 == 0 } });
            }
            sim.Step();
            step++;
            if (god29 || mode == 1) sim.health.hp = sim.health.max; // the floor-standing bot is IMMORTAL here (the Player reel run survives its 40 s): the rule, not the deaths, must make it slow
            if (sim.lantern29 != null)
            {
                int st = (int)sim.lantern29.Stage;
                if (st != lastStage) { if (st == (int)Lantern29Stage.Tell) row.cycles++; lastStage = st; }
                row.minSlow = Math.Min(row.minSlow, sim.Lantern29SlowScale);
            }
            if (sim.lantern29 != null && sim.lantern29.FloorLive) { int k = (int)sim.mover.state; liveState29[k < 6 ? k : 5]++; }
            if (dbg && step % 1200 == 0) Console.WriteLine($"    t={step * dt:0}s {l.phase} hp {l.HpFrac:0.00} stage {sim.lantern29?.Stage} state {sim.mover.state} hp {sim.health.hp:0} full {sim.lantern29?.fullHits} light {sim.lantern29?.lightHits}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.BossRoomBegin("hulk", out spawn, out yaw);
                sim.Respawn(spawn, yaw);
            }
        }
        row.steps = step;
        row.killed = !l.Alive;
        row.hpEnd = l.HpFrac;
        row.rides = pilot.rides;
        foreach (Lantern27Log lg in l.log) if (!lg.resolve) row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(lg.seconds, dt));
        if (dbg) Console.WriteLine($"    lava ground steps not ready because: speed {pilot.whySpeed}, runway {pilot.whyRun}, cooldown {pilot.whyCool}, wall gone {pilot.whyWall}, ready {pilot.whyOk}");
        if (sim.lantern29 != null) { row.fullHits = sim.lantern29.fullHits; row.lightHits = sim.lantern29.lightHits; row.floorDamage = sim.lantern29.floorDamage; row.floorTicks = sim.lantern29.floorTicks; }
        return row;
    }

    static bool god29;
    static List<int> lanternTells29 = new List<int>();
    static HashSet<Glacier28Hazard> hitSeen29 = new HashSet<Glacier28Hazard>();
    static int[] liveState29 = new int[6];

    static double Median29(List<double> v)
    {
        if (v.Count == 0) return 0;
        v.Sort();
        return v[v.Count / 2];
    }

    static int Lantern29Command(int seeds, int first, bool dbg)
    {
        GameConfig cfg = Cfg29(null, null, null, true); // the pool cut is measured WITH design.n29.bossHp on (the row stays off in the file until the lead flips it)
        double dt = cfg.Dt;
        var fails = new List<string>();
        // RED 1: the block off (NIGHT28's rule): the floor-standing bot does ZERO damage (the old shield: nothing hits a face that is not lit) and the lantern has no cycle
        {
            LanternRow29 r = LanternRun29(cfg, 3, 60 * 120, 1, false, dbg);
            Console.WriteLine($"  RED block OFF (n29.boss1 false; precondition: floor-standing bot, pistol, 60 s): hp left {r.hpEnd:0.00} (1.00 = untouched), cycles {r.cycles}, floor damage {r.floorDamage:0}");
            if (r.hpEnd < 0.999 || r.cycles != 0 || r.floorDamage != 0) fails.Add("the block off changed the old fight");
        }
        // the wall-riding bot: the rule's GREEN
        var riders = new List<double>();
        double firstFloor = 0;
        for (uint sd = (uint)first; sd < (uint)(first + seeds); sd++)
        {
            LanternRow29 r = LanternRun29(cfg, sd, 240 * 120, 0, true, dbg);
            if (r.steps < 0) { fails.Add($"seed {sd}: BossRoomBegin failed"); continue; }
            Console.WriteLine($"  GREEN rider seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED (hp " + r.hpEnd.ToString("0.00") + ")")} at {r.steps * dt:0.0} s; deaths {r.deaths}, rides {r.rides}, full hits {r.fullHits}, light hits {r.lightHits}, cycles {r.cycles}, floor damage {r.floorDamage:0} ({r.floorTicks} ticks), lowest world scale {r.minSlow:0.00}");
            lanternTells29.Add(r.minTellSteps);
            if (r.killed) riders.Add(r.steps * dt); else fails.Add($"rider seed {sd} not killed in 240 s");
            if (sd == (uint)first) firstFloor = r.steps * dt;
        }
        Console.WriteLine($"  (live-phase steps by Mover state, all riders: Ground {liveState29[0]}, Air {liveState29[1]}, Slide {liveState29[2]}, Dash {liveState29[3]}, WallRide {liveState29[4]}, other {liveState29[5]})");
        double med = Median29(riders);
        int lminTell = int.MaxValue;
        foreach (int m in lanternTells29) lminTell = Math.Min(lminTell, m);
        if (lminTell == int.MaxValue) fails.Add("no lantern tell was logged"); else if (lminTell < MovementMath.StepsFor(0.8, dt)) fails.Add("a lantern attack tell under 0.8 s");
        // the floor-standing bot (light damage only, and the floor hurts it): at least 3x the rider's median (or not killed inside 3x + cap)
        double riderBest = riders.Count > 0 ? Math.Min(riders[0], riders[riders.Count - 1]) : 30; riders.Sort(); riderBest = riders.Count > 0 ? riders[0] : 30; // the fastest rider (no deaths): the Player rider dies less than the lag bot
        int cap = (int)(Math.Max(riderBest, 30) * 3.0 / dt) + 1;
        var floors = new List<double>();
        int floorKilled = 0;
        double hpLeftSum = 0;
        int nFloor = Math.Min(seeds, 3);
        for (uint sd = (uint)first; sd < (uint)(first + nFloor); sd++)
        {
            LanternRow29 r = LanternRun29(cfg, sd, cap, 1, true, dbg);
            Console.WriteLine($"  RED floor-standing bot (immortal) seed {sd} (cap {cap * dt:0} s = 3x the fastest rider {riderBest:0} s): {(r.killed ? "KILLED at " + (r.steps * dt).ToString("0.0") + " s" : "NOT killed (hp left " + r.hpEnd.ToString("0.00") + ")")}; deaths {r.deaths}, light hits {r.lightHits}, full hits {r.fullHits}, floor damage {r.floorDamage:0}");
            if (r.killed) { floorKilled++; floors.Add(r.steps * dt); }
            hpLeftSum += r.hpEnd;
            if (r.fullHits > 0) fails.Add("a floor-standing bot landed a full hit");
        }
        if (floorKilled > 0 && Median29(floors) < riderBest * 3) fails.Add($"a floor bot killed at {Median29(floors):0} s, under 3x the riders' median");
        if (riders.Count > 0 && (med < 45 || med > 70)) fails.Add($"the riders' median {med:0} s is outside 45-70 s");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss29 lantern {(fails.Count == 0 ? "PASS" : "FAIL")}: wall-riding bot median {med:0} s over {riders.Count} kills (target 45-70 s); shortest tell {(lminTell == int.MaxValue ? 0 : lminTell * dt):0.00} s; floor-standing bot killed {floorKilled} of {nFloor} within 3x ({cap * dt:0} s), mean hp left {(nFloor > 0 ? hpLeftSum / nFloor : 0):0.00}");
        return fails.Count == 0 ? 0 : 1;
    }

    sealed class GlacierRow29
    {
        public double speedSum, bossSpeedSum; public int viewSteps; public int exposedHits, lightHits, nearSteps, sameSteps, seed, steps, deaths, windows, exposedWindows, volleys, frostTells, frostSlow, charges, switches, shardHits, attackHits, lockedSteps, playSteps, enragedSteps, minTellSteps = int.MaxValue;
        public bool killed;
        public double hpEnd, gapMin = 1e9, gapMax = -1e9, hp70At = -1;
    }

    static SurvivalSim Glacier29Sim(GameConfig cfg, uint seed, bool n29On)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n27Override = new N27Blocks { pulse27 = true };
        sim.n29Override = new N29Blocks { boss2 = n29On };
        sim.n26Override = new N26Blocks { surfaces = true, level2 = true };
        sim.bosses25Override = true;
        sim.glacier28Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    /// <summary>mode 0: the catching-up lag bot (GlacierPilot29); 1: the camper (stands at the spawn, pistol at the boss whenever its ray lands: it never closes the gap).</summary>
    static GlacierRow29 GlacierRun29(GameConfig cfg, uint seed, int capSteps, int mode, bool n29On, bool dbg)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = Glacier29Sim(cfg, seed, n29On);
        var row = new GlacierRow29 { seed = (int)seed };
        if (!sim.Glacier28Begin()) { row.steps = -1; return row; }
        sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
        Glacier28 g = sim.glacier28;
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        var si = (ScriptedInput)sim.input;
        var pilot = new GlacierPilot29 { hand = new Mulberry32(seed * 7919u + 13u) };
        var hurtBy = new Dictionary<string, double>();
        sim.events.On<HurtEvent>("hurt", e => { string k = e.source ?? "?"; hurtBy[k] = (hurtBy.TryGetValue(k, out double v0) ? v0 : 0) + e.amount; if (dbg) Console.WriteLine($"      hurt {e.amount:0.0} by {k} hp now {e.hp:0}"); });
        double yawL = sim.Glacier28SpawnYawDeg * Math.PI / 180;
        int step = 0;
        bool hold = false;
        while (g.Alive && step < capSteps)
        {
            if (mode == 0 || mode == 2)
            {
                pilot.Step(sim, yawL, step);
                yawL = pilot.Move.yaw;
                InputFrame mv = pilot.Move;
                if (mode == 2)
                {
                    // the TRACKER: keeps the boss at 28-40 m (holds still under 28 m until it has run to 34 m) and shoots the pistol whenever its ray clears the island
                    if (g.gap < 28) hold = true; else if (g.gap > 34) hold = false;
                    if (hold) { mv.moveX = 0; mv.moveZ = 0; mv.sprint = default; mv.dash = default; mv.slide = default; }
                    Vec3 me2 = sim.mover.position, eye2 = new Vec3(me2.x, me2.y + 1.6, me2.z);
                    Vec3 dn2 = Vec3.Normalize(g.box.c - eye2);
                    double t2 = g.Raycast(eye2, dn2);
                    bool see2 = t2 >= 0 && !(sim.world.RayIntersect(eye2, dn2, out RayHit wh2) && wh2.distance < t2);
                    double dd = g.gap;
                    if (g.phase.StartsWith("p") && see2 && dd >= 25 && dd <= 40) row.viewSteps++;
                }
                si.SetLook(mv.yaw, mv.pitch);
                si.Set(mv, pilot.Combat);
            }
            else
            {
                Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = g.box.c - eye;
                double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                si.SetLook(wy, wp);
                int gun = sim.loadout.owned.IndexOf("revolver");
                Vec3 dn = Vec3.Normalize(d);
                double tHit = g.Raycast(eye, dn);
                bool see = tHit >= 0 && !(sim.world.RayIntersect(eye, dn, out RayHit wh) && wh.distance < tHit);
                si.Set(new InputFrame { yaw = wy, pitch = wp }, new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Skygrind.Core.Button { held = see, pressed = see && step % 4 == 0 } });
            }
            sim.Step();
            step++;
            if (g.phase.StartsWith("p"))
            {
                row.playSteps++;
                row.speedSum += JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z);
                row.bossSpeedSum += g.CurrentSpeed;
                if (g.gap <= 10 && g.gap >= -1.5) { row.nearSteps++; if (sim.glacier29 != null && sim.glacier29.PlayerLane == sim.glacier29.BossLane) row.sameSteps++; }
                row.gapMin = Math.Min(row.gapMin, g.gap);
                row.gapMax = Math.Max(row.gapMax, g.gap);
                if (sim.glacier29 != null && sim.glacier29.lane.Locked) row.lockedSteps++;
                if (sim.glacier29 != null && sim.glacier29.Enraged) row.enragedSteps++;
            }
            if (dbg) foreach (Glacier28Hazard hz in g.hazards) if (hz.hit && hitSeen29.Add(hz)) Console.WriteLine($"      hit by {hz.kind} t={step * dt:0.0} lane {sim.glacier29?.PlayerLane} lat {g.playerLat:0.0} band {hz.lat0:0.0}..{hz.lat1:0.0} state {sim.mover.state} speed {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.0}");
            if (row.hp70At < 0 && g.HpFrac < 0.7) row.hp70At = step * dt;
            if (dbg && step % 1200 == 0) Console.WriteLine($"    t={step * dt:0}s {g.phase} hp {g.HpFrac:0.00} gap {g.gap:0.0} lane p{sim.glacier29?.PlayerLane}/b{sim.glacier29?.BossLane} win {g.WindowOpen} windows {g.stats["windows"]} volleys {sim.glacier29?.volleys} frost {sim.glacier29?.frostTells} charges {sim.glacier29?.charges} enraged {sim.glacier29?.Enraged} pat {sim.glacier29?.PatienceFrac:0.00} hp {sim.health.hp:0}");
            if (god29) sim.health.hp = sim.health.max;
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                if (dbg) Console.WriteLine($"      DEATH t={step * dt:0.0} pos ({sim.mover.position.x - g.origin.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z - g.origin.z:0.0}) state {sim.mover.state} flow {sim.flow} hp {sim.health.hp:0} lat {g.playerLat:0.0} gap {g.gap:0.0}");
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Glacier28Begin();
                sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
            }
        }
        if (dbg) Console.WriteLine("    hurt by: " + string.Join(", ", System.Linq.Enumerable.Select(hurtBy, kv => $"{kv.Key} {kv.Value:0}")));
        row.steps = step;
        row.killed = !g.Alive;
        row.hpEnd = g.HpFrac;
        row.windows = g.stats["windows"];
        row.attackHits = g.stats["attackHits"];
        if (sim.glacier29 != null)
        {
            row.exposedWindows = sim.glacier29.exposedWindows; row.exposedHits = sim.glacier29.exposedHits; row.lightHits = sim.glacier29.lightHits; row.volleys = sim.glacier29.volleys; row.frostTells = sim.glacier29.frostTells; row.frostSlow = sim.glacier29.frostSlowSteps;
            row.charges = sim.glacier29.charges; row.switches = sim.glacier29.lane.switches;
        }
        foreach (Glacier28Log l in g.log) if (!l.resolve) row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(l.seconds, dt));
        return row;
    }

    static int Glacier29Command(int seeds, int first, bool dbg)
    {
        GameConfig cfg = Cfg29(null, null, null, true);
        double dt = cfg.Dt;
        var fails = new List<string>();
        // RED 1: the block off: the camper's pistol does nothing (NIGHT28's rule: guns only crack the glaze)
        {
            GlacierRow29 r = GlacierRun29(cfg, 3, 60 * 120, 1, false, dbg);
            Console.WriteLine($"  RED block OFF (n29.boss2 false; precondition: camper, pistol, 60 s): bar {r.hpEnd:0.00} (1.00 = untouched)");
            if (r.hpEnd < 0.999) fails.Add("the block off changed the old fight");
        }
        // the hit rule, pure: pistol hits outside a window stop at the floor (phases crossed one third at a time); a gauntlet hit outside is the light share too; a gun inside a window has no floor
        {
            SurvivalSim sim = Glacier29Sim(cfg, 3, true);
            sim.Glacier28Begin();
            sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
            Glacier28 g = sim.glacier28;
            var si0 = (ScriptedInput)sim.input;
            for (int i = 0; i < 40 * 120 && g.phase != "p1"; i++) { si0.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
            double floor = cfg.Num("design.n29.boss2.otherGunFloor"), third = g.body.maxHp / g.p.hitsToKill;
            g.Filter(25, "gauntlet", default, out bool bg);
            double gl = g.Filter(25, "gauntlet", default, out _), pl = g.Filter(25, "revolver", default, out _);
            for (int round = 0; round < 40 && g.Alive; round++)
            {
                for (int k = 0; k < 400; k++) { double d = g.Filter(25, "revolver", default, out bool b); if (!b) g.body.Damage(d, 0); }
                for (int i = 0; i < 6 * 120; i++) { si0.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
            }
            bool ok = g.Alive && Math.Abs(g.HpFrac - floor) < 1e-6 && Math.Abs(gl - pl) < 1e-9 && Math.Abs(pl - third * cfg.Num("design.n29.boss2.otherGunFraction")) < 1e-9;
            Console.WriteLine($"  hit rule (no window): a pistol / a glove hit land {pl:0.0} / {gl:0.0} of a third ({third:0}); after thousands of pistol hits across the phases the bar stops at {g.HpFrac:0.00} (floor {floor:0.00}), boss {(g.Alive ? "alive" : "DEAD")}: {(ok ? "OK" : "WRONG")}");
            if (!ok) fails.Add("the other-gun rule or its floor is wrong");
        }
        // RED 2: the camper with the block on: the bar must stay above 70 % at 180 s (and never under the floor)
        double camperMin = 1;
        for (uint sd = (uint)first; sd < (uint)first + Math.Min(seeds, 3); sd++)
        {
            GlacierRow29 r = GlacierRun29(cfg, sd, 180 * 120, 1, true, dbg);
            camperMin = Math.Min(camperMin, r.hpEnd);
            Console.WriteLine($"  RED camper seed {sd} (stays at the spawn, pistol on sight, 180 s): bar {r.hpEnd:0.00} ({(r.killed ? "KILLED (wrong)" : "alive")}), gap {r.gapMin:0}..{r.gapMax:0} m, exposed windows {r.exposedWindows}, enraged {(r.playSteps > 0 ? 100.0 * r.enragedSteps / r.playSteps : 0):0} % of the fight, volleys {r.volleys}, deaths {r.deaths}");
            if (r.killed) fails.Add("a camper killed the glacier");
        }
        // RED 3: the TRACKER keeps the boss in view at 25-40 m and shoots with the pistol for 180 s: the bar must stay at or above the floor and the patience gauge must enrage the boss
        {
            double floor29 = cfg.Num("design.n29.boss2.otherGunFloor");
            double lowest = 1;
            int enr = 0;
            for (uint sd = (uint)first; sd < (uint)first + 2; sd++)
            {
                GlacierRow29 r = GlacierRun29(cfg, sd, 180 * 120, 2, true, dbg);
                lowest = Math.Min(lowest, r.hpEnd);
                enr += r.enragedSteps;
                Console.WriteLine($"  RED tracker seed {sd} (boss in view {r.viewSteps * dt:0} s of 180 s at 25-40 m, pistol only): bar {r.hpEnd:0.00} (floor {floor29:0.00}), {(r.killed ? "KILLED (wrong)" : "alive")}, enraged {(r.playSteps > 0 ? 100.0 * r.enragedSteps / r.playSteps : 0):0} % of the fight, gap {r.gapMin:0}..{r.gapMax:0} m, light hits {r.lightHits}, exposed windows {r.exposedWindows}, deaths {r.deaths}");
                if (r.killed) fails.Add("the tracker killed the glacier");
                if (r.hpEnd < floor29 - 1e-6) fails.Add("the tracker took the bar under the floor");
                if (r.viewSteps < 5 * 120) fails.Add("the tracker had the boss in view under 5 s (the test shows nothing)");
                if (r.enragedSteps == 0) fails.Add("the patience gauge never enraged the boss for the tracker");
            }
            Console.WriteLine($"  tracker summary: lowest bar {lowest:0.00} at 180 s (floor {floor29:0.00}), enraged steps {enr}");
        }
        if (camperMin <= 0.70) fails.Add($"the camper took the bar to {camperMin:0.00} (needs above 0.70 at 180 s)");
        var times = new List<double>();
        int killed = 0;
        double sum = 0;
        int minTell = int.MaxValue;
        for (uint sd = (uint)first; sd < (uint)first + seeds; sd++)
        {
            GlacierRow29 r = GlacierRun29(cfg, sd, 240 * 120, 0, true, dbg);
            if (r.steps < 0) { fails.Add($"seed {sd}: Glacier28Begin failed"); continue; }
            Console.WriteLine($"  GREEN catch-up bot seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED (bar " + r.hpEnd.ToString("0.00") + ")")} at {r.steps * dt:0.0} s; deaths {r.deaths}; exposed windows {r.exposedWindows} ({r.exposedHits} gun hits inside, {r.lightHits} light), lane switches {r.switches}, locked on a lane {(r.playSteps > 0 ? 100.0 * r.lockedSteps / r.playSteps : 0):0} %; mean speed {(r.playSteps > 0 ? r.speedSum / r.playSteps : 0):0.0} vs boss {(r.playSteps > 0 ? r.bossSpeedSum / r.playSteps : 0):0.0} m/s; within 10 m {r.nearSteps * dt:0.0} s (same lane {r.sameSteps * dt:0.0} s), volleys {r.volleys}, frost {r.frostTells} ({r.frostSlow} steps slowed), charges {r.charges}, attack hits {r.attackHits}; gap {r.gapMin:0}..{r.gapMax:0} m; bar under 70 % at {r.hp70At:0} s");
            if (r.killed) { killed++; sum += r.steps * dt; times.Add(r.steps * dt); } else fails.Add($"seed {sd} not killed in 240 s");
            minTell = Math.Min(minTell, r.minTellSteps);
        }
        double med = Median29(times);
        if (killed > 0 && (med < 80 || med > 120)) fails.Add($"median kill time {med:0} s is outside 80-120 s");
        if (minTell == int.MaxValue) fails.Add("no attack tell was logged");
        else if (minTell < MovementMath.StepsFor(0.8, dt)) fails.Add("an attack tell under 0.8 s");
        if (cfg.Num("design.n29.boss2.frost.tellSeconds") < 0.8) fails.Add("the frost trail tell is under 0.8 s");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss29 glacier {(fails.Count == 0 ? "PASS" : "FAIL")}: catch-up bot {killed} of {seeds} killed, median {med:0} s (target 80-120 s); camper lowest bar at 180 s {camperMin:0.00} (needs above 0.70); shortest tell {(minTell == int.MaxValue ? 0 : minTell * dt):0.00} s");
        return fails.Count == 0 ? 0 : 1;
    }
    /// <summary>The worm through the composed sim with the human-lag bot (N24's WyrmRig / WyrmBot), design.n29.bossHp.wormMult as given; the cut rides on n29.boss1.</summary>
    static (int killed, double meanSteps, int deaths) WormRun29(GameConfig cfg, int seeds, int first)
    {
        int killed = 0, deaths = 0;
        double sum = 0;
        for (uint sd = (uint)first + 2; sd < (uint)(first + 2 + seeds); sd++)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n24Override = new N24Blocks { wyrm = true };
            sim.n29Override = new N29Blocks { boss1 = true };
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(sd, null, null);
            sim.husks.frozen = true;
            if (!sim.Wyrm24Begin()) return (0, 0, 0);
            sim.Respawn(sim.Wyrm24Spawn, 0);
            var rig = new WyrmRig(sim);
            var bot = new WyrmBot(true);
            while (sim.wyrm24.Alive && rig.step < 12 * 60 * 120)
            {
                InputFrame f = bot.Next(rig, out bool fire);
                rig.Step(f, fire);
            }
            if (!sim.wyrm24.Alive) { killed++; sum += rig.step; }
            deaths += rig.deaths;
        }
        return (killed, killed > 0 ? sum / killed : 0, deaths);
    }

    /// <summary>The boss HP cuts, before / after: the lantern (the wall-riding bot), the glacier (the catch-up bot: its damage is pool-relative, so a cut changes nothing) and the worm (the composed human-lag bot).</summary>
    static int BossHp29Command(int seeds, int first, bool dbg)
    {
        double dt = Config().Dt;
        var fails = new List<string>();
        GameConfig on = Cfg29(null, null, null, true); // the design values with the row on
        double lm = on.Num("design.n29.bossHp.lanternMult"), gm = on.Num("design.n29.bossHp.glacierMult"), wm = on.Num("design.n29.bossHp.wormMult");
        GameConfig before = Cfg29(1.0, 1.0, 1.0, true);
        int n = Math.Min(seeds, 4);
        // the lantern
        foreach (var (label, cfg) in new[] { ("before x1.00", before), ($"after x{lm:0.00}", on) })
        {
            var t = new List<double>();
            int k = 0;
            for (uint sd = (uint)first; sd < (uint)(first + n); sd++)
            {
                LanternRow29 r = LanternRun29(cfg, sd, 300 * 120, 0, true, dbg);
                if (r.killed) { k++; t.Add(r.steps * dt); }
            }
            Console.WriteLine($"  lantern {label} (pool {(label.StartsWith("b") ? 1.0 : lm):0.00}): {k} of {n} killed, median {Median29(t):0} s (band 45-70 s)");
            if (!label.StartsWith("b") && (k < n || Median29(t) < 45 || Median29(t) > 70)) fails.Add("the lantern cut misses the band");
        }
        // the glacier: x1.00 and x0.50 give the same time (every hit is a share of the pool)
        foreach (var (label, cfg) in new[] { ("before x1.00", before), ("probe x0.50", Cfg29(null, 0.5, null, true)) })
        {
            var t = new List<double>();
            int k = 0;
            for (uint sd = (uint)first; sd < (uint)(first + n); sd++)
            {
                GlacierRow29 r = GlacierRun29(cfg, sd, 240 * 120, 0, true, dbg);
                if (r.killed) { k++; t.Add(r.steps * dt); }
            }
            Console.WriteLine($"  glacier {label}: {k} of {n} killed, median {Median29(t):0} s (band 80-120 s; the chosen cut is x{gm:0.00}: its damage is a share of the pool, so the pool scale cancels)");
        }
        // the worm
        {
            var b = WormRun29(before, Math.Min(seeds, 3), first);
            var a = WormRun29(on, Math.Min(seeds, 3), first);
            double bm = b.meanSteps * dt, am = a.meanSteps * dt;
            Console.WriteLine($"  worm before x1.00: {b.killed} killed, mean {(int)(bm / 60)}:{bm % 60:00}, bot deaths {b.deaths}");
            Console.WriteLine($"  worm after  x{wm:0.00}: {a.killed} killed, mean {(int)(am / 60)}:{am % 60:00}, bot deaths {a.deaths} (band 3:00-4:00)");
            if (a.killed == 0 || am < 180 || am > 240) fails.Add($"the worm cut misses the band (mean {am:0} s)");
        }
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss29 hp {(fails.Count == 0 ? "PASS" : "FAIL")}: lantern x{lm:0.00}, glacier x{gm:0.00}, worm x{wm:0.00}");
        return fails.Count == 0 ? 0 : 1;
    }
}
