// NIGHT72 builder N, slice N3: `n72goal` drives Goal72Math (the pure goal72 state machine, Assets/Scripts/Game/Goal72Math.cs) through planted cases and prints one PASS / FAIL line each:
//   a normal approach (the feet come closer every second): stage 1 all the way, no hint
//   a stuck player (a cleared room, the exit far, the feet still): stage 1 at 0 s, 2 at 10 s, 3 at 20 s, each counted once
//   a scripted player idle (a reel's forced input, the switch off): stage 1 and never a hint; with the planted switch on it reaches 2 and 3
//   progress while a hint is up: the hint is retired (counted), the stage falls back to 1 and the hint does not come back in the room
//   a verb already used: stage 3 does not teach it again (the double jump used: the ride is taught; both used: "KEEP GOING")
//   the texts: every stage and direction (up / down / flat / behind) is at most 6 words, brackets and glyph placeholders aside
//   a new room resets the clock; the player inside the hide radius (active false) shows nothing
// Exit 1 on any FAIL. Usage: dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll n72goal
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void N72GoalInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n72goal") return;
        int code;
        try
        {
            int ci = Array.IndexOf(argv, "--campaign");
            code = ci >= 0 ? N72GoalCampaign(ci + 1 < argv.Length && !argv[ci + 1].StartsWith("--") ? argv[ci + 1] : "n52-foundry") : N72Goal();
        }
        catch (Exception e) { Console.WriteLine("n72goal: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    /// <summary>`n72goal --campaign [map id]`: what the stuck72 reel (FeelReel.N72.N.cs) depends on, on the SIM side, headless on a campaign level (default n52-foundry), the player idle in god mode:
    ///   1  room 1 of the level (a taught LINK on the foundry) is NOT cleared by an idle player and KillAll: the t2 reel's "room 1 was not cleared in 90 s" (its kills have nothing to kill)
    ///   2  the lessons marked done (Campaign24's own lessonCap move: lesson.index = defs.Count) clear the room on the campaign's next step: phase Cleared, room 0
    ///   3  Goal72View's gate, copied (a cleared room, the exit outside the hide radius, past the first seconds, a HUMAN's run: !sim.metaSilent), feeds Goal72Math.State with the sim's own feet and
    ///      the room's exit: a scripted idle with the planted switch off stays at stage 1; planted, stage 2 at 10 s and stage 3 at 20 s
    ///   4  a sim run with the AUTOPILOT attached has metaSilent on (so Goal72View's gate is shut: the t2 reel kept its bot attached) and off again after Detach()
    /// Not the Unity view (its canvas, its text): the Player's reel decides that.</summary>
    static int N72GoalCampaign(string mapId)
    {
        int bad = 0;
        void Say(bool ok, string m) { Console.WriteLine((ok ? "PASS " : "FAIL ") + m); if (!ok) bad++; }
        GameConfig cfg = Config();
        string file = Teach35MapFile(cfg, mapId);
        if (file == null) { Console.WriteLine("n72goal --campaign: no design.maps.list entry for " + mapId); return 2; }
        SurvivalMap map = CampaignMapFrom(file, cfg, out _);
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { campaign = true };
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(7272, null, null);
        void Run(int steps) { for (int s = 0; s < steps; s++) { sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.Step(); } }
        Run(120 * 4); // the entry
        Campaign24 c = sim.campaign;
        if (c == null) { Console.WriteLine("n72goal --campaign: no campaign object after 4 s"); return 2; }
        RoomDef room = c.Room;
        Console.WriteLine($"  level {c.Level.id}: {c.Level.rooms.Count} rooms; room {c.room} '{room?.id}' kind {room?.kind}, waves {room?.WaveCount}, lesson {(c.lesson == null ? "none" : c.lesson.defs.Count + " step(s)")}, spawn ({room?.spawn.x:0.0}, {room?.spawn.y:0.0}, {room?.spawn.z:0.0}) exit ({room?.exit.x:0.0}, {room?.exit.y:0.0}, {room?.exit.z:0.0}), tutorial {N25Blocks.From(cfg).tutorial}");
        // 1: an idle player; the reel's kill loop has nothing to kill
        Run(120 * 24);
        Say(c.room == 0 && c.phase == Campaign24.Phase.Fight, $"1: after 28 s idle room {c.room} is {c.phase} (the t2 reel waited 90 s for a Clear that an idle player on a taught link never gets), alive {sim.enemies.AliveCount}");
        // 2: the lessons done, the campaign clears the room itself
        if (c.lesson != null) c.lesson.index = c.lesson.defs.Count;
        Run(10);
        Say(c.room == 0 && c.phase == Campaign24.Phase.Cleared, $"2: the lessons marked done (lesson.index = defs.Count): room {c.room} is {c.phase} after 10 steps");
        // 3: Goal72View's gate and the pure state machine, on the sim's own numbers
        Run(120 * 6); // past the first seconds
        var st = new Goal72Math.State();
        double[] at = { -1, -1, -1, -1 };
        int maxA = 0;
        double tStart = sim.simStep * sim.step;
        bool human = !sim.metaSilent;
        void View(bool planted, double window, ref int maxStage, double[] stageAt)
        {
            double t0 = sim.simStep * sim.step;
            for (int s = 0; s < window * 120; s++)
            {
                Run(1);
                double t = sim.simStep * sim.step;
                Vec3 f = sim.mover.position, ex = c.Room.exit;
                bool playing = !sim.metaSilent;
                bool cleared = playing && c.phase == Campaign24.Phase.Cleared && !c.finished && c.Room != null;
                double dx = ex.x - f.x, dz = ex.z - f.z, d2 = Math.Sqrt(dx * dx + dz * dz), d3 = Math.Sqrt(dx * dx + dz * dz + (ex.y - f.y) * (ex.y - f.y));
                bool active = cleared && d2 > 6 && c.steps * sim.step >= UnityTuning.Goal72FirstLevelS;
                st.Step(t, active, c.room, d3, true, planted, ex.y - f.y, 0);
                maxStage = Math.Max(maxStage, st.Stage);
                for (int k = 1; k <= 3; k++) if (st.Stage >= k && stageAt[k] < 0) stageAt[k] = t - t0;
            }
        }
        View(false, UnityTuning.Goal72Stuck72WindowS, ref maxA, new double[] { -1, -1, -1, -1 });
        Say(human && maxA == 1 && st.Hint10 == 0 && st.Hint20 == 0 && st.Lines == 1, $"3a: a scripted idle {UnityTuning.Goal72Stuck72WindowS:0} s, planted switch off, human run {human}: stage {maxA} at most, lines {st.Lines}, hints {st.Hint10} / {st.Hint20}");
        int maxB = 0;
        View(true, UnityTuning.Goal72Stuck72WindowS, ref maxB, at);
        Say(Math.Abs(at[2] - UnityTuning.Goal72Hint1S) < 1.0 && Math.Abs(at[3] - UnityTuning.Goal72Hint2S) < 1.0 && st.Hint10 == 1 && st.Hint20 == 1, $"3b: planted: stage 2 at {at[2]:0.0} s, stage 3 at {at[3]:0.0} s (the reel's bar: about {UnityTuning.Goal72Hint1S:0} and {UnityTuning.Goal72Hint2S:0} +-1.5), hints {st.Hint10} / {st.Hint20}");
        // 4: the bot's run rules
        {
            SurvivalSim s2 = SurvivalSim.Headless(cfg, map);
            bool before = s2.metaSilent;
            var ap = new AutopilotInput(s2, "normal", 7272);
            bool during = s2.metaSilent;
            ap.Detach();
            Say(!before && during && !s2.metaSilent, $"4: AutopilotInput sets sim.metaSilent ({before} -> {during}) and Detach() puts it back ({s2.metaSilent}): Goal72View draws for a human's run only");
        }
        Console.WriteLine("n72goal --campaign: " + (bad == 0 ? "all PASS" : bad + " FAIL"));
        return bad == 0 ? 0 : 1;
    }

    static int N72Goal()
    {
        int bad = 0;
        void Say(bool ok, string m) { Console.WriteLine((ok ? "PASS " : "FAIL ") + m); if (!ok) bad++; }
        // 1. a normal approach: 30 m to go, 1.5 m a step of a second -> record every second
        {
            var s = new Goal72Math.State();
            int maxStage = 0;
            for (int i = 0; i < 40; i++) { s.Step(i, true, 0, 30 - i * 0.7, false, false, 0, 0); maxStage = Math.Max(maxStage, s.Stage); }
            Say(maxStage == 1 && s.Hint10 == 0 && s.Hint20 == 0 && s.Lines == 1, $"an approach (0.7 m a second, 40 s): stage {maxStage} at most, lines {s.Lines}, hints {s.Hint10} / {s.Hint20}");
        }
        // 2. stuck
        {
            var s = new Goal72Math.State();
            var at = new[] { -1.0, -1.0, -1.0, -1.0 };
            for (int i = 0; i <= 25 * 10; i++)
            {
                double t = i / 10.0;
                s.Step(t, true, 0, 30, false, false, 0, 0);
                for (int k = 1; k <= 3; k++) if (s.Stage >= k && at[k] < 0) at[k] = t;
            }
            Say(Math.Abs(at[2] - 10) < 0.2 && Math.Abs(at[3] - 20) < 0.2 && s.Lines == 1 && s.Hint10 == 1 && s.Hint20 == 1, $"a stuck player (30 m away, still): stage 2 at {at[2]:0.0} s, stage 3 at {at[3]:0.0} s, counted once each: lines {s.Lines}, 10 s {s.Hint10}, 20 s {s.Hint20}");
        }
        // 3. scripted idle, switch off then planted
        {
            var s = new Goal72Math.State();
            int maxStage = 0;
            for (int i = 0; i <= 250; i++) { s.Step(i / 10.0, true, 0, 30, true, false, 0, 0); maxStage = Math.Max(maxStage, s.Stage); }
            bool none = maxStage == 1 && s.Hint10 == 0 && s.Hint20 == 0;
            double tp = 25, st2 = -1, st3 = -1;
            for (int i = 0; i <= 250; i++)
            {
                double t = 25 + i / 10.0;
                s.Step(t, true, 0, 30, true, true, 0, 0);
                if (s.Stage >= 2 && st2 < 0) st2 = t - tp;
                if (s.Stage >= 3 && st3 < 0) st3 = t - tp;
            }
            Say(none && Math.Abs(st2 - 10) < 0.2 && Math.Abs(st3 - 20) < 0.2, $"a scripted player idle 25 s: max stage {maxStage}, hints 0; then planted: stage 2 at {st2:0.0} s, stage 3 at {st3:0.0} s");
        }
        // 4. progress retires a hint, and it does not come back in the room
        {
            var s = new Goal72Math.State();
            for (int i = 0; i <= 120; i++) s.Step(i / 10.0, true, 0, 30, false, false, 0, 0); // 12 s: stage 2
            int before = s.Stage;
            s.Step(12.1, true, 0, 26, false, false, 0, 0); // 4 m closer: progress
            int after = s.Stage;
            for (int i = 0; i <= 300; i++) s.Step(13 + i / 10.0, true, 0, 26, false, false, 0, 0); // 30 s more, still: no hint comes back
            Say(before == 2 && after == 1 && s.Retired == 1 && s.Stage == 1, $"progress retires a hint: stage {before} -> {after}, retired {s.Retired}, 30 s still afterwards: stage {s.Stage} (the hint is spent for the room)");
        }
        // 5. a verb already used is not taught again
        {
            Goal72Math.Format(3, 5, 0, Goal72Math.Verb.None, out string a, out _);
            Goal72Math.Format(3, 5, 0, Goal72Math.Verb.DoubleJump, out string b, out _);
            Goal72Math.Format(3, 5, 0, Goal72Math.Verb.DoubleJump | Goal72Math.Verb.Ride, out string c, out _);
            Goal72Math.Format(3, 9, 0, Goal72Math.Verb.None, out string d, out _);
            Say(a.Contains("DOUBLE JUMP") && b.Contains("RIDE") && c.Contains("KEEP GOING") && d.Contains("RIDE"), $"a verb used is not taught again: '{a}' / double jump used '{b}' / both used '{c}' / an exit past a double jump '{d}'");
        }
        // 6. the texts
        {
            var lines = new List<string>();
            int worst = 0;
            foreach (double dy in new[] { 0.0, 4.0, 9.0, -4.0 })
                foreach (double bearing in new[] { 0.0, 150.0 })
                    foreach (int stage in new[] { 1, 2, 3 })
                        foreach (var learned in new[] { Goal72Math.Verb.None, Goal72Math.Verb.DoubleJump, Goal72Math.Verb.DoubleJump | Goal72Math.Verb.Ride })
                        {
                            Goal72Math.Format(stage, dy, bearing, learned, out string raw, out _);
                            worst = Math.Max(worst, Goal72Math.Words(raw));
                            if (!lines.Contains(raw)) lines.Add(raw);
                        }
            Say(worst <= 6, $"{lines.Count} distinct lines, at most {worst} words (the bar: 6): " + string.Join(" | ", lines));
        }
        // 7. a new room resets the clock; inside the hide radius nothing shows
        {
            var s = new Goal72Math.State();
            for (int i = 0; i <= 150; i++) s.Step(i / 10.0, true, 0, 30, false, false, 0, 0); // 15 s in room 0: stage 2
            s.Step(15.1, true, 1, 30, false, false, 0, 0); // room 1
            int stageNew = s.Stage;
            s.Step(15.2, false, 1, 3, false, false, 0, 0); // within the hide radius
            Say(stageNew == 1 && s.Stage == 0 && s.StuckS == 0, $"a new room: stage {stageNew} (the clock restarts); inside the hide radius: stage {s.Stage}");
        }
        Console.WriteLine("n72goal: " + (bad == 0 ? "all PASS" : bad + " FAIL"));
        return bad == 0 ? 0 : 1;
    }
}
