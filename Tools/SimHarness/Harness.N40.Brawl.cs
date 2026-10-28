// NIGHT40 builder B2 (brawl40): THE ICEBREAKER, headless. Commands under `n40brawl`:
//   walkin   Brawl40Begin with the row on starts the fight where the player stands (no Respawn), no damage in the reveal, the slab closes at
//            walkin36.lockSeconds; --off40: BossRoomBegin-side answer not handled (Brawl40BeginFor).
//   rule     the damage rule alone: a KESTREL shot's share at 2 / 4 / 8 / 12 / 20 m, the gauntlet's at any range, stuck x, window x.
//   band     the SKILLED pilot (Brawl40Pilot Mixed: KESTREL up close + the PULSE's swing + F punches, dodging every haymaker) over seeds 1..N
//            (--seeds, default 10; --cap S, default 240): the kill time p10 / p50 / p90, per run the haymakers (hit / dodged), the windows,
//            the hits on the stuck fist, the deaths, frames outside the arena, and the per-weapon damage share. PASS (boss_rules 7): every
//            seed killed, p50 30-60 s, p90 <= 70 s, 0 frames outside.
//   range    the pistol-from-range control (Brawl40Pilot Range: the KESTREL alone from ~14 m) beside the mixed band: PASS when its p50 is
//            >= 3x the mixed p50 (or it does not finish inside the cap, which is more).
//   --trace  one line per state change (band / range).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static readonly Vec3 Brawl40At = new Vec3(3000, 0, 0);
    static bool Brawl40Trace, Brawl40ForceAll;
    static int Brawl40FramesPer = 1;
    static InputFrame frameMove;
    static CombatInput frameCombat;

    static partial void N40Brawl(string cmd, string[] args, ref int code)
    {
        if (cmd != "n40brawl") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 240);
        Brawl40Trace = Array.IndexOf(args, "--trace") >= 0;
        Brawl40FramesPer = Math.Max(1, ArgInt36(args, "--frames", 1)); // the Player reel's input: one frame of N sim steps (L-053)
        Brawl40ForceAll = Array.IndexOf(args, "--force") >= 0; // as the reel: a slam, a block and a rush queued once each after the first haymaker
        int fails = 0;
        if (what == "walkin" || what == "all") fails += Brawl40Walkin();
        if (what == "rule" || what == "all") fails += Brawl40Rule();
        if (what == "level" || what == "all") fails += Brawl40Level(1);
        List<double> mixed = null;
        if (what == "band" || what == "range" || what == "all") fails += Brawl40Band(Brawl40Pilot.Style.Mixed, seeds, cap, out mixed);
        if (what == "casual" || what == "all") fails += Brawl40Band(Brawl40Pilot.Style.Casual, seeds, Math.Max(cap, 300), out _);
        if (what == "range" || what == "all")
        {
            fails += Brawl40Band(Brawl40Pilot.Style.Range, Math.Min(seeds, 5), Math.Max(cap, 600), out List<double> range, mixed);
        }
        Console.WriteLine(fails == 0 ? "n40brawl: GREEN" : $"n40brawl: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static string B2P(bool ok) => ok ? "PASS" : "FAIL";

    /// <summary>`n40brawl level`: n40-iceline (L2's) in campaign mode with brawl40 + iceline40 on: DOORFREE, the player put at x9's spawn, stepped
    /// until the campaign begins the fight (through BossRoomBegin, the lead's hook: the walk-in keeps his feet), the arena built at the row's
    /// centre; then the skilled pilot fights it (mortal: the campaign's own retry) and the room must clear on the kill.</summary>
    static int Brawl40Level(uint seed)
    {
        if (N40Blocks.ForceOff) return 0;
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        string path = Path.Combine(MapGenRepo, "Design", "levels", "n40-iceline.json");
        if (!File.Exists(path)) { Console.WriteLine("level: n40-iceline.json not found (L2's): SKIP"); return 0; }
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noLab = sim.noRecords = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n40Override = new N40Blocks { iceline40 = true, brawl40 = true };
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        for (int i = 0; i < 10; i++) sim.Step();
        sim.campaign.SetDoorFree29(true);
        RoomDef x9 = sim.campaignLevel.rooms.Find(r => r.boss == "brawl40");
        if (x9 == null) { Console.WriteLine("level: no room with boss brawl40: FAIL"); return 1; }
        sim.Respawn(x9.spawn, x9.spawnYawDeg);
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        int wait = 0;
        Vec3 at = sim.mover.position;
        while ((sim.brawl40 == null || !sim.brawl40.Alive) && wait++ < 600) sim.Step();
        bool began = sim.brawl40 != null && sim.brawl40.Alive;
        double moved = Vec3.DistanceTo(at, sim.mover.position);
        Console.WriteLine($"level: n40-iceline x9 '{x9.name}': arena at {sim.brawl40Centre?.x:0.0},{sim.brawl40Centre?.y:0.0},{sim.brawl40Centre?.z:0.0}, fight began {began} after {wait} steps (campaign room {sim.campaign.Room?.id}), walk-in {sim.Boss36WalkIn}, feet moved {moved:0.00} m, standing on y {sim.mover.position.y:0.00} (grounded {sim.mover.grounded})");
        if (!began) return 1;
        Brawl40 g = sim.brawl40;
        var si = (ScriptedInput)sim.input;
        var pilot = new Brawl40Pilot { hand = new Mulberry32(seed * 7919u + 40u) };
        int step = 0, deaths = 0;
        double yawL = x9.spawnYawDeg * Math.PI / 180;
        while (g.Alive && step < 240 * 120)
        {
            pilot.Step(sim, yawL, step);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead) { deaths++; sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; sim.health.Reset(); }
        }
        for (int i = 0; i < 240; i++) sim.Step();
        bool cleared = sim.campaign.Room?.id != "x9" || sim.campaign.finished || sim.BossRoomDefeated("brawl40");
        bool ok = !g.Alive && moved < 0.05 && g.outside == 0 && sim.BossRoomDefeated("brawl40");
        Console.WriteLine($"  killed {!g.Alive} in {step / 120.0:0.0} s, deaths {deaths}, frames outside {g.outside}, BossRoomDefeated {sim.BossRoomDefeated("brawl40")}, campaign room now {sim.campaign.Room?.id}, finished {sim.campaign.finished}, slab open {!sim.Brawl40SlabClosed}: {B2P(ok)}");
        return ok ? 0 : 1;
    }

    static SurvivalSim Brawl40Sim(uint seed, bool on)
    {
        GameConfig cfg = Config();
        SurvivalSim.Brawl40Dir = MapGenRepo;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n40Override = on ? new N40Blocks { brawl40 = true } : N40Blocks.Off;
        sim.n42Override = Brawl40N42; // NIGHT42 B1: `n42brawl` turns brawl42 on here (null: the design's, every row off)
        if (Brawl40N43.HasValue) sim.n43Override = Brawl40N43; // NIGHT43 W1: `n43weap band` turns gauntlet43 / fusion43 on here (null: the design's)
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.Brawl40Place(Brawl40At)) throw new InvalidOperationException("n40brawl: the arena file was not found (design.n40.brawl40.arena under " + MapGenRepo + ")");
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        return sim;
    }

    static int Brawl40Walkin()
    {
        int fails = 0;
        bool on = !N40Blocks.ForceOff;
        SurvivalSim sim = Brawl40Sim(1, on);
        double dt = sim.cfg.Dt;
        sim.Respawn(sim.brawl40Arena.spawn + Brawl40At, 0);
        sim.Step();
        Vec3 before = sim.mover.position;
        Vec3 sp = default;
        double yw = 0;
        bool ok = sim.BossRoomBegin("brawl40", out sp, out yw); // the campaign's path (the lead's Boss25 hook)
        bool handled = ok;
        bool owned = ReferenceEquals(sim.BossRoomSystem("brawl40"), sim.brawl40) && sim.brawl40 != null;
        Console.WriteLine($"walkin: handled {handled}, begin {ok}, BossRoomSystem-side owns 'brawl40' {owned}, walkIn {sim.Boss36WalkIn}, feet moved by the begin {Vec3.DistanceTo(before, sim.mover.position):0.000} m");
        if (!on) { Console.WriteLine($"  --off40: not handled, no system: {B2P(!handled && !owned)}"); return handled || owned ? 1 : 0; }
        if (!(ok && owned && sim.Boss36WalkIn)) fails++;
        Brawl40 g = sim.brawl40;
        double hp0 = g.body.hp;
        int reveal = MovementMath.StepsFor(g.p.revealSeconds, dt), lockAt = MovementMath.StepsFor(sim.cfg.Num("design.n36.walkin36.lockSeconds"), dt);
        int slabStep = -1, idleStep = -1;
        sim.mover.invulnerable = true;
        var si = (ScriptedInput)sim.input;
        int gi = sim.loadout.owned.IndexOf("revolver");
        for (int st = 1; st <= reveal + 60; st++)
        {
            Vec3 dv = g.Centre - sim.Eye;
            double ya = JsMath.Atan2(-dv.x, -dv.z), pa = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
            si.SetLook(ya, pa);
            si.Set(new InputFrame { yaw = ya, pitch = pa }, new CombatInput { weapon = st == 1 ? gi : -1, fire = new Skygrind.Core.Button { held = st % 30 == 0, pressed = st % 30 == 0 } });
            sim.Step();
            sim.hitstopLeft = 0;
            if (slabStep < 0 && sim.Brawl40SlabClosed) slabStep = st;
            if (idleStep < 0 && g.state == Brawl40State.Idle) idleStep = st;
            if (st == reveal - 2 && g.body.hp != hp0) { Console.WriteLine($"  damage in the reveal {hp0 - g.body.hp:0}: FAIL"); fails++; }
        }
        bool slabOk = slabStep > 0 && Math.Abs(slabStep - lockAt) <= 2, idleOk = idleStep > 0 && Math.Abs(idleStep - reveal) <= 2;
        Console.WriteLine($"  the reveal ends at {idleStep * dt:0.00} s (want {reveal * dt:0.00}): {B2P(idleOk)}; the slab closes at {slabStep * dt:0.00} s (want {lockAt * dt:0.00}): {B2P(slabOk)}; the name card at its step {sim.brawl40CardStep}");
        if (!slabOk) fails++;
        if (!idleOk) fails++;
        return fails;
    }

    static int Brawl40Rule()
    {
        if (N40Blocks.ForceOff) return 0;
        SurvivalSim sim = Brawl40Sim(1, true);
        sim.Respawn(sim.brawl40Arena.spawn + Brawl40At, 0);
        Vec3 sp = default;
        double yw = 0;
        sim.Brawl40BeginFor("brawl40", out _, ref sp, ref yw);
        Brawl40 g = sim.brawl40;
        g.state = Brawl40State.Idle;
        var ci = CultureInfo.InvariantCulture;
        var cells = new List<string>();
        bool ok = true;
        foreach (double m in new[] { 2.0, 4.0, 8.0, 12.0, 20.0 })
        {
            // put the player m metres off its surface, straight out along +z
            sim.Respawn(g.Feet + new Vec3(0, 0, g.p.radius + m), 0);
            double share = g.Filter(100, "revolver", out bool bl) / 100;
            double gs = g.Filter(100, "gauntlet", out _) / 100;
            cells.Add($"{m:0} m: KESTREL x{share.ToString("0.00", ci)} gauntlet x{gs.ToString("0.00", ci)}");
            double want = m <= 4 ? 1 : m >= 12 ? 0.2 : 1 - 0.8 * (m - 4) / 8;
            ok &= Math.Abs(share - want) < 1e-9 && Math.Abs(gs - 1) < 1e-9 && !bl;
        }
        g.state = Brawl40State.Stuck;
        double stuck = g.Filter(100, "gauntlet", out _) / 100;
        g.state = Brawl40State.Window;
        double win = g.Filter(100, "gauntlet", out _) / 100;
        g.state = Brawl40State.Reveal;
        g.Filter(100, "gauntlet", out bool revealBlocked);
        ok &= Math.Abs(stuck - g.p.stuckMult) < 1e-9 && Math.Abs(win - g.p.windowMult) < 1e-9 && revealBlocked;
        Console.WriteLine($"rule: {string.Join(" | ", cells)}; stuck x{stuck:0.00}, window x{win:0.00}, reveal blocked {revealBlocked}: {B2P(ok)}");
        return ok ? 0 : 1;
    }

    sealed class Brawl40Run
    {
        public bool killed;
        public double seconds, phase2At = -1;
        public int deaths, haymakers, hayHits, hayDodged, windows, stuckHits, outside, swings, swingHits, shots, punches, dodges, stillSteps, jumps, blockPunches, missed;
        public string attacks = "";
        public Dictionary<string, double> by = new Dictionary<string, double>();
        public double dealt, hurt; // hurt: NIGHT42 B1 (the player's health lost)
        public int sweepJumps, leapEscapes; // NIGHT42 B1
        public List<(string, int)> tells = new List<(string, int)>();
    }

    static Brawl40Run Brawl40Fight(uint seed, double cap, Brawl40Pilot.Style style)
    {
        var r = new Brawl40Run();
        SurvivalSim sim = Brawl40Sim(seed, true);
        double dt = sim.cfg.Dt;
        sim.Respawn(sim.brawl40Arena.spawn + Brawl40At, 0);
        Vec3 spawn = default;
        double yaw = 0;
        if (!sim.Brawl40BeginFor("brawl40", out bool handled, ref spawn, ref yaw)) return r;
        Brawl40 g = sim.brawl40;
        var si = (ScriptedInput)sim.input;
        var pilot = new Brawl40Pilot { hand = new Mulberry32(seed * 7919u + 40u), style = style };
        int step = 0, reveal = MovementMath.StepsFor(g.p.revealSeconds, dt), capSteps = (int)(cap / dt) + reveal;
        double yawL = 0;
        Brawl40State last = g.state;
        var forceList = new List<string> { Brawl40.Slam, Brawl40.Block, Brawl40.Rush };
        if (g.on42) { forceList.Add(Brawl40.Sweep); forceList.Add(Brawl40.Leap); } // NIGHT42 B1: the new attacks once each too
        while (g.Alive && step < capSteps)
        {
            if (Brawl40ForceAll && !g.ForcePending && g.state == Brawl40State.Idle && g.haymakers > 0 && forceList.Count > 0 && (step - reveal) * dt > 4)
            {
                g.Force(forceList[0]);
                forceList.RemoveAt(0);
            }
            if (Brawl40FramesPer > 1)
            {
                // as the Player reel (FeelReel.N40.Brawl.cs): every `framesPer` sim steps one frame's input, the pilot stepped once per step of
                // the frame on the same state, every press of any of its steps kept and held for the whole frame (L-053)
                if (step % Brawl40FramesPer == 0)
                {
                    InputFrame mv = default;
                    CombatInput cb = new CombatInput { weapon = -1 };
                    for (int k = 0; k < Brawl40FramesPer; k++)
                    {
                        pilot.Step(sim, yawL, step + k);
                        InputFrame m = pilot.Move;
                        m.jump.pressed |= mv.jump.pressed; m.jump.held |= mv.jump.pressed;
                        m.dash.pressed |= mv.dash.pressed; m.dash.held |= mv.dash.pressed;
                        CombatInput cc = pilot.Combat;
                        cc.fire.pressed |= cb.fire.pressed; cc.fire.held |= cb.fire.pressed;
                        cc.punch.pressed |= cb.punch.pressed; cc.punch.held |= cb.punch.pressed;
                        if (cc.weapon < 0) cc.weapon = cb.weapon;
                        mv = m;
                        cb = cc;
                    }
                    frameMove = mv;
                    frameCombat = cb;
                    yawL = mv.yaw;
                }
                si.SetLook(frameMove.yaw, frameMove.pitch);
                si.Set(frameMove, frameCombat);
            }
            else
            {
                pilot.Step(sim, yawL, step);
                yawL = pilot.Move.yaw;
                si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                si.Set(pilot.Move, pilot.Combat);
            }
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (g.phase == 2 && r.phase2At < 0) r.phase2At = (step - reveal) * dt;
            if (Brawl40Trace && g.state != last)
                Console.WriteLine($"    t {(step - reveal) * dt,6:0.00} {last} -> {g.state} hp {g.body.hp:0} stagger {g.stagger:0.0} surf {g.RangeToPlayer():0.0} m, me hp {sim.health.hp:0} {sim.loadout.Weapon}");
            last = g.state;
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw); // the campaign's retry: the fight's progress kept
                sim.Respawn(spawn, yaw);
            }
        }
        r.killed = !g.Alive;
        r.seconds = Math.Max(0, step - reveal) * dt;
        r.attacks = $"slams {g.slams} (hit {g.slamHits}, camp {g.campPicks}), blocks {g.blocks} (hit {g.blockHits}, parried {g.blockParries}), rushes {g.rushes} (hit {g.rushHits}, walls {g.rushWalls}), floes cracked {g.floesCracked} (bites {g.floeTicks})" + g.Stats42(); r.hurt = g.hurt42; r.sweepJumps = pilot.sweepJumps; r.leapEscapes = pilot.leapEscapes; r.jumps = pilot.jumps; r.blockPunches = pilot.blockPunches; r.missed = pilot.missedReacts;
        r.haymakers = g.haymakers; r.hayHits = g.hayHits; r.hayDodged = g.hayDodged; r.windows = g.windows; r.stuckHits = g.stuckHits; r.outside = g.outside;
        r.swings = pilot.swings; r.swingHits = pilot.swingHits; r.shots = pilot.shots; r.punches = pilot.punches; r.dodges = pilot.dodges; r.stillSteps = pilot.stillSteps;
        foreach (var kv in g.dealtBy) r.by[kv.Key] = kv.Value;
        r.dealt = g.dealt;
        r.tells = new List<(string, int)>(g.tellLog);
        return r;
    }

    static int Brawl40Band(Brawl40Pilot.Style style, int seeds, double cap, out List<double> times, List<double> against = null)
    {
        times = new List<double>();
        if (N40Blocks.ForceOff) { times = null; return 0; }
        int killed = 0, outside = 0;
        var share = new Dictionary<string, double>();
        double dealtAll = 0;
        var tellsAll = new List<(string, int)>();
        for (int i = 1; i <= seeds; i++)
        {
            Brawl40Run r = Brawl40Fight((uint)i, cap, style);
            string by = string.Join(", ", r.by.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value / Math.Max(1, r.dealt) * 100:0}%"));
            Console.WriteLine($"  {style} seed {i}: killed {r.killed} in {r.seconds:0.0} s (phase 2 at {r.phase2At:0.0} s), deaths {r.deaths}; haymakers {r.haymakers} (hit {r.hayHits}, dodged {r.hayDodged}), windows {r.windows}, stuck hits {r.stuckHits}; {r.attacks}; pilot swings {r.swings} (hit {r.swingHits}) shots {r.shots} punches {r.punches} dodges {r.dodges} jumps {r.jumps} block punches {r.blockPunches} missed reacts {r.missed}, still {r.stillSteps / 120.0:0.0} s; frames outside {r.outside}; damage by weapon: {by}");
            if (r.killed) { killed++; times.Add(r.seconds); }
            outside += r.outside;
            foreach (var kv in r.by) { share.TryGetValue(kv.Key, out double h); share[kv.Key] = h + kv.Value; }
            dealtAll += r.dealt;
            tellsAll.AddRange(r.tells);
        }
        times.Sort();
        List<double> t = times;
        double P(double q) => t.Count == 0 ? double.NaN : t[Math.Min(t.Count - 1, (int)Math.Floor(q * (t.Count - 1) + 0.5))];
        string shareLine = string.Join(", ", share.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {kv.Value / Math.Max(1, dealtAll) * 100:0.0}%"));
        var kinds = new Dictionary<string, (int n, double min, double max)>();
        foreach (var (a, st) in tellsAll)
        {
            double s = st / 120.0;
            kinds.TryGetValue(a ?? "?", out var k);
            kinds[a ?? "?"] = (k.n + 1, k.n == 0 ? s : Math.Min(k.min, s), Math.Max(k.max, s));
        }
        string tellLine = string.Join(", ", kinds.Select(kv => $"{kv.Key} {kv.Value.n}x {kv.Value.min:0.00}-{kv.Value.max:0.00} s"));
        if (style == Brawl40Pilot.Style.Mixed)
        {
            bool ok = killed == seeds && P(0.5) >= 30 && P(0.5) <= 60 && P(0.9) <= 70 && outside == 0;
            Console.WriteLine($"band (skilled, mixed): killed {killed}/{seeds}, p10 {P(0.1):0.0} s, p50 {P(0.5):0.0} s, p90 {P(0.9):0.0} s (want p50 30-60, p90 <= 70), frames outside the arena {outside} (want 0): {B2P(ok)}");
            Console.WriteLine($"  damage share (all seeds): {shareLine}; tells: {tellLine}");
            return ok ? 0 : 1;
        }
        if (style == Brawl40Pilot.Style.Casual)
        {
            bool ok = killed == seeds && outside == 0;
            Console.WriteLine($"casual: killed {killed}/{seeds} (cap {cap:0} s), p50 {P(0.5):0.0} s, max {P(1.0):0.0} s, frames outside {outside} (want 10/10 killed, 0 outside; boss_rules 7): {B2P(ok)}");
            Console.WriteLine($"  damage share (all seeds): {shareLine}");
            return ok ? 0 : 1;
        }
        double mixedP50 = against != null && against.Count > 0 ? against[against.Count / 2] : double.NaN;
        double rangeP50 = killed == seeds ? P(0.5) : cap;
        bool rok = !double.IsNaN(mixedP50) && rangeP50 >= 3 * mixedP50;
        Console.WriteLine($"range (the KESTREL alone from ~14 m): killed {killed}/{seeds} (cap {cap:0} s), p50 {(killed == seeds ? P(0.5).ToString("0.0") : ">= " + cap.ToString("0"))} s vs mixed p50 {mixedP50:0.0} s: x{rangeP50 / mixedP50:0.0} (want >= 3): {B2P(rok)}");
        Console.WriteLine($"  damage share (all seeds): {shareLine}");
        return rok ? 0 : 1;
    }
}
