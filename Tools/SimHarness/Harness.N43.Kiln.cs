// NIGHT43 builder K1 (kiln43; NIGHT43.md card K1): level 3's checks, declared and called by the lead's Program.cs (N43Kiln). Every number is
// measured on the real sim (SurvivalSim.Headless, a campaign run on Design/levels/n43-kiln.json with kiln43 on, so every older system reads it
// as n40-kiln: the THUNDERCLAP start set, the awards, Walk38Kiln).
//   n43kiln steps [--write]
//       G26's number: THE STEPS (3.1, the room row's tops43) passed by a SCRIPTED PERSON, boost43 off and forced on. From each step's take-off
//       (tops43.take), standing, the stick forward, the jump pressed and held `hold` s (0.10: a tap, the jump cut halves a rise when it is let
//       go; 0.40: held), the look pitched straight down, the THUNDERCLAP tapped at an offset from the jump press (-0.15 .. +0.45 s: negative =
//       the shot pressed first, on the ground), and in the second variant the double jump at the apex. A try lands when the feet stand on the
//       step's top within 5 s. Prints, per step, the share of tries that land by offset; and the apexes (standing jump, jump + double, the best
//       one boost, the best boost + double) over the take-off. --write: Design/levels/n43/kiln-boost.json (the generator reads it).
//   n43kiln walk [--seeds n] [--first n] [--cap s] [--b43] [--trace]
//       walk29's pilot (Walk29Pilot + Walk38Kiln: the gates, the pours) through n43-kiln with kiln43 on (and rooms43 / burrow43 forced on: E1's
//       and A1's rules where they have landed; --b43: boost43 / chimney43 / ride43 too): per room cleared k of n, the median, deaths, falls;
//       the boost chain's lines (kiln43) of the first seed, and kiln40's (must be none).
//   n43kiln gates [--b43]
//       the gate checker (`n38 gates Design/levels/n43/kiln.plan.json --ride40`) with the design's NIGHT43 movement rows forced on (--b43:
//       boost43, chimney43, ride43) through a scratch copy of the design file (Harness.Root points at it for the run): the gates and THE STEPS
//       must stay GREEN with M1's rules on.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43Kiln(string cmd, string[] args, ref int code)
    {
        if (cmd != "n43kiln") return;
        string what = args.Length > 1 ? args[1] : "";
        if (what == "steps") { code = N43KSteps(args); return; }
        if (what == "trace") { code = N43KTrace(args); return; }
        if (what == "wyrm") { code = N43KWyrm(args); return; }
        if (what == "walk") { code = N43KWalk(args); return; }
        if (what == "gates") { code = N43KGates(args); return; }
        Console.WriteLine("n43kiln: steps [--write] | walk [--seeds n] [--first n] [--cap s] [--b43] [--trace] | gates [--b43]");
        code = 2;
    }

    const string N43KLevel = "Design/levels/n43-kiln.json";

    static string F43K(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.00", CultureInfo.InvariantCulture);

    /// <summary>The NIGHT43 blocks of a K1 run: the design's, kiln43 on, plus the named rows.</summary>
    static N43Blocks N43KBlocks(GameConfig cfg, params string[] on)
    {
        int bits = (N43Blocks.ForceOff ? N43Blocks.Off : N43Blocks.From(cfg)).Bits | (1 << Array.IndexOf(N43Blocks.Names, "kiln43"));
        foreach (string n in on) bits |= 1 << Array.IndexOf(N43Blocks.Names, n);
        return N43Blocks.FromBits(bits);
    }

    /// <summary>The room row (campaign.rooms[i] of the map JSON) with this id.</summary>
    static Dictionary<string, object> N43KRoomRow(Dictionary<string, object> map, string id)
    {
        foreach (object o in PortData.Arr(PortData.Obj(map["campaign"])["rooms"]))
            if (o is Dictionary<string, object> r && r.TryGetValue("id", out object v) && v as string == id) return r;
        return null;
    }

    // ================================================================================================ steps

    static readonly bool N43KStickLate = Environment.GetEnvironmentVariable("N43K_STICK") == "late";
    static bool N43KTraceTry; // n43kiln trace: one try's flight, every 0.05 s
    static int N43KShots; // THUNDERCLAP "fire" events seen (the trace)

    sealed class N43KTry
    {
        public bool landed;
        public double apex; // the feet's top over the take-off
    }

    /// <summary>One scripted try from `take` (feet) facing north: jump held `hold` s, the THUNDERCLAP tapped `shotAt` s after the jump press
    /// (negative: before it, on the ground; NaN: no shot), the double jump at the apex when `apexDouble`. Lands when grounded at y >= top - 0.3
    /// within 5 s of the jump.</summary>
    static N43KTry N43KRun(SurvivalSim sim, ScriptedInput inp, Vec3 take, double top, double hold, double shotAt, int mode)
    {
        double dt = sim.step;
        int S(double s) => MovementMath.StepsFor(s, dt);
        double pitchDown = -89 * Math.PI / 180;
        void Tick(InputFrame f, CombatInput c, double pitch)
        {
            inp.SetLook(0, pitch);
            inp.Set(f, c);
            sim.hitstopLeft = 0;
            sim.health.hp = sim.health.max;
            sim.mover.invulnerable = true;
            sim.Step();
        }
        // stand at the take-off with the THUNDERCLAP in hand, its interval and any charge cooldown run out (3.2 s), then 0.1 s still
        int slot = sim.loadout.owned.IndexOf("shotgun");
        sim.Respawn(take, 0);
        for (int k = 0; k < S(3.2); k++) Tick(new InputFrame(), new CombatInput { weapon = sim.loadout.Weapon != "shotgun" && k % 30 == 0 ? slot : -1 }, 0);
        sim.Respawn(take, 0);
        for (int k = 0; k < S(0.1); k++) Tick(new InputFrame(), new CombatInput { weapon = -1 }, 0);
        double y0 = sim.mover.position.y;
        var res = new N43KTry { apex = 0 };
        int lead = double.IsNaN(shotAt) || shotAt >= 0 ? 0 : S(-shotAt); // steps before the jump press (the early shot)
        int jumpStep = lead, holdSteps = Math.Max(1, S(hold));
        int shotStep = double.IsNaN(shotAt) ? -1 : (shotAt < 0 ? 0 : jumpStep + (shotAt <= 0 ? 0 : S(shotAt)));
        bool doubled = false, left = false, over = false;
        int total = jumpStep + S(5);
        for (int i = 0; i < total; i++)
        {
            // the stick: forward all along (a person pushing at the step), or (N43K_STICK=late, a probe) only once the rise has stopped
            var f = new InputFrame { moveZ = N43KStickLate && !(left && sim.mover.velocity.y <= 0) ? 0 : 1 };
            var c = new CombatInput { weapon = -1 };
            if (i == jumpStep) f.jump = new Button { held = true, pressed = true };
            else if (i > jumpStep && i < jumpStep + holdSteps && !((mode == 1 || mode == 2) && left && sim.mover.velocity.y <= 0)) f.jump = new Button { held = true }; // modes 1-2 let go at the apex for the double
            if (sim.mover.position.y > top + 0.5) over = true; // once over the top a person stops shooting
            if (i == shotStep || (!over && mode == 2 && doubled && i % 2 == 0) || (!over && mode == 3 && i > shotStep && shotStep >= 0 && left && i % 2 == 0)) c.fire = new Button { held = true, pressed = true }; // mode 2 (THE CLIMB): after the double, a fresh pull every other step (the gun fires at its interval)
            bool air = !sim.mover.grounded;
            if (i > jumpStep && air) left = true;
            if ((mode == 1 || mode == 2) && !doubled && left && i > shotStep && i > jumpStep + 2 && sim.mover.velocity.y <= 0)
            {
                f.jump = new Button { held = true, pressed = true };
                doubled = true;
            }
            double pitch = i >= Math.Max(0, Math.Min(jumpStep, shotStep < 0 ? jumpStep : shotStep) - 1) ? pitchDown : 0;
            Tick(f, c, pitch);
            res.apex = Math.Max(res.apex, sim.mover.position.y - y0);
            if (N43KTraceTry && i % 6 == 0) Console.WriteLine($"      t {(i - jumpStep) * dt:+0.00;-0.00} y {F43K(sim.mover.position.y)} vy {F43K(sim.mover.velocity.y)} z {F43K(sim.mover.position.z)} vz {F43K(sim.mover.velocity.z)} {sim.mover.state} g {sim.mover.grounded}{(i == shotStep ? " SHOT" : "")} w {sim.loadout.Weapon} shots {N43KShots}");
            if (left && i > jumpStep + 2 && sim.mover.grounded)
            {
                res.landed = sim.mover.position.y >= top - 0.3;
                break;
            }
        }
        return res;
    }

    static int N43KSteps(string[] args)
    {
        bool write = Array.IndexOf(args, "--write") >= 0;
        GameConfig cfg = Config();
        string path = Path.GetFullPath(N43KLevel);
        var json0 = PortData.Obj(PortData.LoadJson(path));
        var kb1 = N43KRoomRow(json0, "kb1");
        var tops = kb1 != null && kb1.TryGetValue("tops43", out object to) ? PortData.Obj(to) : null;
        if (tops == null) { Console.WriteLine("n43kiln steps: kb1 has no tops43 (regenerate n43-kiln)"); return 2; }
        var takes = PortData.Arr(tops["take"]).ConvertAll(o => PortData.V3(o));
        double[] topY = { PortData.Num(tops["s1"]), PortData.Num(tops["s2"]), PortData.Num(tops["s3"]) };
        double[] offsets = { -0.15, -0.10, -0.05, 0, 0.05, 0.10, 0.15, 0.20, 0.25, 0.30, 0.35, 0.40, 0.45 };
        double[] holds = { 0.10, 0.40, 1.0 };
        var heights = new Dictionary<string, Dictionary<string, double>>();
        foreach (bool b43 in new[] { false, true })
        {
            string side = b43 ? "on" : "off";
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.n43Override = b43 ? N43KBlocks(cfg, "boost43") : N43KBlocks(cfg);
            sim.StartRun(4301, null, null);
            var inp = (ScriptedInput)sim.input;
            for (int k = 0; k < 12; k++) { inp.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); } // the level's start set lands on the first steps (N31UnlocksStep)
            if (!sim.loadout.owned.Contains("shotgun")) { Console.WriteLine($"n43kiln steps: the run holds no THUNDERCLAP ({string.Join(",", sim.loadout.owned)})"); return 2; }
            Console.WriteLine($"n43kiln steps, boost43 {side}: level {sim.campaignLevel?.id} (N43BaseId -> {sim.N43BaseId(sim.campaignLevel?.id)}), kiln43 {sim.n43.kiln43}, boost43 {sim.n43.boost43}; owned {string.Join(",", sim.loadout.owned)}");
            var h = new Dictionary<string, double>();
            // the apexes over the take-off (step 1's take-off: the floor; the room's roof is far over it)
            h["jump"] = N43KRun(sim, inp, takes[0], 1e9, 3.0, double.NaN, 0).apex;
            h["double"] = N43KRun(sim, inp, takes[0], 1e9, 3.0, double.NaN, 1).apex;
            double best1 = 0, best2 = 0, best3 = 0, best4 = 0;
            foreach (double o in offsets)
            {
                best1 = Math.Max(best1, N43KRun(sim, inp, takes[0], 1e9, 3.0, o, 0).apex);
                best2 = Math.Max(best2, N43KRun(sim, inp, takes[0], 1e9, 3.0, o, 1).apex);
                best3 = Math.Max(best3, N43KRun(sim, inp, takes[0], 1e9, 3.0, o, 2).apex);
                best4 = Math.Max(best4, N43KRun(sim, inp, takes[0], 1e9, 3.0, o, 3).apex);
            }
            h["oneBoost"] = best1;
            h["boostDouble"] = best2;
            h["climb"] = best3;
            h["twoShots"] = best4;
            heights[side] = h;
            Console.WriteLine($"  apexes over the take-off (jump held through the flight: no jump cut): jump {F43K(h["jump"])}, jump + double {F43K(h["double"])}, best one boost {F43K(best1)}, best boost + double {F43K(best2)}, best climb (shot, double, shot) {F43K(best3)}, best two shots (no double) {F43K(best4)} m");
            string[] modeName = { "one boost     ", "boost + double", "the climb     ", "two shots     " };
            for (int si = 0; si < 3; si++)
            {
                double top = topY[si];
                for (int dbl = 0; dbl < 4; dbl++)
                {
                    var row = new StringBuilder();
                    int n = 0, ok = 0;
                    foreach (double hold in holds)
                    {
                        row.Append($" | hold {hold:0.00}:");
                        foreach (double o in offsets)
                        {
                            N43KTry t = N43KRun(sim, inp, takes[si], top, hold, o, dbl);
                            n++;
                            if (t.landed) ok++;
                            row.Append(t.landed ? " +" : " .");
                        }
                    }
                    Console.WriteLine($"  S{si + 1} (top {F43K(top)}, rise {F43K(top - takes[si].y)}) {modeName[dbl]}: lands {ok}/{n} ({100.0 * ok / n:0} %){row}");
                }
            }
        }
        Console.WriteLine("  (offsets, left to right: " + string.Join(" ", offsets.Select(o => o.ToString("+0.00;-0.00", CultureInfo.InvariantCulture))) + " s from the jump press; + lands, . falls short)");
        if (write)
        {
            string outPath = Path.GetFullPath("Design/levels/n43/kiln-boost.json");
            var sb = new StringBuilder();
            sb.Append("{\n  \"_why\": \"NIGHT43 K1: `n43kiln steps --write` (Tools/SimHarness/Harness.N43.Kiln.cs): the apexes over a standing take-off on n43-kiln's floor, boost43 off and forced on (the real sim: jump held 1.0 s (no jump cut), the THUNDERCLAP tapped straight down at -0.15..+0.45 s from the jump press, the best of them; the double jump at the apex). Tools/night/gen-n43-kiln.cjs cuts THE STEPS from both (a lower bound takes the larger, an upper bound the smaller); climb = shot, the double jump at the apex, then the gun again as soon as it fires.\",\n");
            sb.Append($"  \"measured\": \"{DateTime.Now:yyyy-MM-dd HH:mm}\",\n");
            int k = 0;
            foreach (var kv in heights)
            {
                sb.Append($"  \"{kv.Key}\": {{ ");
                sb.Append(string.Join(", ", kv.Value.Select(p => $"\"{p.Key}\": {Math.Round(p.Value, 3).ToString(CultureInfo.InvariantCulture)}")));
                sb.Append(k++ < heights.Count - 1 ? " },\n" : " }\n");
            }
            sb.Append("}\n");
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            File.WriteAllText(outPath, sb.ToString(), new UTF8Encoding(false));
            Console.WriteLine("  wrote " + outPath);
        }
        return 0;
    }

    /// <summary>n43kiln trace &lt;step 1-3&gt; &lt;hold s&gt; &lt;shot offset s&gt; &lt;mode 0-2&gt; [--b43]: one scripted try's flight printed every 0.05 s.</summary>
    static int N43KTrace(string[] args)
    {
        int si = int.Parse(args[2], CultureInfo.InvariantCulture) - 1;
        double hold = double.Parse(args[3], CultureInfo.InvariantCulture), off = double.Parse(args[4], CultureInfo.InvariantCulture);
        int mode = int.Parse(args[5], CultureInfo.InvariantCulture);
        bool b43 = Array.IndexOf(args, "--b43") >= 0;
        GameConfig cfg = Config();
        string path = Path.GetFullPath(N43KLevel);
        var tops = PortData.Obj(N43KRoomRow(PortData.Obj(PortData.LoadJson(path)), "kb1")["tops43"]);
        var takes = PortData.Arr(tops["take"]).ConvertAll(o => PortData.V3(o));
        double top = PortData.Num(tops["s" + (si + 1)]);
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n43Override = b43 ? N43KBlocks(cfg, "boost43") : N43KBlocks(cfg);
        sim.StartRun(4301, null, null);
        sim.events.On<string>("fire", w => { if (w == "shotgun") N43KShots++; });
        var inp = (ScriptedInput)sim.input;
        for (int k = 0; k < 12; k++) { inp.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
        N43KTraceTry = true;
        N43KTry t = N43KRun(sim, inp, takes[si], top, hold, off, mode);
        N43KTraceTry = false;
        Console.WriteLine($"n43kiln trace S{si + 1} hold {hold} shot {off:+0.00;-0.00} mode {mode}: {(t.landed ? "LANDS" : "short")}, apex {F43K(t.apex)} m over the take-off (top {F43K(top - takes[si].y)} over it)");
        return 0;
    }

    /// <summary>n43kiln wyrm [--seeds n] [--cap s]: THE KILN WYRM's fight on n43-kiln's k5 (B1's n43wick wyrm setup: DOORFREE, the player put at
    /// k5's spawn, WyrmPilot31 fights, a death respawns at the boss room), kiln43 on: killed or not, the time, the deaths, the arena file the
    /// level names. Before / after THE RING's arena change.</summary>
    static int N43KWyrm(string[] args)
    {
        int seeds = 3;
        double cap = 300;
        for (int i = 2; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
        }
        GameConfig cfg = Config();
        string path = Path.GetFullPath(N43KLevel);
        var k5row = N43KRoomRow(PortData.Obj(PortData.LoadJson(path)), "k5");
        string arena = k5row != null && k5row.TryGetValue("wyrmArena", out object wa) ? wa as string : "(none)";
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        int killed = 0;
        var times = new List<double>();
        try
        {
            for (int s = 0; s < seeds; s++)
            {
                SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
                sim.noLab = sim.noRecords = true;
                sim.n43Override = N43KBlocks(cfg);
                sim.setup.mode = Campaign24.Mode;
                sim.StartRun((uint)(1 + s), null, null);
                sim.husks.frozen = true;
                for (int i = 0; i < 10; i++) sim.Step();
                sim.campaign.SetDoorFree29(true);
                RoomDef k5 = sim.campaignLevel.rooms.Find(r => r.id == "k5");
                sim.Respawn(k5.spawn, k5.spawnYawDeg);
                foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
                int wait = 0;
                while ((sim.wyrm24 == null || !sim.wyrm24.Alive) && wait++ < 600) sim.Step();
                if (sim.wyrm24 == null || !sim.wyrm24.Alive) { Console.WriteLine($"n43kiln wyrm seed {s + 1}: NO FIGHT (arena {arena})"); continue; }
                Wyrm w = sim.wyrm24;
                var si = (ScriptedInput)sim.input;
                var wp = new WyrmPilot31();
                int step = 0, capSteps = (int)(cap * 120), deaths = 0;
                Vec3 retry = k5.spawn;
                while (w.Alive && step < capSteps)
                {
                    wp.Step(sim, step);
                    si.SetLook(wp.Move.yaw, wp.Move.pitch);
                    si.Set(wp.Move, wp.Combat);
                    sim.Step();
                    sim.hitstopLeft = 0;
                    step++;
                    if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead) { deaths++; sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; sim.health.Reset(); if (sim.BossRoomBegin("wyrm", out Vec3 sp, out _)) retry = sp; sim.Respawn(retry, 0); }
                }
                if (!w.Alive) { killed++; times.Add(step / 120.0); }
                Console.WriteLine($"n43kiln wyrm seed {s + 1}: THE KILN WYRM {(w.Alive ? "ALIVE" : "killed")} in {step / 120.0:0.0} s, deaths {deaths}, phase {w.phase}");
            }
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
        times.Sort();
        Console.WriteLine($"n43kiln wyrm SUMMARY (arena {arena}): killed {killed}/{seeds}, median {(times.Count > 0 ? times[times.Count / 2].ToString("0.0", CultureInfo.InvariantCulture) : "-")} s");
        return killed == seeds ? 0 : 1;
    }

    // ================================================================================================ walk

    static int N43KWalk(string[] args)
    {
        int seeds = 10, first = 1;
        double cap = 900;
        bool b43 = Array.IndexOf(args, "--b43") >= 0, trace = Array.IndexOf(args, "--trace") >= 0;
        for (int i = 2; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--first") first = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
        }
        GameConfig cfg = Config();
        string path = Path.GetFullPath(N43KLevel);
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var times = new List<double>[playable];
        var deaths = new int[playable];
        var falls = new int[playable];
        for (int q = 0; q < playable; q++) times[q] = new List<double>();
        int done = 0;
        var extra = b43 ? new[] { "rooms43", "burrow43", "boost43", "chimney43", "ride43" } : new[] { "rooms43", "burrow43" };
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.n43Override = N43KBlocks(cfg, extra);
            var ap = new AutopilotInput(sim, "normal", seed);
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
            int st = 0;
            var enter = new Dictionary<int, int>();
            sim.events.On<RoomEvent>("roomEnter", e => { if (!enter.ContainsKey(e.room)) enter[e.room] = st; });
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) times[e.room].Add(enter.TryGetValue(e.room, out int es) ? (st - es) * sim.step : e.time); });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) deaths[e.room]++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) falls[e.room]++; });
            if (trace) sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} cleared at run {st * sim.step:0.0} s"));
            sim.StartRun(seed, null, null);
            if (s == 0) Console.WriteLine($"n43kiln walk: the campaign loaded {sim.campaignLevel?.id} (N43BaseId -> {sim.N43BaseId(sim.campaignLevel?.id)}, N41BaseId -> {sim.N41BaseId(sim.campaignLevel?.id)}); NIGHT43 on: {string.Join(" ", N43Blocks.Names.Where((n, i) => sim.n43.Flag(i)))}");
            int max = MovementMath.StepsFor(cap, sim.step);
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            }
            bool ok = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (ok) done++;
            else
            {
                Vec3 p = sim.mover.position;
                Campaign24 c = sim.campaign;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, unsticks {pilot.unsticks}");
            }
            if (s == 0)
            {
                Console.WriteLine($"  seed {seed}: the kiln43 chain: {(sim.kiln43Shown.Count == 0 ? "none" : string.Join(", ", sim.kiln43Shown))}");
                Console.WriteLine($"  seed {seed}: kiln40's chain (must be none with kiln43 on): {(sim.kiln40Shown.Count == 0 ? "none" : string.Join(", ", sim.kiln40Shown))}");
            }
            ap.Detach();
        }
        for (int q = 0; q < playable; q++)
        {
            var l = times[q];
            l.Sort();
            string med = l.Count == 0 ? "-" : l[l.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            Console.WriteLine($"n43kiln walk {level.rooms[q].id} {level.rooms[q].name}: cleared {l.Count}/{seeds}, median {med} s, deaths {deaths[q]}, falls {falls[q]}");
        }
        Console.WriteLine($"n43kiln walk SUMMARY: all {playable} playable rooms cleared in {done} of {seeds} runs{(b43 ? " (boost43 / chimney43 / ride43 on)" : "")}");
        return done == seeds ? 0 : 1;
    }

    // ================================================================================================ gates with NIGHT43's movement rows on

    static int N43KGates(string[] args)
    {
        bool b43 = Array.IndexOf(args, "--b43") >= 0;
        string plan = "Design/levels/n43/kiln.plan.json";
        var rest = new List<string> { "n38", "gates", plan, "--ride40" };
        if (!b43) { int c0 = 2; N38Gates("n38", rest.ToArray(), ref c0); return c0; }
        // a scratch root: PortExport/config.json + maps/survival.json copied, Design/unity-config.json with the rows' `enabled` set true
        string scratch = Path.Combine(Path.GetTempPath(), "n43kiln-gates-" + Environment.ProcessId);
        string pe = Path.Combine(scratch, "PortExport");
        Directory.CreateDirectory(Path.Combine(pe, "maps"));
        Directory.CreateDirectory(Path.Combine(scratch, "Design"));
        File.Copy(Path.Combine(Root, "config.json"), Path.Combine(pe, "config.json"), true);
        File.Copy(Path.Combine(Root, "maps", "survival.json"), Path.Combine(pe, "maps", "survival.json"), true);
        string design = File.ReadAllText(GameConfig.DesignPath(Root));
        foreach (string row in new[] { "boost43", "chimney43", "ride43" })
        {
            int at = design.IndexOf("\"" + row + "\": {", StringComparison.Ordinal);
            int en = at < 0 ? -1 : design.IndexOf("\"enabled\": false", at, StringComparison.Ordinal);
            if (at < 0 || en < 0) { Console.WriteLine($"n43kiln gates: design.n43.{row} has no `enabled: false` to force"); continue; }
            design = design.Substring(0, en) + "\"enabled\": true" + design.Substring(en + "\"enabled\": false".Length);
        }
        File.WriteAllText(Path.Combine(scratch, "Design", "unity-config.json"), design, new UTF8Encoding(false));
        string old = Root;
        int code = 2;
        try
        {
            Root = pe;
            n42GatesCfg = null;
            GameConfig cfg = Config();
            Console.WriteLine($"n43kiln gates: NIGHT43 rows forced on in a scratch design ({scratch}): {string.Join(" ", N43Blocks.Names.Where((n, i) => N43Blocks.From(cfg).Flag(i)))}");
            N38Gates("n38", rest.ToArray(), ref code);
        }
        finally
        {
            Root = old;
            n42GatesCfg = null;
            try { Directory.Delete(scratch, true); } catch (IOException) { }
        }
        return code;
    }
}
