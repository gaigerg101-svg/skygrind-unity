// NIGHT37 builder C (crown37): THE CROWN's descent, headless. One entry, N37Crown (declared and called in Program.cs).
//   crown37 [arena|jumps|band|all] [--seeds N] [--cap S] [--trace]
//     arena  n37-spire with the crown37 row on: the pieces (count by kind), the layer; THE CARRY: the player stands still on an orbiting platform
//            for 10 s: the carry error (the rider's offset from the platform's top centre drifting; want 0), still on it at the end;
//            the stones of room v9 drop 2 s after a landing and come back.
//     jumps  the real sim's jump numbers on the spire's bridge (open air over the chasm, the row off: no summit): a run jump and a jump-dash
//            (dash, then jump inside the dash) from 8 m/s, the horizontal metres reached at 0, 3, 6, 9 m of drop; then every designed gap
//            (room v9's stones, v10's ring gap, the gallery's corners, the checkpoint ledge, the bridge to the orbits) against them:
//            PASS when the jump-dash clears it with 1 m to spare (and, for the rehearsal gaps, the run jump does not).
//     band   (round 2) the descending pilot over seeds 1..N.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N37Crown(string cmd, string[] args, ref int code)
    {
        if (cmd != "crown37") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 90);
        Trace37C = Array.IndexOf(args, "--trace") >= 0;
        int fails = 0;
        if (what == "arena" || what == "all") fails += Arena37C();
        if (what == "jumps" || what == "all") fails += Jumps37C();
        if (what == "walkin" || what == "all") fails += Walkin37C();
        if (what == "attacks" || what == "all") fails += Attacks37C();
        if (what == "band" || what == "all") fails += Band37C(seeds, cap);
        Console.WriteLine(fails == 0 ? "crown37: GREEN" : $"crown37: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static bool Trace37C;
    const string Spire37Path = "Design/levels/n37-spire.json";

    static SurvivalSim Sim37C(uint seed, bool on, bool campaign = false)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Spire37Path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.n37Override = on ? new N37Blocks { crown37 = true, boss37 = true } : N37Blocks.Off;
        sim.setup.mode = campaign ? Campaign24.Mode : RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    static string F37(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);

    static int Arena37C()
    {
        int fails = 0;
        SurvivalSim sim = Sim37C(1, true);
        Orbit37 o = sim.orbit37;
        if (o == null || o.Count == 0) { Console.WriteLine("arena: FAIL no pieces (orbit37 null or empty)"); return 1; }
        var byKind = new Dictionary<Piece37Kind, int>();
        foreach (Piece37 q in o.pieces) byKind[q.kind] = (byKind.TryGetValue(q.kind, out int n) ? n : 0) + 1;
        var parts = new List<string>();
        foreach (var kv in byKind) parts.Add($"{kv.Key} {kv.Value}");
        bool spire = sim.spire37Row != null && sim.boss37WalkInHere;
        Console.WriteLine($"arena: {o.Count} pieces ({string.Join(", ", parts)}), layer boxes {o.layer.Count}, ring targets {sim.hoops37.Count}, spire row {sim.spire37Row != null}, walk-in here {sim.boss37WalkInHere}: {Pass36(o.Count <= 50 && spire && sim.hoops37.Count == 1)} (under 50 boxes: BOSSES.md 'keep each moving layer under ~50')");
        if (!(o.Count <= 50 && spire && sim.hoops37.Count == 1)) fails++;

        // THE CARRY: stand still on orbit platform orb00 for 10 s
        int idx = o.pieces.FindIndex(q => q.id == "orb00");
        Piece37 p0 = o.pieces[idx];
        var si = (ScriptedInput)sim.input;
        sim.Respawn(new Vec3(p0.pos.x + 0.5, p0.pos.y + 0.02, p0.pos.z - 0.5), 0);
        for (int s = 0; s < 30; s++) { si.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
        Vec3 off0 = sim.mover.position - p0.pos;
        int onSteps = 0, steps = MovementMath.StepsFor(10, sim.step);
        double travelled = 0;
        Vec3 last = p0.pos;
        for (int s = 0; s < steps; s++)
        {
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Step();
            if (o.standing == idx) onSteps++;
            travelled += Vec3.DistanceTo(p0.pos, last);
            last = p0.pos;
        }
        Vec3 off1 = sim.mover.position - p0.pos;
        double drift = JsMath.Hypot(off1.x - off0.x, off1.z - off0.z);
        bool carryOk = o.carryErrorMax < 0.01 && drift < 0.05 && onSteps == steps;
        Console.WriteLine($"carry: orb00 moved {F37(travelled)} m in 10 s ({F37(o.p.orbitSpeed)} m/s on a {F37(p0.radius)} m ring); the rider on it {onSteps}/{steps} steps; carry error max {o.carryErrorMax.ToString("0.0000", CultureInfo.InvariantCulture)} m, offset drift {drift.ToString("0.0000", CultureInfo.InvariantCulture)} m, feet y {F37(sim.mover.position.y)} vs top {F37(p0.pos.y)}: {Pass36(carryOk)}");
        if (!carryOk) fails++;

        // THE STONES (room v9): a landing starts the drop; it is down after fallDelay and back after fallReturn
        int si1 = o.pieces.FindIndex(q => q.kind == Piece37Kind.Fall);
        Piece37 st = o.pieces[si1];
        sim.Respawn(new Vec3(st.pos.x, st.pos.y + 0.02, st.pos.z), 0);
        int tellAt = -1, downAt = -1, backAt = -1;
        for (int s = 0; s < MovementMath.StepsFor(8, sim.step); s++)
        {
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Step();
            if (tellAt < 0 && st.state == Piece37State.Tell) tellAt = s;
            if (downAt < 0 && st.state == Piece37State.Down) downAt = s;
            if (downAt >= 0 && backAt < 0 && st.state == Piece37State.Up) backAt = s;
        }
        double tellS = (downAt - tellAt) * sim.step, backS = (backAt - downAt) * sim.step;
        bool stoneOk = tellAt >= 0 && Math.Abs(tellS - o.p.fallDelay) < 0.02 && Math.Abs(backS - o.p.fallReturn) < 0.02;
        Console.WriteLine($"stones: tell from step {tellAt}, down after {F37(tellS)} s (want {F37(o.p.fallDelay)}), back after {F37(backS)} s (want {F37(o.p.fallReturn)}), the player fell to y {F37(sim.mover.position.y)}: {Pass36(stoneOk)}");
        if (!stoneOk) fails++;

        // THE RING TARGET (room v10): a jump-dash from the edge through it counts a pass
        Hoop37Target h = sim.hoops37[0];
        var jd = Fly37(sim, new Vec3(-10, 0, -218), 0, -221.0, 1, 60);
        Console.WriteLine($"ring target: a jump-dash from the v10 edge: passes {h.passes}, landed at z {F37(jd.landZ)} y {F37(jd.landY)} (the far floor starts at z -231): {Pass36(h.passes == 1 && jd.landZ < -231 && jd.landY > -0.5)}");
        if (!(h.passes == 1 && jd.landZ < -231 && jd.landY > -0.5)) fails++;
        return fails;
    }

    /// <summary>Run north (yaw 0) from `from` to z `edge` at the walk speed, then: kind 0 a run jump, 1 a jump-dash (dash at the edge, jump 3 steps
    /// later), each held `holdSteps`; the flight's samples (horizontal metres from the take-off, drop) until grounded or 3 s.</summary>
    static (List<(double h, double drop)> path, double landZ, double landY, bool dashJumped) Fly37(SurvivalSim sim, Vec3 from, double yawDeg, double edge, int kind, int holdSteps)
    {
        var si = (ScriptedInput)sim.input;
        sim.Respawn(from, yawDeg);
        double yaw = yawDeg * Math.PI / 180;
        bool dj = false;
        Action<MoverEvent> on = e => { if (e.type == MoverEventType.DashJump) dj = true; };
        sim.mover.Events += on;
        int s = 0;
        for (; s < 600 && sim.mover.position.z > edge + (kind == 1 ? 0.8 : 0); s++)
        {
            si.Set(new InputFrame { moveZ = 1, yaw = yaw, sprint = new Button { held = true } }, new CombatInput { weapon = -1 });
            sim.Step();
        }
        Vec3 take = sim.mover.position;
        var path = new List<(double, double)>();
        int t0 = s;
        for (int k = 0; k < 360; k++)
        {
            var f = new InputFrame { moveZ = 1, yaw = yaw, sprint = new Button { held = true } };
            if (kind == 0 && k == 0) f.jump = new Button { held = true, pressed = true };
            else if (kind == 0 && k < holdSteps) f.jump = new Button { held = true };
            if (kind == 1 && k == 0) f.dash = new Button { held = true, pressed = true };
            if (kind == 1 && k == 3) f.jump = new Button { held = true, pressed = true };
            else if (kind == 1 && k > 3 && k < 3 + holdSteps) f.jump = new Button { held = true };
            si.Set(f, new CombatInput { weapon = -1 });
            sim.Step();
            if (kind == 1 && k == 3) take = sim.mover.position;
            Vec3 m = sim.mover.position;
            path.Add((JsMath.Hypot(m.x - take.x, m.z - take.z), take.y - m.y));
            if (k > 8 && sim.mover.grounded) break;
        }
        sim.mover.Events -= on;
        Vec3 end = sim.mover.position;
        return (path, end.z, end.y, dj);
    }

    /// <summary>The horizontal metres when the flight first falls to `drop` m under the take-off (the last sample's when it never does).</summary>
    static double At37(List<(double h, double drop)> path, double drop)
    {
        bool rose = false;
        for (int i = 1; i < path.Count; i++)
        {
            if (path[i].drop < -0.01) rose = true;
            if ((rose || i > 4) && path[i - 1].drop < drop && path[i].drop >= drop)
            {
                double u = (drop - path[i - 1].drop) / Math.Max(1e-9, path[i].drop - path[i - 1].drop);
                return path[i - 1].h + (path[i].h - path[i - 1].h) * u;
            }
        }
        return -1; // never fell that far (landed or hit something)
    }

    static int Jumps37C()
    {
        int fails = 0;
        // the bridge (row off: no summit, open air for 20 m to the core; 74 m down)
        SurvivalSim sim = Sim37C(1, false);
        var run = Fly37(sim, new Vec3(-10, 0, -256), 0, -264.6, 0, 40);
        sim = Sim37C(1, false);
        var dash = Fly37(sim, new Vec3(-10, 0, -256), 0, -264.6, 1, 60);
        double[] drops = { 0, 3, 6, 9 };
        string Row(List<(double, double)> p) { var l = new List<string>(); foreach (double d in drops) l.Add($"{d:0} m drop {F37(At37(p, d))} m"); return string.Join(", ", l); }
        double apexJ = 0, apexD = 0;
        foreach (var q in run.path) apexJ = Math.Max(apexJ, -q.drop);
        foreach (var q in dash.path) apexD = Math.Max(apexD, -q.drop);
        Console.WriteLine($"jumps (the real sim, from 8 m/s): run jump apex {F37(apexJ)} m: {Row(run.path)}");
        Console.WriteLine($"                                  jump-dash (dash-jumped {dash.dashJumped}) apex {F37(apexD)} m: {Row(dash.path)}");
        // every designed gap: (name, horizontal m, drop m, rehearsal: the run jump must fall short)
        var gaps = new (string name, double h, double drop, bool teach)[]
        {
            ("v9 entry -> stone 1", 9, 0, true), ("v9 stone 1 -> stone 2", 9.22, 0, true), ("v9 stone 2 -> exit", 9, 0, true),
            ("v10 the ring's gap", 10, 0, true), ("v11 bridge stub -> orbit ring 1 (when under)", 11, 8, false),
            ("v11 leg-2 ledge -> gallery slab 1", 5.1, 3, false), ("v11 gallery corner (slab to slab)", 7.07, 3, false),
        };
        foreach (var g in gaps)
        {
            // a flight that hit the core before it fell that far reaches at least its flat reach (a lower landing comes later on the same path)
            double dReach = Math.Max(At37(dash.path, g.drop), At37(dash.path, 0)), jReach = Math.Max(At37(run.path, g.drop), At37(run.path, 0));
            bool ok = dReach >= g.h + 1 && (!g.teach || jReach < g.h);
            Console.WriteLine($"  gap {g.name}: {F37(g.h)} m at {g.drop:0} m drop: jump-dash reaches {F37(dReach)} m, run jump {F37(jReach)} m: {Pass36(ok)}{(g.teach ? " (rehearsal: the run jump must fall short)" : "")}");
            if (!ok) fails++;
        }
        return fails;
    }

    /// <summary>The walk-in on the spire's bridge: the begin owned by crown37 (no Respawn), the reveal (no damage), the summit's shatter (a RED tell,
    /// then down), a player who stands still falls: to an orbit or past leg 1's floor (the fall rule: the checkpoint, -fallDamage).</summary>
    static int Walkin37C()
    {
        int fails = 0;
        SurvivalSim sim = Sim37C(1, true);
        var si = (ScriptedInput)sim.input;
        Vec3 stand = new Vec3(-10, 0, -268); // on the bridge's far half (a summit piece: it shatters)
        sim.Respawn(stand, 0);
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        sim.Step();
        Vec3 before = sim.mover.position;
        var log = new List<string>();
        int stepNo = 0;
        sim.events.On<Crown37Event>("crown37Shatter", e => log.Add($"{stepNo * sim.step:0.00} s shatter {e.stage}"));
        sim.events.On<Piece37Event>("crown37Piece", e => { if (e.kind == "summit" && e.stage == "drop") log.Add($"{stepNo * sim.step:0.00} s summit {e.id} down"); });
        sim.events.On<Crown37Event>("crown37Fall", e => log.Add($"{stepNo * sim.step:0.00} s FALL restart leg {e.leg}"));
        sim.events.On<Boss37Event>("boss37Leg", e => log.Add($"{stepNo * sim.step:0.00} s boss37Leg {e.stage} leg {e.leg}"));
        bool ok = sim.BossRoomBegin("crown37", out Vec3 spawn, out double yaw);
        Crown37 c = sim.crown37;
        bool owned = ok && c != null && c.Alive && ReferenceEquals(sim.BossRoomSystem("crown37"), c);
        double moved = Vec3.DistanceTo(before, sim.mover.position);
        Console.WriteLine($"walkin: begin {ok}, crown37 owns it {owned}, walk-in here {sim.boss37WalkInHere}, feet moved by the begin {F37(moved)} m, torch owned {sim.loadout.owned.Contains("torch")}, checkpoint ({F37(spawn.x)}, {F37(spawn.y)}, {F37(spawn.z)})");
        if (!owned || moved > 0.01) fails++;
        double hp0 = sim.health.hp;
        int revealSteps = MovementMath.StepsFor(c.p.revealSeconds, sim.step);
        double minY = 0;
        for (; stepNo < revealSteps + MovementMath.StepsFor(4, sim.step); stepNo++)
        {
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Step();
            sim.hitstopLeft = 0;
            minY = Math.Min(minY, sim.mover.position.y);
            if (stepNo == revealSteps - 2 && sim.health.hp < hp0) { Console.WriteLine("walkin: FAIL damage in the reveal"); fails++; }
        }
        foreach (string l in log) Console.WriteLine("  " + l);
        Console.WriteLine($"walkin: after the reveal + 4 s standing still: feet y {F37(sim.mover.position.y)} (lowest {F37(minY)}), falls {sim.crown37Falls}, hp {F37(sim.health.hp)}/{F37(hp0)}, crown at ({F37(c.centre.x)}, {F37(c.centre.y)}, {F37(c.centre.z)}) state {c.brain.state}, tells Y/R {c.brain.yellowTells}/{c.brain.redTells}");
        bool shattered = log.Exists(l => l.Contains("summit") && l.Contains("down"));
        if (!shattered) { Console.WriteLine("walkin: FAIL the summit never shattered"); fails++; }
        Console.WriteLine($"walkin: {Pass36(fails == 0)}");
        return fails;
    }

    // ---- round 3: each attack in isolation, and leg 3's finisher -------------------------------------------------------------------------------

    /// <summary>A fight begun from the bridge stub (static: no habit), the reveal run out, `force` queued as the first pick; returns at the step its
    /// tell starts (or after 8 s). The player is idle (ScriptedInput) unless `each` moves him.</summary>
    static (SurvivalSim sim, Crown37 c, bool told) Forced37C(string force, Vec3 feet, Action<SurvivalSim> each = null, uint seed = 1, string onPiece = null)
    {
        SurvivalSim sim = Sim37C(seed, true);
        var si = (ScriptedInput)sim.input;
        sim.Respawn(new Vec3(-10, 0.02, -258), 0);
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        sim.Step();
        sim.BossRoomBegin("crown37", out _, out _);
        Crown37 c = sim.crown37;
        c.p.openers[0] = new List<string>(); // the openers off: the attack under test is the only forced one
        c.p.openers[1] = new List<string>();
        c.p.openers[2] = new List<string>();
        int max = MovementMath.StepsFor(8, sim.step);
        bool placed = false;
        for (int s = 0; s < max; s++)
        {
            if (c.brain.state == Boss37State.Rest && !placed) { c.brain.Force(force); sim.Respawn(onPiece != null ? OrbTop37(sim, onPiece) : feet, 0); placed = true; }
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            each?.Invoke(sim);
            sim.Step();
            sim.hitstopLeft = 0;
            if (c.brain.state == Boss37State.Tell && c.brain.Current != null && c.brain.Current.id == force) return (sim, c, true);
        }
        return (sim, c, false);
    }

    /// <summary>Step the sim `seconds` with idle input (or `each`), stopping early when `until` holds.</summary>
    static void Tick37C(SurvivalSim sim, double seconds, Action<SurvivalSim, int> each = null, Func<bool> until = null)
    {
        var si = (ScriptedInput)sim.input;
        int n = MovementMath.StepsFor(seconds, sim.step);
        for (int s = 0; s < n; s++)
        {
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            each?.Invoke(sim, s);
            sim.Step();
            sim.hitstopLeft = 0;
            if (until != null && until()) return;
        }
    }

    static Vec3 OrbTop37(SurvivalSim sim, string id)
    {
        Piece37 q = sim.orbit37.pieces.Find(x => x.id == id);
        return new Vec3(q.pos.x, q.pos.y + 0.02, q.pos.z);
    }

    static int Attacks37C()
    {
        int fails = 0;
        void Line(string name, bool ok, string measured) { Console.WriteLine($"  {name}: {measured}: {Pass36(ok)}"); if (!ok) fails++; }

        // 1. THE BLADE RING (RED 1.2 s): standing on an orbit fragment at its height: hit; jump + double jump over the cut: no hit; dropped to the
        //    ring below during the tell: no hit
        foreach (string mode in new[] { "stay", "jump", "drop" })
        {
            var (sim, c, told) = Forced37C("bladeRing", default, null, 1, "orb00");
            int piece = sim.orbit37.pieces.FindIndex(q => q.id == "orb00");
            int tellSteps = c.brain.stateLeft;
            double hp0 = sim.health.hp;
            int k = 0;
            Tick37C(sim, 2.0, (s, i) =>
            {
                var si = (ScriptedInput)s.input;
                double left = c.brain.state == Boss37State.Tell ? c.brain.stateLeft * s.step : -1;
                if (mode == "jump")
                {
                    if (k == 0 && left >= 0 && left <= 0.12) { k = 1; si.Set(new InputFrame { jump = new Button { held = true, pressed = true } }, new CombatInput { weapon = -1 }); }
                    else if (k > 0) { k++; if (k == 24) si.Set(new InputFrame { jump = new Button { held = true, pressed = true } }, new CombatInput { weapon = -1 }); else if (k < 30) si.Set(new InputFrame { jump = new Button { held = true } }, new CombatInput { weapon = -1 }); }
                }
                if (mode == "drop" && k == 0 && left >= 0 && left <= 0.5) { k = 1; s.Respawn(OrbTop37(s, "orb20"), 0); }
            });
            bool want = mode == "stay";
            Line($"bladeRing {mode}", told && (c.bladeHits > 0) == want, $"tell {F37(tellSteps * sim.step)} s RED, blade hits {c.bladeHits} (want {(want ? 1 : 0)}), hp {F37(sim.health.hp)}/{F37(hp0)}");
        }

        // 2. THE GEM SHARD (YELLOW 1.0 s): punched back as it comes in reach: a parry (stagger drained by parryStagger, the shard flies home);
        //    not punched: it hits
        foreach (bool punch in new[] { true, false })
        {
            var (sim, c, told) = Forced37C("gemShard", new Vec3(-10, 0.02, -258));
            double stag0 = c.brain.stagger;
            int tell = c.brain.stateLeft;
            Tick37C(sim, 3.0, (s, i) =>
            {
                if (!punch) return;
                var si = (ScriptedInput)s.input;
                Vec3 eye = s.Eye;
                foreach (Crown37Shard sh in c.shards)
                {
                    if (!sh.alive || sh.back) continue;
                    Vec3 dv = sh.pos - eye;
                    double yaw = JsMath.Atan2(-dv.x, -dv.z), pitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
                    si.SetLook(yaw, pitch);
                    bool reach = dv.Length < 2.6;
                    si.Set(new InputFrame { yaw = yaw, pitch = pitch }, new CombatInput { weapon = -1, punch = new Button { held = reach, pressed = reach } });
                }
            }, () => c.parries > 0 || c.shardHits > 0);
            if (punch) Line("gemShard parried", told && c.parries == 1 && c.brain.stagger <= stag0 - c.p.parryStagger + 1e-9, $"tell {F37(tell * sim.step)} s YELLOW, parries {c.parries}, stagger {F37(stag0)} -> {F37(c.brain.stagger)}, shard hits {c.shardHits}");
            else Line("gemShard not parried", told && c.shardHits == 1, $"shard hits {c.shardHits}");
        }

        // 3. THE BEAM CUT (RED 1.4 s): the piece the Crown offers (the one he heads for) tells RED >= 1.0 s, drops, and the Crown offers another
        {
            var (sim, c, told) = Forced37C("beamCut", new Vec3(-10, 0.02, -287));
            int offered = c.passTarget, cut = -1;
            for (int i = 0; i < sim.orbit37.pieces.Count; i++) if (sim.orbit37.pieces[i].state == Piece37State.Tell && sim.orbit37.pieces[i].cause == "cut") cut = i;
            int tellSteps = 0;
            Tick37C(sim, 2.5, (s, i) => { if (cut >= 0 && s.orbit37.pieces[cut].state == Piece37State.Tell) tellSteps++; });
            bool down = cut >= 0 && sim.orbit37.pieces[cut].state == Piece37State.Down;
            Line("beamCut", told && cut >= 0 && cut == offered && tellSteps * sim.step >= 1.0 && down && c.passTarget != cut,
                $"offered piece {offered}, cut piece {cut} ({(cut >= 0 ? sim.orbit37.pieces[cut].id : "-")}), RED for {F37(tellSteps * sim.step)} s, down {down}, the offer after: {c.passTarget}");
        }

        // 4. THE HABIT DROP: one fragment stood on > 3 s tells RED (habitTellSeconds) and drops; the bridge stub (a checkpoint) never does
        {
            var (sim, c, _) = Forced37C("gemShard", new Vec3(-10, 0.02, -258));
            c.shards.Clear();
            sim.Respawn(OrbTop37(sim, "orb00"), 0);
            int idx = sim.orbit37.pieces.FindIndex(q => q.id == "orb00");
            int tellAt = -1, downAt = -1, step = 0;
            Tick37C(sim, 5.5, (s, i) =>
            {
                c.shards.Clear();
                step = i;
                Piece37 q = s.orbit37.pieces[idx];
                if (tellAt < 0 && q.state == Piece37State.Tell) tellAt = i;
                if (downAt < 0 && q.state == Piece37State.Down) downAt = i;
            });
            Line("habit drop", tellAt >= 0 && downAt > tellAt && Math.Abs(tellAt * sim.step - c.p.habitStandSeconds) < 0.1 && Math.Abs((downAt - tellAt) * sim.step - c.p.habitTellSeconds) < 0.02,
                $"orb00 tells at {F37(tellAt * sim.step)} s on it (want {F37(c.p.habitStandSeconds)}), down {F37((downAt - tellAt) * sim.step)} s later (want {F37(c.p.habitTellSeconds)}), habit drops {c.habitDrops}");
        }

        // 5. THE LOW FIRE: airborne > 6 s forces lowFire (RED), its shard aimed under the feet
        {
            var (sim, c, _) = Forced37C("gemShard", new Vec3(-10, 0.02, -258));
            Vec3 hang = new Vec3(c.axis.x + 20, -4, c.axis.z); // in the chasm's air, clear of every piece
            double firstLow = -1, shardY = 0, feetY = 0, forcedAt = -1;
            int startStep = c.stepNo;
            Tick37C(sim, 9.0, (s, i) =>
            {
                c.shards.Clear();
                s.mover.position = hang;
                s.mover.velocity = new Vec3(0, 0, 0);
                s.mover.grounded = false;
                if (forcedAt < 0 && c.airForces > 0) forcedAt = (c.airForceStep - startStep) * s.step;
                if (forcedAt >= 0 && firstLow < 0 && c.brain.state == Boss37State.Tell && c.brain.Current?.id == "lowFire") firstLow = i * s.step;
            }, () => firstLow >= 0);
            Tick37C(sim, 1.5, (s, i) =>
            {
                s.mover.position = hang;
                s.mover.velocity = new Vec3(0, 0, 0);
                foreach (Crown37Shard sh in c.shards) if (sh.red && shardY == 0) { shardY = sh.vel.y; feetY = hang.y; }
            });
            Line("airborne low fire", forcedAt >= c.p.airborneSeconds - 0.05 && forcedAt <= c.p.airborneSeconds + 0.1 && firstLow >= forcedAt && firstLow <= forcedAt + 3, $"the habit forced lowFire at {F37(forcedAt)} s airborne (want {F37(c.p.airborneSeconds)} s), its tell at {F37(firstLow)} s, its shard's vertical speed {F37(shardY)} m/s (aimed {F37(c.p.lowDrop)} m under the feet)");
        }

        // 6. LEG 3's FINISHER: the KESTREL's charged shot (hold, release) through the ring's opening ends the fight
        {
            SurvivalSim sim = Sim37C(1, true);
            var si = (ScriptedInput)sim.input;
            sim.Respawn(new Vec3(-10, 0.02, -258), 0);
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Step();
            sim.BossRoomBegin("crown37", out _, out _);
            Crown37 c = sim.crown37;
            if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
            Tick37C(sim, 2.5);
            c.Apply(c.body.hp - 2 * c.p.gemHp, "test"); // the bar to 2 gems: legs 2 then 3 (each change's 1.5 s)
            sim.Respawn(sim.spire37Checkpoint[2], 0);
            Tick37C(sim, 4.0, null, () => c.Leg >= 3 && c.brain.Fighting);
            int gun = sim.loadout.owned.IndexOf("revolver");
            bool leg3 = c.Leg >= 3;
            int fired = 0;
            Tick37C(sim, 4.0, (s, i) =>
            {
                Vec3 eye = s.Eye, dv = c.centre - eye;
                double yaw = JsMath.Atan2(-dv.x, -dv.z), pitch = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
                si.SetLook(yaw, pitch);
                bool hold = i < 180;
                si.Set(new InputFrame { yaw = yaw, pitch = pitch }, new CombatInput { weapon = s.loadout.Weapon != "revolver" ? gun : -1, fire = new Button { held = hold && i > 40, pressed = i == 41 } });
                fired = i;
            }, () => !c.Alive || c.Dying);
            Line("leg 3 finisher (KESTREL charge)", leg3 && c.finishers >= 1 && (c.Dying || !c.Alive), $"leg {c.Leg} reached {leg3}, finishers {c.finishers}, gems left {c.GemsLeft}, dying/dead {c.Dying || !c.Alive} after {F37(fired * sim.step)} s of the hold");
        }
        return fails;
    }

    sealed class Run37C
    {
        public bool killed;
        public double seconds, stillDamage, totalDamage;
        public int[] legCuts = new int[3], legBlades = new int[3], legBladeHits = new int[3];
        public int dodges;
        public int deaths, falls, legDrops, passes, torchPasses, parries, windows, launches, landings, missed, punches, bladeHits, shardHits, lowHits, cuts, habitDrops, finishers, legs;
        public readonly Dictionary<string, (int n, double min, double max)> tells = new Dictionary<string, (int, double, double)>();
        public readonly List<string> trace = new List<string>();
    }

    /// <summary>One fight: the walk-in on the bridge's far half, Crown37Pilot at the controls (mortal; a death is the campaign's retry: BossRoomBegin
    /// again, the leg restarts), until the kill or `cap` s of fight.</summary>
    static Run37C Fight37C(uint seed, double cap)
    {
        var r = new Run37C();
        SurvivalSim sim = Sim37C(seed, true);
        var si = (ScriptedInput)sim.input;
        sim.Respawn(new Vec3(-10, 0.02, -267), 0);
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        sim.Step();
        if (!sim.BossRoomBegin("crown37", out Vec3 spawn, out double yaw)) return r;
        Crown37 c = sim.crown37;
        double dt = sim.step;
        var pilot = new Crown37Pilot { hand = new Mulberry32(seed * 7919u + 37u) };
        int step = 0, revealSteps = MovementMath.StepsFor(c.p.revealSeconds, dt), capSteps = revealSteps + (int)(cap / dt);
        double yawL = 0;
        int lastL = 0;
        bool wasG = true;
        sim.events.On<BossEvent>("bossHit", e =>
        {
            if (e.boss != "crown37") return;
            r.totalDamage += e.damage;
            double flat = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z);
            if (sim.mover.grounded && flat < 0.5) r.stillDamage += e.damage;
        });
        sim.events.On<Boss37Event>("boss37Leg", e => { if (Trace37C) r.trace.Add($"{step * dt:0.00} leg {e.stage} {e.leg}"); });
        sim.events.On<Crown37Event>("crown37Pass", e => { if (Trace37C) r.trace.Add($"{step * dt:0.00} PASS broke {e.broke} gems left {c.GemsLeft} y {e.at.y:0.0}"); });
        sim.events.On<Crown37Event>("crown37Fall", e => { if (Trace37C) r.trace.Add($"{step * dt:0.00} fall/restart leg {e.leg}"); });
        while (c.Alive && step < capSteps)
        {
            pilot.Step(sim, yawL, step);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (Trace37C && pilot.launches != lastL) { lastL = pilot.launches; var tq = c.passTarget >= 0 ? sim.orbit37.pieces[c.passTarget].pos : c.landHint; r.trace.Add($"{step * dt:0.00} LAUNCH from ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) crown ({c.centre.x:0.0},{c.centre.y:0.0},{c.centre.z:0.0}) target {c.passTarget} at ({tq.x:0.0},{tq.y:0.0},{tq.z:0.0})"); }
            if (Trace37C && sim.mover.grounded && !wasG) r.trace.Add($"{step * dt:0.00} landed ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) on {sim.orbit37.standing}");
            wasG = sim.mover.grounded;
            if (Trace37C && pilot.launches == 2 && sim.mover.state != MoveState.Ground && step % 4 == 0) { Vec3 ch = sim.mover.position + new Vec3(0, 1, 0); r.trace.Add($"   fl {step * dt:0.00} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) v ({sim.mover.velocity.x:0.0},{sim.mover.velocity.y:0.0},{sim.mover.velocity.z:0.0}) {sim.mover.state} dj {sim.mover.DashJumpActive} crown d {Vec3.DistanceTo(ch, c.centre):0.0} locked {c.locked} n ({c.normal.x:0.00},{c.normal.y:0.00},{c.normal.z:0.00})"); }
            if (Trace37C && step % 120 == 0) r.trace.Add($"{step * dt:0.00} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) {sim.mover.state} crown ({c.centre.x:0.0},{c.centre.y:0.0},{c.centre.z:0.0}) {c.brain.state} {c.brain.Current?.id} target {c.passTarget} gems {c.GemsLeft} stagger {c.brain.stagger:0.0} launches {pilot.launches}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.BossRoomBegin("crown37", out spawn, out yaw); // the campaign's retry: the leg's checkpoint, the boss's progress kept
                sim.Respawn(spawn, yaw);
            }
        }
        r.killed = !c.Alive;
        r.seconds = Math.Max(0, step - revealSteps) * dt;
        r.falls = sim.crown37Falls; r.legDrops = sim.crown37LegDrops; r.passes = c.passes; r.torchPasses = c.torchPasses; r.parries = c.parries; r.windows = c.brain.windows;
        for (int k = 0; k < 3; k++) { r.legCuts[k] = c.legCuts[k]; r.legBlades[k] = c.legBlades[k]; r.legBladeHits[k] = c.legBladeHits[k]; }
        r.dodges = pilot.dodges;
        r.launches = pilot.launches; r.landings = pilot.landings; r.missed = pilot.missed; r.punches = pilot.punches;
        r.bladeHits = c.bladeHits; r.shardHits = c.shardHits; r.lowHits = c.lowHits; r.cuts = c.cuts; r.habitDrops = c.habitDrops; r.finishers = c.finishers; r.legs = c.Leg;
        foreach (var (a, steps) in c.brain.tellLog)
        {
            double s = steps * dt;
            r.tells.TryGetValue(a, out var k);
            r.tells[a] = (k.n + 1, k.n == 0 ? s : Math.Min(k.min, s), Math.Max(k.max, s));
        }
        return r;
    }

    static int Band37C(int seeds, double cap)
    {
        int fails = 0;
        var times = new List<double>();
        double still = 0, total = 0;
        int killed = 0;
        int[] legCutAll = new int[3], legBladeAll = new int[3];
        var tellAll = new Dictionary<string, (int n, double min, double max)>();
        for (int s = 1; s <= seeds; s++)
        {
            Run37C r = Fight37C((uint)s, cap);
            if (r.killed) { killed++; times.Add(r.seconds); }
            still += r.stillDamage;
            total += r.totalDamage;
            foreach (var kv in r.tells)
            {
                tellAll.TryGetValue(kv.Key, out var k);
                tellAll[kv.Key] = (k.n + kv.Value.n, k.n == 0 ? kv.Value.min : Math.Min(k.min, kv.Value.min), Math.Max(k.max, kv.Value.max));
            }
            Console.WriteLine($"  seed {s}: killed {r.killed} in {F37(r.seconds)} s, legs reached {r.legs}, passes {r.passes} (torch {r.torchPasses}), parries {r.parries}, windows {r.windows}, finishers {r.finishers}; launches {r.launches}, landings {r.landings}, missed {r.missed}, falls {r.falls}, leg drops {r.legDrops}, deaths {r.deaths}; taken: blade {r.bladeHits} shard {r.shardHits} low {r.lowHits}; cuts {r.cuts}, habit drops {r.habitDrops}, punches {r.punches}; per leg (cuts of the offered piece / blade rings / blade hits): {r.legCuts[0]}/{r.legBlades[0]}/{r.legBladeHits[0]}, {r.legCuts[1]}/{r.legBlades[1]}/{r.legBladeHits[1]}, {r.legCuts[2]}/{r.legBlades[2]}/{r.legBladeHits[2]}, dodge jumps {r.dodges}");
            for (int k = 0; k < 3; k++) { if (r.legCuts[k] > 0) legCutAll[k]++; if (r.legBlades[k] > 0) legBladeAll[k]++; }
            if (Trace37C) foreach (string l in r.trace) Console.WriteLine("      " + l);
        }
        times.Sort();
        double P(double q) => times.Count == 0 ? -1 : times[Math.Min(times.Count - 1, (int)Math.Floor(q * (times.Count - 1) + 0.5))];
        var tl = new List<string>();
        bool tellsOk = true;
        foreach (var kv in tellAll)
        {
            bool inBand = kv.Value.min >= 0.99 && kv.Value.max <= 1.51;
            tellsOk &= inBand;
            tl.Add($"{kv.Key} x{kv.Value.n} {F37(kv.Value.min)}-{F37(kv.Value.max)} s");
        }
        Console.WriteLine($"tells (wind-ups, every seed): {string.Join(", ", tl)}: {Pass36(tellsOk)}");
        double share = total > 0 ? still / total : 0;
        Console.WriteLine($"damage dealt while grounded and still: {F37(still)} of {F37(total)} ({(share * 100).ToString("0.0", CultureInfo.InvariantCulture)} %, want ~0): {Pass36(share < 0.02)}");
        bool pressure = true;
        for (int k = 0; k < 3; k++) pressure &= legCutAll[k] == seeds && legBladeAll[k] == seeds;
        Console.WriteLine($"pressure: seeds with a cut / a blade ring in leg 1 {legCutAll[0]}/{legBladeAll[0]}, leg 2 {legCutAll[1]}/{legBladeAll[1]}, leg 3 {legCutAll[2]}/{legBladeAll[2]} of {seeds}: {Pass36(pressure)}");
        if (!pressure) fails++;
        bool band = killed == seeds && P(0.5) >= 15 && P(0.5) <= 22 && P(0.9) <= 26;
        Console.WriteLine($"band: killed {killed}/{seeds}, p10 {F37(P(0.1))} s, p50 {F37(P(0.5))} s, p90 {F37(P(0.9))} s (want p50 15-22, p90 <= 26): {Pass36(band)}");
        if (!band) fails++;
        if (!tellsOk) fails++;
        if (share >= 0.02) fails++;
        return fails;
    }
}
