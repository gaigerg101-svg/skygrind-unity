// NIGHT34 builder M (NIGHT34.md M1): the movement builder's harness commands, declared and called by the lead's Program.cs (N34Flow).
//   reach34   the Pulse Gauntlet's boss margin (Harness.N34.Reach.cs)
//   flow34    the movement techniques' before / after on the real Mover (Harness.N34.FlowChecks.cs)
//   latency34 steps from a press to its first sim change per verb, flow34 off / on (Harness.N34.Latency.cs)
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N34Flow(string cmd, string[] args, ref int code)
    {
        if (cmd == "reach34") { code = Reach34Command(args); return; }
        if (cmd == "flow34") { code = Flow34Command(args); return; }
        if (cmd == "latency34") { code = Latency34Command(args); return; }
    }
}
