// NIGHT30 (builder S): the systems rows' headless judges. ONE entry method, N30Systems(cmd, args, ref code) (declared and called in Program.cs).
//   arcline30 [--off] [--level path]   the SPIKE + contract span: THE ARCLINE in a CAMPAIGN level. Default level: Design/levels/n30-span.json when it
//                                      exists (its own grappleNodes), else Design/levels/n29-foundry.json with one node added in memory above room 1.
//                                      Checks: owned from the level's start, a press at the node attaches on the NODE, the swing's cooldown after a
//                                      let-go = arcline.cooldown (0.4 s), the yank's = design.n21.arcline.yankCooldown (15 s), and a taped campaign run
//                                      with grapple presses replays EQUAL from the tape alone. `--off`: design.n30.span off: not owned (RED).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N30Systems(string cmd, string[] args, ref int code)
    {
        if (cmd == "arcline30") Arcline30Command(args, ref code);
        else if (cmd == "lesson30") Lesson30Command(args, ref code);
        else if (cmd == "rules30") Rules30Command(args, ref code);
        else if (cmd == "lava30") Lava30Command(args, ref code);
        else if (cmd == "glide30") Glide30Command(args, ref code);
        else if (cmd == "bossline30") BossLine30Command(args, ref code);
        else if (cmd == "spanpilot30") SpanPilot30Command(args, ref code);
        else if (cmd == "lavawalk30") LavaWalk30Command(args, ref code);
        else if (cmd == "tape30") Tape30Command(args, ref code);
    }

    // ---- tape30 ----------------------------------------------------------------------------------------------------------------------------

    /// <summary>`tape30`: the tape-exact rule for tonight's two new floor rules. LAVA: a CAMPAIGN run on n30-foundry with the config's rows (lava on), the player walks
    /// straight on from f1 into f2's lava trench (scripted input only: no harness respawn), burns, is knocked, burns again; taped and replayed from the tape alone.
    /// GLIDE: a SURVIVAL run on the survival map with an ICE surface over the spawn's open run (the map JSON's top-level `surfaces`, design.n26.surfaces), glide on:
    /// run, dash, glide, steer; taped and replayed. Both EQUAL with their rule's events seen.</summary>
    static void Tape30Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        bool ok = true;
        // LAVA
        {
            SurvivalMap map = CampaignMapFrom("Design/levels/n30-foundry.json", cfg, out var json);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            N30Blocks b = N30Blocks.From(cfg);
            b.lava = true;
            sim.n30Override = b;
            var si = new ScriptedInput();
            sim.UseInput(si);
            int burns = 0;
            sim.events.On<Lava30Event>("lavaBurn", e => burns++);
            RunTape tape = TapeScript.Record(sim, 3080, 2400, "tape30-lava", (sm, i) =>
            {
                si.SetLook(0, 0);
                si.Set(new InputFrame { moveZ = 1 }, new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = i % 90 < 6, pressed = i % 90 == 0 } });
            }, TapeScript.God, false);
            TapeResult r = TapeReplay.Replay(Config(), map, tape);
            bool pass = r.Equal && burns > 0 && (tape.n30 & 4) != 0;
            ok &= pass;
            Console.WriteLine($"tape30 lava: n30-foundry campaign, {tape.steps} steps, n30 bits {tape.n30} ({N30Blocks.FromBits(tape.n30)}), lava burns {burns}, end ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.0}) room {(sim.campaign != null && sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "-")}; replay {r.Line} -> {(pass ? "PASS" : "FAIL")}");
        }
        // GLIDE
        {
            var json = PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json")));
            SurvivalMap probeMap = SurvivalMap.FromExport(json);
            Vec3 p0 = probeMap.playerSpawn;
            json["surfaces"] = new List<object> { new Dictionary<string, object> { ["kind"] = "ice", ["rect"] = new List<object> { p0.x - 30, p0.z - 30, p0.x + 30, p0.z + 30 }, ["minY"] = p0.y - 0.5, ["maxY"] = p0.y + 0.5 } };
            SurvivalMap map = SurvivalMap.FromExport(json);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.n26Override = new N26Blocks { surfaces = true };
            sim.n30Override = new N30Blocks { glide = true };
            var si = new ScriptedInput();
            sim.UseInput(si);
            int dashAddsBefore = 0;
            RunTape tape = TapeScript.Record(sim, 3081, 2400, "tape30-glide", (sm, i) =>
            {
                double yaw = (i / 240) * 0.9; // a new heading every 2 s (steering on the ice)
                si.SetLook(yaw, 0);
                si.Set(new InputFrame { moveZ = i % 240 < 90 ? 1 : 0, moveX = i % 240 >= 150 && i % 240 < 200 ? 1 : 0, dash = new Skygrind.Core.Button { held = i % 240 == 60, pressed = i % 240 == 60 } }, new CombatInput { weapon = -1 });
            }, TapeScript.God, false);
            TapeResult r = TapeReplay.Replay(Config(), map, tape);
            int adds = sim.glide30 != null ? sim.glide30.dashAdds - dashAddsBefore : 0;
            int glided = sim.glide30 != null ? sim.glide30.glideSteps : 0;
            bool pass = r.Equal && adds > 0 && glided > 100 && (tape.n30 & 2) != 0;
            ok &= pass;
            Console.WriteLine($"tape30 glide: survival map + an ice sheet 60 x 60 m over the spawn, {tape.steps} steps, n30 bits {tape.n30}, n26 bits {tape.n26}, glide dash adds {adds}, glided steps {glided}; replay {r.Line} -> {(pass ? "PASS" : "FAIL")}");
        }
        Console.WriteLine(ok ? "tape30 PASS: lava and glide runs replay EQUAL from their tapes" : "tape30 FAIL");
        code = ok ? 0 : 1;
    }

    /// <summary>`lavawalk30 [--level path] [--cap s]`: a diagnostic: the level pilot on a lava level (design config), every lavaBurn / lava hurt / room retry printed
    /// with the step's position, hp and flow (why a walk stops in a lava room).</summary>
    static void LavaWalk30Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", "Design/levels/n30-foundry.json");
        double cap = double.Parse(Opt30(args, "--cap", "240"), CultureInfo.InvariantCulture);
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var ap = new AutopilotInput(sim, "normal", 1);
        ap.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        sim.UseInput(new Walk29Pilot(sim, ap));
        int printed = 0, burns = 0, hurts = 0;
        string Where() => $"t {sim.SimTime:0.0} room {(sim.campaign != null && sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "-")} pos ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.0}) hp {sim.health.hp:0}/{sim.health.max:0} flow {sim.flow} inv {sim.mover.invulnerable}";
        sim.events.On<Lava30Event>("lavaBurn", e => { burns++; if (printed++ < 12) Console.WriteLine($"  lavaBurn {Where()}"); });
        sim.events.On<HurtEvent>("hurt", e => { if (e.source == "lava") hurts++; });
        sim.events.On<RoomEvent>("roomRetry", e => { if (printed++ < 30) Console.WriteLine($"  roomRetry {e.id} {Where()}"); });
        sim.StartRun(1, null, null);
        Console.WriteLine($"lavawalk30: n30 {sim.n30}; lava zones {(sim.lava30 != null ? sim.lava30.zones.Count : -1)}");
        int max = MovementMath.StepsFor(cap, sim.step);
        for (int st = 0; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            if (st % (120 * 20) == 0) Console.WriteLine($"  every 20 s: {Where()} lava touchAt {(sim.lava30 != null ? sim.lava30.TouchAt(sim.mover.position) : -9)} grace {(sim.lava30 != null ? sim.lava30.graceSteps : -1)}");
        }
        Console.WriteLine($"lavawalk30: burns {burns}, lava hurts landed {hurts}; end {Where()}");
        ap.Detach();
        code = 0;
    }

    // ---- spanpilot30 -----------------------------------------------------------------------------------------------------------------------

    /// <summary>`spanpilot30 [--off] [--level path] [--seeds n]`: the level pilot (Walk29Pilot + Span30Pilot) crosses a gap by the grapple. Default: n30-span when it
    /// exists (its own nodes; the walk to the end of its playable rooms), else n30-foundry with one node added in memory 4.5 m over f2's lava trench (the pilot must
    /// swing it: the f2 jump script never runs while the grapple script drives). Design config with design.n30.span forced on (off with --off). PASS: f2 (or every
    /// room) cleared with a node attach and 0 lava burns on the crossing.</summary>
    static void SpanPilot30Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", File.Exists("Design/levels/n30-span.json") ? "Design/levels/n30-span.json" : "Design/levels/n30-foundry.json");
        int seeds = int.Parse(Opt30(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        int passes = 0;
        for (int s = 0; s < seeds; s++)
        {
            SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
            LevelDef level = LevelDef.From(json);
            bool added = false;
            string targetRoom = null;
            if (map.grappleNodes.Count == 0 && json.ContainsKey("lava"))
            {
                var lz = PortData.Obj(PortData.Arr(json["lava"])[0]);
                var r = PortData.Arr(lz["rect"]);
                double x0 = PortData.Num(r[0]), z0 = PortData.Num(r[1]), x1 = PortData.Num(r[2]), z1 = PortData.Num(r[3]);
                targetRoom = (string)lz["room"];
                map.grappleNodes.Add(new Vec3((x0 + x1) / 2, 4.5, (z0 + z1) / 2));
                added = true;
            }
            int last = level.rooms.FindAll(rr => rr.kind != "boss").Count - 1;
            int targetIdx = targetRoom != null ? level.rooms.FindIndex(rr => rr.id == targetRoom) : last;
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            N30Blocks b = N30Blocks.From(cfg);
            b.span = !off;
            sim.n30Override = b;
            var ap = new AutopilotInput(sim, "normal", (uint)(s + 1));
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var pilot = new Walk29Pilot(sim, ap);
            sim.UseInput(pilot);
            // the rooms to prove: every CROSSING room (nodes in a line; a ring is the autopilot's); the run goes until the player is past the last of them
            var crossing = level.rooms.FindAll(rr => rr.kind != "boss" && Span30Pilot.IsCrossing(map, rr)).ConvertAll(rr => rr.id);
            if (crossing.Count > 0) targetIdx = level.rooms.FindIndex(rr => rr.id == crossing[crossing.Count - 1]);
            var burnsIn = new Dictionary<string, int>();
            int burns = 0, burnsTarget = 0;
            sim.events.On<Lava30Event>("lavaBurn", e =>
            {
                burns++;
                if (sim.campaign == null || sim.campaign.room < 0) return;
                string id = sim.campaignLevel.rooms[sim.campaign.room].id;
                burnsIn.TryGetValue(id, out int nb);
                burnsIn[id] = nb + 1;
                if (crossing.Contains(id)) burnsTarget++;
            });
            bool trace = Flag(args, "--trace");
            if (trace)
            {
                sim.events.On<RoomEvent>("roomEnter", e => Console.WriteLine($"    t {sim.SimTime:0.0} enter {e.id}"));
                sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"    t {sim.SimTime:0.0} clear {e.id}"));
                sim.events.On<Arcline21RopeEvent>("ropeAttach", e => Console.WriteLine($"    t {sim.SimTime:0.0} attach node {e.node} at ({e.x:0.0}, {e.y:0.0}, {e.z:0.0}) from ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0})"));
                sim.events.On<Arcline21RopeEvent>("ropeRelease", e => Console.WriteLine($"    t {sim.SimTime:0.0} release '{e.reason}' at ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0}) speed {e.speed:0.0}"));
                sim.events.On<Lava30Event>("lavaBurn", e => Console.WriteLine($"    t {sim.SimTime:0.0} lavaBurn at ({e.x:0.0}, {e.y:0.0}, {e.z:0.0})"));
            }
            sim.StartRun((uint)(s + 1), null, null);
            int max = MovementMath.StepsFor(240, sim.step), st = 0;
            bool reached = false;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (trace && st % 600 == 0) Console.WriteLine($"    t {sim.SimTime:0.0} pos ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0}) state {sim.mover.state} rope {sim.mover.rope.phase}");
                if (sim.campaign != null && (sim.campaign.room > targetIdx || sim.campaign.clearSteps.Count > last)) { reached = true; break; }
            }
            ap.Detach();
            Span30Pilot sp = pilot.span30;
            bool allCrossed = crossing.TrueForAll(id => sp.crossedRooms.Contains(id));
            bool pass = reached && sp.attaches > 0 && allCrossed && burnsTarget == 0;
            if (pass) passes++;
            string burnRooms = burnsIn.Count == 0 ? "none" : string.Join(", ", System.Linq.Enumerable.Select(burnsIn, kv => kv.Key + " x" + kv.Value));
            Console.WriteLine($"spanpilot30 seed {s + 1} {(off ? "OFF (span off)" : "ON (span)")}: level {level.id}{(added ? $", one node over '{targetRoom}' lava in memory" : $", {map.grappleNodes.Count} plan nodes")}; crossing rooms [{string.Join(", ", crossing)}], crossed by the grapple [{string.Join(", ", sp.crossedRooms)}]; past {level.rooms[targetIdx].id} {reached} in {(st * sim.step).ToString("0.0", ci)} s; ARCLINE owned {sim.arcline21 != null && sim.arcline21.Owned}; node attaches {sp.attaches}, given up {sp.gaveUp}; lava burns {burns} ({burnsTarget} in crossing rooms; by room: {burnRooms})");
        }
        if (off)
        {
            Console.WriteLine(passes == 0 ? "spanpilot30 RED (as expected: span off, no grapple crossing)" : "spanpilot30 NOT RED");
            code = passes == 0 ? 1 : 2;
            return;
        }
        Console.WriteLine(passes == seeds ? $"spanpilot30 GREEN: {passes}/{seeds} crossings by the grapple" : $"spanpilot30 FAIL: {passes}/{seeds}");
        code = passes == seeds ? 0 : 1;
    }

    // ---- bossline30 ------------------------------------------------------------------------------------------------------------------------

    /// <summary>`bossline30 [--off]`: THE LANTERN RING fought in three attempts of 50 s (BossRoomBegin again after each, as a death restarts it; the player immortal at
    /// the spawn): with design.n30.teach on the Lantern asks the ONE budget (SurvivalSim.BossLine30), so each line ("tell", "core") is said at most maxShows times over
    /// the three attempts and every said line reaches the lesson widget (lessonBegin id "boss") and goes (lessonClear); `--off` (teach off): the Lantern's own count
    /// restarts with each attempt (the round-1 reel's 6 / 6: RED).</summary>
    static void BossLine30Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, "..")); // the boss files' root (as boss30 sets it)
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n27Override = new N27Blocks { boss1 = true };
        sim.n29Override = new N29Blocks { boss1 = true };
        sim.n30Override = new N30Blocks { lantern30 = true, teach = !off };
        sim.n26Override = N26Blocks.Off;
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(3060, null, null);
        sim.husks.frozen = true;
        int tell = 0, core = 0, lessonBoss = 0, lessonClear = 0, attempts = 0;
        sim.events.On<BossLine30>("bossLine30", e => { if (e.key == "tell") tell++; else core++; });
        sim.events.On<LessonEvent>("lessonBegin", e => { if (e.id == "boss") lessonBoss++; });
        sim.events.On<LessonEvent>("lessonClear", e => { if (e.id == "boss") lessonClear++; });
        for (int a = 0; a < 3; a++)
        {
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yaw)) { Console.WriteLine("bossline30: BossRoomBegin(hulk) failed"); code = 2; return; }
            attempts++;
            sim.Respawn(spawn, yaw);
            for (int s = 0; s < 6000; s++)
            {
                sim.health.hp = sim.health.max;
                sim.mover.invulnerable = true;
                sim.Step();
                if (sim.flow == Skygrind.Game.Flow.Down) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; sim.health.Reset(); }
            }
        }
        int max = (int)cfg.Num("design.n30.teach.maxShows");
        Console.WriteLine($"bossline30 {(off ? "OFF (teach off)" : "ON (teach)")}: {attempts} attempts of 50 s: the Lantern said 'tell' {tell}, 'core' {core} (want at most {max} each over the attempts); lesson widget boss lines {lessonBoss}, cleared {lessonClear}");
        if (off)
        {
            bool red = tell > max || core > max;
            Console.WriteLine(red ? "bossline30 RED (as expected: off, each attempt says the lines again)" : "bossline30 NOT RED (the attempts said no more than the budget)");
            code = red ? 1 : 2;
            return;
        }
        bool ok = tell >= 1 && tell <= max && core <= max && lessonBoss == tell + core && lessonClear >= 1;
        Console.WriteLine(ok ? "bossline30 GREEN: one budget, every line through the lesson widget" : "bossline30 FAIL");
        code = ok ? 0 : 1;
    }

    // ---- glide30 ---------------------------------------------------------------------------------------------------------------------------

    /// <summary>A SURVIVAL composition where every floor is ice (glideIceAt30 = all; design.n26.surfaces' ice friction set on the Mover), glide on or off.</summary>
    static SurvivalSim GlideSim30(GameConfig cfg, SurvivalMap map, bool glide)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n30Override = glide ? new N30Blocks { glide = true } : N30Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3050, null, null);
        sim.mover.invulnerable = true;
        sim.glideIceAt30 = _ => true;
        return sim;
    }

    static double HSpeed(SurvivalSim s) => Math.Sqrt(s.mover.velocity.x * s.mover.velocity.x + s.mover.velocity.z * s.mover.velocity.z);

    /// <summary>One step with the ice friction on the Mover (Surfaces26 would set it from a zone; the harness has no zone), the look `yaw`, the move input.</summary>
    static void IceTick30(SurvivalSim sim, double iceMult, double yaw, InputFrame mv)
    {
        sim.mover.groundFrictionMult = sim.mover.grounded ? iceMult : 1;
        var inp = (ScriptedInput)sim.input;
        inp.SetLook(yaw, 0);
        mv.yaw = yaw;
        inp.Set(mv, new CombatInput { weapon = -1 });
        sim.health.hp = sim.health.max;
        sim.Step();
    }

    /// <summary>The player at `p`, moving `speed` m/s along yaw (yaw 0 looks toward -z), settled on the floor.</summary>
    static void Launch30(SurvivalSim sim, Vec3 p, double yaw, double speed, double iceMult)
    {
        sim.Respawn(p, yaw * 180 / Math.PI);
        sim.UseInput(sim.input as ScriptedInput ?? new ScriptedInput());
        for (int i = 0; i < 20; i++) IceTick30(sim, iceMult, yaw, default);
        sim.mover.velocity = new Vec3(-Math.Sin(yaw) * speed, sim.mover.velocity.y, -Math.Cos(yaw) * speed);
    }

    /// <summary>`glide30 --level path [--off]`: the Player reel's glide30 on a real level, headless: a CAMPAIGN run, DOORFREE, the longest ICE surface, a respawn 1 m
    /// inside its +z end, a 0.8 s run-up toward -z, ONE dash, the speed every 0.1 s printed with the position, state and the ice test, until 2.6 s.</summary>
    static void Glide30Level(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", "Design/levels/n30-iceline.json");
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        SurvivalSim sim = CampaignSim30(cfg, map, off ? N30Blocks.Off : N30Blocks.From(cfg) is N30Blocks b0 ? WithGlide(b0) : default);
        sim.n29Override = new N29Blocks { doorFree = true, lockout = true, tank = true, pickups = true, roomRules = true };
        var si = new ScriptedInput();
        sim.UseInput(si);
        sim.StartRun(3070, null, null);
        for (int s = 0; s < 360; s++) { sim.doorFreeHeld = true; sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); }
        sim.doorFreeHeld = false;
        SurfaceZone ice = null;
        double bestLen = 0;
        if (sim.surfaces26 != null)
            foreach (SurfaceZone zz in sim.surfaces26.zones)
                if (zz.kind == SurfaceKind.Ice && Math.Max(zz.x1 - zz.x0, zz.z1 - zz.z0) > bestLen) { bestLen = Math.Max(zz.x1 - zz.x0, zz.z1 - zz.z0); ice = zz; }
        if (ice == null) { Console.WriteLine("glide30 level: no ICE surface (n26.surfaces off?)"); code = 2; return; }
        // the clearest line down the sheet (the n30-r2 reel ran its centre line into i6's 1.2 m pillar at z -164.4): the x whose rays at knee and chest height run
        // furthest toward -z from the sheet's +z end
        double cx = (ice.x0 + ice.x1) / 2, bestRun = -1;
        for (double x = ice.x0 + 0.8; x <= ice.x1 - 0.8 + 1e-9; x += 0.25)
        {
            double run = 60;
            foreach (double hgt in new[] { 0.4, 1.4 })
                if (sim.world.RayIntersect(new Vec3(x, Math.Max(0, ice.minY + 0.5) + hgt, ice.z1 - 1), new Vec3(0, 0, -1), out RayHit rh)) run = Math.Min(run, rh.distance);
            if (run > bestRun + 1e-9) { bestRun = run; cx = x; }
        }
        Console.WriteLine($"  the clearest line: x {cx.ToString("0.00", ci)}, {bestRun.ToString("0.0", ci)} m clear");
        var start = new Vec3(cx, Math.Max(0, ice.minY + 0.5), ice.z1 - 1);
        sim.Respawn(start, 0);
        double before = 0, after = -1;
        bool dashed = false, clearEnemies = !Flag(args, "--keep-enemies");
        for (int i = 0; i < 312; i++)
        {
            double t = i * sim.step;
            InputFrame f = t < 0.8 ? new InputFrame { moveZ = 1 } : !dashed ? new InputFrame { moveZ = 1, dash = new Skygrind.Core.Button { held = true, pressed = true } } : default;
            if (t >= 0.8 && !dashed) { before = HSpeed(sim); dashed = true; }
            si.SetLook(0, 0);
            si.Set(f, new CombatInput { weapon = -1 });
            sim.health.hp = sim.health.max;
            sim.mover.invulnerable = true;
            if (clearEnemies) sim.husks.KillAll(); // the room's wave (DOORFREE spawned it) stands in the sheet's middle: the n30-r2 reel's dash ended against a tank
            sim.Step();
            if (dashed && sim.mover.state != MoveState.Dash && t > 1.4 && after < 0) after = HSpeed(sim);
            if (i % 12 == 0 || (dashed && i % 4 == 0 && t < 1.6)) Console.WriteLine($"  t {t.ToString("0.00", ci)} pos ({sim.mover.position.x.ToString("0.0", ci)}, {sim.mover.position.y.ToString("0.00", ci)}, {sim.mover.position.z.ToString("0.0", ci)}) speed {HSpeed(sim).ToString("0.0", ci)} state {sim.mover.state} grounded {sim.mover.grounded} onIce {sim.OnIce30(sim.mover.position)} fric x{sim.mover.groundFrictionMult.ToString("0.00", ci)} room {(sim.campaign != null && sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "-")} enemies {sim.enemies.AliveCount}");
        }
        double gain = before > 0 && after >= 0 ? after / before - 1 : 0;
        Console.WriteLine($"glide30 level {(off ? "OFF" : "ON")}: {path} the {bestLen.ToString("0", ci)} m ice sheet x {ice.x0}..{ice.x1} z {ice.z0}..{ice.z1}: before the dash {before.ToString("0.0", ci)} m/s, 0.6 s after {after.ToString("0.0", ci)} m/s ({(100 * gain).ToString("+0;-0", ci)} %), glide dash adds {(sim.glide30 != null ? sim.glide30.dashAdds : 0)}");
        code = !off && gain >= 0.2 ? 0 : 1;
    }

    static N29Blocks WithDoorFree(N29Blocks b)
    {
        b.doorFree = true;
        return b;
    }

    static N30Blocks WithGlide(N30Blocks b)
    {
        b.glide = true;
        return b;
    }

    static void Glide30Command(string[] args, ref int code)
    {
        if (Opt30(args, "--level", null) != null) { Glide30Level(args, ref code); return; }
        bool off = Flag(args, "--off");
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double ice = cfg.Num("design.n26.surfaces.iceFrictionMult"), hz = cfg.movement.loop.simHz;
        var lines = new List<string>();
        double[] dashGain = new double[2], decay1 = new double[2], decay2 = new double[2], capMax = new double[2], turn = new double[2], slopeEnd = new double[2], slopeStart = new double[2];
        int[] adds = new int[2];
        for (int k = 0; k < 2; k++)
        {
            bool on = k == 1;
            if (off && on) break;
            // A. decay: 16 m/s, no input, 1 s and 2 s
            SurvivalSim s = GlideSim30(cfg, map, on);
            if (!(s.input is ScriptedInput)) s.UseInput(new ScriptedInput());
            Vec3 p0 = map.playerSpawn;
            // the longest open run through the spawn (64 directions, rays at knee and chest height both ways): start at one end
            double bestLen = 0, yaw = 0, ux = 0, uz = -1;
            Vec3 back = p0;
            for (int d = 0; d < 64; d++)
            {
                double a = d * Math.PI / 32, dx0 = -Math.Sin(a), dz0 = -Math.Cos(a);
                double fwd = 60, bwd = 60;
                foreach (double hgt in new[] { 0.4, 1.4 })
                {
                    var o = new Vec3(p0.x, p0.y + hgt, p0.z);
                    if (s.world.RayIntersect(o, new Vec3(dx0, 0, dz0), out RayHit hr1)) fwd = Math.Min(fwd, hr1.distance);
                    if (s.world.RayIntersect(o, new Vec3(-dx0, 0, -dz0), out RayHit hr2)) bwd = Math.Min(bwd, hr2.distance);
                }
                if (fwd + bwd > bestLen) { bestLen = fwd + bwd; yaw = a; ux = dx0; uz = dz0; double b0 = Math.Max(0, bwd - 1.5); back = new Vec3(p0.x - dx0 * b0, p0.y, p0.z - dz0 * b0); }
            }
            if (k == 0) Console.WriteLine($"glide30: the open run through the spawn {bestLen.ToString("0.0", ci)} m (yaw {(yaw * 180 / Math.PI).ToString("0", ci)} deg)");
            Launch30(s, back, yaw, 16, ice);
            for (int i = 0; i < 120; i++) IceTick30(s, ice, yaw, default);
            decay1[k] = HSpeed(s);
            for (int i = 0; i < 120; i++) IceTick30(s, ice, yaw, default);
            decay2[k] = HSpeed(s);
            // B. one dash at 12 m/s: the speed 0.5 s after the dash ends vs before it
            s = GlideSim30(cfg, map, on);
            if (!(s.input is ScriptedInput)) s.UseInput(new ScriptedInput());
            Launch30(s, back, yaw, 12, ice);
            IceTick30(s, ice, yaw, default);
            double before = HSpeed(s);
            IceTick30(s, ice, yaw, new InputFrame { moveZ = 1, dash = new Btn30 { held = true, pressed = true }.B });
            int guard = 0;
            while (s.mover.state == MoveState.Dash && guard++ < 120) IceTick30(s, ice, yaw, default);
            for (int i = 0; i < 60; i++) IceTick30(s, ice, yaw, default);
            dashGain[k] = HSpeed(s) / Math.Max(1e-6, before) - 1;
            adds[k] = s.glide30 != null ? s.glide30.dashAdds : 0;
            // C. the cap: dash whenever a pip is up for 6 s from 12 m/s (circling the open floor: the look turns 1 rad/s), the top speed
            s = GlideSim30(cfg, map, on);
            if (!(s.input is ScriptedInput)) s.UseInput(new ScriptedInput());
            Launch30(s, p0, yaw, 12, ice);
            double top = 0, y = yaw;
            for (int i = 0; i < 720; i++)
            {
                bool dash = i % 60 == 0;
                IceTick30(s, ice, y, new InputFrame { moveZ = 1, dash = new Btn30 { held = dash, pressed = dash }.B });
                if (s.mover.state != MoveState.Dash) top = Math.Max(top, HSpeed(s));
                if (s.mover.position.y < p0.y - 3) break;
            }
            capMax[k] = top;
            // D. the turn rate at 14 m/s: strafe input held (moveX 1), the heading change over 0.5 s
            s = GlideSim30(cfg, map, on);
            if (!(s.input is ScriptedInput)) s.UseInput(new ScriptedInput());
            Launch30(s, back, yaw, 14, ice);
            double h0 = Math.Atan2(s.mover.velocity.x, s.mover.velocity.z);
            for (int i = 0; i < 60; i++) IceTick30(s, ice, yaw, new InputFrame { moveX = 1 });
            double h1 = Math.Atan2(s.mover.velocity.x, s.mover.velocity.z), dh = h1 - h0;
            while (dh > Math.PI) dh -= 2 * Math.PI;
            while (dh < -Math.PI) dh += 2 * Math.PI;
            turn[k] = Math.Abs(dh) / 0.5 * 180 / Math.PI;
            // E. the slope: the first wedge (8 m, rise 2.9 m), from its high end at 2 m/s downhill, no input: the speed 1 m past the low end
            s = GlideSim30(cfg, map, on);
            if (!(s.input is ScriptedInput)) s.UseInput(new ScriptedInput());
            WedgeSpec w = map.wedges[0];
            double wx = -Math.Sin(w.yaw), wz = -Math.Cos(w.yaw); // ArenaMap.WedgeCorners: the low edge at (x, z), rising along (-sin yaw, -cos yaw) to `rise` at `length`
            Vec3 endA = new Vec3(w.x + wx * (w.length - 0.6), map.playerSpawn.y + w.rise + 0.5, w.z + wz * (w.length - 0.6));
            Vec3 endB = new Vec3(w.x - wx * 0.8, map.playerSpawn.y + 0.5, w.z - wz * 0.8);
            s.Respawn(endA, 0);
            for (int i = 0; i < 30; i++) IceTick30(s, ice, 0, default);
            double yA = s.mover.position.y;
            s.Respawn(endB, 0);
            for (int i = 0; i < 30; i++) IceTick30(s, ice, 0, default);
            double yB = s.mover.position.y;
            Vec3 high = yA > yB ? endA : endB, low = yA > yB ? endB : endA;
            double dx = low.x - high.x, dz = low.z - high.z, dl = Math.Sqrt(dx * dx + dz * dz);
            double syaw = Math.Atan2(-dx, -dz);
            s.Respawn(high, syaw * 180 / Math.PI);
            for (int i = 0; i < 20; i++) IceTick30(s, ice, syaw, default);
            s.mover.velocity = new Vec3(dx / dl * 2, s.mover.velocity.y, dz / dl * 2);
            slopeStart[k] = 2;
            Vec3 st0 = s.mover.position;
            for (int i = 0; i < 360; i++)
            {
                IceTick30(s, ice, syaw, default);
                Vec3 q = s.mover.position;
                double along = ((q.x - st0.x) * dx + (q.z - st0.z) * dz) / dl;
                if (along > dl + 1) break;
            }
            slopeEnd[k] = HSpeed(s);
            lines.Add($"glide30 {(on ? "ON " : "OFF")} (ice friction x{ice.ToString("0.00", ci)}): 16 m/s glide with no input -> {decay1[k].ToString("0.0", ci)} after 1 s, {decay2[k].ToString("0.0", ci)} after 2 s; ONE DASH at {before.ToString("0.0", ci)} m/s -> {(before * (1 + dashGain[k])).ToString("0.0", ci)} 0.5 s after it ({(100 * dashGain[k]).ToString("+0;-0", ci)} %, glide dash adds {adds[k]}); dashing every 0.5 s for 6 s tops at {capMax[k].ToString("0.0", ci)} m/s; turn rate at 14 m/s with pure strafe held (90 deg off) {turn[k].ToString("0", ci)} deg/s; down the {w.length.ToString("0", ci)} m ramp (rise {w.rise.ToString("0.0", ci)} m, the drop {(Math.Max(yA, yB) - Math.Min(yA, yB)).ToString("0.0", ci)} m stood) from 2 m/s -> {slopeEnd[k].ToString("0.0", ci)} m/s at its foot");
        }
        foreach (string l in lines) Console.WriteLine(l);
        if (off)
        {
            bool red = dashGain[0] < 0.2;
            Console.WriteLine(red ? $"glide30 RED (as expected: off, a dash on ice gains {(100 * dashGain[0]).ToString("0", ci)} %, under 20 %)" : "glide30 NOT RED");
            code = red ? 1 : 2;
            return;
        }
        bool ok = dashGain[1] >= 0.2 && decay1[1] > decay1[0] && slopeEnd[1] > slopeEnd[0] && capMax[1] <= cfg.Num("design.n30.glide.softCap") + 6 && turn[1] >= turn[0] * 0.8;
        Console.WriteLine(ok ? "glide30 GREEN: a dash on ice gains >= 20 %, the glide carries further, downhill adds, the cap holds, steering kept" : "glide30 FAIL");
        code = ok ? 0 : 1;
    }

    struct Btn30
    {
        public bool held, pressed;
        public Skygrind.Core.Button B => new Skygrind.Core.Button { held = held, pressed = pressed };
    }

    // ---- lava30 ----------------------------------------------------------------------------------------------------------------------------

    /// <summary>`lava30 [--off] [--level path]`: item 3 on a CAMPAIGN level (default Design/levels/n30-foundry.json). When the level JSON has no `lava` array yet,
    /// the first campaign pit becomes one in memory in L's format (rect = the pit's, y = 0.8 m below the room floor, the room's respawnY removed, as
    /// LEVEL-FORMAT.md says a lava room has none). The player drops onto the lava from 0.5 m above the floor (no input): the first touch costs
    /// damageFraction of max health, the knock lifts the feet above the rim (the room floor), the grace holds, NO stage respawn and no teleport (the
    /// feet stay over the pit's rect); a husk dropped into the lava burns (lavaEnemyBurn) and dies (lavaEnemyDeath) within enemyBurnSeconds + 0.1 s.
    /// `--off`: design.n30.lava off: no burn (RED).</summary>
    static void Lava30Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", "Design/levels/n30-foundry.json");
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        bool injected = false;
        string roomId;
        double floorY = 0;
        if (!json.ContainsKey("lava"))
        {
            var camp = PortData.Obj(json["campaign"]);
            var pit = PortData.Obj(PortData.Arr(camp["pits"])[0]);
            roomId = (string)pit["zone"];
            var rect = PortData.Arr(pit["rect"]);
            foreach (object ro in PortData.Arr(camp["rooms"]))
            {
                var rd = PortData.Obj(ro);
                if ((string)rd["id"] == roomId) rd.Remove("respawnY");
            }
            json["lava"] = new List<object> { new Dictionary<string, object> { ["rect"] = new List<object> { rect[0], rect[1], rect[2], rect[3] }, ["y"] = floorY - 0.8, ["room"] = roomId } };
            injected = true;
        }
        else roomId = (string)PortData.Obj(PortData.Arr(json["lava"])[0])["room"];
        LevelDef level = LevelDef.From(json);
        var lz = PortData.Obj(PortData.Arr(json["lava"])[0]);
        var lr = PortData.Arr(lz["rect"]);
        double x0 = Math.Min(PortData.Num(lr[0]), PortData.Num(lr[2])), x1 = Math.Max(PortData.Num(lr[0]), PortData.Num(lr[2]));
        double z0 = Math.Min(PortData.Num(lr[1]), PortData.Num(lr[3])), z1 = Math.Max(PortData.Num(lr[1]), PortData.Num(lr[3]));
        double ly = PortData.Num(lz["y"]), cx = (x0 + x1) / 2, cz = (z0 + z1) / 2;
        RoomDef room = level.rooms.Find(r => r.id == roomId);
        floorY = room != null ? room.spawn.y : 0;

        SurvivalSim sim = CampaignSim30(cfg, map, off ? N30Blocks.Off : new N30Blocks { lava = true });
        sim.n29Override = new N29Blocks { doorFree = true };
        sim.StartRun(3040, null, null);
        for (int s = 0; s < 360; s++) { sim.doorFreeHeld = true; sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); }
        sim.doorFreeHeld = false;
        if (room != null) { sim.Respawn(room.spawn, room.spawnYawDeg); for (int s = 0; s < 60; s++) { sim.health.hp = sim.health.max; sim.Step(); } }
        int burns = 0, enemyBurns = 0, enemyDeaths = 0, firstBurn = -1, step = 0;
        double hpAfterFirst = -1, maxYAfter = double.MinValue;
        sim.events.On<Lava30Event>("lavaBurn", e => { burns++; if (firstBurn < 0) firstBurn = step; });
        sim.events.On<Lava30Event>("lavaEnemyBurn", e => enemyBurns++);
        sim.events.On<Lava30Event>("lavaEnemyDeath", e => enemyDeaths++);
        sim.mover.invulnerable = false;
        sim.health.hp = sim.health.max;
        double max = sim.health.max;
        int respawns0 = sim.campaign != null ? sim.campaign.stageRespawns : 0;
        sim.Respawn(new Vec3(cx, floorY + 0.5, cz), 0);
        bool stayed = true;
        int graceGap = int.MaxValue, lastBurn = -1;
        sim.events.On<Lava30Event>("lavaBurn", e => { if (lastBurn >= 0) graceGap = Math.Min(graceGap, step - lastBurn); lastBurn = step; });
        for (step = 0; step < 240; step++)
        {
            sim.Step();
            if (firstBurn >= 0 && hpAfterFirst < 0) hpAfterFirst = sim.health.hp;
            if (firstBurn >= 0) maxYAfter = Math.Max(maxYAfter, sim.mover.position.y);
            Vec3 p = sim.mover.position;
            if (p.x < x0 - 1.5 || p.x > x1 + 1.5 || p.z < z0 - 1.5 || p.z > z1 + 1.5) stayed = false;
            if (sim.health.hp < max * 0.3) sim.health.hp = max; // keep the player alive through repeated touches (the count is what is judged)
        }
        int respawns = (sim.campaign != null ? sim.campaign.stageRespawns : 0) - respawns0;
        // the enemy
        sim.mover.invulnerable = true;
        sim.Respawn(room != null ? room.spawn : map.playerSpawn, 0);
        Husk h = sim.husks.Spawn("husk", new Vec3(cx, ly + 0.02, cz), sim.mover.position);
        int deathStep = -1;
        for (step = 0; step < 240 && h != null; step++)
        {
            sim.health.hp = sim.health.max;
            sim.Step();
            if (deathStep < 0 && !h.alive) deathStep = step;
        }
        double burnWant = cfg.Num("design.n30.lava.enemyBurnSeconds");
        int graceWant = MovementMath.StepsFor(cfg.Num("design.n30.lava.graceSeconds"), sim.step);
        double frac = hpAfterFirst >= 0 ? 1 - hpAfterFirst / max : 0;
        Console.WriteLine($"lava30 {(off ? "OFF (design.n30.lava off)" : "ON (lava)")}: level {level.id}{(injected ? $", pit '{roomId}' made lava in memory" : $", plan lava in '{roomId}'")} rect x {x0}..{x1} z {z0}..{z1}, surface y {ly.ToString("0.00", ci)} (floor {floorY.ToString("0.00", ci)})");
        Console.WriteLine($"lava30 player: burns {burns} in 2 s (first at step {firstBurn}), first touch cost {(100 * frac).ToString("0.0", ci)} % of max (want {(100 * cfg.Num("design.n30.lava.damageFraction")).ToString("0", ci)} %), the knock lifted the feet to y {maxYAfter.ToString("0.00", ci)} (rim {floorY.ToString("0.00", ci)}: {(maxYAfter > floorY ? "above" : "BELOW")}), shortest gap between burns {(graceGap == int.MaxValue ? "-" : graceGap.ToString())} steps (grace {graceWant}), stage respawns {respawns}, stayed over the pit {stayed}");
        Console.WriteLine($"lava30 enemy: husk dropped in: lavaEnemyBurn {enemyBurns}, lavaEnemyDeath {enemyDeaths}, dead after {(deathStep < 0 ? "-" : (deathStep * sim.step).ToString("0.00", ci) + " s")} (burn {burnWant.ToString("0.0", ci)} s)");
        if (off)
        {
            bool red = burns == 0 && enemyBurns == 0;
            Console.WriteLine(red ? "lava30 RED (as expected: off, the lava does nothing)" : "lava30 NOT RED: off yet the lava burned");
            code = red ? 1 : 2;
            return;
        }
        bool ok = burns >= 1 && Math.Abs(frac - cfg.Num("design.n30.lava.damageFraction")) < 1e-6 && maxYAfter > floorY && respawns == 0 && stayed && (graceGap == int.MaxValue || graceGap >= graceWant)
            && enemyBurns >= 1 && enemyDeaths >= 1 && deathStep >= 0 && deathStep * sim.step <= burnWant + 0.1;
        Console.WriteLine(ok ? "lava30 GREEN: a touch costs 25 %, the knock lifts the player above the rim, no respawn, the enemy burns and dies" : "lava30 FAIL");
        code = ok ? 0 : 1;
    }

    // ---- rules30 ---------------------------------------------------------------------------------------------------------------------------

    /// <summary>`rules30 [--off] [--steps n]`: item 1's contract on SURVIVAL with NIGHT29's lockout and shotgun and NIGHT27's PULSE glove on (the game GG played),
    /// the revolver, shotgun, nailgun, rocket and gauntlet owned, a punching bag in front, random input for n steps (the trigger held in runs, punches, LT
    /// presses, weapon asks; the glove's pips filled every 2000 steps so the blast happens). The judge reads only events and cooldowns:
    /// an ACTION is m = min(the cooldown the shot / heavy swing / punch set, actionSteps) steps from its step; OVERLAPS = a punch inside a gun-hand action, a
    /// shot inside a punch's action, a melee with the gauntlet in hand, a switch run strictly inside an action; LONG WAITS = a switch that ran more than one step
    /// after the action in progress at its ask ended; FROZEN = a holstered gun's cooldown (or the blast's) that did not tick down by one.
    /// `--off`: design.n30.rules off (NIGHT29's lockout): switches wait the whole cooldown (RED).</summary>
    static void Rules30Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        int total = int.Parse(Opt30(args, "--steps", "10000"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = cfg.Dt;
        int A = MovementMath.StepsFor(cfg.Num("design.n30.rules.actionSeconds"), dt);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n27Override = new N27Blocks { pulse27 = true };
        sim.n29Override = new N29Blocks { lockout = true, shotgun = true };
        sim.n30Override = off ? N30Blocks.Off : new N30Blocks { rules = true };
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(3020, null, null);
        sim.mover.invulnerable = true;
        string[] guns = { "revolver", "shotgun", "nailgun", "rocket", "gauntlet" };
        foreach (string g in guns) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Husk h = Bag25(sim, map.playerSpawn, ux, uz, 1.6);
        var rng = new Mulberry32(3030);
        int step = 0, gunT = -100000, gunM = 0, punchT = -100000, punchM = 0;
        int shots = 0, swings = 0, punches = 0, swaps = 0, blasts = 0, asks = 0;
        int ovPunchInGun = 0, ovFireInPunch = 0, ovGauntletMelee = 0, ovSwitchInAction = 0, longWaits = 0, frozen = 0, ticked = 0, worstExtra = 0;
        string weaponBefore = "revolver";
        int askStep = -1, askLeft = 0;
        Loadout lo = sim.loadout;
        Action<string> gunAction = w =>
        {
            int c = lo.CooldownStepsOf(w);
            gunT = step;
            gunM = Math.Min(c, A);
        };
        sim.events.On("fire", o =>
        {
            shots++;
            if (step >= punchT && step < punchT + punchM) ovFireInPunch++;
            gunAction(o as string ?? "");
        });
        sim.events.On<MeleeEvent>("meleeSwing", e => { swings++; if (step >= punchT && step < punchT + punchM) ovFireInPunch++; gunAction("gauntlet"); });
        sim.events.On("pulse27Blast", o => { blasts++; gunT = step; gunM = A; });
        sim.events.On<MeleeEvent>("punch", e =>
        {
            punches++;
            if (step > gunT && step < gunT + gunM) ovPunchInGun++;
            if (weaponBefore == "gauntlet" && lo.Weapon == "gauntlet") ovGauntletMelee++;
            punchT = step;
            punchM = Math.Min(MovementMath.StepsFor(cfg.Num("punch.cycleTime"), dt), A);
        });
        sim.events.On("weaponSwap", o =>
        {
            swaps++;
            if ((step > gunT && step < gunT + gunM) || (step > punchT && step < punchT + punchM)) ovSwitchInAction++;
            if (askStep >= 0)
            {
                int extra = step - askStep - askLeft;
                if (extra > 1) longWaits++;
                if (extra > worstExtra) worstExtra = extra;
                askStep = -1;
            }
        });
        var prevCd = new Dictionary<string, int>();
        int prevBlast = 0;
        bool fireHeld = false;
        for (step = 0; step < total; step++)
        {
            double py = 0;
            Pin25(h, map.playerSpawn, ux, uz, 1.6);
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            if (step % 2000 == 1000 && lo.pulse27 != null) lo.pulse27.pips = lo.pulse27.p.pips; // the glove full: the blast can go
            if (rng.Next() < 0.06) fireHeld = !fireHeld;
            bool punch = rng.Next() < 0.07;
            bool lt = rng.Next() < 0.02;
            int want = -1;
            if (rng.Next() < 0.02 && lo.Switching30 < 0 && askStep < 0)
            {
                int cur = lo.owned.IndexOf(lo.Weapon);
                want = (cur + 1 + (int)(rng.Next() * (lo.owned.Count - 1))) % lo.owned.Count;
                askStep = step;
                askLeft = Math.Max(0, Math.Max(gunT + gunM - step, punchT + punchM - step));
                asks++;
            }
            var c = new CombatInput { weapon = want };
            if (fireHeld) c.fire = new Button { held = true, pressed = rng.Next() < 0.5 };
            if (punch) c.punch = new Button { held = true, pressed = true };
            if (lt) c.altFire = new Button { held = true, pressed = true };
            weaponBefore = lo.Weapon;
            foreach (string g in guns) prevCd[g] = lo.CooldownStepsOf(g);
            prevBlast = lo.pulse27 != null ? lo.pulse27.cooldownSteps : 0;
            Tick27(sim, yaw, py, c);
            foreach (string g in guns)
            {
                if (g == lo.Weapon || g == weaponBefore) continue; // holstered the whole step
                int p = prevCd[g];
                if (p <= 0) continue;
                if (lo.CooldownStepsOf(g) == p - 1) ticked++;
                else frozen++;
            }
            if (lo.pulse27 != null && prevBlast > 0 && weaponBefore != "gauntlet" && lo.Weapon != "gauntlet")
            {
                if (lo.pulse27.cooldownSteps == prevBlast - 1) ticked++;
                else frozen++;
            }
        }
        int overlaps = ovPunchInGun + ovFireInPunch + ovGauntletMelee + ovSwitchInAction;
        bool alive = shots > 50 && punches > 30 && swaps > 20 && swings > 3;
        Rules30 r = lo.rules30;
        string drops = r != null ? $"; rules dropped {r.droppedMelee} melee presses, {r.droppedFireSteps} trigger steps, {r.waitedSwitches} switch asks waited (longest {r.longestWait} steps)" : sim.lockout29 != null ? $"; lockout29 dropped {sim.lockout29.droppedMelee} melee, {sim.lockout29.droppedFireSteps} trigger steps, {sim.lockout29.waitedSwitches} switches waited" : "";
        Console.WriteLine($"rules30 {(off ? "OFF (NIGHT29's lockout)" : "ON")}: {total} random steps, action cap {A} steps; {shots} shots, {swings} heavy swings, {blasts} blasts, {punches} punches, {asks} switch asks, {swaps} switches{drops}");
        Console.WriteLine($"rules30 OVERLAPS {overlaps} (punch in a gun action {ovPunchInGun}, shot in a punch action {ovFireInPunch}, melee with the gauntlet in hand {ovGauntletMelee}, switch inside an action {ovSwitchInAction}); LONG WAITS {longWaits} (switches that waited past the action in progress; worst +{worstExtra} steps = {(worstExtra * dt).ToString("0.00", CultureInfo.InvariantCulture)} s); holstered cooldown steps ticked {ticked}, FROZEN {frozen}");
        if (off)
        {
            bool red = longWaits > 0;
            Console.WriteLine(red ? "rules30 RED (as expected: off, a switch waits the whole cooldown)" : "rules30 NOT RED: off and no long wait (the judge is blind)");
            code = red ? 1 : 2;
            return;
        }
        // the tape: a rules-ON run (fire held, punches, switches every 2.5 s) replayed from the tape alone
        SurvivalSim st = SurvivalSim.Headless(cfg, map);
        st.noRecords = st.noLab = true;
        st.setup.mode = RunSetup.Survival;
        st.setup.heat = 0;
        st.n27Override = new N27Blocks { pulse27 = true };
        st.n29Override = new N29Blocks { lockout = true, shotgun = true };
        st.n30Override = new N30Blocks { rules = true };
        var sti = new ScriptedInput();
        st.UseInput(sti);
        RunTape tape = TapeScript.Record(st, SimFingerprint.Seed, 3600, "rules30", (sm, i) =>
        {
            sti.Set(new InputFrame { moveX = (i / 120) % 2 == 0 ? 1 : -1, moveZ = 1, jump = new Button { pressed = i % 180 == 0, held = i % 180 < 10 } },
                new CombatInput { fire = new Button { held = true, pressed = i % 40 == 0 }, punch = new Button { held = i % 17 == 0, pressed = i % 17 == 0 }, weapon = i % 300 == 150 ? (i / 300) % 2 : -1 });
        }, TapeScript.God, false);
        TapeResult tr = TapeReplay.Replay(Config(), map, tape);
        Console.WriteLine($"rules30 tape: {tape.steps} steps, n30 bits {tape.n30}, rules waited {st.loadout.rules30?.waitedSwitches} switch asks; replay {tr.Line}");
        // the smoke's step 3d pattern: the gun's own slot asked EVERY step while the trigger is held: the shots must equal the rules-off count (an ask for the slot in
        // hand is no switch and must not hold the trigger)
        int[] smokeShots = new int[2];
        foreach (string gun in new[] { "nailgun", "rocket", "rail" })
            for (int k = 0; k < 2; k++)
            {
                SurvivalSim s3 = SurvivalSim.Headless(cfg, map);
                s3.noRecords = s3.noLab = true;
                s3.n29Override = new N29Blocks { lockout = true };
                s3.n30Override = k == 1 ? new N30Blocks { rules = true } : N30Blocks.Off;
                s3.setup.mode = RunSetup.Survival;
                s3.StartRun(3021, null, null);
                s3.mover.invulnerable = true;
                if (!s3.loadout.owned.Contains(gun)) s3.loadout.AddWeapon(gun);
                int sl = s3.loadout.owned.IndexOf(gun);
                s3.events.On("fire", o => { if (o as string == gun) smokeShots[k]++; });
                for (int i = 0; i < 1440; i++) Tick27(s3, yaw, 0, new CombatInput { weapon = sl, fire = new Skygrind.Core.Button { held = true, pressed = true } });
            }
        Console.WriteLine($"rules30 smoke-3d pattern (slot asked every step, trigger held, 12 s each of nailgun / rocket / rail): shots rules OFF {smokeShots[0]}, ON {smokeShots[1]}");
        bool ok = overlaps == 0 && longWaits == 0 && frozen == 0 && ticked > 100 && alive && tr.Equal && tape.n30 == 1 && smokeShots[1] == smokeShots[0];
        Console.WriteLine(ok ? "rules30 GREEN: 0 overlaps, 0 switches that wait past the action, holstered cooldowns tick" : $"rules30 FAIL{(alive ? "" : " (the run did too little)")}");
        code = ok ? 0 : 1;
    }

    // ---- lesson30 --------------------------------------------------------------------------------------------------------------------------

    /// <summary>`lesson30 [--off]`: P0 b / d + item 12 on the n29 levels (DOORFREE on to walk every room in order, the player invulnerable, 2 s a room):
    /// the line model (the last lessonBegin minus lessonClear) at every roomEnter must hold no line from another room (STALE), and a boss room is entered
    /// with no room line up; BossLine30 shows one line at most design.n30.teach.maxShows times per fight; the boss bar's gate (BossBarHidden30) is
    /// hidden in every non-boss room and open in the boss room. `--off`: design.n30.teach off: NIGHT29's rule (a line stays: RED).</summary>
    static void Lesson30Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        GameConfig cfg = Config();
        int stale = 0, bossWithLine = 0, rooms = 0, begins = 0, clears = 0, barWrong = 0, barRooms = 0;
        var notes = new List<string>();
        bool n30Maps = Flag(args, "--n30"); // round 5: the rebuilt levels (n30-foundry / iceline / kiln / span) with the config's rows
        string[] levels = n30Maps ? new[] { "foundry", "iceline", "kiln", "span" } : new[] { "foundry", "iceline", "kiln" };
        foreach (string lv in levels)
        {
            SurvivalMap map = CampaignMapFrom($"Design/levels/{(n30Maps ? "n30" : "n29")}/{lv}.json", cfg, out var json);
            LevelDef level = LevelDef.From(json);
            N30Blocks rows = n30Maps ? N30Blocks.From(cfg) : new N30Blocks();
            rows.teach = !off;
            SurvivalSim sim = CampaignSim30(cfg, map, off && !n30Maps ? N30Blocks.Off : rows);
            sim.n29Override = n30Maps ? N29Blocks.From(cfg) is N29Blocks r29 ? WithDoorFree(r29) : default : new N29Blocks { doorFree = true, pickups = true, roomRules = true, lockout = true, tank = true };
            sim.StartRun(3010, null, null);
            string cur = null;
            int curRoom = -1;
            sim.events.On<LessonEvent>("lessonBegin", e => { begins++; cur = e.prompt; curRoom = e.room; });
            sim.events.On<LessonEvent>("lessonClear", e => { clears++; cur = null; });
            sim.events.On<RoomEvent>("roomEnter", e =>
            {
                rooms++;
                if (cur != null && curRoom != e.room) { stale++; if (notes.Count < 6) notes.Add($"{lv} {e.id}: '{cur}' (room {curRoom + 1}) still up"); }
                if (cur != null && e.kind == "boss") bossWithLine++;
            });
            for (int s = 0; s < 360; s++) { sim.doorFreeHeld = true; sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); }
            sim.doorFreeHeld = false;
            for (int k = 0; k < level.rooms.Count; k++)
            {
                RoomDef r = level.rooms[k];
                sim.Respawn(r.spawn, r.spawnYawDeg);
                for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); }
                if (sim.campaign != null && sim.campaign.room == k)
                {
                    barRooms++;
                    bool hidden = sim.BossBarHidden30;
                    if (hidden == (r.kind == "boss")) { barWrong++; if (notes.Count < 8) notes.Add($"{lv} {r.id} ({r.kind}): bar hidden {hidden}"); }
                }
            }
        }
        // the r6 frame's path: a line begun in the first room, then a RESPAWN straight into the boss room (no door, no Enter): the line must be gone in 0.25 s
        int jumpStale = 0, jumpChecked = 0;
        var jumpNotes = new List<string>();
        foreach (string lv in levels)
        {
            SurvivalMap map = CampaignMapFrom($"Design/levels/{(n30Maps ? "n30" : "n29")}/{lv}.json", cfg, out var json);
            LevelDef level = LevelDef.From(json);
            N30Blocks rows = n30Maps ? N30Blocks.From(cfg) : new N30Blocks();
            rows.teach = !off;
            SurvivalSim sim = CampaignSim30(cfg, map, off && !n30Maps ? N30Blocks.Off : rows);
            string up = null;
            sim.events.On<LessonEvent>("lessonBegin", e => up = e.id + ": " + e.prompt);
            sim.events.On<LessonEvent>("lessonClear", e => up = null);
            sim.StartRun(3012, null, null);
            for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); }
            string before = up;
            RoomDef boss = level.rooms.Find(r => r.kind == "boss");
            if (before == null || boss == null) continue;
            sim.Respawn(boss.spawn, boss.spawnYawDeg);
            for (int s = 0; s < 30; s++) { sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); }
            jumpChecked++;
            if (up != null && !up.StartsWith("boss")) { jumpStale++; jumpNotes.Add($"{lv}: '{up}' still up in {boss.id}"); }
        }
        Console.WriteLine($"lesson30 respawn into the boss room: {jumpChecked} levels with a first-room line up, still up 0.25 s after the respawn: {jumpStale}{(jumpNotes.Count > 0 ? " (" + string.Join("; ", jumpNotes) + ")" : "")}");
        stale += jumpStale;
        // the boss-line budget
        SurvivalSim sb = CampaignSim30(cfg, CampaignMapFrom("Design/levels/n29/foundry.json", cfg, out _), off ? N30Blocks.Off : new N30Blocks { teach = true });
        sb.StartRun(3011, null, null);
        int shown = 0;
        for (int i = 0; i < 5; i++) if (sb.BossLine30("test.lava", "GET ABOVE THE LAVA")) shown++;
        int max = (int)cfg.Num("design.n30.teach.maxShows");
        Console.WriteLine($"lesson30 {(off ? "OFF (design.n30.teach off)" : "ON (teach)")}: {levels.Length} {(n30Maps ? "n30 levels (" + string.Join(", ", levels) + ", the config's rows)" : "n29 levels")}, {rooms} room entries, {begins} lessonBegin, {clears} lessonClear; STALE lines at a room entry {stale}; boss rooms entered with a room line up {bossWithLine}; boss line asked 5x, shown {shown} (maxShows {max}); boss-bar gate wrong in {barWrong} of {barRooms} rooms");
        foreach (string n in notes) Console.WriteLine("  " + n);
        if (off)
        {
            bool red = stale > 0;
            Console.WriteLine(red ? "lesson30 RED (as expected: off, a room's line stays into the next room)" : "lesson30 NOT RED: off and no stale line (the judge is blind)");
            code = red ? 1 : 2;
            return;
        }
        bool ok = stale == 0 && bossWithLine == 0 && shown == max && barWrong == 0 && rooms > 20 && begins > 0;
        Console.WriteLine(ok ? "lesson30 GREEN" : "lesson30 FAIL");
        code = ok ? 0 : 1;
    }

    // ---- shared ----------------------------------------------------------------------------------------------------------------------------

    /// <summary>A CAMPAIGN composition on a built level map (not started).</summary>
    static SurvivalSim CampaignSim30(GameConfig cfg, SurvivalMap map, N30Blocks n30)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { campaign = true };
        sim.n25Override = new N25Blocks { tutorial = true };
        sim.n30Override = n30;
        sim.setup.mode = Campaign24.Mode;
        return sim;
    }

    static string Opt30(string[] args, string name, string def)
    {
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
        return def;
    }

    // ---- arcline30 -------------------------------------------------------------------------------------------------------------------------

    /// <summary>A node in the open above room 1: 3.5 m up and 7 m out along the first direction with a clear line from the eye (both ways) and 1.5 m of
    /// air around it; the look that aims at it from the spawn.</summary>
    static bool NodeAbove30(SurvivalSim sim, Vec3 spawn, out Vec3 node)
    {
        var eye = new Vec3(spawn.x, spawn.y + 1.6, spawn.z);
        for (int k = 0; k < 32; k++)
        {
            double a = k * Math.PI / 16;
            foreach (double dist in new[] { 7.0, 5.5, 4.5 })
            foreach (double up in new[] { 4.5, 4.0, 3.5 })
            {
                var p = new Vec3(spawn.x + Math.Sin(a) * dist, spawn.y + up, spawn.z - Math.Cos(a) * dist);
                Vec3 t = p - eye;
                double len = t.Length;
                if (sim.world.RayIntersect(eye, t / len, out RayHit hit) && hit.distance < len + 0.5) continue;
                bool air = true;
                foreach (Vec3 d in new[] { new Vec3(1, 0, 0), new Vec3(-1, 0, 0), new Vec3(0, 0, 1), new Vec3(0, 0, -1), new Vec3(0, 1, 0) })
                    if (sim.world.RayIntersect(p, d, out RayHit h2) && h2.distance < 0.8) air = false;
                if (!air) continue;
                node = p;
                return true;
            }
        }
        node = default;
        return false;
    }

    static void Arcline30Command(string[] args, ref int code)
    {
        bool off = Flag(args, "--off");
        CultureInfo ci = CultureInfo.InvariantCulture;
        GameConfig cfg = Config();
        string path = Opt30(args, "--level", File.Exists("Design/levels/n30-span.json") ? "Design/levels/n30-span.json" : "Design/levels/n29-foundry.json");
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        RoomDef r0 = level.rooms[0];
        bool added = false;
        if (map.grappleNodes.Count == 0)
        {
            SurvivalSim probe = CampaignSim30(cfg, map, N30Blocks.Off);
            probe.StartRun(3001, null, null);
            if (!NodeAbove30(probe, r0.spawn, out Vec3 n)) { Console.WriteLine($"arcline30: no open point for a node above {level.id} room 1"); code = 2; return; }
            map.grappleNodes.Add(n);
            added = true;
        }
        var n30 = off ? N30Blocks.Off : new N30Blocks { span = true };
        double hz = cfg.movement.loop.simHz, dt = 1 / hz;
        int swingWant = MovementMath.StepsFor(cfg.movement.arcline.cooldown, dt), yankWant = MovementMath.StepsFor(cfg.Num("design.n21.arcline.yankCooldown"), dt);

        // A. owned from the start; a press at the node attaches on the NODE; the let-go starts the 0.4 s swing cooldown
        SurvivalSim sim = CampaignSim30(cfg, map, n30);
        var si = new ScriptedInput();
        sim.UseInput(si);
        sim.StartRun(3002, null, null);
        for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.mover.invulnerable = true; Tick25(sim, si.Yaw, si.Pitch, new CombatInput { weapon = -1 }); }
        bool owned = sim.arcline21 != null && sim.arcline21.Owned;
        Vec3 node = map.grappleNodes[0];
        // stand at the node's foot side (the spawn of the room whose node it is: the nearest room spawn)
        RoomDef near = r0;
        double bestD = double.MaxValue;
        foreach (RoomDef r in level.rooms) { double d = Vec3.DistanceTo(r.spawn, node); if (d < bestD) { bestD = d; near = r; } }
        sim.Respawn(near.spawn, near.spawnYawDeg);
        sim.mover.invulnerable = true;
        int attachNode = 0, fires = 0, releases = 0, attachStep = -1, releaseStep = -1, cdAfter = -1;
        string releaseWhy = null;
        int step = 0;
        sim.events.On<Arcline21RopeEvent>("ropeFire", e => fires++);
        sim.events.On<Arcline21RopeEvent>("ropeAttach", e => { if (e.node) { attachNode++; if (attachStep < 0) attachStep = step; } });
        sim.events.On<Arcline21RopeEvent>("ropeRelease", e => { releases++; if (releaseStep < 0) { releaseStep = step; releaseWhy = e.reason; } });
        double maxY = sim.mover.position.y, y0 = sim.mover.position.y;
        for (step = 0; step < 360; step++)
        {
            (double yaw, double pitch) = ArclineLook(sim.Eye, node);
            bool held = step < 150; // hold for 1.25 s, then let go
            var inp = si;
            inp.SetLook(yaw, pitch);
            inp.Set(new InputFrame { grapple = new Button { held = held, pressed = step == 0 } }, new CombatInput { weapon = -1 });
            sim.health.hp = sim.health.max;
            sim.Step();
            maxY = Math.Max(maxY, sim.mover.position.y);
            if (step == releaseStep) cdAfter = sim.mover.rope.cooldownSteps;
        }
        // B. the yank: a husk 8 m ahead, a press at it, the yank's cooldown
        SurvivalSim simY = CampaignSim30(cfg, map, n30);
        var siY = new ScriptedInput();
        simY.UseInput(siY);
        simY.StartRun(3003, null, null);
        for (int s = 0; s < 240; s++) { simY.health.hp = simY.health.max; Tick25(simY, siY.Yaw, siY.Pitch, new CombatInput { weapon = -1 }); }
        simY.Respawn(near.spawn, near.spawnYawDeg);
        simY.mover.invulnerable = true;
        (double oy, double ux, double uz) = ArclineOpenDir(simY, near.spawn, 6);
        Husk h = Bag25(simY, near.spawn, ux, uz, 6);
        int pulls = 0;
        simY.events.On<Arcline21PullEvent>("arclinePull", e => pulls++);
        int yankSteps = -1;
        for (int s = 0; s < 6 && h != null; s++)
        {
            (double yaw, double pitch) = ArclineLook(simY.Eye, h.AimPoint);
            siY.SetLook(yaw, pitch);
            siY.Set(new InputFrame { grapple = new Button { held = s == 0, pressed = s == 0 } }, new CombatInput { weapon = -1 });
            simY.health.hp = simY.health.max;
            simY.Step();
            if (s == 0) yankSteps = simY.arcline21 != null ? simY.arcline21.YankSteps : -1;
        }
        // C. the tape: a campaign run with grapple presses at the node every 2 s (held 1 s), replayed from the tape alone
        SurvivalSim simT = CampaignSim30(cfg, map, n30);
        var siT = new ScriptedInput();
        simT.UseInput(siT);
        int tapeFires = 0;
        simT.events.On<Arcline21RopeEvent>("ropeFire", e => tapeFires++);
        RunTape tape = TapeScript.Record(simT, 3004, 2400, "arcline30", (sm, i) =>
        {
            // aim at the node nearest the player (no harness respawn mid tape: the replay has only the tape's inputs)
            Vec3 aimAt = node;
            double bd = double.MaxValue;
            foreach (Vec3 gn in map.grappleNodes) { double d = Vec3.DistanceTo(gn, sm.mover.position); if (d < bd) { bd = d; aimAt = gn; } }
            (double yaw, double pitch) = ArclineLook(sm.Eye, aimAt);
            int k = i % 240;
            siT.SetLook(yaw, pitch);
            siT.Set(new InputFrame { moveZ = k > 150 ? 1 : 0, grapple = new Button { held = i >= 240 && k < 120, pressed = i >= 240 && k == 0 }, jump = new Button { pressed = k == 200, held = k >= 200 && k < 206 } },
                new CombatInput { weapon = -1, fire = new Button { held = k % 60 < 10, pressed = k % 60 == 0 } });
        }, TapeScript.God, false);
        TapeResult tr = TapeReplay.Replay(Config(), map, tape);

        Console.WriteLine($"arcline30 {(off ? "OFF (design.n30.span off)" : "ON (span)")}: level {level.id} ({path}){(added ? $", one node added in memory at ({node.x.ToString("0.0", ci)}, {node.y.ToString("0.0", ci)}, {node.z.ToString("0.0", ci)})" : $", {map.grappleNodes.Count} plan node(s)")}; campaignOn {sim.campaignOn}; ARCLINE owned at the start {owned}");
        Console.WriteLine($"arcline30 swing: fires {fires}, attached on a node {attachNode} (step {attachStep}), released '{releaseWhy}' at step {releaseStep} (held to step 150), climbed {(maxY - y0).ToString("0.00", ci)} m; swing cooldown after the let-go {cdAfter} steps (want {swingWant} = arcline.cooldown {cfg.movement.arcline.cooldown.ToString("0.00", ci)} s)");
        Console.WriteLine($"arcline30 yank: pulls {pulls}, the yank's cooldown {yankSteps} steps after its own step (want {yankWant} - 1; {yankWant} = {(yankWant / hz).ToString("0.0", ci)} s)");
        Console.WriteLine($"arcline30 tape: {tape.steps} campaign steps, {tapeFires} hooks fired, tape bits n30 {tape.n30}, level '{tape.campaign.levelId}'; replay: {tr.Line}");
        if (off)
        {
            bool red = !owned && fires == 0;
            Console.WriteLine(red ? "arcline30 RED (as expected: span off, no ARCLINE in the campaign)" : "arcline30 NOT RED: span off yet the ARCLINE is owned");
            code = red ? 1 : 2;
            return;
        }
        bool ok = owned && attachNode >= 1 && cdAfter == swingWant && releaseWhy == "letGo" && pulls == 1 && yankSteps + 1 == yankWant && tapeFires > 0 && tr.Equal && tape.n30 == 64;
        Console.WriteLine(ok ? "arcline30 GREEN: owned from the start, a node swing, 0.4 s swing / 15 s yank, the taped campaign run replays EQUAL" : "arcline30 FAIL");
        code = ok ? 0 : 1;
    }
}
