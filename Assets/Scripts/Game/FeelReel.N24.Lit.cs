// NIGHT24 reel `lit` (builder L; UNCOMPILED by Unity, dn-checked): the room lighting kit on the Player. Layer 1's combat rooms and links one after
// the other (the boss room is the worm's own room): the campaign is set to each room cleared (so no enemy is on the screen), the player stands at
// the room's spawn and turns through three headings, 40 frames each, after a 30-frame settle. The frames are written; the LUM meter
// (Tools/feel-metrics.py) is run by the lead on them: the result line gives each room's frame range so the meter can slice per room, and the
// accent colour the trim wears. Boot with -skygrindMap layer1 and design.n24.lighting on (RED: "SKIPPED (RED)" while it is off).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N24LitBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            body = N24LitScenario(g, dir, res, offs, pre);
        }

        static IEnumerator N24LitScenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            string why = N24CampaignGate(g);
            if (why != null)
            {
                res[0] = why;
                yield break;
            }
            if (N24Blocks.ForceOff || !N24Blocks.From(g.cfg).lighting)
            {
                res[0] = "SKIPPED (RED): design.n24.lighting is off";
                yield break;
            }
            Quiet(g);
            g.noRecords = true;
            g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            offs.Add(() => g.setup.mode = RunSetup.Survival);
            g.StartRun(Seed);
            God(g);
            if (!g.sim.campaignOn)
            {
                res[0] = "SKIPPED (RED): the run did not start as a CAMPAIGN";
                yield break;
            }
            yield return Frames(30);
            LevelDef lv = g.sim.campaignLevel;
            Start(g, "lit", "ui", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            var rows = new StringBuilder();
            for (int r = 0; r < lv.rooms.Count; r++)
            {
                RoomDef room = lv.rooms[r];
                if (room.kind == "boss") continue; // the worm's room is B's
                Campaign24 c = g.sim.campaign;
                c.room = r; // the lights follow the campaign's room; Cleared: no wave, no enemy in the frame
                c.phase = Campaign24.Phase.Cleared;
                g.Respawn(room.spawn, room.spawnYawDeg);
                int first = FeelProbe.FrameCount;
                FeelProbe.Event("lit:" + room.id);
                for (int k = 0; k < 3; k++)
                {
                    g.input.SetLook(room.spawnYawDeg * Math.PI / 180 + k * 2 * Math.PI / 3, 0.05);
                    for (int f = 0; f < (k == 0 ? 30 : 0) + 40; f++)
                    {
                        God(g);
                        yield return null;
                    }
                }
                rows.Append($"{room.id} frames {first}-{FeelProbe.FrameCount - 1}; ");
            }
            yield return Stop();
            string overlap = HudOverlap25.Report(dir, out bool noOverlap); // NIGHT25: no two HUD text rects overlap at any frame
            res[0] = $"{FeelProbe.FrameCount} frames; {overlap}{(noOverlap ? "" : " (RED)")}; design.n24.lighting ON; rooms (3 headings each, boss room not shot): {rows}run the LUM meter on these slices (want 20..80), M-MAGENTA 0";
        }
    }
}
