// NIGHT40 builder B5 (crown40; NIGHT40.md B5): THE CROWN, headless. One entry, N40Crown (declared and called in Program.cs).
//   n40crown [defects|line|all] [--seeds N] [--cap S] [--level n37|n38|n40]
//     defects  THE CROWN's fight with BOTH pilots (Crown37Pilot skilled, and casual: L-058) on a level (default n38: level 5 as GG plays it;
//              n40: n40-spire with crown40 on), seeds 1..N, and per run the defects GG can hit, measured:
//                snap    the anti-hide teleport (Crown37.cs:407 `centre = HoverPoint(inp)`): steps where the ring's centre moved more than
//                        1.5x moveSpeed x dt; how many, the largest jump (m)
//                core    steps where a point of the ring's rim (16 samples) is inside the spire's core column (the frame f0500: the ring
//                        through the core's wall): no clamp keeps the ring out of the core
//                outside steps where the ring's centre is outside room v11's rect (no clamp to the arena, boss_rules 4)
//                close   steps where the player's chest is within the ring's radius of its centre while not passing (the ring fills the
//                        screen: frames f0700, f1000)
//              plus the kill time, passes, deaths, falls, hits taken; then the band per pilot (p50 / p90, killed n/N).
//     line     crown40's entry line on n40-spire: with the row on, the "bossLine40" lines (text, the step after the begin, the re-show after a
//              fall) and the widget's lessonBegin / lessonClear; with the row off: 0 lines (NIGHT39's fight).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N40Crown(string cmd, string[] args, ref int code)
    {
        if (cmd != "n40crown") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 240);
        int li = Array.IndexOf(args, "--level");
        string level = li >= 0 && li + 1 < args.Length ? args[li + 1] : "n38";
        Trace40C = Array.IndexOf(args, "--trace") >= 0;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] != "--set40") continue;
            string[] kv = args[i + 1].Split('=');
            Crown40Params.Probe ??= new Dictionary<string, double>();
            Crown40Params.Probe[kv[0]] = double.Parse(kv[1], CultureInfo.InvariantCulture);
            Console.WriteLine($"probe: crown40 {kv[0]} = {kv[1]}");
        }
        int fails = 0;
        if (what == "defects" || what == "all") fails += Defects40C(level, seeds, cap);
        if (what == "line" || what == "all") fails += Line40C();
        Console.WriteLine(fails == 0 ? "n40crown: GREEN" : $"n40crown: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static bool Trace40C;

    static string Path40C(string level) => level == "n37" ? "Design/levels/n37-spire.json" : level == "n40" ? "Design/levels/n40-spire.json" : "Design/levels/n38-spire.json";

    static SurvivalSim Sim40C(string level, uint seed, bool crown40)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path40C(level))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.n37Override = new N37Blocks { crown37 = true, boss37 = true };
        sim.n40Override = crown40 ? new N40Blocks { crown40 = true } : N40Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    /// <summary>Room v11's rect [x0, z0, x1, z1] from the level file.</summary>
    static double[] V11Rect40(string level)
    {
        var j = PortData.Obj(PortData.LoadJson(Path40C(level)));
        foreach (object o in PortData.Arr(PortData.Obj(j["campaign"])["rooms"]))
        {
            var r = PortData.Obj(o);
            if (r.TryGetValue("id", out object id) && (id as string) == "v11")
            {
                var a = PortData.Arr(r["rect"]);
                return new[] { PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]), PortData.Num(a[3]) };
            }
        }
        return null;
    }

    sealed class Run40C
    {
        public bool killed;
        public double seconds, snapMax, closeMin = 1e9;
        public int snaps, coreSteps, outsideSteps, closeSteps, fightSteps, passes, deaths, falls, bladeHits, shardHits, lowHits, parries, blocked, launches, missed;
        public double firstSnapAt = -1, firstCoreAt = -1, firstOutsideAt = -1;
        /// <summary>Why each fall happened: the last piece stood on (kind, its state and cause at the fall) or "no piece" / "reveal".</summary>
        public readonly Dictionary<string, int> fallWhy = new Dictionary<string, int>();
    }

    static Run40C Fight40C(string level, uint seed, double cap, bool casual, bool crown40)
    {
        var r = new Run40C();
        SurvivalSim sim = Sim40C(level, seed, crown40);
        var si = (ScriptedInput)sim.input;
        sim.Respawn(new Vec3(-10, 0.02, -267), 0);
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        sim.Step();
        if (!sim.BossRoomBegin("crown37", out Vec3 spawn, out double yaw)) return r;
        Crown37 c = sim.crown37;
        double dt = sim.step;
        double[] rect = V11Rect40(level);
        double coreH = c.orbitCoreHalf, coreTop = c.axis.y, R = c.p.ringRadius;
        var pilot = new Crown37Pilot { hand = new Mulberry32(seed * 7919u + 37u), casual = casual };
        int step = 0, revealSteps = MovementMath.StepsFor(c.p.revealSeconds, dt), capSteps = revealSteps + (int)(cap / dt);
        double yawL = 0, snapLimit = c.p.moveSpeed * dt * 1.5;
        int lastStand = -1, airSince = 0;
        string leftWhy = "no piece";
        sim.events.On<Crown37Event>("crown37Fall", e =>
        {
            if (sim.crown37.brain.state == Boss37State.LegChange) return;
            string why;
            if (c.brain.state == Boss37State.Reveal) why = "reveal";
            else why = leftWhy;
            if (Trace40C) Console.WriteLine($"      fall {F37(step * dt)} s: leg {c.Leg} {c.brain.state}, feet ({F37(sim.mover.position.x)}, {F37(sim.mover.position.y)}, {F37(sim.mover.position.z)}), airborne {F37(airSince * dt)} s, falls {sim.crown37Falls}, retries {sim.crown37Retries}, hp {F37(sim.health.hp)}: {why}");
            r.fallWhy[why] = (r.fallWhy.TryGetValue(why, out int n) ? n : 0) + 1;
        });
        while (c.Alive && step < capSteps)
        {
            int nowStand = sim.orbit37.standing;
            if (nowStand < 0 && lastStand >= 0 && airSince == 0)
            {
                // the step he left his piece: dropped under him (DOWN, its cause) or left while it stood (a jump: a missed landing if he falls)
                Piece37 q = sim.orbit37.pieces[lastStand];
                string kind = q.kind.ToString().ToLowerInvariant();
                leftWhy = q.state == Piece37State.Down ? $"{kind} dropped under him ({q.cause ?? "-"})" : $"left a {kind} standing (a jump: the landing missed)";
            }
            if (nowStand >= 0) { lastStand = nowStand; airSince = 0; }
            else if (sim.mover.grounded) { lastStand = -1; airSince = 0; leftWhy = "left static ground (the core's top, a ledge, a checkpoint)"; }
            else airSince++;
            pilot.Step(sim, yawL, step);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            Vec3 before = c.centre;
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (!c.Alive || c.Dying) break;
            if (c.brain.state == Boss37State.Reveal) continue;
            r.fightSteps++;
            double t = step * dt;
            if (Trace40C && step % 120 == 0)
            {
                Vec3 me = sim.mover.position;
                double fx = c.centre.x - me.x, fz = c.centre.z - me.z, lx = c.landHint.x - me.x, lz = c.landHint.z - me.z;
                double fl = JsMath.Hypot(fx, fz), ll = JsMath.Hypot(lx, lz), cosA = fl > 1e-6 && ll > 1e-6 ? (fx * lx + fz * lz) / (fl * ll) : 0;
                Console.WriteLine($"      {F37(t)} s me ({F37(me.x)},{F37(me.y)},{F37(me.z)}) g {sim.mover.grounded} ring ({F37(c.centre.x)},{F37(c.centre.y)},{F37(c.centre.z)}) fl {F37(fl)} cos {F37(cosA)} dy {F37(c.centre.y - me.y - 1)} target {c.passTarget} landing {c.HasLanding} {c.brain.state} {c.brain.Current?.id} gems {c.GemsLeft} rounds {c.rounds40} pushes {c.corePushes40} launches {pilot.launches}");
            }
            double moved = Vec3.DistanceTo(before, c.centre);
            if (moved > snapLimit) { r.snaps++; r.snapMax = Math.Max(r.snapMax, moved); if (r.firstSnapAt < 0) r.firstSnapAt = t; }
            // the rim: 16 points round the centre in the ring's plane (normal c.normal)
            Vec3 n = c.normal, u = Math.Abs(n.y) < 0.9 ? Vec3.Normalize(new Vec3(-n.z, 0, n.x)) : new Vec3(1, 0, 0);
            Vec3 v = new Vec3(n.y * u.z - n.z * u.y, n.z * u.x - n.x * u.z, n.x * u.y - n.y * u.x);
            bool inCore = false;
            for (int k = 0; k < 16 && !inCore; k++)
            {
                double a = k * Math.PI / 8;
                double ca = JsMath.Cos(a), sa = JsMath.Sin(a);
                double px = c.centre.x + R * (u.x * ca + v.x * sa), py = c.centre.y + R * (u.y * ca + v.y * sa), pz = c.centre.z + R * (u.z * ca + v.z * sa);
                if (Math.Abs(px - c.axis.x) < coreH && Math.Abs(pz - c.axis.z) < coreH && py < coreTop) inCore = true;
            }
            if (inCore) { r.coreSteps++; if (r.firstCoreAt < 0) r.firstCoreAt = t; }
            if (rect != null && (c.centre.x < rect[0] + R || c.centre.x > rect[2] - R || c.centre.z < rect[1] + R || c.centre.z > rect[3] - R))
            { r.outsideSteps++; if (r.firstOutsideAt < 0) r.firstOutsideAt = t; }
            Vec3 chest = new Vec3(sim.mover.position.x, sim.mover.position.y + 1, sim.mover.position.z);
            double dc = Vec3.DistanceTo(chest, c.centre);
            r.closeMin = Math.Min(r.closeMin, dc);
            if (dc < R && sim.mover.grounded) r.closeSteps++;
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.BossRoomBegin("crown37", out spawn, out yaw);
                sim.Respawn(spawn, yaw);
            }
        }
        r.killed = !c.Alive || c.Dying;
        r.seconds = Math.Max(0, step - revealSteps) * dt;
        r.passes = c.passes; r.falls = sim.crown37Falls; r.bladeHits = c.bladeHits; r.shardHits = c.shardHits; r.lowHits = c.lowHits;
        r.parries = c.parries; r.blocked = c.blocked; r.launches = pilot.launches; r.missed = pilot.missed;
        return r;
    }

    static string Pct40(int a, int b) => (b > 0 ? 100.0 * a / b : 0).ToString("0.0", CultureInfo.InvariantCulture) + " %";

    static int Defects40C(string level, int seeds, double cap)
    {
        bool on40 = level == "n40";
        Console.WriteLine($"defects: THE CROWN on {Path40C(level)} (crown37 on{(on40 ? ", crown40 on" : "")}), seeds 1..{seeds}, cap {cap:0} s");
        foreach (bool casual in new[] { false, true })
        {
            string who = casual ? "casual" : "skilled";
            var times = new List<double>();
            int killed = 0, snapRuns = 0, coreRuns = 0, outRuns = 0, snaps = 0, core = 0, outside = 0, close = 0, steps = 0, deaths = 0, falls = 0;
            double snapMax = 0;
            var why = new Dictionary<string, int>();
            for (int s = 1; s <= seeds; s++)
            {
                Run40C r = Fight40C(level, (uint)s, cap, casual, on40);
                foreach (var kv in r.fallWhy) why[kv.Key] = (why.TryGetValue(kv.Key, out int n0) ? n0 : 0) + kv.Value;
                if (r.killed) { killed++; times.Add(r.seconds); }
                snaps += r.snaps; core += r.coreSteps; outside += r.outsideSteps; close += r.closeSteps; steps += r.fightSteps; deaths += r.deaths; falls += r.falls;
                if (r.snaps > 0) snapRuns++;
                if (r.coreSteps > 0) coreRuns++;
                if (r.outsideSteps > 0) outRuns++;
                snapMax = Math.Max(snapMax, r.snapMax);
                Console.WriteLine($"  {who} seed {s}: killed {r.killed} in {F37(r.seconds)} s; passes {r.passes}, launches {r.launches}, missed {r.missed}, falls {r.falls}, deaths {r.deaths}; " +
                    $"taken blade {r.bladeHits} shard {r.shardHits} low {r.lowHits}; parries {r.parries}, blocked hits {r.blocked}; " +
                    $"snap {r.snaps} (max {F37(r.snapMax)} m, first {F37(r.firstSnapAt)} s), ring in the core {r.coreSteps} steps (first {F37(r.firstCoreAt)} s), " +
                    $"outside v11 {r.outsideSteps} steps, chest inside the ring's radius while grounded {r.closeSteps} steps (closest {F37(r.closeMin)} m)");
            }
            times.Sort();
            double P(double q) => times.Count == 0 ? -1 : times[Math.Min(times.Count - 1, (int)Math.Floor(q * (times.Count - 1) + 0.5))];
            Console.WriteLine($"{who}: killed {killed}/{seeds}, p50 {F37(P(0.5))} s, p90 {F37(P(0.9))} s, max {F37(P(1.0))} s; deaths {deaths}, falls {falls}; " +
                $"snaps {snaps} in {snapRuns}/{seeds} runs (max {F37(snapMax)} m); ring in the core {Pct40(core, steps)} of fight steps in {coreRuns}/{seeds} runs; " +
                $"outside v11 {Pct40(outside, steps)} in {outRuns}/{seeds} runs; chest inside the ring's radius (grounded) {Pct40(close, steps)}");
            var wl = new List<string>();
            foreach (var kv in why) wl.Add($"{kv.Value} x {kv.Key}");
            Console.WriteLine($"{who} falls by cause (-fallDamage each): {string.Join("; ", wl)}");
        }
        Hide40C(level);
        return 0; // a measurement (round 1's defect list); the bars come with the fixes
    }

    /// <summary>The anti-hide probe (Crown37.cs:407): after the reveal the player is put on leg 3's checkpoint (the plaza, ~60 m under the Crown)
    /// and stands still 6 s; then on leg 1's ledge far out on the bridge (> hideReach). Prints each snap: the centre's jump (m) and where it lands
    /// (its distance to the player's chest, inside the core or not).</summary>
    static void Hide40C(string level)
    {
        foreach (int place in new[] { 2, 0 })
        {
            SurvivalSim sim = Sim40C(level, 1, level == "n40");
            var si = (ScriptedInput)sim.input;
            sim.Respawn(new Vec3(-10, 0.02, -267), 0);
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Step();
            sim.BossRoomBegin("crown37", out _, out _);
            Crown37 c = sim.crown37;
            Tick37C(sim, c.p.revealSeconds + 0.1);
            Vec3 at = place == 2 ? sim.spire37Checkpoint[2] : new Vec3(-10, 0.02, -256);
            sim.Respawn(at, 0);
            var snaps = new List<string>();
            for (int i = 0; i < MovementMath.StepsFor(6, sim.step); i++)
            {
                si.Set(new InputFrame(), new CombatInput { weapon = -1 });
                Vec3 before = c.centre;
                int hides = c.brain.hides;
                sim.Step();
                sim.hitstopLeft = 0;
                if (sim.health.Dead) { sim.health.Reset(); sim.flow = Skygrind.Game.Flow.Playing; }
                double jump = Vec3.DistanceTo(before, c.centre);
                if (c.brain.hides > hides || jump > c.p.moveSpeed * sim.step * 1.5)
                {
                    Vec3 chest = sim.mover.position + new Vec3(0, 1, 0);
                    snaps.Add($"{F37(i * sim.step)} s: hide fired {c.brain.hides > hides}, the ring jumped {F37(jump)} m to {F37(Vec3.DistanceTo(chest, c.centre))} m from his chest");
                }
            }
            Console.WriteLine($"hide probe ({(place == 2 ? "leg 3's checkpoint, under the Crown" : "the bridge's far end, away from it")}): hides {c.brain.hides}; {(snaps.Count == 0 ? "no snap" : string.Join("; ", snaps))}");
        }
    }

    static int Line40C()
    {
        int fails = 0;
        foreach (bool on in new[] { true, false })
        {
            SurvivalSim sim = Sim40C("n40", 1, on);
            var si = (ScriptedInput)sim.input;
            var lines = new List<string>();
            int begins = 0, clears = 0, beginStep = -1, step = 0;
            sim.events.On<BossLine30>("bossLine40", e => lines.Add($"'{e.text}' (show {e.show}, step {step - beginStep})"));
            sim.events.On<LessonEvent>("lessonBegin", e => { if (e.kind == "boss40") begins++; });
            sim.events.On<LessonEvent>("lessonClear", e => { if (e.kind == "boss40") clears++; });
            sim.Respawn(new Vec3(-10, 0.02, -267), 0);
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Step();
            bool ok = sim.BossRoomBegin("crown37", out _, out _);
            beginStep = step;
            // stand still through the reveal: the summit shatters under him: a fall (the re-show), then 8 s more
            for (int k = 0; k < MovementMath.StepsFor(12, sim.step); k++)
            {
                si.Set(new InputFrame(), new CombatInput { weapon = -1 });
                sim.Step();
                sim.hitstopLeft = 0;
                step++;
            }
            string seen = lines.Count == 0 ? "none" : string.Join(", ", lines);
            bool pass = on ? ok && lines.Count == 2 && begins == 2 && clears >= 1 && sim.crown37Falls >= 1 : lines.Count == 0 && begins == 0;
            Console.WriteLine($"line (crown40 {(on ? "on" : "off")}): begin {ok}, falls {sim.crown37Falls}, lines {lines.Count}: {seen}; widget begins {begins}, clears {clears}: {Pass36(pass)}");
            if (!pass) fails++;
        }
        return fails;
    }
}
