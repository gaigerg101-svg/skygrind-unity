// NIGHT34 builder B (B1, design.n34.bend34): the grapple BEND's headless judge. ONE entry method, N34Bend(cmd, args, ref code) (declared and called
// in Program.cs). The rows are forced on inside each run by sim.n34Override (bend34, and bendSplit34 for the split runs), whatever the config says.
//   bend34 [--seed n]   the SURVIVAL map, the ARCLINE owned, NIGHT32's charges off for the latch bots (n32Override: the charges would cap 20 latches in
//                       30 s), invulnerable. The latch bot is NIGHT32's TAP bot (Harness.N32.Grapple.cs G32Target: a node, a big enemy, the farthest wall).
//     (a) every latch gives a short bend: on every step a latch fired (the Mover's RopeAttach or the ARCLINE's yank), the bend is on after the step.
//     (b) 20 latches in 30 s, no kills (no enemies, no fire): exactly 1 FULL bend (the starting meter), the rest SHORT.
//     (c) global: the sim's share on every Hold step is 0.25 +- 0.02. split: inside a bend the world steps 1 in N (N = round(1 / scale)); the split
//         table: on a held step no enemy moves (the world), while the player moves, fires and latches (the player's hooks: mover, loadout, grapple,
//         whip zips counted when the bot takes one), round 1 live.
//     (d) the instant swap: inside a bend the gun in hand changes on the press step; outside a bend (row on) and with the row off the swap takes the
//         same number of steps (NIGHT33's timeline).
//     (e) two tape replays EQUAL (n33-span: its nodes own the ARCLINE from the start, campaign, the latch bot firing and swapping, 25 s): global and split.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N34Bend(string cmd, string[] args, ref int code)
    {
        if (cmd == "bend34") Bend34Command(args, ref code);
        else if (cmd == "air34") Air34Command(args, ref code); // leftover 1 (Harness.N34.Air.cs)
        else if (cmd == "combo34") Combo34Command(args, ref code); // leftover 3 (Harness.N34.Combo.cs)
        else if (cmd == "rea34") Rea34Command(args, ref code); // the REA addendum: arsenal34, quickdraw34, whipready34 (Harness.N34.Combo.cs)
    }

    static string B34f(double v, string f = "0.000") => v.ToString(f, CultureInfo.InvariantCulture);

    static SurvivalSim B34Sim(GameConfig cfg, bool on, bool split, uint seed, ScriptedInput si, bool live)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n32Override = new N32Blocks(); // the charges off: they would cap the latch count (b)
        sim.n34Override = new N34Blocks { bend34 = on, bendSplit34 = on && split };
        sim.UseInput(si);
        sim.StartRun(seed, null, null);
        if (sim.arcline21 == null) return null;
        sim.arcline21.Own(true);
        if (live) sim.run.Ready();
        else sim.holdRunClock = true;
        for (int s = 0; s < 30; s++) B34Tick(sim, si, null, false, false, false, -1);
        return sim;
    }

    static void B34Tick(SurvivalSim sim, ScriptedInput si, Vec3? aim, bool held, bool pressed, bool fire, int weapon)
    {
        if (aim.HasValue)
        {
            (double yaw, double pitch) = ArclineLook(sim.Eye, aim.Value);
            si.SetLook(yaw, pitch);
        }
        si.Set(new InputFrame { grapple = new Button { held = held, pressed = pressed } }, new CombatInput { weapon = weapon, fire = new Button { held = fire, pressed = fire } });
        sim.health.hp = sim.health.max;
        sim.mover.invulnerable = true;
        sim.hitstopLeft = 0;
        sim.Step();
    }

    /// <summary>The latch bot (NIGHT32's TAP / HOLD bots): whenever the rope is idle and off cooldown, aim at G32Target and tap (press one step) or,
    /// with holdSteps > 0, hold that long (the swing: it attaches to a bare wall too, where NIGHT32's tapNodesOnly refuses a tap).</summary>
    sealed class B34Bot
    {
        int tapUp, holdLeft;
        Vec3? aim;
        public int holdSteps;

        public (Vec3? aim, bool held, bool pressed) Next(SurvivalSim sim)
        {
            Rope rope = sim.mover.rope;
            bool press = false;
            if (rope.phase == RopePhase.None && tapUp == 0 && holdLeft == 0)
            {
                aim = G32Target(sim, rope);
                if (aim.HasValue && rope.cooldownSteps == 0)
                {
                    press = true;
                    if (holdSteps > 0) holdLeft = holdSteps;
                    else tapUp = 1;
                }
            }
            else if (holdLeft > 0)
            {
                holdLeft--;
                return (aim, true, false);
            }
            else if (tapUp > 0) tapUp--;
            return (aim, press, press);
        }
    }

    static void Bend34Command(string[] args, ref int code)
    {
        uint seed = uint.Parse(Opt30(args, "--seed", "3401"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        Bend34Rules rules = Bend34Rules.From(cfg);
        double dt = 1 / cfg.movement.loop.simHz;
        int Steps(double sec) => MovementMath.StepsFor(sec, dt);
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }
        int n = (int)JsMath.Max(1, JsMath.Round(1 / rules.scale));
        Console.WriteLine($"bend34: scale {rules.scale} (split N {n}), short {rules.shortS} s, full {rules.holdS} s +{rules.killExtendS} s / kill (max {rules.maxS} s), ease {rules.inS} / {rules.outS} s, refill {rules.refillPerKill} / kill (x{rules.rankDoubleMult} from rank index {rules.doubleRank}); the rows forced on per run");

        // (a) + (b): 30 s of the latch bot, no enemies, no fire, global and split
        foreach (bool split in new[] { false, true })
        {
            var si = new ScriptedInput();
            SurvivalSim sim = B34Sim(cfg, true, split, seed, si, false);
            if (sim == null) { fails.Add("no ARCLINE"); break; }
            bool latchedThisStep = false;
            int latchSteps = 0, bentAfter = 0, kills = 0, starts = 0, fullStarts = 0;
            sim.mover.Events += e => { if (e.type == MoverEventType.RopeAttach) latchedThisStep = true; };
            sim.events.On("arclinePull", _ => latchedThisStep = true);
            sim.events.On<KillEvent>("kill", _ => kills++);
            sim.events.On<Bend34Event>("bend34Start", e => { starts++; if (e.tier == "full") fullStarts++; });
            var bot = new B34Bot();
            int latches0 = sim.bend34.latches;
            for (int s = 0; s < Steps(30) && sim.bend34.latches - latches0 < 20; s++)
            {
                latchedThisStep = false;
                (Vec3? aim, bool held, bool pressed) = bot.Next(sim);
                B34Tick(sim, si, aim, held, pressed, false, -1);
                if (!latchedThisStep) continue;
                latchSteps++;
                if (sim.bend34.tier != Bend34Tier.None) bentAfter++;
            }
            int latches = sim.bend34.latches - latches0;
            Console.WriteLine($"  ({(split ? "split" : "global")}) (a) latch steps {latchSteps}, the bend on after {bentAfter} / {latchSteps}; (b) latches {latches} in <= 30 s, kills {kills}: bends started {starts} (FULL {sim.bend34.fullBends}, SHORT {sim.bend34.shortBends}), bend34Start full events {fullStarts}, meter {B34f(sim.bend34.meter, "0.00")}");
            Need(latchSteps > 0 && bentAfter == latchSteps, $"(a) {(split ? "split" : "global")}: every latch bends ({bentAfter} / {latchSteps})");
            Need(latches >= 20, $"(b) {(split ? "split" : "global")}: 20 latches in 30 s ({latches})");
            Need(kills == 0 && sim.bend34.fullBends == 1 && fullStarts == 1, $"(b) {(split ? "split" : "global")}: exactly 1 FULL bend with no kills ({sim.bend34.fullBends})");
        }

        // (c) global: the share on every Hold step
        {
            var si = new ScriptedInput();
            SurvivalSim sim = B34Sim(cfg, true, false, seed, si, false);
            sim.Bend34LatchForCheck();
            double lo = 1, hi = 0, sum = 0;
            int hold = 0, inside = 0;
            for (int s = 0; s < Steps(8); s++)
            {
                B34Tick(sim, si, null, false, false, false, -1);
                if (sim.bend34.Inside) inside++;
                if (sim.bend34.phase != Bend34Phase.Hold) continue;
                hold++;
                lo = Math.Min(lo, sim.Bend34Share);
                hi = Math.Max(hi, sim.Bend34Share);
                sum += sim.Bend34Share;
            }
            Console.WriteLine($"  (c) global: FULL bend: {inside} sim steps inside ({sim.bend34.holdSteps} = StepsFor({rules.holdS} x {rules.scale})), {hold} Hold steps: share min {B34f(lo)} max {B34f(hi)} mean {B34f(hold > 0 ? sum / hold : 0)}; n34HoldWorld never (world stepped every step: {sim.Bend34WorldStepped})");
            Need(hold > 0 && lo >= rules.scale - 0.02 && hi <= rules.scale + 0.02, "(c) global share 0.25 +- 0.02");
        }

        // (c) split: the world 1 in N, the split table
        {
            var si = new ScriptedInput();
            SurvivalSim sim = B34Sim(cfg, true, true, seed + 1, si, true);
            for (int s = 0; s < Steps(4); s++) B34Tick(sim, si, null, false, false, false, -1); // round 1's first husks come in
            bool latched = false;
            int shotsNow = 0, whipNow = 0;
            sim.mover.Events += e => { if (e.type == MoverEventType.RopeAttach) latched = true; };
            sim.events.On("arclinePull", _ => latched = true);
            sim.events.On<ShotEvent>("shotResult", e => { if (e.owner == "player") shotsNow++; });
            sim.events.On<Whip33Event>("whip33Latch", _ => whipNow++);
            var bot = new B34Bot();
            int inside = 0, world = 0, held = 0, enemyMovedHeld = 0, enemyMovedWorld = 0, playerMovedHeld = 0, shotsHeld = 0, latchHeld = 0, whipHeld = 0, liveEnemies = 0;
            var before = new Dictionary<int, Vec3>();
            for (int s = 0; s < Steps(30); s++)
            {
                before.Clear();
                foreach (Enemy e in sim.enemies.all) if (e.alive) before[e.id] = e.position;
                Vec3 p0 = sim.mover.position;
                latched = false;
                shotsNow = whipNow = 0;
                (Vec3? aim, bool gh, bool gp) = bot.Next(sim);
                B34Tick(sim, si, aim, gh, gp, true, -1);
                bool wasInside = sim.bend34.Inside;
                bool heldStep = !sim.Bend34WorldStepped;
                if (heldStep && !wasInside) fails.Add("a held step outside a bend");
                if (!wasInside && !heldStep) continue;
                inside++;
                int moved = 0;
                foreach (Enemy e in sim.enemies.all)
                    if (e.alive && before.TryGetValue(e.id, out Vec3 b) && Vec3.DistanceTo(b, e.position) > 1e-12) moved++;
                liveEnemies = Math.Max(liveEnemies, before.Count);
                if (heldStep)
                {
                    held++;
                    enemyMovedHeld += moved;
                    if (Vec3.DistanceTo(p0, sim.mover.position) > 1e-9) playerMovedHeld++;
                    shotsHeld += shotsNow;
                    if (latched) latchHeld++;
                    whipHeld += whipNow;
                }
                else
                {
                    world++;
                    enemyMovedWorld += moved;
                }
            }
            Console.WriteLine($"  (c) split: steps inside a bend {inside}: world steps {world}, held {held} (1 in {(world > 0 ? B34f(inside / (double)world, "0.00") : "-")}, want 1 in {n}); share stayed {B34f(sim.Bend34Share, "0.00")}");
            Console.WriteLine($"      split table, held steps: enemies moved {enemyMovedHeld} (world: none) vs {enemyMovedWorld} on world steps (up to {liveEnemies} live); player moved on {playerMovedHeld} / {held} (mover), shots {shotsHeld} (loadout), latches {latchHeld} (grapple), whip zips {whipHeld} (whip); kills {sim.run.stats.kills}, FULL {sim.bend34.fullBends} SHORT {sim.bend34.shortBends}, kill extends {sim.bend34.killExtends}");
            Need(held > 0 && world > 0 && Math.Abs(inside / (double)world - n) < 0.5, $"(c) split: the world steps 1 in {n} ({world} / {inside})");
            Need(enemyMovedHeld == 0 && enemyMovedWorld > 0, "(c) split: no enemy moves on a held step, they move on world steps");
            Need(playerMovedHeld > 0 && shotsHeld > 0 && latchHeld > 0, "(c) split: the player moves, fires and latches on held steps");
        }

        // (d) the instant swap
        {
            int SwapSteps(bool on, bool bend, out int pressSlot, out int instant)
            {
                var si = new ScriptedInput();
                SurvivalSim sim = B34Sim(cfg, on, false, seed, si, false);
                if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
                if (!sim.loadout.owned.Contains("shotgun")) sim.loadout.AddWeapon("shotgun");
                for (int s = 0; s < Steps(0.6); s++) B34Tick(sim, si, null, false, false, false, -1);
                if (bend)
                {
                    sim.Bend34LatchForCheck();
                    for (int s = 0; s < 4; s++) B34Tick(sim, si, null, false, false, false, -1);
                }
                int from = sim.loadout.slot, want = (from + 1) % sim.loadout.owned.Count;
                pressSlot = want;
                for (int s = 0; s < Steps(1); s++)
                {
                    B34Tick(sim, si, null, false, false, false, s == 0 ? want : -1);
                    if (sim.loadout.slot == want)
                    {
                        instant = sim.loadout.bend34InstantSwaps;
                        return s;
                    }
                }
                instant = sim.loadout.bend34InstantSwaps;
                return -1;
            }
            int inBend = SwapSteps(true, true, out _, out int inst), noBend = SwapSteps(true, false, out _, out int inst2), off = SwapSteps(false, false, out _, out _);
            Console.WriteLine($"  (d) the gun in hand changes {inBend} steps after the press inside a bend (instant swaps {inst}); outside a bend (row on) {noBend} steps (instant {inst2}); row off {off} steps (NIGHT33's timeline)");
            Need(inBend == 0 && inst == 1, "(d) inside a bend: the press step");
            Need(noBend == off && off > 0 && inst2 == 0, "(d) outside a bend / row off: the swap timeline unchanged");
        }

        // (e) two tape replays EQUAL (global, split)
        foreach (bool split in new[] { false, true })
        {
            string path = Path.Combine("Design", "levels", "n33-span.json");
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.n34Override = new N34Blocks { bend34 = true, bendSplit34 = split };
            var si = new ScriptedInput();
            sim.UseInput(si);
            var bot = new B34Bot { holdSteps = Steps(0.6) };
            RunTape tape = TapeScript.Record(sim, seed, Steps(25), "bend34-tape", (g, i) =>
            {
                (Vec3? aim, bool gh, bool gp) = bot.Next(g);
                if (aim.HasValue)
                {
                    (double yaw, double pitch) = ArclineLook(g.Eye, aim.Value);
                    si.SetLook(yaw, pitch);
                }
                int w = i % 90 == 45 && g.loadout.owned.Count > 1 ? (g.loadout.slot + 1) % g.loadout.owned.Count : -1;
                si.Set(new InputFrame { grapple = new Button { held = gh, pressed = gp } }, new CombatInput { weapon = w, fire = new Button { held = i % 40 < 20, pressed = i % 40 == 0 } });
            }, null, scripted: false);
            RunTape back = RunTape.FromBytes(tape.ToBytes());
            TapeResult rep = TapeReplay.Replay(cfg, map, back);
            bool bits = N34Blocks.FromBits(back.n34).bend34 && N34Blocks.FromBits(back.n34).bendSplit34 == split;
            Console.WriteLine($"  (e) {(split ? "split" : "global")} tape: {sim.run.steps} steps on n33-span, ARCLINE owned {sim.arcline21 != null && sim.arcline21.Owned}, latches {sim.bend34.latches}, bends FULL {sim.bend34.fullBends} SHORT {sim.bend34.shortBends}, instant swaps {sim.loadout.bend34InstantSwaps}, kills {sim.run.stats.kills}; n34 bits {back.n34} ({(bits ? "PASS" : "FAIL")}); replay {(rep.Equal ? "EQUAL" : "DIFFERS: " + rep.Line)} ({rep.Checkpoints} of {rep.CheckpointsTotal} checkpoints)");
            Need(bits && rep.Equal, $"(e) {(split ? "split" : "global")} tape replay EQUAL");
            Need(sim.bend34.latches > 0, $"(e) {(split ? "split" : "global")}: the tape has bends");
        }

        Console.WriteLine(fails.Count == 0 ? "bend34 GREEN" : $"bend34 FAIL: {string.Join("; ", fails)}");
        code = fails.Count == 0 ? 0 : 1;
    }
}
