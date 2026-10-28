// NIGHT28 builder B2: the Player scenario of the lap fight (scenario id "glacier28", PhaseJ in FeelReel.N27.cs; the lead's N27Scenario calls N28Glacier28Body, the body lives here).
//   glacier28   THE GLACIER on the rink (design.n28.glacier): the fight starts through Glacier28Begin (the plan id 'glacier' routes to it when the lead wires BossRoomBegin). The harness's GlacierPilot28
//               (Game/Bot/GlacierPilot28.cs) skates the lap, takes the lines the attacks leave open, cracks the glaze with the pistol and blasts with LT in the window: its Move / Combat go in through
//               g.forcedMove / g.forcedCombat / g.input.SetLook, the entries every reel scenario uses (stepped once per SIM step through PilotDrive27). The glove starts FULL (the ice has no kills) and a hit gives
//               5 pips back. NO HP CUT anywhere: the scenario only READS boss HP. A PASS needs the boss dead by the pilot's own blasts (the alive flag and a bossDown event). Every loop is bounded by GAME time
//               (the sim's steps) and a wall-clock guard. One PASS / RED line.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N28Glacier28Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Glacier28Scenario(g, dir, res, offs, pre);

        static bool Down28(SurvivalGame g)
        {
            SurvivalSim sim = g.sim;
            if (!(g.health.Dead || sim.flow == Flow.Down)) return false;
            g.health.Reset();
            sim.downSteps = 0;
            sim.flow = Flow.Playing;
            sim.Glacier28Begin();
            g.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
            return true;
        }

        static IEnumerator Glacier28Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, g.Feet);
            if (!sim.Glacier28On) { res[0] = "RED: design.n28.glacier.enabled is off (the plan's boss 'glacier' is NIGHT27's glacier) | " + pc; yield break; }
            offs.Add(() => Quiet(g));
            g.husks.frozen = true;
            if (!sim.Glacier28Begin() || sim.glacier28 == null || !sim.glacier28.Alive) { res[0] = "RED: Glacier28Begin did not start the lap fight (no BossKit) | " + pc; yield break; }
            Glacier28 gl = sim.glacier28;
            if (sim.loadout.owned.IndexOf("gauntlet") < 0) sim.loadout.AddWeapon("gauntlet");
            Vec3 spawn = sim.Glacier28Spawn;
            double yawDeg = sim.Glacier28SpawnYawDeg;
            g.Respawn(spawn, yawDeg);
            pc = Preconditions(g, spawn);
            PulseCharge27 p27 = sim.pulse27;
            bool live = sim.n27.pulse27 && p27 != null && p27.active;
            int banked = live ? p27.pips : 0; // Glacier28Begin has already banked the glove: the pips it arrives with
            if (live) while (p27.pips < p27.p.pips) { p27.AddKill(); }
            yield return Frames(20);
            Start(g, "glacier28", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 260;
            int downs = 0;
            offs.Add(g.events.On<BossEvent>("bossDown", e => downs++));
            var pilot = new GlacierPilot28 { hand = new Mulberry32(Seed * 7919u + 13u) };
            var drive = new PilotDrive27 { tick = sim.simStep };
            double yawL = yawDeg * Math.PI / 180;
            int step0 = sim.simStep, deaths = 0, retries0 = sim.glacier28Retries, hits0 = 0;
            double gapMin = 1e9, gapMax = -1e9;
            bool markedWin = false, markedHit = false, markedCrack = false;
            string lastPhase = "";
            var phases = new List<string>();
            while (gl.Alive && (sim.simStep - step0) * sim.step < 240.0 && Time.realtimeSinceStartup < wallEnd)
            {
                if (Down28(g)) { deaths++; drive.tick = sim.simStep; }
                drive.Advance(sim.simStep, t =>
                {
                    pilot.Step(sim, yawL, t);
                    yawL = pilot.Move.yaw;
                    return (pilot.Move, pilot.Combat);
                });
                InputFrame mv = drive.mv;
                if (gl.WindowOpen && !markedWin) { markedWin = true; FeelProbe.Event("glacier28:window"); }
                if (!gl.glazed && !markedCrack) { markedCrack = true; FeelProbe.Event("glacier28:crack"); }
                if (gl.phase.StartsWith("p", StringComparison.Ordinal)) { gapMin = Math.Min(gapMin, gl.gap); gapMax = Math.Max(gapMax, gl.gap); }
                if (!markedHit && gl.hits > hits0) { markedHit = true; FeelProbe.Event("glacier28:pulseHit"); }
                g.input.SetLook(mv.yaw, mv.pitch);
                g.forcedMove = mv;
                g.forcedCombat = drive.cb;
                if (gl.phase != lastPhase) { lastPhase = gl.phase; if (gl.phase.StartsWith("p", StringComparison.Ordinal)) phases.Add($"{gl.phase}@{(sim.simStep - step0) * sim.step:0}s"); }
                yield return null;
            }
            bool killed = !gl.Alive;
            double killSeconds = (sim.simStep - step0) * sim.step;
            Quiet(g);
            yield return Frames(Ms(800));
            yield return Stop();
            int windows = Stat27(gl.stats, "windows"), misses = Stat27(gl.stats, "misses"), pulseHits = Stat27(gl.stats, "pulseHits"), clangs = Stat27(gl.stats, "clangs"), cracks = Stat27(gl.stats, "cracks");
            var fails = new List<string>();
            if (!live) fails.Add("design.n27.pulse27 is off: no glove to blast with");
            if (!killed) fails.Add($"the glacier was not killed in {killSeconds.ToString("0", ci)} s of game time ({gl.hits} of 3 pulse hits, phase {gl.phase}{(Time.realtimeSinceStartup >= wallEnd ? ", wall guard hit" : "")})");
            if (killed && downs < 1) fails.Add("the glacier died but no bossDown event came");
            if (killed && pulseHits < 3) fails.Add($"killed by {pulseHits} pulse hits, not 3");
            res[0] = (fails.Count == 0 ? "PASS" : "RED: " + string.Join("; ", fails)) + " | " + pc + $" | {FeelProbe.FrameCount} frames, GlacierPilot28 through forcedMove / forcedCombat, boss HP only READ (never cut), glove banked {banked} pips at the start: "
                + $"gap {gapMin.ToString("0", ci)}..{gapMax.ToString("0", ci)} m; glaze cracked {cracks}x, windows {windows}, PULSE hits {pulseHits} ({pilot.blasts} LT blasts), misses {misses}, clangs {clangs}, reversals {Stat27(gl.stats, "flips")}; "
                + $"{(killed ? "KILLED" : "NOT killed")} at {killSeconds.ToString("0.0", ci)} s of game time, deaths {deaths}, retries {sim.glacier28Retries - retries0}, bossDown x{downs}; {string.Join(" ", phases)}";
        }
    }
}
