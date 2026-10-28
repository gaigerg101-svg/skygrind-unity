// NIGHT26 builder B: the Player scenarios of the three bosses (hooks in the lead's FeelReel.N26.cs): `hulk` here (THE SLAG HULK, contract hulk); `glacier` and
// `wyrm26` are added below as their rows land. Every line starts with the PRECONDITIONS. RED: the row off (no hulk), no HulkView (the lead's two lines in
// SurvivalGame.Wyrm24.cs are missing), the fight not started, a beat not reached. Not run by its author (dn-check only): the lead runs it.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N26HulkBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) => body = HulkScenario(g, dir, res, pre);

        /// <summary>THE SLAG HULK: the player stands at the room's south end; the fight starts through the campaign's own call (BossRoomBegin "hulk"); the scenario waits
        /// for each beat in turn and marks it for the probe: hulk:intro, hulk:slam (a ring runs), hulk:charge (the line, the run, the stun at a wall), hulk:p2, hulk:p3,
        /// hulk:finisher. The player is in god mode except for the slam (a ring is cleared by a jump: the scenario jumps and prints whether the ring hurt).</summary>
        static IEnumerator HulkScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            SurvivalSim sim = g.sim;
            string pc = Preconditions(g, sim.hulk26Arena == null ? default : sim.Hulk26Spawn);
            if (!sim.Hulk26On)
            {
                res[0] = pc + " | RED: design.n26 level1 is off (no SLAG HULK)";
                yield break;
            }
            if (g.hulkView == null)
            {
                res[0] = pc + " | RED: no HulkView (SurvivalGame.Wyrm24InitView does not call Boss26InitViews)";
                yield break;
            }
            if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg))
            {
                res[0] = pc + " | RED: BossRoomBegin(hulk) failed (the row is on: the arena file was not found, or no BossKit yet)";
                yield break;
            }
            Hulk h = sim.hulk26;
            pc = Preconditions(g, spawn);
            g.husks.frozen = true;
            g.Respawn(spawn, yawDeg);
            g.input.SetLook(0, 0.2);
            God(g);
            yield return Frames(20);
            Start(g, "hulk", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();
            var seen = new List<string>();

            FeelProbe.Event("hulk:intro");
            int guard = 0;
            while (h.phase == "intro" && guard++ < Ms(6000)) { God(g); g.input.SetLook(0, 0.2); yield return null; }
            if (h.phase == "intro") fails.Add("still in the intro after 6 s");
            else seen.Add("intro");

            // the slam: the ring runs; stand in the room's middle ring and jump it (not in god mode for the ring)
            guard = 0;
            while (h.rings.Count == 0 && guard++ < Ms(30000)) { God(g); g.input.SetLook(0, 0.2); yield return null; }
            if (h.rings.Count > 0) { FeelProbe.Event("hulk:slam"); seen.Add("slam ring (view rings drawn " + g.hulkView.Shown + ")"); }
            else fails.Add("no slam ring in 30 s");
            guard = 0;
            while (h.rings.Count > 0 && guard++ < Ms(4000)) { God(g); yield return null; }

            // the charge: its line, the run, the stun at the wall
            guard = 0;
            while (!h.Stunned && guard++ < Ms(40000)) { God(g); g.input.SetLook(0, 0.2); yield return null; }
            if (h.Stunned) { FeelProbe.Event("hulk:charge"); seen.Add("charge into a wall, stunned"); for (int i = 0; i < Ms(1200); i++) { God(g); yield return null; } }
            else fails.Add("no stun in 40 s");

            // phases: cut its HP to the thresholds, then the finisher
            h.body.hp = h.body.maxHp * 0.6;
            h.Apply(0);
            guard = 0;
            while (h.phase != "p2" && guard++ < Ms(8000)) { God(g); yield return null; }
            if (h.phase == "p2") { FeelProbe.Event("hulk:p2"); seen.Add("phase 2"); } else fails.Add("phase 2 not reached (" + h.phase + ")");
            for (int i = 0; i < Ms(1500); i++) { God(g); yield return null; }
            h.body.hp = h.body.maxHp * 0.3;
            h.Apply(0);
            guard = 0;
            while (h.phase != "p3" && guard++ < Ms(8000)) { God(g); yield return null; }
            if (h.phase == "p3") { FeelProbe.Event("hulk:p3"); seen.Add("phase 3"); } else fails.Add("phase 3 not reached (" + h.phase + ")");
            for (int i = 0; i < Ms(1500); i++) { God(g); yield return null; }
            FeelProbe.Event("hulk:finisher");
            h.body.hp = 1;
            h.Apply(5);
            guard = 0;
            while (h.Alive && guard++ < Ms(8000)) { God(g); yield return null; }
            if (!h.Alive && g.events.Emitted.ContainsKey("bossDown")) seen.Add("finisher");
            else fails.Add("the hulk did not die (phase " + h.phase + ")");
            yield return Frames(Ms(800));
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {pc}; design.n26 level1 ON; beats seen: {string.Join(", ", seen)}; tells logged {h.log.Count} | " + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
