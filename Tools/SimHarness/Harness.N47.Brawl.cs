// NIGHT47 builder B (NIGHT47.md G25-G35; Research/2026-10-18-wave47/B-cards.md section 5, the PROOF plan): levels 2 and 3's meters, headless.
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n47b" (Harness.N46.Flow.cs's pattern;
// no older harness file touched; helpers carry the B47 prefix). Round 0 (baselines, rows off):
//   n47b wyrm [--level n46-kiln] [--seeds 10] [--cap 300] [--rows r1,r2]
//     n46wyrm fight's setup (B1Start: DOORFREE, the player at k5's spawn, WyrmPilot31 fights): per seed the kill time and, from the worm's own
//     tell log (Wyrm.log: every Tell's name and seconds), each tell's count and its length; the share of the fight's steps in each pattern
//     (Wyrm.PatternName) and how many times each pattern started; the burrows (DIVE starts) per minute (G34's baseline); the THUNDERCLAP boosts
//     (a shotgun shot fired while airborne: Combat.cs:992's ShotgunJump43 line) per minute (G33's baseline). Summary over seeds.
//   n47b reach [--level n46-kiln]
//     the height the player reaches from flat floor (kb3 CASTING FLOOR's spawn, 18 m roof; the husks frozen): a jump, a jump + double jump,
//     a jump + one THUNDERCLAP shot straight down at the apex, a jump + shot + double jump + shot (the kiln43 chain), so G33's "heights only
//     the boost clears" has a measured line (the islands sit >= 1.5 m above the double jump's apex).
//   n47b charger [--seeds 40]
//     G26's late-vs-early dodge on one CHARGER in N36 Core's sandbox (B47Charger's summary below). N47B_TRACE=1 prints one line a seed.
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
    internal static void B47Init()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (Array.IndexOf(argv, "--b47-gauntlet-only") >= 0) SurvivalSim.Probe47GauntletOnly = true; // G27's policy, for any command (n46flow)
        if (Array.IndexOf(argv, "--b47-mixed") >= 0) SurvivalSim.Probe47Mixed = true; // G27's mixed policy (the KESTREL + the gauntlet owned)
        if (argv.Length < 2 || argv[1] != "n47b") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try { code = B47Main(args); }
        catch (Exception e) { Console.WriteLine("n47b: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string B47Arg(string[] args, string name, string dflt)
    {
        int i = Array.LastIndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : dflt;
    }

    static int B47Main(string[] args)
    {
        string what = args.Length > 1 ? args[1] : "";
        string level = B47Arg(args, "--level", "n46-kiln");
        int seeds = int.Parse(B47Arg(args, "--seeds", "10"), CultureInfo.InvariantCulture);
        double cap = double.Parse(B47Arg(args, "--cap", "300"), CultureInfo.InvariantCulture);
        string rows = B47Arg(args, "--rows", "");
        B1Rows = rows.Length > 0 ? rows.Split(',') : new string[0];
        B1Walk = false;
        if (what == "wyrm") return B47Wyrm(level, seeds, cap);
        if (what == "reach") return B47Reach(level);
        if (what == "charger") return B47Charger(seeds);
        if (what == "brawlcheck") return B47BrawlCheck();
        if (what == "beats") return B47Beats(B47Arg(args, "--level", "n47-kiln"));
        if (what == "stampede") return B47Stampede();
        if (what == "trail") return B47Trail();
        Console.WriteLine("n47b: unknown '" + what + "' (wyrm, reach, charger)");
        return 2;
    }

    static string B47F(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "n/a" : v.ToString("0.00", CultureInfo.InvariantCulture);

    static int B47Wyrm(string level, int seeds, double cap)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", level + ".json");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        var hurtBy = new SortedDictionary<string, double>(); // the player's damage taken by source (the tail's deaths)
        var tells = new SortedDictionary<string, (int n, double min, double max)>();
        var patSteps = new SortedDictionary<string, long>();
        var patStarts = new SortedDictionary<string, int>();
        var times = new List<double>();
        var divesPerMin = new List<double>();
        var boostsPerMin = new List<double>();
        long allSteps = 0;
        int withDive = 0, fights = 0;
        for (int s = 0; s < seeds; s++)
        {
            SurvivalSim sim = B1Start(cfg, path, (uint)(1 + s), out RoomDef k5);
            if (sim == null) { Console.WriteLine($"n47b wyrm seed {s + 1}: NO FIGHT (level {level})"); continue; }
            fights++;
            Wyrm w = sim.wyrm24;
            sim.events.On<HurtEvent>("hurt", e => { string k = e.source ?? "?"; hurtBy.TryGetValue(k, out double v); hurtBy[k] = v + e.amount; });
            var si = (ScriptedInput)sim.input;
            var wp = new WyrmPilot31();
            int step = 0, capSteps = (int)(cap * 120), dives = 0, boosts = 0, logFrom = w.log.Count;
            string prevPat = null;
            var seedStarts = new SortedDictionary<string, int>();
            while (w.Alive && step < capSteps)
            {
                wp.Step(sim, step);
                si.SetLook(wp.Move.yaw, wp.Move.pitch);
                si.Set(wp.Move, wp.Combat);
                bool air = !sim.mover.grounded;
                string gunBefore = sim.loadout.Weapon;
                int firedBefore = sim.events.Emitted.TryGetValue("shotResult", out int fb) ? fb : 0;
                sim.Step();
                sim.hitstopLeft = 0;
                step++;
                int firedAfter = sim.events.Emitted.TryGetValue("shotResult", out int fa) ? fa : 0;
                if (air && gunBefore == "shotgun" && firedAfter > firedBefore) boosts++;
                string pat = w.PatternName ?? "-";
                patSteps.TryGetValue(pat, out long ps); patSteps[pat] = ps + 1;
                if (pat != prevPat)
                {
                    patStarts.TryGetValue(pat, out int pn); patStarts[pat] = pn + 1;
                    seedStarts.TryGetValue(pat, out int sn); seedStarts[pat] = sn + 1;
                    if (pat == "dive") dives++;
                }
                prevPat = pat;
            }
            allSteps += step;
            for (int i = logFrom; i < w.log.Count; i++)
            {
                var e = w.log[i];
                if (e.resolve || e.name == null) continue;
                tells.TryGetValue(e.name, out var t);
                tells[e.name] = (t.n + 1, t.n == 0 ? e.seconds : Math.Min(t.min, e.seconds), Math.Max(t.max, e.seconds));
            }
            double mins = step / 120.0 / 60.0;
            bool killed = !w.Alive;
            if (killed) times.Add(step / 120.0);
            if (dives > 0) withDive++;
            divesPerMin.Add(dives / mins);
            boostsPerMin.Add(boosts / mins);
            Console.WriteLine($"n47b wyrm seed {s + 1}: {(killed ? "killed" : "ALIVE")} in {step / 120.0:0.0} s; burrows (dive starts) {dives} ({B47F(dives / mins)} a min); boosts {boosts}; pattern starts {string.Join(" ", seedStarts.Select(kv => kv.Key + " " + kv.Value))}");
        }
        times.Sort();
        double Med(List<double> l) { var o = l.OrderBy(x => x).ToList(); return o.Count == 0 ? double.NaN : o[o.Count / 2]; }
        Console.WriteLine($"n47b wyrm SUMMARY (level {level}, rows [{string.Join(",", B1Rows)}]): killed {times.Count}/{fights}, median kill {B47F(Med(times))} s");
        Console.WriteLine($"  tells (Wyrm.log; name: count, min-max s): {string.Join(", ", tells.Select(kv => $"{kv.Key} {kv.Value.n}x {B47F(kv.Value.min)}-{B47F(kv.Value.max)}"))}");
        Console.WriteLine($"  under 0.8 s: {string.Join(", ", tells.Where(kv => kv.Value.min < 0.8 - 1e-9).Select(kv => kv.Key))}");
        Console.WriteLine($"  pattern share of the fight's steps: {string.Join(", ", patSteps.Select(kv => $"{kv.Key} {kv.Value * 100.0 / Math.Max(1, allSteps):0.0} %"))}");
        Console.WriteLine($"  pattern starts (all seeds): {string.Join(", ", patStarts.Select(kv => $"{kv.Key} {kv.Value}"))}");
        Console.WriteLine($"  burrows a minute: median {B47F(Med(divesPerMin))}; fights with >= 1 burrow {withDive}/{fights}");
        Console.WriteLine($"  THUNDERCLAP boosts a minute (WyrmPilot31): median {B47F(Med(boostsPerMin))}");
        Console.WriteLine($"  damage taken by source (all seeds): {string.Join(", ", hurtBy.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value:0}"))}");
        return 0;
    }

    /// <summary>`n47b charger` (G26's late-vs-early dodge): B47Sandbox (n46-iceline g1, the player invulnerable at its spawn, only the
    /// check's own bodies alive; the ON game's rows: tells36 1.2 s), one CHARGER 10-14 m straight ahead (the seed picks the distance),
    /// the player facing it and still until ONE sidestep (moveX 1 for 0.4 s, then still) starts: "early" 0.6 s before the release,
    /// "late" 0.2 s before it (inside the card's 0.25 s lock), "still" never. A hit = the charger in its charge within its hitRange
    /// (horizontal) of the player's feet. Today the line locks at the wind-up's start (Charger.cs:265) and never turns, so both strafes are
    /// expected to dodge it: this is the RED line charger47 must split (late >= 90 % dodged, early <= 30 %).</summary>
    /// <summary>The sidestep (s), and when it starts before the charge's release (s): early = when the line is first read, late = inside
    /// charger47's 0.25 s lock (harness-side only).</summary>
    const double B47SideS = 0.4, B47EarlyS = 0.6, B47LateS = 0.2;

    static int B47Charger(int seeds)
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        double hitRange = cfg.Num("charger.hitRange");
        string[] modes = { "still", "early", "late", "husk" };
        foreach (string mode in modes)
        {
            int hits = 0, charges = 0, bursts = 0;
            for (int s = 0; s < seeds; s++)
            {
                var rnd = new Random(4700 + s); // harness-side only (not sim code)
                double dist = 10 + rnd.NextDouble() * 4;
                SurvivalSim sim = B47Sandbox(cfg, (uint)(4700 + s), out Vec3 sp, out double yaw, out double ux, out double uz);
                Husk c = Core36Spawn(sim, "charger", sp, ux, uz, dist, 1e9);
                Husk body = mode == "husk" ? Core36Spawn(sim, "husk", sp, ux, uz, dist * 0.5, 100) : null; // "husk": a still player behind a husk in the line (G26: a charge bursts it)
                if (body != null) body.cooldownSteps = 99999;
                if (!(c is Charger ch)) { Console.WriteLine($"n47b charger {mode} seed {s + 1}: no charger placed"); continue; }
                // one sidestep of B47SideS s (a strafe, then still): early = it starts B47EarlyS s before the release, late = B47LateS s before
                int earlySteps = MovementMath.StepsFor(B47EarlyS, dt), lateSteps = MovementMath.StepsFor(B47LateS, dt);
                int tellFull = -1, strafeLeft = -1;
                bool strafing = false, charged = false, hit = false;
                string prev = ch.state;
                for (int i = 0; i < MovementMath.StepsFor(8, dt) && !charged; i++)
                {
                    if (ch.state == "tell")
                    {
                        if (tellFull < 0) tellFull = ch.stateSteps;
                        if (strafeLeft < 0 && mode == "early" && ch.stateSteps <= earlySteps) strafeLeft = MovementMath.StepsFor(B47SideS, dt);
                        if (strafeLeft < 0 && mode == "late" && ch.stateSteps <= lateSteps) strafeLeft = MovementMath.StepsFor(B47SideS, dt);
                    }
                    strafing = strafeLeft > 0;
                    if (strafeLeft > 0) strafeLeft--;
                    if (ch.state == "charge")
                    {
                        Vec3 p = sim.mover.position;
                        double hx = ch.position.x - p.x, hz = ch.position.z - p.z;
                        if (Math.Sqrt(hx * hx + hz * hz) <= hitRange) hit = true;
                    }
                    if (prev == "charge" && ch.state != "charge") charged = true;
                    prev = ch.state;
                    var f = new InputFrame { yaw = yaw, pitch = 0, moveX = strafing ? 1 : 0 };
                    Core36Tick(sim, yaw, 0, NoCombat36, f);
                }
                if (charged) charges++;
                if (hit) hits++;
                if (body != null && !body.alive) bursts++;
                if (Environment.GetEnvironmentVariable("N47B_TRACE") == "1")
                    Console.WriteLine($"    [{mode} seed {s + 1}] dist {dist:0.0}, tell steps {tellFull}, charged {charged}, hit {hit}, state now {ch.state}, charger at ({ch.position.x:0.0},{ch.position.z:0.0}) player ({sim.mover.position.x:0.0},{sim.mover.position.z:0.0})");
            }
            Console.WriteLine($"n47b charger ({(B1Rows.Length > 0 ? string.Join(",", B1Rows) : "rows off")}): {mode}: hit {hits} of {charges} charges ({(charges > 0 ? 100.0 * (charges - hits) / charges : 0):0} % dodged){(mode == "husk" ? $", the husk in its line burst {bursts} of {seeds}" : "")}, {seeds} seeds, 10-14 m");
        }
        return 0;
    }

    /// <summary>Core36Sim's sandbox (the survival map, tells36 on, the player invulnerable, every run body dead) with --rows forced on before the
    /// run starts (Core36Sim itself cannot take rows: its StartRun is inside).</summary>
    static SurvivalSim B47Sandbox(GameConfig cfg, uint seed, out Vec3 sp, out double yaw, out double ux, out double uz)
    {
        // level 2's first room (n46-iceline g1 TANK ALONE: 22 x 26 m, open between its two ride panels at x +-8): the player at its spawn
        // (0, 0, -2) facing -z, the charger straight ahead; the room's own wave is killed each step (Core36Tick keeps only Core36Keep)
        Core36Keep.Clear();
        string path = Path.Combine(MapGenRepo, "Design", "levels", "n46-iceline.json");
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        if (B1Rows.Length > 0)
        {
            NightRowState st = sim.rowsRegistry.Defaults();
            foreach (string r in B1Rows) st = st.Set(r, true);
            sim.rowsOverride = st;
        }
        sim.StartRun(seed, null, null);
        for (int i = 0; i < 10; i++) sim.Step();
        sim.campaign.SetDoorFree29(true);
        sim.mover.invulnerable = true;
        sp = new Vec3(0, 0, -2);
        sim.Respawn(sp, 0);
        foreach (Enemy e in sim.enemies.all) e.alive = false;
        for (int i = 0; i < 30; i++) Core36Tick(sim, 0, 0, NoCombat36);
        yaw = 0; ux = 0; uz = -1;
        return sim;
    }

    /// <summary>`n47b brawlcheck [--rows brawl47]`: n47human's THE ICEBREAKER setup (the survival map's arena), the bare Mixed pilot, one seed:
    /// prints brawl47's numbers as the fight reads them, the phase changes, the chargers sent and their hits (the row's plumbing).</summary>
    static int B47BrawlCheck()
    {
        GameConfig cfg = Config();
        SurvivalSim.Brawl40Dir = MapGenRepo;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        if (B1Rows.Length > 0)
        {
            NightRowState st = sim.rowsRegistry.Defaults();
            foreach (string r in B1Rows) st = st.Set(r, true);
            sim.rowsOverride = st;
        }
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(1, null, null);
        if (!sim.Brawl40Place(Brawl40At)) { Console.WriteLine("n47b brawlcheck: no arena"); return 1; }
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        sim.Respawn(sim.brawl40Arena.spawn + Brawl40At, 0);
        Vec3 spawn = default;
        double yaw0 = 0;
        if (!sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw0)) { Console.WriteLine("n47b brawlcheck: no begin"); return 1; }
        Brawl40 g = sim.brawl40;
        var pilot = new Brawl40Pilot { hand = new Mulberry32(7919u + 40u), style = Brawl40Pilot.Style.Mixed };
        var si = (ScriptedInput)sim.input;
        double yawL = 0;
        int step = 0, changes = 0;
        bool p3 = false;
        while (g.Alive && step < 120 * 300)
        {
            pilot.Step(sim, yawL, step);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (step == 2) Console.WriteLine($"n47b brawlcheck: Brawl47On {sim.Brawl47On}; p.phase2At {g.p.phase2At}, phase3At47 {g.p.phase3At47}, rest {g.p.restSeconds} / {g.p.restSeconds2} / rest2 {g.p42?.rest2}, meter [{string.Join(",", g.p.stagger)}], rush {g.p.rushWindup}, block {g.p.blockWindup}");
            if (g.phaseChanges != changes) { changes = g.phaseChanges; Console.WriteLine($"  {step / 120.0:0.0} s: phase change {changes} (phase {g.phase}, P3 {g.Phase3_47}, bar {g.HpFrac:0.00})"); }
            if (g.Phase3_47 && !p3) p3 = true;
        }
        Console.WriteLine($"n47b brawlcheck: {(g.Alive ? "ALIVE" : "killed")} in {step / 120.0:0.0} s; windows {g.windows}; chargers sent {sim.brawl47Sent}, charger hits {sim.brawl47ChargerHits}; armour broken {g.armorBroken47}, timed out {g.armorTimedOut47}");
        return 0;
    }

    /// <summary>`n47b beats [--level n47-kiln] [--rows wyrm47]` (G35): the three foreshadow beats' triggers on level 3 (DOORFREE, the husks
    /// frozen): the GLIMPSE (the feet put 8.5 m over kb1's spawn), the PASS (7 s in kb2 from its spawn), the CAMEO twice in kb3 standing still
    /// (both should hit) and twice jumping 0.3 s before each breach (none should hit; the jump 0.3 s before). RED with the row off (no beat).</summary>
    static int B47Beats(string level)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", level + ".json");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        string line = "";
        foreach (string mode in new[] { "still", "jump" })
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noLab = sim.noRecords = true;
            sim.setup.mode = Campaign24.Mode;
            if (B1Rows.Length > 0)
            {
                NightRowState st = sim.rowsRegistry.Defaults();
                foreach (string r in B1Rows) st = st.Set(r, true);
                sim.rowsOverride = st;
            }
            sim.StartRun(1, null, null);
            sim.husks.frozen = true;
            for (int i = 0; i < 10; i++) sim.Step();
            sim.campaign.SetDoorFree29(true);
            var si = (ScriptedInput)sim.input;
            RoomDef Room(string id) => sim.campaignLevel.rooms.Find(r => r.id == id);
            void Run(double s, Func<int, bool> jump)
            {
                for (int i = 0; i < (int)(s * 120); i++)
                {
                    bool j = jump != null && jump(i);
                    si.SetLook(0, 0);
                    si.Set(new InputFrame { yaw = 0, pitch = 0, jump = new Button { held = j, pressed = j } }, default);
                    sim.Step();
                }
            }
            if (mode == "still")
            {
                RoomDef kb1 = Room("kb1"), kb2 = Room("kb2");
                sim.Respawn(new Vec3(kb1.spawn.x, kb1.spawn.y + 8.5, kb1.spawn.z - 3), 0);
                Run(0.5, null);
                sim.Respawn(kb2.spawn, 0);
                Run(7, null);
            }
            RoomDef kb3 = Room("kb3");
            sim.Respawn(kb3.spawn, 0);
            int lead = MovementMath.StepsFor(0.25, cfg.Dt);
            Run(16, i => mode == "jump" && sim.Cameo47Telling && sim.Cameo47Progress >= 1.0 - 0.3 / Math.Max(0.1, cfg.Num("design.n47.wyrm47.numbers.cameoTellS")));
            line += $"n47b beats ({level}, rows [{string.Join(",", B1Rows)}], {mode}): glimpse {sim.beats47Glimpse}, pass {sim.beats47Pass}, cameo breaches {sim.beats47Cameo}, cameo hits {sim.beats47CameoHits}\n";
        }
        Console.Write(line);
        return 0;
    }

    // ---- n47human policies (the lead's hook, found by reflection; each runs only with its flag on the command line, so nobody else's run changes)
    static readonly bool B47TallyOn = Array.IndexOf(Environment.GetCommandLineArgs(), "--b47-tally") >= 0;
    static readonly bool B47BoostOn = Array.IndexOf(Environment.GetCommandLineArgs(), "--b47-boost") >= 0;
    static readonly Dictionary<string, long> B47TallyAll = new Dictionary<string, long>();
    static int B47TallyFights, B47BoostFights;
    static double B47BoostMinutes;
    static long B47BoostsAll;

    /// <summary>`n47human --boss wyrm --b47-tally`: what hits the player in THE KILN WYRM's fight (the tail's deaths): the worm's own hit
    /// counters (Wyrm.stats: contactsBody / contactsHead / contactsLunge / shotsHit; Wyrm31: sweepHits, spitHits) and the player's deaths, per
    /// fight when it ends (a kill or the 300 s cap) and summed over the fights so far. Changes no input.</summary>
    static void H47PolicyB47Tally(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (!B47TallyOn || ctx.boss != "wyrm") return;
        SurvivalSim sim = ctx.sim;
        Wyrm w = sim.wyrm24;
        if (w == null) return;
        if (!ctx.state.ContainsKey("b47deaths")) { ctx.state["b47deaths"] = 0; ctx.state["b47dead"] = false; ctx.state["b47done"] = false; }
        bool dead = sim.health.Dead;
        if (dead && !(bool)ctx.state["b47dead"]) ctx.state["b47deaths"] = (int)ctx.state["b47deaths"] + 1;
        ctx.state["b47dead"] = dead;
        bool end = !w.Alive || ctx.step >= 300 * 120 - 2;
        if (!end || (bool)ctx.state["b47done"]) return;
        ctx.state["b47done"] = true;
        var t = new Dictionary<string, long>();
        foreach (string k in new[] { "contactsBody", "contactsHead", "contactsLunge", "shotsHit", "aggroHits" }) t[k] = w.stats.TryGetValue(k, out int v) ? v : 0;
        if (sim.wyrm31 != null) { t["sweepHits"] = sim.wyrm31.sweepHits; t["spitHits31"] = sim.wyrm31.spitHits; }
        t["deaths"] = (int)ctx.state["b47deaths"];
        B47TallyFights++;
        foreach (var kv in t) { B47TallyAll.TryGetValue(kv.Key, out long s); B47TallyAll[kv.Key] = s + kv.Value; }
        Console.WriteLine($"  b47 tally fight {B47TallyFights} ({(w.Alive ? "CAP" : "killed")} at {ctx.step / 120.0:0} s): {string.Join(", ", t.Select(kv => kv.Key + " " + kv.Value))} | so far: {string.Join(", ", B47TallyAll.Select(kv => kv.Key + " " + kv.Value))}");
    }

    static int B47PolicyBoostState(H47Ctx ctx, string k) => ctx.state.TryGetValue(k, out object o) ? (int)o : 0;

    /// <summary>`n47human --boss wyrm --b47-boost` (G33's meter: the same policy in both arenas): when the worm's core is open, or one of the
    /// worm's floor tells has just started (the crack where it dives), and the player is on the floor: a boost chain (jump, the THUNDERCLAP
    /// straight down at the apex, a double jump, the THUNDERCLAP down again: n47b reach's 7.92 m chain) while aiming at the head the rest of the
    /// time; a "boost" = a THUNDERCLAP shot fired airborne inside the chain. Prints boosts a minute per fight and over the fights so far.</summary>
    static void H47PolicyB47Boost(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (!B47BoostOn || ctx.boss != "wyrm") return;
        SurvivalSim sim = ctx.sim;
        Wyrm w = sim.wyrm24;
        if (w == null) return;
        int t = B47PolicyBoostState(ctx, "b47t"); // steps into the chain (0: none)
        int lastLog = B47PolicyBoostState(ctx, "b47log");
        bool floorTell = false;
        for (int i = lastLog; i < w.log.Count; i++) if (!w.log[i].resolve && w.log[i].name == "crack") floorTell = true;
        ctx.state["b47log"] = w.log.Count;
        if (t == 0 && sim.mover.grounded && (w.CoreOpen || floorTell) && !w.Charging30) t = 1;
        if (t > 0)
        {
            int gun = sim.loadout.owned.IndexOf("shotgun");
            bool fire = false, jump = false;
            if (t == 1 || t == 50) jump = true; // takeoff, then the double jump near the first boost's apex
            if (t == 30 || t == 75) fire = true; // the THUNDERCLAP straight down
            move.pitch = -1.5;
            move.jump = new Button { held = jump || (t > 1 && t < 30) || (t > 50 && t < 75), pressed = jump };
            combat = new CombatInput { weapon = sim.loadout.Weapon != "shotgun" && gun >= 0 ? gun : -1, fire = new Button { held = fire, pressed = fire } };
            if (fire && !sim.mover.grounded && sim.loadout.Weapon == "shotgun") ctx.state["b47n"] = B47PolicyBoostState(ctx, "b47n") + 1;
            t++;
            if (t > 110) t = 0;
        }
        ctx.state["b47t"] = t;
        bool end = !w.Alive || ctx.step >= 300 * 120 - 2;
        if (end && !ctx.state.ContainsKey("b47end"))
        {
            ctx.state["b47end"] = true;
            int n = B47PolicyBoostState(ctx, "b47n");
            double min = Math.Max(1, ctx.step) / 120.0 / 60.0;
            B47BoostFights++;
            B47BoostMinutes += min;
            B47BoostsAll += n;
            Console.WriteLine($"  b47 boost fight {B47BoostFights} ({sim.campaignLevel?.id}, {(w.Alive ? "CAP" : "killed")} at {ctx.step / 120.0:0} s): boosts {n} ({n / min:0.00} a minute) | so far {B47BoostsAll / Math.Max(1e-9, B47BoostMinutes):0.00} a minute over {B47BoostFights} fights");
        }
    }

    /// <summary>`n47b stampede [--rows stampede47,charger47]` (round 4): a SURVIVAL run on the survival map, the player invulnerable and still,
    /// every live enemy killed through the loadout each step except a stampede's chargers for their first 3 s (so they charge), until round 14
    /// or 600 s: the rounds reached, the stampedes sent (event stampede47) and their chargers, the bursts (chargerBurst47).</summary>
    static int B47Stampede()
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        if (B1Rows.Length > 0)
        {
            NightRowState st = sim.rowsRegistry.Defaults();
            foreach (string r in B1Rows) st = st.Set(r, true);
            sim.rowsOverride = st;
        }
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(1, null, null);
        sim.mover.invulnerable = true;
        var si = (ScriptedInput)sim.input;
        int stampedes = 0, bursts = 0, step = 0, herdUntil = -1;
        var rounds = new List<int>();
        sim.events.On<object>("stampede47", _ => { stampedes++; herdUntil = step + 360; rounds.Add(sim.director.Round); });
        sim.events.On<object>("chargerBurst47", _ => bursts++);
        while (step < 600 * 120 && sim.director.Round < 14)
        {
            si.SetLook(0, 0);
            si.Set(default, default);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (step % 30 == 0)
                foreach (Enemy e in sim.enemies.all)
                {
                    if (!e.alive) continue;
                    if (e is Charger && step < herdUntil) continue; // the herd charges first
                    sim.loadout.Damage(e, e.hp + 1e5, false, "revolver", sim.mover.position);
                }
        }
        Console.WriteLine($"n47b stampede (rows [{string.Join(",", B1Rows)}]): round {sim.director.Round} in {step / 120.0:0} s; stampedes {stampedes} (at rounds {string.Join(", ", rounds)}), chargers sent {sim.stampedeChargers47}, bursts {bursts}");
        return 0;
    }

    /// <summary>`n47b trail [--rows wyrm47,trail47]` (round 4): n47b wyrm's bot fight (WyrmPilot31, n46-kiln, 10 seeds): the trail's burns on
    /// the player and the fight's time (the RED line: the row off, no burn).</summary>
    static int B47Trail()
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", "n46-kiln.json");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        var times = new List<double>();
        int burns = 0;
        for (int s = 0; s < 10; s++)
        {
            SurvivalSim sim = B1Start(cfg, path, (uint)(1 + s), out RoomDef k5);
            if (sim == null) continue;
            var si = (ScriptedInput)sim.input;
            var wp = new WyrmPilot31();
            int step = 0;
            while (sim.wyrm24.Alive && step < 300 * 120)
            {
                wp.Step(sim, step);
                si.SetLook(wp.Move.yaw, wp.Move.pitch);
                si.Set(wp.Move, wp.Combat);
                sim.Step();
                sim.hitstopLeft = 0;
                step++;
            }
            times.Add(step / 120.0);
            burns += sim.trailBurns47;
        }
        times.Sort();
        Console.WriteLine($"n47b trail (rows [{string.Join(",", B1Rows)}]): 10 fights, median {B47F(times[times.Count / 2])} s, trail burns on the player {burns}");
        return 0;
    }

    static int B47Reach(string level)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", level + ".json");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        string[] names = { "jump", "jump + double jump", "jump + shot down at the apex", "jump + shot + double jump + shot" };
        for (int mode = 0; mode < names.Length; mode++)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noLab = sim.noRecords = true;
            sim.setup.mode = Campaign24.Mode;
            if (B1Rows.Length > 0)
            {
                NightRowState st = sim.rowsRegistry.Defaults();
                foreach (string r in B1Rows) st = st.Set(r, true);
                sim.rowsOverride = st;
            }
            sim.StartRun(1, null, null);
            sim.husks.frozen = true;
            for (int i = 0; i < 10; i++) sim.Step();
            sim.campaign.SetDoorFree29(true);
            RoomDef room = sim.campaignLevel.rooms.Find(r => r.id == "kb3");
            if (!sim.loadout.owned.Contains("shotgun")) sim.loadout.AddWeapon("shotgun");
            sim.Respawn(room.spawn, room.spawnYawDeg);
            var si = (ScriptedInput)sim.input;
            int gun = sim.loadout.owned.IndexOf("shotgun");
            // settle on the floor with the THUNDERCLAP in hand
            for (int i = 0; i < 120; i++)
            {
                si.SetLook(0, -1.5);
                si.Set(new InputFrame { yaw = 0, pitch = -1.5 }, new CombatInput { weapon = sim.loadout.Weapon != "shotgun" ? gun : -1 });
                sim.Step();
            }
            double y0 = sim.mover.position.y, top = y0, prevY = y0;
            int shots = 0, jumps = 0, next = 0;
            // the actions, one at the takeoff and one at each later apex: J = a jump press (held while rising), S = a shot straight down
            string[][] plan = { new[] { "J" }, new[] { "J", "J" }, new[] { "J", "S" }, new[] { "J", "S", "J", "S" } };
            string[] acts = plan[mode];
            int sinceAct = 0;
            for (int i = 0; i < 480; i++)
            {
                bool press = false, fire = false;
                double y = sim.mover.position.y;
                bool apex = i == 0 || (sinceAct > 6 && y <= prevY); // the takeoff, then the first step that does not rise
                if (apex && next < acts.Length)
                {
                    if (acts[next] == "J") { press = true; jumps++; } else { fire = true; shots++; }
                    next++;
                    sinceAct = 0;
                }
                else sinceAct++;
                bool held = press || (sinceAct > 0 && y > prevY); // a held jump while rising (the variable jump height)
                prevY = y;
                si.SetLook(0, -1.5);
                si.Set(new InputFrame { yaw = 0, pitch = -1.5, jump = new Button { held = held, pressed = press } }, new CombatInput { fire = new Button { held = fire, pressed = fire } });
                sim.Step();
                top = Math.Max(top, sim.mover.position.y);
                if (i > 10 && sim.mover.grounded) break;
            }
            Console.WriteLine($"n47b reach ({level} kb3 floor, rows [{string.Join(",", B1Rows)}]): {names[mode]}: apex {top - y0:0.00} m above the floor (jumps {jumps}, shots {shots})");
        }
        return 0;
    }
}
