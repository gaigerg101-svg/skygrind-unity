// NIGHT39 builder V (the NIGHT38 views and the two carried tests, C1 rows 8 and 9). One entry, N39Views (declared and called in
// Program.cs). Nothing here changes the sim.
//   n39views maptime [--reps n]
//     MapChoiceTests.EveryListedMap_LoadsAndPlays600Steps, timed per map: the same list (design.maps.list, files that exist, the
//     arena first when missing: SurvivalGame.MapList's rule, copied because SurvivalGame is not in the harness), the same body per map
//     (load + FromExport, Headless, noLab, metaSilent, StartRun(77), 600 steps, flow Playing). Prints load / compose / run ms per map,
//     the slowest maps and the total on CoreCLR. Unity's EditMode run is Mono: compare the total with the suite's measured duration
//     (Logs/n3x-freeze-tests.xml) for the Mono factor.
//   C1 row 8 (AutopilotTests.PerStep_AllocatesNothing) uses the existing `alloc` command (Harness.N22.Alloc.cs): run it plain, with
//     --off38, and at u3-n37 (a git archive of e6902b00) to compare the window's run.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N39Views(string cmd, string[] args, ref int code)
    {
        if (cmd != "n39views") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "maptime";
        if (what == "maptime") code = N39MapTime(int.Parse(StampArg(args, "--reps", "1")));
        else if (what == "wall")
        {
            var ids = new List<string>();
            for (int i = 2; i < args.Length; i++) if (!args[i].StartsWith("--")) ids.Add(args[i]);
            if (ids.Count == 0) ids.AddRange(new[] { "n38-span", "n38-foundry", "n38-spire" });
            int red = 0;
            bool old = Array.IndexOf(args, "--old") >= 0; // --old: ride NIGHT38's plain pick instead (the before)
            foreach (string id in ids) red += N39Wall(id, old);
            Console.WriteLine($"n39views wall: {ids.Count - red}/{ids.Count} levels with rows 9 and 11 PASS on the picked wall");
            code = red == 0 ? 0 : 1;
        }
        else if (what == "hurt") code = N39Hurt();
        else
        {
            Console.WriteLine($"n39views: unknown '{what}' (maptime | wall [level ids] | hurt)");
            code = 2;
        }
    }

    /// <summary>`n39views wall [ids]`: the verbs reel's wall for rows 9-11 (Bot/VerbsWall39.Pick, the reel's own code) on each campaign level: NIGHT38's
    /// plain pick and tonight's (noride faces and floorless lanes end the run), the solid behind the new start, then rows 9 and 11 re-performed as the reel
    /// does them (FeelReel.N35.Verbs.cs: 0.7 m off, 11 m/s along with a 0.5 m/s lean, jump pressed and held, stick forward and half into the wall,
    /// let go (len - 3) / 11.7 s after the first start, at least 1.3 s) and row 10 (the wall jump from 1.2 m). Row 9 PASS: a ride, the longest single
    /// ride over 1.2 s (and rising at most 1.5 m with wallgrip36); row 11 PASS: faster in the 0.25 s after letting go than the frame before.</summary>
    static int N39Wall(string id, bool rideOld)
    {
        GameConfig cfg = Config();
        // NIGHT66 T2: an id ending .json is a level file's path (a round's or a scratch assembly's level: gen-n65-foundry.cjs --out)
        string path = Path.GetFullPath(id.EndsWith(".json", StringComparison.Ordinal) ? id : Path.Combine("Design", "levels", id + ".json"));
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var si = new ScriptedInput();
        sim.UseInput(si);
        sim.StartRun(1, null, null);
        var p = new Probe37 { sim = sim, si = si };
        Mover m = sim.mover;
        var log = new List<(double t, MoverEvent e)>();
        m.Events += e => { p.mv[e.type] = p.M(e.type) + 1; log.Add((sim.simStep * sim.step, e)); };
        sim.husks.KillAll();
        sim.husks.frozen = true;
        int nr = m.flow34.ride35.noRide != null ? m.flow34.ride35.noRide.Length : 0;
        bool oldOk = VerbsWall39.Pick(sim, false, false, out Vec3 oS, out Vec3 oA, out Vec3 oN, out double oL, out string oR);
        bool ok = VerbsWall39.Pick(sim, false, true, out Vec3 wStart, out Vec3 wAlong, out Vec3 wNormal, out double wLen, out string wRoom);
        Console.WriteLine($"{id}: level {sim.campaignLevel?.id}, noride boxes on the Mover {nr}, wallride35 {sim.n35.wallride35}, wallgrip36 {sim.n36.wallgrip36}");
        Console.WriteLine($"  NIGHT38 pick: {(oldOk ? $"{oL} m in {oR} at ({oS.x:0.0}, {oS.y:0.0}, {oS.z:0.0}) along ({oA.x:0}, {oA.z:0})" : "none >= 8 m")}");
        if (!ok) { Console.WriteLine("  NIGHT39 pick: none >= 8 m: rows 9-11 RED"); return 1; }
        bool same = oldOk && oR == wRoom && oL == wLen && (oS - wStart).Length < 1e-6;
        Console.WriteLine($"  NIGHT39 pick: {wLen} m in {wRoom} at ({wStart.x:0.0}, {wStart.y:0.0}, {wStart.z:0.0}) along ({wAlong.x:0}, {wAlong.z:0}){(same ? " (unchanged)" : " (CHANGED)")}");
        if (rideOld)
        {
            if (!oldOk) return 1;
            wStart = oS; wAlong = oA; wNormal = oN; wLen = oL;
            Console.WriteLine("  --old: riding the NIGHT38 pick");
        }
        Vec3 face = new Vec3(wStart.x - wNormal.x * 0.75, wStart.y + 1.0, wStart.z - wNormal.z * 0.75);
        Console.WriteLine($"  the solid at the start:{N39BoxesAt(map, face, 0.06)}");
        // ---- rows 9 / 11 as the reel ----
        double ry = PYaw(wAlong);
        Vec3 rightV = MovementMath.WishDirection(1, 0, ry);
        double into = rightV.x * wNormal.x + rightV.z * wNormal.z > 0 ? -1 : 1;
        p.Reset(wStart, ry);
        int r0 = p.M(MoverEventType.WallRideStart);
        log.Clear();
        double t0 = sim.simStep * sim.step, rideHold = Math.Max(1.3, (wLen - 3) / 11.7);
        double startT = double.NaN, endT = double.NaN, releaseT = double.NaN, attachY = double.NaN, rise = 0, offSpeed = 0, preSpeed = 0, releasedAt = -1;
        bool released = false;
        var rides = new List<(double st, double en)>();
        var reasons = new List<string>();
        int n = MovementMath.StepsFor(rideHold + 1.2, sim.step);
        for (int i = 0; i < n; i++)
        {
            double t = i * sim.step, now = sim.simStep * sim.step;
            if (t < 0.05) m.velocity = new Vec3(wAlong.x * 11 - wNormal.x * 0.5, m.velocity.y, wAlong.z * 11 - wNormal.z * 0.5);
            if (m.state == MoveState.WallRide && !double.IsNaN(attachY)) rise = Math.Max(rise, m.position.y - attachY);
            double onWall = double.IsNaN(startT) ? t : now - startT;
            bool off = !double.IsNaN(endT) && now - endT > 0.15;
            bool hold = !released && onWall < rideHold && !off;
            if (!hold && !released) { released = true; releasedAt = t; releaseT = now; preSpeed = m.velocity.Length; }
            si.SetLook(ry, 0);
            si.Set(new InputFrame { moveZ = 1, moveX = released ? 0 : into * 0.5, jump = new Button { pressed = t < 0.02, held = hold } }, new CombatInput { weapon = -1 });
            if (released && t - releasedAt > 0.02 && t - releasedAt < 0.25) offSpeed = Math.Max(offSpeed, m.velocity.Length);
            int c0 = log.Count;
            m.invulnerable = true;
            sim.Step();
            for (int k = c0; k < log.Count; k++)
            {
                var (et, e) = log[k];
                if (e.type == MoverEventType.WallRideStart)
                {
                    if (double.IsNaN(startT)) startT = et;
                    attachY = m.position.y;
                    rides.Add((et, double.NaN));
                }
                if (e.type == MoverEventType.WallRideEnd)
                {
                    endT = et;
                    reasons.Add(e.reason);
                    if (rides.Count > 0 && double.IsNaN(rides[rides.Count - 1].en)) rides[rides.Count - 1] = (rides[rides.Count - 1].st, et);
                }
            }
        }
        double nowEnd = sim.simStep * sim.step, rideS = 0;
        var spans = new List<string>();
        foreach (var (st, en) in rides)
        {
            double e = !double.IsNaN(en) ? en : !double.IsNaN(releaseT) && releaseT > st ? releaseT : nowEnd;
            rideS = Math.Max(rideS, e - st);
            spans.Add($"{st - t0:0.00}-{(double.IsNaN(en) ? "on" : (en - t0).ToString("0.00"))}");
        }
        bool rode = p.M(MoverEventType.WallRideStart) > r0;
        bool grip = sim.n36.wallgrip36;
        bool row9 = sim.n35.wallride35 && rode && rideS > 1.2 && (!grip || rise <= 1.5);
        bool row11 = sim.n35.wallride35 && rode && offSpeed > preSpeed;
        Console.WriteLine($"  row 9 {(row9 ? "PASS" : "RED")}: rides {rides.Count} [{string.Join(", ", spans)}], longest {rideS:0.00} s, ends [{string.Join(", ", reasons)}], rose {rise:0.00} m");
        Console.WriteLine($"  row 11 {(row11 ? "PASS" : "RED")}: {offSpeed:0.00} m/s after letting go vs {preSpeed:0.00} m/s before");
        // ---- row 10: face the wall from 1.2 m, jump, jump again ----
        var near = new Vec3(wStart.x + wAlong.x * 2 + wNormal.x * 0.5, wStart.y, wStart.z + wAlong.z * 2 + wNormal.z * 0.5);
        p.Reset(near, PYaw(new Vec3(-wNormal.x, 0, -wNormal.z)));
        int wj0 = p.M(MoverEventType.WallJump);
        p.Run(1.0, t => si.Set(new InputFrame { moveZ = 1, jump = new Button { pressed = t < 0.02 || (t >= 0.3 && t < 0.32), held = t < 0.15 || (t >= 0.3 && t < 0.45) } }, new CombatInput { weapon = -1 }));
        Console.WriteLine($"  row 10 {(p.M(MoverEventType.WallJump) > wj0 ? "PASS" : "RED")}: WallJump events {p.M(MoverEventType.WallJump) - wj0}");
        return row9 && row11 ? 0 : 1;
    }

    /// <summary>`n39views hurt`: juice38's burst-or-tick rule (Game/Juice39Hurt.cs, the view's own code) on hurt streams. (1) REAL: the player stood 3 s
    /// in n38-foundry's first Lava30 pool (god mode as the reels: max 1e6, refilled each step), every player `hurt` (time, source, amount) fed to the
    /// rule. (2) The streams the sim's tick code makes (its cadence, read from the code): THE LANTERN's / hazard27's floor ("hazard", 0.25 s, the
    /// n39-r1 f0900 case), THE WICK's hot wall ("boss", 0.5 s cooldown), spitter acid ("acid"). (3) Real hits kept: husk hits 2 s apart, and two 0.4 s
    /// apart. PASS: every 3 s stand at most one full burst; hits 2 s apart all bursts.</summary>
    static int N39Hurt()
    {
        int fails = 0;
        // (1) Lava30, real
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(Path.Combine("Design", "levels", "n38-foundry.json")))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var si = new ScriptedInput();
        sim.UseInput(si);
        sim.StartRun(1, null, null);
        sim.husks.KillAll();
        sim.husks.frozen = true;
        if (sim.lava30 == null || sim.lava30.zones.Count == 0) { Console.WriteLine("  lava: n38-foundry has no Lava30 zone live (n30.lava off?): RED"); return 1; }
        Lava30Zone z = sim.lava30.zones[0];
        sim.health.max = sim.health.hp = 1e6;
        sim.Respawn(new Vec3((z.x0 + z.x1) / 2, z.y + 0.05, (z.z0 + z.z1) / 2), 0);
        var real = new List<(double t, string src, double amt)>();
        double t0 = sim.simStep * sim.step;
        sim.events.On<HurtEvent>("hurt", e => { if (e != null && e.owner == "player") real.Add((sim.simStep * sim.step - t0, e.source, e.amount)); });
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        int n = MovementMath.StepsFor(3.0, sim.step);
        for (int i = 0; i < n; i++)
        {
            sim.health.hp = sim.health.max;
            sim.Step();
        }
        var lava = new Juice39Hurt();
        foreach (var (t, src, _) in real) lava.Hit(t, src);
        Console.WriteLine($"  lava (real, n38-foundry room {z.room}, 3 s): {real.Count} hurts at [{string.Join(", ", real.ConvertAll(h => $"{h.t:0.00} s {h.src}"))}] -> bursts {lava.Bursts}, ticks {lava.Ticks} {(lava.Bursts <= 1 ? "PASS" : "RED")}");
        if (lava.Bursts > 1 || real.Count == 0) fails++;
        // (2) the tick streams, 3 s each
        foreach (var (name, src, every) in new[] { ("hazard floor (THE LANTERN, hazard27; f0900)", "hazard", 0.25), ("THE WICK's hot wall", "boss", 0.5), ("spitter acid", "acid", 0.25) })
        {
            var r = new Juice39Hurt();
            int k = 0;
            for (double t = 0; t < 3.0 - 1e-9; t += every, k++) r.Hit(t, src);
            bool ok = r.Bursts <= 1;
            Console.WriteLine($"  {name}: {k} hurts every {every} s -> bursts {r.Bursts}, ticks {r.Ticks} {(ok ? "PASS" : "RED")}");
            if (!ok) fails++;
        }
        // (3) real hits keep their burst
        var apart = new Juice39Hurt();
        for (int i = 0; i < 3; i++) apart.Hit(i * 2.0, "husk");
        var close = new Juice39Hurt();
        close.Hit(0, "husk");
        close.Hit(0.4, "husk");
        bool okApart = apart.Bursts == 3;
        Console.WriteLine($"  husk hits 2 s apart: bursts {apart.Bursts}/3 {(okApart ? "PASS" : "RED")}; two hits 0.4 s apart: bursts {close.Bursts}, ticks {close.Ticks} (the first burst still on screen)");
        if (!okApart) fails++;
        Console.WriteLine($"n39views hurt: {(fails == 0 ? "PASS" : $"RED ({fails})")}");
        return fails == 0 ? 0 : 1;
    }

    sealed class N39MapRow
    {
        public string id, file;
        public double load, compose, run;
        public bool playing;
        public double Total => load + compose + run;
    }

    /// <summary>SurvivalGame.MapList's rule (SurvivalGame.Maps.cs), without the label lookups the test does not time.</summary>
    static List<(string id, string file)> N39MapList(GameConfig cfg)
    {
        var list = new List<(string id, string file)>();
        string baseDir = Path.GetDirectoryName(Root) ?? ".";
        if (cfg.Has("design.maps.list"))
            foreach (object o in cfg.Arr("design.maps.list"))
            {
                var d = PortData.Obj(o);
                string file = Path.Combine(baseDir, (string)d["file"]);
                if (File.Exists(file)) list.Add(((string)d["id"], file));
            }
        if (list.Count == 0 || list.FindIndex(m => m.id == RunSetup.DefaultMap) < 0)
            list.Insert(0, (RunSetup.DefaultMap, Path.Combine(Root, "maps", "survival.json")));
        return list;
    }

    static int N39MapTime(int reps)
    {
        GameConfig cfg = Config();
        List<(string id, string file)> maps = N39MapList(cfg);
        int fails = 0;
        var all = new Stopwatch();
        List<N39MapRow> rows = null;
        for (int r = 0; r < reps; r++)
        {
            rows = new List<N39MapRow>();
            all.Restart();
            foreach ((string id, string file) in maps)
            {
                var row = new N39MapRow { id = id, file = file };
                var t = Stopwatch.StartNew();
                SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(file)));
                row.load = t.Elapsed.TotalMilliseconds;
                t.Restart();
                SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                sim.noLab = true;
                sim.metaSilent = true;
                row.compose = t.Elapsed.TotalMilliseconds;
                t.Restart();
                sim.StartRun(77u, null, null);
                for (int i = 0; i < 600; i++) sim.Step();
                row.run = t.Elapsed.TotalMilliseconds;
                row.playing = sim.flow == Skygrind.Game.Flow.Playing && map.name == id;
                if (!row.playing) fails++;
                rows.Add(row);
            }
            all.Stop();
            Console.WriteLine($"maptime rep {r + 1}/{reps}: {rows.Count} maps in {all.Elapsed.TotalSeconds:0.00} s (CoreCLR, Release)");
        }
        Console.WriteLine("  map            load ms  compose ms   run ms   total ms  playing");
        double load = 0, compose = 0, run = 0;
        foreach (N39MapRow row in rows)
        {
            Console.WriteLine($"  {row.id,-14} {row.load,7:0} {row.compose,11:0} {row.run,8:0} {row.Total,10:0}  {(row.playing ? "yes" : "NO")}");
            load += row.load;
            compose += row.compose;
            run += row.run;
        }
        rows.Sort((a, b) => b.Total.CompareTo(a.Total));
        Console.Write("  slowest:");
        for (int i = 0; i < Math.Min(5, rows.Count); i++) Console.Write($" {rows[i].id} {rows[i].Total / 1000:0.00} s;");
        Console.WriteLine();
        Console.WriteLine($"maptime: {rows.Count} maps, load {load / 1000:0.00} s + compose {compose / 1000:0.00} s + run {run / 1000:0.00} s = {(load + compose + run) / 1000:0.00} s on CoreCLR; {fails} not playing");
        return fails == 0 ? 0 : 1;
    }
}
