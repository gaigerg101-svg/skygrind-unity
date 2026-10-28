// NIGHT24 builder B: the reel scenario `wyrm` (the contract: "the reel shows the intro, the coil, the dive telegraph and the finisher").
// The player (god mode) stands at the worm room's spawn looking at its middle; the sim's fight starts (Wyrm24Begin: the BossKit's intro card,
// the worm rising from the floor hole), and the scenario waits for each beat in turn, marking it for the probe: wyrm:intro, wyrm:coil, wyrm:dive
// (the marked wall spot is up), wyrm:finisher (the HP is cut to the thresholds so the phase beats and the slow-mo finisher play). RED: the row
// off (no worm), no WyrmView (the lead's hook does not call Wyrm24InitView), the fight not started, or any beat not reached in its time.
// Every result line starts with the scenario's preconditions. Not compiled by its author.
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N24WyrmBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = WyrmScenario(g, dir, res, pre);
        }

        static IEnumerator WyrmScenario(SurvivalGame g, string dir, string[] res, int pre)
        {
            Vec3 at = g.sim.wyrm24Arena == null ? default : g.sim.Wyrm24Spawn;
            string pc = Preconditions(g, at);
            if (!g.sim.n24.wyrm)
            {
                res[0] = pc + " | RED: design.n24.wyrm is off (no worm)";
                yield break;
            }
            if (g.wyrmView == null)
            {
                res[0] = pc + " | RED: no WyrmView (SurvivalGame.N24InitViewsHook does not call Wyrm24InitView)";
                yield break;
            }
            if (!g.sim.Wyrm24Begin())
            {
                res[0] = pc + " | RED: Wyrm24Begin failed (the row is on: the design files were not found, or no BossKit yet)";
                yield break;
            }
            Wyrm w = g.sim.wyrm24;
            pc = Preconditions(g, g.sim.Wyrm24Spawn);
            g.husks.frozen = true;
            g.Respawn(g.sim.Wyrm24Spawn, 0); // yaw 0 looks toward web -z: from the room's south wall to its middle
            g.input.SetLook(0, 0.28); // a little up: the coil rises, the dive's wall spot is high
            God(g);
            yield return Frames(20);
            Start(g, "wyrm", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var fails = new List<string>();
            var seen = new List<string>();

            // --- the intro: the card, the worm rising out of the floor hole ----------------------------------------------------------
            FeelProbe.Event("wyrm:intro");
            int guard = 0;
            while (w.phase == "intro" && guard++ < Ms(9000))
            {
                God(g);
                g.input.SetLook(0, 0.28);
                yield return null;
            }
            if (w.phase == "intro") fails.Add("still in the intro after 9 s");
            else seen.Add("intro");
            bool card = g.events.Emitted.ContainsKey("bossIntro");
            if (!card) fails.Add("no bossIntro event");

            // --- the coil: the first pattern, a few seconds of it ----------------------------------------------------------------------
            FeelProbe.Event("wyrm:coil");
            guard = 0;
            while (w.PatternName != "coil" && guard++ < Ms(6000)) yield return null;
            if (w.PatternName == "coil") seen.Add("coil");
            else fails.Add("no coil pattern");
            for (int i = 0; i < Ms(4500); i++)
            {
                God(g);
                g.input.SetLook(0, 0.28);
                yield return null;
            }

            // --- the dive telegraph: wait for the marked wall spot, hold on it ---------------------------------------------------------
            guard = 0;
            bool marked = false;
            while (!marked && guard++ < Ms(60000))
            {
                God(g);
                g.input.SetLook(0, 0.28);
                foreach (WyrmMarker m in w.markers)
                    if (m.kind == "diveExit") marked = true;
                yield return null;
            }
            if (marked)
            {
                FeelProbe.Event("wyrm:dive");
                seen.Add("dive telegraph");
                for (int i = 0; i < Ms(2200); i++)
                {
                    God(g);
                    yield return null;
                }
            }
            else fails.Add("no marked wall spot (diveExit) in 60 s");

            // --- a real phase 2: the beat, then >= 12 s of the fight with the player NOT in god mode, standing in the lunge's line ----------------------------
            // (it was a flick through phase 2 before: 0 spit volleys). The player's HP is topped up to 100 whenever it falls under 30, the damage taken is summed.
            FeelProbe.Event("wyrm:finisher");
            w.body.hp = w.body.maxHp * 0.6;
            w.Apply(0);
            guard = 0;
            while (w.phase != "p2" && guard++ < Ms(4000)) { God(g); yield return null; }
            if (w.phase == "p2") seen.Add("phase 2");
            else fails.Add("phase 2 not reached (" + w.phase + ")");
            g.Respawn(g.sim.wyrm24Origin, 0);
            g.health.max = g.health.hp = 100;
            double lost = 0, prevHp = g.health.hp;
            int vol0 = w.stats["volleys"], hit0 = w.stats["shotsHit"], body0 = w.stats["contactsBody"], head0 = w.stats["contactsHead"], lunge0 = w.stats["contactsLunge"];
            double hpStart = g.health.hp;
            int p2Frames = 0;
            while (w.phase == "p2" && (p2Frames < Ms(12000) || (p2Frames < Ms(45000) && w.stats["contactsBody"] + w.stats["contactsHead"] + w.stats["contactsLunge"] == body0 + head0 + lunge0)))
            {
                p2Frames++;
                if (g.health.hp < prevHp) lost += prevHp - g.health.hp;
                if (g.health.hp < 30) g.health.hp = 100;
                prevHp = g.health.hp;
                g.input.SetLook(0, 0.1);
                yield return null;
            }
            if (g.health.hp < prevHp) lost += prevHp - g.health.hp;
            int volleys = w.stats["volleys"] - vol0, spitHits = w.stats["shotsHit"] - hit0;
            int cBody = w.stats["contactsBody"] - body0, cHead = w.stats["contactsHead"] - head0, cLunge = w.stats["contactsLunge"] - lunge0;
            string p2Line = $"phase 2 held {p2Frames / (double)Ms(1000):0.0} s: {volleys} spit volleys, {spitHits} spit hits on the player, contacts body {cBody} / head {cHead} / lunge head {cLunge}, HP lost {lost:0} (start {hpStart:0}, end {g.health.hp:0})";
            if (volleys == 0) fails.Add("no spit volley in the held phase 2");
            God(g);
            w.body.hp = w.body.maxHp * 0.3;
            w.Apply(0);
            guard = 0;
            while (w.phase != "p3" && guard++ < Ms(4000)) { God(g); yield return null; }
            if (w.phase == "p3") seen.Add("phase 3");
            else fails.Add("phase 3 not reached (" + w.phase + ")");
            for (int i = 0; i < Ms(2500); i++) { God(g); yield return null; }
            w.body.hp = 1;
            w.Apply(5);
            guard = 0;
            while (w.Alive && guard++ < Ms(8000)) { God(g); yield return null; }
            if (!w.Alive && g.events.Emitted.ContainsKey("bossDown")) seen.Add("finisher");
            else fails.Add("the worm did not die (phase " + w.phase + ")");
            yield return Frames(Ms(800));
            yield return Stop();
            res[0] = $"{FeelProbe.FrameCount} frames; {pc}; design.n24.wyrm ON; beats seen: {string.Join(", ", seen)}; {p2Line}; whole reel: spit volleys {w.stats["volleys"]}, shards {w.stats["shards"]}, dives {w.stats["dives"]}, lunges {w.stats["lunges"]} | "
                + (fails.Count == 0 ? "PASS" : "FAIL (RED): " + string.Join("; ", fails));
        }
    }
}
