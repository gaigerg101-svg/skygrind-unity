// NIGHT14 (the shared types): headless runs of the sim composition outside Unity. From the repo root:
//   dotnet run --project Tools/SimHarness -c Release -- <command> [args...]
// Commands (each agent's in its own Harness.<Topic>.cs, a partial method this file calls):
//   fingerprint            the fingerprint run (seed 1) on a fresh composition, compared with FingerprintBaseline
//   twice                  two seed-1 fingerprint runs back to back on one composition (U3-27.2's clean slate)
//   ... and whatever the agents add (bot, autopilot, farm, tape).
// The data is the repo's PortExport (config.json + Design/unity-config.json beside it, maps/survival.json).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    public static string Root = Path.GetFullPath("PortExport");

    public static GameConfig Config() => GameConfig.Load(Path.Combine(Root, "config.json"));

    public static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

    public static int Main(string[] args)
    {
        string cmd = args.Length > 0 ? args[0] : "fingerprint";
        // NIGHT19: --off19 runs every command with every NIGHT19 design block off (NIGHT18's game; the judges' A side)
        bool off19 = Array.IndexOf(args, "--off19") >= 0;
        N19Blocks.ForceOff = off19;
        // NIGHT20: --off20 runs every command with every NIGHT20 design block off (NIGHT19's game; tonight's A side);
        // --off19 turns off both nights' blocks (NIGHT18's game)
        bool off20 = off19 || Array.IndexOf(args, "--off20") >= 0;
        N20Blocks.ForceOff = off20;
        // NIGHT21: --off21 runs every command with every NIGHT21 design block off (NIGHT20's game; tonight's A side);
        // --off20 and --off19 turn off the older nights' blocks as well
        bool off21 = off20 || Array.IndexOf(args, "--off21") >= 0;
        N21Blocks.ForceOff = off21;
        // NIGHT22: --off22 runs every command with every NIGHT22 design block off (NIGHT21's game; tonight's A side);
        // --off21, --off20 and --off19 turn off the older nights' blocks as well
        bool off22 = off21 || Array.IndexOf(args, "--off22") >= 0;
        N22Blocks.ForceOff = off22;
        // NIGHT24: --off24 runs every command with every NIGHT24 design block off (NIGHT22's game; tonight's A side);
        // --off22, --off21, --off20 and --off19 turn off the older nights' blocks as well
        bool off24 = off22 || Array.IndexOf(args, "--off24") >= 0;
        N24Blocks.ForceOff = off24;
        // NIGHT25: --off25 runs every command with every NIGHT25 design block off (NIGHT24's game; tonight's A side);
        // --off24 and the older switches turn off these blocks as well
        bool off25 = off24 || Array.IndexOf(args, "--off25") >= 0;
        N25Blocks.ForceOff = off25;
        // NIGHT26: --off26 runs every command with every NIGHT26 design block off (NIGHT25's game; tonight's A side);
        // --off25 and the older switches turn off these blocks as well
        bool off26 = off25 || Array.IndexOf(args, "--off26") >= 0;
        N26Blocks.ForceOff = off26;
        // NIGHT27: --off27 runs every command with every NIGHT27 design block off (NIGHT26's game; tonight's A side);
        // --off26 and the older switches turn off these blocks as well
        bool off27 = off26 || Array.IndexOf(args, "--off27") >= 0;
        N27Blocks.ForceOff = off27;
        // NIGHT29: --off29 runs every command with every NIGHT29 design block off (NIGHT28's game); every older switch turns them off as well
        bool off29 = off27 || Array.IndexOf(args, "--off29") >= 0;
        N29Blocks.ForceOff = off29;
        // NIGHT30: --off30 runs every command with every NIGHT30 design block off (NIGHT29's game); every older switch turns them off as well
        bool off30 = off29 || Array.IndexOf(args, "--off30") >= 0;
        N30Blocks.ForceOff = off30;
        // NIGHT31: --off31 runs every command with every NIGHT31 design block off (NIGHT30's game); every older switch turns them off as well
        bool off31 = off30 || Array.IndexOf(args, "--off31") >= 0;
        N31Blocks.ForceOff = off31;
        // NIGHT32: --off32 runs every command with every NIGHT32 design block off (NIGHT31's game); every older switch turns them off as well
        bool off32 = off31 || Array.IndexOf(args, "--off32") >= 0;
        N32Blocks.ForceOff = off32;
        // NIGHT33: --off33 runs every command with every NIGHT33 design block off (NIGHT32's game); every older switch turns them off as well
        bool off33 = off32 || Array.IndexOf(args, "--off33") >= 0;
        N33Blocks.ForceOff = off33;
        // NIGHT34: --off34 runs every command with every NIGHT34 design block off (NIGHT33's game); every older switch turns them off as well
        bool off34 = off33 || Array.IndexOf(args, "--off34") >= 0;
        N34Blocks.ForceOff = off34;
        // NIGHT35: --off35 runs every command with every NIGHT35 design block off (NIGHT34's game); every older switch turns them off as well
        bool off35 = off34 || Array.IndexOf(args, "--off35") >= 0;
        N35Blocks.ForceOff = off35;
        // NIGHT36: --off36 runs every command with every NIGHT36 design block off (NIGHT35's game); every older switch turns them off as well
        bool off36 = off35 || Array.IndexOf(args, "--off36") >= 0;
        N36Blocks.ForceOff = off36;
        // NIGHT37: --off37 runs every command with every NIGHT37 design block off (NIGHT36's game); every older switch turns them off as well
        bool off37 = off36 || Array.IndexOf(args, "--off37") >= 0;
        N37Blocks.ForceOff = off37;
        // NIGHT38: --off38 runs every command with every NIGHT38 design block off (NIGHT37's game); every older switch turns them off as well
        bool off38 = off37 || Array.IndexOf(args, "--off38") >= 0;
        N38Blocks.ForceOff = off38;
        // NIGHT39: --off39 runs every command with every NIGHT39 design block off (NIGHT38's game); every older switch turns them off as well
        bool off39 = off38 || Array.IndexOf(args, "--off39") >= 0;
        N39Blocks.ForceOff = off39;
        // NIGHT40: --off40 runs every command with every NIGHT40 design block off (NIGHT39's game); every older switch turns them off as well
        bool off40 = off39 || Array.IndexOf(args, "--off40") >= 0;
        N40Blocks.ForceOff = off40;
        // NIGHT41: --off41 runs every command with every NIGHT41 design block off (NIGHT40's game); every older switch turns them off as well
        bool off41 = off40 || Array.IndexOf(args, "--off41") >= 0;
        N41Blocks.ForceOff = off41;
        // NIGHT42: --off42 runs every command with every NIGHT42 design block off (NIGHT41's game); every older switch turns them off as well
        bool off42 = off41 || Array.IndexOf(args, "--off42") >= 0;
        N42Blocks.ForceOff = off42;
        // NIGHT43: --off43 runs every command with every NIGHT43 design block off (NIGHT42's game); every older switch turns them off as well
        bool off43 = off42 || Array.IndexOf(args, "--off43") >= 0;
        N43Blocks.ForceOff = off43;
        // NIGHT45: --off45 (and --off44, and every older switch): NIGHT44's level order (the level-only rows off)
        N45Levels.ForceOff = off43 || Array.IndexOf(args, "--off44") >= 0 || Array.IndexOf(args, "--off45") >= 0;
        NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off"); // NIGHT46 rows: --offNN turns off every row of night NN and later (every older switch: all of them)
        int code = -1;
        switch (cmd)
        {
            case "fingerprint":
            {
                SurvivalSim sim = SurvivalSim.Headless(Config(), Map());
                var t = System.Diagnostics.Stopwatch.StartNew();
                if (off19) sim.n19Override = N19Blocks.Off; // NIGHT19: the off path (RunFingerprint keeps an override it is given)
                if (off20) sim.n20Override = N20Blocks.Off; // NIGHT20: the same
                if (off21) sim.n21Override = N21Blocks.Off; // NIGHT21: the same
                if (off22) sim.n22Override = N22Blocks.Off; // NIGHT22: the same
                if (off24) sim.n24Override = N24Blocks.Off; // NIGHT24: the same
                if (off25) sim.n25Override = N25Blocks.Off; // NIGHT25: the same
                if (off26) sim.n26Override = N26Blocks.Off; // NIGHT26: the same
                if (off27) sim.n27Override = N27Blocks.Off; // NIGHT27: the same
                if (off29) sim.n29Override = N29Blocks.Off; // NIGHT29: the same
                if (off30) sim.n30Override = N30Blocks.Off; // NIGHT30: the same
                if (off31) sim.n31Override = N31Blocks.Off; // NIGHT31: the same
                if (off32) sim.n32Override = N32Blocks.Off; // NIGHT32: the same
                if (off33) sim.n33Override = N33Blocks.Off; // NIGHT33: the same
                if (off34) sim.n34Override = N34Blocks.Off; // NIGHT34: the same
                if (off35) sim.n35Override = N35Blocks.Off; // NIGHT35: the same
                if (off36) sim.n36Override = N36Blocks.Off; // NIGHT36: the same
                if (off37) sim.n37Override = N37Blocks.Off; // NIGHT37: the same
                if (off38) sim.n38Override = N38Blocks.Off; // NIGHT38: the same
                if (off39) sim.n39Override = N39Blocks.Off; // NIGHT39: the same
                if (off40) sim.n40Override = N40Blocks.Off; // NIGHT40: the same
                if (off41) sim.n41Override = N41Blocks.Off; // NIGHT41: the same
                if (off42) sim.n42Override = N42Blocks.Off; // NIGHT42: the same
                if (off43) sim.n43Override = N43Blocks.Off; // NIGHT43: the same
                List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
                bool off21Only = off21 && !off20; // NIGHT21: --off21 alone is NIGHT20's game (FingerprintBaselineN20); --off20 was never recorded
                int diff = off19 ? FingerprintBaselineN18.FirstDifference(lines) : off21Only ? FingerprintBaselineN20.FirstDifference(lines) : FingerprintBaseline.FirstDifference(lines);
                if (Array.IndexOf(args, "--print") >= 0) foreach (string l in lines) Console.WriteLine(l); // NIGHT20 close: the lines a re-record copies
                Console.WriteLine($"fingerprint seed {SimFingerprint.Seed}{(off19 ? " (--off19: every NIGHT19 block off, vs FingerprintBaselineN18)" : "")}: {lines.Count} hashes in {t.Elapsed.TotalSeconds:0.00} s: {(diff < 0 ? "EQUAL to the baseline" : $"first difference at checkpoint {diff + 1}")}; {summary}");
                return diff < 0 ? 0 : 1;
            }
            case "twice":
            {
                SurvivalSim sim = SurvivalSim.Headless(Config(), Map());
                List<string> a = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string sa);
                List<string> b = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string sb);
                int first = -1;
                for (int i = 0; i < a.Count; i++)
                    if (a[i] != b[i]) { first = i; break; }
                Console.WriteLine($"twice: run 1 vs baseline {(FingerprintBaseline.FirstDifference(a) < 0 ? "EQUAL" : "DIFFERS")}; run 2 vs run 1 {(first < 0 ? "EQUAL (12 of 12)" : $"first difference at checkpoint {first + 1}")}\n  run 1: {sa}\n  run 2: {sb}");
                return first < 0 ? 0 : 1;
            }
        }
        Bot(cmd, args, ref code);
        Autopilot(cmd, args, ref code);
        Tape(cmd, args, ref code);
        Lead(cmd, args, ref code);
        MapGen(cmd, args, ref code); // NIGHT15 U3-28.6: builder G's commands (Harness.MapGen.cs)
        Night16(cmd, args, ref code); // NIGHT16: spam (builder M), impact (builder I) (Harness.Night16.cs)
        N26(cmd, args, ref code); // NIGHT26: n26shared (the lead) and the rows' commands (Harness.N26.cs)
        N27Level(cmd, args, ref code); // NIGHT27: hazard27 (builder A, Harness.N27.Level.cs)
        N27Boss(cmd, args, ref code); // NIGHT27: boss27 [lantern|glacier] (builder B, Harness.N27.Boss.cs)
        N27Tape(cmd, args, ref code); // NIGHT27: tape27 (the lead, Harness.N27.Tape.cs): a rows-ON taped run replayed from the tape
        N28Boss(cmd, args, ref code); // NIGHT28 B2: boss28 [--seeds n] [--first n] [--dbg] (Harness.N28.Boss.cs)
        N29Systems(cmd, args, ref code); // NIGHT29 S: lockout29, shotgun29, tank29, doorfree29, tape29, systems29 (Harness.N29.Systems.cs)
        N29Boss(cmd, args, ref code); // NIGHT29 B: boss29 [lantern|glacier|hp] (Harness.N29.Boss.cs)
        N29Walk(cmd, args, ref code); // NIGHT29 R: walk29 <foundry|iceline|kiln> [--seeds n] (Harness.N29.Walk.cs)
        N30Systems(cmd, args, ref code); // NIGHT30 S: rules30, lava30, glide30, arcline30, lesson30 ... (Harness.N30.Systems.cs)
        N30Boss(cmd, args, ref code); // NIGHT30 B: boss30 [lantern|glacier|wyrm|loom], seam30 (Harness.N30.Boss.cs)
        N30Levels(cmd, args, ref code); // NIGHT30 L: walk30 / level checks for the n30 plans (Harness.N30.Levels.cs)
        N30View(cmd, args, ref code); // NIGHT30 V: quantise30 and other headless view checks (Harness.N30.View.cs)
        N31Grapple(cmd, args, ref code); // NIGHT31 G: grapple31 (pull speed, chains) ... (Harness.N31.Grapple.cs)
        N31Weapons(cmd, args, ref code); // NIGHT31 W: charge31, ltalways31, unlocks31 (Harness.N31.Weapons.cs)
        N31Boss(cmd, args, ref code); // NIGHT31 B: camp31 [lantern|stag|glacier|wyrm|loom], boss31 (Harness.N31.Boss.cs)
        N31Levels(cmd, args, ref code); // NIGHT31 L: arena and weapon-room checks (Harness.N31.Levels.cs)
        N32Boss(cmd, args, ref code); // NIGHT32 B: boss32 sweeps (kill-time p10 / p50 / p90), camp32, boss5 (Harness.N32.Boss.cs)
        N32Grapple(cmd, args, ref code); // NIGHT32 G: grapple32 (charges, spam bot), balance32 (Harness.N32.Grapple.cs)
        N32Arenas(cmd, args, ref code); // NIGHT32 A: arenas32, ring32 (Harness.N32.Arenas.cs)
        N32Span(cmd, args, ref code); // NIGHT32 S: span32 (Harness.N32.Span.cs)
        N32Finale(cmd, args, ref code); // NIGHT32 F: level5 (Harness.N32.Finale.cs)
        N32Crown(cmd, args, ref code); // NIGHT32 C: crown32 (THE CROWN's sweep and campers) (Harness.N32.Crown.cs)
        N33Look(cmd, args, ref code); // NIGHT33 L: light33 (every light has a fixture) (Harness.N33.Look.cs)
        N33Kit(cmd, args, ref code); // NIGHT33 K: kit33 <level> (K1's 15 plan checks) (Harness.N33.Kit.cs)
        N33Arenas(cmd, args, ref code); // NIGHT33 R: arenas33 (rooms, cover, devices, walk 10/10) (Harness.N33.Arenas.cs)
        N33Enemies(cmd, args, ref code); // NIGHT33 E: ai33 (lava deaths), spawns33, bosses33 (Harness.N33.Enemies.cs)
        N33Weapons(cmd, args, ref code); // NIGHT33 W: whip33, pierce33 (Harness.N33.Weapons.cs)
        N33Shop(cmd, args, ref code); // NIGHT33 P: shop33 (the typical earn, prices, upgrades on the tape) (Harness.N33.Shop.cs)
        N34Bend(cmd, args, ref code); // NIGHT34 B: bend34 (the short / full bend, the meter, the instant swap, the split's world steps) (Harness.N34.Bend.cs)
        N34Lines(cmd, args, ref code); // NIGHT34 A: lines34 / calm34 (movement lines per fight room, the prop count) (Harness.N34.Lines.cs)
        N34Flow(cmd, args, ref code); // NIGHT34 M: flow34, reach34, latency34 (Harness.N34.Flow.cs)
        N35Weapons(cmd, args, ref code); // NIGHT35 W: charge35 (Harness.N35.Weapons.cs)
        N35Teach(cmd, args, ref code); // NIGHT35 T: teach35 (Harness.N35.Teach.cs)
        N35Move(cmd, args, ref code); // NIGHT35 M: wallride35, grapple35, the bots (Harness.N35.Move.cs)
        N35Look(cmd, args, ref code); // NIGHT35 V: the PS2 pass's maths (Harness.N35.Look.cs)
        N35Levels(cmd, args, ref code); // NIGHT35 A: campaign6, the wall-ride rooms, the walk (Harness.N35.Levels.cs)
        N36Guns(cmd, args, ref code); // NIGHT36 F: tap36, crunch36 (Harness.N36.Guns.cs)
        N36Core(cmd, args, ref code); // NIGHT36 P: tells36, parry36, launch36, heal36 (Harness.N36.Core.cs)
        N36Move(cmd, args, ref code); // NIGHT36 M: wallgrip36, the bots (Harness.N36.Move.cs)
        N36Level(cmd, args, ref code); // NIGHT36 L: level 1 from scratch, the jobs check, the arena flow (Harness.N36.Level.cs)
        N36Boss(cmd, args, ref code); // NIGHT36 B: walkin36, boss36, THE LANTERN (Harness.N36.Boss.cs)
        N37Kit(cmd, args, ref code); // NIGHT37 K: the Boss37 brain (Harness.N37.Kit.cs)
        N37Wick(cmd, args, ref code); // NIGHT37 W: THE WICK (Harness.N37.Wick.cs)
        N37Stag(cmd, args, ref code); // NIGHT37 S: THE STAG (Harness.N37.Stag.cs)
        N37Loom(cmd, args, ref code); // NIGHT37 O: THE LOOM (Harness.N37.Loom.cs)
        N37Crown(cmd, args, ref code); // NIGHT37 C: THE CROWN (Harness.N37.Crown.cs)
        N38Gates(cmd, args, ref code); // NIGHT38 E: the envelopes and the gate checker (Harness.N38.Gates.cs)
        N38Ride(cmd, args, ref code); // NIGHT38 M: ride38 and doors38 probes (Harness.N38.Ride.cs)
        N38Hud(cmd, args, ref code); // NIGHT38 H: hud38 and juice38 probes (Harness.N38.Hud.cs)
        N38Foundry(cmd, args, ref code); // NIGHT38 F: level 1 and level 6 (Harness.N38.Foundry.cs)
        N38Iceline(cmd, args, ref code); // NIGHT38 S2: level 2 and stag38 (Harness.N38.Iceline.cs)
        N38Kiln(cmd, args, ref code); // NIGHT38 K3: level 3 (Harness.N38.Kiln.cs)
        N38Span(cmd, args, ref code); // NIGHT38 O2: level 4 (Harness.N38.Span.cs)
        N38Spire(cmd, args, ref code); // NIGHT38 C2: level 5 (Harness.N38.Spire.cs)
        N39Pistol(cmd, args, ref code); // NIGHT39 P1: the KESTREL (Harness.N39.Pistol.cs)
        N39Ride(cmd, args, ref code); // NIGHT39 R1: ride39 (Harness.N39.Ride.cs)
        N39Slow(cmd, args, ref code); // NIGHT39 F1: slow39 and the parry (Harness.N39.Slow.cs)
        N39Boss(cmd, args, ref code); // NIGHT39 B1: boss39 bands (Harness.N39.Boss.cs)
        N39Views(cmd, args, ref code); // NIGHT39 V: the NIGHT38 views and the two tests (Harness.N39.Views.cs)
        N40Start(cmd, args, ref code); // NIGHT40 S1: start40, kiln40, cut40, rods40 (Harness.N40.Start.cs)
        N40Weapons(cmd, args, ref code); // NIGHT40 W1: charge40 (Harness.N40.Weapons.cs)
        N40Ride(cmd, args, ref code); // NIGHT40 R2: ride40 (Harness.N40.Ride.cs)
        N40Iceline(cmd, args, ref code); // NIGHT40 L2: n40-iceline (Harness.N40.Iceline.cs)
        N40Brawl(cmd, args, ref code); // NIGHT40 B2: the brawler (Harness.N40.Brawl.cs)
        N40Loom(cmd, args, ref code); // NIGHT40 B4: THE LOOM (Harness.N40.Loom.cs)
        N40Crown(cmd, args, ref code); // NIGHT40 B5: THE CROWN (Harness.N40.Crown.cs)
        N40Foes(cmd, args, ref code); // NIGHT40 E3: the campaign roster (Harness.N40.Foes.cs)
        N41Proof(cmd, args, ref code); // NIGHT41 T2: the proof tooling (Harness.N41.Proof.cs)
        N41Juice(cmd, args, ref code); // NIGHT41 J2: the boss moment, doors, bars, the select (Harness.N41.Juice.cs)
        N41Look(cmd, args, ref code); // NIGHT41 K2: the levels' look (Harness.N41.Look.cs)
        N41Bosses(cmd, args, ref code); // NIGHT41 B8: sweep41, THE ICEBREAKER (Harness.N41.Bosses.cs)
        N41Enemies(cmd, args, ref code); // NIGHT41 E6: tokens41, heavy41, kiln41 (Harness.N41.Enemies.cs)
        N41Pad(cmd, args, ref code); // NIGHT41 P3: aim41 (Harness.N41.Pad.cs)
        N41Levels(cmd, args, ref code); // NIGHT41 R5: spire41, foundry41 (Harness.N41.Levels.cs)
        N41Finale(cmd, args, ref code); // NIGHT41 L6: the level 6 greybox (Harness.N41.Finale.cs)
        N42Move(cmd, args, ref code); // NIGHT42 M1: wallfling42, boost42 (Harness.N42.Move.cs)
        N42Level(cmd, args, ref code); // NIGHT42 L1: n42-foundry, teach42 (Harness.N42.Level.cs)
        N42Weapons(cmd, args, ref code); // NIGHT42 W1: gauntlet42, parry42, charge42, reticle42 (Harness.N42.Weapons.cs)
        N42Impact(cmd, args, ref code); // NIGHT42 F1: impact42, kills42 (Harness.N42.Impact.cs)
        N42Ui(cmd, args, ref code); // NIGHT42 U1: meter42, style42, health42 (Harness.N42.Ui.cs)
        N42Brawl(cmd, args, ref code); // NIGHT42 B1: brawl42 (Harness.N42.Brawl.cs)
        N42Arena(cmd, args, ref code); // NIGHT42 K1: arena42 (Harness.N42.Arena.cs)
        N43Move(cmd, args, ref code); // NIGHT43 M1: chimney43, ride43, boost43 (Harness.N43.Move.cs)
        N43Level(cmd, args, ref code); // NIGHT43 L1: n43-foundry (Harness.N43.Level.cs)
        N43Weapons(cmd, args, ref code); // NIGHT43 W1: gauntlet43, parry43, fusion43 (Harness.N43.Weapons.cs)
        N43Rooms(cmd, args, ref code); // NIGHT43 E1: rooms43, iceline43 (Harness.N43.Rooms.cs)
        N43Kiln(cmd, args, ref code); // NIGHT43 K1: kiln43 (Harness.N43.Kiln.cs)
        N43Burrow(cmd, args, ref code); // NIGHT43 A1: burrow43 (Harness.N43.Burrow.cs)
        N43Wick(cmd, args, ref code); // NIGHT43 B1: wick43 (Harness.N43.Wick.cs)
        Rink28(cmd, args, ref code); // NIGHT28: rink28 [build|friction|stop|bank|cap|ride|lane|lip|sim] (builder A2, Harness.N28.Rink.cs)
        N25(cmd, args, ref code); // NIGHT25: n25shared (the lead) and the rows' commands (Harness.N25.cs)
        N24(cmd, args, ref code); // NIGHT24: n24shared (the lead) and the rows' commands (Harness.N24.cs)
        N22(cmd, args, ref code); // NIGHT22: n22shared (the lead) and the rows' commands (Harness.N22.cs)
        N21(cmd, args, ref code); // NIGHT21: n21shared (the lead), arcline / slidebite (M), stamp / deadline (S), grind (W) (Harness.N21.cs)
        Sandbox(cmd, args, ref code); // NIGHT19: THE RANGE, THE CHASE, THE PARITY FARM (Harness.Sandbox.cs, the lead)
        if (code < 0)
        {
            Console.WriteLine($"unknown command '{cmd}'");
            return 2;
        }
        return code;
    }

    /// <summary>Agent N's commands (Harness.Bot.cs): code = the exit code when the command was theirs.</summary>
    static partial void Bot(string cmd, string[] args, ref int code);
    // NIGHT30: the builders' harness commands (a partial with no body costs nothing)
    static partial void N30Systems(string cmd, string[] args, ref int code);
    static partial void N30Boss(string cmd, string[] args, ref int code);
    static partial void N30Levels(string cmd, string[] args, ref int code);
    static partial void N30View(string cmd, string[] args, ref int code);
    // NIGHT31: the builders' harness commands (a partial with no body costs nothing)
    static partial void N31Grapple(string cmd, string[] args, ref int code);
    static partial void N31Weapons(string cmd, string[] args, ref int code);
    static partial void N31Boss(string cmd, string[] args, ref int code);
    static partial void N31Levels(string cmd, string[] args, ref int code);
    // NIGHT32: the builders' harness commands (a partial with no body costs nothing)
    static partial void N32Boss(string cmd, string[] args, ref int code);
    static partial void N32Grapple(string cmd, string[] args, ref int code);
    static partial void N32Arenas(string cmd, string[] args, ref int code);
    static partial void N32Span(string cmd, string[] args, ref int code);
    static partial void N32Finale(string cmd, string[] args, ref int code);
    static partial void N32Crown(string cmd, string[] args, ref int code);
    // NIGHT33: the builders' harness commands (a partial with no body costs nothing)
    static partial void N33Look(string cmd, string[] args, ref int code);
    static partial void N33Kit(string cmd, string[] args, ref int code);
    static partial void N33Arenas(string cmd, string[] args, ref int code);
    static partial void N33Enemies(string cmd, string[] args, ref int code);
    static partial void N33Weapons(string cmd, string[] args, ref int code);
    static partial void N33Shop(string cmd, string[] args, ref int code);
    // NIGHT34: the builders' harness commands (a partial with no body costs nothing)
    static partial void N34Bend(string cmd, string[] args, ref int code);
    static partial void N34Lines(string cmd, string[] args, ref int code);
    static partial void N34Flow(string cmd, string[] args, ref int code);
    // NIGHT35: the builders' harness commands (a partial with no body costs nothing)
    static partial void N35Weapons(string cmd, string[] args, ref int code);
    static partial void N35Teach(string cmd, string[] args, ref int code);
    static partial void N35Move(string cmd, string[] args, ref int code);
    static partial void N35Look(string cmd, string[] args, ref int code);
    static partial void N35Levels(string cmd, string[] args, ref int code);
    // NIGHT36: the builders' harness commands (a partial with no body costs nothing)
    static partial void N36Guns(string cmd, string[] args, ref int code);
    static partial void N36Core(string cmd, string[] args, ref int code);
    static partial void N36Move(string cmd, string[] args, ref int code);
    static partial void N36Level(string cmd, string[] args, ref int code);
    static partial void N36Boss(string cmd, string[] args, ref int code);
    // NIGHT37: the builders' harness commands (a partial with no body costs nothing)
    static partial void N37Kit(string cmd, string[] args, ref int code);
    static partial void N37Wick(string cmd, string[] args, ref int code);
    static partial void N37Stag(string cmd, string[] args, ref int code);
    static partial void N37Loom(string cmd, string[] args, ref int code);
    static partial void N37Crown(string cmd, string[] args, ref int code);
    // NIGHT38: the builders' harness commands (a partial with no body costs nothing)
    static partial void N38Gates(string cmd, string[] args, ref int code);
    static partial void N38Ride(string cmd, string[] args, ref int code);
    static partial void N38Hud(string cmd, string[] args, ref int code);
    static partial void N38Foundry(string cmd, string[] args, ref int code);
    static partial void N38Iceline(string cmd, string[] args, ref int code);
    static partial void N38Kiln(string cmd, string[] args, ref int code);
    static partial void N38Span(string cmd, string[] args, ref int code);
    static partial void N38Spire(string cmd, string[] args, ref int code);
    // NIGHT39: the builders' harness commands (a partial with no body costs nothing)
    static partial void N39Pistol(string cmd, string[] args, ref int code);
    static partial void N39Ride(string cmd, string[] args, ref int code);
    static partial void N39Slow(string cmd, string[] args, ref int code);
    static partial void N39Boss(string cmd, string[] args, ref int code);
    static partial void N39Views(string cmd, string[] args, ref int code);
    // NIGHT40: the builders' harness commands (a partial with no body costs nothing)
    static partial void N40Start(string cmd, string[] args, ref int code);
    static partial void N40Weapons(string cmd, string[] args, ref int code);
    static partial void N40Ride(string cmd, string[] args, ref int code);
    static partial void N40Iceline(string cmd, string[] args, ref int code);
    static partial void N40Brawl(string cmd, string[] args, ref int code);
    static partial void N40Loom(string cmd, string[] args, ref int code);
    static partial void N40Crown(string cmd, string[] args, ref int code);
    static partial void N40Foes(string cmd, string[] args, ref int code);
    // NIGHT41: the builders' harness commands (a partial with no body costs nothing)
    static partial void N41Proof(string cmd, string[] args, ref int code);
    static partial void N41Juice(string cmd, string[] args, ref int code);
    static partial void N41Look(string cmd, string[] args, ref int code);
    static partial void N41Bosses(string cmd, string[] args, ref int code);
    static partial void N41Enemies(string cmd, string[] args, ref int code);
    static partial void N41Pad(string cmd, string[] args, ref int code);
    static partial void N41Levels(string cmd, string[] args, ref int code);
    static partial void N41Finale(string cmd, string[] args, ref int code);
    // NIGHT42: the builders' harness commands (a partial with no body costs nothing)
    static partial void N42Move(string cmd, string[] args, ref int code);
    static partial void N42Level(string cmd, string[] args, ref int code);
    static partial void N42Weapons(string cmd, string[] args, ref int code);
    static partial void N42Impact(string cmd, string[] args, ref int code);
    static partial void N42Ui(string cmd, string[] args, ref int code);
    static partial void N42Brawl(string cmd, string[] args, ref int code);
    static partial void N42Arena(string cmd, string[] args, ref int code);
    // NIGHT43: the builders' harness commands (a partial with no body costs nothing)
    static partial void N43Move(string cmd, string[] args, ref int code);
    static partial void N43Level(string cmd, string[] args, ref int code);
    static partial void N43Weapons(string cmd, string[] args, ref int code);
    static partial void N43Rooms(string cmd, string[] args, ref int code);
    static partial void N43Kiln(string cmd, string[] args, ref int code);
    static partial void N43Burrow(string cmd, string[] args, ref int code);
    static partial void N43Wick(string cmd, string[] args, ref int code);

    /// <summary>Agent A's commands (Harness.Autopilot.cs).</summary>
    static partial void Autopilot(string cmd, string[] args, ref int code);

    /// <summary>Agent T's commands (Harness.Tape.cs).</summary>
    static partial void Tape(string cmd, string[] args, ref int code);

    /// <summary>The lead's commands (Harness.Lead.cs).</summary>
    static partial void Lead(string cmd, string[] args, ref int code);

    /// <summary>NIGHT15 U3-28.6: builder G's commands (Harness.MapGen.cs: a map built from its plan without the Editor).</summary>
    static partial void MapGen(string cmd, string[] args, ref int code);
}
