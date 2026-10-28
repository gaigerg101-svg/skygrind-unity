// NIGHT45 builder A2 (NIGHT45.md card A2: g7 THE MIX as THE THAW LOCK in n45-iceline; docs/ROOMS.md's identity card). Headless proof, hooked without a
// Program.cs line: a module initializer runs the command and exits when argv[1] starts with "n45mix" (NIGHT44 R2's pattern; no lead file touched).
//   n45mix level [--level <id>]
//       the level's g7 as built: its lava zones, the ice surface, the boiler, the gantries, nodes / highNodes / waveNodes43, and N45Levels.BaseId of the id.
//   n45mix walk [--level <id>] [--seeds N] [--cap S]
//       the walk bot (Walk29Pilot through A2MixInput: NIGHT40's iceline blast script copied, asking N45Levels.BaseId, so it runs on n43 / n45 too; the
//       NIGHT40 one matches only "n40-iceline") from g6's door into g7 until g7 clears; per seed: cleared, time, deaths, the player's lava burns, the
//       enemies the grate killed (lavaEnemyDeath), FUSIONs. Run it with --level n43-iceline for the before.
//   n45mix jump [--level <id>]   (the ice apron of the first cut is gone: the ground dash way now wants "not over")
//       the card's earned shortcut (M-5): the real Mover in the level from the apron over the grate's middle lane (x -4.5) three ways: a plain run's jump at
//       the lip, a dash on the ice then the jump, a run's jump with an air dash. Per way: the speed at take-off, where the feet land, lava burns.
//   n45mix all [--level <id>]   level + jump + walk (3 seeds)
// Default level n45-iceline (Design/levels/<id>.json). Every helper carries A2 / Mix in its name (NIGHT43's N43Arg clash).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void A2N45MixInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || !argv[1].StartsWith("n45mix")) return;
        string[] args = argv.Skip(1).ToArray();
        int code = 0;
        try { code = A2Mix(args); }
        catch (Exception e) { Console.WriteLine("n45mix: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string A2MixArg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static int A2Mix(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        string level = A2MixArg(args, "--level", "n45-iceline");
        int seeds = int.Parse(A2MixArg(args, "--seeds", what == "all" ? "3" : "5"));
        double cap = double.Parse(A2MixArg(args, "--cap", "240"), System.Globalization.CultureInfo.InvariantCulture);
        A2MixTrace = Array.IndexOf(args, "--trace") >= 0;
        int fails = 0;
        if (what == "level" || what == "all") fails += A2MixLevel(level);
        if (what == "jump" || what == "all") fails += A2MixJump(level);
        if (what == "walk" || what == "all") fails += A2MixWalk(level, seeds, cap);
        Console.WriteLine(fails == 0 ? $"n45mix {what} {level}: GREEN" : $"n45mix {what} {level}: RED ({fails} failing)");
        return fails == 0 ? 0 : 1;
    }

    static bool A2MixTrace;
    static string A2MixPath(string level) => "Design/levels/" + level + ".json";

    static SurvivalSim A2MixSim(string level)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(A2MixPath(level)))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n40Override = new N40Blocks { iceline40 = true, foes40 = true, brawl40 = false }; // as `n40ice walk` (L2): the gauntlet curriculum, foes40 on
        return sim;
    }

    /// <summary>Start a run and enter room `roomId` as a door would (Campaign24.Enter by reflection, `n40ice walk --from`'s way); past g6 the KESTREL.</summary>
    static int A2MixEnter(SurvivalSim sim, LevelDef lv, string roomId, uint seed)
    {
        sim.StartRun(seed, null, null);
        AutopilotInput.TakeDraft(sim);
        sim.Step();
        sim.husks.KillAll();
        sim.brain.Clear();
        int from = lv.rooms.FindIndex(r => r.id == roomId);
        typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { from });
        sim.Respawn(lv.rooms[from].spawn, 0);
        if (from > lv.rooms.FindIndex(r => r.id == "g6") && !sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        return from;
    }

    // ---- level --------------------------------------------------------------------------------------------------------------------------------
    static int A2MixLevel(string level)
    {
        var json = PortData.Obj(PortData.LoadJson(A2MixPath(level)));
        GameConfig cfg = Config();
        LevelDef lv = LevelDef.From(json);
        RoomDef g7 = lv.rooms.Find(r => r.id == "g7");
        int lava = 0, ice = 0;
        if (json.ContainsKey("lava")) foreach (object o in PortData.Arr(json["lava"])) if (PortData.Obj(o).TryGetValue("room", out object rm) && rm as string == "g7") lava++;
        var camp = PortData.Obj(json["campaign"]);
        if (camp.ContainsKey("surfaces")) foreach (object o in PortData.Arr(camp["surfaces"])) if (PortData.Obj(o).TryGetValue("room", out object rm) && rm as string == "g7") ice++;
        int boxes = 0;
        if (camp.ContainsKey("boxes")) foreach (object o in PortData.Arr(camp["boxes"])) if (PortData.Obj(o).TryGetValue("zone", out object z) && z as string == "g7") boxes++;
        bool waveNodes = false;
        foreach (object o in PortData.Arr(camp["rooms"])) { var r = PortData.Obj(o); if (r["id"] as string == "g7") waveNodes = r.ContainsKey("waveNodes43"); }
        string baseId = N45Levels.BaseId(cfg, level);
        Console.WriteLine($"n45mix level {level} (BaseId {baseId}): g7 '{g7?.name}' rect [{g7?.x0}, {g7?.z0}, {g7?.x1}, {g7?.z1}], lava zones {lava}, ice surfaces {ice}, campaign boxes {boxes}, waveNodes43 {(waveNodes ? "yes" : "no")}, waves {g7?.waves.Count}");
        if (level.StartsWith("n45-"))
        {
            bool ok = lava == 3 && ice == 0 && boxes == 3 && waveNodes && baseId == "n43-iceline";
            Console.WriteLine($"  the card's pieces (3 lava pits, no ice (cut), boiler + 2 gantries, wave 2's nodes, BaseId n43-iceline): {(ok ? "PASS" : "FAIL")}");
            return ok ? 0 : 1;
        }
        return 0;
    }

    // ---- jump ---------------------------------------------------------------------------------------------------------------------------------
    /// <summary>The scripted run over the grate: forward (-z) at x `x`; `dashAt` z for a ground dash (NaN none), the jump at `jumpAt`, an air dash
    /// `airDash` s after the jump (NaN none).</summary>
    sealed class A2MixJumpInput : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly double x, dashAt, jumpAt, airDash;
        public bool jumped, dashed, airDashed;
        public double takeoffSpeed, jumpT;
        int s;
        public A2MixJumpInput(SurvivalSim sim, double x, double dashAt, double jumpAt, double airDash) { this.sim = sim; this.x = x; this.dashAt = dashAt; this.jumpAt = jumpAt; this.airDash = airDash; }
        public double Yaw => 0;
        public double Pitch => 0;
        public void SetLook(double y, double p) { }
        public bool Captured => true;
        public InputFrame SampleMove(bool active)
        {
            s++;
            Vec3 p = sim.mover.position;
            var f = new InputFrame { moveZ = 1, moveX = Math.Max(-1, Math.Min(1, (x - p.x) * 2)), yaw = 0 };
            if (!dashed && !double.IsNaN(dashAt) && p.z <= dashAt && sim.mover.grounded) { dashed = true; f.dash = new Button { pressed = true, held = true }; }
            if (!jumped && p.z <= jumpAt && sim.mover.grounded) { jumped = true; jumpT = s; takeoffSpeed = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z); f.jump = new Button { pressed = true, held = true }; }
            else if (jumped && s - jumpT < 30) f.jump = new Button { held = true };
            if (jumped && !airDashed && !double.IsNaN(airDash) && (s - jumpT) * sim.step >= airDash) { airDashed = true; f.dash = new Button { pressed = true, held = true }; }
            return f;
        }
        public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
    }

    static int A2MixJump(string level)
    {
        LevelDef lv = LevelDef.From(PortData.Obj(PortData.LoadJson(A2MixPath(level))));
        if (!level.StartsWith("n45-")) { Console.WriteLine($"n45mix jump: {level} has no grate (n45 only)"); return 0; }
        const double laneX = -4.5, startZ = -186.5, lipZ = -195.3, farEdge = -202.5, nearEdge = -195.5;
        var ways = new (string name, double dashAt, double airDash, bool wantOver)[]
        {
            ("run + jump at the lip", double.NaN, double.NaN, false),
            ("a ground dash ended before the lip + jump (no ice: the speed is gone)", -188.3, double.NaN, false),
            ("run + jump + air dash", double.NaN, 0.15, true),
        };
        int fails = 0;
        foreach (var w in ways)
        {
            SurvivalSim sim = A2MixSim(level);
            int from = A2MixEnter(sim, lv, "g7", 1);
            sim.Respawn(new Vec3(laneX, 0, startZ), 0);
            var input = new A2MixJumpInput(sim, laneX, w.dashAt, lipZ, w.airDash);
            sim.UseInput(input);
            int burns = 0;
            sim.events.On<Lava30Event>("lavaBurn", e => burns++);
            bool left = false, landed = false;
            Vec3 land = default;
            for (int st = 0; st < MovementMath.StepsFor(4, sim.step) && !landed; st++)
            {
                sim.husks.KillAll(); // the shortcut alone: no enemy in the way
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (input.jumped && !sim.mover.grounded) left = true;
                if (left && (sim.mover.grounded || burns > 0)) { landed = true; land = sim.mover.position; }
            }
            bool over = landed && burns == 0 && land.z < farEdge;
            bool ok = over == w.wantOver;
            if (!ok) fails++;
            Console.WriteLine($"  jump {w.name}: take-off {F42(input.takeoffSpeed)} m/s, {(landed ? $"down at z {F42(land.z)} (y {F42(land.y)})" : "never down")}, lava burns {burns}: {(over ? "OVER the 7 m grate" : land.z <= nearEdge && landed ? "INTO the fire" : "short")} (want {(w.wantOver ? "over" : "not over")}): {(ok ? "PASS" : "FAIL")}");
        }
        return fails;
    }

    // ---- walk ---------------------------------------------------------------------------------------------------------------------------------
    /// <summary>NIGHT40 L2's iceline blast script (Walk40Iceline.cs, THE BLAST) for n43 / n45 levels: the level check asks N45Levels.BaseId and accepts
    /// the iceline chain (n45-iceline -> n43-iceline; n40-iceline). With 3+ swarmers / husks within 5 m, the {altFire} blast at the nearest (the
    /// gauntlet selected). Bot numbers only (Walk40Iceline's: BlastNearM 5, BlastCount 3, 0.5 s between tries).</summary>
    sealed class A2MixInput : IPlayerInput
    {
        public readonly Walk29Pilot pilot;
        readonly SurvivalSim sim;
        readonly bool iceline;
        int step = 0, lastBlast = -1000;
        bool blastNow;
        double yaw, pitch;
        bool mine;
        public int blasts;

        public A2MixInput(SurvivalSim sim, Walk29Pilot pilot, string levelId)
        {
            this.sim = sim;
            this.pilot = pilot;
            string b = N45Levels.BaseId(sim.cfg, levelId);
            iceline = b == "n43-iceline" || b == "n40-iceline";
            var json = PortData.Obj(PortData.LoadJson(A2MixPath(levelId)));
            if (json.ContainsKey("lava"))
                foreach (object o in PortData.Arr(json["lava"]))
                {
                    var r = PortData.Obj(o);
                    if (!(r.TryGetValue("room", out object rm) && rm as string == "g7")) continue;
                    List<object> rc = PortData.Arr(r["rect"]);
                    double x0 = PortData.Num(rc[0]), z0 = PortData.Num(rc[1]), x1 = PortData.Num(rc[2]), z1 = PortData.Num(rc[3]);
                    pits.Add(new Lava30Zone { x0 = Math.Min(x0, x1), x1 = Math.Max(x0, x1), z0 = Math.Min(z0, z1), z1 = Math.Max(z0, z1), y = PortData.Num(r["y"]), room = "g7" });
                }
            grate = pits.Count > 0;
        }

        public double Yaw => mine ? yaw : pilot.Yaw;
        public double Pitch => mine ? pitch : pilot.Pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; pilot.SetLook(y, p); }
        public bool Captured => pilot.Captured;

        public InputFrame SampleMove(bool active)
        {
            step++;
            InputFrame f = pilot.SampleMove(active);
            mine = blastNow = false;
            if (!active || !iceline || sim.campaign == null || sim.campaign.room < 0) return f;
            RoomDef rd = sim.campaignLevel.rooms[sim.campaign.room];
            if (rd.id != "g7") return f;
            Vec3 p = sim.mover.position;
            if (grate && A2MixGrateSteer(ref f, p)) { grateSteps++; mine = false; }
            if (step - lastBlast < MovementMath.StepsFor(0.5, sim.step)) return f;
            int n = 0;
            Husk first = null;
            double fd = double.PositiveInfinity;
            foreach (string k in new[] { "swarmer", "husk" })
                foreach (Husk h in sim.husks.PoolOf(k))
                {
                    if (!h.alive) continue;
                    double d = JsMath.Hypot(h.position.x - p.x, h.position.z - p.z);
                    if (d > 5) continue;
                    n++;
                    if (d < fd) { fd = d; first = h; }
                }
            if (n < 3 || first == null) return f;
            Vec3 eye = new Vec3(p.x, p.y + sim.cfg.movement.body.eyeHeight, p.z);
            Vec3 v = new Vec3(first.position.x, first.position.y + 0.8, first.position.z) - eye;
            yaw = JsMath.Atan2(-v.x, -v.z);
            pitch = JsMath.Atan2(v.y, JsMath.Hypot(v.x, v.z));
            mine = blastNow = true;
            lastBlast = step;
            blasts++;
            return new InputFrame { yaw = yaw, pitch = pitch };
        }

        // THE GRATE (n45's g7, like NIGHT40's CANAL script for g4): the pilot steers straight at its targets; walkers keep to the bridges, so does the bot.
        // When the pilot's wish would carry the feet into a lava pit within LookM (grounded), the move goes to the nearest bridge's end on this side
        // instead (or along the bridge when on it) and the dash / jump of that step are dropped; the look (the aim) stays the pilot's.
        // Bot numbers only: LookM 1.5 m ahead walking, JumpLookM 7 m when the pilot jumps or dashes (a jump carries 5.8 m, a dash 6 m); the bridges and pits are read from the level's lava zones (x outside every pit's x range = a bridge).
        readonly bool grate;
        readonly List<Lava30Zone> pits = new List<Lava30Zone>();
        public int grateSteps;
        const double LookM = 1.5, JumpLookM = 7;

        bool InPit(double x, double z)
        {
            foreach (Lava30Zone q in pits) if (x >= q.x0 && x <= q.x1 && z >= q.z0 && z <= q.z1) return true;
            return false;
        }

        bool A2MixGrateSteer(ref InputFrame f, Vec3 p)
        {
            if (pits.Count == 0 || !sim.mover.grounded) return false;
            double fx = -Math.Sin(f.yaw), fz = -Math.Cos(f.yaw), rx = Math.Cos(f.yaw), rz = -Math.Sin(f.yaw);
            double wx = f.moveZ * fx + f.moveX * rx, wz = f.moveZ * fz + f.moveX * rz;
            double wl = Math.Sqrt(wx * wx + wz * wz);
            bool dashing = f.dash.pressed || f.dash.held || f.jump.pressed;
            if (wl < 1e-6 && !dashing) return false;
            if (wl < 1e-6) { wx = fx; wz = fz; wl = 1; }
            double reach = dashing ? JumpLookM : LookM;
            bool into = false;
            for (double d = 0.5; d <= reach; d += 0.5) if (InPit(p.x + wx / wl * d, p.z + wz / wl * d)) { into = true; break; }
            if (!into) return false;
            double gz0 = double.PositiveInfinity, gz1 = double.NegativeInfinity;
            foreach (Lava30Zone q in pits) { gz0 = Math.Min(gz0, q.z0); gz1 = Math.Max(gz1, q.z1); }
            // the bridges: the x gaps between the pits (the room's own walls bound the outer pits)
            var xs = new List<(double a, double b)>();
            foreach (Lava30Zone q in pits) xs.Add((q.x0, q.x1));
            xs.Sort((u, v) => u.a.CompareTo(v.a));
            double tx = double.NaN, best = double.PositiveInfinity;
            for (int i = 0; i + 1 < xs.Count; i++)
            {
                double bx = 0.5 * (xs[i].b + xs[i + 1].a);
                if (xs[i + 1].a - xs[i].b < 1) continue;
                double dd = Math.Abs(bx - p.x);
                if (dd < best) { best = dd; tx = bx; }
            }
            if (double.IsNaN(tx)) return false;
            bool nearSide = p.z > gz1 - 0.01, farSide = p.z < gz0 + 0.01;
            double tz = nearSide ? gz1 + 1.0 : farSide ? gz0 - 1.0 : (wz < 0 ? gz0 - 1.5 : gz1 + 1.5); // on a bridge: along it, the way the pilot wanted
            if ((nearSide || farSide) && Math.Abs(p.x - tx) < 0.6) tz = nearSide ? gz0 - 1.5 : gz1 + 1.5; // at the bridge's end: onto it
            double dx = tx - p.x, dz = tz - p.z, dl = Math.Sqrt(dx * dx + dz * dz);
            if (dl < 1e-6) return false;
            dx /= dl; dz /= dl;
            f.moveZ = dx * fx + dz * fz;
            f.moveX = dx * rx + dz * rz;
            f.dash = default;
            f.jump = default;
            return true;
        }

        public CombatInput SampleCombat(bool active)
        {
            CombatInput c = pilot.SampleCombat(active);
            if (!active || !blastNow) return c;
            int g = sim.loadout.owned.IndexOf("gauntlet");
            return new CombatInput { weapon = g >= 0 && sim.loadout.Weapon != "gauntlet" ? g : -1, altFire = new Button { held = true, pressed = true } };
        }
    }

    static int A2MixWalk(string level, int seeds, double capS)
    {
        LevelDef lv = LevelDef.From(PortData.Obj(PortData.LoadJson(A2MixPath(level))));
        int g7 = lv.rooms.FindIndex(r => r.id == "g7");
        int done = 0;
        var times = new List<double>();
        int deathsAll = 0, burnsAll = 0, grateAll = 0, fusionAll = 0, blastsAll = 0, returnsAll = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(1 + s);
            SurvivalSim sim = A2MixSim(level);
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef l = sim.campaignLevel;
                if (c == null || c.room < 0) return l.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return l.rooms[Math.Min(c.room + 1, l.rooms.Count - 1)].spawn;
                return l.rooms[c.room].kind == "link" ? l.rooms[c.room].exit : l.rooms[c.room].spawn;
            };
            var input = new A2MixInput(sim, new Walk29Pilot(sim, ap), level);
            sim.UseInput(input);
            int deaths = 0, burns = 0, grate = 0, fusions = 0, returns = 0;
            double clearT = double.NaN;
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room == g7) clearT = e.time; });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room == g7) deaths++; });
            sim.events.On<Lava30Event>("lavaBurn", e => burns++);
            sim.events.On<Lava30Event>("lavaEnemyDeath", e => grate++);
            sim.events.On<FallReturn43Event>("fallReturn43", e => returns++);
            sim.events.On<Fusion43Event>(N43Events.Fusion, e => fusions++);
            A2MixEnter(sim, lv, "g7", seed);
            int max = MovementMath.StepsFor(capS, sim.step);
            int st = 0;
            int traced = 0;
            Vec3 prevP = sim.mover.position;
            for (; st < max && double.IsNaN(clearT) && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Vec3 q = sim.mover.position;
                if (A2MixTrace && traced < 12 && q.y < -3 && prevP.y >= -3) { traced++; Console.WriteLine($"    [trace] t {F42(st * sim.step)}: below -3 m at ({F42(q.x)}, {F42(q.y)}, {F42(q.z)}) from ({F42(prevP.x)}, {F42(prevP.y)}, {F42(prevP.z)}), state {sim.mover.state}, burns {burns}"); }
                prevP = q;
            }
            bool ok = !double.IsNaN(clearT);
            if (ok) { done++; times.Add(st * sim.step); }
            deathsAll += deaths; burnsAll += burns; grateAll += grate; fusionAll += fusions; blastsAll += input.blasts; returnsAll += returns;
            Vec3 p = sim.mover.position;
            Console.WriteLine($"  walk {level} g7 seed {seed}: {(ok ? $"CLEARED in {F42(st * sim.step)} s" : $"STOPPED after {F42(st * sim.step)} s at ({F42(p.x)}, {F42(p.y)}, {F42(p.z)}), alive {sim.enemies.AliveCount}")}; deaths {deaths}, the player's lava burns {burns}, enemies the grate killed {grate}, FUSIONs {fusions}, blasts pressed {input.blasts}, falls returned to the lip {returns}, grate-steer steps {input.grateSteps}, hp {F42(sim.health.hp)}");
            ap.Detach();
        }
        times.Sort();
        string med = times.Count == 0 ? "n/a" : F42(times[times.Count / 2]);
        Console.WriteLine($"walk {level} g7 THE MIX SUMMARY: cleared {done}/{seeds}, median {med} s (par {lv.rooms[g7].par}), deaths {deathsAll}, lava burns {burnsAll}, grate kills {grateAll}, FUSIONs {fusionAll}, blasts {blastsAll}, lip returns {returnsAll}: {(done == seeds ? "PASS" : "FAIL")}");
        return done == seeds ? 0 : 1;
    }
}
