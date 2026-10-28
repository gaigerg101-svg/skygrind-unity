// NIGHT16 (the shared commit; the lead's dispatch): the feel night's headless instruments, each builder's in its own file:
//   spam [map.json]      builder M (Harness.Spam.cs, SpamCommand): scripted abuse per verb on flat ground (the arena's floor
//                        and DRYDOCK's deck), each against walking and the skilled line: metres in 10 s, the peak speed, the
//                        speed at 10 s, the inputs per second (slide tapped every 0.15 / 0.3 / 0.5 s, slide held 10 s, a
//                        slide-hop chain from walk speed, dash mashed, a wall-jump chain, slam-boost chains)
//   impact [map.json]    builder I (Harness.Impact.cs, ImpactCommand): the freeze ledger over a 300 s AUTOPILOT run (what
//                        ImpactRules would request): freezes by cause, frozen ms per minute, the worst rolling second,
//                        the slow-mos, the requests skipped over the budget
using System;

public static partial class Harness
{
    static void Night16(string cmd, string[] args, ref int code)
    {
        if (cmd == "spam") SpamCommand(args, ref code);
        else if (cmd == "impact") ImpactCommand(args, ref code);
        else if (cmd == "residual") ResidualCommand(args, ref code); // builder M: the NIGHT14 residual (Harness.Residual.cs)
    }

    /// <summary>Builder M's `residual` (Harness.Residual.cs): code = the exit code (0 when the played composition equals the baseline).</summary>
    static partial void ResidualCommand(string[] args, ref int code);

    /// <summary>Builder M's `spam` (Harness.Spam.cs): code = the exit code (0 when every target holds).</summary>
    static partial void SpamCommand(string[] args, ref int code);

    /// <summary>Builder I's `impact` (Harness.Impact.cs): code = the exit code (0 when the budget holds).</summary>
    static partial void ImpactCommand(string[] args, ref int code);
}
