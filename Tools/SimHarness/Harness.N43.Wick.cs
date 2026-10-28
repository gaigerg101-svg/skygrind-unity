// NIGHT43 builder B1 (wick43; NIGHT43.md card B1, G13-G16, G10): THE WICK readable, headless. One entry, N43Wick (Program.cs calls it).
//   n43wick <what> [--seeds N] [--cap S] [--blocks a,b,c] [--off] [--trace] [--every N]
//     The stack at (2600, 0, 0) on the survival map (as `wick37`), wick37 on. --blocks: the NIGHT43 blocks forced on (default: wick43, chimney43,
//     ride43); --off: every NIGHT43 block off (NIGHT42's game: the BEFORE). --off43 (Program.cs) wins over both.
//     climb   Wick37Pilot alone in the stack (no boss), 10 hands: the cap share, the rides, ride40's attach refusals by rule (G13).
//     band    the fight with the skilled pilot over seeds 1..N: kill time, deaths, parries, pierces, launches (the n37 band, wick43 on).
//     casual  the fight with the CASUAL pilot (slower aim, late releases, misses parries) over seeds 1..N.
//     state   one fight, every step: N43BossState valid from the begin to the death (G14), the states and hints seen, the bossLeg43 /
//             bossOpen43 events (G16), the legs' variations used.
//     all     climb + band + casual + state.
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static N43Blocks Wick43Blocks;
    static bool Trace43W;
    static int Every43W = 30;
    static double Lean43W = -1; // --lean M (tenths: --lean 15 = 1.5 m): the pilot's wallLean; -1 = its default

    static partial void N43Wick(string cmd, string[] args, ref int code)
    {
        if (cmd != "n43wick") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 150);
        Trace43W = Array.IndexOf(args, "--trace") >= 0;
        Every43W = ArgInt36(args, "--every", 30);
        Lean43W = ArgInt36(args, "--lean", -10) / 10.0;
        bool off = Array.IndexOf(args, "--off") >= 0 || N43Blocks.ForceOff;
        int bi = Array.IndexOf(args, "--blocks");
        string list = bi >= 0 && bi + 1 < args.Length ? args[bi + 1] : "wick43,chimney43,ride43";
        Wick43Blocks = off ? N43Blocks.Off : Blocks43W(list);
        Console.WriteLine($"n43wick {what}: NIGHT43 blocks {Wick43Blocks}");
        int fails = 0;
        if (what == "climb" || what == "all") fails += Climb43W();
        if (what == "band" || what == "all") fails += Band43W(seeds, cap, false);
        if (what == "casual" || what == "all") fails += Band43W(seeds, Math.Max(cap, 240), true);
        if (what == "state" || what == "all") fails += State43W(cap);
        if (what == "entry" || what == "all") fails += Entry43W();
        if (what == "bosses" || what == "all") fails += Bosses43W(Math.Max(cap, 240));
        if (what == "bosses" || what == "wyrm" || what == "all") fails += Wyrm43W(Math.Max(cap, 240));
        Console.WriteLine(fails == 0 ? "n43wick: GREEN" : $"n43wick: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static N43Blocks Blocks43W(string list)
    {
        int bits = 0;
        foreach (string n in list.Split(','))
        {
            int i = Array.IndexOf(N43Blocks.Names, n.Trim());
            if (i >= 0) bits |= 1 << i;
        }
        return N43Blocks.FromBits(bits);
    }

    static SurvivalSim Sim43W(uint seed)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n37Override = !N37Blocks.ForceOff ? new N37Blocks { wick37 = true } : N37Blocks.Off;
        sim.n43Override = N43Blocks.ForceOff ? N43Blocks.Off : Wick43Blocks;
        sim.wick37CentreOverride = Stack37;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        return sim;
    }

    sealed class Climb43Run
    {
        public double capT = -1, ledge1 = -1, ledge2 = -1, maxY;
        public int rides, burns, slag, low, slow, noIntent, shortWall, locked, presses, ridePresses, deadPresses;
        public List<int> ridden = new List<int>();
    }

    static Climb43Run ClimbRun43W(uint seed, double seconds, bool casual = false, bool press = false, double mixed = 0)
    {
        var r = new Climb43Run();
        SurvivalSim sim = Sim43W(seed);
        Wick37Stack s = sim.wick37Stack;
        sim.Respawn(Stack37 + new Vec3(0, 0, s.p.radius + 1.5), 0);
        sim.mover.invulnerable = true;
        var si = (ScriptedInput)sim.input;
        var hand = new Mulberry32(seed * 7919u + 3u);
        var pilot = new Wick37Pilot { mode = Wick37Pilot.Mode.Climb, shoot = false, parry = false, hand = hand, casual = casual, press = press, pressShare = mixed };
        if (casual) pilot.aimRate = 10;
        if (Lean43W >= 0) pilot.wallLean = Lean43W;
        if (seed > 1) { pilot.scriptRel = 0.5 + 0.2 * hand.Next(); pilot.scriptPitch = 40 + 10 * hand.Next(); pilot.scriptLedgeRel = 0.6 + 0.2 * hand.Next(); }
        double yaw = 0, dt = sim.cfg.Dt;
        int steps = (int)(seconds / dt), lastNi = 0;
        for (int i = 0; i < steps; i++)
        {
            pilot.Step(sim, yaw, i);
            yaw = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            if (pilot.Move.jump.pressed) { r.presses++; if (sim.mover.state == MoveState.WallRide) { r.ridePresses++; if (sim.mover.wallJumpsLeft <= 0) r.deadPresses++; } }
            sim.Step();
            Vec3 l = sim.mover.position - Stack37;
            r.maxY = Math.Max(r.maxY, l.y);
            double tt = i * dt;
            if (sim.mover.grounded)
            {
                if (r.ledge1 < 0 && Math.Abs(l.y - s.p.ledges[0]) < 0.3) r.ledge1 = tt;
                if (r.ledge2 < 0 && s.p.ledges.Length > 1 && Math.Abs(l.y - s.p.ledges[1]) < 0.3) r.ledge2 = tt;
                if (r.capT < 0 && l.y > s.p.capY - 0.3) { r.capT = tt; break; }
            }
            int ni = sim.mover.flow34.ride35.r40NoIntent + sim.mover.flow34.ride35.r40Slow;
            if (Trace43W && (i % Every43W == 0 || ni != lastNi))
            {
                int pi = s.NearestPanel(l, out double pd);
                Wick37Panel pn = pi >= 0 ? s.panels[pi] : default;
                double along = pi >= 0 ? ((l.x - pn.start.x) * (pn.end.x - pn.start.x) + (l.z - pn.start.z) * (pn.end.z - pn.start.z)) / Math.Max(1e-9, Vec3.Dot(pn.end - pn.start, pn.end - pn.start)) : -9;
                Vec3 w = MovementMath.WishDirection(pilot.Move.moveX, pilot.Move.moveZ, pilot.Move.yaw);
                double into = pi >= 0 ? -(w.x * pn.normal.x + w.z * pn.normal.z) : -9;
                Console.WriteLine($"    t {tt:0.00} y {l.y:0.00} r {JsMath.Hypot(l.x, l.z):0.00} a {JsMath.Atan2(l.z, l.x) * 180 / Math.PI:0} {sim.mover.state} jump {(pilot.Move.jump.held ? "H" : "-")}{(pilot.Move.jump.pressed ? "P" : "")} v {sim.mover.Speed:0.0} panel {pi} dist {pd:0.00} along {along:0.00} wish-into {into:0.00}{(ni != lastNi ? " REFUSED" : "")}");
            }
            lastNi = ni;
        }
        ref Ride35.State rr = ref sim.mover.flow34.ride35;
        r.low = rr.r40Low; r.slow = rr.r40Slow; r.noIntent = rr.r40NoIntent; r.shortWall = rr.r40Short; r.locked = rr.r40Locked;
        r.burns = sim.wick37HotBurns;
        r.slag = sim.wick37SlagTouches;
        r.rides = pilot.rides;
        r.ridden = pilot.ridden;
        return r;
    }

    /// <summary>G13: the climb share (the cap within 60 s) of 10 hands, for the held-ride climb (skilled and casual hands) and the PRESS climb
    /// (tapped wall jumps), and why ride40 refused attaches. PASS: the skilled held climb 10/10 (the others are measurements: before / after).</summary>
    static int Climb43W()
    {
        int fails = 0;
        foreach (var (name, casual, press, mixed) in new[] { ("held", false, false, 0.0), ("held casual", true, false, 0.0), ("press", false, true, 0.0), ("mixed 30 % presses", false, false, 0.3) })
        {
            int made = 0, low = 0, slow = 0, noI = 0, sh = 0, lk = 0, rides = 0, rp = 0, dead = 0, l1 = 0, l2 = 0;
            var times = new List<double>();
            for (uint sd = 1; sd <= 10; sd++)
            {
                Climb43Run q = ClimbRun43W(sd, 60, casual, press, mixed);
                if (q.capT > 0) { made++; times.Add(q.capT); }
                if (q.ledge1 >= 0) l1++;
                if (q.ledge2 >= 0) l2++;
                low += q.low; slow += q.slow; noI += q.noIntent; sh += q.shortWall; lk += q.locked; rides += q.rides; rp += q.ridePresses; dead += q.deadPresses;
                if (Trace43W || Every43W < 30) Console.WriteLine($"  {name} seed {sd}: the cap {(q.capT > 0 ? q.capT.ToString("0.0") + " s" : "never (max feet " + q.maxY.ToString("0.0") + " m)")}, ledge 1 {(q.ledge1 < 0 ? "-" : q.ledge1.ToString("0.0"))}, ledge 2 {(q.ledge2 < 0 ? "-" : q.ledge2.ToString("0.0"))}, rides {q.rides} [{string.Join(" ", q.ridden.Take(20))}], presses on a ride {q.ridePresses} (with no wall jump left {q.deadPresses}), refused: low {q.low} slow {q.slow} no-intent {q.noIntent} short-wall {q.shortWall} locked {q.locked}; hot burns {q.burns}, slag {q.slag}");
            }
            times.Sort();
            Console.WriteLine($"n43wick climb {name}: {made}/10 reach the cap (ledge 1 {l1}/10, ledge 2 {l2}/10); median {(times.Count > 0 ? times[times.Count / 2].ToString("0.0") : "-")} s; rides {rides}; presses on a ride {rp}, with no wall jump left {dead}; ride40 refusals: low {low} slow {slow} no-intent {noI} short-wall {sh} locked {lk}{(name == "held" ? ": " + Pass36(made == 10) : "")}");
            if (name == "held" && made != 10) fails++;
        }
        return fails;
    }

    sealed class Fight43Run
    {
        public bool killed;
        public double seconds, capAt = -1;
        public int deaths, restarts, parries, pierces, launches, windows, chips, slag, burns, leg;
        public int legBeats, opens, volleys, gifts, capFlares, legEvents, openEvents;
        public int invalidSteps, liveSteps, lockedSteps, openSteps;
        public string refusals = "";
        public HashSet<string> hints = new HashSet<string>();
        public List<(string, int)> tells = new List<(string, int)>();
    }

    /// <summary>One fight with the pilot (skilled or casual), mortal-counted (a 'death' is counted and healed, never played out, as wick37 band).
    /// Every step: N43BossState's validity while the fight is live (the brain past Begin and not Dead).</summary>
    static Fight43Run Fight43W(uint seed, double cap, bool casual)
    {
        var r = new Fight43Run();
        SurvivalSim sim = Sim43W(seed);
        sim.Respawn(Stack37 + new Vec3(0, 0, sim.wick37Stack.p.radius + 1.5), 0);
        int legEv = 0, openEv = 0;
        sim.events.On<Boss43Event>(N43Events.BossLeg, e => legEv++);
        sim.events.On<Boss43Event>(N43Events.BossOpen, e => openEv++);
        if (!sim.BossRoomBegin("wick", out Vec3 spawn, out double yaw0)) return r;
        Wick37 w = sim.wick37;
        var si = (ScriptedInput)sim.input;
        var pilot = new Wick37Pilot { hand = new Mulberry32(seed * 7919u + 13u), casual = casual };
        if (casual) pilot.aimRate = 10;
        double yaw = 0, dt = sim.cfg.Dt;
        int steps = (int)(cap / dt), reveal = MovementMath.StepsFor(w.p.revealSeconds, dt), capStep = -1;
        sim.health.max = sim.health.hp = 1e6;
        int i = 0;
        for (; i < steps + reveal && (w.Alive || w.brain.state == Boss37State.Dying); i++)
        {
            pilot.Step(sim, yaw, i);
            yaw = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            if (capStep < 0 && sim.mover.grounded && (sim.mover.position - Stack37).y > sim.wick37Stack.p.capY - 0.3) capStep = i;
            if (sim.health.hp < sim.health.max - 100) { r.deaths++; sim.health.hp = sim.health.max; }
            Boss43State st = sim.N43BossState;
            bool live = w.brain.state != Boss37State.Off && w.brain.state != Boss37State.Dead;
            if (live)
            {
                r.liveSteps++;
                if (!st.valid) r.invalidSteps++;
                if (st.locked) r.lockedSteps++;
                if (st.open) r.openSteps++;
                if (st.hint != null) r.hints.Add(st.hint);
            }
            if (Trace43W && i % Every43W == 0) Console.WriteLine($"    t {i * dt:0.0} y {(sim.mover.position - Stack37).y:0.0} {sim.mover.state} brain {w.brain.state} {w.AttackId} leg {w.brain.leg} hp {w.body.hp:0} | bar valid {st.valid} hp01 {st.hp01:0.00} lock {st.lockAt:0.0} locked {st.locked} open {st.open} ({st.openLeft:0.0} s) stagger {st.stagger01:0.00} leg {st.leg}/{st.legs} \"{st.hint}\"");
        }
        r.killed = !w.Alive;
        r.seconds = (i - reveal) * dt;
        r.capAt = capStep >= 0 ? (capStep - reveal) * dt : -1;
        r.parries = w.parries; r.pierces = w.pierces; r.launches = w.launches; r.windows = w.brain.windows; r.chips = w.chips;
        r.slag = sim.wick37SlagTouches; r.burns = sim.wick37HotBurns; r.restarts = w.restarts; r.leg = w.brain.leg;
        r.legBeats = w.legBeats43; r.opens = w.opens43; r.volleys = w.volleys43; r.gifts = w.gifts43; r.capFlares = w.capFlares43;
        r.legEvents = legEv; r.openEvents = openEv;
        { ref Ride35.State rr = ref sim.mover.flow34.ride35; Move43.State m = sim.mover.m43; r.refusals = $"ride40 refused low {rr.r40Low} slow {rr.r40Slow} no-intent {rr.r40NoIntent} short-wall {rr.r40Short} locked {rr.r40Locked}; move43 refills {m.refills} ride-refills {m.rideRefills} same-wall {m.sameWall} swept {m.sweptDraws} grace {m.graceSaves}"; }
        r.tells = new List<(string, int)>(w.brain.tellLog);
        return r;
    }

    /// <summary>The n37 wick band with the NIGHT43 blocks (skilled: DECIDE39 D-1's bot band 30-60 s; casual: every seed killed inside the cap).</summary>
    static int Band43W(int seeds, double cap, bool casual)
    {
        var runs = new List<Fight43Run>();
        for (uint s = 1; s <= seeds; s++)
        {
            Fight43Run r = Fight43W(s, cap, casual);
            runs.Add(r);
            Console.WriteLine($"  {(casual ? "casual" : "skilled")} seed {s}: {(r.killed ? "killed" : "ALIVE")} {r.seconds:0.0} s (the cap at {r.capAt:0.0} s), leg {r.leg}, deaths {r.deaths}, restarts {r.restarts}, parries {r.parries}, pierces {r.pierces}, windows {r.windows}, launches {r.launches}, chips {r.chips}, slag {r.slag}, hot {r.burns}; wick43: leg beats {r.legBeats} (bossLeg43 {r.legEvents}), gifts {r.gifts}, leg-2 volleys {r.volleys}, cap flares {r.capFlares}, opens {r.opens} (bossOpen43 {r.openEvents}); bar invalid {r.invalidSteps}/{r.liveSteps} steps{(Trace43W || !r.killed ? "; " + r.refusals : "")}");
        }
        var ks = runs.Where(x => x.killed).Select(x => x.seconds).OrderBy(x => x).ToList();
        double P(double q) => ks.Count == 0 ? double.NaN : ks[Math.Min(ks.Count - 1, (int)Math.Floor(q * (ks.Count - 1) + 0.5))];
        var tellSteps = runs.SelectMany(x => x.tells).GroupBy(x => x.Item1).Select(g => $"{g.Key} x{g.Count()}");
        int invalid = runs.Sum(x => x.invalidSteps);
        bool ok = casual ? ks.Count == runs.Count && invalid == 0 : ks.Count == runs.Count && P(0.5) >= 30 && P(0.9) <= 60 && invalid == 0;
        Console.WriteLine($"n43wick {(casual ? "casual" : "band")}: killed {ks.Count}/{runs.Count}; p10 {P(0.1):0.0} s, p50 {P(0.5):0.0} s, p90 {P(0.9):0.0} s; deaths {runs.Sum(x => x.deaths)}, slag {runs.Sum(x => x.slag)}; tells {string.Join(", ", tellSteps)}; bar invalid steps {invalid} ({(casual ? "want every seed killed" : "want 30 <= p50, p90 <= 60 s (DECIDE39 D-1)")}, 0 invalid): {Pass36(ok)}");
        return ok ? 0 : 1;
    }

    /// <summary>G14 / G16 / G15 in one fight (seed 1, skilled): the bar valid every live step, the states it showed, the hints, the leg beats and
    /// their events, the variations used; then the reveal's look ease on a still player (first, then a retry), and a player who turns (yields).</summary>
    static int State43W(double cap)
    {
        int fails = 0;
        bool on = Wick43Blocks.wick43 && !N43Blocks.ForceOff;
        Fight43Run r = Fight43W(1, cap, false);
        Console.WriteLine($"n43wick state: killed {r.killed} in {r.seconds:0.0} s; bar valid {r.liveSteps - r.invalidSteps}/{r.liveSteps} live steps, locked {r.lockedSteps}, open {r.openSteps}; hints seen ({r.hints.Count}): {string.Join(" | ", r.hints)}");
        Console.WriteLine($"  legs: beats {r.legBeats}, bossLeg43 {r.legEvents}, gifts {r.gifts}, leg-2 second volleys {r.volleys}, cap flares {r.capFlares}, bossOpen43 {r.openEvents} (opens {r.opens})");
        if (on)
        {
            // the lock: a fresh fight, the reveal stepped out, the bar pushed onto leg 1's lock (a hit far over it): LOCKED and its hint
            SurvivalSim ls = Sim43W(1);
            ls.Respawn(Stack37 + new Vec3(0, 0, ls.wick37Stack.p.radius + 1.5), 0);
            ls.BossRoomBegin("wick", out _, out _);
            for (int k = 0; k < 400; k++) { ((ScriptedInput)ls.input).Set(new InputFrame(), new CombatInput { weapon = -1 }); ls.Step(); }
            ls.wick37.Apply(ls.wick37.body.maxHp);
            Boss43State lk = ls.N43BossState;
            bool okLock = lk.valid && lk.locked && Math.Abs(lk.hp01 - lk.lockAt) < 1e-6 && lk.hint == ls.cfg.Str("design.n43.wick43.hints.locked");
            Console.WriteLine($"  the lock: a hit of the whole pool in leg 1 leaves hp01 {lk.hp01:0.00} (lockAt {lk.lockAt:0.00}), locked {lk.locked}, hint \"{lk.hint}\": {Pass36(okLock)}");
            if (!okLock) fails++;
            bool okBar = r.invalidSteps == 0 && r.liveSteps > 0 && r.openSteps > 0 && r.hints.Count >= 4;
            bool okLegs = r.legBeats == 2 && r.legEvents == 2 && r.gifts == 2 && r.volleys > 0 && r.openEvents > 0;
            Console.WriteLine($"  the bar valid every live step, OPEN shown, >= 4 hints: {Pass36(okBar)}; two leg beats with bossLeg43, two gifts, a leg-2 second volley, bossOpen43: {Pass36(okLegs)}");
            if (!okBar) fails++;
            if (!okLegs) fails++;
        }
        else
        {
            bool none = r.liveSteps > 0 && r.invalidSteps == r.liveSteps && r.legEvents == 0 && r.openEvents == 0 && r.volleys == 0;
            Console.WriteLine($"  wick43 off: no bar state, no NIGHT43 event, no variation (NIGHT42's fight): {Pass36(none)}");
            if (!none) fails++;
        }
        fails += Reveal43W(on);
        return fails;
    }

    /// <summary>The reveal (G15, D-7): a still player at the tunnel mouth looking away (south) - the yaw error to THE WICK at the start and the end
    /// of the reveal; a retry's shorter window; and a player turning 2 deg a step (his own look) - the ease yields.</summary>
    static int Reveal43W(bool on)
    {
        int fails = 0;
        foreach (string kind in new[] { "still", "turning" })
        {
            SurvivalSim sim = Sim43W(1);
            sim.Respawn(Stack37 + new Vec3(0, 0, sim.wick37Stack.p.radius + 1.5), 0);
            var si = (ScriptedInput)sim.input;
            double look0 = 0.6; // radians: the player looks 34 degrees off the shaft's axis
            si.SetLook(look0, 0);
            sim.Step();
            sim.BossRoomBegin("wick", out _, out _);
            Wick37 w = sim.wick37;
            double Err() { Vec3 d = w.Centre - sim.Eye; return Math.Abs(WrapPi43T(JsMath.Atan2(-d.x, -d.z) - sim.input.Yaw)) * 180 / Math.PI; }
            double e0 = Err(), eMin = e0;
            int n = sim.wick43RevealSteps;
            for (int k = 0; k < n + 10; k++)
            {
                if (kind == "turning") si.SetLook(sim.input.Yaw + 2 * Math.PI / 180, sim.input.Pitch);
                si.Set(new InputFrame(), new CombatInput { weapon = -1 });
                sim.Step();
                eMin = Math.Min(eMin, Err());
            }
            double e1 = Err();
            Console.WriteLine($"  reveal ({kind}): first {sim.wick43RevealFirst}, {n} steps ({n * sim.cfg.Dt:0.00} s), eased steps {sim.wick43EasedSteps}, yielded {sim.wick43LookYielded}; yaw error to THE WICK {e0:0.0} -> min {eMin:0.0} -> end {e1:0.0} deg");
            if (on && kind == "still" && !(sim.wick43RevealFirst && e1 < 10 && e0 > 20)) fails++;
            if (on && kind == "turning" && !(sim.wick43LookYielded && sim.wick43EasedSteps <= 2)) fails++;
            if (!on && sim.wick43EasedSteps != 0) fails++;
            if (on && kind == "still")
            {
                // a retry: Wick37Begin again with the fight live (a death's retry): the short window
                sim.BossRoomBegin("wick", out _, out _);
                Console.WriteLine($"  reveal (retry): first {sim.wick43RevealFirst}, {sim.wick43RevealSteps} steps ({sim.wick43RevealSteps * sim.cfg.Dt:0.00} s)");
                if (sim.wick43RevealFirst || sim.wick43RevealSteps >= n) fails++;
            }
        }
        Console.WriteLine($"  the reveal eases the look onto THE WICK, yields to the player's own turn, and a retry is shorter: {Pass36(fails == 0)}");
        return fails;
    }

    /// <summary>G10's "brick wall" at THE STACK's door: the player comes through the tunnel at 18 m/s (a dash / boost carried in) on the floor and
    /// in a jump; the speed the shaft's air leaves 0.1 s and 0.4 s after crossing the shaft's wall, and the biggest one-step loss.</summary>
    static int Entry43W()
    {
        foreach (bool air in new[] { false, true })
        {
            SurvivalSim sim = Sim43W(1);
            Wick37Stack s = sim.wick37Stack;
            sim.Respawn(Stack37 + new Vec3(0, 0, s.p.radius + 3.5), 0);
            var si = (ScriptedInput)sim.input;
            si.SetLook(0, 0); // yaw 0 looks along -z (into the shaft)
            sim.mover.velocity = new Vec3(0, air ? 4 : 0, -18);
            int crossed = -1;
            double at01 = -1, at04 = -1, worst = 0, last = 18;
            for (int k = 0; k < 120; k++)
            {
                si.Set(new InputFrame { moveZ = 1, sprint = new Skygrind.Core.Button { held = true } }, new CombatInput { weapon = -1 });
                sim.Step();
                Vec3 l = sim.mover.position - Stack37;
                double hs = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z);
                if (crossed < 0 && JsMath.Hypot(l.x, l.z) < s.p.radius) { crossed = k; worst = Math.Max(worst, last - hs); }
                else if (crossed >= 0) worst = Math.Max(worst, last - hs);
                if (crossed >= 0 && k - crossed == 12) at01 = hs;
                if (crossed >= 0 && k - crossed == 48) at04 = hs;
                last = hs;
            }
            Console.WriteLine($"n43wick entry ({(air ? "a jump in" : "on the floor")}, 18 m/s through the tunnel): crossed the shaft's wall at step {crossed}; speed 0.1 s later {at01:0.0} m/s, 0.4 s later {at04:0.0} m/s; the biggest one-step loss {worst:0.0} m/s; floor steps left alone {sim.wick43AirFloor}, eased steps {sim.wick43AirEased}");
        }
        return 0;
    }

    /// <summary>Round 2 (DESIGN-RULES B-1, every boss): THE ICEBREAKER fought by NIGHT40's skilled pilot (Brawl40Pilot, its arena at 3000, 0, 0)
    /// with the blocks on: N43BossState valid on every live step, the phases seen, OPEN steps (window / stuck), the hints, bossOpen43 / bossLeg43.</summary>
    static int Bosses43W(double cap)
    {
        int fails = 0;
        bool on = Wick43Blocks.wick43 && !N43Blocks.ForceOff;
        N43Blocks? was = Brawl40N43;
        Brawl40N43 = N43Blocks.ForceOff ? N43Blocks.Off : Wick43Blocks;
        SurvivalSim sim = Brawl40Sim(1, true);
        Brawl40N43 = was;
        double dt = sim.cfg.Dt;
        sim.Respawn(sim.brawl40Arena.spawn + Brawl40At, 0);
        Vec3 spawn = default;
        double yaw = 0;
        int openEv = 0, legEv = 0;
        sim.events.On<Boss43Event>(N43Events.BossOpen, e => { if (e.boss != "THE WICK") openEv++; });
        sim.events.On<Boss43Event>(N43Events.BossLeg, e => { if (e.boss != "THE WICK") legEv++; });
        if (!sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw)) { Console.WriteLine("n43wick bosses: THE ICEBREAKER did not begin: FAIL"); return 1; }
        Brawl40 g = sim.brawl40;
        var si = (ScriptedInput)sim.input;
        var pilot = new Brawl40Pilot { hand = new Mulberry32(7919u + 40u), style = Brawl40Pilot.Style.Mixed };
        int live = 0, invalid = 0, openSteps = 0, stuckOpen = 0, windowOpen = 0, phase2 = 0;
        var hints = new HashSet<string>();
        double yawL = 0;
        int step = 0, capSteps = (int)(cap / dt);
        while ((g.Alive || g.state == Brawl40State.Dying) && step < capSteps)
        {
            pilot.Step(sim, yawL, step);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (sim.health.Dead || sim.flow == Skygrind.Game.Flow.Down) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; sim.health.Reset(); sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw); sim.Respawn(spawn, yaw); }
            if (g.state == Brawl40State.Off || g.state == Brawl40State.Reveal || g.state == Brawl40State.Dead) continue;
            Boss43State st = sim.N43BossState;
            live++;
            if (!st.valid) { invalid++; continue; }
            if (st.open) { openSteps++; if (g.StuckNow) stuckOpen++; if (g.Open) windowOpen++; }
            if (st.leg == 2) phase2++;
            if (st.hint != null) hints.Add(st.hint);
        }
        Console.WriteLine($"n43wick bosses: THE ICEBREAKER {(g.Alive ? "ALIVE" : "killed")} in {step * dt:0.0} s; bar valid {live - invalid}/{live} live steps, phase 2 steps {phase2}, OPEN steps {openSteps} (stuck {stuckOpen}, window {windowOpen}); bossOpen43 {openEv}, bossLeg43 {legEv}; hints ({hints.Count}): {string.Join(" | ", hints)}");
        bool ok = on ? live > 0 && invalid == 0 && stuckOpen > 0 && phase2 > 0 && openEv > 0 && legEv >= 1 && hints.Count >= 3 : invalid == live && openEv == 0 && legEv == 0;
        Console.WriteLine($"  {(on ? "the bar valid every live step, OPEN while stuck, phase 2 reached, bossOpen43 + bossLeg43, >= 3 hints" : "wick43 off: no state, no event")}: {Pass36(ok)}");
        if (!ok) fails++;
        return fails;
    }

    /// <summary>Round 2: THE KILN WYRM on n38-kiln's k5 (Kiln39Band's setup and boss32's WyrmPilot31), the blocks on: the bar valid every live
    /// step, phases 2 and 3 seen, OPEN steps, hints, bossOpen43 / bossLeg43.</summary>
    static int Wyrm43W(double cap)
    {
        bool on = Wick43Blocks.wick43 && !N43Blocks.ForceOff;
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        try
        {
            string path = System.IO.Path.Combine(MapGenRepo, "Design", "levels", "n38-kiln.json");
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noLab = sim.noRecords = true;
            sim.n43Override = N43Blocks.ForceOff ? N43Blocks.Off : Wick43Blocks;
            sim.setup.mode = Campaign24.Mode;
            sim.StartRun(1, null, null);
            sim.husks.frozen = true;
            for (int i = 0; i < 10; i++) sim.Step();
            sim.campaign.SetDoorFree29(true);
            RoomDef k5 = sim.campaignLevel.rooms.Find(r => r.id == "k5");
            sim.Respawn(k5.spawn, k5.spawnYawDeg);
            foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
            int openEv = 0, legEv = 0;
            sim.events.On<Boss43Event>(N43Events.BossOpen, e => openEv++);
            sim.events.On<Boss43Event>(N43Events.BossLeg, e => legEv++);
            int wait = 0;
            while ((sim.wyrm24 == null || !sim.wyrm24.Alive) && wait++ < 600) sim.Step();
            if (sim.wyrm24 == null || !sim.wyrm24.Alive) { Console.WriteLine("n43wick wyrm: NO FIGHT: FAIL"); return 1; }
            Wyrm w = sim.wyrm24;
            var si = (ScriptedInput)sim.input;
            var wp = new WyrmPilot31();
            int step = 0, capSteps = (int)(cap * 120), live = 0, invalid = 0, open = 0, maxLeg = 0;
            var hints = new HashSet<string>();
            Vec3 retry = k5.spawn;
            while (w.Alive && step < capSteps)
            {
                wp.Step(sim, step);
                si.SetLook(wp.Move.yaw, wp.Move.pitch);
                si.Set(wp.Move, wp.Combat);
                sim.Step();
                sim.hitstopLeft = 0;
                step++;
                if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; sim.health.Reset(); if (sim.BossRoomBegin("wyrm", out Vec3 sp, out _)) retry = sp; sim.Respawn(retry, 0); }
                if (w.phase == "intro" || w.phase == "dead") continue;
                live++;
                Boss43State st = sim.N43BossState;
                if (!st.valid) { invalid++; continue; }
                if (st.open) open++;
                maxLeg = Math.Max(maxLeg, st.leg);
                if (st.hint != null) hints.Add(st.hint);
            }
            Console.WriteLine($"n43wick wyrm: THE KILN WYRM {(w.Alive ? "ALIVE" : "killed")} in {step / 120.0:0.0} s; bar valid {live - invalid}/{live} live steps, phase up to {maxLeg}/3, OPEN steps {open}; bossOpen43 {openEv}, bossLeg43 {legEv}; hints ({hints.Count}): {string.Join(" | ", hints)}");
            bool ok = on ? live > 0 && invalid == 0 && maxLeg >= 2 && open > 0 && openEv > 0 && legEv >= 1 : invalid == live && openEv == 0;
            Console.WriteLine($"  {(on ? "the bar valid every live step, a phase change with bossLeg43, OPEN with bossOpen43" : "wick43 off: no state, no event")}: {Pass36(ok)}");
            return ok ? 0 : 1;
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
    }

    static double WrapPi43T(double a)
    {
        while (a > Math.PI) a -= 2 * Math.PI;
        while (a < -Math.PI) a += 2 * Math.PI;
        return a;
    }
}
