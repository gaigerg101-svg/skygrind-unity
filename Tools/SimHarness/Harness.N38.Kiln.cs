// NIGHT38 builder K3 (level 3, n38-kiln): the kiln builder's harness commands, declared and called by the lead's Program.cs (N38Kiln).
//   n38kiln probe   the height envelopes the kiln's boost gates sit between, measured on the real Mover (Core/Mover.cs, NIGHT34-36 flow / ride /
//                   grip rows as the game runs them) on a flat floor, from a run at walk speed. The THUNDERCLAP's boost is applied exactly as
//                   Core/Combat.cs FireShotgun does it (airborne: velocity -= aim x weapons.shotgun.jumpSpeed), at most once per
//                   weapons.shotgun.fireInterval. Rows: jump; jump + double jump; slam jump (+ double jump); dash jump (+ double jump); jump + one
//                   boost (at take-off + 0.05 s, at the apex), + double jump; jump + two boosts (+ double jump between). Each row: the feet's top
//                   height and the horizontal metres covered when the feet come back down through that top - 0.5 m (a ledge's reach).
//                   Builder E's `n38 gates` is the authority (every verb and combo in the full sim); this is the kiln's first design pass.
//   n38kiln walk [--seeds N] [--trace]   walk29's pilot (Bot/Walk38Kiln.cs on the gates and the pours) on n38-kiln: every playable room cleared, per seed.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N38Kiln(string cmd, string[] args, ref int code)
    {
        if (cmd != "n38kiln") return;
        string sub = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "probe";
        if (sub == "probe") { code = N38KilnProbe(args); return; }
        if (sub == "walk")
        {
            // walk29's pilot (Walk29Pilot + Bot/Walk38Kiln.cs) on Design/levels/n38-kiln.json, 10 seeds unless --seeds: every playable room cleared
            var rest = new List<string> { "walk29", "kiln" };
            for (int i = 2; i < args.Length; i++) rest.Add(args[i]);
            if (!rest.Contains("--seeds")) { rest.Add("--seeds"); rest.Add("10"); }
            string old = walkPrefix;
            walkPrefix = "n38";
            try { N29Walk("walk29", rest.ToArray(), ref code); }
            finally { walkPrefix = old; }
            return;
        }
        if (sub == "trace") { code = N38KilnTrace(args); return; }
        if (sub == "corner") { code = N38KilnCorner(args); return; }
        if (sub == "blastwall") { code = N38KilnBlastWall(args); return; }
        if (sub == "slidewall") { code = N38KilnSlideWall(args); return; }
        Console.WriteLine("n38kiln: unknown sub-command " + sub + " (probe | walk | trace)");
        code = 2;
    }

    static string F38K(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.00", CultureInfo.InvariantCulture);

    /// <summary>One scripted air time: the Mover runs north at walk speed, jumps at step 0 (or the script's own take-off), and the script
    /// presses jump / slide / dash / fires the boost at the steps it names. Returns (top of the feet, the flat metres from the take-off when the feet
    /// come down through top - 0.5, the air seconds).</summary>
    static (double top, double reach, double air) N38KilnRun(GameConfig cfg, double dt, Func<int, Mover, (InputFrame f, bool boost)> script, double aimPitchDeg, int takeoffs = 1)
    {
        TriangleWorld w = N35Floor();
        Mover m = N35Mover(cfg, w, true, null);
        m.Reset(0, 0, 60);
        for (int s = 0; s < 120; s++) m.Step(N35In(0, 1, false, false), dt); // a second of running north (yaw 0 looks toward -z): walk speed
        double sgJump = cfg.Num("weapons.shotgun.jumpSpeed");
        int interval = MovementMath.StepsFor(cfg.Num("weapons.shotgun.fireInterval"), dt);
        double p = aimPitchDeg * Math.PI / 180;
        Vec3 aim = new Vec3(0, JsMath.Sin(p), -JsMath.Cos(p)); // looking north (-z), pitched
        double z0 = m.position.z, top = 0;
        int lastShot = -100000, airSteps = 0;
        bool left = false, wasGrounded = true;
        int ups = 0;
        double reach = double.NaN;
        for (int s = 0; s < 1200; s++)
        {
            var (f, boost) = script(s, m);
            m.Step(f, dt);
            if (boost && !m.grounded && s - lastShot >= interval)
            {
                m.velocity = m.velocity - aim * sgJump;
                lastShot = s;
            }
            if (wasGrounded && !m.grounded && ++ups >= takeoffs && !left) { left = true; z0 = m.position.z; top = 0; airSteps = 0; } // measured from the last take-off
            wasGrounded = m.grounded;
            if (!left) continue;
            if (!m.grounded) airSteps++;
            if (m.position.y > top) top = m.position.y;
            if (left && double.IsNaN(reach) && m.velocity.y < 0 && m.position.y <= top - 0.5) reach = z0 - m.position.z;
            if (left && m.grounded && s > 10) break;
        }
        return (top, reach, airSteps * dt);
    }

    static int N38KilnProbe(string[] args)
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        int apexWait = -1;
        bool held = false;
        // jump held from step 0; a double jump pressed at the first falling step (the apex) when dj
        Func<bool, int, Func<int, Mover, (InputFrame, bool)>> jumpScript = (dj, boostAt) =>
        {
            bool djDone = false, prev = false;
            return (s, m) =>
            {
                bool press = s == 0;
                if (dj && !djDone && s > 5 && m.velocity.y <= 0) { press = true; djDone = true; }
                bool h = s < 200;
                var f = N35In(0, 1, h, prev && !press, 0);
                f.jump = new Button { held = h, pressed = press };
                prev = h;
                bool boost = boostAt >= 0 && s >= boostAt;
                return (f, boost);
            };
        };
        var rows = new List<(string name, (double top, double reach, double air) r)>();
        rows.Add(("jump", N38KilnRun(cfg, dt, jumpScript(false, -1), -90)));
        rows.Add(("jump + double jump", N38KilnRun(cfg, dt, jumpScript(true, -1), -90)));
        // slam jump: jump, slam (slide pressed in the air) 0.15 s later, jump pressed on the landing step (the boost-jump window), then the double jump at its apex
        Func<bool, int, Func<int, Mover, (InputFrame, bool)>> slamScript = (dj, boostAfter) =>
        {
            int phase = 0, landed = -1;
            bool djDone = false;
            return (s, m) =>
            {
                var f = N35In(0, 1, false, false, 0);
                if (s == 0) f.jump = new Button { held = true, pressed = true };
                else if (phase == 0 && s == 18) { f.slide = new Button { held = true, pressed = true }; phase = 1; }
                else if (phase == 1) { f.slide = new Button { held = true }; if (m.grounded) { phase = 2; landed = s; } }
                if (phase == 2 && s == landed) { f.jump = new Button { held = true, pressed = true }; f.slide = default; phase = 3; }
                else if (phase == 3)
                {
                    f.jump = new Button { held = true };
                    if (dj && !djDone && m.velocity.y <= 0 && !m.grounded) { f.jump = new Button { held = true, pressed = true }; djDone = true; }
                }
                bool boost = boostAfter >= 0 && phase == 3 && s >= landed + boostAfter;
                return (f, boost);
            };
        };
        rows.Add(("slam jump", N38KilnRun(cfg, dt, slamScript(false, -1), -90, 2)));
        rows.Add(("slam jump + double jump", N38KilnRun(cfg, dt, slamScript(true, -1), -90, 2)));
        // dash jump: dash at step 0 (on the ground), jump 6 steps later inside the dash, the double jump at its apex
        Func<bool, int, Func<int, Mover, (InputFrame, bool)>> dashScript = (dj, boostAt) =>
        {
            bool djDone = false;
            return (s, m) =>
            {
                var f = N35In(0, 1, s >= 6, false, 0);
                if (s == 0) f.dash = new Button { held = true, pressed = true };
                if (s == 6) f.jump = new Button { held = true, pressed = true };
                if (dj && !djDone && s > 12 && m.velocity.y <= 0 && !m.grounded) { f.jump = new Button { held = true, pressed = true }; djDone = true; }
                return (f, boostAt >= 0 && s >= boostAt);
            };
        };
        rows.Add(("dash jump", N38KilnRun(cfg, dt, dashScript(false, -1), -90)));
        rows.Add(("dash jump + double jump", N38KilnRun(cfg, dt, dashScript(true, -1), -90)));
        int early = MovementMath.StepsFor(0.05, dt);
        foreach (double pitch in new[] { -90.0, -70.0 })
        {
            string a = pitch == -90 ? "aim straight down" : "aim -70 deg";
            rows.Add(($"jump + boost at take-off ({a})", N38KilnRun(cfg, dt, OneShot(jumpScript(false, early)), pitch)));
            rows.Add(($"jump + boost at take-off + double jump ({a})", N38KilnRun(cfg, dt, OneShot(jumpScript(true, early)), pitch)));
            rows.Add(($"jump + boost at the apex ({a})", N38KilnRun(cfg, dt, OneShot(ApexBoost(jumpScript(false, -1))), pitch)));
            rows.Add(($"jump + two boosts (+0.05 s, +0.9 s) + double jump between ({a})", N38KilnRun(cfg, dt, jumpScript(true, early), pitch)));
            rows.Add(($"slam jump + boost ({a})", N38KilnRun(cfg, dt, OneShot(slamScript(false, early)), pitch, 2)));
        }
        Console.WriteLine($"n38kiln probe (the real Mover, flat floor, a run at walk speed north; the boost = Combat.FireShotgun's -aim x {F38K(cfg.Num("weapons.shotgun.jumpSpeed"))} m/s, one per {F38K(cfg.Num("weapons.shotgun.fireInterval"))} s):");
        Console.WriteLine("  row | feet top (m) | reach at top - 0.5 m (flat m from take-off) | air (s)");
        foreach (var (name, r) in rows) Console.WriteLine($"  {name} | {F38K(r.top)} | {F38K(r.reach)} | {F38K(r.air)}");
        _ = apexWait; _ = held;
        return 0;
    }

    /// <summary>`n38kiln corner`: the WALL-JUMP cheese of a one-boost gate, on the real Mover (wall jumps work on noride walls: Mover.cs NoRide38 refuses
    /// only rides; builder E's checker drops every try that touches a noride solid). A box world with the gate's shape (gen-n38-kiln.cjs gate()):
    /// the take-off floor under the lintel (ceiling 4.0 m, closed above), the lip, a dry void (3.9 m deep, `void` m long), the ledge wall to wall,
    /// the side walls 16 m apart. From a sprint along the west wall (or the axis), a jump at the lip, then three jump presses at searched steps (each a
    /// wall jump when a wall is near, else the double jump), the stick north / into the corner / out of it. For ledge tops from 6.0 m down: the highest
    /// top any try LANDS on (grounded on it). The gate is safe when that is under the ledge's top / 1.1.</summary>
    static int N38KilnCorner(string[] args)
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        double voidM = 5;
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--void") voidM = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
        const double W = 8, lip = 0, ceil = 4.0, pit = 3.9;
        double far = lip - voidM; // north is -z
        double best = 0;
        string bestTry = "none";
        int tries = 0;
        for (double top = 6.0; top >= 2.5; top -= 0.25)
        {
            TriangleWorld w = new TriangleWorld();
            w.AddBox(new Vec3(-W, -1, lip), new Vec3(W, 0, lip + 30)); // the take-off floor (south of the lip)
            w.AddBox(new Vec3(-W, ceil, lip), new Vec3(W, 30, lip + 6)); // the lintel, closed above
            w.AddBox(new Vec3(-W, -pit - 1, far), new Vec3(W, -pit, lip)); // the void's floor
            w.AddBox(new Vec3(-W, -pit - 1, far - 4), new Vec3(W, top, far)); // the ledge, wall to wall
            w.AddBox(new Vec3(-W, -pit - 1, far - 30), new Vec3(W, 0, far - 4)); // the floor past it
            w.AddBox(new Vec3(-W - 1, -pit - 1, far - 30), new Vec3(-W, 30, lip + 30)); // the west wall (liner)
            w.AddBox(new Vec3(W, -pit - 1, far - 30), new Vec3(W + 1, 30, lip + 30)); // the east wall
            bool landed = false;
            foreach (double startX in new[] { -W + 0.45, -W + 1.5, 0.0 })
                foreach (int stick in new[] { 0, 1, 2 }) // 0 north, 1 north + into the west wall, 2 north + away from it
                    for (int k1 = 10; k1 <= 70 && !landed; k1 += 5)
                        for (int k2 = k1 + 6; k2 <= k1 + 60 && !landed; k2 += 6)
                            for (int k3 = k2 + 6; k3 <= k2 + 60 && !landed; k3 += 6)
                            {
                                tries++;
                                Mover m = N35Mover(cfg, w, true, null);
                                m.Reset(startX, 0, lip + 12);
                                bool prev = false;
                                int air = -1;
                                for (int s = 0; s < 900; s++)
                                {
                                    bool atLip = m.position.z <= lip + 0.1 && m.grounded && air < 0;
                                    bool press = atLip || (air >= 0 && (s - air == k1 || s - air == k2 || s - air == k3));
                                    if (atLip) air = s;
                                    bool held = press || (prev && air >= 0 && s - air < 4);
                                    var f = N35In(stick == 1 ? -0.7 : stick == 2 ? 0.7 : 0, 1, held, prev, 0);
                                    f.jump = new Button { held = held, pressed = press };
                                    prev = held;
                                    m.Step(f, dt);
                                    if (air >= 0 && m.grounded && m.position.y > top - 0.05 && m.position.z < far) { landed = true; break; }
                                    if (air >= 0 && m.grounded && s - air > 10) break;
                                }
                                if (landed) { best = top; bestTry = $"start x {F38K(startX)}, stick {(stick == 0 ? "north" : stick == 1 ? "into the wall" : "away")}, presses at +{k1} / +{k2} / +{k3} steps"; }
                            }
            if (landed) break;
        }
        Console.WriteLine($"n38kiln corner (void {F38K(voidM)} m, lintel {F38K(ceil)} m, walls 16 m apart, the real Mover's wall jumps on every wall): {tries} tries; the highest ledge top a try without the boost lands on: {F38K(best)} m ({bestTry})");
        return 0;
    }

    /// <summary>`n38kiln trace <roomId> [--seed S] [--seconds T]`: walk29's pilot on n38-kiln; from the step the player enters the room, every 6th step
    /// for T seconds: the room-relative position, the state, the weapon in hand, shots fired (the debugging view of one gate room).</summary>
    static int N38KilnTrace(string[] args)
    {
        string roomId = args.Length > 2 ? args[2] : "kb1b";
        uint seed = 1;
        double secs = 6;
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "--seed") seed = uint.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--seconds") secs = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
        }
        GameConfig cfg = Config();
        string levelId = "n38-kiln";
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--level") levelId = args[i + 1];
        string path = System.IO.Path.GetFullPath(System.IO.Path.Combine("Design", "levels", levelId + ".json"));
        SurvivalSim.Wyrm24Dir = MapGenRepo; // the worm's files (Design/bosses, Design/maps), as the campaign harness does
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        sim.UseInput(new Walk29Pilot(sim, ap));
        int shots = 0;
        sim.events.On<string>("fire", w => shots++);
        sim.StartRun(seed, null, null);
        int max = MovementMath.StepsFor(300, sim.step), from = -1, until = 0;
        for (int st = 0; st < max; st++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) continue;
            RoomDef rd = lv.rooms[c.room];
            if (from < 0 && rd.id == roomId) { from = st; until = st + MovementMath.StepsFor(secs, sim.step); Console.WriteLine($"entered {roomId} at {st * sim.step:0.00} s; owned {string.Join(",", sim.loadout.owned)}"); }
            if (from >= 0 && (st - from) % 6 == 0)
            {
                Vec3 p = sim.mover.position;
                double cx = (rd.x0 + rd.x1) / 2;
                // NIGHT39 K4: which arena the worm loaded (SurvivalSim.Wyrm24Ensure: design.n26.bosses.wyrm26.arena when Wyrm26On, else design.n24.wyrm.arena)
                string arenaKey = sim.wyrm24Is26 ? "design.n26.bosses.wyrm26.arena" : "design.n24.wyrm.arena";
                string arenaLine = sim.wyrm24 != null && sim.wyrm24Arena != null ? $" | arena {sim.wyrm39ArenaBuilt ?? cfg.Str(arenaKey)} ({sim.wyrm24Arena.boxes.Count} boxes; config {arenaKey}; is26 {sim.wyrm24Is26}, Wyrm26On {sim.Wyrm26On}, half {sim.wyrm24Arena.half:0.##}, arenaHalf {sim.wyrm24.arenaHalf:0.##})" : "";
                string boss = rd.kind == "boss" ? $" | wyrm {(sim.wyrm24 != null ? (sim.wyrm24.Alive ? "ALIVE" : "dead") : "none")} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}){arenaLine}" : "";
                Console.WriteLine($"  t {(st - from) * sim.step:0.00} lx {p.x - cx:0.00} lz {rd.z1 - p.z:0.00} y {p.y:0.00} vy {sim.mover.velocity.y:0.00} v {sim.mover.Speed:0.0} {sim.mover.state} hand {sim.loadout.Weapon} shots {shots} room {rd.id}{boss}");
            }
            if (from >= 0 && st >= until) break;
        }
        ap.Detach();
        return 0;
    }

    /// <summary>NIGHT39 K4: `n38kiln blastwall [plan.json]`: the one family builder E's checker does not fly against a CLIMB gate: the PULSE blast,
    /// then jump presses while the capsule is at the noride cliff face (each a WALL JUMP when the face is near: the Mover allows wall jumps off noride
    /// walls; the walk bot's trace showed a double pressed at the face is one), then a further press (the double, or a second wall jump). On builder
    /// E's own rig and runner (N38Run: the real SurvivalSim, the real Loadout's blast), the gate's own line, every gate of the plan: blast pitch
    /// 30-89 deg at 0.05 / 0.15 / 0.3 s, the first press 0.1-1.2 s after the blast, the second none / +0.2 / +0.4 / +0.6 s. Prints the tries that
    /// LAND on the cliff (a gate any of them lands is not a gate) and the highest feet over the take-off any try reached.</summary>
    static int N38KilnBlastWall(string[] args)
    {
        string path = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : "Design/levels/n38/kiln.plan.json";
        GameConfig cfg = Config();
        N38Plan plan = N38LoadPlan(path);
        N38Rig rig = N38NewRig(cfg, false);
        double r = cfg.movement.body.capsuleRadius;
        int bad = 0;
        foreach (N38Solid sol in plan.solids)
        {
            N38Gate g = sol.gate;
            if (g == null) continue;
            Vec3 flat = new Vec3(g.to.x - g.from.x, 0, g.to.z - g.from.z);
            Vec3 dir = flat / flat.Length;
            double top = sol.max.y, edge = N38Edge(g.from, dir, sol.min, sol.max);
            N38Arena full = N38BuildArena(plan, g.from - dir * g.runUp, g.to, 45, null);
            Vec3 O = g.from + N38Off, start = g.from - dir * g.runUp + new Vec3(0, 0.05, 0) + N38Off;
            int tries = 0, landed = 0, wj = 0;
            double best = double.NegativeInfinity;
            string bestName = "", landNames = "";
            foreach (double p in new[] { 30.0, 50, 60, 70, 80, 89 })
                foreach (double at in new[] { 0.05, 0.15, 0.3 })
                    for (double d1 = 0.1; d1 <= 1.2001; d1 += 0.1)
                        foreach (double d2 in new[] { 0.0, 0.2, 0.4, 0.6 })
                        {
                            var seq = new List<(string, double)> { ("jump", 0), ("pulse", at), ("double", at + d1) };
                            if (d2 > 0) seq.Add(("double", at + d1 + d2));
                            var t = N38Seq($"pulse p{p:0} @{at:0.0#} + press +{d1:0.0#}{(d2 > 0 ? $" + press +{d2:0.0#}" : "")}", "jump pulse double walljump", seq.ToArray());
                            t.weapon = "gauntlet";
                            t.pulsePitchDeg = p;
                            N38Trace tr = N38Run(rig, full, start, O, dir, t, top, edge, 5);
                            tries++;
                            if (tr.wallJumps > 0) wj++;
                            if (tr.apex > best) { best = tr.apex; bestName = t.name + (tr.wallJumps > 0 ? $" ({tr.wallJumps} wall jump(s))" : ""); }
                            bool crossed = tr.landed && !tr.dropped && tr.landY >= top - 0.1 && tr.landProg >= edge - r;
                            if (crossed) { landed++; if (landed <= 5) landNames += " | " + t.name; }
                        }
            Console.WriteLine($"  gate {g.id}: {tries} blast + face-press tries ({wj} made a wall jump): {landed} land on the {F38K(top - g.from.y)} m rise{landNames}; the highest feet {F38K(best)} m over the take-off ({bestName})");
            if (landed > 0) bad++;
        }
        Console.WriteLine($"n38kiln blastwall {plan.id}: {(bad == 0 ? "no try lands: GREEN" : bad + " gate(s) beaten: RED")}");
        return bad == 0 ? 0 : 1;
    }

    /// <summary>A scripted input: the frame a Func gives each step (no combat).</summary>
    sealed class K4Input : IPlayerInput
    {
        public Func<InputFrame> next;
        double yaw, pitch;
        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; }
        public bool Captured => true;
        public InputFrame SampleMove(bool active) { InputFrame f = next(); f.yaw = yaw; f.pitch = pitch; return f; }
        public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
    }

    /// <summary>NIGHT39 K4 round 2: `n38kiln slidewall [--level id]`: the verbs reel's row 13 (slide bounce, FeelReel.N35.Verbs.cs) on the harness: the
    /// wall the reel picks (Bot/VerbsWall39.Pick with floors, the reel's own code), the start 3.3 m off it, 9.9 m/s at 45 deg into it, the slide pressed on
    /// the first step and held, the jump on the first wall contact; the trace of the first 0.6 s (state, grounded, wall contact) and the row's verdict.</summary>
    static int N38KilnSlideWall(string[] args)
    {
        GameConfig cfg = Config();
        string levelId = "n38-kiln";
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--level") levelId = args[i + 1];
        string path = System.IO.Path.GetFullPath(System.IO.Path.Combine("Design", "levels", levelId + ".json"));
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var inp = new K4Input();
        bool slideHeld = true, pressed = false, slid = false;
        int t = 0;
        inp.next = () => new InputFrame { moveZ = 1, slide = new Button { pressed = t == 0, held = slideHeld && !pressed }, jump = new Button { pressed = false, held = false } };
        sim.UseInput(inp);
        sim.StartRun(1, null, null);
        for (int s = 0; s < 60; s++) sim.Step();
        if (!VerbsWall39.Pick(sim, true, false, out Vec3 st, out Vec3 al, out Vec3 nm, out double len, out string room)) { Console.WriteLine("n38kiln slidewall: no wall with floors"); return 1; }
        Vec3 inDir = new Vec3((al.x - nm.x) / Math.Sqrt(2), 0, (al.z - nm.z) / Math.Sqrt(2));
        var back = new Vec3(st.x + al.x * 2 + nm.x * 3.3, st.y, st.z + al.z * 2 + nm.z * 3.3);
        Console.WriteLine($"n38kiln slidewall {levelId}: the reel's wall in room {room}: start ({st.x:0.00}, {st.y:0.00}, {st.z:0.00}) along ({al.x:0.##}, {al.z:0.##}) normal ({nm.x:0.##}, {nm.z:0.##}) {len:0.0} m; the slide's start ({back.x:0.00}, {back.y:0.00}, {back.z:0.00})");
        sim.mover.Reset(back.x, back.y, back.z);
        inp.SetLook(Math.Atan2(-inDir.x, -inDir.z), 0);
        bool jumpNow = false;
        inp.next = () => new InputFrame { moveZ = 1, slide = new Button { pressed = t == 0, held = !pressed }, jump = new Button { pressed = jumpNow, held = jumpNow } };
        double before = 0, after = 0;
        int pressT = -1;
        for (t = 0; t < MovementMath.StepsFor(1.6, sim.step); t++)
        {
            if (t * sim.step < 0.05) sim.mover.velocity = new Vec3(inDir.x * 9.9, sim.mover.velocity.y, inDir.z * 9.9);
            jumpNow = !pressed && t * sim.step > 0.05 && sim.mover.touchingWall;
            if (jumpNow) { pressed = true; pressT = t; before = sim.mover.Speed; }
            sim.Step();
            if (sim.mover.state == MoveState.Slide) slid = true;
            if (pressT >= 0 && t - pressT > 0 && (t - pressT) * sim.step < 0.15) after = Math.Max(after, sim.mover.Speed);
            if (t < 72 && t % 6 == 0) Console.WriteLine($"  t {t * sim.step:0.00} ({sim.mover.position.x:0.00}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.00}) {sim.mover.state} grounded {sim.mover.grounded} wall {sim.mover.touchingWall} v {sim.mover.Speed:0.0}");
        }
        Console.WriteLine($"  row 13: slid {slid}, wall contact {pressed}, speed {before:0.00} -> {after:0.00}: {(slid && pressed && after >= 0.9 * before && before > 0 ? "PASS" : "RED")}");
        return 0;
    }

    /// <summary>A script whose boost fires once (the first step it asks).</summary>
    static Func<int, Mover, (InputFrame, bool)> OneShot(Func<int, Mover, (InputFrame, bool)> inner)
    {
        bool fired = false;
        return (s, m) =>
        {
            var (f, b) = inner(s, m);
            if (b && !fired && !m.grounded) { fired = true; return (f, true); }
            return (f, false);
        };
    }

    /// <summary>A jump script with the boost at the first falling step (the apex).</summary>
    static Func<int, Mover, (InputFrame, bool)> ApexBoost(Func<int, Mover, (InputFrame, bool)> inner) =>
        (s, m) => { var (f, _) = inner(s, m); return (f, s > 5 && m.velocity.y <= 0 && !m.grounded); };
}
