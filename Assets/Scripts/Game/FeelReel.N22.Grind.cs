// NIGHT22 U3-36.6 reel `grind` (builder W; UNCOMPILED, written without the Editor): THE GRIND on the Player. A GRIND run on the
// SafeSpot (>= 12 m from every jump pad and buy station; FeelReel.Preconditions opens the result line), the HUD captured:
// wave 1's break (the WAVE banner), the telegraph's rising-edge marks in the break's last seconds, the reshape, the fight,
// then one death and RESULTS (the wave reached and the best wave). The GRIND's records are in memory for the reel
// (GrindRecordsFile(null)) so a reel never writes the player's file; the persistence across a restart is the harness's
// `grind2` and Grind22Tests (a Player cannot restart itself inside a reel).
// RED: with design.n21.grind off the scenario prints SKIPPED; with the feature absent the lead's default is "NOT WRITTEN (RED)".
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N22GrindBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N22GrindScenario(g, dir, res, offs, pre);
        }

        static IEnumerator N22GrindScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            if (N21Blocks.ForceOff || !N21Blocks.From(g.cfg).grind)
            {
                res[0] = "SKIPPED (RED): design.n21.grind is off";
                yield break;
            }
            Quiet(g);
            string pathWas = g.sim.GrindRecordsPath;
            g.sim.GrindRecordsFile(null); // memory only: a reel never writes the player's GRIND file
            g.sim.grindRecords.Clear();
            offs.Add(() =>
            {
                g.setup.mode = RunSetup.Survival;
                g.sim.GrindReleaseHold();
                g.noRecords = true;
                if (pathWas != null) g.sim.GrindRecordsFile(pathWas);
            });
            g.noRecords = false; // the GRIND judges its run against its own (in-memory) records and holds SURVIVAL's off
            g.noLab = true;
            g.setup.mode = RunSetup.Grind;
            g.setup.heat = 0;
            g.StartRun(Seed);
            God(g);
            if (!g.sim.grindOn)
            {
                res[0] = "SKIPPED (RED): the run did not start as a GRIND";
                yield break;
            }
            (Vec3 spot, double yaw, bool gapOk) = SafeSpot(g);
            g.Respawn(spot, yaw * 180 / Math.PI);
            g.input.SetLook(yaw, 0);
            string pre0 = Preconditions(g, spot) + (gapOk ? "" : "; NO SPOT 12 m from every pad and station (the sandbox's open spot)");
            int waveBreaks = 0, waveBanners = 0, markedFrames = 0, rising = 0;
            string preset = "-";
            offs.Add(g.events.On("waveBreak", _ => waveBreaks++));
            offs.Add(g.events.On<BannerEvent>("banner", b => { if (b.text != null && b.text.StartsWith("WAVE", StringComparison.Ordinal)) waveBanners++; }));
            yield return Frames(20);
            Start(g, "grind", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            // wave 1: the break, the telegraph, the reshape, until the fight begins (the 4 s + 1.5 s of config.endless, with room)
            for (int i = 0; i < Ms(9000) && !(g.sim.grind != null && g.sim.grind.Phase == Endless21.State.Fight); i++)
            {
                God(g);
                if (g.sim.GrindTelegraphing)
                {
                    if (markedFrames++ == 0)
                    {
                        preset = g.sim.GrindNextPreset;
                        if (g.sim.GrindRising != null) foreach (bool b in g.sim.GrindRising) if (b) rising++;
                    }
                }
                yield return null;
            }
            yield return Frames(Ms(800)); // the fight's first moments on the new shape
            // one death: the GRIND ends, RESULTS shows the wave and the best wave
            g.health.max = g.health.hp = 100;
            g.health.Damage(g.health.max * 2, "husk", new Vec3(spot.x, spot.y, spot.z - 3));
            for (int i = 0; i < Ms(8000) && g.flow != Flow.Results; i++) yield return null;
            yield return Frames(Ms(1500)); // the tally counts up
            yield return Stop();
            EndlessStats gs = g.sim.lastGrindStats;
            res[0] = $"{pre0}; {FeelProbe.FrameCount} frames; waveBreak {waveBreaks}x, WAVE banners {waveBanners}x, telegraph on for {markedFrames} frames ('{preset}', {rising} pillars marked rising); " +
                     $"death: flow {g.flow}, wave {(gs != null ? gs.lastWave : 0)}, best wave {g.sim.grindRecords.BestWave(0)}, line '{g.sim.lastGrindJudgement?.text}'";
        }
    }
}
