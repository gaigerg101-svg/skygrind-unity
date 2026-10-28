// NIGHT26 (builder W): the weapon rows' headless judges.
//   weapons [--seeds n]       contract weapon-identity: the weapon-by-enemy table (THE RANGE's aim model, composed sim, solo guns, 8 bodies of each kind and a
//                             mixed room, the median score of the seeds: clear seconds + 0.05 x HP taken), printed with design.n26.weaponIdentity OFF (NIGHT25's
//                             arsenal: the RED side) and ON. Asserts on ON: no weapon is the best on every kind row; the ROCKET and the BOUNCER are not more than
//                             15 % faster in the mixed room than the best other gun.
//   pulse26                   contract pulse-lt (also `weapons --pulse`): the PULSE held, LT held charges by its hits and LT released launches; the left click held and
//                             released does not (ON); with design.n26.pulseLT off it is NIGHT25's: LT does nothing and the click released launches (the RED side).
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
    static readonly string[] Kinds26 = { "husk", "swarmer", "charger", "drone", "warden" };

    static SandboxSpec Weapons26Spec(int seeds)
    {
        SandboxSpec spec = SandboxSpec.Load(Path.Combine(RepoRoot, "Tools", "sandbox-cells.json"));
        spec.seeds = seeds;
        spec.cells.Clear();
        foreach (string k in Kinds26)
        {
            var c = new SandboxCell { id = "K-" + k, name = "8 " + k, round = 8, script = "stand" };
            c.groups.Add(new SandboxGroup { kind = k, count = 8, distance = 12, spread = 8 });
            spec.cells.Add(c);
        }
        var room = new SandboxCell { id = "ROOM", name = "mixed room", round = 8, script = "stand" };
        room.groups.Add(new SandboxGroup { kind = "husk", count = 4, distance = 12, spread = 8 });
        room.groups.Add(new SandboxGroup { kind = "swarmer", count = 4, distance = 15, spread = 8 });
        room.groups.Add(new SandboxGroup { kind = "charger", count = 1, distance = 18 });
        room.groups.Add(new SandboxGroup { kind = "drone", count = 2, distance = 14, spread = 6 });
        spec.cells.Add(room);
        return spec;
    }

    /// <summary>One table: gun -> cell id -> median score. Prints it and returns the asserts' verdict lines.</summary>
    static (Dictionary<string, Dictionary<string, double>> table, List<string> guns) Weapons26Table(GameConfig cfg, bool identity, int seeds, string only = null)
    {
        SandboxSpec spec = Weapons26Spec(seeds);
        var range = new SandboxRange(cfg, Map(), spec);
        range.sim.n26Override = identity ? new N26Blocks { weaponIdentity = true } : N26Blocks.Off;
        if (Weapons26N27.HasValue) range.sim.n27Override = Weapons26N27; // NIGHT27 (`weapons --n27`): pulse27 and chargeShots27 on in the table's sims
        SandboxRange.AltUse = AutopilotGuns.AltHeld;
        SandboxRange.SpecialUse = AutopilotGuns.SpecialHeld;
        List<string> guns = Skygrind.Game.Sandbox.Reachable(range.sim);
        guns.Add("gauntlet");
        if (only != null) guns = guns.Where(g => only.Split(',').Contains(g)).ToList(); // tuning: --gun rocket,rail
        var table = new Dictionary<string, Dictionary<string, double>>();
        var sb = new StringBuilder();
        sb.Append("| gun |");
        foreach (SandboxCell c in spec.cells) sb.Append(" ").Append(c.id).Append(" |");
        sb.Append("\n");
        foreach (string g in guns)
        {
            var row = new Dictionary<string, double>();
            sb.Append("| ").Append(g).Append(" |");
            foreach (SandboxCell c in spec.cells)
            {
                double m = range.Median(c, new List<string> { g }, null);
                row[c.id] = m;
                sb.Append(double.IsNaN(m) ? " n/a |" : " " + m.ToString("0.0", CultureInfo.InvariantCulture) + " |");
            }
            sb.Append("\n");
            table[g] = row;
        }
        Console.Write(sb.ToString());
        return (table, guns);
    }

    static (bool ok, string line) Weapons26Verdict(Dictionary<string, Dictionary<string, double>> table, List<string> guns)
    {
        var wins = new Dictionary<string, int>();
        foreach (string g in guns) wins[g] = 0;
        int rows = 0;
        foreach (string k in Kinds26)
        {
            string best = null;
            double bv = double.PositiveInfinity;
            foreach (string g in guns)
            {
                double v = table[g]["K-" + k];
                if (!double.IsNaN(v) && v < bv) { bv = v; best = g; }
            }
            if (best == null) continue;
            wins[best]++;
            rows++;
        }
        string top = wins.OrderByDescending(kv => kv.Value).First().Key;
        bool noSweep = wins[top] < rows;
        string bad = "";
        bool roomOk = true;
        foreach (string g in new[] { "rocket", "bouncer" })
        {
            if (!table.ContainsKey(g)) continue;
            var others = guns.Where(x => x != "rocket" && x != "bouncer").Select(x => table[x]["ROOM"]).Where(v => !double.IsNaN(v)).OrderBy(v => v).ToList();
            double median = others.Count % 2 == 1 ? others[others.Count / 2] : (others[others.Count / 2 - 1] + others[others.Count / 2]) / 2;
            double mine = table[g]["ROOM"];
            double ratio = mine / median;
            bool fastestRow = false;
            foreach (string k in Kinds26)
            {
                double best = guns.Select(x => table[x]["K-" + k]).Where(v => !double.IsNaN(v)).DefaultIfEmpty(double.NaN).Min();
                if (!double.IsNaN(best) && table[g]["K-" + k] <= best) fastestRow = true;
            }
            bool ok = ratio >= 1.0 && ratio <= 1.5 && !fastestRow; // GG: "not broken, not useless": 1.0x to 1.5x of the median of the other guns, the fastest on no kind row
            roomOk &= ok;
            bad += $" {g} ROOM {mine:0.0} = {ratio:0.00} x the median {median:0.0} of the other guns ({(ok ? "in the 1.0-1.5 band" : "OUT OF BAND")}{(fastestRow ? ", FASTEST ON A ROW" : ", fastest on no row")});";
        }
        string line = $"most kind rows won: {top} {wins[top]} of {rows} ({(noSweep ? "no sweep" : "SWEEP")});{bad}";
        return (noSweep && roomOk, line);
    }

    static partial void WeaponsCommand(string[] args, ref int code)
    {
        if (Flag(args, "--pulse27")) { Pulse27Command(args, ref code); return; } // NIGHT27 builder W: Harness.N27.Weapons.cs
        if (Flag(args, "--buy27")) { Buy27Command(ref code); return; }
        if (Flag(args, "--charge27")) { Charge27Command(args, ref code); return; }
        if (Flag(args, "--n27")) Weapons26N27 = new N27Blocks { pulse27 = true, chargeShots27 = true };
        if (Flag(args, "--pulse")) { Pulse26Command(args, ref code); return; }
        if (Flag(args, "--lt")) { Lt26Command(ref code); return; }
        GameConfig cfg = Config();
        int seeds = int.Parse(Opt(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        string only = Opt(args, "--gun", null);
        if (Flag(args, "--on-only"))
        {
            var (t1, g1) = Weapons26Table(cfg, true, seeds, only);
            code = 0;
            return;
        }
        Console.WriteLine($"weapons: design.n26.weaponIdentity OFF (NIGHT25's arsenal), {seeds} seeds, score = clear s + 0.05 x HP taken (cap 30)");
        var (off, guns) = Weapons26Table(cfg, false, seeds, only);
        var (offOk, offLine) = Weapons26Verdict(off, guns);
        Console.WriteLine($"weapons OFF: {(offOk ? "asserts pass (unexpected)" : "RED")}: {offLine}");
        Console.WriteLine("weapons: design.n26.weaponIdentity ON");
        var (on, guns2) = Weapons26Table(cfg, true, seeds, only);
        var (onOk, onLine) = Weapons26Verdict(on, guns2);
        Console.WriteLine($"weapons ON: {(onOk ? "PASS" : "FAIL")}: {onLine}");
        code = onOk ? 0 : 1;
    }

    /// <summary>`weapons --lt`: one row per weapon the game fires: its LT as the loadout reads it (zoom x, or its own alt) with the identity row on and off, beside the config's words.</summary>
    static void Lt26Command(ref int code)
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var rows = new List<string>();
        string[] guns = { "revolver", "gauntlet", "shotgun", "nailgun", "rocket", "rail", "zapper", "bouncer", "torch", "vortex" };
        int missing = 0;
        var sims = new Dictionary<bool, SurvivalSim>();
        foreach (bool on in new[] { false, true })
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.n26Override = on ? new N26Blocks { pulseLT = true, weaponIdentity = true } : N26Blocks.Off;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(2602, null, null);
            sims[on] = sim;
        }
        Console.WriteLine("| gun | LT with the identity row OFF (NIGHT25) | LT ON | config `lt` |");
        foreach (string g in guns)
        {
            string[] cell = new string[2];
            foreach (bool on in new[] { false, true })
            {
                Loadout lo = sims[on].loadout;
                double z = lo.ZoomGun(g);
                cell[on ? 1 : 0] = z > 1 ? $"zoom {z:0.##}x" : (g == "gauntlet" ? (on ? "blast (LT)" : "none (click)") : "own alt");
            }
            string key = "design.n26.weaponIdentity.weapons." + g + ".lt";
            string lt = cfg.Has(key) ? cfg.Str(key) : "NO ROW";
            if (lt == "NO ROW") missing++;
            Console.WriteLine($"| {g} | {cell[0]} | {cell[1]} | {lt} |");
        }
        Console.WriteLine($"weapons --lt: {guns.Length} weapons, {missing} without a config row: {(missing == 0 ? "PASS" : "FAIL")}");
        code = missing == 0 ? 0 : 1;
    }

    // --- the PULSE on LT -----------------------------------------------------------------------------------------------------------------

    /// <summary>One run: the PULSE in hand, `useLt` (LT) or the left click held against a husk 1.6 m ahead until the second notch (12 s at most), then let go.</summary>
    static (int hits, int tier, int flicksAfter, double allowed, int launchHits) Pulse26Run(GameConfig cfg, SurvivalMap map, bool pulseLt, bool useLt)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { chargeFist = true };
        sim.n25Override = new N25Blocks { meleeFire = true, pulseCharge = true, shockwaveOff = true };
        sim.n26Override = pulseLt ? new N26Blocks { pulseLT = true } : N26Blocks.Off;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(2601, null, null);
        sim.mover.invulnerable = true;
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        int slot = sim.loadout.owned.IndexOf("gauntlet");
        sim.Respawn(map.playerSpawn, 0);
        (double yaw, double ux, double uz) = ArclineOpenDir(sim, map.playerSpawn, 4);
        Husk h = Bag25(sim, map.playerSpawn, ux, uz, 1.6);
        int hits = 0;
        sim.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") hits++; });
        Fist24 f = sim.fist24;
        int maxSteps = (int)(12 / sim.step);
        for (int s = 0; s < maxSteps && f.Tier < 2; s++)
        {
            Pin25(h, map.playerSpawn, ux, uz, 1.6);
            double py = 0;
            if (h != null && h.alive) (yaw, py) = ArclineLook(sim.Eye, h.AimPoint);
            var c = new CombatInput { weapon = s == 0 ? slot : -1 };
            var b = new Button { held = true, pressed = s == 0 };
            if (useLt) c.altFire = b; else c.fire = b;
            Tick25(sim, yaw, py, c);
        }
        int tier = f.Tier;
        int flicks0 = f.flicks;
        int lh0 = sim.loadout.pulseLaunchHits;
        for (int s = 0; s < 20; s++)
        {
            Pin25(h, map.playerSpawn, ux, uz, 1.6);
            Tick25(sim, yaw, 0, new CombatInput { weapon = -1 });
            if (s == 0) { }
        }
        return (hits, tier, f.flicks - flicks0, f.lastAllowed, sim.loadout.pulseLaunchHits - lh0);
    }

    static void Pulse26Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        bool allOk = true;
        foreach (bool on in new[] { false, true })
        {
            var lt = Pulse26Run(cfg, map, on, true);
            var click = Pulse26Run(cfg, map, on, false);
            bool ok = on
                ? lt.hits >= 14 && lt.tier >= 2 && lt.flicksAfter == 1 && lt.launchHits >= 1 && click.flicksAfter == 0
                : lt.hits == 0 && lt.flicksAfter == 0 && click.flicksAfter == 1;
            Console.WriteLine($"pulse26 design.n26.pulseLT {(on ? "ON" : "OFF (NIGHT25's click version: the RED side)")}: LT held: {lt.hits} hits, notch {lt.tier}, then LT released: {lt.flicksAfter} launch(es) over {lt.allowed:0.0} m, {lt.launchHits} body hit by the launch; "
                + $"left click held: {click.hits} hits, notch {click.tier}, then released: {click.flicksAfter} launch(es) | {(on ? (ok ? "PASS" : "FAIL") : (ok ? "RED as expected" : "unexpected"))}");
            if (on) allOk &= ok;
            else allOk &= ok;
        }
        code = allOk ? 0 : 1;
    }
}
