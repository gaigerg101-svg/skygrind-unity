// NIGHT57 builder G (G2 press57; Core/Press57.cs, SurvivalSim.N57.Press.cs): two things in `n46flow`, both silent while the row is off (the flow
// file with --off57, or with the row off, stays equal to Logs/n57/flow-base.txt line for line):
//   THE PILOT'S ANSWER  (F1Input's one line, Harness.N46.Flow.cs) the walk pilot answers a press57 circle as a player does: F57ReactS after a
//                       circle appears under its feet (inside markR) it steps out of it, away from the circle's centre, keeping its own look and
//                       trigger (it aims and fires as before), until it stands F57ExitM beyond the circle's edge; the lava rule after it still
//                       wins (a step into lava is refused there). Without an answer the bot measures a player who never sees the floor.
//   THE PRESS57 LINE    (after TOKENS / RANGED / IDLE; fed from F56TokStep / F56TokTable, the tokens56 meter's own hooks) per level, in the
//                       measured rooms' fights: spots marked, volleys thrown at a mark, the pilot's answers (count, s), and the damage the player
//                       took from the spitters (globs "spitter" + their acid "acid") split by the player's horizontal speed at the hurt: moving
//                       (>= 2 m/s, the stall threshold: 25 % of ground.walkSpeed) or still, per minute of fight in that state. The guard "a moving
//                       player's hits taken not up" is the moving column against the row-off run's.
// Bot numbers only (no gameplay effect): F57ReactS, F57ExitM.
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    // the pilot's answer (bot numbers): a human's reaction to a red-orange circle under the feet (~0.25-0.35 s to a visual cue; the middle), and
    // the step past the circle's edge it takes before it stops answering (a body width and a margin: the capsule is 0.4 m)
    const double F57ReactS = 0.3, F57ExitM = 1.0;
    static SurvivalSim f57Sim;
    static SpitterAcid.Mark57 f57Flee;
    static long f57Answers, f57AnswerSteps, f57Marks, f57Strikes, f57StepsMove, f57StepsStill;
    static double f57DmgMove, f57DmgStill;
    static int f57HitsMove, f57HitsStill;
    static bool f57Seen;
    /// <summary>`--g57` on n46flow's command line: the PRESS57 line also with the row off (the damage split's row-off side; flow-base never
    /// has the flag, so its file stays as it was).</summary>
    static bool F57Meter => f57Meter ??= Array.IndexOf(Environment.GetCommandLineArgs(), "--g57") >= 0;
    static bool? f57Meter;

    /// <summary>F1Input's line: a press57 circle under the pilot's feet for F57ReactS turns its move away from the centre (its look and trigger
    /// kept) until it stands F57ExitM past the edge. Row off: returns at its first test, the frame untouched.</summary>
    static void F57Answer(SurvivalSim sim, ref InputFrame f)
    {
        f57MoveIn = Math.Abs(f.moveX) + Math.Abs(f.moveZ); // the pilot's own stick this step (the meter's still-by-choice / blocked split)
        if (!sim.Press57On || sim.husks == null || sim.husks.acid.marks57 == null || sim.campaign == null || sim.campaign.phase != Campaign24.Phase.Fight) return;
        F57Hook(sim);
        SpitterAcid acid = sim.husks.acid;
        Vec3 p = sim.mover.position;
        double r = acid.p57.markR;
        if (f57Flee != null && (!f57Flee.alive || JsMath.Hypot(p.x - f57Flee.at.x, p.z - f57Flee.at.z) > r + F57ExitM)) f57Flee = null;
        if (f57Flee == null)
        {
            foreach (SpitterAcid.Mark57 m in acid.marks57)
            {
                if (!m.alive || m.age < F57ReactS || Math.Abs(p.y - m.at.y) > 2 || JsMath.Hypot(p.x - m.at.x, p.z - m.at.z) > r) continue;
                f57Flee = m;
                f57Answers++;
                break;
            }
            if (f57Flee == null) return;
        }
        double dx = p.x - f57Flee.at.x, dz = p.z - f57Flee.at.z, len = JsMath.Hypot(dx, dz);
        if (len < 0.2)
        {
            Vec3 side = MovementMath.WishDirection(1, 0, f.yaw); // on the centre: a strafe right
            dx = side.x; dz = side.z;
        }
        else { dx /= len; dz /= len; }
        Vec3 fwd = MovementMath.WishDirection(0, 1, f.yaw), right = MovementMath.WishDirection(1, 0, f.yaw);
        double mz0 = f.moveZ, mx0 = f.moveX;
        f.moveZ = dx * fwd.x + dz * fwd.z;
        f.moveX = dx * right.x + dz * right.z;
        f57AnswerSteps++;
        if (F1Trace && f57AnswerSteps % 20 == 0)
            Console.WriteLine($"      F57 answer: at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}) v {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.0} grounded {sim.mover.grounded} state {sim.mover.state} mark ({f57Flee.at.x:0.0}, {f57Flee.at.z:0.0}) d {len:0.0} age {f57Flee.age:0.00} move ({mx0:0.0}, {mz0:0.0}) -> ({f.moveX:0.0}, {f.moveZ:0.0}) slide {f.slide.held} jump {f.jump.held} dash {f.dash.held}");
    }

    static void F57Hook(SurvivalSim sim)
    {
        if (ReferenceEquals(sim, f57Sim)) return;
        F57Flush();
        f57Sim = sim;
        f57Flee = null;
        sim.events.On<HurtEvent>("hurt", h =>
        {
            if (!(sim.Press57On || F57Meter) || h.owner != "player" || (h.source != "spitter" && h.source != "acid") || !F56Live(sim)) return;
            double v = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z);
            if (v >= 0.25 * sim.cfg.Num("ground.walkSpeed")) { f57HitsMove++; f57DmgMove += h.amount; }
            else { f57HitsStill++; f57DmgStill += h.amount; }
        });
    }

    /// <summary>A seed's end (a new sim): its acid's counters to the level's.</summary>
    static void F57Flush()
    {
        if (f57Sim?.husks?.acid != null && f57Sim.Press57On) { f57Marks += f57Sim.husks.acid.marked57; f57Strikes += f57Sim.husks.acid.struck57; }
        f57Sim = null;
    }

    /// <summary>One in-fight step (F56TokStep): the moving / still clock while the row is on.</summary>
    static void F57PressStep(SurvivalSim sim)
    {
        F58StillStep(sim); // NIGHT58 G G2: the chosen-stillness clock (Harness.N58.Still.cs; --g58 only)
        if (!sim.Press57On && !F57Meter) return;
        f57Seen = true;
        F57Hook(sim);
        double v = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z);
        bool mv = v >= 0.25 * sim.cfg.Num("ground.walkSpeed");
        if (mv) f57StepsMove++; else f57StepsStill++;
        if (f57Flee != null) { if (mv) f57AnsMove++; else f57AnsStill++; }
        if (!mv) { if (f57MoveIn < 0.05) f57StillChoice++; else f57StillBlocked++; }
    }
    static long f57AnsMove, f57AnsStill, f57StillChoice, f57StillBlocked;
    // G1's target (per-enemy idle): a drone's idle steps split by the brain's state (--g57 only): holding the ranged token or waiting; in the sim
    // cone or not; within 2 m of its ring (ai.rings[waitRing] radius from the player) or off it
    static long f57DroneHold, f57DroneWait, f57DroneInCone, f57DroneOnRing;
    static void F57DroneIdle(SurvivalSim sim, Husk h)
    {
        if (!F57Meter || sim.brain == null) return;
        if (sim.brain.pools.Holds("ranged", h.id)) f57DroneHold++; else f57DroneWait++;
        if (F56InCone(sim, h)) f57DroneInCone++;
        double ring = PortData.Num(PortData.Obj(sim.cfg.Arr("ai.rings")[sim.cfg.Int("ai.kinds.drone.waitRing")])["radius"]);
        if (Math.Abs(JsMath.Hypot(h.position.x - sim.mover.position.x, h.position.z - sim.mover.position.z) - ring) < 2) f57DroneOnRing++;
    }
    static double f57MoveIn;

    /// <summary>The level's PRESS57 line (F56TokTable), only when the row ran on; then the level's counters reset.</summary>
    static void F57PressTable(string id, Action<string> Line)
    {
        F58StillTable(id, Line); // NIGHT58 G G2: the STILL58 line (Harness.N58.Still.cs; --g58 only)
        F57Flush();
        if (f57Seen)
        {
            double dt = f56TokSim != null ? f56TokSim.step : 1.0 / 120;
            double minMove = f57StepsMove * dt / 60, minStill = f57StepsStill * dt / 60;
            string Per(double dmg, double min) => min > 0 ? (dmg / min).ToString("0.0", F1Inv) : "-";
            Line($"n46flow {id} PRESS57: spots marked {f57Marks}, volleys thrown at a mark {f57Strikes}; the pilot answered {f57Answers} circles ({f57AnswerSteps * dt:0.0} s); spitter + acid damage taken: moving {f57HitsMove} hits {f57DmgMove:0} HP = {Per(f57DmgMove, minMove)} HP per min moving ({minMove:0.0} min), still {f57HitsStill} hits {f57DmgStill:0} HP = {Per(f57DmgStill, minStill)} HP per min still ({minStill:0.0} min); in-fight steps while answering: moving {f57AnsMove}, still {f57AnsStill}; still steps with the pilot's stick at rest {f57StillChoice}, pushing {f57StillBlocked}; drone idle steps: holding the token {f57DroneHold}, waiting {f57DroneWait}, in the cone {f57DroneInCone}, on its ring {f57DroneOnRing}");
        }
        f57AnsMove = f57AnsStill = f57StillChoice = f57StillBlocked = 0;
        f57DroneHold = f57DroneWait = f57DroneInCone = f57DroneOnRing = 0;
        f57Seen = false;
        f57Answers = f57AnswerSteps = f57Marks = f57Strikes = f57StepsMove = f57StepsStill = 0;
        f57DmgMove = f57DmgStill = 0;
        f57HitsMove = f57HitsStill = 0;
        f57Flee = null;
    }
}
