// NIGHT37 builder S (stag37): THE STAG's fight, headless (the canal's checks: Harness.N37.Stag.cs). Commands under `stag37`:
//   walkin   BossRoomBegin("stag37") with the row on owns the begin, the feet unmoved (the walk-in), no damage in the reveal, the Stag bolts at
//            revealSeconds, the slab closes at walkin36.lockSeconds; --off37: the begin is not handled.
//   band     Stag37Pilot (mortal, retries) over seeds 1..N (--seeds, default 10): kill time p10 / p50 / p90; per run: leg punches, launches, chunk
//            parries, the charge parry, falls, deaths, the slipstream seconds, every tell's length and colour; the share of damage dealt while the
//            player stood still (grounded, under 1 m/s): want ~0. PASS: every seed killed, p50 15-22 s, p90 <= 26 s, every tell 0.8-1.5 s.
//   pacer    the 2.9 rehearsal's pacer on the canal's straight A: a skater who drafts it catches it (the slipstream proven on the pacer).
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N37StagMore(string what, string[] args, ref int fails)
    {
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 90);
        StagTrace37 = Array.IndexOf(args, "--trace") >= 0;
        if (what == "walkin" || what == "all") fails += StagWalkin37();
        if (what == "band" || what == "all") fails += StagBand37(seeds, cap);
        if (what == "pacer" || what == "all") fails += StagPacer37();
        if (what == "casual" || what == "all") fails += StagCasual37(seeds, Math.Max(cap, 120));
    }

    static Vec3 StagGap37(SurvivalSim sim) => sim.stag37Arena.GapPoint + StagCanal37 + new Vec3(0, 0, -1.5);

    static int StagWalkin37()
    {
        int fails = 0;
        bool on = !N37Blocks.ForceOff;
        SurvivalSim sim = Sim37S(1, on);
        double dt = sim.cfg.Dt;
        sim.Respawn(StagGap37(sim), 0);
        sim.Step();
        Vec3 before = sim.mover.position;
        bool ok = sim.BossRoomBegin("stag37", out Vec3 spawn, out double yaw);
        bool owned = on && sim.stag37 != null && sim.stag37.Alive && ReferenceEquals(sim.BossRoomSystem("stag37"), sim.stag37);
        Console.WriteLine($"walkin: begin {ok}, stag37 owns 'stag37' {owned}, walkIn {sim.Boss36WalkIn}, feet moved by the begin {Vec3.DistanceTo(before, sim.mover.position):0.000} m");
        if (!on) { Console.WriteLine($"  --off37: the begin is not handled: {StagP37(!ok && !owned)}"); return ok || owned ? 1 : 0; }
        if (!(ok && owned && sim.Boss36WalkIn)) fails++;
        Stag37 g = sim.stag37;
        double hp0 = g.body.hp;
        int reveal = MovementMath.StepsFor(g.p.revealSeconds, dt), lockAt = MovementMath.StepsFor(sim.cfg.Num("design.n36.walkin36.lockSeconds"), dt);
        int slabStep = -1, boltStep = -1;
        sim.mover.invulnerable = true;
        var si = (ScriptedInput)sim.input;
        for (int st = 1; st <= reveal + 120; st++)
        {
            Vec3 dv = g.Centre - sim.Eye;
            double ya = JsMath.Atan2(-dv.x, -dv.z), pa = JsMath.Atan2(dv.y, JsMath.Hypot(dv.x, dv.z));
            si.SetLook(ya, pa);
            si.Set(new InputFrame { yaw = ya, pitch = pa }, new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = (st / 2) % 2 == 0, pressed = st % 4 == 0 } });
            sim.Step();
            sim.hitstopLeft = 0;
            if (slabStep < 0 && sim.Stag37SlabClosed) slabStep = st;
            if (boltStep < 0 && g.speed > 1e-6) boltStep = st;
            if (st == reveal - 2 && g.body.hp != hp0) { Console.WriteLine($"  damage in the reveal {hp0 - g.body.hp:0}: FAIL"); fails++; }
        }
        bool slabOk = slabStep > 0 && Math.Abs(slabStep - lockAt) <= 2, boltOk = boltStep > 0 && Math.Abs(boltStep - reveal) <= 3;
        Console.WriteLine($"  the Stag stands {g.p.startAhead} m ahead in the reveal, bolts at {boltStep * dt:0.00} s (want {reveal * dt:0.00}): {StagP37(boltOk)}; the slab closes at {slabStep * dt:0.00} s (want {lockAt * dt:0.00}): {StagP37(slabOk)}");
        Console.WriteLine($"  the name card at step {sim.stag37CardStep} (want {MovementMath.StepsFor(sim.cfg.Num("design.n36.walkin36.cardSeconds"), dt)})");
        if (!slabOk) fails++;
        if (!boltOk) fails++;
        return fails;
    }

    sealed class StagRun37
    {
        public bool killed;
        public double seconds, slip, stillShare, slipMax;
        public int kickForces, kickTells, waits;
        public double firstSlip = -1, firstCatch = -1, worstReturn, meanSpeed;
        public int deaths, falls, legPunches, launches, chunkParries, chargeParries, charges, shardHits, chunkHits, kickHits, chargeHits, cuts, kicks, punches, dashes, dodges, stumbles, windows, floeDrops, windowPunches;
        public int[] legAt = new int[4];
        public List<(string, int)> tells = new List<(string, int)>();
    }

    static StagRun37 StagFight37(uint seed, double cap, bool casual = false)
    {
        var r = new StagRun37();
        SurvivalSim sim = Sim37S(seed, true);
        double dt = sim.cfg.Dt;
        sim.Respawn(StagGap37(sim), 0);
        if (!sim.BossRoomBegin("stag37", out Vec3 spawn, out double yaw)) return r;
        Stag37 g = sim.stag37;
        var si = (ScriptedInput)sim.input;
        var pilot = new Stag37Pilot { hand = new Mulberry32(seed * 7919u + 37u), casual = casual };
        double behindAt = -1, speedSum = 0;
        int speedN = 0;
        double yawL = 0;
        if (StagTrace37) sim.events.On<Stag37Event>("stag37Fall", e => { Console.WriteLine($"      FALL at t {sim.SimTime:0.00}: leg {g.brain.leg}, the player's lane {g.playerLane}, speed {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.0}"); });
        if (StagTrace37) sim.events.On<Stag37Event>("stag37Floe", e => { if (e.stage == "drop") { Vec3 me = sim.mover.position - StagCanal37; int under = sim.stag37Arena.FloeAt(me.x, me.z); Console.WriteLine($"      floe {e.count} (lane {e.lane}) drops; the player on floe {under} lane {g.playerLane} y {me.y:0.0}"); } });
        if (StagTrace37) sim.events.On<MeleeEvent>("punch", e => { var (ey, dr) = sim.Aim(); if (g.leg3 != Stag37Leg3.None) Console.WriteLine($"      punch: leg3 {g.leg3} {g.brain.state} head-aimEye {Vec3.DistanceTo(ey, g.Head):0.00} result '{sim.stag37LastPunch}'"); });
        int step = 0, reveal = MovementMath.StepsFor(g.p.revealSeconds, dt), capSteps = (int)(cap / dt) + reveal, legNow = 1;
        while (g.Alive && step < capSteps)
        {
            pilot.Step(sim, yawL, step);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (g.brain.leg != legNow) { legNow = g.brain.leg; r.legAt[legNow] = step - reveal; }
            double tf = (step - reveal) * dt;
            if (tf > 0)
            {
                speedSum += JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z); speedN++;
                if (g.InSlip && r.firstSlip < 0) r.firstSlip = tf;
                if (g.legPunches > 0 && r.firstCatch < 0) r.firstCatch = tf;
                if (g.leg3 == Stag37Leg3.None && g.brain.Fighting && !g.brain.Open && g.gap > g.p.farGap && behindAt < 0) behindAt = tf;
                if (behindAt >= 0 && (g.InSlip || g.gap <= g.p.slipRange || g.leg3 != Stag37Leg3.None || g.brain.Open)) { r.worstReturn = Math.Max(r.worstReturn, tf - behindAt); behindAt = -1; }
            }
            if (StagTrace37 && (g.brain.entered || step % 240 == 0))
                Console.WriteLine($"    t {(step - reveal) * dt:0.00} {g.brain.state} {g.AttackId} leg {g.brain.leg} hp {g.body.hp:0} stagger {g.brain.stagger:0} gap {g.gap:0.0} stag {g.speed:0.0} me {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.0} {sim.mover.state} lane me {g.playerLane} it {sim.stag37Arena.LaneOf(g.lat)} slip {g.InSlip} leg3 {g.leg3} y {sim.mover.position.y:0.0}");
            if (StagTrace37 && sim.mover.position.y < -0.3 && sim.mover.position.y > -0.5)
            {
                Vec3 me = sim.mover.position - StagCanal37, v = sim.mover.velocity;
                sim.stag37Arena.Project(me.x, me.z, out double fs, out double fl);
                int fi = -1; for (int k = 0; k < sim.stag37Arena.floes.Count; k++) { var fq = sim.stag37Arena.floes[k]; if (me.x >= fq.min.x - 0.4 && me.x <= fq.max.x + 0.4 && me.z >= fq.min.z - 0.4 && me.z <= fq.max.z + 0.4 && sim.stag37FloeDown[k]) fi = k; }
                Console.WriteLine($"      SINKING: along B {fs - sim.stag37Arena.SB:0.0} m, lat {fl:0.0} (lane {sim.stag37Arena.LaneOf(fl)}), speed {JsMath.Hypot(v.x, v.z):0.0}, the down floe under {fi}, the pilot wants lat {pilot.LaneLatNow:0.0}, state {sim.mover.state}");
            }
            if (StagTrace37 && g.leg3 == Stag37Leg3.Charge && step % 3 == 0)
                Console.WriteLine($"      charge: head-eye {Vec3.DistanceTo(g.Head, sim.Eye):0.00} m, stag lat {g.lat:0.0} me lat {g.plat:0.0}, dir {g.dir}, me {sim.mover.state}, punches {pilot.punches}, last punch '{sim.stag37LastPunch}', hp {sim.health.hp:0}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.BossRoomBegin("stag37", out spawn, out yaw); // the campaign's retry: the leg restarts
                sim.Respawn(spawn, yaw);
            }
        }
        r.killed = !g.Alive;
        r.seconds = Math.Max(0, step - reveal) * dt;
        r.tells = new List<(string, int)>(g.brain.tellLog);
        r.legPunches = g.legPunches; r.launches = g.launches; r.chunkParries = g.chunkParries; r.chargeParries = g.chargeParries; r.charges = g.charges;
        r.shardHits = g.shardHits; r.chunkHits = g.chunkHits; r.kickHits = g.kickHits; r.chargeHits = g.chargeHits; r.cuts = g.cuts; r.kicks = g.kicks;
        r.stumbles = g.stumbles; r.windows = g.brain.windows; r.windowPunches = g.windowPunches; r.slip = g.slipSeconds; r.falls = sim.stag37Falls; r.floeDrops = sim.stag37FloeDrops;
        r.punches = pilot.punches; r.dashes = pilot.dashes; r.dodges = pilot.dodges;
        r.slipMax = g.slipRecentMax / 120.0;
        r.kickForces = g.kickForces; r.kickTells = g.kickTells; r.waits = g.waits; r.meanSpeed = speedN > 0 ? speedSum / speedN : 0;
        if (behindAt >= 0) r.worstReturn = Math.Max(r.worstReturn, r.seconds - behindAt);
        r.stillShare = g.dealt > 0 ? g.dealtStill / g.dealt : 0;
        return r;
    }

    static int StagBand37(int seeds, double cap)
    {
        int fails = 0;
        var times = new List<double>();
        var tellsAll = new List<(string, int)>();
        double still = 0, dealtRuns = 0;
        int killed = 0;
        for (int i = 1; i <= seeds; i++)
        {
            StagRun37 r = StagFight37((uint)i, cap);
            Console.WriteLine($"  seed {i}: killed {r.killed} in {r.seconds:0.0} s (leg 2 at {r.legAt[2] / 120.0:0.0} s, leg 3 at {r.legAt[3] / 120.0:0.0} s), deaths {r.deaths}, falls {r.falls}; leg punches {r.legPunches}, chunk parries {r.chunkParries}, windows {r.windows}, launches {r.launches}, window punches {r.windowPunches}, charges {r.charges} (parried {r.chargeParries}); taken: shards {r.shardHits} chunks {r.chunkHits} kicks {r.kickHits}/{r.kicks} (slip in 6 s max {r.slipMax:0.0} s, forced {r.kickForces}, kick wind-ups {r.kickTells}) charges {r.chargeHits}; cuts {r.cuts}, floes dropped {r.floeDrops}; slipstream {r.slip:0.0} s; pilot punches {r.punches} dashes {r.dashes} dodges {r.dodges}; damage while still {r.stillShare * 100:0.0} %");
            if (r.killed) { killed++; times.Add(r.seconds); }
            tellsAll.AddRange(r.tells);
            still += r.stillShare; dealtRuns++;
        }
        times.Sort();
        double P(double q) => times.Count == 0 ? double.NaN : times[Math.Min(times.Count - 1, (int)Math.Floor(q * (times.Count - 1) + 0.5))];
        bool bandOk = killed == seeds && P(0.5) >= 15 && P(0.5) <= 22 && P(0.9) <= 26;
        Console.WriteLine($"band: killed {killed}/{seeds}, p10 {P(0.1):0.0} s, p50 {P(0.5):0.0} s, p90 {P(0.9):0.0} s (want p50 15-22, p90 <= 26): {StagP37(bandOk)}; damage dealt while standing still {still / Math.Max(1, dealtRuns) * 100:0.0} % (want ~0)");
        if (!bandOk) fails++;
        var kinds = new Dictionary<string, (int n, double min, double max)>();
        foreach (var (a, st) in tellsAll)
        {
            double s = st / 120.0;
            kinds.TryGetValue(a ?? "?", out var k);
            kinds[a ?? "?"] = (k.n + 1, k.n == 0 ? s : Math.Min(k.min, s), Math.Max(k.max, s));
        }
        foreach (var kv in kinds) Console.WriteLine($"  tell {kv.Key}: {kv.Value.n}x, {kv.Value.min:0.00}-{kv.Value.max:0.00} s");
        return fails;
    }

    static int StagPacer37()
    {
        SurvivalSim sim = Sim37S(1, true);
        Stag37Arena a = sim.stag37Arena;
        // the pacer on straight A's middle lane, 10 m ahead of a skater who starts 2 m short of its line
        Vec3 from = a.PointAt(12, 0) + StagCanal37, to = a.PointAt(84, 0) + StagCanal37;
        sim.Stag37PacerPlace(from, to, from.z + 4);
        var si = (ScriptedInput)sim.input;
        sim.Respawn(a.PointAt(0, 0) + StagCanal37, 0);
        double dt = sim.cfg.Dt;
        int dashCool = 0, pushes0 = sim.stag37SlipPushes;
        double closest = 1e9;
        bool caught = false;
        for (int st = 0; st < 8 * 120 && !caught; st++)
        {
            var pc = sim.stag37Pacer;
            Vec3 aim = pc.phase == 1 ? pc.pos : to;
            Vec3 d = aim - sim.mover.position;
            double yawA = JsMath.Atan2(-d.x, -d.z);
            var fr = new InputFrame { yaw = yawA, moveZ = 1 };
            if (dashCool > 0) dashCool--;
            if (dashCool == 0 && sim.mover.grounded && sim.mover.pips >= 1 && Vec3.DistanceTo(pc.pos, sim.mover.position) > 5) { fr.dash = new Skygrind.Core.Button { held = true, pressed = true }; dashCool = 42; }
            bool punchIt = pc.phase == 1 && Vec3.DistanceTo(pc.pos, sim.mover.position) < 3.5;
            si.SetLook(yawA, 0);
            si.Set(fr, new CombatInput { weapon = -1, punch = new Skygrind.Core.Button { held = punchIt, pressed = punchIt } });
            sim.Step();
            if (pc.phase == 1) closest = Math.Min(closest, Vec3.DistanceTo(pc.pos, sim.mover.position));
            caught = pc.catches > 0;
        }
        int pushes = sim.stag37SlipPushes - pushes0;
        bool ok = caught && pushes > 0;
        Console.WriteLine($"pacer: runs {sim.stag37Pacer.runs}, slipstream pushes {pushes}, closest {closest:0.0} m, caught {caught}: {StagP37(ok)}");
        return ok ? 0 : 1;
    }

    /// <summary>casual: the CASUAL pilot (sprint-glide, no dash pumping) over seeds 1..N: does it enter the slipstream, its first catch (a hind-leg
    /// punch), the worst time from falling past farGap behind back into the slipstream, the kill time. PASS: every seed killed under 45 s, every
    /// return within 8 s (the lead, round 3).</summary>
    static int StagCasual37(int seeds, double cap)
    {
        var kills = new List<double>();
        double worst = 0;
        int killed = 0, slipped = 0;
        for (int i = 1; i <= seeds; i++)
        {
            StagRun37 r = StagFight37((uint)i, cap, true);
            Console.WriteLine($"  casual seed {i}: killed {r.killed} in {r.seconds:0.0} s (leg 2 at {r.legAt[2] / 120.0:0.0}, leg 3 at {r.legAt[3] / 120.0:0.0}); mean speed {r.meanSpeed:0.0} m/s; first slipstream {r.firstSlip:0.0} s, first catch {r.firstCatch:0.0} s, look-backs {r.waits}, worst return to the slipstream {r.worstReturn:0.0} s; slipstream {r.slip:0.0} s; leg punches {r.legPunches}, launches {r.launches}, window punches {r.windowPunches}, charge parried {r.chargeParries}; falls {r.falls}, deaths {r.deaths}");
            if (r.killed) { killed++; kills.Add(r.seconds); }
            if (r.firstSlip >= 0) slipped++;
            worst = Math.Max(worst, r.worstReturn);
        }
        kills.Sort();
        double P(double q) => kills.Count == 0 ? double.NaN : kills[Math.Min(kills.Count - 1, (int)Math.Floor(q * (kills.Count - 1) + 0.5))];
        bool ok = killed == seeds && P(1.0) < 45 && worst <= 8;
        Console.WriteLine($"casual: entered the slipstream {slipped}/{seeds}, killed {killed}/{seeds}, kill p50 {P(0.5):0.0} s, max {P(1.0):0.0} s (want < 45), worst return {worst:0.0} s (want <= 8): {StagP37(ok)}");
        return ok ? 0 : 1;
    }
}
