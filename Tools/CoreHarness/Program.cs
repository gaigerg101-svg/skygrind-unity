// NIGHT9: the golden oracle outside Unity (Skygrind.Core's Golden, the code the EditMode tests and the Player's
// -skygrindGolden run). From the repo root:
//   dotnet run --project Tools/CoreHarness -c Release -- [portExportDir] [set | replay:<name> ...]
// No sets: every vector set and every trajectory replay, one line each, then a total (exit 1 on any mismatch).
// The default folder is the repo's PortExport; a port-export-pNN folder works the same (config.json, vectors/,
// trajectories/, maps/).
using System;
using System.IO;
using System.Linq;
using Skygrind.Core;

public static class Program
{
    public static int Main(string[] args)
    {
        // NIGHT15 U3-28.3: `navgrid [portExportDir]` checks the nav grid port against the exported maps (NavGridCheck.cs).
        if (args.Length > 0 && args[0] == "navgrid") return NavGridCheck.Run(args.Length > 1 ? args[1] : Path.GetFullPath("PortExport"), Console.WriteLine) == 0 ? 0 : 1;
        string root = args.Length > 0 ? args[0] : Path.GetFullPath("PortExport");
        var data = new GoldenData(root);
        var sets = args.Skip(1).ToList();
        if (sets.Count == 0) return Golden.RunAll(data, Console.WriteLine) == 0 ? 0 : 1;
        int bad = 0;
        foreach (string s in sets)
        {
            if (s.StartsWith("replay:"))
            {
                ReplayReport rep = Golden.Replay(data, s.Substring(7));
                Console.WriteLine(rep.Exact ? $"replay PASS {rep.name}: {rep.steps} steps" : $"replay FAIL {rep.name}: step {rep.firstDivergentStep}: {rep.firstDifference}");
                if (!rep.Exact) bad++;
                continue;
            }
            GoldenResult r = Golden.Check(data, s);
            Console.WriteLine((r.Ok ? "PASS " : "FAIL ") + r.Summary);
            foreach (string m in r.mismatches.Take(25)) Console.WriteLine("   " + m);
            if (!r.Ok) bad++;
        }
        return bad == 0 ? 0 : 1;
    }
}
