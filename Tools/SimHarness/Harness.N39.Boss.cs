// NIGHT39 builder B1 (boss39; NIGHT39.md card B1, DECIDE39 D-1): every boss's band with design.n39.boss39 on, headless. One entry, N39Boss
// (declared and called in Program.cs).
//   boss39 [wick|stag|wyrm|kiln|loom|crown|stag31|finale|all] [--seeds N] [--cap S] [--set boss.key=v[,v,v]]... [--show] [--off39]
//     Runs each boss's own NIGHT37 / NIGHT32 band (the same pilots, seeds 1..N) with the boss39 bit forced on (SurvivalSim.Force39Boss;
//     --off39: every NIGHT39 block off, NIGHT38's numbers: the BEFORE table) and prints one row per boss: killed, kill time p10 / p50 / p90
//     (THE WICK: the total, walk-in + climb + cap, as its D-7 band), the person estimate (x2: Research/2026-10-14-wave39/B1.md), the verdict.
//       wick    wick37 band (Band37W)          stag    stag37 band (StagBand37)     loom    loom37 band (Loom37Band)
//       crown   crown37 band (Band37C)         wyrm    boss32 wyrm (Boss32Sweep)    stag31  boss32 stag (Boss32Sweep: level 6's old Stag)
//       kiln    THE KILN WYRM on level 3 itself (n38-kiln k5, K4's wyrm39-arena; Kiln39Band; the wyrm row's numbers)
//       finale  Stag31 on level 6 itself (n35-finale x6; Finale39Band; the stag31 row's numbers). Both load the level with the game's config.
//     --set boss.key=v: a tuning probe in place of design.n39.boss39.<boss>.<key> (SurvivalSim.Probe39Boss; arrays as v,v,v).
//     --p39: P1's p39 (the KESTREL's hold rule, fire interval x1.2) forced on as well (SurvivalSim.Force39Pistol); --noboss39: boss39 as the
//       config says (with --p39: NIGHT38's bosses under the new KESTREL).
//     --show: the band's own lines too (each band still prints its NIGHT37 / NIGHT32 PASS rule: those were the old 15-22 s bands).
//     PASS (boss39 on): every seed killed, p50 30-60 s, p90 <= 70 s (a person 60-120 s at the factor 2, "maybe three if you're slow").
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N39Boss(string cmd, string[] args, ref int code)
    {
        if (cmd != "boss39") return;
        string which = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 240);
        bool show = Array.IndexOf(args, "--show") >= 0;
        bool off = N39Blocks.ForceOff;
        var probe = new Dictionary<string, double[]>();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] != "--set") continue;
            string[] kv = args[i + 1].Split('=');
            if (kv.Length != 2) { Console.WriteLine($"boss39: bad --set {args[i + 1]} (want boss.key=v[,v,v])"); code = 2; return; }
            probe[kv[0]] = Array.ConvertAll(kv[1].Split(','), s => double.Parse(s, CultureInfo.InvariantCulture));
        }
        SurvivalSim.Probe39Boss = probe.Count > 0 ? probe : null;
        bool p39 = Array.IndexOf(args, "--p39") >= 0;
        bool noBoss = Array.IndexOf(args, "--noboss39") >= 0;
        SurvivalSim.Force39Boss = off || noBoss ? (bool?)null : true;
        SurvivalSim.Force39Pistol = p39 && !off;
        int fails = 0;
        var rows = new List<string>();
        try
        {
            foreach (string b in Boss39Rigs)
            {
                if (which != "all" && which != b) continue;
                SurvivalSim.p39ForcedRuns = SurvivalSim.p39LiveRuns = 0;
                Band39 r = Band39Run(b, seeds, cap, show);
                if (p39 && !off) r.note += (r.note.Length > 0 ? "; " : "") + $"p39 live in {SurvivalSim.p39LiveRuns}/{SurvivalSim.p39ForcedRuns} runs";
                bool ok = r.killed == r.seeds && r.p50 >= 30 && r.p50 <= 60 && r.p90 <= 70;
                if (!off && !ok) fails++;
                rows.Add($"  {b,-7} {r.killed}/{r.seeds} killed  p10 {r.p10,6:0.0} s  p50 {r.p50,6:0.0} s  p90 {r.p90,6:0.0} s  person ~{r.p50 * 2,5:0} s (p90 ~{r.p90 * 2:0} s)  " +
                    (off ? "(off39: NIGHT38's numbers)" : ok ? "PASS" : "RED") + (r.note.Length > 0 ? "  " + r.note : ""));
            }
        }
        finally
        {
            SurvivalSim.Force39Boss = null;
            SurvivalSim.Probe39Boss = null;
            SurvivalSim.Force39Pistol = false;
        }
        Console.WriteLine($"boss39 {(off ? "OFF (--off39)" : noBoss ? "as the config says (--noboss39)" : "ON")}{(p39 && !off ? " + p39 forced ON" : "")}{(probe.Count > 0 ? " probe " + string.Join(" ", ProbeText39(probe)) : "")}: seeds {seeds}, cap {cap:0} s");
        foreach (string row in rows) Console.WriteLine(row);
        if (!off) Console.WriteLine(fails == 0 ? "boss39: GREEN (every boss: all killed, p50 30-60 s, p90 <= 70 s)" : $"boss39: RED ({fails} outside the band)");
        code = off || fails == 0 ? 0 : 1;
    }

    /// <summary>The rigs in order: the row's bosses, then `kiln` (THE KILN WYRM on level 3 itself: the wyrm row's numbers in K4's arena).</summary>
    static readonly string[] Boss39Rigs = { "wick", "stag", "wyrm", "kiln", "loom", "crown", "stag31", "finale" };

    /// <summary>`boss39 kiln`: THE KILN WYRM on n38-kiln's k5 THE RING (the level's own map and campaign, so the worm loads the room's wyrmArena,
    /// Design/maps/wyrm39-arena.json, and its crumble31 boxes): DOORFREE, the player put at k5's spawn, stepped until the campaign begins the worm,
    /// then boss32's WyrmPilot31 (punch and slug) with the gauntlet and the THUNDERCLAP, mortal (a death: health back, BossRoomBegin("wyrm")'s
    /// retry, its spawn). The game's config as it is (every row as the file says; boss39 / p39 as forced). Prints the bands' summary format.</summary>
    static void Kiln39Band(int seeds, double cap)
    {
        var times = new List<double>();
        int killed = 0;
        string poolNote = "";
        for (uint seed = 1; seed <= seeds; seed++)
        {
            GameConfig cfg = Config();
            SurvivalSim.Wyrm24Dir = MapGenRepo;
            try
            {
                string path = Path.Combine(MapGenRepo, "Design", "levels", "n38-kiln.json");
                SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
                SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                sim.noLab = sim.noRecords = true;
                sim.setup.mode = Campaign24.Mode;
                sim.StartRun(seed, null, null);
                sim.husks.frozen = true;
                for (int i = 0; i < 10; i++) sim.Step();
                sim.campaign.SetDoorFree29(true);
                RoomDef k5 = sim.campaignLevel.rooms.Find(r => r.id == "k5");
                sim.Respawn(k5.spawn, k5.spawnYawDeg);
                foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
                int wait = 0;
                while ((sim.wyrm24 == null || !sim.wyrm24.Alive) && wait++ < 600) sim.Step();
                if (sim.wyrm24 == null || !sim.wyrm24.Alive) { Console.WriteLine($"  seed {seed}: NO FIGHT (campaign room {sim.campaign.Room?.id})"); times.Add(cap); continue; }
                Wyrm w = sim.wyrm24;
                var si = (ScriptedInput)sim.input;
                var wp = new WyrmPilot31();
                int step = 0, deaths = 0, capSteps = (int)(cap * 120);
                Vec3 retry = k5.spawn;
                while (w.Alive && step < capSteps)
                {
                    wp.Step(sim, step);
                    si.SetLook(wp.Move.yaw, wp.Move.pitch);
                    si.Set(wp.Move, wp.Combat);
                    sim.Step();
                    sim.hitstopLeft = 0;
                    step++;
                    if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
                    {
                        deaths++;
                        sim.downSteps = 0;
                        sim.flow = Skygrind.Game.Flow.Playing;
                        sim.health.Reset();
                        if (sim.BossRoomBegin("wyrm", out Vec3 sp, out double yaw)) retry = sp;
                        sim.Respawn(retry, 0);
                    }
                }
                bool k = !w.Alive;
                if (k) { killed++; times.Add(step / 120.0); }
                string arena = sim.wyrm39ArenaBuilt ?? "?";
                poolNote = $"pool {w.body.maxHp}";
                Console.WriteLine($"  seed {seed}: {(k ? "killed" : "NOT killed")} in {step / 120.0:0.0} s, deaths {deaths}, pool {w.body.maxHp}, phase {w.phase}, arena {arena} (half {sim.wyrm24Arena?.half:0.#}), room {sim.campaign.Room?.id}");
            }
            finally { SurvivalSim.Wyrm24Dir = null; }
        }
        times.Sort();
        double Q(double q) => times.Count == 0 ? double.NaN : times[Math.Min(times.Count - 1, (int)Math.Floor(q * (times.Count - 1) + 0.5))];
        Console.WriteLine($"kiln band: killed {killed}/{seeds}, p10 {Q(0.1):0.0} s, p50 {Q(0.5):0.0} s, p90 {Q(0.9):0.0} s; {poolNote}");
    }

    /// <summary>`boss39 finale`: level 6's old Stag (Stag31) on n35-finale's boss room (plan id glacier) with the game's config as it is (the
    /// `boss32 stag` rig builds NIGHT31's rows on the survival map, with no charge35 object): DOORFREE, the player put at the room's spawn, stepped
    /// until the campaign begins the Stag, then NIGHT31's StagPilot31, mortal (a death: health back, BossRoomBegin("glacier")'s retry).</summary>
    static void Finale39Band(int seeds, double cap)
    {
        var times = new List<double>();
        int killed = 0;
        for (uint seed = 1; seed <= seeds; seed++)
        {
            GameConfig cfg = Config();
            SurvivalSim.Wyrm24Dir = MapGenRepo;
            try
            {
                string path = Path.Combine(MapGenRepo, "Design", "levels", "n35-finale.json");
                SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
                SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                sim.noLab = sim.noRecords = true;
                sim.setup.mode = Campaign24.Mode;
                sim.StartRun(seed, null, null);
                sim.husks.frozen = true;
                for (int i = 0; i < 10; i++) sim.Step();
                sim.campaign.SetDoorFree29(true);
                RoomDef room = sim.campaignLevel.rooms.Find(r => r.boss == "glacier");
                sim.Respawn(room.spawn, room.spawnYawDeg);
                if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
                int wait = 0;
                while ((sim.stag31 == null || !sim.stag31.Alive) && wait++ < 600) sim.Step();
                if (sim.stag31 == null || !sim.stag31.Alive) { Console.WriteLine($"  seed {seed}: NO FIGHT (campaign room {sim.campaign.Room?.id})"); continue; }
                Stag31 st = sim.stag31;
                var si = (ScriptedInput)sim.input;
                var pilot = new StagPilot31();
                int step = 0, deaths = 0, capSteps = (int)(cap * 120);
                while (st.Alive && step < capSteps)
                {
                    pilot.Step(sim, step);
                    si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                    si.Set(pilot.Move, pilot.Combat);
                    sim.Step();
                    sim.hitstopLeft = 0;
                    step++;
                    if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
                    {
                        deaths++;
                        sim.downSteps = 0;
                        sim.flow = Skygrind.Game.Flow.Playing;
                        sim.health.Reset();
                        if (sim.BossRoomBegin("glacier", out Vec3 sp, out double yaw)) sim.Respawn(sp, yaw);
                    }
                }
                bool k = !st.Alive;
                if (k) { killed++; times.Add(step / 120.0); }
                Console.WriteLine($"  seed {seed}: {(k ? "killed" : "NOT killed")} in {step / 120.0:0.0} s, deaths {deaths}, pool {st.body.maxHp}, room {sim.campaign.Room?.id}");
            }
            finally { SurvivalSim.Wyrm24Dir = null; }
        }
        times.Sort();
        double Q(double q) => times.Count == 0 ? double.NaN : times[Math.Min(times.Count - 1, (int)Math.Floor(q * (times.Count - 1) + 0.5))];
        Console.WriteLine($"finale band: killed {killed}/{seeds}, p10 {Q(0.1):0.0} s, p50 {Q(0.5):0.0} s, p90 {Q(0.9):0.0} s");
    }

    static IEnumerable<string> ProbeText39(Dictionary<string, double[]> probe)
    {
        foreach (var kv in probe) yield return kv.Key + "=" + string.Join(",", Array.ConvertAll(kv.Value, v => v.ToString("0.###", CultureInfo.InvariantCulture)));
    }

    sealed class Band39
    {
        public int killed, seeds;
        public double p10 = double.NaN, p50 = double.NaN, p90 = double.NaN;
        public string note = "";
    }

    /// <summary>One boss's own band, its printed summary parsed (killed, the first p10 / p50 / p90: THE WICK's total).</summary>
    static Band39 Band39Run(string boss, int seeds, double cap, bool show)
    {
        TextWriter keep = Console.Out;
        var sw = new StringWriter();
        Console.SetOut(sw);
        try
        {
            switch (boss)
            {
                case "wick": Band37W(seeds, cap); break;
                case "stag": StagBand37(seeds, cap); break;
                case "loom": Loom37Band(seeds, cap); break;
                case "crown": Band37C(seeds, cap); break;
                case "kiln": Kiln39Band(seeds, cap); break;
                case "finale": Finale39Band(seeds, cap); break;
                case "wyrm":
                case "stag31":
                    sets30.Clear();
                    pistolFist32 = false;
                    SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
                    try { Boss32Sweep(boss == "wyrm" ? "wyrm" : "stag", seeds, (int)cap); }
                    finally { SurvivalSim.Wyrm24Dir = null; SurvivalSim.Boss32Probe = null; }
                    break;
            }
        }
        finally { Console.SetOut(keep); }
        string text = sw.ToString();
        if (show) Console.Write(text);
        var r = new Band39 { seeds = seeds };
        Match k = Regex.Match(text, @"killed (\d+)/(\d+)|(\d+)/(\d+) killed");
        if (k.Success) r.killed = int.Parse(k.Groups[1].Success ? k.Groups[1].Value : k.Groups[3].Value);
        double Q(string q)
        {
            Match m = Regex.Match(text, q + @" ([0-9.]+|NaN)");
            return m.Success && m.Groups[1].Value != "NaN" ? double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : double.NaN;
        }
        r.p10 = Q("p10");
        r.p50 = Q("p50");
        r.p90 = Q("p90");
        if (boss == "wick")
        {
            Match c = Regex.Match(text, @"FROM THE CAP p10 [0-9.]+ s, p50 ([0-9.]+) s");
            if (c.Success) r.note = "from the cap p50 " + c.Groups[1].Value + " s";
        }
        if (boss == "wyrm" || boss == "kiln")
        {
            Match pool = Regex.Match(text, @"pool (\d+)");
            if (pool.Success) r.note = "pool " + pool.Groups[1].Value;
        }
        return r;
    }
}
