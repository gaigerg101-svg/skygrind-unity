// NIGHT24 leftover 8 (builder B): the reel scenario `monolith` (NOT registered in the lead's phase list; the two lines to add are in the report:
// "monolith" in FeelReel.PhaseG and `case "monolith": body = MonolithScenario(g, dir, res, pre); break;` in N24Scenario). The player stands at the room's
// south end looking at the boss; the fight starts (Monolith24Begin: the BossKit's intro card, the stone rising), and the scenario waits for each beat in turn,
// marking it for the probe: monolith:intro, monolith:slam (the first shock ring), monolith:kneel (the core drops, white-hot), monolith:p2 (held 12 s, the player
// NOT in god mode: surges and sweeps), monolith:laser (phase 3), monolith:finisher. RED: the row off, no view, the fight not started, a beat not reached in its time.
// Not compiled by its author before dn-check.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static IEnumerator MonolithScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            Vec3 at = g.sim.monolith24Arena == null ? default : g.sim.Monolith24Spawn;
            string pc = Preconditions(g, at);
            if (!g.sim.Monolith24On)
            {
                res[0] = pc + " | RED: design.n24.monolith is off (no MONOLITH)";
                yield break;
            }
            if (g.monolithView == null)
            {
                res[0] = pc + " | RED: no MonolithView (SurvivalGame.Wyrm24InitView does not build it)";
                yield break;
            }
            if (!g.sim.Monolith24Begin())
            {
                res[0] = pc + " | RED: Monolith24Begin failed (the row is on: the arena file was not found, or no BossKit yet)";
                yield break;
            }
            Monolith m = g.sim.monolith24;
            pc = Preconditions(g, g.sim.Monolith24Spawn);
            g.husks.frozen = true;
            g.Respawn(g.sim.Monolith24Spawn, 0);
            g.input.SetLook(0, 0.3);
            God(g);
            yield return Frames(20);
            Start(g, "monolith", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();
            var seen = new List<string>();

            FeelProbe.Event("monolith:intro");
            int guard = 0;
            while (m.phase == "intro" && guard++ < Ms(6000)) { God(g); g.input.SetLook(0, 0.3); yield return null; }
            if (m.phase == "intro") fails.Add("still in the intro after 6 s");
            else seen.Add("intro");

            // phase 1: the first ring, then the kneel after the third slam
            guard = 0;
            bool ring = false;
            while (!ring && guard++ < Ms(30000))
            {
                God(g);
                g.input.SetLook(0, 0.3);
                for (int i = 0; i < m.RingCount; i++) if (m.RingLive(i)) ring = true;
                yield return null;
            }
            if (ring) { FeelProbe.Event("monolith:slam"); seen.Add("slam ring"); }
            else fails.Add("no shock ring in 30 s");
            guard = 0;
            while (!m.Kneeling && guard++ < Ms(60000)) { God(g); g.input.SetLook(0, 0.3); yield return null; }
            if (m.Kneeling) { FeelProbe.Event("monolith:kneel"); seen.Add("kneel"); for (int i = 0; i < Ms(1500); i++) { God(g); yield return null; } }
            else fails.Add("no kneel in 60 s");

            // phase 2, real: the midpoint, then 12 s with the player at 100 HP in the boss's reach
            m.body.hp = m.body.maxHp * 0.6;
            m.Apply(0);
            guard = 0;
            while (m.phase != "p2" && guard++ < Ms(8000)) { God(g); yield return null; }
            if (m.phase == "p2") seen.Add("midpoint, phase 2");
            else fails.Add("phase 2 not reached (" + m.phase + ")");
            FeelProbe.Event("monolith:p2");
            g.health.max = g.health.hp = 100;
            double lost = 0, prevHp = g.health.hp;
            int slam0 = m.stats["slams"], ring0 = m.stats["ringHits"], orb0 = m.stats["orbHits"], surge0 = m.stats["surges"], sweep0 = m.stats["sweeps"], hits0 = m.stats["surgeHits"] + m.stats["sweepHits"];
            int p2Frames = 0;
            while (m.phase == "p2" && p2Frames < Ms(14000))
            {
                p2Frames++;
                if (g.health.hp < prevHp) lost += prevHp - g.health.hp;
                if (g.health.hp < 30) g.health.hp = 100;
                prevHp = g.health.hp;
                g.input.SetLook(0, 0.3);
                yield return null;
            }
            string p2Line = $"phase 2 held {p2Frames / (double)Ms(1000):0.0} s: slams {m.stats["slams"] - slam0}, ring hits {m.stats["ringHits"] - ring0}, orb hits {m.stats["orbHits"] - orb0}, surges {m.stats["surges"] - surge0}, sweeps {m.stats["sweeps"] - sweep0}, surge + sweep hits {m.stats["surgeHits"] + m.stats["sweepHits"] - hits0}, HP lost {lost:0}";
            God(g);

            // phase 3: the laser
            m.body.hp = m.body.maxHp * 0.3;
            m.Apply(0);
            guard = 0;
            while (m.phase != "p3" && guard++ < Ms(4000)) { God(g); yield return null; }
            guard = 0;
            while (!m.LaserLive && guard++ < Ms(40000)) { God(g); g.input.SetLook(0, 0.3); yield return null; }
            if (m.LaserLive) { FeelProbe.Event("monolith:laser"); seen.Add("laser"); for (int i = 0; i < Ms(3000); i++) { God(g); yield return null; } }
            else fails.Add("no laser in 40 s of phase 3");

            // the finisher
            FeelProbe.Event("monolith:finisher");
            m.body.hp = 1;
            m.Apply(5);
            guard = 0;
            while (m.Alive && guard++ < Ms(8000)) { God(g); yield return null; }
            if (!m.Alive && g.events.Emitted.ContainsKey("bossDown")) seen.Add("finisher");
            else fails.Add("MONOLITH did not die (phase " + m.phase + ")");
            yield return Frames(Ms(800));
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {pc}; design.n24.monolith ON; beats seen: {string.Join(", ", seen)}; {p2Line} | " + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
