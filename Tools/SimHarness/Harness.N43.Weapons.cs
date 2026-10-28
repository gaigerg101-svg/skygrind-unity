// NIGHT43 builder W1 (the weapons): W1's headless judges. ONE entry method, N43Weapons(cmd, args, ref code) (declared and called in Program.cs).
// Every sub-command runs the row OFF (NIGHT42's game: the design's NIGHT42 rows, every NIGHT43 block off: the before) and ON (only the NIGHT43
// block under test forced through sim.n43Override: the after) and prints both.
//   n43weap gauntlet   G17 gauntlet43: fire held with the PULSE gauntlet against a bag 1.6 m ahead for 8 s: the press-to-next-swing gap
//                      (meleeSwing to meleeSwing), the damage a hit, DPS. GREEN: OFF gauntlet42's 1.8 s (216 steps), ON
//                      design.n43.gauntlet43.swingSeconds (1.25 s, 150 steps).
//   n43weap band       G17: THE ICEBREAKER's skilled bot band (NIGHT42's n42brawl band: the design's NIGHT42 rows, brawl42 and gauntlet42 on)
//                      with gauntlet43 forced on (--seeds N, default 10; --fusion: fusion43 on too). GREEN: p50 and p90 inside 40-70 s.
//                      `n43weap freeze` re-runs NIGHT42's `n42hit brawl` ledger with gauntlet43 on (the gauntlet's share of a minute).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    /// <summary>NIGHT43 W1: the NIGHT43 blocks Brawl40Sim hands the sim (null: the design's).</summary>
    static N43Blocks? Brawl40N43;

    static partial void N43Weapons(string cmd, string[] args, ref int code)
    {
        if (cmd != "n43weap") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int fails = 0;
        if (what == "gauntlet" || what == "all") fails += W43Gauntlet();
        if (what == "parry" || what == "all") fails += W43Parry();
        if (what == "globs" || what == "parry" || what == "all") fails += W43Globs();
        if (what == "fusion" || what == "all") fails += W43Fusion();
        if (what == "slug" || what == "all") fails += W43Slug();
        if (what == "torch" || what == "all") fails += W43Torch();
        if (what == "band") fails += W43Band(ArgInt36(args, "--seeds", 10), Array.IndexOf(args, "--fusion") >= 0, Array.IndexOf(args, "--parry") >= 0);
        if (what == "freeze") fails += W43Freeze(ArgInt36(args, "--seeds", 3));
        Console.WriteLine($"n43weap {what} {(fails == 0 ? "GREEN" : "RED")}");
        code = fails == 0 ? 0 : 1;
    }

    /// <summary>A headless survival run with the game's design rows (NIGHT42's as they are) and the NIGHT43 blocks `b` (the before: N43Blocks.Off).</summary>
    static SurvivalSim W43Sim(GameConfig cfg, SurvivalMap map, N43Blocks b, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n43Override = b;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(seed, null, null);
        sim.mover.invulnerable = true;
        return sim;
    }

    // ---- G17 gauntlet43 ----------------------------------------------------------------------------------------------------------------------

    static (List<int> swings, double dmg) W43GauntletRun(GameConfig cfg, SurvivalMap map, bool on)
    {
        SurvivalSim sim = W43Sim(cfg, map, on ? new N43Blocks { gauntlet43 = true } : N43Blocks.Off, 4301);
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Vec3 sp = map.playerSpawn;
        int s = 0;
        var swings = new List<int>();
        double dmg = 0;
        sim.events.On<MeleeEvent>("meleeSwing", e => { if (e.weapon == "gauntlet") swings.Add(s); });
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") dmg = e.damage; });
        Husk h = null;
        for (s = 0; s < (int)(8 / sim.step); s++)
        {
            h = Pin27(sim, h, sp, ux, uz, 1.6, 1e9);
            double py = 0;
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            Tick27(sim, yaw, py, new CombatInput { weapon = s == 0 ? slot : -1, fire = new Button { held = true, pressed = s == 0 } });
        }
        return (swings, dmg);
    }

    static int W43Gauntlet()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = cfg.Dt;
        bool g42 = !N42Blocks.ForceOff && cfg.Design("n42.gauntlet42.enabled");
        int wantOff = MovementMath.StepsFor(g42 ? cfg.Num("design.n42.gauntlet42.swingSeconds") : cfg.Num("design.n27.pulse27.swingSeconds"), dt);
        int wantOn = MovementMath.StepsFor(cfg.Num("design.n43.gauntlet43.swingSeconds"), dt);
        int fails = 0;
        Console.WriteLine($"n43weap gauntlet: the PULSE swing held 8 s at a bag 1.6 m ahead; NIGHT42's swing {(g42 ? "gauntlet42" : "pulse27")} -> gauntlet43");
        foreach (bool on in new[] { false, true })
        {
            var (sw, dmg) = W43GauntletRun(cfg, map, on);
            var gaps = new List<int>();
            for (int i = 1; i < sw.Count; i++) gaps.Add(sw[i] - sw[i - 1]);
            int want = on ? wantOn : wantOff;
            bool ok = gaps.Count > 0 && gaps.TrueForAll(g => g == want);
            if (!ok) fails++;
            string gs = gaps.Count == 0 ? "none" : string.Join(",", gaps);
            Console.WriteLine($"  {(ok ? "PASS" : "RED ")} gauntlet43 {(on ? "ON " : "OFF")}: {sw.Count} swings in 8 s, press-to-next-swing gaps {gs} steps = {(gaps.Count > 0 ? gaps[0] * dt : 0):0.000} s (want {want} = {want * dt:0.000} s); {dmg:0} dmg a hit, {(gaps.Count > 0 ? dmg / (gaps[0] * dt) : 0):0.0} dps");
        }
        return fails;
    }

    /// <summary>G17: THE ICEBREAKER's skilled band (n42brawl's) with gauntlet43 (and fusion43 when asked) forced on.</summary>
    static int W43Band(int seeds, bool fusion, bool parry = false)
    {
        Brawl40N42 = null; // the design's NIGHT42 rows (brawl42, gauntlet42 on at the NIGHT42 freeze)
        int fails = 0;
        foreach (bool on in new[] { false, true })
        {
            Brawl40N43 = on ? new N43Blocks { gauntlet43 = true, fusion43 = fusion, parry43 = parry } : N43Blocks.Off;
            Console.WriteLine($"n43weap band: THE ICEBREAKER, skilled pilot, the design's NIGHT42 rows, NIGHT43 {Brawl40N43.Value}");
            int f = Brawl42Band(Brawl40Pilot.Style.Mixed, seeds, 240, false, out List<double> times, null);
            if (on) fails += f;
        }
        Brawl40N43 = null;
        return fails;
    }

    // ---- G18 parry43 -------------------------------------------------------------------------------------------------------------------------

    sealed class W43ParryTrial
    {
        public int offset;      // the press's step minus the touch step (negative: before the touch)
        public double eyeDist;  // the orb's centre to the eye at the press (-1: gone)
        public bool parried, buffered;
    }

    /// <summary>One mode ("punch" | "swing"), parry43 on or off (NIGHT42's game): the touch step (no press) and one trial per press offset.
    /// The player hangs 30 m over the spawn (open sky), a DRONE orb is fired at 12 m/s from `from` m at the chest (eye - 0.4 m).</summary>
    static (int touch, List<W43ParryTrial> trials, string note, int[] by) W43ParryRun(GameConfig cfg, SurvivalMap map, bool on, string mode, int fromOff, int toOff, double from)
    {
        SurvivalSim sim = W43Sim(cfg, map, on ? new N43Blocks { parry43 = true } : N43Blocks.Off, 4302);
        sim.mover.invulnerable = false;
        string gun = mode == "swing" ? "gauntlet" : "revolver";
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        Core36Keep.Clear();
        DroneOrbs orbs = sim.husks.orbs;
        const double speed = 12;
        double yaw = 0, ux = 0, uz = -1;
        Vec3 hang = new Vec3(map.playerSpawn.x, map.playerSpawn.y + 30, map.playerSpawn.z);
        void Hang()
        {
            sim.mover.position = hang;
            sim.mover.velocity = new Vec3(0, 0, 0);
            sim.mover.grounded = false;
        }
        void Idle(int n, bool pick)
        {
            for (int i = 0; i < n; i++)
            {
                sim.health.hp = sim.health.max;
                Hang();
                Core36Tick(sim, yaw, 0, new CombatInput { weapon = pick && i == 0 ? slot : -1 });
            }
        }
        Idle(30, true);
        (yaw, ux, uz) = W42OpenDir(sim, from);
        string note = double.IsNaN(yaw) ? "no open line" : "";
        if (double.IsNaN(yaw)) yaw = 0;
        (int touchStep, double dist, bool parried, bool buf) Trial(int pressAt)
        {
            Idle(mode == "swing" ? 330 : 150, false); // every cooldown and buffer runs out (the swing's 2.5 s at the most)
            orbs.Clear();
            Vec3 eye = sim.Eye;
            var src = new Vec3(eye.x + ux * from, eye.y - 0.4, eye.z + uz * from);
            var aim = new Vec3(eye.x, eye.y - 0.4, eye.z);
            DroneOrbs.Orb orb = orbs.Fire(src, Vec3.Normalize(aim - src), null, speed);
            int reflects0 = orbs.reflects, buffered0 = sim.loadout.orbBufferParries42;
            int touch = -1;
            double dist = -1;
            for (int s = 0; s < 160; s++)
            {
                bool press = s == pressAt;
                if (press) dist = orb.alive && !orb.reflected ? Vec3.DistanceTo(orb.position, sim.Eye) : -1;
                sim.health.hp = sim.health.max;
                var c = new CombatInput { weapon = -1 };
                if (press && mode == "swing") c.fire = new Button { pressed = true, held = true };
                if (press && mode == "punch") c.punch = new Button { pressed = true, held = true };
                Hang();
                Core36Tick(sim, yaw, -0.15, c);
                if (touch < 0 && !orb.reflected && (!orb.alive || orb.mercy42 > 0 || orb.dodged)) touch = s;
                if (orb.reflected && s > pressAt + 30) break;
            }
            return (touch, dist, orbs.reflects > reflects0, sim.loadout.orbBufferParries42 > buffered0);
        }
        var cal = Trial(-1);
        var trials = new List<W43ParryTrial>();
        if (cal.touchStep < 0) return (-1, trials, note + " (the orb never touched)", new int[3]);
        orbs.Parry43Set(orbs.parry43, orbs.catchRadius43, orbs.catchAhead43, orbs.windowSteps43, orbs.eyeHeight43); // the counters from 0
        for (int o = fromOff; o <= toOff; o++)
        {
            int at = cal.touchStep + o;
            if (at < 0) continue;
            var t = Trial(at);
            trials.Add(new W43ParryTrial { offset = o, eyeDist = t.dist, parried = t.parried, buffered = t.buf });
        }
        return (cal.touchStep, trials, note, new[] { orbs.caughtBody43, orbs.caughtAhead43, orbs.caughtSoon43 });
    }

    /// <summary>parry43 round 2: the same trial with a SPITTER glob lobbed from `from` m to arrive at the chest in `from` / 16 s (its gravity),
    /// the gauntlet swing ("swing") or the punch ("punch"); returns the touch step and per offset parried or not.</summary>
    static (int touch, List<W43ParryTrial> trials, string note, int[] by) W43GlobRun(GameConfig cfg, SurvivalMap map, bool on, string mode, int fromOff, int toOff, double from)
    {
        SurvivalSim sim = W43Sim(cfg, map, on ? new N43Blocks { parry43 = true } : N43Blocks.Off, 4306);
        sim.mover.invulnerable = false;
        string gun = mode == "swing" ? "gauntlet" : "revolver";
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        Core36Keep.Clear();
        SpitterAcid acid = sim.husks.acid;
        double g = MovementMath.GravityFor(cfg.movement.jump.height, cfg.movement.jump.apexTime);
        double yaw = 0, ux = 0, uz = -1;
        Vec3 hang = new Vec3(map.playerSpawn.x, map.playerSpawn.y + 30, map.playerSpawn.z);
        void Hang() { sim.mover.position = hang; sim.mover.velocity = new Vec3(0, 0, 0); sim.mover.grounded = false; }
        void Idle(int n, bool pick)
        {
            for (int i = 0; i < n; i++) { sim.health.hp = sim.health.max; Hang(); Core36Tick(sim, yaw, 0, new CombatInput { weapon = pick && i == 0 ? slot : -1 }); }
        }
        Idle(30, true);
        (yaw, ux, uz) = W42OpenDir(sim, from);
        if (double.IsNaN(yaw)) yaw = 0;
        SpitterAcid.Glob glob = null;
        (int touchStep, bool parried) Trial(int pressAt)
        {
            Idle(mode == "swing" ? 330 : 150, false);
            acid.Clear();
            Vec3 eye = sim.Eye;
            var src = new Vec3(eye.x + ux * from, eye.y - 0.4, eye.z + uz * from);
            var dst = new Vec3(eye.x, eye.y - 0.4, eye.z);
            double T = from / acid.p.globSpeed;
            glob = acid.globs[0];
            glob.alive = true;
            glob.reflected = false;
            glob.age = 0;
            glob.damage = acid.p.globDamage;
            glob.shooter = null;
            glob.target = null;
            glob.position = glob.previous = src;
            glob.velocity = new Vec3((dst.x - src.x) / T, (dst.y - src.y) / T + 0.5 * g * T, (dst.z - src.z) / T);
            int touch = -1;
            for (int s = 0; s < 160; s++)
            {
                bool press = s == pressAt;
                sim.health.hp = sim.health.max;
                var c = new CombatInput { weapon = -1 };
                if (press && mode == "swing") c.fire = new Button { pressed = true, held = true };
                if (press && mode == "punch") c.punch = new Button { pressed = true, held = true };
                Hang();
                Core36Tick(sim, yaw, -0.15, c);
                if (touch < 0 && !glob.reflected && !glob.alive) touch = s;
                if (glob.reflected && s > pressAt + 30) break;
                if (!glob.alive && !glob.reflected && s > pressAt + 2) break;
            }
            return (touch, glob.reflected);
        }
        var cal = Trial(-1);
        var trials = new List<W43ParryTrial>();
        if (cal.touchStep < 0) return (-1, trials, " (the glob never touched)", new int[3]);
        acid.globBody43 = acid.globAhead43 = acid.globSoon43 = 0;
        for (int o = fromOff; o <= toOff; o++)
        {
            int at = cal.touchStep + o;
            if (at < 0) continue;
            var t = Trial(at);
            trials.Add(new W43ParryTrial { offset = o, parried = t.parried, eyeDist = -1 });
        }
        return (cal.touchStep, trials, "", new[] { acid.globBody43, acid.globAhead43, acid.globSoon43 });
    }

    static int W43Globs()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = cfg.Dt;
        int fromOff = -MovementMath.StepsFor(0.7, dt), toOff = MovementMath.StepsFor(0.1, dt);
        string Ms(int steps) => (steps * dt * 1000).ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        Console.WriteLine($"n43weap globs: a SPITTER glob (globSpeed {cfg.Num("spitter.globSpeed")} m/s, its gravity) lobbed from 8 m to the chest, one press per trial at {Ms(fromOff)}..+{Ms(toOff)} ms of its touch; NIGHT42's 2 m eye-point test -> parry43's");
        int fails = 0;
        foreach (string mode in new[] { "swing", "punch" })
            foreach (bool on in new[] { false, true })
            {
                var (touch, trials, note, by) = W43GlobRun(cfg, map, on, mode, fromOff, toOff, 8);
                if (touch < 0) { Console.WriteLine($"  RED  {mode} {(on ? "ON " : "OFF")}:{note}"); fails++; continue; }
                int first = int.MaxValue, last = int.MinValue, n = 0, b200 = 0, b200Ok = 0;
                foreach (var t in trials)
                {
                    if (t.parried) { n++; first = Math.Min(first, t.offset); last = Math.Max(last, t.offset); }
                    if (t.offset < 0 && t.offset >= -MovementMath.StepsFor(0.2, dt)) { b200++; if (t.parried) b200Ok++; }
                }
                bool ok = !on || (b200 > 0 && b200Ok == b200);
                if (!ok) fails++;
                Console.WriteLine($"  {(on ? (ok ? "PASS" : "RED ") : "    ")} glob {mode} {(on ? "ON " : "OFF")}: touch at step {touch}; window {(n == 0 ? "none" : $"{Ms(first)}..{(last >= 0 ? "+" : "")}{Ms(last)} ms ({n} presses)")}; the 200 ms before the touch {b200Ok}/{b200}{(on ? $"; caught by body / sphere / window {by[0]} / {by[1]} / {by[2]}" : "")}");
            }
        return fails;
    }

    static int W43Parry()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = cfg.Dt;
        int fromOff = -MovementMath.StepsFor(0.7, dt), toOff = MovementMath.StepsFor(0.15, dt);
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        string Ms(int steps) => (steps * dt * 1000).ToString("0", ci);
        const string pp = "design.n43.parry43.";
        Console.WriteLine($"n43weap parry: NIGHT42's game (orb.reflectRange {cfg.Num("orb.reflectRange")} m from the eye; parry42 {(cfg.Design("n42.parry42.enabled") ? "on" : "off")}; parry36 buffer {cfg.Num("design.n36.parry36.bufferS")} s, foes40 {(cfg.Design("n40.foes40.enabled") ? "on" : "off")}) -> parry43 (body reach x{cfg.Num(pp + "reachMult")}, mercy {cfg.Num(pp + "mercyS")} s, sphere r {cfg.Num(pp + "catchRadius")} m {cfg.Num(pp + "catchAhead")} m ahead, window {cfg.Num(pp + "windowS")} s, buffer {cfg.Num(pp + "bufferS")} s); a DRONE orb at 12 m/s from 8 m at the chest, one press per trial at {Ms(fromOff)}..+{Ms(toOff)} ms from its touch");
        int fails = 0;
        foreach (string mode in new[] { "swing", "punch" })
        {
            var lines = new List<string>();
            int[] binOk = null, binN = null;
            foreach (bool on in new[] { false, true })
            {
                var (touch, trials, note, by) = W43ParryRun(cfg, map, on, mode, fromOff, toOff, 8);
                if (touch < 0) { Console.WriteLine($"  RED  {mode} {(on ? "ON " : "OFF")}: {note}"); fails++; continue; }
                int firstOk = int.MaxValue, lastOk = int.MinValue, okCount = 0, buf = 0, before = 0, beforeOk = 0, b200 = 0, b200Ok = 0;
                double farthest = 0;
                const int bins = 15; // 50 ms bins from -700 ms
                var ok = new int[bins];
                var n = new int[bins];
                foreach (var t in trials)
                {
                    if (t.parried) { okCount++; firstOk = Math.Min(firstOk, t.offset); lastOk = Math.Max(lastOk, t.offset); if (t.buffered) buf++; if (t.eyeDist > farthest) farthest = t.eyeDist; }
                    if (t.offset < 0) { before++; if (t.parried) beforeOk++; }
                    if (t.offset < 0 && t.offset >= -MovementMath.StepsFor(0.2, dt)) { b200++; if (t.parried) b200Ok++; }
                    int bi = (int)Math.Floor((t.offset * dt + 0.7) / 0.05);
                    if (bi >= 0 && bi < bins) { n[bi]++; if (t.parried) ok[bi]++; }
                }
                string win = okCount == 0 ? "none" : $"{Ms(firstOk)}..{(lastOk >= 0 ? "+" : "")}{Ms(lastOk)} ms ({okCount} presses = {Ms(okCount)} ms)";
                bool pass = !on || (b200 > 0 && b200Ok == b200 && firstOk <= -MovementMath.StepsFor(0.2, dt));
                if (!pass) fails++;
                Console.WriteLine($"  {(on ? (pass ? "PASS" : "RED ") : "    ")} {mode} {(on ? "ON " : "OFF")}: touch at flight step {touch}; parry window {win}; the earliest parry with the orb {farthest:0.00} m from the eye; presses in the 200 ms before the touch {b200Ok}/{b200}; all before the touch {beforeOk}/{before}; by the buffer {buf}{(on ? $"; caught by the body volume {by[0]}, the sphere ahead {by[1]}, the time window {by[2]}" : "")} {note}");
                var sb = new System.Text.StringBuilder("        by press time before the touch (ms: parried/presses): ");
                for (int i = 0; i < bins; i++) sb.Append($"{-700 + 50 * i}:{ok[i]}/{n[i]} ");
                Console.WriteLine(sb.ToString());
            }
        }
        return fails;
    }

    // ---- G19 fusion43 ------------------------------------------------------------------------------------------------------------------------

    sealed class W43FusionTrial
    {
        public double dist, offsetS;
        public bool swung, hit, fusion, killed;
        public int fusionEvents, hitStep = -1, reachPicks, held;
        public double damage, staggerS, knockedM, speed;
        public string why, weapon;
    }

    /// <summary>A flat lane from `p`: nothing solid at knee, chest and eye height for 10 m, and a husk fits at every 1 m from 1 to 8 m (a test
    /// spawn each, killed at once). (yaw, unit x, unit z); the first of 64 directions that passes.</summary>
    static (double yaw, double dx, double dz) W43Lane(SurvivalSim sim, Vec3 p)
    {
        for (int k = 0; k < 64; k++)
        {
            double a = k * Math.PI / 32;
            double dx = Math.Sin(a), dz = -Math.Cos(a);
            bool clear = true;
            foreach (double hgt in new[] { 0.4, 1.0, 1.6 })
                if (sim.world.RayIntersect(new Vec3(p.x, p.y + hgt, p.z), new Vec3(dx, 0, dz), out RayHit hit) && hit.distance < 10) clear = false;
            for (int d = 1; d <= 8 && clear; d++)
            {
                Husk t = sim.husks.Spawn("husk", new Vec3(p.x + dx * d, p.y, p.z + dz * d), p);
                if (t == null) clear = false;
                else t.alive = false;
            }
            if (clear) return (Math.Atan2(-dx, -dz), dx, dz);
        }
        Console.WriteLine("    (no flat lane of 10 m)");
        return (0, 0, -1);
    }

    /// <summary>One scripted dash-in: on the ground at the spawn, the gauntlet in hand (or the KESTREL and the punch), a husk `dist` m ahead
    /// (hp 1e9: the damage is read, nothing dies), a dash toward it pressed at step 0 (stick forward), fire / punch pressed `offsetS` after it.</summary>
    static W43FusionTrial W43FusionRun(GameConfig cfg, SurvivalMap map, bool on, double dist, double offsetS, bool punch, double hp = 1e9, bool dash = true)
    {
        SurvivalSim sim = W43Sim(cfg, map, on ? new N43Blocks { fusion43 = true } : N43Blocks.Off, 4303);
        string gun = punch ? "revolver" : "gauntlet";
        if (!sim.loadout.owned.Contains(gun)) sim.loadout.AddWeapon(gun);
        int slot = sim.loadout.owned.IndexOf(gun);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = W43Lane(sim, map.playerSpawn); // one lane for every distance
        Vec3 sp = map.playerSpawn;
        Core36Keep.Clear();
        var r = new W43FusionTrial { dist = dist, offsetS = offsetS, weapon = punch ? "punch" : "gauntlet" };
        int s = 0;
        sim.events.On<MeleeEvent>("meleeSwing", e => { if (e.weapon == "gauntlet" && s >= 0) r.swung = true; });
        sim.events.On<MeleeEvent>("punch", e => { if (s >= 0) r.swung = true; });
        sim.events.On<HitEvent>("hit", e =>
        {
            if (e.owner != "player" || (e.weapon != "gauntlet" && e.weapon != "punch") || r.hit) return;
            r.hit = true;
            r.damage = e.damage;
            r.hitStep = s;
        });
        sim.events.On<Fusion43Event>(N43Events.Fusion, e => { r.fusionEvents++; r.fusion = true; r.speed = e.speed; r.killed = e.killed; });
        // settle with the weapon in hand (the swap), the husk not there yet
        for (int i = 0; i < 40; i++) Core36Tick(sim, yaw, 0, new CombatInput { weapon = i == 0 ? slot : -1 });
        Vec3 p0 = sim.mover.position;
        var spot = new Vec3(p0.x + ux * dist, p0.y, p0.z + uz * dist);
        Husk h = sim.husks.Spawn("husk", spot, p0);
        if (h == null) { r.why = "no husk"; Console.WriteLine($"    (no husk spawned {dist} m ahead)"); return r; }
        h.hp = h.maxHp = hp;
        h.position = h.previous = spot; // exactly dist m ahead, on the player's floor (Spawn may lift or move it)
        Core36Keep.Add(h);
        int press = MovementMath.StepsFor(offsetS, sim.step);
        Vec3 at = default;
        int afterHit = MovementMath.StepsFor(0.3, sim.step);
        for (s = 0; s < 120; s++)
        {
            double py = 0;
            if (h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            var f = new InputFrame { yaw = yaw, pitch = py, moveZ = dash && s < 40 ? 1 : 0, dash = new Button { pressed = dash && s == 0, held = dash && s < 3 } };
            var c = new CombatInput { weapon = -1 };
            if (s == press && !punch) c.fire = new Button { pressed = true, held = true };
            if (s == press && punch) c.punch = new Button { pressed = true, held = true };
            sim.health.hp = sim.health.max;
            Core36Tick(sim, yaw, py, c, f);
            if (W43Trace && s < 60 && s % 3 == 0) Console.WriteLine($"      s{s} player {sim.mover.position.x:0.00},{sim.mover.position.y:0.00},{sim.mover.position.z:0.00} v {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.0} dash {sim.mover.DashStepsLeft} | husk {(h.alive ? "" : "DEAD ")}{h.position.x:0.00},{h.position.y:0.00},{h.position.z:0.00} gap {JsMath.Hypot(h.position.x - sim.mover.position.x, h.position.z - sim.mover.position.z):0.00} cool {sim.loadout.GunCycleLeft29}");
            if (r.hitStep == s)
            {
                r.staggerS = h.staggerSteps * sim.step;
                at = h.position;
                if (on && sim.loadout.fusion43 != null) r.why = sim.loadout.fusion43.why;
            }
            if (r.hitStep >= 0 && s == r.hitStep + afterHit) r.knockedM = JsMath.Hypot(h.position.x - at.x, h.position.z - at.z);
            if (r.hitStep >= 0 && s > r.hitStep + afterHit) break;
        }
        if (sim.loadout.fusion43 != null) { r.reachPicks = sim.loadout.fusion43.reachPicks; r.held = sim.loadout.fusion43.heldSwings; }
        return r;
    }

    static readonly bool W43Trace = Environment.GetEnvironmentVariable("W43_TRACE") != null;

    static int W43Fusion()
    {
        if (W43Trace)
        {
            var tt = W43FusionRun(Config(), Map(), true, double.Parse(Environment.GetEnvironmentVariable("W43_TRACE"), System.Globalization.CultureInfo.InvariantCulture), 0.1, false);
            Console.WriteLine($"  trace: hit {tt.hit} fusion {tt.fusion} dmg {tt.damage:0} swung {tt.swung}");
            return 0;
        }
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        const string fr = "design.n43.fusion43.";
        Console.WriteLine($"n43weap fusion: a dash (24 m/s, 0.25 s) at a husk 3-7 m ahead, the press 0-0.4 s after it; fusion43 speedMin {cfg.Num(fr + "speedMin")} m/s, dashAfterS {cfg.Num(fr + "dashAfterS")}, x{cfg.Num(fr + "damageMult")} (boss x{cfg.Num(fr + "bossDamageMult")}), knock {cfg.Num(fr + "knockSpeed")} m/s {cfg.Num(fr + "knockTime")} s, stagger {cfg.Num(fr + "staggerS")} s, hitstop {cfg.Num(fr + "hitstopS")} / boss {cfg.Num(fr + "bossHitstopS")} s, reach +{cfg.Num(fr + "reachBonus")} m");
        double[] dists = { 3, 4, 5, 6, 7 };
        double[] offs = { 0, 0.05, 0.1, 0.15, 0.2, 0.3, 0.4 };
        int fails = 0;
        foreach (bool punch in new[] { false, true })
        {
            foreach (bool on in new[] { false, true })
            {
                int trials = 0, hits = 0, fusions = 0, extraEvents = 0, reach = 0, heldN = 0;
                double dmgSum = 0, dmgFusion = 0, stagSum = 0, knockSum = 0;
                int nFusionDmg = 0;
                var grid = new System.Text.StringBuilder();
                foreach (double d in dists)
                {
                    grid.Append($"        {d:0} m: ");
                    foreach (double o in offs)
                    {
                        var t = W43FusionRun(cfg, map, on, d, o, punch);
                        trials++;
                        if (t.hit) { hits++; dmgSum += t.damage; stagSum += t.staggerS; knockSum += t.knockedM; }
                        if (t.fusion) { fusions++; dmgFusion += t.damage; nFusionDmg++; }
                        if (t.fusionEvents > 1) extraEvents++;
                        reach += t.reachPicks;
                        heldN += t.held;
                        grid.Append(!t.hit ? " .   " : t.fusion ? $" F{t.damage,-4:0}" : $" h{t.damage,-4:0}");
                    }
                    grid.AppendLine();
                }
                string mode = punch ? "punch (KESTREL in hand)" : "gauntlet swing";
                bool ok = !on || (fusions * 10 >= hits * 8 && hits > 0 && extraEvents == 0);
                if (!ok) fails++;
                Console.WriteLine($"  {(on ? (ok ? "PASS" : "RED ") : "    ")} {mode} {(on ? "ON " : "OFF")}: {hits}/{trials} presses landed ({100.0 * hits / trials:0} %), FUSION {fusions}/{hits} of the hits ({(hits > 0 ? 100.0 * fusions / hits : 0):0} %), mean damage a hit {(hits > 0 ? dmgSum / hits : 0):0.0}{(nFusionDmg > 0 ? $" (a FUSION {dmgFusion / nFusionDmg:0.0})" : "")}, mean stagger {(hits > 0 ? stagSum / hits : 0):0.00} s, mean knock travel in 0.3 s {(hits > 0 ? knockSum / hits : 0):0.00} m; landed by the +reach {reach}, by the held swing {heldN}; >1 event a hit: {extraEvents}");
                Console.Write($"        grid (rows: the husk's distance; columns: the press {string.Join(" / ", Array.ConvertAll(offs, x => x.ToString("0.00")))} s after the dash; F = FUSION, h = a plain hit, . = no hit; the damage):\n{grid}");
            }
        }
        // ImpactRules' Fusion43 tier (the Game's freeze): the sim's "fusion43" request at its length, beside the gauntlet's own request
        {
            double hs = cfg.Num(fr + "hitstopS"), bs = cfg.Num(fr + "bossHitstopS");
            foreach (var (name, cls, killed, simS) in new[] { ("a husk hit", GauntletHit42Event.Small, false, hs), ("a husk kill", GauntletHit42Event.Small, true, hs), ("THE ICEBREAKER hit", GauntletHit42Event.Boss, false, bs) })
            {
                var r = new ImpactRules(cfg) { impact42 = true, crunch36 = cfg.Design("n36.crunch36.enabled") };
                r.BeginFrame(5, false);
                r.ClearPending();
                Gauntlet42Ask(r, cls, killed);
                double plain = r.Gauntlet42Seconds(cls, killed);
                r.OnSimHitstop(N43Events.Fusion);
                r.EndStep(false, simS);
                bool ok = r.granted == ImpactTier.Fusion43 && Math.Abs(r.grantedSeconds - simS) < 1e-9 && simS >= plain;
                if (!ok) fails++;
                Console.WriteLine($"  {(ok ? "PASS" : "RED ")} ImpactRules, {name}: the gauntlet's own freeze {plain * 1000:0} ms; with the FUSION's request: {r.granted} {r.grantedSeconds * 1000:0} ms");
            }
            // the share: a FUSION asked every 0.5 s for 90 s: the worst rolling minute stays inside design.n43.fusion43.perMinute
            var g = new ImpactRules(cfg) { impact42 = true };
            double share = cfg.Num(fr + "perMinute") * 60, worst = 0;
            var starts = new List<(double at, double s)>();
            for (int k = 0; k < 180; k++)
            {
                g.BeginFrame(k == 0 ? 1 : 0.5, false);
                g.ClearPending();
                g.OnSimHitstop(N43Events.Fusion);
                if (g.EndStep(false, bs)) starts.Add((g.Clock, g.grantedSeconds));
                g.BeginFrame(g.FreezeLeft, false);
            }
            foreach (var (at, _) in starts)
            {
                double sum = 0;
                foreach (var (a, s2) in starts) if (a <= at && a > at - 60) sum += s2;
                worst = Math.Max(worst, sum);
            }
            bool sok = worst <= share + 1e-6 && starts.Count > 0;
            if (!sok) fails++;
            Console.WriteLine($"  {(sok ? "PASS" : "RED ")} ImpactRules, the FUSION's share: a boss FUSION asked every 0.5 s: {starts.Count} freezes, the worst rolling minute {worst:0.00} s (share {share:0.00} s), cut {g.cuts}, skipped {g.skips[(int)ImpactTier.Fusion43]}");
        }
        // the standing control: no dash, the husk 1.6 m ahead: never a FUSION
        {
            var t = W43FusionRun(cfg, map, true, 1.6, 0.1, false, 1e9, false);
            bool ok = t.hit && !t.fusion;
            Console.WriteLine($"  {(ok ? "PASS" : "RED ")} control (fusion43 ON, standing, no dash, a swing at a husk 1.6 m ahead): hit {t.hit}, FUSION {t.fusion}, damage {t.damage:0}");
            if (!ok) fails++;
        }
        return fails;
    }

    // ---- G20 slug43 --------------------------------------------------------------------------------------------------------------------------

    sealed class W43SlugResult
    {
        public bool fired;
        public int events, blastHits, blastKills;
        public double[] dmg = new double[3], travel = new double[3], stagger = new double[3];
        public bool[] dead = new bool[3];
        public string note = "";
    }

    /// <summary>One charged THUNDERCLAP slug on the ground at a husk A 6 m ahead, with husk B 1.5 m behind A and husk C 1.5 m beside A
    /// (each `hp`), held to FULL and released; the bodies pinned until the release, then free for 0.5 s.</summary>
    static W43SlugResult W43SlugRun(GameConfig cfg, SurvivalMap map, bool on, double hp)
    {
        SurvivalSim sim = W43Sim(cfg, map, on ? new N43Blocks { slug43 = true } : N43Blocks.Off, 4304);
        var r = new W43SlugResult();
        if (!sim.loadout.owned.Contains("shotgun")) sim.loadout.AddWeapon("shotgun");
        int slot = sim.loadout.owned.IndexOf("shotgun");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = W43Lane(sim, map.playerSpawn);
        Core36Keep.Clear();
        for (int i = 0; i < 40; i++) Core36Tick(sim, yaw, 0, new CombatInput { weapon = i == 0 ? slot : -1 });
        Vec3 p = sim.mover.position;
        double rx = -uz, rz = ux; // the lane's right
        var at = new[] { new Vec3(p.x + ux * 6, p.y, p.z + uz * 6), new Vec3(p.x + ux * 7.5, p.y, p.z + uz * 7.5), new Vec3(p.x + ux * 6 + rx * 1.5, p.y, p.z + uz * 6 + rz * 1.5) };
        var h = new Husk[3];
        for (int k = 0; k < 3; k++)
        {
            h[k] = sim.husks.Spawn("husk", at[k], p);
            if (h[k] == null) { r.note += $" (husk {k} not spawned)"; return r; }
            Core36Keep.Add(h[k]);
        }
        void Pin()
        {
            for (int k = 0; k < 3; k++) if (h[k].alive) { h[k].position = h[k].previous = at[k]; h[k].hp = h[k].maxHp = hp; h[k].staggerSteps = 0; }
        }
        int fired = -1;
        sim.events.On<ChargeShot27Event>("chargeShot27Fire", e => { if (e.gun == "shotgun") r.fired = true; });
        sim.events.On<Slug43Event>(Slug43.Event, e => { r.events++; r.blastHits = e.blastHits; r.blastKills = e.blastKills; });
        sim.events.On<HitEvent>("hit", e =>
        {
            if (e.owner != "player") return;
            for (int k = 0; k < 3; k++) if (h[k].id == e.enemy) r.dmg[k] += e.damage;
        });
        int full = MovementMath.StepsFor(cfg.Num("design.n40.charge40.shotgunFullS"), sim.step) + 6;
        int lead = 30, rel = lead + full;
        for (int s = 0; s < rel + 120; s++)
        {
            if (fired < 0) Pin();
            double py = 0;
            (yaw, py) = ArclineLook(sim.Eye, new Vec3(at[0].x, at[0].y + 1.0, at[0].z));
            bool held = s >= lead && s < rel;
            var c = new CombatInput { weapon = -1, fire = new Button { held = held, pressed = s == lead } };
            sim.health.hp = sim.health.max;
            Core36Tick(sim, yaw, py, c);
            if (fired < 0 && r.fired)
            {
                fired = s;
                for (int k = 0; k < 3; k++) { r.stagger[k] = h[k].alive ? h[k].staggerSteps * sim.step : -1; }
            }
            if (fired >= 0 && s == fired + 60)
                for (int k = 0; k < 3; k++) r.travel[k] = JsMath.Hypot(h[k].position.x - at[k].x, h[k].position.z - at[k].z);
        }
        for (int k = 0; k < 3; k++) r.dead[k] = !h[k].alive;
        if (!r.fired) r.note += " (no charged shot fired)";
        return r;
    }

    static int W43Slug()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        const string sr = "design.n43.slug43.";
        Console.WriteLine($"n43weap slug: a charged THUNDERCLAP slug (design.n31.charge.shotgun.slug {cfg.Bool("design.n31.charge.shotgun.slug")}) at husk A 6 m ahead, B 1.5 m behind A, C 1.5 m beside A; slug43 knock {cfg.Num(sr + "knockSpeed")} m/s {cfg.Num(sr + "knockTime")} s, stagger {cfg.Num(sr + "staggerS")} s, blast r {cfg.Num(sr + "blastRadius")} m {cfg.Num(sr + "blastDamage")} dmg (edge x{cfg.Num(sr + "blastEdge")}), push {cfg.Num(sr + "blastKnock")} m/s, stagger {cfg.Num(sr + "blastStaggerS")} s");
        int fails = 0;
        string[] names = { "A (struck)", "B (behind)", "C (beside)" };
        foreach (double hp in new[] { 1e9, 40.0 })
        {
            foreach (bool on in new[] { false, true })
            {
                var r = W43SlugRun(cfg, map, on, hp);
                var sb = new System.Text.StringBuilder();
                for (int k = 0; k < 3; k++)
                    sb.Append($"{names[k]}: dmg {r.dmg[k]:0}{(hp < 1e6 ? (r.dead[k] ? " DEAD" : " alive") : "")}, stagger {(r.stagger[k] < 0 ? "-" : r.stagger[k].ToString("0.00"))} s, travel {r.travel[k]:0.00} m; ");
                bool ok = !on || (r.fired && r.events == 1 && (hp > 1e6 ? r.travel[0] > 3 && r.dmg[1] > 0 && r.dmg[2] > 0 : true));
                if (!ok) fails++;
                Console.WriteLine($"  {(on ? (ok ? "PASS" : "RED ") : "    ")} slug43 {(on ? "ON " : "OFF")} hp {(hp > 1e6 ? "1e9" : hp.ToString("0"))}: fired {r.fired}, slug43 events {r.events} (blast hits {r.blastHits}, kills {r.blastKills}); {sb}{r.note}");
            }
        }
        return fails;
    }

    // ---- G20 torch43 -------------------------------------------------------------------------------------------------------------------------

    /// <summary>One TORCH run on the ground at a body `dist` m ahead (kind "husk" | "tank", hp: its own when <= 0), fire held `holdS` s, then
    /// `afterS` s with the trigger up. Returns (damage while held, damage after, the kill time from the first press (-1: alive), the first
    /// flame hit's time, ignites, burning at the release, the note).</summary>
    static (double held, double after, double killAt, double firstHit, int ignites, bool burningAtRelease, string note) W43TorchRun(GameConfig cfg, SurvivalMap map, bool on, string kind, double dist, double hp, double holdS, double afterS)
    {
        SurvivalSim sim = W43Sim(cfg, map, on ? new N43Blocks { torch43 = true } : N43Blocks.Off, 4305);
        string note = "";
        if (!sim.loadout.owned.Contains("torch")) sim.loadout.AddWeapon("torch");
        int slot = sim.loadout.owned.IndexOf("torch");
        if (slot < 0) return (0, 0, -1, -1, 0, false, " (no TORCH in the loadout)");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = W43Lane(sim, map.playerSpawn);
        Core36Keep.Clear();
        for (int i = 0; i < 40; i++) Core36Tick(sim, yaw, 0, new CombatInput { weapon = i == 0 ? slot : -1 });
        if (sim.loadout.Weapon != "torch") note += $" (in hand: {sim.loadout.Weapon}; b5 {sim.loadout.B5On})";
        Vec3 p = sim.mover.position;
        var at = new Vec3(p.x + ux * dist, p.y, p.z + uz * dist);
        Husk h = sim.husks.Spawn(kind, at, p);
        if (h == null) return (0, 0, -1, -1, 0, false, note + " (no body spawned)");
        if (hp > 0) h.hp = h.maxHp = hp;
        Core36Keep.Add(h);
        double held = 0, after = 0, killAt = -1, firstHit = -1;
        int s = 0, holdSteps = MovementMath.StepsFor(holdS, sim.step), total = holdSteps + MovementMath.StepsFor(afterS, sim.step);
        sim.events.On<HitEvent>("hit", e =>
        {
            if (e.owner != "player" || e.enemy != h.id) return;
            if (s < holdSteps) held += e.damage; else after += e.damage;
            if (firstHit < 0 && e.weapon == "torch") firstHit = s * sim.step;
            if (e.killed && killAt < 0) killAt = (s + 1) * sim.step;
        });
        bool burning = false;
        for (s = 0; s < total; s++)
        {
            if (h.alive) { h.position = h.previous = at; } // pinned: the reach stays the test's
            double py = 0;
            (yaw, py) = ArclineLook(sim.Eye, new Vec3(at.x, at.y + 1.0, at.z));
            var c = new CombatInput { weapon = -1, fire = new Button { held = s < holdSteps, pressed = s == 0 } };
            sim.health.hp = sim.health.max;
            Core36Tick(sim, yaw, py, c);
            if (s == holdSteps - 1) burning = sim.loadout.Burning43(h);
            if (!h.alive && killAt >= 0 && s > holdSteps) break;
        }
        int ign = sim.loadout.torch43 != null ? sim.loadout.torch43.ignites : 0;
        return (held, after, killAt, firstHit, ign, burning, note);
    }

    static int W43Torch()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        const string tr = "design.n43.torch43.";
        Console.WriteLine($"n43weap torch: the TORCH (weapons.torch range {cfg.Num("weapons.torch.range")} m, {cfg.Num("weapons.torch.damage")} a {cfg.Num("weapons.torch.tick")} s tick) on the ground; torch43 burn {cfg.Num(tr + "burnS")} s at {cfg.Num(tr + "burnDps")} dps (tick {cfg.Num(tr + "burnTickS")} s), ignite flinch {cfg.Num(tr + "igniteStaggerS")} s, reach ramp {cfg.Num(tr + "rampFrom")} x -> 1 x over {cfg.Num(tr + "rampS")} s");
        int fails = 0;
        foreach (bool on in new[] { false, true })
        {
            string O = on ? "ON " : "OFF";
            var bag = W43TorchRun(cfg, map, on, "husk", 4, 1e9, 3, 3.5);
            var tap = W43TorchRun(cfg, map, on, "husk", 4, 1e9, 0.3, 3.5);
            var far = W43TorchRun(cfg, map, on, "husk", 5.5, 1e9, 1.0, 0.1);
            var husk = W43TorchRun(cfg, map, on, "husk", 4, 0, 4, 3.5);
            var tank = W43TorchRun(cfg, map, on, "tank", 4, 0, 3.9, 3.5);
            bool ok = !on || (bag.after > 0 && tap.after > 0 && bag.ignites > 0 && bag.burningAtRelease);
            if (!ok) fails++;
            Console.WriteLine($"  {(on ? (ok ? "PASS" : "RED ") : "    ")} torch43 {O}: held 3 s at a 1e9 husk 4 m ahead: {bag.held:0} dmg = {bag.held / 3:0.0} dps, burning at the release {bag.burningAtRelease}, then {bag.after:0} more in 3.5 s; a 0.3 s spray: {tap.held:0} + {tap.after:0} after; ignites {bag.ignites}{bag.note}");
            Console.WriteLine($"         the first flame hit at 5.5 m: +{far.firstHit:0.000} s (the ramp); kill time from the press: a husk (its own hp) {(husk.killAt < 0 ? "alive" : husk.killAt.ToString("0.000") + " s")}, a tank {(tank.killAt < 0 ? "alive" : tank.killAt.ToString("0.000") + " s")} (held 3.9 s, to the heat limit: {tank.held:0} dmg, then {tank.after:0} after){husk.note}{tank.note}");
        }
        return fails;
    }

    /// <summary>G17: NIGHT42's `n42hit brawl` freeze ledger (impact42 off / on) with gauntlet43 on: the gauntlet's share of a minute.</summary>
    static int W43Freeze(int seeds)
    {
        Brawl40N42 = null;
        Brawl40N43 = new N43Blocks { gauntlet43 = true };
        int f = Hit42Brawl(seeds);
        Brawl40N43 = null;
        return f;
    }
}
