// NIGHT57 builder G: `n57g`, G's own checks. Hooked without a Program.cs line (a module initializer runs the command and exits when argv[1] is
// "n57g", Harness.N56.cs's pattern); it applies the off switches itself. Builds in the DEFAULT SimHarness project (a test class not compiled in
// is reported, never a build error).
//   n57g selftest [Class]   G's EditMode classes on the harness's mini NUnit (each one compiled in); build with
//                           -p:Extra="../../Assets/Tests/EditMode/Game/N57PressTests.cs"
// press57's bot numbers are n46flow's (Harness.N57.Press.cs: the pilot's answer and the PRESS57 line).
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;

public static partial class Harness
{
    static readonly string[] N57GTestClasses = { "Skygrind.Game.Tests.N57PressTests", "Skygrind.Game.Tests.N56TokensTests", "Skygrind.Game.Tests.N56RankTests", "Skygrind.Game.Tests.N55KillTests" };

    [ModuleInitializer]
    internal static void N57GInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n57g") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            // --fixture36: the Editor's [SetUpFixture] N36TestBaseline (N36SharedTests.cs) sets N36Blocks.ForceOff for every test in
            // Skygrind.Game.Tests; the mini NUnit runs no fixture, so this flag stands in for it (T-137's condition)
            if (Array.IndexOf(args, "--fixture36") >= 0) N36Blocks.ForceOff = true;
            string sub = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "selftest";
            if (sub == "selftest") code = N57GSelftest(args);
            else { Console.WriteLine("n57g: unknown subcommand " + sub + " (selftest)"); code = 2; }
        }
        catch (Exception e) { Console.WriteLine("n57g: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static int N57GSelftest(string[] args)
    {
        int failed = 0, classes = 0;
        string only = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : null;
        foreach (string name in N57GTestClasses)
        {
            if (only != null && !name.EndsWith(only, StringComparison.Ordinal)) continue;
            Type t = typeof(Harness).Assembly.GetType(name);
            if (t == null) { Console.WriteLine("n57g selftest: " + name + " not compiled in"); continue; }
            classes++;
            failed += NUnit.Framework.MiniRunner.Run(t, Console.WriteLine);
        }
        Console.WriteLine($"n57g selftest: {(failed == 0 && classes > 0 ? "GREEN" : "RED")} ({classes} class(es), {failed} failing)");
        return failed == 0 && classes > 0 ? 0 : 1;
    }
}
