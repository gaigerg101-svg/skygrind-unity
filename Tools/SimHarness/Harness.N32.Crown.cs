// NIGHT32 builder C: `crown32 [--seeds N] [--first n] [--cap s] [--nocamp] [--dbg] [--set key.path=value ...]` (DESIGN32 C, rules 1 and 2). Declared and called by the lead's
// Program.cs (N32Crown). THE CROWN (design.n32.boss5, on through the run's override; NIGHT31's charge shots and LT rule on: the KESTREL charge shot, the PULSE parry, the slug,
// the NAILGUN detonate) in its ring at design.n32.boss5.room.origin, with all four weapons owned:
//  1. the SWEEP: the moving pilot (Game/Bot/Crown32Pilot.cs, mortal: a death is a retry, the bar back to the beat's start) over N seeds: kill time p10 / p50 / p90 (band 50-75 s,
//     p90 at most 100 s), deaths, the full-damage share (hits that land at x1.0 or more / every hit that reached the boss), the plates' share of the damage, unhittable seconds per
//     minute of fight (beat changes; the intro is not counted), and the beat start times.
//  2. the CAMP TABLE: four MORTAL campers (centre on the inner disc; the rim at the wall; the high spot: a rim ledge top; the far side across the ring), each in a fresh fight, hold
//     their spot (walk back when knocked off), aim at the core and fire their best gun by range. The 30 s clock starts when the core can be hurt. A camper LOSES when it dies or
//     loses at least 50 % of its health within 30 s and has not killed the boss. Its damage rate is printed against a third of the pilot's (camp31's strict R2 column).
// --set key.path=value changes one number under design.n32.boss5 for a tuning probe (e.g. --set pool=2800 --set sweep.rate=1.4).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static readonly List<string> crownSets32 = new List<string>();

    static partial void N32Crown(string cmd, string[] args, ref int code)
    {
        if (cmd == "crown32walk")
        {
            uint seed = 1;
            for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--seed") uint.TryParse(args[i + 1], out seed);
            code = CrownWalk32(seed, Array.IndexOf(args, "--dbg") >= 0, Array.IndexOf(args, "--die") >= 0);
            return;
        }
        if (cmd != "crown32") return;
        int seeds = 20, first = 1;
        double cap = 240;
        crownSets32.Clear();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") int.TryParse(args[i + 1], out seeds);
            if (args[i] == "--first") int.TryParse(args[i + 1], out first);
            if (args[i] == "--cap") double.TryParse(args[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out cap);
            if (args[i] == "--set") crownSets32.Add(args[i + 1]);
        }
        bool dbg = Array.IndexOf(args, "--dbg") >= 0, noCamp = Array.IndexOf(args, "--nocamp") >= 0;
        crownKit32 = new[] { "gauntlet", "shotgun", "nailgun" };
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--kit") crownKit32 = args[i + 1].Split(',');
            if (args[i] == "--svg") { CrownSvg32(CrownCfg32(), args[i + 1]); code = 0; return; }
        }
        code = Crown32Sweep(seeds, (uint)first, cap, dbg, noCamp);
    }

    /// <summary>The config with the --set probes applied under design.n32.boss5 (a key path's segments are found in order after "boss5").</summary>
    static GameConfig CrownCfg32()
    {
        string design = File.ReadAllText(Path.Combine(Root, "..", "Design", "unity-config.json"));
        foreach (string kv in crownSets32)
        {
            string[] a = kv.Split('=');
            int at = design.IndexOf("\"boss5\": {", StringComparison.Ordinal);
            if (at < 0 || a.Length != 2) throw new Exception("crown32 --set: no design.n32.boss5 or a bad pair " + kv);
            string[] path = a[0].Split('.');
            for (int i = 0; i < path.Length - 1; i++)
            {
                at = design.IndexOf("\"" + path[i] + "\":", at, StringComparison.Ordinal);
                if (at < 0) throw new Exception("crown32 --set: no " + a[0]);
            }
            var rx = new System.Text.RegularExpressions.Regex("(\"" + path[path.Length - 1] + "\"\\s*:\\s*)(\\[[^\\]]*\\]|[-0-9.a-z]+)");
            var m = rx.Match(design, at);
            if (!m.Success) throw new Exception("crown32 --set: no " + a[0]);
            design = design.Substring(0, m.Index) + m.Groups[1].Value + a[1] + design.Substring(m.Index + m.Length);
        }
        return GameConfig.FromText(File.ReadAllText(Path.Combine(Root, "config.json")), design);
    }

    /// <summary>A fresh CROWN fight: the player at the ring's spawn with all four weapons; null when it cannot begin.</summary>
    static SurvivalSim CrownSim32(GameConfig cfg, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n31Override = new N31Blocks { charge = true, ltAlways = true };
        sim.n32Override = new N32Blocks { boss5 = true };
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.Crown32Begin(out Vec3 spawn, out double yaw)) return null;
        sim.Respawn(spawn, yaw);
        foreach (string g in crownKit32) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        return sim;
    }

    /// <summary>The weapons added to the start's KESTREL (--kit gauntlet: DESIGN32 B's "pistol + fist only" check).</summary>
    static string[] crownKit32 = { "gauntlet", "shotgun", "nailgun" };

    /// <summary>`--svg out.svg`: a top-down drawing of the ring (rule 3: look at it): the floor (inner disc, the eight sectors, the drop pattern's first sectors red-orange),
    /// the 64 wall facets, the ledges, the beat-2 rings, the spawn and the four camp spots, the core's beat-2 orbit; 1 m = 20 px.</summary>
    static void CrownSvg32(GameConfig cfg, string path)
    {
        var a = Crown32Arena.Build(cfg);
        var p = Crown32Params.From(cfg);
        var ci = CultureInfo.InvariantCulture;
        double sc = 20, half = (a.p.apothem + 3) * sc;
        string P(double x, double z) => (half + x * sc).ToString("0.0", ci) + "," + (half + z * sc).ToString("0.0", ci);
        var sb = new System.Text.StringBuilder();
        sb.Append($"<svg xmlns='http://www.w3.org/2000/svg' width='{2 * half:0}' height='{2 * half + 60:0}' viewBox='0 0 {2 * half:0} {2 * half + 60:0}'><rect width='100%' height='100%' fill='#15121c'/>");
        int mask = 0;
        foreach (int o in p.dropPattern) mask |= 1 << (o % a.p.sectors);
        foreach (Crown32Tri t in a.tris)
        {
            string fill = t.part == Crown32Part.Inner ? "#3a3550" : t.part == Crown32Part.Sector ? ((mask & (1 << t.sector)) != 0 ? "#7a2a14" : "#2c283a") : t.part == Crown32Part.LedgeTop ? "#8a86a0" : null;
            if (fill == null) continue;
            sb.Append($"<polygon points='{P(t.a.x, t.a.z)} {P(t.b.x, t.b.z)} {P(t.c.x, t.c.z)}' fill='{fill}' stroke='{fill}' stroke-width='0.6'/>");
        }
        var wall = new System.Text.StringBuilder();
        for (int i = 0; i <= a.p.facets; i++) { Vec3 c = Crown32Arena.Polar(i * a.FacetAngle, a.CornerRadius, 0); wall.Append(P(c.x, c.z) + " "); }
        sb.Append($"<polyline points='{wall}' fill='none' stroke='#b9b0d8' stroke-width='3'/>");
        for (int s = 0; s < a.p.sectors; s++)
        {
            Vec3 i0 = Crown32Arena.Polar(s * a.SectorAngle, a.p.innerRadius, 0), o0 = Crown32Arena.Polar(s * a.SectorAngle, a.p.apothem, 0), m = Crown32Arena.Polar((s + 0.5) * a.SectorAngle, (a.p.innerRadius + a.p.apothem) / 2, 0);
            sb.Append($"<line x1='{P(i0.x, i0.z).Split(',')[0]}' y1='{P(i0.x, i0.z).Split(',')[1]}' x2='{P(o0.x, o0.z).Split(',')[0]}' y2='{P(o0.x, o0.z).Split(',')[1]}' stroke='#15121c' stroke-width='2'/>");
            sb.Append($"<text x='{P(m.x, m.z).Split(',')[0]}' y='{P(m.x, m.z).Split(',')[1]}' fill='#cfc8e8' font-size='18' font-family='monospace' text-anchor='middle'>s{s}</text>");
        }
        var orbit = new System.Text.StringBuilder();
        for (int i = 0; i <= 64; i++) { Vec3 c = Crown32Arena.Polar(i * 2 * Math.PI / 64, p.orbitRadius, 0); orbit.Append(P(c.x, c.z) + " "); }
        sb.Append($"<polyline points='{orbit}' fill='none' stroke='#ff5a1f' stroke-width='1.5' stroke-dasharray='6 6'/>");
        var dummy = new Crown32(cfg, p, a, new EventBus(), new BossTarget { position = () => default }, () => 1) { origin = default };
        for (int k = 0; k < p.ringCount; k++) { Vec3 r = dummy.RingAt(k); string q = P(r.x, r.z); sb.Append($"<circle cx='{q.Split(',')[0]}' cy='{q.Split(',')[1]}' r='12' fill='none' stroke='#19d3e6' stroke-width='4'/>"); }
        foreach (var (name, at) in CrownSpots32(a, default))
        {
            string q = P(at.x, at.z);
            sb.Append($"<circle cx='{q.Split(',')[0]}' cy='{q.Split(',')[1]}' r='7' fill='#ffffff'/><text x='{q.Split(',')[0]}' y='{double.Parse(q.Split(',')[1], ci) - 12:0}' fill='#ffffff' font-size='14' font-family='monospace' text-anchor='middle'>{name.Split(' ')[0]}</text>");
        }
        string sp = P(0, a.p.spawnRadius);
        sb.Append($"<rect x='{double.Parse(sp.Split(',')[0], ci) - 8:0}' y='{double.Parse(sp.Split(',')[1], ci) - 8:0}' width='16' height='16' fill='#19d3e6'/>");
        sb.Append($"<rect x='{half - a.p.innerRadius * 0 - p.coreHalf * sc:0}' y='{half - p.coreHalf * sc:0}' width='{2 * p.coreHalf * sc:0}' height='{2 * p.coreHalf * sc:0}' fill='#1c1826' stroke='#ff5a1f' stroke-width='3'/>");
        sb.Append($"<text x='10' y='{2 * half + 40:0}' fill='#cfc8e8' font-size='18' font-family='monospace'>THE CROWN ring: apothem {a.p.apothem} m, {a.p.facets} facets, inner disc {a.p.innerRadius} m, {a.p.sectors} sectors (red: drop pattern), ledges grey, rings cyan, spawn cyan square, campers white, beat-2 orbit dashed</text></svg>");
        File.WriteAllText(path, sb.ToString());
        Console.WriteLine("crown32 svg: wrote " + path + " (" + a.tris.Count + " triangles)");
    }

    sealed class CrownRow32
    {
        public bool killed;
        public double seconds, hpEnd, fullShare, plateShare, unhitPerMin, parryDmg;
        public int deaths;
        public string beats = "", counters = "";
        public readonly Dictionary<string, double> byWeapon = new Dictionary<string, double>();
    }

    static CrownRow32 CrownPilotRun32(GameConfig cfg, uint seed, double capSeconds, bool dbg)
    {
        var row = new CrownRow32();
        SurvivalSim sim = CrownSim32(cfg, seed);
        if (sim == null) { row.counters = "NOT BUILT"; return row; }
        Crown32 cr = sim.crown32;
        double dt = sim.cfg.Dt;
        var si = (ScriptedInput)sim.input;
        var pilot = new Crown32Pilot(seed);
        int step = 0, cap = (int)(capSeconds / dt);
        sim.events.On<Crown32Event>("crownBeat", e => row.beats += $"b{e.beat}@{step * dt:0}s ");
        if (dbg) sim.events.On<DetonateEvent>("nailDetonate", e => Console.WriteLine($"      detonate t={step * dt:0.0}s nails {e.nails}"));
        if (dbg) sim.events.On<Crown32Event>("crownSweep", e => { if (e.stage == "hit") Console.WriteLine($"      beam hit at t={step * dt:0.00}s feet {sim.mover.position.y - cr.origin.y:0.00} m, state {sim.mover.state}, grounded {sim.mover.grounded}, pilot jumps {pilot.jumps}, step {step}; {pilot.jumpNote}"); });
        while (cr.Alive && step < cap)
        {
            pilot.Step(sim, step);
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (dbg && step % 1200 == 0) Console.WriteLine($"      t={step * dt:0}s {cr.phase} beat {cr.beat}/{cr.act} hp {cr.HpFrac:0.00} php {sim.health.hp:0} gun {pilot.gun} at r {JsMath.Hypot(sim.mover.position.x - cr.origin.x, sim.mover.position.z - cr.origin.z):0.0} y {sim.mover.position.y - cr.origin.y:0.0} core hits {cr.coreHits} plates {cr.plateHits} blocked {cr.blockedHits}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Crown32Begin(out Vec3 sp, out double yw);
                sim.Respawn(sp, yw);
            }
        }
        row.killed = !cr.Alive;
        row.seconds = step * dt;
        row.hpEnd = cr.HpFrac;
        int landed = cr.coreHits + cr.plateHits + cr.parryHits;
        row.fullShare = landed + cr.blockedHits > 0 ? (double)landed / (landed + cr.blockedHits) : 0;
        row.plateShare = cr.totalDamage > 0 ? cr.plateDamage / cr.totalDamage : 0;
        row.unhitPerMin = cr.unhittableSteps + cr.hurtableSteps > 0 ? 60.0 * cr.unhittableSteps / (cr.unhittableSteps + cr.hurtableSteps) : 0;
        row.parryDmg = sim.crown32ParryDamage;
        foreach (var kv in cr.byWeapon) row.byWeapon[kv.Key] = kv.Value;
        row.counters += $", pilot detonates {pilot.detonates}, kit {CrownShare32(row.byWeapon)}";
        row.counters = row.counters.TrimStart(',', ' ') + "; " + $"sweeps {cr.sweeps} ({cr.sweepHits} hit, pilot jumps {pilot.jumps}), orbs {cr.orbsFired} ({cr.orbHits} hit, parried {cr.parryHits}), mortars {cr.mortars} ({cr.mortarHits} hit), drops {cr.drops} ({cr.lavaHits} burns), charges {cr.charges} ({cr.chargeHits} hit, slams {cr.slams}, pilot dodges {pilot.dodges}), hits core {cr.coreHits} / plates {cr.plateHits} / blocked {cr.blockedHits}";
        return row;
    }

    static double CrownPct32(List<double> v, double q)
    {
        if (v.Count == 0) return double.NaN;
        var s = new List<double>(v);
        s.Sort();
        double idx = q * (s.Count - 1);
        int lo = (int)Math.Floor(idx), hi = (int)Math.Ceiling(idx);
        return s[lo] + (s[hi] - s[lo]) * (idx - lo);
    }

    sealed class CrownCampRow32
    {
        public string name;
        public double alive, lost, rate, dist;
        public bool died, killedBoss;
        public string weapons = "";
        public int mortarHits;
    }

    static CrownCampRow32 CrownCampRun32(GameConfig cfg, uint seed, int spot, bool dbg)
    {
        SurvivalSim sim = CrownSim32(cfg, seed);
        if (sim == null) return null;
        Crown32 cr = sim.crown32;
        Crown32Arena a = cr.arena;
        Vec3 o = cr.origin;
        double dt = sim.cfg.Dt;
        var spots = CrownSpots32(a, o);
        var (name, at) = spots[spot];
        var row = new CrownCampRow32 { name = name };
        sim.Respawn(at, 0);
        sim.mover.invulnerable = false;
        var si = (ScriptedInput)sim.input;
        var camper = new Crown32Camper();
        var used = new HashSet<string>();
        int clock = -1, step = 0, cap = 30 * 120;
        double hp0 = 0, prevHp = sim.health.hp, maxHp = sim.health.max, distSum = 0;
        int distN = 0;
        while (step < 60 * 120 && cr.Alive)
        {
            if (clock < 0 && cr.Hurtable) { clock = 0; hp0 = cr.HpFrac; sim.health.hp = sim.health.max; prevHp = sim.health.hp; }
            camper.Step(sim, at, step, clock >= 0);
            si.SetLook(camper.Move.yaw, camper.Move.pitch);
            si.Set(camper.Move, camper.Combat);
            sim.Step();
            step++;
            sim.hitstopLeft = 0;
            if (clock < 0) { sim.health.hp = sim.health.max; prevHp = sim.health.hp; continue; }
            if (camper.see) used.Add(camper.gun);
            distSum += camper.dist; distN++;
            row.lost += Math.Max(0, prevHp - sim.health.hp);
            prevHp = sim.health.hp;
            clock++;
            if (dbg && clock % 600 == 0) Console.WriteLine($"      t={clock * dt:0}s hp {sim.health.hp:0} boss {cr.HpFrac:0.000} beat {cr.beat} see {camper.see} gun {camper.gun} mortars {cr.mortars} ({cr.mortarHits} hit)");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead) { row.died = true; break; }
            if (clock >= cap) break;
        }
        row.killedBoss = !cr.Alive || cr.phase == "dying";
        row.alive = Math.Max(0, clock) * dt;
        row.rate = row.alive > 0 ? (hp0 - cr.HpFrac) / row.alive : 0;
        row.lost /= maxHp;
        row.dist = distN > 0 ? distSum / distN : 0;
        row.weapons = string.Join("+", used);
        row.mortarHits = cr.mortarHits;
        if (clock < 0) row.name += " (never hurtable in 60 s)";
        return row;
    }

    /// <summary>The four camp spots (world): the centre (inner disc), the rim (floor at the wall, sector 2), the high spot (ledge 0's top), the far side (across from the spawn).</summary>
    static List<(string name, Vec3 at)> CrownSpots32(Crown32Arena a, Vec3 o)
    {
        var l = new List<(string, Vec3)>();
        double sa = a.SectorAngle;
        l.Add(("centre (inner disc)", o + new Vec3(1.5, 0.05, 1.5)));
        l.Add(("rim (floor at the wall, sector 2)", o + Crown32Arena.Polar(2.5 * sa, a.p.apothem - 1.2, 0.05)));
        Vec3 ledge = a.LedgeCentre(0);
        l.Add(($"high spot (ledge top {a.p.ledgeTop} m, sector {a.p.ledgeSectors[0]})", o + new Vec3(ledge.x, ledge.y + 0.05, ledge.z)));
        l.Add(("far side (floor across from the spawn)", o + new Vec3(0, 0.05, -(a.p.apothem - 4))));
        return l;
    }

    static int Crown32Sweep(int seeds, uint first, double cap, bool dbg, bool noCamp)
    {
        GameConfig cfg = CrownCfg32();
        Crown32Params p = Crown32Params.From(cfg);
        Crown32ArenaParams ap = Crown32ArenaParams.From(cfg);
        var arena = new Crown32Arena(ap);
        Console.WriteLine($"crown32: THE CROWN (pool {p.pool}, beats at {p.beat2At} / {p.beat3At}, plates x{p.plateMult}); ring apothem {ap.apothem} m, {ap.facets} facets, {ap.sectors} drop sectors, {ap.ledgeSectors.Length} ledges, {arena.tris.Count} triangles{(crownSets32.Count > 0 ? "; --set " + string.Join(" ", crownSets32) : "")}");
        var times = new List<double>();
        var deaths = new List<double>();
        double fullSum = 0, plateSum = 0, unhitSum = 0;
        var kitAll = new Dictionary<string, double>();
        int killed = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = first + (uint)s;
            CrownRow32 r = CrownPilotRun32(cfg, seed, cap, dbg);
            if (r.killed) { killed++; times.Add(r.seconds); }
            else times.Add(cap * 2); // a pilot that did not kill counts past the cap (a p90 over the cap reads as a fail)
            deaths.Add(r.deaths);
            fullSum += r.fullShare; plateSum += r.plateShare; unhitSum += r.unhitPerMin;
            foreach (var kv in r.byWeapon) kitAll[kv.Key] = (kitAll.TryGetValue(kv.Key, out double w0) ? w0 : 0) + kv.Value;
            Console.WriteLine($"  seed {seed,3}: {(r.killed ? "KILLED " + r.seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s" : "NOT killed in " + r.seconds.ToString("0") + " s (bar " + r.hpEnd.ToString("0.00", CultureInfo.InvariantCulture) + ")"),-26} deaths {r.deaths}, full {r.fullShare * 100:0} %, plates {r.plateShare * 100:0} % of dmg, unhittable {r.unhitPerMin:0.0} s/min, parry dmg {r.parryDmg:0}; beats {r.beats}| {r.counters}");
        }
        double p10 = CrownPct32(times, 0.1), p50 = CrownPct32(times, 0.5), p90 = CrownPct32(times, 0.9);
        bool band = killed == seeds && p50 >= 25 && p50 <= 40 && p90 <= 45;
        Console.WriteLine($"crown32 sweep ({seeds} seeds from {first}): killed {killed}/{seeds}; kill time p10 {p10:0.0} s, p50 {p50:0.0} s, p90 {p90:0.0} s (band, the lead 20:10 after GG 'bosses still too tanky!': p50 25-40 s, about 30, p90 <= 45 s) {(band ? "IN BAND" : "OUT OF BAND")}; deaths mean {CrownAvg32(deaths):0.00} (max {CrownMax32(deaths):0}); full-damage share {fullSum / seeds * 100:0.0} %; plates {plateSum / seeds * 100:0} % of the damage; unhittable {unhitSum / seeds:0.00} s per minute");
        // by weapon: the KESTREL (taps + charge), the PULSE (punches + parries), the THUNDERCLAP (pellets + slug), the NAILGUN (nails + detonate)
        var byGun = new Dictionary<string, double>();
        foreach (var kv in kitAll)
        {
            string g = kv.Key == "revolver" || kv.Key == "charge" ? "KESTREL" : kv.Key == "gauntlet" || kv.Key == "parry" ? "PULSE" : kv.Key == "shotgun" || kv.Key == "slug" ? "THUNDERCLAP" : kv.Key == "nailgun" || kv.Key == "detonate" ? "NAILGUN" : kv.Key;
            byGun[g] = (byGun.TryGetValue(g, out double w1) ? w1 : 0) + kv.Value;
        }
        double kitTop = 0, kitSum = 0;
        foreach (var kv in byGun) kitSum += kv.Value;
        foreach (var kv in byGun) kitTop = Math.Max(kitTop, kitSum > 0 ? kv.Value / kitSum : 0);
        Console.WriteLine($"crown32 kit share (all seeds' damage): by weapon {CrownShare32(byGun)}; by answer {CrownShare32(kitAll)}; the largest weapon {kitTop * 100:0} % (the lead's bar: about 45 % or under) {(kitTop <= 0.46 ? "OK" : "OVER")}");
        if (noCamp) return band ? 0 : 1;
        double pilotRate = p50 > 0 ? 1.0 / p50 : 0;
        Console.WriteLine($"crown32 camp table (pilot p50 {p50:0.0} s = {pilotRate * 100:0.00} % of the bar / s; a third = {pilotRate / 3 * 100:0.00} % / s; seed {first})");
        Console.WriteLine($"  {"camper",-44} {"alive",9} {"hp lost",8} {"dmg %/s",8} {"dist",6} {"mortar",6} {"weapons",-22} verdict (strict R2 rate)");
        int camps = 0;
        var spots = CrownSpots32(arena, new Vec3(0, 0, 0));
        for (int i = 0; i < spots.Count; i++)
        {
            CrownCampRow32 r = CrownCampRun32(cfg, first, i, dbg);
            if (r == null) { Console.WriteLine($"  spot {i}: rig failed"); camps++; continue; }
            bool loses = (r.died || r.lost >= 0.5) && !r.killedBoss;
            if (!loses) camps++;
            bool strict = loses && r.rate < pilotRate / 3;
            Console.WriteLine($"  {r.name,-44} {(r.died ? "DIED " : "") + r.alive.ToString("0.0", CultureInfo.InvariantCulture) + " s",9} {r.lost * 100,7:0} % {r.rate * 100,8:0.00} {r.dist,5:0} m {r.mortarHits,6} {r.weapons,-22} {(loses ? "LOSES" : r.killedBoss ? "CAMPS (killed the boss)" : "CAMPS (safe)")} ({(strict ? "under a third" : "rate over a third")})");
        }
        Console.WriteLine($"crown32 camp {(camps == 0 ? "PASS" : "RED")}: {spots.Count - camps} of {spots.Count} campers lose");
        return band && camps == 0 ? 0 : 1;
    }

    /// <summary>`crown32walk [--seed n]` (the lead, 19:07): THE CROWN inside F's level 5 (Design/levels/n32-spire.json) with the build's own rows (Design/unity-config.json as
    /// it is: boss5 and level5 on): a CAMPAIGN run, DOORFREE (every door open), the player put at v8's exit and walked forward (-z) through v8's door into v9. PASS: the campaign
    /// enters v9, BossRoomBegin("crown") puts the player at the ring's spawn (design.n32.boss5.room.origin + spawnRadius), the pilot kills it (the campaign's own death retry,
    /// Campaign24.Retry -> BeginBoss -> Crown32Begin, handles deaths), v9 clears (roomClear) and the level ends (layerClear: v9 is the last room, so its `exit` is never walked).</summary>
    static int CrownWalk32(uint seed, bool dbg, bool die)
    {
        string design = File.ReadAllText(Path.Combine(Root, "..", "Design", "unity-config.json"));
        GameConfig cfg = GameConfig.FromText(File.ReadAllText(Path.Combine(Root, "config.json")), design);
        SurvivalMap map = CampaignMapFrom("Design/levels/n32-spire.json", cfg, out var json);
        var level = LevelDef.From(json);
        int v8 = level.rooms.FindIndex(r => r.id == "v8"), v9 = level.rooms.FindIndex(r => r.id == "v9");
        if (v8 < 0 || v9 < 0) { Console.WriteLine("crown32walk RED: n32-spire has no v8 / v9"); return 1; }
        RoomDef r8 = level.rooms[v8], r9 = level.rooms[v9];
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        try
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            N29Blocks n29 = N29Blocks.From(cfg);
            n29.doorFree = true;
            sim.n29Override = n29;
            sim.setup.mode = Campaign24.Mode;
            sim.StartRun(seed, null, null);
            Console.WriteLine($"crown32walk: n32-spire ({level.rooms.Count} rooms), the run's NIGHT32 blocks: {sim.n32}; v9 boss \"{r9.boss}\", kind {r9.kind}, spawn ({r9.spawn.x}, {r9.spawn.y}, {r9.spawn.z}); v8 exit ({r8.exit.x}, {r8.exit.y}, {r8.exit.z})");
            if (!sim.n32.boss5 || !sim.n32.level5) { Console.WriteLine("crown32walk RED: boss5 / level5 off in the design rows"); return 1; }
            for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; sim.Step(); }
            if (sim.campaign == null) { Console.WriteLine("crown32walk RED: no campaign in the run"); return 1; }
            sim.campaign.SetDoorFree29(true);
            foreach (string g in new[] { "gauntlet", "shotgun", "nailgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
            var si = (ScriptedInput)sim.input;
            string cleared = "", layer = "";
            double dt = sim.cfg.Dt;
            int step = 0;
            sim.events.On<RoomEvent>("roomClear", e => cleared += $"{e.id}@{step * dt:0.0}s ");
            sim.events.On<RoomEvent>("layerClear", e => layer = $"{e.id}@{step * dt:0.0}s");
            // walk in: from v8's exit straight on (-z) through v8's door into v9
            sim.Respawn(new Vec3(r9.spawn.x, r8.exit.y + 0.05, r8.exit.z), 0);
            Vec3 door0 = sim.mover.position;
            int walk = 0;
            while (walk < 12 * 120 && (sim.crown32 == null || !sim.crown32.Alive))
            {
                si.SetLook(0, 0);
                si.Set(new InputFrame { yaw = 0, moveZ = 1 }, new CombatInput { weapon = -1 });
                sim.Step();
                sim.health.hp = sim.health.max;
                walk++;
            }
            if (sim.crown32 == null || !sim.crown32.Alive) { Console.WriteLine($"crown32walk RED: walked {walk * dt:0.0} s from ({door0.x:0.0}, {door0.y:0.0}, {door0.z:0.0}) and the crown never began (campaign room {sim.campaign.room}, at ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0}))"); return 1; }
            Vec3 at = sim.mover.position, want = sim.Crown32Spawn;
            double off = Vec3.DistanceTo(at, want);
            string roomId = sim.campaign.room >= 0 && sim.campaign.room < level.rooms.Count ? level.rooms[sim.campaign.room].id : "?";
            Console.WriteLine($"  walked in: {walk * dt:0.00} s from v8's exit ({door0.x:0.0}, {door0.y:0.0}, {door0.z:0.0}); the campaign is in room {roomId}; the player at ({at.x:0.00}, {at.y:0.00}, {at.z:0.00}), the ring's spawn ({want.x:0.00}, {want.y:0.00}, {want.z:0.00}): {off:0.00} m off; ring origin ({sim.crown32Origin.x}, {sim.crown32Origin.y}, {sim.crown32Origin.z}), F's crown32.center (0, 0, -204) is not used (the ring is its own room, as THE LANTERN's)");
            // stand on the ring's floor? one second still on the spawn
            for (int s = 0; s < 120; s++) { si.Set(new InputFrame { yaw = 0 }, new CombatInput { weapon = -1 }); sim.Step(); }
            double floorY = sim.mover.position.y - sim.crown32Origin.y;
            Console.WriteLine($"  after 1 s on the spawn: feet {floorY:0.00} m over the ring's floor, grounded {sim.mover.grounded}, the crown {sim.crown32.phase} beat {sim.crown32.beat}");
            var pilot = new Crown32Pilot(seed);
            Crown32 cr = sim.crown32;
            int deaths0 = sim.campaign.deaths, cap = 240 * 120, dieBeat = 0;
            double dieBar = -1, afterDie = -1;
            Vec3 dieAt = default;
            while (step < cap && !sim.campaign.finished)
            {
                if (cr.Alive)
                {
                    pilot.Step(sim, step);
                    si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                    si.Set(pilot.Move, pilot.Combat);
                }
                else si.Set(new InputFrame { yaw = 0 }, new CombatInput { weapon = -1 });
                sim.Step();
                sim.hitstopLeft = 0;
                step++;
                if (die && step == 25 * 120 && cr.Alive)
                {
                    dieBar = cr.HpFrac;
                    dieBeat = cr.beat;
                    sim.health.Damage(sim.health.max * 2, "boss"); // --die: one death mid-fight (the campaign's own retry path)
                }
                if (die && dieBeat > 0 && afterDie < 0 && sim.campaign.deaths > deaths0 && cr.retries > 0) { afterDie = cr.HpFrac; dieAt = sim.mover.position; }
                if (dbg && step % 1200 == 0) Console.WriteLine($"      t={step * dt:0}s crown {cr.phase} beat {cr.beat} hp {cr.HpFrac:0.00}; player hp {sim.health.hp:0}, campaign room {sim.campaign.room}, deaths {sim.campaign.deaths - deaths0}");
            }
            bool killed = !cr.Alive;
            bool pass = off < 0.6 && Math.Abs(floorY) < 0.2 && killed && cleared.Contains("v9") && layer != "";
            if (die)
            {
                double start = dieBeat == 1 ? 1.0 : dieBeat == 2 ? cr.p.beat2At : cr.p.beat3At;
                bool ok = afterDie >= start - 0.01 && Vec3.DistanceTo(dieAt, sim.Crown32Spawn) < 0.6;
                Console.WriteLine($"  --die at 25 s: beat {dieBeat}, bar {dieBar:0.00} -> after the campaign's retry {afterDie:0.00} (the beat's start {start:0.00}), respawned {Vec3.DistanceTo(dieAt, sim.Crown32Spawn):0.00} m from the ring's spawn: {(ok ? "OK" : "WRONG")}");
                pass &= ok;
            }
            Console.WriteLine($"  the fight: {(killed ? "KILLED" : "NOT killed, bar " + cr.HpFrac.ToString("0.00", CultureInfo.InvariantCulture))} in {step * dt:0.0} s; campaign deaths {sim.campaign.deaths - deaths0} (its own retry: Crown32Begin, retries {cr.retries}); rooms cleared {cleared}; layerClear {(layer == "" ? "NONE" : layer)}; finished {sim.campaign.finished}");
            Console.WriteLine($"crown32walk {(pass ? "PASS" : "RED")}: enter v9 from v8's door -> the ring's spawn, the fight, v9 clear, level end");
            return pass ? 0 : 1;
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
    }

    /// <summary>"shotgun 31 %, charge 22 %, ..." (largest first).</summary>
    static string CrownShare32(Dictionary<string, double> d)
    {
        double sum = 0;
        foreach (var kv in d) sum += kv.Value;
        var l = new List<KeyValuePair<string, double>>(d);
        l.Sort((a, b) => b.Value.CompareTo(a.Value));
        var parts = new List<string>();
        foreach (var kv in l) parts.Add($"{kv.Key} {(sum > 0 ? kv.Value / sum * 100 : 0):0} %");
        return string.Join(", ", parts);
    }

    static double CrownAvg32(List<double> v) { double s = 0; foreach (double x in v) s += x; return v.Count > 0 ? s / v.Count : 0; }
    static double CrownMax32(List<double> v) { double m = 0; foreach (double x in v) m = Math.Max(m, x); return m; }
}
