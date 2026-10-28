// NIGHT32 (builder F, DESIGN32 card F): LEVEL 5 THE SPIRE's harness commands (dispatched by the lead's Program.cs: N32Finale(cmd, args, ref code)).
//   spire32 [--seeds n] [--first n] [--cap seconds] [--trace] [--noguard]
//     (round 3: the pilot runs inside Spire32Guard, its lava-edge path guard and lava escape; --noguard: the bare walk29 pilot)
//     the walk29 pilot (Walk29Pilot: the autopilot's fights and walks; Span30Pilot in rooms with rings while the ARCLINE is owned) plays
//     Design/levels/n32-spire.json room by room through the REAL sim with the design config as it is (the loadout from design.n31.unlocks: row n 5,
//     all four weapons); prints per room cleared k of n, the median seconds, deaths, falls, lessons; one SUMMARY line; exit 1 unless every seed clears
//     every playable room. The boss room (v9) is not played (builder C's). Same code as walk29 / walk30 (Harness.N29.Walk.cs) with the prefix n32.
//   order32
//     the campaign order (Core/Curriculum25.Order) with design.n32.level5 as the config has it, then forced on, then forced off (N32Blocks.ForceOff):
//     with the row on the order has n32-spire right after n30-span (5 levels); off, NIGHT31's 4. Then the weapons THE SPIRE starts with (design.n31.unlocks
//     row n 5) and earns (Design/levels/unlocks.json), and the level file (9 rooms, combat rooms <= 24 m, the last the boss room with boss "crown").
//   spiretrace32 [seconds] [seed] [room] [every]
//     a debugging aid: the spire32 pilot printing the feet, state, health, enemies, the campaign room / wave / phase and the doors every `every` s (0.5);
//     with a room id, DOORFREE and a respawn at its entry once the campaign runs; every room retry is printed with where it happened.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N32Finale(string cmd, string[] args, ref int code)
    {
        if (cmd == "spire32")
        {
            code = 0;
            bool noGuard = Array.IndexOf(args, "--noguard") >= 0;
            var rest = new List<string> { "walk29", "spire" };
            for (int i = 1; i < args.Length; i++) if (args[i] != "--noguard") rest.Add(args[i]);
            string old = walkPrefix;
            Action<SurvivalSim> oldHook = walkAfterStart;
            walkPrefix = "n32";
            if (!noGuard) walkAfterStart = sim => { if (sim.input is Walk29Pilot w) sim.UseInput(new Spire32Guard(sim, w)); };
            Console.WriteLine($"spire32: the walk29 pilot on Design/levels/n32-spire.json (the design config's loadout and blocks as they are){(noGuard ? "" : " + Spire32Guard (the pilot's lava-edge path guard)")}");
            try { N29Walk("walk29", rest.ToArray(), ref code); }
            finally { walkPrefix = old; walkAfterStart = oldHook; }
            return;
        }
        if (cmd == "order32")
        {
            code = 0;
            Order32(ref code);
        }
        if (cmd == "spiretrace32") { code = 0; SpireTrace32(args); }
    }

    /// <summary>`spiretrace32 [seconds] [seed]`: the spire32 pilot for a few seconds, printing every 0.5 s the feet, the move state, the campaign room / phase,
    /// the zone under the feet and the doors' open state (a debugging aid for a room the pilot stops in).</summary>
    static void SpireTrace32(string[] args)
    {
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        double secs = args.Length > 1 ? double.Parse(args[1], ci) : 20;
        uint seed = args.Length > 2 ? uint.Parse(args[2], ci) : 1;
        string startRoom = args.Length > 3 ? args[3] : ""; // a room id: DOORFREE, respawned at its entry after StartRun
        GameConfig cfg = Config();
        string path = System.IO.Path.Combine("Design", "levels", "n32-spire.json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(System.IO.Path.GetFullPath(path))));
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        var sim = Skygrind.Game.SurvivalSim.Headless(cfg, map);
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
        sim.UseInput(new Skygrind.Game.Walk29Pilot(sim, ap));
        sim.StartRun(seed, null, null);
        RoomDef sr = startRoom != "" ? sim.campaignLevel.rooms.Find(r => r.id == startRoom) : null;
        int dead = 0;
        sim.events.On<RoomEvent>("roomRetry", e => { dead++; Vec3 q = sim.mover.position; Console.WriteLine($"  RETRY in room {e.room} at ({q.x.ToString("0.0", ci)}, {q.y.ToString("0.0", ci)}, {q.z.ToString("0.0", ci)})"); });
        int every = MovementMath.StepsFor(args.Length > 4 ? double.Parse(args[4], ci) : 0.5, sim.step), max = MovementMath.StepsFor(secs, sim.step);
        for (int st = 0; st < max; st++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            if (sr != null && sim.campaign != null && sim.campaign.room >= 0) { sim.campaign.SetDoorFree29(true); sim.Respawn(sr.spawn, sr.spawnYawDeg); sr = null; } // the start room, once the campaign runs
            if (st % every != 0) continue;
            Vec3 p = sim.mover.position;
            Campaign24 c = sim.campaign;
            string doors = "";
            foreach (DoorDef d in sim.campaignLevel.doors) doors += $"{d.id}:{(sim.zones.IsOpen(d.id) ? "open" : "shut")} "; foreach (RoomDef rr in sim.campaignLevel.rooms) doors += $"{rr.id}:{(sim.zones.IsOpen(rr.id) ? "o" : "x")} ";
            Console.WriteLine($"t {(st * sim.step).ToString("0.0", ci)} feet ({p.x.ToString("0.00", ci)}, {p.y.ToString("0.00", ci)}, {p.z.ToString("0.00", ci)}) {sim.mover.state} g {sim.mover.grounded} hp {sim.health.hp.ToString("0", ci)} alive {sim.enemies.AliveCount} wave {c?.wave} room {c?.room} {c?.phase} zone {sim.zones.ZoneAt(new Vec3(p.x, p.y + 1, p.z))} | {doors}");
        }
        ap.Detach();
    }

    /// <summary>NIGHT32 F round 3 (the lead: "if the bot dies, fix the bot's path, not the room"): a wrapper round the walk29 pilot for THE SPIRE. On dry ground,
    /// a step that would carry the feet into a lava rect (the level's `lava` rows) is cancelled (the pilot stands and shoots; its own unstick still runs), except
    /// while a room's grapple crossing is ahead (feet south of the room's last ring + 1 m: Span30Pilot runs at the lava on purpose). In the lava, it walks to
    /// the nearest rim, hopping. Bot-only numbers (the harness pilot's), not gameplay.</summary>
    sealed class Spire32Guard : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly Walk29Pilot inner;
        readonly List<double[]> lava = new List<double[]>(); // [x0, z0, x1, z1]
        const double Probe = 1.2, RimOut = 1.5;
        int step, lavaSteps;
        public int blocked, escapes;

        public Spire32Guard(SurvivalSim sim, Walk29Pilot inner)
        {
            this.sim = sim;
            this.inner = inner;
            var json = PortData.Obj(PortData.LoadJson(System.IO.Path.GetFullPath(System.IO.Path.Combine("Design", "levels", "n32-spire.json"))));
            if (json.TryGetValue("lava", out object lo))
                foreach (object o in PortData.Arr(lo))
                {
                    List<object> r = PortData.Arr(PortData.Obj(o)["rect"]);
                    lava.Add(new[] { PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]) });
                }
        }

        public double Yaw => inner.Yaw;
        public double Pitch => inner.Pitch;
        public void SetLook(double y, double p) => inner.SetLook(y, p);
        public bool Captured => inner.Captured;
        public CombatInput SampleCombat(bool active) => inner.SampleCombat(active);

        double[] LavaAt(double x, double z)
        {
            foreach (double[] r in lava) if (x > r[0] && x < r[2] && z > r[1] && z < r[3]) return r;
            return null;
        }

        public InputFrame SampleMove(bool active)
        {
            InputFrame f = inner.SampleMove(active);
            step++;
            Campaign24 c = sim.campaign;
            if (!active || c == null || c.room < 0 || sim.mover.rope.phase != RopePhase.None) return f;
            RoomDef rd = sim.campaignLevel.rooms[c.room];
            Vec3 p = sim.mover.position;
            double[] inLava = LavaAt(p.x, p.z);
            if (inLava != null && p.y < -0.3) lavaSteps = 180; // touched the lava: its knock-ups keep the feet in the air over it for a while
            else if (inLava == null) lavaSteps = 0;
            else if (lavaSteps > 0) lavaSteps--;
            if (inLava != null && lavaSteps > 0) // in (or bouncing over) the lava: out by the nearest rim (the autopilot's graph has no way out)
            {
                double dW = p.x - inLava[0], dE = inLava[2] - p.x, dN = p.z - inLava[1], dS = inLava[3] - p.z, m = Math.Min(Math.Min(dW, dE), Math.Min(dN, dS));
                double tx = p.x, tz = p.z;
                if (m == dW) tx = inLava[0] - RimOut; else if (m == dE) tx = inLava[2] + RimOut; else if (m == dN) tz = inLava[1] - RimOut; else tz = inLava[3] + RimOut;
                escapes++;
                return new InputFrame { moveZ = 1, yaw = Math.Atan2(-(tx - p.x), -(tz - p.z)), pitch = f.pitch, jump = new Button { held = true, pressed = sim.mover.grounded && step % 12 == 0 } };
            }
            if (!sim.mover.grounded || p.y < -0.3) return f;
            // a grapple crossing ahead in this room: the span pilot runs at the lava on purpose
            double lastRing = double.PositiveInfinity;
            foreach (Vec3 n in sim.map.grappleNodes)
                if (n.x >= rd.x0 && n.x <= rd.x1 && n.z >= rd.z0 && n.z <= rd.z1) lastRing = Math.Min(lastRing, n.z);
            if (!double.IsPositiveInfinity(lastRing) && p.z > lastRing - 1) return f;
            double fx = -Math.Sin(f.yaw), fz = -Math.Cos(f.yaw), rx = Math.Cos(f.yaw), rz = -Math.Sin(f.yaw);
            double dx = fx * f.moveZ + rx * f.moveX, dz = fz * f.moveZ + rz * f.moveX, len = Math.Sqrt(dx * dx + dz * dz);
            if (len < 0.05) return f;
            if (LavaAt(p.x + dx / len * Probe, p.z + dz / len * Probe) == null) return f;
            blocked++;
            f.moveX = 0; f.moveZ = 0; f.jump = default; f.dash = default;
            return f;
        }
    }

    static void Order32(ref int code)
    {
        GameConfig cfg = Config();
        var n25 = N25Blocks.From(cfg);
        Console.WriteLine($"order32 config (level5 {(cfg.Design("n32.level5.enabled") ? "on" : "off")}): {string.Join(", ", Curriculum25.Order(cfg, n25))}");
        // forced on: the same config with design.n32.level5.enabled true (a copy of the design tree: the loaded config is left alone)
        object row = cfg.Get("design.n32.level5");
        if (row is Dictionary<string, object> d)
        {
            object was = d.TryGetValue("enabled", out object w) ? w : false;
            d["enabled"] = true;
            try
            {
                var cfgOn = new GameConfig(cfg.root); // a fresh config over the edited tree (GameConfig caches every path it has read)
                List<string> on = Curriculum25.Order(cfgOn, n25);
                Console.WriteLine($"order32 level5 forced on: {string.Join(", ", on)} ({on.Count} levels)");
                int at = on.IndexOf("n32-spire");
                if (at < 0 || at == 0 || on[at - 1] != (cfg.Str("design.n32.level5.after"))) { Console.WriteLine("order32 RED: n32-spire is not right after design.n32.level5.after"); code = 1; }
                bool was32 = N32Blocks.ForceOff;
                N32Blocks.ForceOff = true;
                try
                {
                    List<string> off = Curriculum25.Order(cfgOn, n25);
                    Console.WriteLine($"order32 level5 on, --off32: {string.Join(", ", off)} ({off.Count} levels)");
                    if (off.Contains("n32-spire")) { Console.WriteLine("order32 RED: --off32 still lists n32-spire"); code = 1; }
                }
                finally { N32Blocks.ForceOff = was32; }
            }
            finally { d["enabled"] = was; }
        }
        else { Console.WriteLine("order32 RED: no design.n32.level5 row"); code = 1; }
        // the weapons (Level5Tests' second test, outside Unity): design.n31.unlocks row n 5 and Design/levels/unlocks.json
        List<string> start = Unlocks31.From(cfg).StartFor("n32-spire");
        string earned = Unlocks26.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine("Design", "levels", "unlocks.json"))).Earned("n32-spire");
        Console.WriteLine($"order32 n32-spire starts with [{(start == null ? "null: not in the table" : string.Join(", ", start))}], earns {earned ?? "nothing"}");
        if (start == null || start.Count != 4 || !start.Contains("revolver") || !start.Contains("gauntlet") || !start.Contains("shotgun") || !start.Contains("nailgun")) { Console.WriteLine("order32 RED: not the four weapons"); code = 1; }
        // the level file (Level5Tests' third test)
        LevelDef lv = LevelDef.From(PortData.Obj(PortData.LoadJson(System.IO.Path.GetFullPath(System.IO.Path.Combine("Design", "levels", "n32-spire.json")))));
        RoomDef last = lv.rooms[lv.rooms.Count - 1];
        int big = lv.rooms.FindAll(r => r.kind == "room" && Math.Max(r.x1 - r.x0, r.z1 - r.z0) > 24).Count;
        Console.WriteLine($"order32 the level: {lv.rooms.Count} rooms, last {last.id} [{last.kind}] boss {last.boss}, combat rooms over 24 m: {big}");
        if (lv.rooms.Count != 9 || last.kind != "boss" || last.boss != "crown" || big > 0) { Console.WriteLine("order32 RED: the level file"); code = 1; }
        if (code == 0) Console.WriteLine("order32 PASS");
    }
}
