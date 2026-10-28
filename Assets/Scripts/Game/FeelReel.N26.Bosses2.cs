// NIGHT26 builder B: the Player scenarios `glacier` and `wyrm26` (hooks in the lead's FeelReel.N26.cs; `hulk` is in FeelReel.N26.Bosses.cs). Every line starts with the
// PRECONDITIONS. RED: the row off, no view, the fight not started, a beat not reached. Not run by its author (dn-check only): the lead runs them.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N26GlacierBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = GlacierDriveScenario(g, dir, res, offs, pre);
        static partial void N26Wyrm26Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = Wyrm26Scenario(g, dir, res, pre);


        /// <summary>The worm at level 3, BEFORE (n26 level3 off: NIGHT25's worm) and AFTER (on): the player stands at the room's spawn for 60 s of each (not in god mode; HP topped up
        /// under 30) and the player hits (spit hits + contacts) per minute are printed for both, with the aggros, the lane-2 volleys and the arena's box count. AFTER must be higher.</summary>
        static IEnumerator Wyrm26Scenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, sim.Wyrm24Spawn);
            if (!sim.n26.level3) { res[0] = pc + " | RED: design.n26 level3 is off (NIGHT25's worm only)"; yield break; }
            var rates = new double[2];
            var lines = new string[2];
            Start(g, "wyrm26", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            for (int mode = 0; mode < 2; mode++)
            {
                bool on = mode == 1;
                if (sim.wyrm24 != null && sim.wyrm24.Alive) sim.wyrm24.Clear();
                sim.n26.level3 = on;
                if (!sim.Wyrm24Begin()) { res[0] = pc + " | RED: Wyrm24Begin failed"; yield return Stop(); yield break; }
                Wyrm w = sim.wyrm24;
                g.husks.frozen = true;
                g.Respawn(sim.Wyrm24Spawn, 0);
                g.health.max = g.health.hp = 100;
                FeelProbe.Event(on ? "wyrm26:after" : "wyrm26:before");
                int guard = 0;
                while (w.phase == "intro" && guard++ < Ms(8000)) { g.health.hp = 100; yield return null; }
                w.body.hp = w.body.maxHp * 0.5; // phase 2 straight away: the spit lanes and the aggro are what differ
                w.Apply(0);
                int t0 = w.stepNo;
                for (int i = 0; i < Ms(60000); i++)
                {
                    if (g.health.hp < 30) g.health.hp = 100;
                    g.input.SetLook(0, 0.0);
                    yield return null;
                }
                int hits = w.stats["shotsHit"] + w.stats["contactsBody"] + w.stats["contactsHead"] + w.stats["contactsLunge"];
                double minutes = Math.Max(1e-6, (w.stepNo - t0) * g.cfg.Dt / 60);
                rates[mode] = hits / minutes;
                lines[mode] = $"level3 {(on ? "ON" : "off")}: {hits} player hits in {minutes:0.0} min = {rates[mode]:0.0}/min, aggros {w.stats["aggros"]}, lane-2 volleys {w.stats["spits2"]}, arena boxes {sim.wyrm24Arena.boxes.Count}";
            }
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {pc}; {lines[0]}; {lines[1]} | " + (rates[1] > rates[0] ? "PASS" : "FAIL (RED): hits per minute did not rise");
        }
    }
}
