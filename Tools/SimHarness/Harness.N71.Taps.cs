// NIGHT71 builder F (F4: does a KESTREL tap pressed and released inside a freeze fire?). `taps71h`: the reel taps71's sim side, headless. A freeze
// is no sim step (SurvivalGame.Update runs none while the impact layer holds); both input readers latch a press until the next step and read the
// held bit live (CombatReader's fireLatch, PadReader's frame.latched), so a tap pressed and released inside a freeze reaches the first step after
// it as { pressed: true, held: false }. This feeds the sim exactly those steps, with the step that started the freeze before them, and counts
// the KESTREL's `fire` events after:
//   rel      the freeze-starting shot is a p39 tap fired on its RELEASE step (pressed false on that step), then the tap  { pressed, !held }
//   prs      a FULL charged shot first, then (the charge cooling) a press fired ON ITS PRESS step { pressed, held }, then the tap { pressed, !held }
//   prs-gap  the control: as prs, with one step { !pressed, !held } between (no freeze: a step sees the trigger let go before the tap)
// Hooked without a Program.cs line (tap53's module initializer pattern):
//   dotnet run --project Tools/SimHarness -c Release -- taps71h [--level n52-foundry] [--reps 3]
// Probe numbers only (no gameplay effect): the reel's (UnityTuning.Taps71* in UnityTuning.F71.cs, a file this harness does not compile),
// copied here as the probe's own, in whole steps at simHz.
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    // the reel's numbers (UnityTuning.Taps71*): s after placing; s held for a FULL charge; s from the charged shot to the press-step shot (past the
    // 0.483 s tap interval, inside charge35's 2.5 s cooldown); s after the freeze for the tap's shot; s for charge35's cooldown to end between reps
    const double T71SettleS = 0.9, T71ChargeHoldS = 1.2, T71AfterChargeS = 1.0, T71FireWaitS = 1.0, T71CoolS = 2.6;

    [ModuleInitializer]
    internal static void N71TapsInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "taps71h") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = N71Taps(args);
        }
        catch (Exception e) { Console.WriteLine("taps71h: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static int N71Taps(string[] args)
    {
        string Arg(string k, string d) { int i = Array.IndexOf(args, k); return i >= 0 && i + 1 < args.Length ? args[i + 1] : d; }
        string id = Arg("--level", "n52-foundry");
        int reps = int.Parse(Arg("--reps", "3"));
        GameConfig cfg = Config();
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var si = new E53Script();
        sim.UseInput(si);
        sim.StartRun(160716, null, null);
        int fires = 0;
        sim.events.On("fire", o => { if (o as string == "revolver") fires++; });
        // NIGHT71 L10: --press71 routes every step's fire press through the reader's press71 rule (Game/Press71.cs, the class itself)
        Press71 p71 = Array.IndexOf(args, "--press71") >= 0 ? new Press71() : null;
        void Step(bool pressed, bool held)
        {
            if (p71 != null) pressed = p71.Deliver(pressed);
            sim.health.max = 100;
            sim.health.hp = 100;
            sim.health.over = 0;
            si.combat = new CombatInput { weapon = -1, fire = new Button { pressed = pressed, held = held } };
            sim.Step();
        }
        void Idle(double s)
        {
            int n = MovementMath.StepsFor(s, sim.step);
            for (int i = 0; i < n; i++) Step(false, false);
        }
        for (int i = 0; i < 1200 && (sim.campaign == null || sim.campaign.room < 0); i++) Step(false, false);
        sim.campaign?.SetDoorFree29(true);
        sim.husks.frozen = true;
        // the KESTREL in hand (Equip27's way: the slot asked for until in hand, then the raise)
        if (sim.loadout.owned.IndexOf("revolver") < 0) sim.loadout.AddWeapon("revolver");
        int slot = sim.loadout.owned.IndexOf("revolver");
        for (int i = 0; i < 360 && sim.loadout.Weapon != "revolver"; i++)
        {
            si.combat = new CombatInput { weapon = slot };
            sim.health.hp = sim.health.max;
            sim.Step();
        }
        Idle(1.0);
        ChargeShots27 cs = sim.loadout.charge27; // the live charge object (later nights put charge31's here, not sim.charge27)
        Charge35 q = cs?.c35;
        Console.WriteLine($"taps71h on {id}: campaign {sim.campaignLevel?.id ?? "none"} room {sim.campaign?.room}, in hand {sim.loadout.Weapon}, p39 {(q != null && q.p39 ? "on" : "off")}, simHz {1 / sim.step:0}");
        if (sim.loadout.Weapon != "revolver" || q == null) { Console.WriteLine("taps71h: RED (no KESTREL in hand or no charge35)"); return 1; }
        int hold = MovementMath.StepsFor(T71ChargeHoldS, sim.step);
        int insideN = 0, insideM = 0;
        foreach (string kind in new[] { "rel", "prs", "prs-gap" })
            for (int r = 0; r < reps; r++)
            {
                Idle(T71SettleS + T71CoolS); // the gun's interval and charge35's 2.5 s cooldown both over
                if (kind != "rel")
                {
                    // a FULL charged shot (held past charge40's 0.9 s), then the charge cools: a press fires on its press step
                    Step(true, true);
                    for (int i = 1; i < hold; i++) Step(false, true);
                    Step(false, false);
                    Idle(T71AfterChargeS);
                }
                int f0 = fires;
                // the freeze-starting shot
                if (kind == "rel")
                {
                    Step(true, true); // a p39 press: a hold starts, nothing fires
                    for (int i = 0; i < 3; i++) Step(false, true);
                    Step(false, false); // the release step: the tap fires here (pressed false on this step)
                }
                else Step(true, true); // the charge cooling: the press fires ON its press step
                bool startShot = fires > f0;
                bool lastPressed = q.pressedLast;
                int f1 = fires, pt = q.pressTaps39, rt = q.releaseTaps39;
                if (kind == "prs-gap") Step(false, false); // the control: a step sees the trigger let go (no freeze)
                // THE FREEZE: no step. The tap pressed and released inside it reaches the next step as pressed, not held:
                Step(true, false);
                Idle(T71FireWaitS);
                bool fired = fires > f1;
                if (kind != "prs-gap")
                {
                    insideN++;
                    if (fired) insideM++;
                }
                Console.WriteLine($"  {kind} {r + 1}: the freeze-starting shot fired {(startShot ? "yes" : "NO")} (its step's pressed bit {lastPressed}); the tap after it fired {(fired ? "yes" : "NO")} ({fires - f1} shots; p39 press taps +{q.pressTaps39 - pt}, release taps +{q.releaseTaps39 - rt})");
            }
        Console.WriteLine($"taps71h: {(insideM == insideN ? "GREEN" : "RED")} taps inside freezes {insideN}, fired {insideM} (rel and prs; prs-gap is the no-freeze control){(p71 != null ? $"; press71 on: presses delivered one step late {Press71.Deferred}" : "")}");
        return 0;
    }
}
