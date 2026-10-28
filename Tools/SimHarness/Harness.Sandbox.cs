// NIGHT19 (the lead; builders never edit this file: NIGHT19.md "How tonight works" rule 10): the sandbox judges.
//   range [--gun id] [--cell id] [--pair a,b] [--points] [--probes] [--seeds n] [--tag t] [--off19]
//                  THE RANGE (Game/Bot/SandboxRange.cs): each gun x cell's median score (the clear time, capped, + 0.05 x HP
//                  taken), the pairs of design.arsenal.combos (S4), the points mode (S7), the gust probes (S10's RANGE half)
//   chase [--bot kite|stand|fight] [--round 10|15] [--gust on|off] [--twist deadline] [--seeds n] [--tag t] [--off19]
//                  THE CHASE (Game/Bot/SandboxChase.cs): survival, damage per minute, the first hit, hits by source, empty bar
//   pfarm --gun id [--seeds n] [--from s] [--tag t] [--off19]
//                  THE PARITY FARM (Farm.RunOne lockGun): the gun and the GAUNTLET, never another gun; normal; a JSONL line
//                  per run and the median round
//   sbreport [--tag t]   SANDBOX.md: the S1-S11 rows from the tag's range.json, chase.json, pfarm-*.jsonl and farm.jsonl
// Each writes Markdown and JSON to Logs/sandbox/<tag>/ (default "dev"); each is deterministic (SandboxHarnessTests).
// --off19 (Program.cs) turns every NIGHT19 design block off: NIGHT18's game, the A side.
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
    static void Sandbox(string cmd, string[] args, ref int code)
    {
        if (cmd == "range") RangeCommand(args, ref code);
        else if (cmd == "chase") ChaseCommand(args, ref code);
        else if (cmd == "pfarm") ParityFarmCommand(args, ref code);
        else if (cmd == "sbreport") SandboxReportCommand(args, ref code);
        else if (cmd == "spot") SpotCommand(ref code);
        else if (cmd == "climb") ClimbCommand(args, ref code); // NIGHT20: THE CLIMB (Harness.Climb.cs)
        else if (cmd == "marathon") MarathonCommand(args, ref code); // NIGHT20: THE MARATHON (Harness.Climb.cs)
    }

    /// <summary>The open spot's search, stage by stage (a debug view of Sandbox.OpenSpot).</summary>
    static void SpotCommand(ref int code)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        MapBox fl = sim.map.floor;
        double y = fl.max.y;
        double below = cfg.Num("ai.grid.lookBelow"), above = cfg.Num("ai.grid.lookAbove");
        int all = 0, node = 0, dist = 0, free = 0;
        double bestClear = 0;
        for (double x = fl.min.x + 4; x <= fl.max.x - 4; x += 4)
            for (double z = fl.min.z + 4; z <= fl.max.z - 4; z += 4)
            {
                all++;
                if (sim.nav.grid.NodeAt(x, y, z, below, above) < 0) continue;
                node++;
                sim.zones.OpenAll();
                dist++;
                var p = new Vec3(x, y, z);
                double c0 = Skygrind.Game.Sandbox.Clear(sim, p, 0), c1 = Skygrind.Game.Sandbox.Clear(sim, p, Math.PI / 2);
                if (c0 < 1 || c1 < 1) continue;
                free++;
                for (int h = 0; h < 16; h++) bestClear = Math.Max(bestClear, Skygrind.Game.Sandbox.Clear(sim, p, h * Math.PI / 8));
            }
        Console.WriteLine($"floor {fl.min.x}..{fl.max.x} x {fl.min.z}..{fl.max.z} at y {y}; points {all}, with a node {node}, finite nav distance {dist}, free {free}; the longest single ray {bestClear:0.0} m");
        var (pos, yaw, clear) = Skygrind.Game.Sandbox.OpenSpot(sim);
        Console.WriteLine($"OpenSpot: ({pos.x}, {pos.y}, {pos.z}) yaw {yaw * 180 / Math.PI:0} deg, clear {clear:0.0} m");
        code = 0;
    }

    static string Opt(string[] args, string name, string fallback)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
    }

    static bool Flag(string[] args, string name) => Array.IndexOf(args, name) >= 0;

    static string RepoRoot => Path.GetFullPath(Path.Combine(Root, ".."));

    static SandboxSpec Spec(string[] args)
    {
        SandboxSpec s = SandboxSpec.Load(Path.Combine(RepoRoot, "Tools", "sandbox-cells.json"));
        string seeds = Opt(args, "--seeds", null);
        if (seeds != null) s.seeds = int.Parse(seeds, CultureInfo.InvariantCulture);
        return s;
    }

    static string Inv(double v, string f = "0.00") => Skygrind.Game.Sandbox.F(v, f);

    // --- THE RANGE -----------------------------------------------------------------------------------------------------------

    static void RangeCommand(string[] args, ref int code)
    {
        var wall = System.Diagnostics.Stopwatch.StartNew();
        GameConfig cfg = Config();
        SandboxSpec spec = Spec(args);
        string tag = Opt(args, "--tag", "dev");
        string dir = Skygrind.Game.Sandbox.Dir(RepoRoot, tag);
        var range = new SandboxRange(cfg, Map(), spec);
        SandboxRange.AltUse = AutopilotGuns.AltHeld; // NIGHT20: builder G's bot rule (AltUse finished tonight): the LT tools and the zooms
        SandboxRange.SpecialUse = AutopilotGuns.SpecialHeld;
        string onlyGun = Opt(args, "--gun", null), onlyCell = Opt(args, "--cell", null), pair = Opt(args, "--pair", null);
        if (Flag(args, "--trace"))
        {
            // the per-shot log of one episode (--gun, --cell, --seed): every fire, hit, kill and hurt with its step and time
            // from the episode's start; the hand checks recompute a cell's score from it
            SandboxCell tc = spec.Cell(onlyCell ?? "C1");
            uint seed = uint.Parse(Opt(args, "--seed", "1"), CultureInfo.InvariantCulture);
            int s0 = -1;
            double T() => (range.sim.simStep - s0) * range.sim.step;
            range.sim.events.On("fire", o =>
            {
                Console.WriteLine($"  t {T():0.000} s (step {range.sim.simStep - s0}): fire {o}");
            });
            range.sim.events.On<HitEvent>("hit", h => Console.WriteLine($"  t {T():0.000} s: hit {h.weapon} on {h.kind} #{h.enemy}: {h.damage:0.##}{(h.headshot ? " HEAD" : "")}{(h.killed ? " KILLED" : "")}"));
            range.sim.events.On<KillEvent>("kill", k => Console.WriteLine($"  t {T():0.000} s: kill {k.kind} #{k.enemy} by {k.weapon}{(k.headshot ? " (head)" : "")}"));
            range.sim.events.On<HurtEvent>("hurt", h => Console.WriteLine($"  t {T():0.000} s: hurt {h.amount:0.##} by {h.source}"));
            s0 = range.sim.simStep; // the episode starts here (StartRun steps nothing)
            SandboxRange.Result tr = range.Run(tc, new List<string> { onlyGun ?? "revolver" }, seed);
            Console.WriteLine($"trace {onlyGun ?? "revolver"} {tc.id} seed {seed}: cleared {tr.cleared} in {tr.seconds:0.000} s ({tr.steps} steps), HP taken {tr.hurt:0.##}, score {tr.score:0.000}, kills {tr.kills}; the round's HP mult {range.sim.director.HpMult:0.###}");
            code = 0;
            return;
        }
        List<string> guns = Skygrind.Game.Sandbox.Reachable(range.sim);
        guns.Add("gauntlet");
        if (onlyGun != null) guns = new List<string> { onlyGun };
        var cells = new List<SandboxCell>();
        foreach (SandboxCell c in spec.cells) if (onlyCell == null || c.id == onlyCell) cells.Add(c);
        var md = new StringBuilder();
        var js = new StringBuilder();
        md.Append($"# THE RANGE ({tag})\n\n{Skygrind.Game.Sandbox.Version}; {Side()}; seeds 1..{spec.seeds} (median); the spot ({Inv(range.spot.x, "0.0")}, {Inv(range.spot.z, "0.0")}) facing {Inv(range.spotYaw * 180 / Math.PI, "0")} deg, {Inv(range.spotClear, "0.0")} m clear.\n");
        md.Append("A cell's score: the clear time in s (capped at " + Inv(spec.capSeconds, "0") + ") + " + Inv(spec.damageWeight) + " x the HP taken; lower is better. n/a: a kind or affix with no port on this build.\n\n");
        js.Append("{\"version\":").Append(Skygrind.Game.Sandbox.Q(Skygrind.Game.Sandbox.Version)).Append(",\"tag\":").Append(Skygrind.Game.Sandbox.Q(tag)).Append(",\"blocks\":").Append(range.sim.n19.Bits).Append(",\"seeds\":").Append(spec.seeds);
        if (pair == null)
        {
            md.Append("| Gun |");
            foreach (SandboxCell c in cells) md.Append($" {c.id} {c.name} |");
            md.Append("\n| --- |");
            foreach (SandboxCell c in cells) md.Append(" --- |");
            md.Append('\n');
            js.Append(",\"cells\":[");
            for (int i = 0; i < cells.Count; i++) js.Append(i > 0 ? "," : "").Append(Skygrind.Game.Sandbox.Q(cells[i].id));
            js.Append("],\"table\":{");
            var detail = new StringBuilder();
            for (int gi = 0; gi < guns.Count; gi++)
            {
                string g = guns[gi];
                md.Append($"| {Skygrind.Game.Sandbox.Name(cfg, g)} ({g}) |");
                js.Append(gi > 0 ? "," : "").Append(Skygrind.Game.Sandbox.Q(g)).Append(":{");
                for (int ci = 0; ci < cells.Count; ci++)
                {
                    var eps = new List<SandboxRange.Result>();
                    double m = range.Median(cells[ci], new List<string> { g }, eps);
                    string why = eps.Count > 0 && eps[0].na ? eps[0].why : null;
                    int cleared = eps.Count(e => e.cleared);
                    md.Append(double.IsNaN(m) ? " n/a |" : $" {Inv(m)} ({cleared}/{eps.Count}) |");
                    js.Append(ci > 0 ? "," : "").Append(Skygrind.Game.Sandbox.Q(cells[ci].id)).Append(':').Append(double.IsNaN(m) ? "null" : Inv(m, "0.####"));
                    if (why != null && gi == 0) detail.Append($"- {cells[ci].id}: n/a ({why})\n");
                    if (gi == 0 && ci == 0 && eps.Count > 0)
                    {
                        SandboxRange.Result e0 = eps[0];
                        detail.Append($"- the hand check's row ({g} {cells[ci].id} seed {e0.seed}): cleared {e0.cleared} in {Inv(e0.seconds, "0.000")} s ({e0.steps} steps), {e0.kills} kills, HP taken {Inv(e0.hurt, "0.0")}, score {Inv(e0.score, "0.000")}, points {Inv(e0.points, "0")}\n");
                    }
                }
                md.Append('\n');
                js.Append('}');
                Console.WriteLine($"range {g}: {wall.Elapsed.TotalSeconds:0} s");
            }
            js.Append('}');
            md.Append("\n(n/m): the seeds that cleared before the cap.\n\n").Append(detail);
        }
        // the pairs: design.arsenal.combos (each {name, setup, payoff, cell, setupSeconds, payoffSeconds}), or --pair a,b in C6
        var combos = new List<(string name, string a, string b, string cell, double sa, double sb)>();
        if (pair != null)
        {
            string[] ab = pair.Split(',');
            combos.Add(("--pair", ab[0], ab[1], onlyCell ?? "C6", 1.5, 1.5));
        }
        else if (onlyGun == null && cfg.Has("design.arsenal.combos"))
        {
            foreach (object o in PortData.Arr(cfg.Get("design.arsenal.combos")))
            {
                var d = PortData.Obj(o);
                combos.Add(((string)d["name"], (string)d["setup"], (string)d["payoff"], (string)d["cell"], PortData.Num(d["setupSeconds"]), PortData.Num(d["payoffSeconds"])));
            }
        }
        if (combos.Count > 0)
        {
            md.Append("\n## The pairs (S4)\n\n| Combo | Setup -> payoff | Cell | Pair score | Best single (gun) | Gain |\n| --- | --- | --- | --- | --- | --- |\n");
            js.Append(",\"pairs\":[");
            for (int i = 0; i < combos.Count; i++)
            {
                var (name, a, b, cellId, sa, sb) = combos[i];
                SandboxCell cell = spec.Cell(cellId);
                double period = sa + sb;
                double pairScore = range.Median(cell, new List<string> { a, b }, null, t => (t % period) < sa ? 0 : 1);
                double bestSingle = double.PositiveInfinity;
                string bestGun = "?";
                foreach (string g in Skygrind.Game.Sandbox.Reachable(range.sim))
                {
                    double m = range.Median(cell, new List<string> { g }, null);
                    if (m < bestSingle)
                    {
                        bestSingle = m;
                        bestGun = g;
                    }
                }
                double gain = bestSingle / pairScore - 1;
                md.Append($"| {name} | {a} -> {b} ({Inv(sa, "0.0")} s / {Inv(sb, "0.0")} s) | {cellId} | {Inv(pairScore)} | {Inv(bestSingle)} ({bestGun}) | {Inv(gain * 100, "0")} % |\n");
                js.Append(i > 0 ? "," : "").Append($"{{\"name\":{Skygrind.Game.Sandbox.Q(name)},\"setup\":{Skygrind.Game.Sandbox.Q(a)},\"payoff\":{Skygrind.Game.Sandbox.Q(b)},\"cell\":{Skygrind.Game.Sandbox.Q(cellId)},\"score\":{Inv(pairScore, "0.####")},\"bestSingle\":{Inv(bestSingle, "0.####")},\"bestGun\":{Skygrind.Game.Sandbox.Q(bestGun)}}}");
            }
            js.Append(']');
        }
        if (Flag(args, "--points") || (onlyGun == null && onlyCell == null && pair == null))
        {
            // S7: points per minute in C6, rotating every rotateEvery s through the reachable guns vs each gun alone
            SandboxCell c6 = spec.Cell("C6");
            List<string> reach = Skygrind.Game.Sandbox.Reachable(range.sim);
            double Ppm(List<string> gs, Func<double, int> slotAt)
            {
                var vals = new List<double>();
                for (int s = 1; s <= spec.seeds; s++)
                {
                    SandboxRange.Result r = range.Run(c6, gs, (uint)s, slotAt);
                    vals.Add(r.seconds > 0 ? r.points / (r.seconds / 60) : 0);
                }
                return Skygrind.Game.Sandbox.Median(vals);
            }
            int n = reach.Count;
            double rot = Ppm(reach, t => (int)Math.Floor(t / spec.rotateEvery) % n);
            var singles = new List<double>();
            md.Append("\n## Points mode (S7, C6)\n\n| Loadout | Points / min |\n| --- | --- |\n");
            foreach (string g in reach)
            {
                double p = Ppm(new List<string> { g }, null);
                singles.Add(p);
                md.Append($"| {g} alone | {Inv(p, "0")} |\n");
            }
            double med = Skygrind.Game.Sandbox.Median(singles);
            md.Append($"| rotating every {Inv(spec.rotateEvery, "0")} s through {n} | {Inv(rot, "0")} |\n\nThe ratio rotating / the median single gun: **{Inv(rot / med)}** (S7 target 1.3-2.0).\n");
            js.Append($",\"points\":{{\"rotating\":{Inv(rot, "0.##")},\"singleMedian\":{Inv(med, "0.##")},\"ratio\":{Inv(rot / med, "0.####")}}}");
        }
        if (Flag(args, "--probes") || (onlyGun == null && onlyCell == null && pair == null))
        {
            // S10's RANGE half: a gust-jump's height gain (vs a plain jump, below the COMET's rocket jump); C3's push-back
            // NIGHT20: the LT tool is the THUNDERCLAP's shockwave when design.shockwave is live (the KESTREL's gust before)
            string ltGun = !N20Blocks.ForceOff && N20Blocks.From(Config()).shockwave ? "shotgun" : "revolver";
            double plain = range.JumpPeak("revolver", false, 0);
            double gustJump = range.JumpPeak(ltGun, true, 0.25);
            double rocketJump = range.JumpPeak("rocket", false, 0.25);
            var (pushed, of, fired) = range.GustSwarm(spec.Cell("C3"), 5, 3, 1, ltGun);
            md.Append($"\n## The gust probes (S10)\n\n- a plain jump peaks at {Inv(plain)} m; with the {(ltGun == "shotgun" ? "THUNDERCLAP's shockwave" : "KESTREL's gust")} (LT) 0.25 s in, looking down: {Inv(gustJump)} m (gain {Inv(gustJump - plain)} m, target 2-4 m); the COMET at the feet 0.25 s in: {Inv(rocketJump)} m (the gust must stay below it).\n");
            md.Append($"- C3: the LT when the nearest SWARMER is within 5 m: a gust fired: {fired}; {pushed} of {of} pushed back >= 3 m in 0.5 s (target >= 3).\n");
            js.Append($",\"probes\":{{\"plainJump\":{Inv(plain, "0.####")},\"gustJump\":{Inv(gustJump, "0.####")},\"rocketJump\":{Inv(rocketJump, "0.####")},\"gustFired\":{(fired ? "true" : "false")},\"pushed\":{pushed},\"of\":{of}}}");
        }
        js.Append('}');
        string suffix = onlyGun != null || onlyCell != null || pair != null ? "-" + (onlyGun ?? "") + (onlyCell ?? "") + (pair != null ? "pair-" + pair.Replace(',', '-') : "") : "";
        Skygrind.Game.Sandbox.Write(Path.Combine(dir, "RANGE" + suffix + ".md"), md.ToString());
        Skygrind.Game.Sandbox.Write(Path.Combine(dir, "range" + suffix + ".json"), js.ToString());
        Console.Write(md.ToString());
        Console.WriteLine($"range: {wall.Elapsed.TotalSeconds:0.0} s wall -> {dir}");
        code = 0;
    }

    // --- THE CHASE -----------------------------------------------------------------------------------------------------------

    static void ChaseCommand(string[] args, ref int code)
    {
        var wall = System.Diagnostics.Stopwatch.StartNew();
        GameConfig cfg = Config();
        SandboxSpec spec = Spec(args);
        string tag = Opt(args, "--tag", "dev");
        string dir = Skygrind.Game.Sandbox.Dir(RepoRoot, tag);
        string onlyBot = Opt(args, "--bot", null), gust = Opt(args, "--gust", null);
        string onlyRound = Opt(args, "--round", null);
        string twist = Opt(args, "--twist", null); // NIGHT21: `--twist deadline` forces the DEADLINE twist on the round under test
        Skygrind.Game.SandboxChase.Twist = twist;
        var bots = onlyBot != null ? new[] { onlyBot } : new[] { "kite", "stand", "fight" };
        if (Flag(args, "--trace"))
        {
            uint seed = uint.Parse(Opt(args, "--seed", "1"), CultureInfo.InvariantCulture);
            int rr = int.Parse(Opt(args, "--round", "10"), CultureInfo.InvariantCulture);
            SandboxChase.Trace = l => Console.WriteLine("  " + l);
            SandboxChase.Result tr = SandboxChase.Run(cfg, Map(), bots[0], rr, seed);
            SandboxChase.Trace = null;
            Console.WriteLine($"trace {bots[0]} round {rr} seed {seed}: survival {tr.survival:0.000} s, died {tr.died}, HP taken {tr.hurt:0.##}, {tr.hitsBy.Values.Sum()} hits, first hit {tr.firstHit:0.000} s, empty bar {tr.emptyBar:0.0} s");
            code = 0;
            return;
        }
        var rounds = onlyRound != null ? new[] { int.Parse(onlyRound, CultureInfo.InvariantCulture) } : new[] { 10, 15 };
        var md = new StringBuilder($"# THE CHASE ({tag})\n\n{Skygrind.Game.Sandbox.Version}; seeds 1..{spec.seeds}; a {Inv(SandboxChase.Limit, "0")} s limit from the round's start; {(N19Blocks.ForceOff ? "every NIGHT19 block off (--off19)" : "the design blocks as configured")}{(gust == "off" ? ", the LT tool (the gust / the shockwave) OFF" : "")}.\n\n");
        md.Append("| Bot | Round | Survival (median s) | Died | Damage / min (median) | First hit (median s) | Empty bar (median s) | Hits by source (sum) |\n| --- | --- | --- | --- | --- | --- | --- | --- |\n");
        var js = new StringBuilder("{\"version\":" + Skygrind.Game.Sandbox.Q(Skygrind.Game.Sandbox.Version) + ",\"rows\":[");
        bool first = true;
        foreach (int round in rounds)
        {
            foreach (string bot in bots)
            {
                var surv = new List<double>();
                var dpm = new List<double>();
                var fh = new List<double>();
                var empty = new List<double>();
                var hits = new SortedDictionary<string, int>(StringComparer.Ordinal);
                int died = 0;
                for (int s = 1; s <= spec.seeds; s++)
                {
                    SandboxChase.GustOff = gust == "off";
                    SandboxChase.Result r = SandboxChase.Run(cfg, Map(), bot, round, (uint)s);
                    SandboxChase.GustOff = false;
                    surv.Add(r.survival);
                    dpm.Add(r.dpm);
                    if (r.firstHit >= 0) fh.Add(r.firstHit);
                    empty.Add(r.emptyBar);
                    if (r.died) died++;
                    foreach (var kv in r.hitsBy) hits[kv.Key] = (hits.TryGetValue(kv.Key, out int n) ? n : 0) + kv.Value;
                }
                string hs = string.Join(", ", hits.Select(kv => $"{kv.Key} {kv.Value}"));
                md.Append($"| {bot.ToUpperInvariant()} | {round} | {Inv(Skygrind.Game.Sandbox.Median(surv), "0.0")} | {died}/{spec.seeds} | {Inv(Skygrind.Game.Sandbox.Median(dpm), "0.0")} | {Inv(Skygrind.Game.Sandbox.Median(fh), "0.0")} | {Inv(Skygrind.Game.Sandbox.Median(empty), "0.0")} | {hs} |\n");
                js.Append(first ? "" : ",").Append($"{{\"bot\":{Skygrind.Game.Sandbox.Q(bot)},\"round\":{round},\"gust\":{Skygrind.Game.Sandbox.Q(gust ?? "on")},\"survival\":{Inv(Skygrind.Game.Sandbox.Median(surv), "0.###")},\"died\":{died},\"dpm\":{Inv(Skygrind.Game.Sandbox.Median(dpm), "0.###")},\"firstHit\":{(fh.Count > 0 ? Inv(Skygrind.Game.Sandbox.Median(fh), "0.###") : "null")},\"emptyBar\":{Inv(Skygrind.Game.Sandbox.Median(empty), "0.###")},\"survivals\":[{string.Join(",", surv.Select(v => Inv(v, "0.###")))}]}}");
                first = false;
                Console.WriteLine($"chase {bot} r{round}: {wall.Elapsed.TotalSeconds:0} s");
            }
        }
        js.Append("]}");
        string suffix = (onlyBot != null ? "-" + onlyBot : "") + (onlyRound != null ? "-r" + onlyRound : "") + (gust == "off" ? "-gustoff" : "") + (twist != null ? "-" + twist : "");
        Skygrind.Game.Sandbox.Write(Path.Combine(dir, "CHASE" + suffix + ".md"), md.ToString());
        Skygrind.Game.Sandbox.Write(Path.Combine(dir, "chase" + suffix + ".json"), js.ToString());
        Console.Write(md.ToString());
        Console.WriteLine($"chase: {wall.Elapsed.TotalSeconds:0.0} s wall -> {dir}");
        code = 0;
    }

    // --- THE PARITY FARM -----------------------------------------------------------------------------------------------------

    static void ParityFarmCommand(string[] args, ref int code)
    {
        var wall = System.Diagnostics.Stopwatch.StartNew();
        GameConfig cfg = Config();
        string gun = Opt(args, "--gun", "revolver");
        int seeds = int.Parse(Opt(args, "--seeds", "40"), CultureInfo.InvariantCulture);
        uint from = uint.Parse(Opt(args, "--from", "1"), CultureInfo.InvariantCulture);
        string tag = Opt(args, "--tag", "dev");
        string dir = Skygrind.Game.Sandbox.Dir(RepoRoot, tag);
        if (BotNav.CacheDir == null) BotNav.CacheDir = Path.Combine(Path.GetTempPath(), "skygrind-botnav");
        Farm.BuildStamp = "harness";
        string path = Path.Combine(dir, $"pfarm-{gun}-{from}.jsonl");
        var rounds = new List<double>();
        using (var w = new StreamWriter(path, false, new UTF8Encoding(false)))
        {
            for (int i = 0; i < seeds; i++)
            {
                uint seed = from + (uint)i;
                Farm.Record r = Farm.RunOne(cfg, Map(), seed, "normal", cfg.Num("design.farm.maxMinutes") * 60, null, 600, gun);
                w.Write(r.ToJson());
                w.Write('\n');
                w.Flush();
                rounds.Add(r.round);
                Console.WriteLine($"pfarm {gun} seed {seed}: round {r.round}, {r.cause}, kills {r.kills} ({wall.Elapsed.TotalSeconds:0} s)");
            }
        }
        Console.WriteLine($"pfarm {gun}: {seeds} runs from seed {from}: median round {Inv(Skygrind.Game.Sandbox.Median(rounds), "0.0")}; {wall.Elapsed.TotalSeconds:0.0} s wall -> {path}");
        code = 0;
    }

    // --- SANDBOX.md: the S1-S11 rows -----------------------------------------------------------------------------------------

    static Dictionary<string, object> ReadJson(string path) => File.Exists(path) ? PortData.Obj(PortData.LoadJson(path)) : null;

    static void SandboxReportCommand(string[] args, ref int code)
    {
        string tag = Opt(args, "--tag", "dev");
        string dir = Skygrind.Game.Sandbox.Dir(RepoRoot, tag);
        GameConfig cfg = Config();
        var md = new StringBuilder($"# SANDBOX ({tag}): S1-S11\n\n{Skygrind.Game.Sandbox.Version}. The rows come from this folder's range.json, chase*.json, pfarm-*.jsonl and farm*.jsonl (each judge's own .md has the tables).\n\n");
        md.Append("| # | Metric | Value | Target | Verdict |\n| --- | --- | --- | --- | --- |\n");
        var range = ReadJson(Path.Combine(dir, "range.json"));
        // S1-S3 from the table (the reachable guns: every row but the GAUNTLET)
        if (range != null && range.ContainsKey("table"))
        {
            var table = PortData.Obj(range["table"]);
            var cells = PortData.Arr(range["cells"]).Select(o => (string)o).ToList();
            var guns = table.Keys.Where(g => g != "gauntlet").ToList();
            double V(string g, string c) => PortData.Obj(table[g]).TryGetValue(c, out object v) && v != null ? PortData.Num(v) : double.NaN;
            var valued = cells.Where(c => guns.All(g => !double.IsNaN(V(g, c)))).ToList();
            // S1: a dominates b when a <= 1.05 x b in every valued cell
            var dominated = new List<string>();
            foreach (string a in guns)
                foreach (string b in guns)
                    if (a != b && valued.All(c => V(a, c) <= V(b, c) * 1.05)) dominated.Add($"{b} by {a}");
            md.Append($"| S1 | No gun is dominated | {(dominated.Count == 0 ? "none" : string.Join("; ", dominated))} ({valued.Count} cells: {string.Join(" ", valued)}) | 0 dominated | {(dominated.Count == 0 ? "PASS" : "FAIL")} |\n");
            // S2: near-best (<= 1.10 x best) in >= 2 cells; best in <= 35 % of cells
            var s2 = new List<string>();
            bool s2ok = true;
            foreach (string g in guns)
            {
                int near = 0, best = 0;
                foreach (string c in valued)
                {
                    double min = guns.Min(x => V(x, c));
                    if (V(g, c) <= min * 1.10) near++;
                    if (V(g, c) == min) best++;
                }
                double share = valued.Count > 0 ? (double)best / valued.Count : 0;
                if (near < 2 || share > 0.35) s2ok = false;
                s2.Add($"{g} near {near} best {best} ({Inv(share * 100, "0")} %)");
            }
            md.Append($"| S2 | Every gun has a niche | {string.Join("; ", s2)} | each near-best in >= 2 cells, best in <= 35 % | {(s2ok ? "PASS" : "FAIL")} |\n");
            if (cells.Contains("C7"))
            {
                double min = guns.Min(x => V(x, "C7"));
                double worst = guns.Max(x => V(x, "C7") / min);
                string worstGun = guns.OrderByDescending(x => V(x, "C7")).First();
                md.Append($"| S3 | No gun falls off (C7) | worst {worstGun} {Inv(worst)} x the best ({Inv(min)}) | <= 1.6 x | {(worst <= 1.6 ? "PASS" : "FAIL")} |\n");
            }
            else md.Append("| S3 | No gun falls off (C7) | n/a (C7 not run) | <= 1.6 x | n/a |\n");
        }
        else md.Append("| S1-S3 | the RANGE table | n/a (no range.json in this tag) | | n/a |\n");
        // S4
        if (range != null && range.ContainsKey("pairs"))
        {
            var pairs = PortData.Arr(range["pairs"]).Select(PortData.Obj).ToList();
            var gains = pairs.Select(p => $"{p["name"]} {Inv((PortData.Num(p["bestSingle"]) / PortData.Num(p["score"]) - 1) * 100, "0")} %").ToList();
            int ok = pairs.Count(p => PortData.Num(p["bestSingle"]) / PortData.Num(p["score"]) - 1 >= 0.2);
            md.Append($"| S4 | Combos pay | {pairs.Count} combos, {ok} at >= 20 %: {string.Join("; ", gains)} | >= 5 combos, each >= 20 %, every gun in one | {(pairs.Count >= 5 && ok == pairs.Count ? "PASS (check every gun is in one)" : "FAIL")} |\n");
        }
        else md.Append("| S4 | Combos pay | n/a (no design.arsenal.combos on this build) | >= 5 combos, each >= 20 % | n/a |\n");
        // S5: the parity farm
        var byGun = new SortedDictionary<string, List<double>>(StringComparer.Ordinal);
        foreach (string f in Directory.GetFiles(dir, "pfarm-*.jsonl"))
        {
            string gun = Path.GetFileNameWithoutExtension(f).Split('-')[1];
            foreach (string line in File.ReadAllLines(f))
            {
                if (line.Trim().Length == 0) continue;
                var o = PortData.Obj(JsJson.Parse(line));
                if (!byGun.TryGetValue(gun, out var l)) byGun[gun] = l = new List<double>();
                l.Add(PortData.Num(o["round"]));
            }
        }
        if (byGun.Count > 0)
        {
            var meds = byGun.ToDictionary(kv => kv.Key, kv => Skygrind.Game.Sandbox.Median(kv.Value));
            double all = Skygrind.Game.Sandbox.Median(meds.Values.ToList());
            bool ok = meds.Values.All(m => Math.Abs(m - all) <= 0.2 * all);
            md.Append($"| S5 | One gun goes as far as another | {string.Join("; ", meds.Select(kv => $"{kv.Key} {Inv(kv.Value, "0.0")} ({byGun[kv.Key].Count})"))}; median of medians {Inv(all, "0.0")} | each within +/-20 % | {(ok ? "PASS" : "FAIL")} |\n");
        }
        else md.Append("| S5 | One gun goes as far as another | n/a (no pfarm-*.jsonl) | each within +/-20 % | n/a |\n");
        // S6: free play
        var kills = new SortedDictionary<string, double>(StringComparer.Ordinal);
        var farmRounds = new List<double>();
        foreach (string f in Directory.GetFiles(dir, "farm*.jsonl"))
        {
            foreach (string line in File.ReadAllLines(f))
            {
                if (line.Trim().Length == 0) continue;
                var o = PortData.Obj(JsJson.Parse(line));
                farmRounds.Add(PortData.Num(o["round"]));
                foreach (var kv in PortData.Obj(o["killsByGun"])) kills[kv.Key] = (kills.TryGetValue(kv.Key, out double n) ? n : 0) + PortData.Num(kv.Value);
            }
        }
        if (kills.Count > 0)
        {
            double total = kills.Values.Sum();
            var guns = kills.Where(kv => kv.Key != "gauntlet" && kv.Key != "punch").OrderByDescending(kv => kv.Value).ToList();
            double top = guns.Count > 0 ? guns[0].Value / total : 0;
            int five = guns.Count(kv => kv.Value / total >= 0.05);
            md.Append($"| S6 | Free play uses many guns | top {(guns.Count > 0 ? guns[0].Key : "?")} {Inv(top * 100, "0")} %; {five} guns >= 5 %: {string.Join(", ", kills.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {Inv(kv.Value / total * 100, "0")} %"))}; {farmRounds.Count} runs, median round {Inv(Skygrind.Game.Sandbox.Median(farmRounds), "0.0")} | top <= 30 %, >= 5 guns at >= 5 % | {(top <= 0.30 && five >= 5 ? "PASS" : "FAIL")} |\n");
        }
        else md.Append("| S6 | Free play uses many guns | n/a (no farm*.jsonl) | top <= 30 %, >= 5 guns at >= 5 % | n/a |\n");
        // S7
        if (range != null && range.ContainsKey("points"))
        {
            var p = PortData.Obj(range["points"]);
            double ratio = PortData.Num(p["ratio"]);
            md.Append($"| S7 | Rotation pays | rotating {Inv(PortData.Num(p["rotating"]), "0")} / single median {Inv(PortData.Num(p["singleMedian"]), "0")} points/min = {Inv(ratio)} | 1.3-2.0 (and damage identical with freshness on / off: FreshnessN19Tests) | {(ratio >= 1.3 && ratio <= 2.0 ? "PASS" : "FAIL")} |\n");
        }
        else md.Append("| S7 | Rotation pays | n/a | 1.3-2.0 | n/a |\n");
        // S8, S9, S11 (THE CHASE); S10 (the probes + the KITE with the gust on / off)
        var rows = new List<Dictionary<string, object>>();
        var gustOffRows = new List<Dictionary<string, object>>();
        foreach (string f in Directory.GetFiles(dir, "chase*.json"))
        {
            var j = ReadJson(f);
            foreach (object o in PortData.Arr(j["rows"]))
            {
                var d = PortData.Obj(o);
                if ((string)d["gust"] == "off") gustOffRows.Add(d);
                else rows.Add(d);
            }
        }
        Dictionary<string, object> Row(string bot, int round) => rows.LastOrDefault(d => (string)d["bot"] == bot && (int)PortData.Num(d["round"]) == round);
        var k10 = Row("kite", 10);
        var k15 = Row("kite", 15);
        if (k10 != null || k15 != null)
        {
            double s10 = k10 != null ? PortData.Num(k10["survival"]) : double.NaN, s15 = k15 != null ? PortData.Num(k15["survival"]) : double.NaN;
            md.Append($"| S8 | Kiting isn't free | KITE survives {Inv(s10, "0.0")} s at round 10, {Inv(s15, "0.0")} s at round 15 (median) | <= 120 s and <= 60 s | {(s10 <= 120 && s15 <= 60 ? "PASS" : "FAIL")} |\n");
        }
        else md.Append("| S8 | Kiting isn't free | n/a (no chase rows) | <= 120 s / <= 60 s | n/a |\n");
        var f10 = Row("fight", 10);
        var st10 = Row("stand", 10);
        if (f10 != null && st10 != null)
        {
            double a = PortData.Num(f10["dpm"]), b = PortData.Num(st10["dpm"]);
            md.Append($"| S9 | Movement still matters | FIGHT {Inv(a, "0.0")} HP/min vs STAND {Inv(b, "0.0")} = {Inv(b > 0 ? a / b * 100 : double.NaN, "0")} % | <= 50 % | {(b > 0 && a <= 0.5 * b ? "PASS" : "FAIL")} |\n");
        }
        else md.Append("| S9 | Movement still matters | n/a | <= 50 % | n/a |\n");
        string probes = "n/a";
        bool probesOk = false;
        if (range != null && range.ContainsKey("probes"))
        {
            var p = PortData.Obj(range["probes"]);
            double gain = PortData.Num(p["gustJump"]) - PortData.Num(p["plainJump"]);
            bool below = PortData.Num(p["gustJump"]) < PortData.Num(p["rocketJump"]);
            int pushed = (int)PortData.Num(p["pushed"]);
            probes = $"gust-jump +{Inv(gain)} m (rocket jump {Inv(PortData.Num(p["rocketJump"]) - PortData.Num(p["plainJump"]))} m); C3 {pushed}/{(int)PortData.Num(p["of"])} pushed >= 3 m";
            probesOk = gain >= 2 && gain <= 4 && below && pushed >= 3;
        }
        var kOff = gustOffRows.LastOrDefault(d => (string)d["bot"] == "kite" && (int)PortData.Num(d["round"]) == 10);
        string kite = "KITE gust on/off: n/a";
        bool kiteOk = false;
        if (kOff != null && k10 != null)
        {
            double on = PortData.Num(k10["survival"]), off = PortData.Num(kOff["survival"]);
            kite = $"KITE r10 {Inv(on, "0.0")} s with the gust vs {Inv(off, "0.0")} s without (+{Inv(off > 0 ? (on / off - 1) * 100 : double.NaN, "0")} %)";
            kiteOk = off > 0 && on <= 1.25 * off;
        }
        md.Append($"| S10 | The gust is a tool | {probes}; {kite} | +2-4 m below the rocket jump; >= 3 of 8; <= +25 % | {(probesOk && kiteOk ? "PASS" : "FAIL or n/a")} |\n");
        if (k10 != null && f10 != null)
        {
            double ek = PortData.Num(k10["emptyBar"]), ef = PortData.Num(f10["emptyBar"]);
            md.Append($"| S11 | Sprint is earned back by fighting | FIGHT on an empty bar {Inv(ef, "0.0")} s vs KITE {Inv(ek, "0.0")} s (round 10, median); the sprint length and the range rewards: StaminaN19Tests | FIGHT < KITE; 3.0-3.6 s; >= 0.25 within 6 m, <= 0.1 beyond 15 m | {(ef < ek ? "PASS (with the tests)" : "FAIL")} |\n");
        }
        else md.Append("| S11 | Sprint is earned back | n/a | | n/a |\n");
        Skygrind.Game.Sandbox.Write(Path.Combine(dir, "SANDBOX.md"), md.ToString());
        Console.Write(md.ToString());
        code = 0;
    }
}

