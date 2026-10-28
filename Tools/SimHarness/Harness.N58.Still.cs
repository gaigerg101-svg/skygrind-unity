// NIGHT58 builder G (G2 press57's measure; DECIDE58 D-3, Logs/n58/slate.json G2, fixed before any number): CHOSEN STILLNESS in `n46flow`
// with `--g58` (silent without the flag, so flow-base and every gate's flow file stay as they were):
//   the share of combat steps (n46flow's in-fight steps of the measured rooms: phase Fight, the player not down; the same steps as r.fight /
//   F56StyleStep -> F56TokStep -> F57PressStep) where the pilot's stick is NOT pushing (the move input that drove the step, after every pilot
//   rule: hypot(moveX, moveZ) == 0) AND the player's horizontal speed < F58StillV (0.5 m/s).
// The stick is read from the Mover's own `input` (the frame Mover.Step used, Mover.cs:183/574) by reflection, read only: the harness never
// writes it, so the sim is untouched (fingerprints EQUAL). One line per level: n46flow <id> STILL58: chosen stillness a / b = x % of combat
// steps; beside it NIGHT57's split for reference (still = < 2 m/s, stick at rest / pushing) and the blocked share (stick pushing, < 0.5 m/s).
// Bot numbers only (no gameplay effect): F58StillV (the slate's 0.5 m/s), F58StickZero (a float zero).
using System;
using System.Reflection;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    const double F58StillV = 0.5, F58StickZero = 1e-9;
    static bool F58Meter => f58Meter ??= Array.IndexOf(Environment.GetCommandLineArgs(), "--g58") >= 0;
    static bool? f58Meter;
    static FieldInfo f58InputField;
    static long f58Steps, f58Chosen, f58Blocked, f58Slow;

    /// <summary>One in-fight step (F57PressStep's first line): the chosen-stillness clock (--g58 only).</summary>
    static void F58StillStep(SurvivalSim sim)
    {
        if (!F58Meter) return;
        f58InputField ??= typeof(Mover).GetField("input", BindingFlags.NonPublic | BindingFlags.Instance);
        var fr = (InputFrame)f58InputField.GetValue(sim.mover);
        double stick = JsMath.Hypot(fr.moveX, fr.moveZ);
        double v = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z);
        f58Steps++;
        if (v < F58StillV)
        {
            f58Slow++;
            if (stick < F58StickZero) f58Chosen++; else f58Blocked++;
        }
    }

    /// <summary>The level's STILL58 line (F57PressTable's first line), then its counters reset.</summary>
    static void F58StillTable(string id, Action<string> Line)
    {
        if (F58Meter && f58Steps > 0)
            Line($"n46flow {id} STILL58: chosen stillness (stick 0 AND speed < {F58StillV:0.0} m/s) {f58Chosen}/{f58Steps} = {F56Pct(f58Chosen, f58Steps)} % of combat steps; blocked (stick pushing, < {F58StillV:0.0} m/s) {f58Blocked} = {F56Pct(f58Blocked, f58Steps)} %; all < {F58StillV:0.0} m/s {F56Pct(f58Slow, f58Steps)} %");
        f58Steps = f58Chosen = f58Blocked = f58Slow = 0;
    }
}
