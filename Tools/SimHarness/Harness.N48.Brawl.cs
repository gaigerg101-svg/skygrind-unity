// NIGHT48 builder B (NIGHT48.md section 1, "Level 2 + 3"; NIGHT48-BRIEF.md): levels 2 and 3's meters, headless. Hooked without a Program.cs
// line: a module initializer runs the command and exits when argv[1] is "n48brawl" or "n48wyrm" (Harness.N46.Flow.cs's pattern; no older harness
// file touched; helpers carry the B48 prefix).
//   n48brawl stampede [--rows stampede47,charger47] [--rounds 14] [--old]
//     stampede47's round-6 stall, found and fixed (G-row: the R4 row stampede47). NIGHT47's `n47b stampede` stopped at round 6 with every row OFF
//     too: round 6 is SURVIVAL's first HOLD round (director.rounds.objectiveFrom 6, objectiveEvery 4, holdOffset 2: 6 % 4 = 2), and a HOLD round
//     cannot clear until the player has stood holdTime s in its ring (Objectives.Holding gates the round's clear). The check's player stood
//     still at the spawn, so it never held: the stall was the check's pilot, not the game and not stampede47 (the bot walks into the ring:
//     AutopilotInput.cs's `objectives.Holding` branch). The fixed check stands the player in the ring while a HOLD runs (and in an exfil's site
//     while one runs). --old runs NIGHT47's check unchanged (the RED line).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void B48Init()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || (argv[1] != "n48brawl" && argv[1] != "n48wyrm")) return;
        string[] args = argv.Skip(1).ToArray();
        NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off"); // the module initializer runs before Program.Main
        int code;
        try { code = argv[1] == "n48brawl" ? B48Brawl(args) : B48Wyrm(args); }
        catch (Exception e) { Console.WriteLine(argv[1] + ": EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string B48Arg(string[] args, string name, string dflt)
    {
        int i = Array.LastIndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : dflt;
    }

    static string[] B48Rows(string[] args)
    {
        string rows = B48Arg(args, "--rows", "");
        return rows.Length > 0 ? rows.Split(',', StringSplitOptions.RemoveEmptyEntries) : new string[0];
    }

    static int B48Brawl(string[] args)
    {
        string what = args.Length > 1 ? args[1] : "";
        if (what == "stampede") return B48Stampede(args);
        Console.WriteLine("n48brawl: unknown '" + what + "' (stampede)");
        return 2;
    }

    static int B48Wyrm(string[] args)
    {
        string what = args.Length > 1 ? args[1] : "";
        if (what == "island") return B48Island(B48Arg(args, "--level", "n47-kiln"));
        if (what == "hop") return B48HopProbe(B48Arg(args, "--level", "n47-kiln"), Array.IndexOf(args, "--trace") >= 0);
        Console.WriteLine("n48wyrm: unknown '" + what + "' (island)");
        return 2;
    }

    /// <summary>`n48wyrm hop [--trace]` (G33 round 2: the pay pilot's island hop, alone): the worm's fight begun (kilnBoost48 on), the player on
    /// the arena floor 4 / 6 / 8 / 10 m from island 0's centre (outward and to the side), hp 40, the KESTREL in hand (the swap is part of the hop),
    /// B48Hop to the island: landed on its top (feet within 0.5 m of islandTop, grounded), the heal, the boosts, the steps. --trace: a line every
    /// 10 steps (height over the floor, the distance to the centre, the phase, the gun).</summary>
    static int B48HopProbe(string level, bool trace)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", level + ".json");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        B1Rows = new[] { "kilnBoost48" };
        B1Walk = false;
        const string R = "design.n48.kilnBoost48.numbers.";
        int landed = 0, tries = 0;
        foreach (double dist in new[] { 4.0, 6.0, 8.0, 10.0 })
            foreach (double side in new[] { 0.0, 60.0 })
            {
                SurvivalSim sim = B1Start(cfg, path, 1, out RoomDef k5);
                if (sim == null || k5 == null) { Console.WriteLine("n48wyrm hop: no fight"); return 1; }
                sim.Respawn(k5.spawn, k5.spawnYawDeg);
                for (int i = 0; i < 600 && (sim.wyrm24 == null || !sim.wyrm24.Alive); i++) sim.Step();
                Wyrm w = sim.wyrm24;
                if (w == null || !w.arenaCenter.HasValue) { Console.WriteLine("n48wyrm hop: the worm did not begin"); return 1; }
                Vec3 c = w.arenaCenter.Value;
                double a = cfg.Num(R + "islandFirstDeg") * Math.PI / 180, r = cfg.Num(R + "islandR"), b = a + side * Math.PI / 180;
                var isl = new Vec3(c.x + r * Math.Cos(a), c.y + cfg.Num(R + "islandTop"), c.z + r * Math.Sin(a));
                // the start: dist m from the island's centre, along the ring's radius toward the arena's centre turned by `side`
                var start = new Vec3(isl.x - dist * Math.Cos(b), c.y + 0.05, isl.z - dist * Math.Sin(b));
                sim.Respawn(start, 0);
                if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
                var si = (ScriptedInput)sim.input;
                for (int i = 0; i < 90; i++)
                {
                    sim.mover.invulnerable = true;
                    int rv = sim.loadout.owned.IndexOf("revolver");
                    si.SetLook(0, 0); si.Set(default, new CombatInput { weapon = sim.loadout.Weapon != "revolver" ? rv : -1 });
                    sim.Step(); sim.hitstopLeft = 0;
                }
                sim.health.hp = 40;
                int before = sim.heals48;
                var hop = new B48Hop();
                hop.Begin(true, isl);
                tries++;
                double maxY = 0;
                bool onTop = false;
                int n = 0;
                while (hop.Active && n < 600)
                {
                    sim.mover.invulnerable = true;
                    InputFrame mv = default;
                    CombatInput cb = default;
                    hop.Step(sim, ref mv, ref cb, cfg.Num(R + "islandHalf") - 0.3, cfg.Num(R + "islandTop"));
                    si.SetLook(mv.yaw, mv.pitch);
                    si.Set(mv, cb);
                    sim.Step();
                    sim.hitstopLeft = 0;
                    n++;
                    Vec3 p = sim.mover.position;
                    maxY = Math.Max(maxY, p.y - c.y);
                    if (trace && n % 10 == 0) Console.WriteLine($"    step {n}: up {p.y - c.y:0.00} m, to the centre {JsMath.Hypot(isl.x - p.x, isl.z - p.z):0.00} m, phase {hop.phase}, gun {sim.loadout.Weapon}, grounded {sim.mover.grounded}, vy {sim.mover.velocity.y:0.0}");
                }
                for (int i = 0; i < 30; i++) { sim.mover.invulnerable = true; si.Set(default, default); sim.Step(); sim.hitstopLeft = 0; }
                Vec3 q = sim.mover.position;
                onTop = sim.mover.grounded && Math.Abs(q.y - isl.y) < 0.5;
                if (onTop) landed++;
                Console.WriteLine($"n48wyrm hop from {dist:0} m (side {side:0} deg): {(onTop ? "LANDED" : "missed")} (feet {q.y - c.y:0.00} m up, {JsMath.Hypot(isl.x - q.x, isl.z - q.z):0.00} m from the centre), top reached {maxY:0.00} m, boosts {hop.boosts}, heals {sim.heals48 - before}, steps {n}");
            }
        Console.WriteLine($"n48wyrm hop SUMMARY: landed {landed}/{tries}");
        return 0;
    }

    /// <summary>`n48wyrm island [--level n47-kiln]` (kilnBoost48's geometry check): the worm's fight begun (B1Start, rows kilnBoost48 on), the
    /// player put 1 m over each slag island's top as design.n48.kilnBoost48.numbers places it (arena-local to the worm's arena centre), hp 40,
    /// 2 s of standing: where the feet settle (over the arena floor), grounded or not, and the island heals (kiln48Heal) taken.</summary>
    static int B48Island(string level)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", level + ".json");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        B1Rows = new[] { "kilnBoost48" };
        B1Walk = false;
        SurvivalSim sim = B1Start(cfg, path, 1, out RoomDef k5);
        if (sim == null || k5 == null) { Console.WriteLine("n48wyrm island: no fight"); return 1; }
        sim.Respawn(k5.spawn, k5.spawnYawDeg);
        for (int i = 0; i < 600 && (sim.wyrm24 == null || !sim.wyrm24.Alive); i++) sim.Step();
        Wyrm w = sim.wyrm24;
        if (w == null || !w.Alive || !w.arenaCenter.HasValue) { Console.WriteLine("n48wyrm island: the worm did not begin"); return 1; }
        Vec3 c = w.arenaCenter.Value;
        const string R = "design.n48.kilnBoost48.numbers.";
        var si = (ScriptedInput)sim.input;
        for (int k = 0; k < (int)cfg.Num(R + "islandCount"); k++)
        {
            double a = (cfg.Num(R + "islandFirstDeg") + k * cfg.Num(R + "islandStepDeg")) * Math.PI / 180, r = cfg.Num(R + "islandR");
            var at = new Vec3(c.x + r * Math.Cos(a), c.y + cfg.Num(R + "islandTop") + 1.0, c.z + r * Math.Sin(a));
            sim.Respawn(at, 0);
            sim.health.hp = 40;
            int before = sim.heals48;
            for (int i = 0; i < 240; i++) { sim.mover.invulnerable = true; si.SetLook(0, 0); si.Set(default, default); sim.Step(); sim.hitstopLeft = 0; }
            Vec3 p = sim.mover.position;
            Console.WriteLine($"n48wyrm island {k} ({level}, centre ({at.x:0.0}, {at.z:0.0})): feet {p.y - c.y:0.00} m over the arena floor at ({p.x:0.0}, {p.z:0.0}), grounded {sim.mover.grounded}, heals {sim.heals48 - before}, hp {sim.health.hp:0}");
        }
        return 0;
    }

    // ---- n47human policies (the lead's hook, found by reflection; each runs only with its own flag, so nobody else's run changes) ----------
    static readonly bool B48MeterOn = Array.IndexOf(Environment.GetCommandLineArgs(), "--b48-meter") >= 0;
    // a static field initializer, not the module initializer: n47human's module initializer can run first and exits the process when it ends, but
    // it reads Harness statics first, so the Harness type initializer (every static field of every Harness file) runs before any fight
    static readonly bool B48ProbesSet = B48SetProbes();

    static bool B48SetProbes()
    {
        string[] argv = Environment.GetCommandLineArgs();
        string ts = B48Arg(argv, "--b48-tellS", ""), ls = B48Arg(argv, "--b48-lockS", "");
        if (ts.Length > 0) SurvivalSim.Probe48TellS = double.Parse(ts, CultureInfo.InvariantCulture);
        if (ls.Length > 0) SurvivalSim.Probe48LockS = double.Parse(ls, CultureInfo.InvariantCulture);
        string bx = B48Arg(argv, "--b48-beat", "");
        if (bx.Length > 0) { SurvivalSim.Probe48BeatExtra = double.Parse(bx, CultureInfo.InvariantCulture); Console.WriteLine($"b48 probes: beat extra {bx} s"); }
        string dw = B48Arg(argv, "--b48-when", "");
        if (dw.Length > 0) { SurvivalSim.Probe48DiveWhen = double.Parse(dw, CultureInfo.InvariantCulture); Console.WriteLine($"b48 probes: dive when {dw}"); }
        string ws = B48Arg(argv, "--b48-share", "");
        if (ws.Length > 0) { SurvivalSim.Probe48WindowShare = double.Parse(ws, CultureInfo.InvariantCulture); Console.WriteLine($"b48 probes: windowShare {ws}"); }
        // (no ProcessExit hook: n47human calls Environment.Exit inside its module initializer, and a handler calling back into this module
        // deadlocks the exit; each fight is flushed on its worm's bossDown event, the next fight's start, or not at all at a 300 s cap's end)
        if (ts.Length > 0 || ls.Length > 0) Console.WriteLine($"b48 probes: tellS {SurvivalSim.Probe48TellS}, lockS {SurvivalSim.Probe48LockS}");
        return true;
    }
    static readonly List<double> B48DivesPerMin = new List<double>(), B48Times = new List<double>();
    static readonly SortedDictionary<string, (int n, double min, double max)> B48Tells = new SortedDictionary<string, (int n, double min, double max)>();
    static int B48Fights, B48WithDive, B48Killed, B48Staggers, B48Parries, B48TellStag, B48DivesAfter, B48DivesMissed, B48CoreFull, B48CoreFloor, B48Heals;

    static double B48Median(List<double> l)
    {
        if (l.Count == 0) return double.NaN;
        var s = l.OrderBy(x => x).ToList();
        return s[Math.Min(s.Count - 1, (int)Math.Floor(0.5 * (s.Count - 1) + 0.5))];
    }

    /// <summary>`n47human --boss wyrm --b48-meter` (G31 / G34 / G33 / the worm's min; changes no input): per fight, at its end (a kill or the 300 s
    /// cap): every tell's length from the worm's own log (Wyrm.log), the DIVE starts (the burrows) a minute, the staggers (wyrm30Staggers), the
    /// punch / parry staggers of wyrmTell48 and those inside the tell, the dives wyrmDive48 started / dropped, kilnBoost48's core hits at full
    /// vs floor x and its island heals; and the running totals and medians over the fights so far (the last line is the run's summary).</summary>
    static void H47PolicyB48Meter(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (!B48MeterOn || ctx.boss != "wyrm") return;
        SurvivalSim sim = ctx.sim;
        Wyrm w = sim.wyrm24;
        if (w == null) return;
        // the pilot loop calls no policy after the kill step, so a fight is flushed when the next one starts (or at the process's exit)
        if (!ReferenceEquals(B48CurSim, sim))
        {
            B48MeterFlush();
            B48CurSim = sim; B48CurDives = 0; B48CurPat = ""; B48CurLog = w.log.Count;
            sim.events.On<object>("bossDown", _ => { if (ReferenceEquals(B48CurSim, sim) && !sim.wyrm24.Alive) { B48CurSteps++; B48MeterFlush(); } });
        }
        string pat = w.PatternName ?? "-";
        if (pat == "dive" && B48CurPat != "dive") B48CurDives++;
        B48CurPat = pat;
        B48CurSteps = ctx.step + 1;
    }

    static SurvivalSim B48CurSim;
    static int B48CurDives, B48CurLog, B48CurSteps;
    static string B48CurPat;

    static void B48MeterFlush()
    {
        SurvivalSim sim = B48CurSim;
        B48CurSim = null;
        if (sim == null || sim.wyrm24 == null) return;
        Wyrm w = sim.wyrm24;
        int dives = 0; // the burrows: the worm's own "dive" tells (one per DIVE pattern, back-to-back dives counted apart; the pattern-edge count B48CurDives merges them)
        int steps = B48CurSteps;
        for (int i = B48CurLog; i < w.log.Count; i++)
        {
            var e = w.log[i];
            if (e.resolve) continue;
            if (e.name == "dive") dives++;
            B48Tells.TryGetValue(e.name, out var t);
            B48Tells[e.name] = t.n == 0 ? (1, e.seconds, e.seconds) : (t.n + 1, Math.Min(t.min, e.seconds), Math.Max(t.max, e.seconds));
        }
        double min = Math.Max(1, steps) / 120.0 / 60.0;
        B48Fights++;
        if (!w.Alive) B48Killed++;
        if (dives > 0) B48WithDive++;
        B48DivesPerMin.Add(dives / min);
        B48Times.Add(steps / 120.0);
        B48Staggers += sim.wyrm30Staggers; B48Parries += sim.parry48; B48TellStag += sim.tellStaggers48;
        B48DivesAfter += sim.dives48; B48DivesMissed += sim.divesMissed48; B48CoreFull += sim.coreFullHits48; B48CoreFloor += sim.coreFloorHits48; B48Heals += sim.heals48;
        Console.WriteLine($"  b48 meter fight {B48Fights} ({(w.Alive ? "CAP" : "killed")} at {steps / 120.0:0.0} s): dives {dives} ({dives / min:0.00} a minute), staggers {sim.wyrm30Staggers} (parry {sim.parry48}, in the tell {sim.tellStaggers48}), dives after a stagger {sim.dives48} (dropped {sim.divesMissed48}, windows spent {sim.windowsSpent48}, reopened at the eruption {sim.reopens48}), core hits full {sim.coreFullHits48} / floor {sim.coreFloorHits48}, island heals {sim.heals48}"
            + $" | so far {B48Fights} fights: killed {B48Killed}, time median {B48Median(B48Times):0.0} s, dives a minute median {B48Median(B48DivesPerMin):0.00}, fights with a dive {B48WithDive}/{B48Fights}, staggers {B48Staggers} (parry {B48Parries}, in the tell {B48TellStag}), dives after a stagger {B48DivesAfter} (dropped {B48DivesMissed}), core hits full {B48CoreFull} / floor {B48CoreFloor}, heals {B48Heals}; tells: {string.Join(", ", B48Tells.Select(kv => $"{kv.Key} {kv.Value.n}x {kv.Value.min:0.00}-{kv.Value.max:0.00}"))}");
    }

    /// <summary>NIGHT48 round 2: the boost chain as a player does it (the round-1 chain fired on fixed steps 30 / 75: a shot before the swap to
    /// the THUNDERCLAP had finished was lost, and an island chain steered at the slab's centre from the takeoff, so it rose into the slab's
    /// underside). Phases: 0 the THUNDERCLAP in hand on the ground (up to 1 s; an island hop also walks to the slab's edge, half + 1.6 m out);
    /// 1 the jump, held while rising; at its apex the shot straight down; 2 at the next apex the double jump; 3 at its apex the second shot down;
    /// then the fall. Horizontal (an island hop): outside the slab's footprint until the feet are over its top + 0.3 m, then in to its centre.
    /// Done when the feet land after the takeoff (or 4 s).</summary>
    sealed class B48Hop
    {
        public int phase = -1, steps, boosts, lastAct;
        public bool island, lateTakeoff;
        public double maxUp;
        public Vec3 target;
        double prevY;
        public bool Active => phase >= 0;

        public void Begin(bool toIsland, Vec3 at) { phase = 0; steps = 0; island = toIsland; target = at; maxUp = 0; lateTakeoff = false; }

        public void Step(SurvivalSim sim, ref InputFrame move, ref CombatInput combat, double half, double top)
        {
            if (phase < 0) return;
            steps++;
            Vec3 me = sim.mover.position;
            double y = me.y, floorY = sim.wyrm24 != null && sim.wyrm24.arenaCenter.HasValue ? sim.wyrm24.arenaCenter.Value.y : 0;
            maxUp = Math.Max(maxUp, y - floorY);
            int gun = sim.loadout.owned.IndexOf("shotgun");
            bool sg = sim.loadout.Weapon == "shotgun";
            bool jump = false, fire = false;
            double hd = island ? JsMath.Hypot(target.x - me.x, target.z - me.z) : 0;
            double mz = 0;
            if (island) move.yaw = JsMath.Atan2(-(target.x - me.x), -(target.z - me.z));
            move.moveX = 0;
            move.pitch = -1.5;
            double hs = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z);
            if (phase == 0)
            {
                // round 2 trace: a takeoff while still walking in carried the momentum under the slab, and steering back out drifted 12 m away:
                // stop 0.7-1.4 m outside the slab's edge, then take off with no horizontal speed (the rise is straight up beside the slab)
                if (island) mz = hd > half + 0.9 ? 1 : hd < half + 0.45 ? -1 : 0;
                bool atEdge = !island || (hd <= half + 0.9 && hd >= half + 0.45 && hs < 0.6);
                if ((sg && atEdge && sim.mover.grounded) || steps > 180) { lateTakeoff = steps > 180; phase = 1; jump = true; lastAct = steps; }
            }
            else
            {
                // an apex: the first step that does not rise, at least 7 steps after the last action (a shot's push lands a step later: without
                // the gap the double jump fired on the shot's own apex and overwrote its lift: 5.18 m reached, not 7.9)
                bool apex = steps - lastAct > 6 && y <= prevY && !sim.mover.grounded;
                if (phase == 1 && apex && sg) { fire = true; phase = 2; lastAct = steps; }
                else if (phase == 2 && apex) { jump = true; phase = 3; lastAct = steps; }
                else if (phase == 3 && apex && sg) { fire = true; phase = 4; lastAct = steps; }
                if (island)
                {
                    bool over = y - floorY >= top + 0.05;
                    // round 2 trace: a shot at pitch -1.5 is not straight down, so its push carries a little back from the facing (3.5 -> 4.5 m out
                    // over two shots): while rising, face AWAY from the slab (the push then carries in); once over the top, face it and move in
                    if (!over) move.yaw = JsMath.Atan2(target.x - me.x, target.z - me.z);
                    mz = over ? (hd > 0.6 ? 1 : 0) : 0;
                }
                if (phase >= 1 && steps > 12 && sim.mover.grounded) phase = -1;
                if (steps > 480) phase = -1;
            }
            prevY = y;
            move.moveZ = mz;
            bool rising = !sim.mover.grounded && sim.mover.velocity.y > 0;
            move.jump = new Button { held = jump || (rising && (phase == 1 || phase == 3)), pressed = jump };
            combat = new CombatInput { weapon = !sg && gun >= 0 ? gun : -1, fire = new Button { held = fire, pressed = fire } };
            if (fire && !sim.mover.grounded) boosts++;
        }
    }

    static readonly bool B48PayOn = Array.IndexOf(Environment.GetCommandLineArgs(), "--b48-pay") >= 0;
    static readonly List<double> B48BoostsPerMin = new List<double>();
    static int B48PayFights, B48PayBoosts, B48PayCrack, B48PayCore, B48PayIsland;
    static double B48PayMinutes;

    /// <summary>`n47human --boss wyrm --b48-pay` (G33's meter, NIGHT48.md: "a human pilot that boosts only when it pays"): the boost chain of
    /// --b47-boost (jump, the THUNDERCLAP straight down at the apex, a double jump, the THUNDERCLAP down again: n47b reach's 7.92 m chain), started
    /// only when one of three payoffs is there for a player who knows the rules: (1) the floor is about to be the worm's (a crack tell starts: the
    /// burrow), (2) the core is open AND a shot from the floor lands less than one from the air (kilnBoost48's floorMult under the core's x3; with the
    /// row off the core lands x3 from the floor, so this never pays), (3) an island's health pickup is ready, hp under 75 % and the island within
    /// 14 m (kilnBoost48 on only): the chain steers at it. A boost = a THUNDERCLAP shot fired airborne inside a chain. Run with the same policy on
    /// n47-kiln rows off (the RED control) and with kilnBoost48: boosts a minute per fight and the median over the fights so far.</summary>
    static void H47PolicyB48Pay(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (!B48PayOn || ctx.boss != "wyrm") return;
        SurvivalSim sim = ctx.sim;
        Wyrm w = sim.wyrm24;
        if (w == null) return;
        int St(string k) => ctx.state.TryGetValue(k, out object o) ? (int)o : 0;
        int t = St("b48t"), lastLog = St("b48plog");
        bool crack = false;
        for (int i = lastLog; i < w.log.Count; i++) if (!w.log[i].resolve && w.log[i].name == "crack") crack = true;
        ctx.state["b48plog"] = w.log.Count;
        // one chain per core opening (a player boosts up once and shoots from the air); an island tried at most once every 6 s, within 10 m
        bool coreRise = w.CoreOpen && !(ctx.state.TryGetValue("b48core", out object co) && (bool)co);
        ctx.state["b48core"] = w.CoreOpen;
        if (coreRise) ctx.state["b48coreDue"] = true;
        if (!w.CoreOpen) ctx.state["b48coreDue"] = false;
        bool corePays = w.CoreOpen && ctx.state.TryGetValue("b48coreDue", out object cd) && (bool)cd && sim.KilnBoost48On && sim.cfg.Num("design.n48.kilnBoost48.numbers.floorMult") < 3;
        bool ready = sim.Kiln48ReadyIsland(out Vec3 isl);
        bool island = ready && ctx.step >= St("b48islNext") && sim.health.hp < 0.75 * sim.health.max && JsMath.Hypot(isl.x - sim.mover.position.x, isl.z - sim.mover.position.z) <= 10;
        if (!ctx.state.TryGetValue("b48hop", out object ho)) { ho = new B48Hop(); ctx.state["b48hop"] = ho; }
        var hop = (B48Hop)ho;
        const string KR = "design.n48.kilnBoost48.numbers.";
        if (!hop.Active && sim.mover.grounded && !w.Charging30 && (crack || corePays || island))
        {
            if (crack) B48PayCrack++; else if (corePays) B48PayCore++; else B48PayIsland++;
            if (corePays) ctx.state["b48coreDue"] = false;
            bool toIsland = island && !crack && !corePays;
            if (toIsland) ctx.state["b48islNext"] = ctx.step + 6 * 120;
            hop.Begin(toIsland, isl);
        }
        bool wasActive = hop.Active;
        if (hop.Active) hop.Step(sim, ref move, ref combat, sim.cfg.Num(KR + "islandHalf") - 0.3, sim.cfg.Num(KR + "islandTop"));
        if (wasActive && !hop.Active && hop.island)
        {
            // an island hop's outcome (round 2: 285 hops, 2 heals in the fight while n48wyrm hop lands 8 / 8 alone)
            Vec3 me = sim.mover.position;
            double up = me.y - (w.arenaCenter.HasValue ? w.arenaCenter.Value.y : 0), hd = JsMath.Hypot(hop.target.x - me.x, hop.target.z - me.z);
            string k = Math.Abs(up - sim.cfg.Num(KR + "islandTop")) < 0.5 && hd < 2.3 ? "landed" : hop.maxUp < sim.cfg.Num(KR + "islandTop") + 0.05 ? "too low" : "missed wide";
            B48HopOut.TryGetValue(k, out int c); B48HopOut[k] = c + 1;
            if (hop.lateTakeoff) { B48HopOut.TryGetValue("forced takeoff", out int f); B48HopOut["forced takeoff"] = f + 1; }
        }
        ctx.state["b48n"] = hop.boosts;
        ctx.state["b48t"] = t;
        if (!ReferenceEquals(B48PayCurSim, sim))
        {
            B48PayFlush();
            B48PayCurSim = sim;
            sim.events.On<object>("bossDown", _ => { if (ReferenceEquals(B48PayCurSim, sim) && !sim.wyrm24.Alive) { B48PayCurSteps++; B48PayFlush(); } });
        }
        B48PayCurN = St("b48n");
        B48PayCurSteps = ctx.step + 1;
    }

    static readonly SortedDictionary<string, int> B48HopOut = new SortedDictionary<string, int>();
    static SurvivalSim B48PayCurSim;
    static int B48PayCurN, B48PayCurSteps;

    static void B48PayFlush()
    {
        SurvivalSim sim = B48PayCurSim;
        B48PayCurSim = null;
        if (sim == null || sim.wyrm24 == null) return;
        int n = B48PayCurN;
        double min = Math.Max(1, B48PayCurSteps) / 120.0 / 60.0;
        B48PayFights++;
        B48PayMinutes += min;
        B48PayBoosts += n;
        B48BoostsPerMin.Add(n / min);
        Console.WriteLine($"  b48 pay fight {B48PayFights} ({(sim.wyrm24.Alive ? "CAP" : "killed")} at {B48PayCurSteps / 120.0:0} s): boosts {n} ({n / min:0.00} a minute) | so far: boosts a minute median {B48Median(B48BoostsPerMin):0.00}, pooled {B48PayBoosts / Math.Max(1e-9, B48PayMinutes):0.00}; chains begun for a crack {B48PayCrack}, the core {B48PayCore}, an island {B48PayIsland}; island heals {sim.heals48}; island hops so far: {string.Join(", ", B48HopOut.Select(kv => kv.Key + " " + kv.Value))}");
    }

    /// <summary>`n48brawl stampede`: NIGHT47's n47b stampede (a SURVIVAL run on the survival map, the player invulnerable, every live enemy killed
    /// through the loadout every 30 steps except a stampede's chargers for their first 3 s), plus the HOLD ring: while a HOLD round holds, the player
    /// stands at the ring's spot (an exfil's REACH: at its site). Until --rounds (14) or 600 s. Per round: its type, its seconds, the HOLD's held.</summary>
    static int B48Stampede(string[] args)
    {
        string[] rowsOn = B48Rows(args);
        int until = int.Parse(B48Arg(args, "--rounds", "14"), CultureInfo.InvariantCulture);
        bool old = Array.IndexOf(args, "--old") >= 0;
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        if (rowsOn.Length > 0)
        {
            NightRowState st = sim.rowsRegistry.Defaults();
            foreach (string r in rowsOn) st = st.Set(r, true);
            sim.rowsOverride = st;
        }
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(1, null, null);
        sim.mover.invulnerable = true;
        var si = (ScriptedInput)sim.input;
        int blockedKills = 0, stampedes = 0, bursts = 0, step = 0, herdUntil = -1, holds = 0, held = 0, roundAt = 0, lastRound = sim.director.Round;
        var rounds = new List<int>();
        var lines = new List<string>();
        sim.events.On<object>("stampede47", _ => { stampedes++; herdUntil = step + 360; rounds.Add(sim.director.Round); });
        sim.events.On<object>("chargerBurst47", _ => bursts++);
        sim.events.On<object>("objectiveStart", _ => holds++);
        sim.events.On<object>("objectiveDone", _ => held++);
        var hurtBy = new SortedDictionary<string, double>();
        string endLine = null;
        sim.events.On<HurtEvent>("hurt", e => { string k = e.source ?? "?"; hurtBy.TryGetValue(k, out double v); hurtBy[k] = v + e.amount; });
        sim.events.On<object>("runEnd", _ => endLine ??= $"  the run ended at {step / 120.0:0.0} s in round {sim.director.Round}: health {sim.health.hp:0}, killer {sim.lastDeath?.killer ?? "-"}, at ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0})");
        while (step < 600 * 120 && sim.director.Round < until)
        {
            if (!old && sim.objectives.Holding && sim.objectives.Spot.HasValue)
            {
                Vec3 s = sim.objectives.Spot.Value;
                Vec3 p = sim.mover.position;
                if (JsMath.Hypot(p.x - s.x, p.z - s.z) > 0.5 || Math.Abs(p.y - s.y) > 1.5) sim.Respawn(new Vec3(s.x, s.y + 0.1, s.z), 0);
            }
            else if (!old && sim.exfil != null && sim.exfil.phase == ExfilPhase.Reach)
            {
                Vec3 s = sim.exfil.SitePos;
                Vec3 p = sim.mover.position;
                if (JsMath.Hypot(p.x - s.x, p.z - s.z) > 0.5) sim.Respawn(new Vec3(s.x, s.y + 0.1, s.z), 0);
            }
            if (!old) sim.mover.invulnerable = true; // every step: Mover.Reset (the ring's respawn) and a dash's end clear it (NIGHT47's check set it once)
            si.SetLook(0, 0);
            si.Set(default, default);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (sim.director.Round != lastRound)
            {
                lines.Add($"  round {lastRound} ({DirectorMath.RoundType(lastRound, DirectorMath.From(cfg))}): {(step - roundAt) / 120.0:0.0} s");
                lastRound = sim.director.Round;
                roundAt = step;
            }
            if (step % 30 == 0)
                foreach (Enemy e in sim.enemies.all)
                {
                    if (!e.alive) continue;
                    if (e is Charger && step < herdUntil) continue; // the herd charges first
                    sim.loadout.Damage(e, e.hp + 1e5, false, "revolver", sim.mover.position);
                    if (!old && e.alive)
                    {
                        // a hit from the player's side can be blocked (a SHIELDED plate, a WARDEN's arc: EnemyHits.Filter): hit it from behind
                        Vec3 a = e.position, me = sim.mover.position;
                        blockedKills++;
                        sim.loadout.Damage(e, e.hp + 1e5, false, "revolver", new Vec3(2 * a.x - me.x, me.y, 2 * a.z - me.z));
                    }
                }
        }
        foreach (string l in lines) Console.WriteLine(l);
        if (endLine != null) Console.WriteLine(endLine);
        Console.WriteLine("  damage taken by source: " + (hurtBy.Count == 0 ? "none" : string.Join(", ", hurtBy.Select(kv => $"{kv.Key} {kv.Value:0}"))));
        Console.WriteLine($"  at the end: round {sim.director.Round} ({DirectorMath.RoundType(sim.director.Round, DirectorMath.From(cfg))}), holding {sim.objectives.Holding}, remaining {sim.director.Remaining}, done {sim.director.Done}, run {sim.run.state}, exfil {(sim.exfil == null ? "none" : sim.exfil.phase.ToString())}, enemies alive {sim.enemies.all.Count(e => e.alive)} [{string.Join(", ", sim.enemies.all.Where(e => e.alive).Select(e => $"{e.GetType().Name} {e.kind} hp {e.hp:0} at ({e.position.x:0}, {e.position.y:0}, {e.position.z:0})"))}]");
        Console.WriteLine($"n48brawl stampede ({(old ? "NIGHT47's check, no ring" : "the ring stood in")}; rows [{string.Join(",", rowsOn)}]): round {sim.director.Round} in {step / 120.0:0} s; objectives begun {holds}, holds done {held}, blocked hits re-hit from behind {blockedKills}; stampedes {stampedes} (at rounds {string.Join(", ", rounds)}), chargers sent {sim.stampedeChargers47}, bursts {bursts}");
        return 0;
    }
}
