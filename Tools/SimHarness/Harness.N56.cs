// NIGHT56 builder G: `n56`, G's own checks. Hooked without a Program.cs line (a module initializer runs the command and exits when argv[1] is
// "n56", Harness.N53.RideKill.cs's pattern); it applies the off switches itself. Builds in the DEFAULT SimHarness project (no -p:Extra needed:
// a test class not compiled in is reported, never a build error).
//   n56 selftest     G's EditMode classes on the harness's mini NUnit (each one that is compiled in); build with
//                    -p:Extra="../../Assets/Tests/EditMode/Game/N56RankTests.cs;../../Assets/Tests/EditMode/Game/N56TokensTests.cs;../../Assets/Tests/EditMode/Wyrm55Tests.cs"
// The style column and the tokens columns live in n46flow (Harness.N56.Style.cs, Harness.N56.Tokens.cs).
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;

public static partial class Harness
{
    static readonly string[] N56TestClasses = { "Skygrind.Game.Tests.N56RankTests", "Skygrind.Game.Tests.N56TokensTests", "Skygrind.Core.Tests.Wyrm55Tests" };

    [ModuleInitializer]
    internal static void N56Init()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n56") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string sub = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "selftest";
            code = sub == "selftest" ? N56Selftest(args) : sub == "kestrel" ? N56Kestrel(args) : sub == "wickkit" ? N56WickKit() : sub == "wickfight" ? N56WickFight(args) : 2;
            if (code == 2 && sub != "selftest" && sub != "kestrel" && sub != "wickkit" && sub != "wickfight") Console.WriteLine("n56: unknown subcommand " + sub + " (selftest | kestrel | wickkit | wickfight)");
        }
        catch (Exception e) { Console.WriteLine("n56: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    /// <summary>`n56 wickkit`: Wick55Tests.RowOn_EveryAttack_ExactlyAsWick54Ships's comparison headless (the mini NUnit cannot compile that file:
    /// it uses Within): THE WICK built as the test builds it (wick43 / wick47 / wick54 on, wick55 on and off), every attack of the on-kit against
    /// the off-kit and against the config with wick47's kit rule (the DIVE legMask 0, the SLAM legMask 1); one line per mismatching field.</summary>
    static int N56WickKit()
    {
        GameConfig c = Config();
        Wick37 Make(bool on55)
        {
            var events = new EventBus();
            var health = new Health(1e7, events, "player");
            var stack = new Wick37Stack(Wick37StackParams.From(c));
            Vec3 player = new Vec3(0, 0, stack.p.radius - 2.5);
            var target = new BossTarget { position = () => player, health = health, impulse = (x, y, z) => { } };
            int id = 0;
            var w = new Wick37(c, Wick37Params.From(c), events, target, () => ++id) { origin = Vec3.Zero, stack = stack };
            w.n43 = true; w.p43 = Wick43Params.From(c);
            w.w47 = true; w.p47 = Wick47Params.From(c);
            w.w54 = true; w.p54 = Wick54Params.From(c);
            w.w55 = on55; w.p55 = Wick55Params.From(c);
            w.StartFight(new BossFight { center = Vec3.Zero, scale = BossScale.One });
            return w;
        }
        Wick37 on = Make(true), off = Make(false);
        var cfgAttack = new System.Collections.Generic.Dictionary<string, Boss37Attack>();
        foreach (string k in new[] { "ember", "flare", "slam", "dive" }) { Boss37Attack a = Boss37Attack.From(c, "design.n37.wick37.attacks." + k, c.Dt); cfgAttack[a.id] = a; }
        foreach (string k in new[] { "geyser", "curtain", "ring", "charge" }) { Boss37Attack a = Boss37Attack.From(c, "design.n47.wick47.numbers.attacks." + k, c.Dt); cfgAttack[a.id] = a; }
        cfgAttack[Wick37.Dive].legMask = 0;
        cfgAttack[Wick37.Slam].legMask = 1;
        var offBy = new System.Collections.Generic.Dictionary<string, Boss37Attack>();
        foreach (Boss37Attack a in off.brain.attacks) offBy[a.id] = a;
        int bad = 0;
        void Cmp(Boss37Attack a, Boss37Attack b, string why)
        {
            var f = new (string n, object x, object y)[] { ("kind", a.kind, b.kind), ("windup", a.windupSteps, b.windupSteps), ("active", a.activeSteps, b.activeSteps), ("recover", a.recoverSteps, b.recoverSteps),
                ("cost", a.cost, b.cost), ("weight", a.weight, b.weight), ("minDist", a.minDist, b.minDist), ("maxDist", a.maxDist, b.maxDist), ("habit", a.habit, b.habit), ("legs", a.legMask, b.legMask) };
            foreach (var t in f) if (!Equals(t.x, t.y)) { bad++; Console.WriteLine($"  {why} {t.n}: {t.x} vs {t.y}"); }
        }
        foreach (Boss37Attack a in on.brain.attacks)
        {
            if (!offBy.ContainsKey(a.id)) { bad++; Console.WriteLine("  " + a.id + " not in the off kit"); continue; }
            Cmp(a, offBy[a.id], a.id + " (on vs off)");
            if (!cfgAttack.ContainsKey(a.id)) { bad++; Console.WriteLine("  " + a.id + " not from the config"); continue; }
            Cmp(a, cfgAttack[a.id], a.id + " (on vs config + wick47's kit rule)");
        }
        Console.WriteLine($"n56 wickkit: {on.brain.attacks.Count} attacks on, {off.brain.attacks.Count} off; {bad} mismatching field(s): {(bad == 0 ? "PASS" : "FAIL")}");
        return bad == 0 ? 0 : 1;
    }

    static int N56Selftest(string[] args)
    {
        int failed = 0, classes = 0;
        string only = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : null;
        foreach (string name in N56TestClasses)
        {
            if (only != null && !name.EndsWith(only, StringComparison.Ordinal)) continue;
            Type t = typeof(Harness).Assembly.GetType(name);
            if (t == null) { Console.WriteLine("n56 selftest: " + name + " not compiled in"); continue; }
            classes++;
            failed += NUnit.Framework.MiniRunner.Run(t, Console.WriteLine);
        }
        Console.WriteLine($"n56 selftest: {(failed == 0 && classes > 0 ? "GREEN" : "RED")} ({classes} class(es), {failed} failing)");
        return failed == 0 && classes > 0 ? 0 : 1;
    }
}
