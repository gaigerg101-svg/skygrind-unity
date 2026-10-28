// NIGHT24 reel `layer1` (builder L; UNCOMPILED, written without the Editor): a bot run through the whole of layer 1, door to door, on the
// Player (the contract's `reel layer1`): each room's clear time against its par, deaths and retries per room, the links' crossing times
// (the vent hall's jump pad, the flue), and the boss room entered. The boss fight itself is the `wyrm` reel's. Boot with -skygrindMap layer1.
// RED: "NOT WRITTEN (RED)" before this file; "SKIPPED (RED)" while the campaign row is off or the booted map is not a campaign level.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N24LayerBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N24LayerScenario(g, dir, res, offs, pre);
        }

        static IEnumerator N24LayerScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null)
            {
                res[0] = why;
                yield break;
            }
            var rows = new List<string>();
            offs.Add(g.events.On<RoomEvent>("roomClear", e => rows.Add($"{e.name} {e.time:0.0} s (par {e.par:0}{(e.withinPar ? "" : ", over")})")));
            int retries = 0, layerClears = 0;
            offs.Add(g.events.On<RoomEvent>("roomRetry", _ => retries++));
            offs.Add(g.events.On<RoomEvent>("layerClear", _ => layerClears++));
            N24StartCampaign(g, offs);
            string pre0 = Preconditions(g, g.Feet);
            if (!g.sim.campaignOn)
            {
                res[0] = "SKIPPED (RED): the run did not start as a CAMPAIGN";
                yield break;
            }
            LevelDef lv = g.sim.campaignLevel;
            int bossIndex = lv.rooms.FindIndex(r => r.kind == "boss");
            yield return Frames(20);
            Start(g, "layer1", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            int i = 0;
            for (; i < Ms(300000); i++) // 5 minutes of game time at most (the bot needs about 3 for rooms 1-5)
            {
                AutopilotInput.TakeDraft(g.sim);
                Campaign24 c = g.sim.campaign;
                if (c != null && (c.finished || (bossIndex >= 0 && c.room >= bossIndex))) break;
                if (g.flow == Flow.Results) break;
                yield return null;
            }
            // THE FURNACE: the player is in the worm's room. The bot cannot play the fight here (the room is off the nav grid; builder B's headless
            // WyrmBot kills it in about 109 s), so the worm is fast-forwarded the way the `wyrm` reel does: the intro, then the two phase beats, then the
            // last HP; the campaign must see it die and end the layer ("layerClear").
            string boss = "no boss fight (the wyrm row is off or its files are missing)";
            Campaign24 cb = g.sim.campaign;
            Wyrm w = g.sim.wyrm24;
            if (cb != null && !cb.finished && w != null && w.Alive)
            {
                for (int k = 0; k < Ms(3000); k++) { God(g); yield return null; } // the intro card
                w.body.hp = w.body.maxHp * 0.6;
                w.Apply(0);
                for (int k = 0; k < Ms(1500) && w.phase != "p2"; k++) { God(g); yield return null; }
                w.body.hp = w.body.maxHp * 0.3;
                w.Apply(0);
                for (int k = 0; k < Ms(1500) && w.phase != "p3"; k++) { God(g); yield return null; }
                for (int k = 0; k < Ms(2000); k++) { God(g); yield return null; }
                w.body.hp = 1;
                w.Apply(5);
                for (int k = 0; k < Ms(8000) && !cb.finished && g.flow != Flow.Results; k++) { God(g); yield return null; }
                boss = $"the worm {(w.Alive ? "STILL ALIVE" : "dead")} (phase {w.phase}), bossDown seen {g.events.Emitted.ContainsKey("bossDown")}";
            }
            yield return Frames(Ms(1000));
            yield return Stop();
            Campaign24 end = g.sim.campaign;
            res[0] = $"{pre0}; {FeelProbe.FrameCount} frames; cleared in order: {string.Join("; ", rows)}; deaths {(end != null ? end.deaths : 0)}, retries {retries}; THE FURNACE: {boss}; whole layer {(end != null ? end.TotalSeconds : 0):0.0} s of game time, layerClear event {layerClears}x; " +
                     $"ended in room {(end != null ? end.room + 1 : 0)} of {lv.rooms.Count} ({(end != null && end.room >= 0 ? lv.rooms[end.room].name : "-")}), {(end != null && end.finished ? "LAYER CLEARED" : "boss room entered / not finished")}, flow {g.flow}";
        }
    }
}
